'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { api } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';

interface ChartEntry {
  id: string;
  surface?: string;
  condition: string;
  severity?: string;
  notes?: string;
  recordedAt: string;
}

interface ToothChart {
  toothId: number;
  fdiNumber: number;
  nameAr: string;
  nameEn: string;
  jaw: string;
  side: string;
  toothType: string;
  isPrimary: boolean;
  currentConditions: ChartEntry[];
}

interface ChartResponse {
  patientId: string;
  teeth: ToothChart[];
}

// ── Condition metadata ─────────────────────────────────────────────

const conditionFill: Record<string, string> = {
  Healthy:   '#dcfce7', Caries:    '#fecaca', Filled:    '#bfdbfe',
  Missing:   '#d1d5db', Extracted: '#9ca3af', Crown:     '#fef08a',
  Bridge:    '#fed7aa', Implant:   '#e9d5ff', RootCanal: '#fbcfe8',
  Fracture:  '#fca5a5', Impacted:  '#fde68a', Sensitive: '#a5f3fc',
  Mobility:  '#c7d2fe', Other:     '#e5e7eb',
};

const conditionStroke: Record<string, string> = {
  Healthy:   '#86efac', Caries:    '#f87171', Filled:    '#60a5fa',
  Missing:   '#9ca3af', Extracted: '#6b7280', Crown:     '#facc15',
  Bridge:    '#fb923c', Implant:   '#c084fc', RootCanal: '#f472b6',
  Fracture:  '#ef4444', Impacted:  '#fbbf24', Sensitive: '#22d3ee',
  Mobility:  '#818cf8', Other:     '#9ca3af',
};

const conditionBadge: Record<string, string> = {
  Healthy:   'bg-green-100 text-green-800 border-green-300',
  Caries:    'bg-red-100 text-red-800 border-red-300',
  Filled:    'bg-blue-100 text-blue-800 border-blue-300',
  Missing:   'bg-gray-100 text-gray-500 border-gray-300',
  Extracted: 'bg-gray-200 text-gray-600 border-gray-400',
  Crown:     'bg-yellow-100 text-yellow-800 border-yellow-300',
  Bridge:    'bg-orange-100 text-orange-800 border-orange-300',
  Implant:   'bg-purple-100 text-purple-800 border-purple-300',
  RootCanal: 'bg-pink-100 text-pink-800 border-pink-300',
  Fracture:  'bg-red-200 text-red-900 border-red-400',
  Impacted:  'bg-amber-100 text-amber-800 border-amber-300',
  Sensitive: 'bg-cyan-100 text-cyan-800 border-cyan-300',
  Mobility:  'bg-indigo-100 text-indigo-800 border-indigo-300',
  Other:     'bg-gray-100 text-gray-700 border-gray-300',
};

const conditionAr: Record<string, string> = {
  Healthy:'سليم', Caries:'تسوس', Filled:'حشو', Missing:'مفقود',
  Extracted:'مخلوع', Crown:'تلبيسة', Bridge:'جسر', Implant:'زراعة',
  RootCanal:'علاج عصب', Fracture:'كسر', Impacted:'مطمور',
  Sensitive:'حساس', Mobility:'تحرك', Other:'أخرى',
};

const SURFACES = ['M', 'B', 'O', 'L', 'D'] as const;
type Surface = typeof SURFACES[number];

const surfaceAr: Record<Surface, string> = {
  M: 'أمامي', B: 'خدي', O: 'مضغ', L: 'لساني', D: 'خلفي',
};

type ViewMode = '2d' | '3d';

// ── Helpers ───────────────────────────────────────────────────────

function getSurfFill(conditions: ChartEntry[], s: Surface): string {
  const c = conditions.find((e) => e.surface === s);
  return c ? (conditionFill[c.condition] ?? '#f9fafb') : '#f9fafb';
}

function getWholeFill(conditions: ChartEntry[]): string {
  const c = conditions.find((e) => !e.surface);
  return c ? (conditionFill[c.condition] ?? '#f9fafb') : '#f9fafb';
}

