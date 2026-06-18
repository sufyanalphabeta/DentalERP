"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface LabOrderSummary {
  id: string;
  orderNumber: string;
  status: string;
  externalLabName: string | null;
  totalCost: number;
  totalRevenue: number;
  isExternal: boolean;
  createdAt: string;
}

const STATUS_LABELS: Record<string, string> = {
  Draft: "مسودة",
  Sent: "مُرسل",
  InProgress: "جاري",
  ResultReceived: "نتيجة واردة",
  Completed: "مكتمل",
  Cancelled: "ملغى",
};

const STATUS_COLORS: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-700",
  Sent: "bg-blue-100 text-blue-700",
  InProgress: "bg-yellow-100 text-yellow-700",
  ResultReceived: "bg-purple-100 text-purple-700",
  Completed: "bg-green-100 text-green-700",
  Cancelled: "bg-red-100 text-red-700",
};

export default function PatientLabOrdersPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [orders, setOrders] = useState<LabOrderSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [newOrder, setNewOrder] = useState({ description: "", notes: "" });

  useEffect(() => {
    api.get(`/lab/orders?patientId=${id}`).then((r) => {
      setOrders(r.data.items ?? []);
      setLoading(false);
    });
  }, [id]);

  const handleCreate = async () => {
    await api.post("/lab/orders", {
      patientId: id,
      isExternalClient: false,
      ...newOrder,
    });
    setShowCreate(false);
    setLoading(true);
    api.get(`/lab/orders?patientId=${id}`).then((r) => {
      setOrders(r.data.items ?? []);
      setLoading(false);
    });
  };

  return (
    <div className="p-6 max-w-5xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <button onClick={() => router.back()} className="text-sm text-blue-600 hover:underline mb-1">
            → رجوع
          </button>
          <h1 className="text-2xl font-bold text-gray-900">طلبات المختبر</h1>
        </div>
        <button
          onClick={() => setShowCreate(true)}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 text-sm"
        >
          + طلب جديد
        </button>
      </div>

      {showCreate && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md" dir="rtl">
            <h2 className="text-lg font-semibold mb-4">طلب مختبر جديد</h2>
            <div className="space-y-3">
              <textarea
                placeholder="الوصف"
                rows={2}
                className="w-full border rounded-lg p-2 text-sm"
                value={newOrder.description}
                onChange={(e) => setNewOrder({ ...newOrder, description: e.target.value })}
              />
              <textarea
                placeholder="ملاحظات"
                rows={2}
                className="w-full border rounded-lg p-2 text-sm"
                value={newOrder.notes}
                onChange={(e) => setNewOrder({ ...newOrder, notes: e.target.value })}
              />
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={handleCreate} className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700">
                إنشاء
              </button>
              <button onClick={() => setShowCreate(false)} className="flex-1 border py-2 rounded-lg text-sm">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}

      {loading ? (
        <div className="text-center py-12 text-gray-500">جاري التحميل...</div>
      ) : orders.length === 0 ? (
        <div className="text-center py-12 text-gray-400">لا توجد طلبات مختبر</div>
      ) : (
        <div className="space-y-3">
          {orders.map((order) => (
            <div
              key={order.id}
              onClick={() => router.push(`/lab/orders/${order.id}`)}
              className="bg-white border rounded-xl p-4 cursor-pointer hover:shadow-md transition-shadow"
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-semibold text-gray-900">{order.orderNumber}</p>
                  <p className="text-sm text-gray-500 mt-1">
                    {order.externalLabName ?? "—"}
                    {order.isExternal && " (عميل خارجي)"}
                  </p>
                </div>
                <div className="text-left">
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${STATUS_COLORS[order.status] ?? "bg-gray-100 text-gray-700"}`}>
                    {STATUS_LABELS[order.status] ?? order.status}
                  </span>
                  <p className="text-sm text-gray-500 mt-1">{order.totalCost.toFixed(2)} د.ل</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
