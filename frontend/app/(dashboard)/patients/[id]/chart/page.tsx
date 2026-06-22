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

const conditionColors: Record<string, string> = {
  Healthy:    'bg-green-100 text-green-800 border-green-300',
  Caries:     'bg-red-100 text-red-800 border-red-300',
  Filled:     'bg-blue-100 text-blue-800 border-blue-300',
  Missing:    'bg-gray-100 text-gray-500 border-gray-300',
  Extracted:  'bg-gray-200 text-gray-600 border-gray-400',
  Crown:      'bg-yellow-100 text-yellow-800 border-yellow-300',
  Bridge:     'bg-orange-100 text-orange-800 border-orange-300',
  Implant:    'bg-purple-100 text-purple-800 border-purple-300',
  RootCanal:  'bg-pink-100 text-pink-800 border-pink-300',
  Fracture:   'bg-red-200 text-red-900 border-red-400',
  Impacted:   'bg-amber-100 text-amber-800 border-amber-300',
  Sensitive:  'bg-cyan-100 text-cyan-800 border-cyan-300',
  Mobility:   'bg-indigo-100 text-indigo-800 border-indigo-300',
  Other:      'bg-gray-100 text-gray-700 border-gray-300',
};

// solid fill colors for surface cells
const surfaceFill: Record<string, string> = {
  Healthy:   '#dcfce7',
  Caries:    '#fecaca',
  Filled:    '#bfdbfe',
  Missing:   '#e5e7eb',
  Extracted: '#d1d5db',
  Crown:     '#fef08a',
  Bridge:    '#fed7aa',
  Implant:   '#e9d5ff',
  RootCanal: '#fbcfe8',
  Fracture:  '#fca5a5',
  Impacted:  '#fde68a',
  Sensitive: '#a5f3fc',
  Mobility:  '#c7d2fe',
  Other:     '#f3f4f6',
};

const conditionAr: Record<string, string> = {
  Healthy:'سليم', Caries:'تسوس', Filled:'حشو', Missing:'مفقود',
  Extracted:'مخلوع', Crown:'تلبيسة', Bridge:'جسر', Implant:'زراعة',
  RootCanal:'علاج عصب', Fracture:'كسر', Impacted:'مطمور',
  Sensitive:'حساس', Mobility:'تحرك', Other:'أخرى',
};

// FDI surface codes
const SURFACES = ['M', 'B', 'O', 'L', 'D'] as const;
type Surface = typeof SURFACES[number];

const surfaceAr: Record<Surface, string> = {
  M: 'أمامي', B: 'خد', O: 'مضغ', L: 'لسان', D: 'خلفي',
};

// Visual positions in the 3x3 cross grid: [row, col]
const surfacePos: Record<Surface, [number, number]> = {
  M: [0, 1], B: [1, 0], O: [1, 1], L: [1, 2], D: [2, 1],
};

function getSurfaceColor(conditions: ChartEntry[], surface: Surface): string {
  const entry = conditions.find(c => c.surface === surface);
  if (!entry) return '#f9fafb';
  return surfaceFill[entry.condition] ?? '#f9fafb';
}

function getToothBg(tooth: ToothChart): string {
  if (tooth.currentConditions.length === 0) return 'bg-white';
  const main = tooth.currentConditions[0];
  const cls = conditionColors[main.condition] ?? '';
  return cls.split(' ')[0] ?? 'bg-white';
}