function getMainColor(conditions: ChartEntry[]): string {
  if (conditions.length === 0) return '#f9fafb';
  const caries = conditions.find((c) => c.condition === 'Caries');
  if (caries) return conditionFill['Caries'];
  return conditionFill[conditions[0].condition] ?? '#f9fafb';
}

function getMainStroke(conditions: ChartEntry[]): string {
  if (conditions.length === 0) return '#d1d5db';
  return conditionStroke[conditions[0].condition] ?? '#d1d5db';
}

// ── Sort helpers ──────────────────────────────────────────────────

function sortUpper(teeth: ToothChart[]): ToothChart[] {
  const right = teeth.filter((t) => t.fdiNumber >= 11 && t.fdiNumber <= 18).sort((a, b) => b.fdiNumber - a.fdiNumber);
  const left  = teeth.filter((t) => t.fdiNumber >= 21 && t.fdiNumber <= 28).sort((a, b) => a.fdiNumber - b.fdiNumber);
  return [...right, ...left];
}

function sortLower(teeth: ToothChart[]): ToothChart[] {
  const right = teeth.filter((t) => t.fdiNumber >= 41 && t.fdiNumber <= 48).sort((a, b) => b.fdiNumber - a.fdiNumber);
  const left  = teeth.filter((t) => t.fdiNumber >= 31 && t.fdiNumber <= 38).sort((a, b) => a.fdiNumber - b.fdiNumber);
  return [...right, ...left];
}

// ══════════════════════════════════════════════════════════════════
// 2D SVG Odontogram (Phase 6)
// ══════════════════════════════════════════════════════════════════

const TOOTH_W = 36;
const TOOTH_H = 40;
const INSET   = 10;

interface ToothSVGProps {
  tooth: ToothChart;
  isSelected: boolean;
  onSelectTooth: (t: ToothChart) => void;
  onSelectSurface: (t: ToothChart, s: Surface) => void;
}

