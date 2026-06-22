'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { api } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';

interface TreatmentPlanItem {
  id: string;
  procedureName: string;
  toothId?: number;
  surface?: string;
  quantity: number;
  unitPrice: number;
  discountPercent: number;
  totalPrice: number;
  status: string;
  sequenceNumber: number;
}

interface TreatmentPlan {
  id: string;
  title: string;
  priority: string;
  status: string;
  estimatedCost: number;
  totalCost: number;
  actualCost: number;
  paidAmount: number;
  createdAt: string;
  items?: TreatmentPlanItem[];
}

const priorityColors: Record<string, string> = {
  Low: 'bg-gray-100 text-gray-600',
  Normal: 'bg-blue-100 text-blue-700',
  High: 'bg-orange-100 text-orange-700',
  Urgent: 'bg-red-100 text-red-700',
};

const priorityAr: Record<string, string> = {
  Low: 'منخفضة', Normal: 'عادية', High: 'عالية', Urgent: 'عاجلة',
};

const statusColors: Record<string, string> = {
  Draft: 'bg-gray-100 text-gray-600',
  Active: 'bg-green-100 text-green-700',
  Completed: 'bg-blue-100 text-blue-700',
  Cancelled: 'bg-red-100 text-red-600',
  OnHold: 'bg-yellow-100 text-yellow-700',
};

const statusAr: Record<string, string> = {
  Draft: 'مسودة', Active: 'نشطة', Completed: 'مكتملة',
  Cancelled: 'ملغاة', OnHold: 'موقوفة',
};

export default function TreatmentPlansPage() {
  const { id } = useParams<{ id: string }>();
  const token = useAuthStore((s) => s.accessToken);
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const [plans, setPlans] = useState<TreatmentPlan[]>([]);
  const [loading, setLoading] = useState(true);
  const [showNew, setShowNew] = useState(false);
  const [newPlan, setNewPlan] = useState({ title: '', estimatedCost: '', priority: 'Normal', description: '' });

  const load = () => {
    if (!id || !token) return;
    api.get<TreatmentPlan[]>(`/patients/${id}/treatment-plans`)
      .then((r) => setPlans(r.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  };

  useEffect(load, [id, token]);

  const handleCreate = async () => {
    await api.post(`/patients/${id}/treatment-plans`, {
      title: newPlan.title,
      estimatedCost: parseFloat(newPlan.estimatedCost),
      priority: newPlan.priority,
      description: newPlan.description || null,
    });
    setShowNew(false);
    setNewPlan({ title: '', estimatedCost: '', priority: 'Normal', description: '' });
    setLoading(true);
    load();
  };

  const handleStatusChange = async (planId: string, status: string) => {
    await api.patch(`/treatment-plans/${planId}/status`, { status });
    setLoading(true);
    load();
  };

  if (loading) return <div className="p-6">جارٍ التحميل...</div>;

  return (
    <div className="p-6 max-w-4xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-xl font-bold">خطط العلاج</h1>
        {hasPermission('Patients.Edit') && (
          <button
            onClick={() => setShowNew(true)}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm"
          >
            + خطة جديدة
          </button>
        )}
      </div>

      {plans.length === 0 ? (
        <div className="bg-gray-50 rounded-xl p-8 text-center text-gray-500">
          لا توجد خطط علاج بعد
        </div>
      ) : (
        <div className="space-y-4">
          {plans.map((plan) => (
            <div key={plan.id} className="bg-white rounded-xl border border-gray-200 p-5">
              <div className="flex items-start justify-between mb-3">
                <div>
                  <h3 className="font-semibold text-gray-900">{plan.title}</h3>
                  <div className="flex gap-2 mt-1">
                    <span className={`text-xs px-2 py-0.5 rounded ${statusColors[plan.status]}`}>
                      {statusAr[plan.status]}
                    </span>
                    <span className={`text-xs px-2 py-0.5 rounded ${priorityColors[plan.priority]}`}>
                      {priorityAr[plan.priority]}
                    </span>
                  </div>
                </div>
                {hasPermission('Patients.Edit') && plan.status === 'Draft' && (
                  <button
                    onClick={() => handleStatusChange(plan.id, 'Active')}
                    className="text-xs bg-green-600 text-white px-3 py-1 rounded"
                  >
                    تفعيل
                  </button>
                )}
                {hasPermission('Patients.Edit') && plan.status === 'Active' && (
                  <div className="flex gap-2">
                    <button
                      onClick={() => handleStatusChange(plan.id, 'Completed')}
                      className="text-xs bg-blue-600 text-white px-3 py-1 rounded"
                    >
                      إتمام
                    </button>
                    <button
                      onClick={() => handleStatusChange(plan.id, 'OnHold')}
                      className="text-xs bg-yellow-500 text-white px-3 py-1 rounded"
                    >
                      تعليق
                    </button>
                  </div>
                )}
              </div>

              {/* Cost Summary */}
              <div className="grid grid-cols-4 gap-3 bg-gray-50 rounded-lg p-3 text-sm">
                <div className="text-center">
                  <div className="text-gray-500 text-xs">التقدير</div>
                  <div className="font-semibold">{plan.estimatedCost.toLocaleString()} ر.س</div>
                </div>
                <div className="text-center">
                  <div className="text-gray-500 text-xs">الإجمالي</div>
                  <div className="font-semibold">{plan.totalCost.toLocaleString()} ر.س</div>
                </div>
                <div className="text-center">
                  <div className="text-gray-500 text-xs">المنفَّذ</div>
                  <div className="font-semibold text-green-600">{plan.actualCost.toLocaleString()} ر.س</div>
                </div>
                <div className="text-center">
                  <div className="text-gray-500 text-xs">المتبقي</div>
                  <div className="font-semibold text-orange-600">
                    {(plan.totalCost - plan.paidAmount).toLocaleString()} ر.س
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* New Plan Modal */}
      {showNew && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-96" dir="rtl">
            <h3 className="font-bold text-lg mb-4">خطة علاج جديدة</h3>
            <div className="space-y-3">
              <input
                placeholder="عنوان الخطة *"
                value={newPlan.title}
                onChange={(e) => setNewPlan((f) => ({ ...f, title: e.target.value }))}
                className="w-full border rounded-lg p-2"
              />
              <input
                type="number"
                placeholder="التكلفة التقديرية *"
                value={newPlan.estimatedCost}
                onChange={(e) => setNewPlan((f) => ({ ...f, estimatedCost: e.target.value }))}
                className="w-full border rounded-lg p-2"
              />
              <select
                value={newPlan.priority}
                onChange={(e) => setNewPlan((f) => ({ ...f, priority: e.target.value }))}
                className="w-full border rounded-lg p-2"
              >
                {Object.entries(priorityAr).map(([k, v]) => (
                  <option key={k} value={k}>{v}</option>
                ))}
              </select>
              <textarea
                placeholder="وصف الخطة (اختياري)"
                value={newPlan.description}
                onChange={(e) => setNewPlan((f) => ({ ...f, description: e.target.value }))}
                className="w-full border rounded-lg p-2 h-20"
              />
            </div>
            <div className="flex gap-2 mt-4">
              <button
                onClick={handleCreate}
                disabled={!newPlan.title || !newPlan.estimatedCost}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg disabled:opacity-50"
              >
                إنشاء
              </button>
              <button onClick={() => setShowNew(false)} className="flex-1 bg-gray-200 py-2 rounded-lg">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