function ToothCell({ tooth, selected, onSelect }: {
  tooth: ToothChart;
  selected: boolean;
  onSelect: (t: ToothChart) => void;
}) {
  const bgClass = getToothBg(tooth);
  const hasIssue = tooth.currentConditions.some(c => c.condition !== 'Healthy');

  return (
    <button
      onClick={() => onSelect(tooth)}
      title={`${tooth.nameAr} (${tooth.fdiNumber})`}
      className={`w-10 h-11 rounded-lg border-2 text-xs font-bold flex flex-col items-center justify-center transition-all
        ${selected ? 'ring-2 ring-blue-500 border-blue-400' : 'border-gray-200 hover:border-blue-300'}
        ${bgClass}
        ${hasIssue ? 'shadow-sm' : ''}
      `}
    >
      <span className="text-[11px] font-bold text-gray-700">{tooth.fdiNumber}</span>
      {tooth.currentConditions.length > 0 && (
        <span className={`w-2 h-2 rounded-full mt-0.5 ${hasIssue ? 'bg-red-400' : 'bg-green-400'}`} />
      )}
    </button>
  );
}

function SurfaceDiagram({ tooth, selectedSurface, onSurfaceSelect }: {
  tooth: ToothChart;
  selectedSurface: Surface | null;
  onSurfaceSelect: (s: Surface) => void;
}) {
  // Build 3x3 grid, only positions defined in surfacePos are filled
  const grid: (Surface | null)[][] = [
    [null, null, null],
    [null, null, null],
    [null, null, null],
  ];
  SURFACES.forEach(s => {
    const [r, c] = surfacePos[s];
    grid[r][c] = s;
  });

  return (
    <div className="flex flex-col items-center">
      <div className="text-xs text-gray-500 mb-2 font-medium">خريطة الأسطح — السن {tooth.fdiNumber}</div>
      <div className="grid grid-rows-3 gap-1">
        {grid.map((row, ri) => (
          <div key={ri} className="flex gap-1">
            {row.map((surface, ci) => {
              if (!surface) {
                return <div key={ci} className="w-12 h-12" />;
              }
              const bg = getSurfaceColor(tooth.currentConditions, surface);
              const condEntry = tooth.currentConditions.find(c => c.surface === surface);
              const isSelected = selectedSurface === surface;
              return (
                <button
                  key={ci}
                  onClick={() => onSurfaceSelect(surface)}
                  title={`${surfaceAr[surface]} (${surface})`}
                  style={{ backgroundColor: bg }}
                  className={`w-12 h-12 rounded-lg border-2 flex flex-col items-center justify-center text-xs font-bold transition-all
                    ${isSelected ? 'ring-2 ring-blue-500 border-blue-400 scale-105' : 'border-gray-300 hover:border-blue-300 hover:scale-105'}
                  `}
                >
                  <span className="text-gray-700 text-[10px] font-bold">{surface}</span>
                  <span className="text-gray-500 text-[9px] leading-none mt-0.5">{surfaceAr[surface]}</span>
                  {condEntry && (
                    <span className="text-[8px] text-gray-600 leading-none mt-0.5 truncate max-w-[44px] px-0.5 text-center">
                      {conditionAr[condEntry.condition]}
                    </span>
                  )}
                </button>
              );
            })}
          </div>
        ))}
      </div>
      <div className="mt-2 text-[10px] text-gray-400 text-center">
        B=خد · L=لسان · M=أمامي · D=خلفي · O=مضغ
      </div>
    </div>
  );
}