function ToothSVG({ tooth, isSelected, onSelectTooth, onSelectSurface }: ToothSVGProps) {
  const W = TOOTH_W, H = TOOTH_H, i = INSET;
  const isMissing = tooth.currentConditions.some((c) => c.condition === 'Extracted' || c.condition === 'Missing');

  if (isMissing) {
    return (
      <svg width={W + 4} height={H + 14} viewBox={`-2 -2 ${W + 4} ${H + 14}`} className="cursor-pointer" onClick={() => onSelectTooth(tooth)}>
        <rect x={0} y={0} width={W} height={H} rx={4} fill="#f3f4f6" stroke="#9ca3af" strokeWidth="1" strokeDasharray="3 2" />
        <line x1={4} y1={4} x2={W-4} y2={H-4} stroke="#9ca3af" strokeWidth="1.5" />
        <line x1={W-4} y1={4} x2={4} y2={H-4} stroke="#9ca3af" strokeWidth="1.5" />
        <text x={W/2} y={H+10} textAnchor="middle" fontSize="9" fill="#6b7280" fontFamily="sans-serif">{tooth.fdiNumber}</text>
        {isSelected && <rect x={-1} y={-1} width={W+2} height={H+2} rx={5} fill="none" stroke="#3b82f6" strokeWidth="2" />}
      </svg>
    );
  }

  const [oFill, oStr] = [getSurfFill(tooth.currentConditions, 'O'), conditionStroke[(tooth.currentConditions.find(c=>c.surface==='O')?.condition ?? '')] ?? '#d1d5db'];
  const [bFill, bStr] = [getSurfFill(tooth.currentConditions, 'B'), conditionStroke[(tooth.currentConditions.find(c=>c.surface==='B')?.condition ?? '')] ?? '#d1d5db'];
  const [lFill, lStr] = [getSurfFill(tooth.currentConditions, 'L'), conditionStroke[(tooth.currentConditions.find(c=>c.surface==='L')?.condition ?? '')] ?? '#d1d5db'];
  const [mFill, mStr] = [getSurfFill(tooth.currentConditions, 'M'), conditionStroke[(tooth.currentConditions.find(c=>c.surface==='M')?.condition ?? '')] ?? '#d1d5db'];
  const [dFill, dStr] = [getSurfFill(tooth.currentConditions, 'D'), conditionStroke[(tooth.currentConditions.find(c=>c.surface==='D')?.condition ?? '')] ?? '#d1d5db'];

  return (
    <svg width={W+4} height={H+14} viewBox={`-2 -2 ${W+4} ${H+14}`} className="cursor-pointer" aria-label={`سن ${tooth.fdiNumber}`}>
      {isSelected && <rect x={-1} y={-1} width={W+2} height={H+2} rx={5} fill="none" stroke="#3b82f6" strokeWidth="2" />}
      <polygon points={`0,0 ${W},0 ${W-i},${i} ${i},${i}`} fill={bFill} stroke={bStr} strokeWidth="0.6" className="hover:opacity-80" onClick={() => { onSelectTooth(tooth); onSelectSurface(tooth,'B'); }} />
      <polygon points={`${i},${H-i} ${W-i},${H-i} ${W},${H} 0,${H}`} fill={lFill} stroke={lStr} strokeWidth="0.6" className="hover:opacity-80" onClick={() => { onSelectTooth(tooth); onSelectSurface(tooth,'L'); }} />
      <polygon points={`0,0 ${i},${i} ${i},${H-i} 0,${H}`} fill={mFill} stroke={mStr} strokeWidth="0.6" className="hover:opacity-80" onClick={() => { onSelectTooth(tooth); onSelectSurface(tooth,'M'); }} />
      <polygon points={`${W-i},${i} ${W},0 ${W},${H} ${W-i},${H-i}`} fill={dFill} stroke={dStr} strokeWidth="0.6" className="hover:opacity-80" onClick={() => { onSelectTooth(tooth); onSelectSurface(tooth,'D'); }} />
      <rect x={i} y={i} width={W-2*i} height={H-2*i} fill={oFill} stroke={oStr} strokeWidth="0.6" className="hover:opacity-80" onClick={() => { onSelectTooth(tooth); onSelectSurface(tooth,'O'); }} />
      <rect x={0} y={0} width={W} height={H} rx={4} fill="none" stroke="#6b7280" strokeWidth="0.8" onClick={() => onSelectTooth(tooth)} />
      <text x={W/2} y={H+10} textAnchor="middle" fontSize="9" fill={isSelected?'#2563eb':'#374151'} fontWeight={isSelected?'bold':'normal'} fontFamily="sans-serif">{tooth.fdiNumber}</text>
    </svg>
  );
}

// ══════════════════════════════════════════════════════════════════
// 3D CSS Perspective Odontogram (Phase 7)
// ══════════════════════════════════════════════════════════════════

const BOX_W  = 28;
const BOX_H  = 32;
const BOX_D  = 14; // depth
const ISO_X  = 0.866; // cos(30deg)
const ISO_Y  = 0.5;   // sin(30deg)

interface Tooth3DProps {
  tooth: ToothChart;
  isSelected: boolean;
  onSelectTooth: (t: ToothChart) => void;
  jaw: 'Upper' | 'Lower';
}

