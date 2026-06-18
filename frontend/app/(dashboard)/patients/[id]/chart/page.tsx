'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
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
  Healthy: 'bg-green-100 text-green-800',
  Caries: 'bg-red-100 text-red-800',
  Filled: 'bg-blue-100 text-blue-800',
  Missing: 'bg-gray-100 text-gray-500',
  Extracted: 'bg-gray-200 text-gray-600',
  Crown: 'bg-yellow-100 text-yellow-800',
  Bridge: 'bg-orange-100 text-orange-800',
  Implant: 'bg-purple-100 text-purple-800',
  RootCanal: 'bg-pink-100 text-pink-800',
  Fracture: 'bg-red-200 text-red-900',
  Impacted: 'bg-amber-100 text-amber-800',
  Sensitive: 'bg-cyan-100 text-cyan-800',
  Mobility: 'bg-indigo-100 text-indigo-800',
  Other: 'bg-gray-100 text-gray-700',
};

const conditionAr: Record<string, string> = {
  Healthy: 'سليم', Caries: 'تسوس', Filled: 'حشو', Missing: 'مفقود',
  Extracted: 'مخلوع', Crown: 'تلبيسة', Bridge: 'جسر', Implant: 'زراعة',
  RootCanal: 'علاج عصب', Fracture: 'كسر', Impacted: 'مطمور',
  Sensitive: 'حساس', Mobility: 'تحرك', Other: 'أخرى',
};

function ToothCell({ tooth, onSelect }: { tooth: ToothChart; onSelect: (t: ToothChart) => void }) {
  const mainCondition = tooth.currentConditions[0];
  const colorClass = mainCondition ? conditionColors[mainCondition.condition] : 'bg-white';

  return (
    <button
      onClick={() => onSelect(tooth)}
      title={`${tooth.nameAr} (${tooth.fdiNumber})`}
      className={`w-10 h-10 rounded border border-gray-300 text-xs font-bold flex flex-col items-center justify-center hover:ring-2 hover:ring-blue-400 transition-all ${colorClass}`}
    >
      <span>{tooth.fdiNumber}</span>
      {tooth.currentConditions.length > 1 && (
        <span className="text-[9px] text-blue-600">+{tooth.currentConditions.length - 1}</span>
      )}
    </button>
  );
}

