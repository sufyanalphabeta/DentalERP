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
  Healthy:   '#dcfce7',
  Caries:    '#fecaca',
  Filled:    '#bfdbfe',
  Missing:   '#d1d5db',
  Extracted: '#9ca3af',
  Crown:     '#fef08a',
  Bridge:    '#fed7aa',
  Implant:   '#e9d5ff',
  RootCanal: '#fbcfe8',
  Fracture:  '#fca5a5',
  Impacted:  '#fde68a',
  Sensitive: '#a5f3fc',
  Mobility:  '#c7d2fe',
  Other:     '#e5e7eb',
};

const conditionStroke: Record<string, string> = {
  Healthy:   '#86efac',
  Caries:    '#f87171',
  Filled:    '#60a5fa',
  Missing:   '#9ca3af',
  Extracted: '#6b7280',
  Crown:     '#facc15',
  Bridge:    '#fb923c',
  Implant:   '#c084fc',
  RootCanal: '#f472b6',
  Fracture:  '#ef4444',
  Impacted:  '#fbbf24',
  Sensitive: '#22d3ee',
  Mobility:  '#818cf8',
  Other:     '#9ca3af',
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

// ── SVG Tooth with 5 clickable surfaces ──────────────────────────

const TOOTH_W = 36;
const TOOTH_H = 40;
const INSET = 10; // size of the outer bands

interface ToothSVGProps {
  tooth: ToothChart;
  isSelected: boolean;
  onSelectTooth: (t: ToothChart) => void;
  onSelectSurface: (t: ToothChart, s: Surface) => void;
}

function getConditionColor(conditions: ChartEntry[], surface?: string): [string, string] {
  const cond = conditions.find((c) => c.surface === (surface ?? null) || (!surface && !c.surface));
  if (cond) return [conditionFill[cond.condition] ?? '#f9fafb', conditionStroke[cond.condition] ?? '#9ca3af'];
  return ['#f9fafb', '#d1d5db'];
}

function ToothSVG({ tooth, isSelected, onSelectTooth, onSelectSurface }: ToothSVGProps) {
  const isExtracted = tooth.currentConditions.some((c) => c.condition === 'Extracted' || c.condition === 'Missing');
  const isMissing = isExtracted;

  const [oFill, oStr] = getConditionColor(tooth.currentConditions, 'O');
  const [bFill, bStr] = getConditionColor(tooth.currentConditions, 'B');
  const [lFill, lStr] = getConditionColor(tooth.currentConditions, 'L');
  const [mFill, mStr] = getConditionColor(tooth.currentConditions, 'M');
  const [dFill, dStr] = getConditionColor(tooth.currentConditions, 'D');

  const W = TOOTH_W;
  const H = TOOTH_H;
  const i = INSET;
  const selectedRing = isSelected ? '#3b82f6' : 'transparent';

  if (isMissing) {
    return (
      <svg
        width={W + 4} height={H + 14}
        viewBox={`-2 -2 ${W + 4} ${H + 14}`}
        className="cursor-pointer"
        onClick={() => onSelectTooth(tooth)}
        aria-label={`سن ${tooth.fdiNumber}`}
      >
        <rect x={0} y={0} width={W} height={H} rx={4} fill="#f3f4f6" stroke="#9ca3af" strokeWidth="1" strokeDasharray="3 2" />
        <line x1={4} y1={4} x2={W - 4} y2={H - 4} stroke="#9ca3af" strokeWidth="1.5" />
        <line x1={W - 4} y1={4} x2={4} y2={H - 4} stroke="#9ca3af" strokeWidth="1.5" />
        <text x={W / 2} y={H + 10} textAnchor="middle" fontSize="9" fill="#6b7280" fontFamily="sans-serif">
          {tooth.fdiNumber}
        </text>
        {isSelected && <rect x={-1} y={-1} width={W + 2} height={H + 2} rx={5} fill="none" stroke={selectedRing} strokeWidth="2" />}
      </svg>
    );
  }

  return (
    <svg
      width={W + 4} height={H + 14}
      viewBox={`-2 -2 ${W + 4} ${H + 14}`}
      className="cursor-pointer"
      aria-label={`سن ${tooth.fdiNumber}`}
    >
      {/* Outer border / selection ring */}
      <rect x={-1} y={-1} width={W + 2} height={H + 2} rx={5}
        fill="none" stroke={selectedRing} strokeWidth={isSelected ? 2 : 0} />

      {/* B — top band */}
      <polygon
        points={`0,0 ${W},0 ${W - i},${i} ${i},${i}`}
        fill={bFill} stroke={bStr} strokeWidth="0.6"
        onClick={() => { onSelectTooth(tooth); onSelectSurface(tooth, 'B'); }}
        className="hover:opacity-80"
      />
      {/* L — bottom band */}
      <polygon
        points={`${i},${H - i} ${W - i},${H - i} ${W},${H} 0,${H}`}
        fill={lFill} stroke={lStr} strokeWidth="0.6"
        onClick={() => { onSelectTooth(tooth); onSelectSurface(tooth, 'L'); }}
        className="hover:opacity-80"
      />
      {/* M — left band */}
      <polygon
        points={`0,0 ${i},${i} ${i},${H - i} 0,${H}`}
        fill={mFill} stroke={mStr} strokeWidth="0.6"
        onClick={() => { onSelectTooth(tooth); onSelectSurface(tooth, 'M'); }}
        className="hover:opacity-80"
      />
      {/* D — right band */}
      <polygon
        points={`${W - i},${i} ${W},0 ${W},${H} ${W - i},${H - i}`}
        fill={dFill} stroke={dStr} strokeWidth="0.6"
        onClick={() => { onSelectTooth(tooth); onSelectSurface(tooth, 'D'); }}
        className="hover:opacity-80"
      />
      {/* O — center */}
      <rect
        x={i} y={i} width={W - 2 * i} height={H - 2 * i}
        fill={oFill} stroke={oStr} strokeWidth="0.6"
        onClick={() => { onSelectTooth(tooth); onSelectSurface(tooth, 'O'); }}
        className="hover:opacity-80"
      />

      {/* Outer tooth border */}
      <rect x={0} y={0} width={W} height={H} rx={4}
        fill="none" stroke="#6b7280" strokeWidth="0.8"
        onClick={() => onSelectTooth(tooth)}
      />

      {/* FDI number label */}
      <text x={W / 2} y={H + 10} textAnchor="middle" fontSize="9"
        fill={isSelected ? '#2563eb' : '#374151'} fontWeight={isSelected ? 'bold' : 'normal'}
        fontFamily="sans-serif">
        {tooth.fdiNumber}
      </text>
    </svg>
  );
}

// ── Sort helpers ──────────────────────────────────────────────────

function sortUpper(teeth: ToothChart[]): ToothChart[] {
  // Upper jaw: right side (11-18) then left side (21-28), FDI order
  const right = teeth.filter((t) => t.fdiNumber >= 11 && t.fdiNumber <= 18).sort((a, b) => b.fdiNumber - a.fdiNumber);
  const left = teeth.filter((t) => t.fdiNumber >= 21 && t.fdiNumber <= 28).sort((a, b) => a.fdiNumber - b.fdiNumber);
  return [...right, ...left];
}

function sortLower(teeth: ToothChart[]): ToothChart[] {
  // Lower jaw: right side (41-48) then left side (31-38), FDI order
  const right = teeth.filter((t) => t.fdiNumber >= 41 && t.fdiNumber <= 48).sort((a, b) => b.fdiNumber - a.fdiNumber);
  const left = teeth.filter((t) => t.fdiNumber >= 31 && t.fdiNumber <= 38).sort((a, b) => a.fdiNumber - b.fdiNumber);
  return [...right, ...left];
}

// ── Main Page ────────────────────────────────────────────────────

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

  useEffect(() => {
    if (!id || !token) return;
    fetchChart();
  }, [id, token]);

  async function fetchChart() {
    setLoading(true);
    try {
      const r = await api.get<ChartResponse>(`/patients/${id}/chart`);
      setChart(r.data);
    } finally {
      setLoading(false);
    }
  }

  function handleSelectTooth(tooth: ToothChart) {
    setSelected(tooth);
  }

  function handleSelectSurface(tooth: ToothChart, surface: Surface) {
    setSelected(tooth);
    if (hasPermission('Patients.Edit')) {
      setForm({ condition: 'Caries', surface, severity: '', notes: '' });
      setShowUpdate(true);
    }
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
        toothId: selected.toothId,
        condition: form.condition,
        surface: form.surface || null,
        severity: form.severity || null,
        notes: form.notes || null,
      });
      setShowUpdate(false);
      await fetchChart();
      // Re-select updated tooth
      setChart((prev) => {
        const updated = prev?.teeth.find((t) => t.toothId === selected.toothId);
        if (updated) setSelected(updated);
        return prev;
      });
    } finally {
      setSaving(false);
    }
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
          <Link href={`/patients/${id}`} className="text-xs text-blue-600 hover:underline">
            ← العودة لملف المريض
          </Link>
        </div>
        {selected && hasPermission('Patients.Edit') && (
          <button
            onClick={() => openUpdateModal()}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700 transition-colors"
          >
            + تسجيل حالة — السن {selected.fdiNumber}
          </button>
        )}
      </div>

      {/* ── Odontogram ──────────────────────────────────────────── */}
      <div className="bg-white rounded-2xl border border-gray-100 shadow-sm px-4 pt-4 pb-3 select-none">
        {/* Surface legend */}
        <div className="flex justify-center gap-4 mb-3 flex-wrap">
          {(['B', 'M', 'O', 'D', 'L'] as Surface[]).map((s) => (
            <span key={s} className="flex items-center gap-1 text-[11px] text-gray-500">
              <span className="inline-block w-3 h-3 rounded-sm border border-gray-300 bg-gray-100" />
              {s} = {surfaceAr[s]}
            </span>
          ))}
        </div>

        {/* Upper jaw label */}
        <div className="flex items-center justify-between mb-1 px-2">
          <span className="text-[11px] text-gray-400 font-medium">الفك العلوي</span>
          <div className="flex gap-4 text-[11px] text-gray-400">
            <span>← يسار</span>
            <span>يمين →</span>
          </div>
        </div>

        {/* Upper row */}
        <div className="flex justify-center gap-1 flex-wrap">
          {upper.map((t) => (
            <ToothSVG
              key={t.toothId}
              tooth={t}
              isSelected={selected?.toothId === t.toothId}
              onSelectTooth={handleSelectTooth}
              onSelectSurface={handleSelectSurface}
            />
          ))}
        </div>

        {/* Midline */}
        <div className="border-t-2 border-dashed border-gray-200 my-3 mx-4" />

        {/* Lower row */}
        <div className="flex justify-center gap-1 flex-wrap">
          {lower.map((t) => (
            <ToothSVG
              key={t.toothId}
              tooth={t}
              isSelected={selected?.toothId === t.toothId}
              onSelectTooth={handleSelectTooth}
              onSelectSurface={handleSelectSurface}
            />
          ))}
        </div>

        {/* Lower jaw label */}
        <div className="text-center mt-1">
          <span className="text-[11px] text-gray-400 font-medium">الفك السفلي</span>
        </div>

        {/* Tip */}
        <p className="text-center text-[10px] text-gray-400 mt-2">
          انقر على أي سطح من أسطح السن لتسجيل حالة • النقر على السن لعرض التفاصيل
        </p>
      </div>

      {/* ── Condition summary bar ─────────────────────────────────── */}
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

            {/* Left: surface detail SVG (large) */}
            <div>
              <h3 className="font-semibold text-gray-800 mb-1">
                السن {selected.fdiNumber} — {selected.nameAr}
                <span className="text-xs font-normal text-gray-400 mr-2">({toothTypeAr(selected.toothType)})</span>
              </h3>
              <p className="text-xs text-gray-400 mb-4">
                {selected.jaw === 'Upper' ? 'الفك العلوي' : 'الفك السفلي'} •{' '}
                {selected.side === 'Right' ? 'يمين' : 'يسار'}
              </p>

              {/* Large interactive surface diagram */}
              <div className="flex justify-center">
                <LargeSurfaceDiagram
                  tooth={selected}
                  onSurfaceClick={(s) => {
                    if (hasPermission('Patients.Edit')) openUpdateModal(s);
                  }}
                />
              </div>
            </div>

            {/* Right: conditions list */}
            <div>
              <div className="flex items-center justify-between mb-3">
                <h4 className="text-sm font-semibold text-gray-700">الحالات المسجلة</h4>
                {hasPermission('Patients.Edit') && (
                  <button
                    onClick={() => openUpdateModal()}
                    className="text-xs text-blue-600 hover:text-blue-800 border border-blue-200 px-2 py-1 rounded-lg hover:bg-blue-50"
                  >
                    + إضافة
                  </button>
                )}
              </div>

              {selected.currentConditions.length === 0 ? (
                <div className="bg-gray-50 rounded-xl p-6 text-center text-gray-400 text-sm">
                  لا توجد حالات مسجلة لهذا السن
                </div>
              ) : (
                <div className="space-y-2 max-h-64 overflow-y-auto">
                  {selected.currentConditions.map((entry) => (
                    <div
                      key={entry.id}
                      className={`flex items-start gap-3 rounded-xl p-3 border ${conditionBadge[entry.condition]}`}
                    >
                      <div className="flex-1">
                        <div className="flex items-center gap-2 flex-wrap">
                          <span className="font-semibold text-sm">{conditionAr[entry.condition]}</span>
                          {entry.surface && (
                            <span className="text-xs bg-white/60 px-2 py-0.5 rounded-full">
                              {surfaceAr[entry.surface as Surface] ?? entry.surface} ({entry.surface})
                            </span>
                          )}
                          {entry.severity && (
                            <span className="text-xs bg-white/60 px-2 py-0.5 rounded-full">
                              {entry.severity === 'Mild' ? 'خفيف' : entry.severity === 'Moderate' ? 'متوسط' : 'شديد'}
                            </span>
                          )}
                        </div>
                        {entry.notes && <p className="text-xs mt-1 opacity-80">{entry.notes}</p>}
                        <p className="text-xs opacity-60 mt-0.5">
                          {new Date(entry.recordedAt).toLocaleDateString('ar-LY', {
                            year: 'numeric', month: 'short', day: 'numeric',
                          })}
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
                <select
                  value={form.condition}
                  onChange={(e) => setForm((f) => ({ ...f, condition: e.target.value }))}
                  className="w-full border rounded-lg p-2.5 text-sm"
                >
                  {Object.entries(conditionAr).map(([k, v]) => (
                    <option key={k} value={k}>{v}</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">السطح</label>
                <select
                  value={form.surface}
                  onChange={(e) => setForm((f) => ({ ...f, surface: e.target.value as Surface | '' }))}
                  className="w-full border rounded-lg p-2.5 text-sm"
                >
                  <option value="">كامل السن</option>
                  {SURFACES.map((s) => (
                    <option key={s} value={s}>{surfaceAr[s]} ({s})</option>
                  ))}
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">درجة الخطورة</label>
                <select
                  value={form.severity}
                  onChange={(e) => setForm((f) => ({ ...f, severity: e.target.value }))}
                  className="w-full border rounded-lg p-2.5 text-sm"
                >
                  <option value="">— غير محدد —</option>
                  <option value="Mild">خفيف</option>
                  <option value="Moderate">متوسط</option>
                  <option value="Severe">شديد</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
                <textarea
                  placeholder="ملاحظات إضافية..."
                  value={form.notes}
                  onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))}
                  className="w-full border rounded-lg p-2.5 text-sm h-20 resize-none"
                />
              </div>
            </div>

            <div className="flex gap-2 mt-5">
              <button
                onClick={handleSave}
                disabled={saving}
                className="flex-1 bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
              >
                {saving ? 'جاري الحفظ...' : 'حفظ'}
              </button>
              <button
                onClick={() => setShowUpdate(false)}
                className="flex-1 bg-gray-100 text-gray-700 py-2.5 rounded-lg text-sm hover:bg-gray-200"
              >
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

// ── Large surface diagram (detail panel) ─────────────────────────

function LargeSurfaceDiagram({ tooth, onSurfaceClick }: {
  tooth: ToothChart;
  onSurfaceClick: (s: Surface) => void;
}) {
  const W = 120;
  const H = 130;
  const i = 36;

  const getSurf = (s: Surface): [string, string] => {
    const cond = tooth.currentConditions.find((c) => c.surface === s);
    if (cond) return [conditionFill[cond.condition] ?? '#f9fafb', conditionStroke[cond.condition] ?? '#9ca3af'];
    return ['#f9fafb', '#d1d5db'];
  };

  const [oFill, oStr] = getSurf('O');
  const [bFill, bStr] = getSurf('B');
  const [lFill, lStr] = getSurf('L');
  const [mFill, mStr] = getSurf('M');
  const [dFill, dStr] = getSurf('D');

  const labelStyle = (fill: string) => ({
    fontSize: '11px', fontFamily: 'sans-serif', fontWeight: 'bold',
    fill: '#374151',
  } as React.CSSProperties);

  return (
    <div className="space-y-2">
      <svg width={W} height={H} viewBox={`0 0 ${W} ${H}`} className="block">
        {/* B top */}
        <polygon points={`0,0 ${W},0 ${W-i},${i} ${i},${i}`}
          fill={bFill} stroke={bStr} strokeWidth="1"
          className="cursor-pointer hover:opacity-75 transition-opacity"
          onClick={() => onSurfaceClick('B')} />
        <text x={W/2} y={i/2+5} textAnchor="middle" style={labelStyle(bFill)}>خدي</text>

        {/* L bottom */}
        <polygon points={`${i},${H-i} ${W-i},${H-i} ${W},${H} 0,${H}`}
          fill={lFill} stroke={lStr} strokeWidth="1"
          className="cursor-pointer hover:opacity-75 transition-opacity"
          onClick={() => onSurfaceClick('L')} />
        <text x={W/2} y={H-i/2+5} textAnchor="middle" style={labelStyle(lFill)}>لساني</text>

        {/* M left */}
        <polygon points={`0,0 ${i},${i} ${i},${H-i} 0,${H}`}
          fill={mFill} stroke={mStr} strokeWidth="1"
          className="cursor-pointer hover:opacity-75 transition-opacity"
          onClick={() => onSurfaceClick('M')} />
        <text x={i/2} y={H/2+4} textAnchor="middle" style={labelStyle(mFill)}>م</text>

        {/* D right */}
        <polygon points={`${W-i},${i} ${W},0 ${W},${H} ${W-i},${H-i}`}
          fill={dFill} stroke={dStr} strokeWidth="1"
          className="cursor-pointer hover:opacity-75 transition-opacity"
          onClick={() => onSurfaceClick('D')} />
        <text x={W-i/2} y={H/2+4} textAnchor="middle" style={labelStyle(dFill)}>خ</text>

        {/* O center */}
        <rect x={i} y={i} width={W-2*i} height={H-2*i}
          fill={oFill} stroke={oStr} strokeWidth="1"
          className="cursor-pointer hover:opacity-75 transition-opacity"
          onClick={() => onSurfaceClick('O')} />
        <text x={W/2} y={H/2+4} textAnchor="middle" style={{ ...labelStyle(oFill), fontSize: '12px' }}>مضغ</text>

        {/* Outer border */}
        <rect x={0} y={0} width={W} height={H} fill="none" stroke="#9ca3af" strokeWidth="1.5" />
      </svg>

      <div className="text-[10px] text-gray-400 text-center space-y-0.5">
        <div>م = أمامي · خ = خلفي</div>
        <div>انقر على سطح لتسجيل حالة</div>
      </div>
    </div>
  );
}