function Tooth3D({ tooth, isSelected, onSelectTooth, jaw }: Tooth3DProps) {
  const isMissing = tooth.currentConditions.some((c) => c.condition === 'Extracted' || c.condition === 'Missing');
  const topColor   = getSurfFill(tooth.currentConditions, 'O');    // occlusal = top face
  const frontColor = jaw === 'Upper'
    ? getSurfFill(tooth.currentConditions, 'B')   // upper: buccal faces outward (bottom when viewed from above)
    : getSurfFill(tooth.currentConditions, 'L');  // lower: lingual faces inward
  const sideColor  = getSurfFill(tooth.currentConditions, 'D');
  const mainStroke = getMainStroke(tooth.currentConditions);
  const mainFill   = getMainColor(tooth.currentConditions);

  // Total SVG dimensions (isometric projection)
  const svgW  = BOX_W + BOX_D * ISO_X + 4;
  const svgH  = BOX_H + BOX_D * ISO_Y + 18;
  const ox    = 2; // origin x
  const oy    = BOX_D * ISO_Y + 2; // origin y (push down for depth)

  // Top face vertices (parallelogram)
  const topPts = [
    [ox,           oy],
    [ox + BOX_W,   oy],
    [ox + BOX_W + BOX_D * ISO_X, oy - BOX_D * ISO_Y],
    [ox + BOX_D * ISO_X,         oy - BOX_D * ISO_Y],
  ].map(([x, y]) => `${x},${y}`).join(' ');

  // Front face (left-bottom)
  const frontPts = [
    [ox,         oy],
    [ox + BOX_W, oy],
    [ox + BOX_W, oy + BOX_H],
    [ox,         oy + BOX_H],
  ].map(([x, y]) => `${x},${y}`).join(' ');

  // Right side face
  const sidePts = [
    [ox + BOX_W,               oy],
    [ox + BOX_W + BOX_D*ISO_X, oy - BOX_D*ISO_Y],
    [ox + BOX_W + BOX_D*ISO_X, oy - BOX_D*ISO_Y + BOX_H],
    [ox + BOX_W,               oy + BOX_H],
  ].map(([x, y]) => `${x},${y}`).join(' ');

  const labelY = oy + BOX_H + 12;

  if (isMissing) {
    return (
      <svg width={svgW} height={svgH} viewBox={`0 0 ${svgW} ${svgH}`} className="cursor-pointer" onClick={() => onSelectTooth(tooth)}>
        <polygon points={frontPts} fill="#f3f4f6" stroke="#9ca3af" strokeWidth="0.8" strokeDasharray="3 2" />
        <line x1={ox+4} y1={oy+4} x2={ox+BOX_W-4} y2={oy+BOX_H-4} stroke="#9ca3af" strokeWidth="1" />
        <line x1={ox+BOX_W-4} y1={oy+4} x2={ox+4} y2={oy+BOX_H-4} stroke="#9ca3af" strokeWidth="1" />
        <text x={ox+BOX_W/2} y={labelY} textAnchor="middle" fontSize="8" fill="#9ca3af" fontFamily="sans-serif">{tooth.fdiNumber}</text>
      </svg>
    );
  }

  return (
    <svg width={svgW} height={svgH} viewBox={`0 0 ${svgW} ${svgH}`}
      className="cursor-pointer drop-shadow-sm" onClick={() => onSelectTooth(tooth)}>

      {/* selection glow */}
      {isSelected && (
        <rect x={ox-2} y={oy-BOX_D*ISO_Y-2} width={BOX_W+4} height={BOX_H+BOX_D*ISO_Y+4}
          fill="none" stroke="#3b82f6" strokeWidth="1.5" strokeDasharray="3 2" rx={2} />
      )}

      {/* Right side face */}
      <polygon points={sidePts} fill={sideColor} stroke={mainStroke} strokeWidth="0.6" />

      {/* Front face */}
      <polygon points={frontPts} fill={frontColor} stroke={mainStroke} strokeWidth="0.6" />

      {/* Top face */}
      <polygon points={topPts} fill={topColor} stroke={mainStroke} strokeWidth="0.6" />

      {/* Shine on top */}
      {topColor !== '#f9fafb' && (
        <polygon
          points={[
            [ox + 2,         oy - 2],
            [ox + BOX_W/3,   oy - 2],
            [ox + BOX_W/3 + BOX_D*ISO_X*0.4, oy - 2 - BOX_D*ISO_Y*0.4],
            [ox + 2 + BOX_D*ISO_X*0.4,       oy - 2 - BOX_D*ISO_Y*0.4],
          ].map(([x, y]) => `${x},${y}`).join(' ')}
          fill="rgba(255,255,255,0.35)"
        />
      )}

      {/* Highlight edge */}
      <line x1={ox} y1={oy} x2={ox+BOX_W} y2={oy} stroke="rgba(255,255,255,0.5)" strokeWidth="1" />

      {/* FDI label */}
      <text x={ox + BOX_W/2} y={labelY} textAnchor="middle" fontSize="8"
        fill={isSelected ? '#2563eb' : '#374151'} fontWeight={isSelected ? 'bold' : 'normal'}
        fontFamily="sans-serif">{tooth.fdiNumber}</text>
    </svg>
  );
}