export default function DentalChartPage() {
  const { id } = useParams<{ id: string }>();
  const token = useAuthStore((s) => s.accessToken);
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const [chart, setChart] = useState<ChartResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<ToothChart | null>(null);
  const [showUpdate, setShowUpdate] = useState(false);
  const [form, setForm] = useState({ condition: 'Caries', surface: '', severity: '', notes: '' });

  useEffect(() => {
    if (!id || !token) return;
    fetch(`/api/patients/${id}/chart`, { headers: { Authorization: `Bearer ${token}` } })
      .then((r) => r.json())
      .then(setChart)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [id, token]);

  const handleUpdate = async () => {
    if (!selected) return;
    await fetch(`/api/patients/${id}/chart`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({ toothId: selected.toothId, ...form, surface: form.surface || null }),
    });
    setShowUpdate(false);
    setLoading(true);
    fetch(`/api/patients/${id}/chart`, { headers: { Authorization: `Bearer ${token}` } })
      .then((r) => r.json())
      .then(setChart)
      .finally(() => setLoading(false));
  };

  if (loading) return <div className="p-6">جارٍ التحميل...</div>;

  const permanent = chart?.teeth.filter((t) => !t.isPrimary) ?? [];
  const upper = permanent.filter((t) => t.jaw === 'Upper').sort((a, b) => {
    if (a.side === 'Right' && b.side === 'Left') return -1;
    if (a.side === 'Left' && b.side === 'Right') return 1;
    return a.side === 'Right' ? b.fdiNumber - a.fdiNumber : a.fdiNumber - b.fdiNumber;
  });
  const lower = permanent.filter((t) => t.jaw === 'Lower').sort((a, b) => {
    if (a.side === 'Right' && b.side === 'Left') return -1;
    if (a.side === 'Left' && b.side === 'Right') return 1;
    return a.side === 'Right' ? b.fdiNumber - a.fdiNumber : a.fdiNumber - b.fdiNumber;
  });

  return (
    <div className="p-6 max-w-4xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-bold">مخطط الأسنان</h1>
        {hasPermission('Patients.Edit') && selected && (
          <button
            onClick={() => setShowUpdate(true)}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm"
          >
            تحديث حالة السن {selected.fdiNumber}
          </button>
        )}
      </div>

      {/* Dental Chart Grid */}
      <div className="bg-white rounded-xl border border-gray-200 p-6 mb-6">
        <p className="text-center text-xs text-gray-400 mb-3">الفك العلوي</p>
        <div className="flex justify-center gap-1 mb-1">
          {upper.map((t) => (
            <ToothCell key={t.toothId} tooth={t} onSelect={setSelected} />
          ))}
        </div>
        <div className="border-t-2 border-gray-400 my-3" />
        <div className="flex justify-center gap-1">
          {lower.map((t) => (
            <ToothCell key={t.toothId} tooth={t} onSelect={setSelected} />
          ))}
        </div>
        <p className="text-center text-xs text-gray-400 mt-3">الفك السفلي</p>
      </div>

      {/* Legend */}
      <div className="flex flex-wrap gap-2 mb-4">
        {Object.entries(conditionAr).map(([k, v]) => (
          <span key={k} className={`text-xs px-2 py-1 rounded ${conditionColors[k]}`}>
            {v}
          </span>
        ))}
      </div>

      {/* Selected Tooth Panel */}
      {selected && (
        <div className="bg-blue-50 rounded-xl border border-blue-200 p-4">
          <h3 className="font-semibold text-blue-800 mb-2">
            السن {selected.fdiNumber} — {selected.nameAr}
          </h3>
          {selected.currentConditions.length === 0 ? (
            <p className="text-gray-500 text-sm">لا توجد حالات مسجلة</p>
          ) : (
            <div className="space-y-2">
              {selected.currentConditions.map((entry) => (
                <div key={entry.id} className="flex items-center gap-3 bg-white rounded p-2 text-sm">
                  <span className={`px-2 py-0.5 rounded text-xs ${conditionColors[entry.condition]}`}>
                    {conditionAr[entry.condition]}
                  </span>
                  {entry.surface && <span className="text-gray-500">سطح: {entry.surface}</span>}
                  {entry.severity && <span className="text-gray-500">{entry.severity}</span>}
                  {entry.notes && <span className="text-gray-600">{entry.notes}</span>}
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Update Modal */}
      {showUpdate && selected && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-96" dir="rtl">
            <h3 className="font-bold text-lg mb-4">تحديث حالة السن {selected.fdiNumber}</h3>
            <div className="space-y-3">
              <select
                value={form.condition}
                onChange={(e) => setForm((f) => ({ ...f, condition: e.target.value }))}
                className="w-full border rounded-lg p-2"
              >
                {Object.entries(conditionAr).map(([k, v]) => (
                  <option key={k} value={k}>{v}</option>
                ))}
              </select>
              <select
                value={form.surface}
                onChange={(e) => setForm((f) => ({ ...f, surface: e.target.value }))}
                className="w-full border rounded-lg p-2"
              >
                <option value="">كامل السن</option>
                {['M', 'D', 'B', 'L', 'O'].map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
              <select
                value={form.severity}
                onChange={(e) => setForm((f) => ({ ...f, severity: e.target.value }))}
                className="w-full border rounded-lg p-2"
              >
                <option value="">— درجة الخطورة —</option>
                {['Mild', 'Moderate', 'Severe'].map((s) => (
                  <option key={s} value={s}>{s === 'Mild' ? 'خفيف' : s === 'Moderate' ? 'متوسط' : 'شديد'}</option>
                ))}
              </select>
              <textarea
                placeholder="ملاحظات"
                value={form.notes}
                onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))}
                className="w-full border rounded-lg p-2 h-20"
              />
            </div>
            <div className="flex gap-2 mt-4">
              <button onClick={handleUpdate} className="flex-1 bg-blue-600 text-white py-2 rounded-lg">
                حفظ
              </button>
              <button onClick={() => setShowUpdate(false)} className="flex-1 bg-gray-200 py-2 rounded-lg">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