export default function DentalChartPage() {
  const { id } = useParams<{ id: string }>();
  const token = useAuthStore((s) => s.accessToken);
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const [chart, setChart] = useState<ChartResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<ToothChart | null>(null);
  const [selectedSurface, setSelectedSurface] = useState<Surface | null>(null);
  const [showUpdate, setShowUpdate] = useState(false);
  const [form, setForm] = useState({ condition: 'Caries', surface: '' as Surface | '', severity: '', notes: '' });
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!id || !token) return;
    fetchChart();
  }, [id, token]);

  async function fetchChart() {
    setLoading(true);
    const r = await api.get<ChartResponse>(`/patients/${id}/chart`);
    setChart(r.data);
    setLoading(false);
  }

  function openUpdate(surface?: Surface) {
    if (!hasPermission('Patients.Edit')) return;
    setForm({ condition: 'Caries', surface: surface ?? '', severity: '', notes: '' });
    setShowUpdate(true);
  }

  async function handleUpdate() {
    if (!selected) return;
    setSaving(true);
    await api.post(`/patients/${id}/chart`, {
      toothId: selected.toothId,
      condition: form.condition,
      surface: form.surface || null,
      severity: form.severity || null,
      notes: form.notes || null,
    });
    setSaving(false);
    setShowUpdate(false);
    await fetchChart();
    // Re-select the same tooth from updated data
    setSelected(prev => chart?.teeth.find(t => t.toothId === prev?.toothId) ?? null);
  }

  if (loading) return <div className="p-6 text-center text-gray-400" dir="rtl">جارٍ التحميل...</div>;

  const permanent = chart?.teeth.filter((t) => !t.isPrimary) ?? [];
  const upper = permanent.filter((t) => t.jaw === 'Upper').sort((a, b) =>
    a.side === b.side
      ? (a.side === 'Right' ? b.fdiNumber - a.fdiNumber : a.fdiNumber - b.fdiNumber)
      : a.side === 'Right' ? -1 : 1
  );
  const lower = permanent.filter((t) => t.jaw === 'Lower').sort((a, b) =>
    a.side === b.side
      ? (a.side === 'Right' ? b.fdiNumber - a.fdiNumber : a.fdiNumber - b.fdiNumber)
      : a.side === 'Right' ? -1 : 1
  );

  const conditionStats = Object.entries(conditionAr).map(([k, v]) => ({
    key: k,
    label: v,
    count: (chart?.teeth ?? []).flatMap(t => t.currentConditions).filter(c => c.condition === k).length,
  })).filter(s => s.count > 0);

  return (
    <div className="max-w-5xl mx-auto space-y-5" dir="rtl">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-xl font-bold text-gray-900">مخطط الأسنان</h1>
          <Link href={`/patients/${id}`} className="text-xs text-blue-600 hover:underline">← العودة لملف المريض</Link>
        </div>
        {selected && hasPermission('Patients.Edit') && (
          <button
            onClick={() => openUpdate()}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
          >
            + تسجيل حالة للسن {selected.fdiNumber}
          </button>
        )}
      </div>

      {/* Chart grid */}
      <div className="bg-white rounded-2xl border border-gray-100 shadow-sm p-6">
        <div className="flex items-center justify-between mb-3">
          <span className="text-xs text-gray-400 font-medium">الفك العلوي</span>
          <div className="flex gap-3 text-xs text-gray-400">
            <span>اليمين ◄</span>
            <span>► اليسار</span>
          </div>
        </div>
        <div className="flex justify-center gap-1.5 mb-1">
          {upper.map((t) => (
            <ToothCell key={t.toothId} tooth={t} selected={selected?.toothId === t.toothId} onSelect={setSelected} />
          ))}
        </div>
        <div className="border-t-2 border-dashed border-gray-300 my-4" />
        <div className="flex justify-center gap-1.5">
          {lower.map((t) => (
            <ToothCell key={t.toothId} tooth={t} selected={selected?.toothId === t.toothId} onSelect={setSelected} />
          ))}
        </div>
        <span className="text-xs text-gray-400 font-medium mt-3 block text-center">الفك السفلي</span>
      </div>

      {/* Stats bar */}
      {conditionStats.length > 0 && (
        <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-4">
          <div className="text-xs text-gray-500 mb-3 font-medium">ملخص الحالات</div>
          <div className="flex flex-wrap gap-2">
            {conditionStats.map(s => (
              <span key={s.key} className={`text-xs px-3 py-1.5 rounded-full border font-medium ${conditionColors[s.key]}`}>
                {s.label}: {s.count}
              </span>
            ))}
          </div>
        </div>
      )}

      {/* Selected tooth panel */}
      {selected && (
        <div className="bg-white rounded-2xl border border-blue-200 shadow-sm p-5">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* Surface diagram */}
            <div>
              <h3 className="font-semibold text-gray-800 mb-4">
                السن {selected.fdiNumber} — {selected.nameAr}
                <span className="text-xs font-normal text-gray-400 mr-2">({selected.toothType === 'Anterior' ? 'أمامي' : selected.toothType === 'Premolar' ? 'ضاحك' : 'ضرس'})</span>
              </h3>
              <SurfaceDiagram
                tooth={selected}
                selectedSurface={selectedSurface}
                onSurfaceSelect={(s) => {
                  setSelectedSurface(s);
                  if (hasPermission('Patients.Edit')) openUpdate(s);
                }}
              />
            </div>

            {/* Conditions list */}
            <div>
              <div className="flex items-center justify-between mb-3">
                <h4 className="text-sm font-semibold text-gray-700">الحالات المسجلة</h4>
                {hasPermission('Patients.Edit') && (
                  <button onClick={() => openUpdate()} className="text-xs text-blue-600 hover:text-blue-800 border border-blue-200 px-2 py-1 rounded-lg hover:bg-blue-50">
                    + إضافة حالة
                  </button>
                )}
              </div>
              {selected.currentConditions.length === 0 ? (
                <div className="bg-gray-50 rounded-xl p-4 text-center text-gray-400 text-sm">
                  لا توجد حالات مسجلة لهذا السن
                </div>
              ) : (
                <div className="space-y-2">
                  {selected.currentConditions.map((entry) => (
                    <div key={entry.id} className={`flex items-start gap-3 rounded-xl p-3 border ${conditionColors[entry.condition]}`}>
                      <div className="flex-1">
                        <div className="flex items-center gap-2 flex-wrap">
                          <span className="font-semibold text-sm">{conditionAr[entry.condition]}</span>
                          {entry.surface && (
                            <span className="text-xs bg-white/60 px-2 py-0.5 rounded-full">
                              سطح: {surfaceAr[entry.surface as Surface] ?? entry.surface} ({entry.surface})
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
                          {new Date(entry.recordedAt).toLocaleDateString('ar-LY', { year: 'numeric', month: 'short', day: 'numeric' })}
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

      {/* Legend */}
      <div className="bg-white rounded-xl border border-gray-100 shadow-sm p-4">
        <div className="text-xs text-gray-500 mb-3 font-medium">المفتاح اللوني</div>
        <div className="flex flex-wrap gap-2">
          {Object.entries(conditionAr).map(([k, v]) => (
            <span key={k} className={`text-xs px-2.5 py-1 rounded-full border ${conditionColors[k]}`}>
              {v}
            </span>
          ))}
        </div>
      </div>

      {/* Update/Add modal */}
      {showUpdate && selected && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-2xl p-6 w-full max-w-sm shadow-xl" dir="rtl">
            <h3 className="font-bold text-lg mb-1">تسجيل حالة</h3>
            <p className="text-sm text-gray-500 mb-4">
              السن {selected.fdiNumber} — {selected.nameAr}
              {form.surface && ` / سطح ${surfaceAr[form.surface as Surface] ?? form.surface}`}
            </p>
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الحالة *</label>
                <select
                  value={form.condition}
                  onChange={(e) => setForm(f => ({ ...f, condition: e.target.value }))}
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
                  onChange={(e) => setForm(f => ({ ...f, surface: e.target.value as Surface | '' }))}
                  className="w-full border rounded-lg p-2.5 text-sm"
                >
                  <option value="">كامل السن</option>
                  {SURFACES.map(s => (
                    <option key={s} value={s}>{surfaceAr[s]} ({s})</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">درجة الخطورة</label>
                <select
                  value={form.severity}
                  onChange={(e) => setForm(f => ({ ...f, severity: e.target.value }))}
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
                  onChange={(e) => setForm(f => ({ ...f, notes: e.target.value }))}
                  className="w-full border rounded-lg p-2.5 text-sm h-20 resize-none"
                />
              </div>
            </div>
            <div className="flex gap-2 mt-5">
              <button
                onClick={handleUpdate}
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