// ── Large surface diagram (2D detail panel) ───────────────────────

function LargeSurfaceDiagram({ tooth, onSurfaceClick }: {
  tooth: ToothChart;
  onSurfaceClick: (s: Surface) => void;
}) {
  const W = 120, H = 130, i = 36;

  const getS = (s: Surface): [string, string] => {
    const c = tooth.currentConditions.find((e) => e.surface === s);
    return c
      ? [conditionFill[c.condition] ?? '#f9fafb', conditionStroke[c.condition] ?? '#d1d5db']
      : ['#f9fafb', '#d1d5db'];
  };

  const [oFill, oStr] = getS('O');
  const [bFill, bStr] = getS('B');
  const [lFill, lStr] = getS('L');
  const [mFill, mStr] = getS('M');
  const [dFill, dStr] = getS('D');

  const ts = { fontSize: '11px', fontFamily: 'sans-serif', fontWeight: 'bold', fill: '#374151' } as React.CSSProperties;

  return (
    <div className="space-y-2">
      <svg width={W} height={H} viewBox={`0 0 ${W} ${H}`} className="block">
        <polygon points={`0,0 ${W},0 ${W-i},${i} ${i},${i}`} fill={bFill} stroke={bStr} strokeWidth="1" className="cursor-pointer hover:opacity-75" onClick={() => onSurfaceClick('B')} />
        <text x={W/2} y={i/2+5} textAnchor="middle" style={ts}>خدي</text>
        <polygon points={`${i},${H-i} ${W-i},${H-i} ${W},${H} 0,${H}`} fill={lFill} stroke={lStr} strokeWidth="1" className="cursor-pointer hover:opacity-75" onClick={() => onSurfaceClick('L')} />
        <text x={W/2} y={H-i/2+5} textAnchor="middle" style={ts}>لساني</text>
        <polygon points={`0,0 ${i},${i} ${i},${H-i} 0,${H}`} fill={mFill} stroke={mStr} strokeWidth="1" className="cursor-pointer hover:opacity-75" onClick={() => onSurfaceClick('M')} />
        <text x={i/2} y={H/2+4} textAnchor="middle" style={ts}>م</text>
        <polygon points={`${W-i},${i} ${W},0 ${W},${H} ${W-i},${H-i}`} fill={dFill} stroke={dStr} strokeWidth="1" className="cursor-pointer hover:opacity-75" onClick={() => onSurfaceClick('D')} />
        <text x={W-i/2} y={H/2+4} textAnchor="middle" style={ts}>خ</text>
        <rect x={i} y={i} width={W-2*i} height={H-2*i} fill={oFill} stroke={oStr} strokeWidth="1" className="cursor-pointer hover:opacity-75" onClick={() => onSurfaceClick('O')} />
        <text x={W/2} y={H/2+4} textAnchor="middle" style={{ ...ts, fontSize: '12px' }}>مضغ</text>
        <rect x={0} y={0} width={W} height={H} fill="none" stroke="#9ca3af" strokeWidth="1.5" />
      </svg>
      <div className="text-[10px] text-gray-400 text-center">م = أمامي · خ = خلفي · انقر لتسجيل حالة</div>
    </div>
  );
}

// ── Main Page ─────────────────────────────────────────────────────

