'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { useAuthStore } from '@/stores/authStore';

interface Procedure {
  id: string;
  procedureName: string;
  procedureCode?: string;
  toothId?: number;
  surface?: string;
  notes?: string;
  billingStatus: string;
  performedAt: string;
  durationMinutes?: number;
}

const billingAr: Record<string, string> = {
  Pending: 'في الانتظار',
  SentToTreasury: 'أُرسل للخزينة',
  Paid: 'مدفوع',
  Cancelled: 'ملغى',
};

const billingColors: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-700',
  SentToTreasury: 'bg-blue-100 text-blue-700',
  Paid: 'bg-green-100 text-green-700',
  Cancelled: 'bg-gray-100 text-gray-500',
};

export default function ProceduresPage() {
  const { id } = useParams<{ id: string }>();
  const token = useAuthStore((s) => s.accessToken);
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const [procedures, setProcedures] = useState<Procedure[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAdd, setShowAdd] = useState(false);
  const [form, setForm] = useState({
    patientId: '',
    procedureName: '',
    toothId: '',
    surface: '',
    procedureCode: '',
    notes: '',
    durationMinutes: '',
    updateChartEntry: false,
    chartCondition: '',
  });

  const load = () => {
    if (!id || !token) return;
    fetch(`/api/appointments/${id}/procedures`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then((r) => r.json())
      .then(setProcedures)
      .catch(console.error)
      .finally(() => setLoading(false));
  };

  useEffect(load, [id, token]);

  const handleAdd = async () => {
    await fetch(`/api/appointments/${id}/procedures`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        patientId: form.patientId,
        procedureName: form.procedureName,
        toothId: form.toothId ? parseInt(form.toothId) : null,
        surface: form.surface || null,
        procedureCode: form.procedureCode || null,
        notes: form.notes || null,
        durationMinutes: form.durationMinutes ? parseInt(form.durationMinutes) : null,
        updateChartEntry: form.updateChartEntry,
        chartCondition: form.chartCondition || null,
      }),
    });
    setShowAdd(false);
    setLoading(true);
    load();
  };

  if (loading) return <div className="p-6">جارٍ التحميل...</div>;

  return (
    <div className="p-6 max-w-3xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-bold">إجراءات الموعد</h1>
        {hasPermission('Patients.Edit') && (
          <button
            onClick={() => setShowAdd(true)}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm"
          >
            + إضافة إجراء
          </button>
        )}
      </div>

      {procedures.length === 0 ? (
        <div className="bg-gray-50 rounded-xl p-8 text-center text-gray-500">
          لا توجد إجراءات مسجلة لهذا الموعد
        </div>
      ) : (
        <div className="space-y-3">
          {procedures.map((proc) => (
            <div key={proc.id} className="bg-white rounded-xl border border-gray-200 p-4">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="font-semibold text-gray-900">{proc.procedureName}</h3>
                  <div className="flex gap-3 mt-1 text-sm text-gray-500">
                    {proc.procedureCode && <span>رمز: {proc.procedureCode}</span>}
                    {proc.toothId && <span>سن: {proc.toothId}</span>}
                    {proc.surface && <span>سطح: {proc.surface}</span>}
                    {proc.durationMinutes && <span>{proc.durationMinutes} دقيقة</span>}
                  </div>
                </div>
                <span className={`text-xs px-2 py-1 rounded ${billingColors[proc.billingStatus]}`}>
                  {billingAr[proc.billingStatus]}
                </span>
              </div>
              {proc.notes && <p className="text-sm text-gray-600 mt-2">{proc.notes}</p>}
              <p className="text-xs text-gray-400 mt-2">
                {new Date(proc.performedAt).toLocaleDateString('ar-SA')}
              </p>
            </div>
          ))}
        </div>
      )}

      {showAdd && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-96 max-h-[90vh] overflow-y-auto" dir="rtl">
            <h3 className="font-bold text-lg mb-4">إضافة إجراء جديد</h3>
            <div className="space-y-3">
              <input
                placeholder="اسم الإجراء *"
                value={form.procedureName}
                onChange={(e) => setForm((f) => ({ ...f, procedureName: e.target.value }))}
                className="w-full border rounded-lg p-2"
              />
              <input
                placeholder="رقم السن (اختياري)"
                type="number"
                value={form.toothId}
                onChange={(e) => setForm((f) => ({ ...f, toothId: e.target.value }))}
                className="w-full border rounded-lg p-2"
              />
              <select
                value={form.surface}
                onChange={(e) => setForm((f) => ({ ...f, surface: e.target.value }))}
                className="w-full border rounded-lg p-2"
              >
                <option value="">— السطح —</option>
                {['M', 'D', 'B', 'L', 'O'].map((s) => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
              <input
                placeholder="رمز الإجراء (CDT)"
                value={form.procedureCode}
                onChange={(e) => setForm((f) => ({ ...f, procedureCode: e.target.value }))}
                className="w-full border rounded-lg p-2"
              />
              <input
                type="number"
                placeholder="المدة (دقائق)"
                value={form.durationMinutes}
                onChange={(e) => setForm((f) => ({ ...f, durationMinutes: e.target.value }))}
                className="w-full border rounded-lg p-2"
              />
              <textarea
                placeholder="ملاحظات"
                value={form.notes}
                onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))}
                className="w-full border rounded-lg p-2 h-16"
              />
              <label className="flex items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  checked={form.updateChartEntry}
                  onChange={(e) => setForm((f) => ({ ...f, updateChartEntry: e.target.checked }))}
                />
                تحديث مخطط الأسنان تلقائياً
              </label>
            </div>
            <div className="flex gap-2 mt-4">
              <button
                onClick={handleAdd}
                disabled={!form.procedureName}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg disabled:opacity-50"
              >
                إضافة
              </button>
              <button onClick={() => setShowAdd(false)} className="flex-1 bg-gray-200 py-2 rounded-lg">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