export default function DentalChartPage() {
  const { id } = useParams<{ id: string }>();
  const token = useAuthStore((s) => s.accessToken);
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const [chart, setChart] = useState<ChartResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<ToothChart | null>(null);
  const [showUpdate, setShowUpdate] = useState(false);
  const [form, setForm] = useState({ condition: 'Caries', surface: '' as Surface | '', severity: '', notes: '' });
  const [saving, setSaving] = useState(false);
  const [viewMode, setViewMode] = useState<ViewMode>('2d');

  useEffect(() => { if (id && token) fetchChart(); }, [id, token]);

  async function fetchChart() {
    setLoading(true);
    try {
      const r = await api.get<ChartResponse>(`/patients/${id}/chart`);
      setChart(r.data);
    } finally { setLoading(false); }
  }

  function openUpdateModal(surface?: Surface) {
    if (!hasPermission('Patients.Edit')) return;
    setForm({ condition: 'Caries', surface: surface ?? '', severity: '', notes: '' });
    setShowUpdate(true);
  }

  async function handleSave() {
    if (!selected) return;
    setSaving(true);
    try {
      await api.post(`/patients/${id}/chart`, {
        toothId: selected.toothId, condition: form.condition,
        surface: form.surface || null, severity: form.severity || null, notes: form.notes || null,
      });
      setShowUpdate(false);
      await fetchChart();
    } finally { setSaving(false); }
  }

  if (loading) {
    return (
      <div className="p-6 text-center text-gray-400" dir="rtl">
        <div className="animate-spin w-8 h-8 border-4 border-blue-500 border-t-transparent rounded-full mx-auto mb-3" />
        جارٍ تحميل مخطط الأسنان...
      </div>
    );
  }

  const permanent = chart?.teeth.filter((t) => !t.isPrimary) ?? [];
  const upper = sortUpper(permanent.filter((t) => t.jaw === 'Upper'));
  const lower = sortLower(permanent.filter((t) => t.jaw === 'Lower'));

  const conditionStats = Object.entries(conditionAr).map(([k, v]) => ({
    key: k, label: v,
    count: permanent.flatMap((t) => t.currentConditions).filter((c) => c.condition === k).length,
  })).filter((s) => s.count > 0);

  const toothTypeAr = (t: string) =>
    t === 'Anterior' ? 'أمامي' : t === 'Premolar' ? 'ضاحك' : 'ضرس';

  return (
    <div className="max-w-5xl mx-auto p-4 space-y-5" dir="rtl">

      {/* ── Header ───────────────────────────────────────────────── */}
      <div className="flex items-center justify-between flex-wrap gap-3">
        <div>
          <h1 className="text-xl font-bold text-gray-900">مخطط الأسنان</h1>
          <Link href={`/patients/${id}`} className="text-xs text-blue-600 hover:underline">← العودة لملف المريض</Link>
        </div>
        <div className="flex items-center gap-2">
          {/* View toggle */}
          <div className="flex rounded-lg border border-gray-200 overflow-hidden text-xs">
            <button
              onClick={() => setViewMode('2d')}
              className={`px-3 py-1.5 transition-colors ${viewMode === '2d' ? 'bg-blue-600 text-white' : 'bg-white text-gray-600 hover:bg-gray-50'}`}
            >
              مسطح 2D
            </button>
            <button
              onClick={() => setViewMode('3d')}
              className={`px-3 py-1.5 transition-colors ${viewMode === '3d' ? 'bg-blue-600 text-white' : 'bg-white text-gray-600 hover:bg-gray-50'}`}
            >
              مجسم 3D
            </button>
          </div>
          {selected && hasPermission('Patients.Edit') && (
            <button
              onClick={() => openUpdateModal()}
              className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700 transition-colors"
            >
              + حالة — السن {selected.fdiNumber}
            </button>
          )}
        </div>
      </div>

      {/* ── Odontogram ──────────────────────────────────────────── */}
      <div className={`rounded-2xl border border-gray-100 shadow-sm px-4 pt-4 pb-3 select-none ${viewMode === '3d' ? 'bg-gradient-to-b from-slate-800 to-slate-900' : 'bg-white'}`}>

        {viewMode === '2d' && (
          <div className="flex justify-center gap-4 mb-3 flex-wrap">
            {(['B','M','O','D','L'] as Surface[]).map((s) => (
              <span key={s} className="flex items-center gap-1 text-[11px] text-gray-500">
                <span className="inline-block w-3 h-3 rounded-sm border border-gray-300 bg-gray-100" />
                {s} = {surfaceAr[s]}
              </span>
            ))}
          </div>
        )}

        {viewMode === '3d' && (
          <div className="text-center text-xs text-slate-400 mb-3">
            عرض مجسم • اضغط على السن للتفاصيل والتسجيل
          </div>
        )}

        {/* Labels + jaw direction */}
        <div className="flex items-center justify-between mb-1 px-2">
          <span className={`text-[11px] font-medium ${viewMode === '3d' ? 'text-slate-400' : 'text-gray-400'}`}>الفك العلوي</span>
          <div className={`flex gap-4 text-[11px] ${viewMode === '3d' ? 'text-slate-400' : 'text-gray-400'}`}>
            <span>← يسار</span>
            <span>يمين →</span>
          </div>
        </div>

        {/* Upper row */}
        <div className="flex justify-center gap-1 flex-wrap">
          {upper.map((t) =>
            viewMode === '2d' ? (
              <ToothSVG key={t.toothId} tooth={t}
                isSelected={selected?.toothId === t.toothId}
                onSelectTooth={setSelected}
                onSelectSurface={(tooth, s) => { setSelected(tooth); if (hasPermission('Patients.Edit')) { setForm({ condition:'Caries', surface:s, severity:'', notes:'' }); setShowUpdate(true); } }} />
            ) : (
              <Tooth3D key={t.toothId} tooth={t}
                isSelected={selected?.toothId === t.toothId}
                onSelectTooth={setSelected}
                jaw="Upper" />
            )
          )}
        </div>

        {/* Midline */}
        <div className={`border-t-2 border-dashed my-3 mx-4 ${viewMode === '3d' ? 'border-slate-600' : 'border-gray-200'}`} />

        {/* Lower row */}
        <div className="flex justify-center gap-1 flex-wrap">
          {lower.map((t) =>
            viewMode === '2d' ? (
              <ToothSVG key={t.toothId} tooth={t}
                isSelected={selected?.toothId === t.toothId}
                onSelectTooth={setSelected}
                onSelectSurface={(tooth, s) => { setSelected(tooth); if (hasPermission('Patients.Edit')) { setForm({ condition:'Caries', surface:s, severity:'', notes:'' }); setShowUpdate(true); } }} />
            ) : (
              <Tooth3D key={t.toothId} tooth={t}
                isSelected={selected?.toothId === t.toothId}
                onSelectTooth={setSelected}
                jaw="Lower" />
            )
          )}
        </div>

        <div className={`text-center mt-1 text-[11px] font-medium ${viewMode === '3d' ? 'text-slate-400' : 'text-gray-400'}`}>
          الفك السفلي
        </div>

        {viewMode === '2d' && (
          <p className="text-center text-[10px] text-gray-400 mt-2">
            انقر على أي سطح من أسطح السن لتسجيل حالة • النقر على السن لعرض التفاصيل
          </p>
        )}
      </div>

      {/* ── Condition summary ─────────────────────────────────────── */}
      {conditionStats.length > 0 && (
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-4">
          <div className="text-xs text-gray-500 mb-3 font-medium">ملخص الحالات</div>
          <div className="flex flex-wrap gap-2">
            {conditionStats.map((s) => (
              <span key={s.key} className={`text-xs px-3 py-1.5 rounded-full border font-medium ${conditionBadge[s.key]}`}>
                {s.label}: {s.count}
              </span>
            ))}
          </div>
        </div>
      )}

      {/* ── Selected tooth detail ─────────────────────────────────── */}
      {selected && (
        <div className="bg-white rounded-2xl border border-blue-200 shadow-sm p-5">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <h3 className="font-semibold text-gray-800 mb-1">
                السن {selected.fdiNumber} — {selected.nameAr}
                <span className="text-xs font-normal text-gray-400 mr-2">({toothTypeAr(selected.toothType)})</span>
              </h3>
              <p className="text-xs text-gray-400 mb-4">
                {selected.jaw === 'Upper' ? 'الفك العلوي' : 'الفك السفلي'} •{' '}
                {selected.side === 'Right' ? 'يمين' : 'يسار'}
              </p>
              <div className="flex justify-center">
                <LargeSurfaceDiagram tooth={selected} onSurfaceClick={(s) => openUpdateModal(s)} />
              </div>
            </div>

            <div>
              <div className="flex items-center justify-between mb-3">
                <h4 className="text-sm font-semibold text-gray-700">الحالات المسجلة</h4>
                {hasPermission('Patients.Edit') && (
                  <button onClick={() => openUpdateModal()} className="text-xs text-blue-600 border border-blue-200 px-2 py-1 rounded-lg hover:bg-blue-50">
                    + إضافة
                  </button>
                )}
              </div>
              {selected.currentConditions.length === 0 ? (
                <div className="bg-gray-50 rounded-xl p-6 text-center text-gray-400 text-sm">لا توجد حالات مسجلة</div>
              ) : (
                <div className="space-y-2 max-h-64 overflow-y-auto">
                  {selected.currentConditions.map((entry) => (
                    <div key={entry.id} className={`flex items-start gap-3 rounded-xl p-3 border ${conditionBadge[entry.condition]}`}>
                      <div className="flex-1">
                        <div className="flex items-center gap-2 flex-wrap">
                          <span className="font-semibold text-sm">{conditionAr[entry.condition]}</span>
                          {entry.surface && <span className="text-xs bg-white/60 px-2 py-0.5 rounded-full">{surfaceAr[entry.surface as Surface] ?? entry.surface} ({entry.surface})</span>}
                          {entry.severity && <span className="text-xs bg-white/60 px-2 py-0.5 rounded-full">{entry.severity === 'Mild' ? 'خفيف' : entry.severity === 'Moderate' ? 'متوسط' : 'شديد'}</span>}
                        </div>
                        {entry.notes && <p className="text-xs mt-1 opacity-80">{entry.notes}</p>}
                        <p className="text-xs opacity-60 mt-0.5">
                          {new Date(entry.recordedAt).toLocaleDateString('ar-LY', { year:'numeric', month:'short', day:'numeric' })}
                        </p>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      )}

      {/* ── Color legend ──────────────────────────────────────────── */}
      <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-4">
        <div className="text-xs text-gray-500 mb-3 font-medium">المفتاح اللوني</div>
        <div className="flex flex-wrap gap-2">
          {Object.entries(conditionAr).map(([k, v]) => (
            <span key={k} className="flex items-center gap-1.5 text-xs">
              <span className="inline-block w-4 h-4 rounded border" style={{ backgroundColor: conditionFill[k], borderColor: conditionStroke[k] }} />
              <span className="text-gray-600">{v}</span>
            </span>
          ))}
        </div>
      </div>

      {/* ── Record condition modal ────────────────────────────────── */}
      {showUpdate && selected && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl p-6 w-full max-w-sm shadow-xl" dir="rtl">
            <h3 className="font-bold text-lg mb-1">تسجيل حالة</h3>
            <p className="text-sm text-gray-500 mb-4">
              السن {selected.fdiNumber} — {selected.nameAr}
              {form.surface && ` / ${surfaceAr[form.surface as Surface]} (${form.surface})`}
            </p>
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الحالة *</label>
                <select value={form.condition} onChange={(e) => setForm((f) => ({ ...f, condition: e.target.value }))} className="w-full border rounded-lg p-2.5 text-sm">
                  {Object.entries(conditionAr).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">السطح</label>
                <select value={form.surface} onChange={(e) => setForm((f) => ({ ...f, surface: e.target.value as Surface | '' }))} className="w-full border rounded-lg p-2.5 text-sm">
                  <option value="">كامل السن</option>
                  {SURFACES.map((s) => <option key={s} value={s}>{surfaceAr[s]} ({s})</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">درجة الخطورة</label>
                <select value={form.severity} onChange={(e) => setForm((f) => ({ ...f, severity: e.target.value }))} className="w-full border rounded-lg p-2.5 text-sm">
                  <option value="">— غير محدد —</option>
                  <option value="Mild">خفيف</option>
                  <option value="Moderate">متوسط</option>
                  <option value="Severe">شديد</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
                <textarea placeholder="ملاحظات إضافية..." value={form.notes} onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))} className="w-full border rounded-lg p-2.5 text-sm h-20 resize-none" />
              </div>
            </div>
            <div className="flex gap-2 mt-5">
              <button onClick={handleSave} disabled={saving} className="flex-1 bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
                {saving ? 'جاري الحفظ...' : 'حفظ'}
              </button>
              <button onClick={() => setShowUpdate(false)} className="flex-1 bg-gray-100 text-gray-700 py-2.5 rounded-lg text-sm hover:bg-gray-200">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
