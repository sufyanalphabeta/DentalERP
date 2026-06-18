"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface LabOrderDetail {
  id: string;
  orderNumber: string;
  status: string;
  patientId: string | null;
  isExternal: boolean;
  totalCost: number;
  totalRevenue: number;
  description: string | null;
  notes: string | null;
  cancelledReason: string | null;
  createdAt: string;
  sentAt: string | null;
  receivedAt: string | null;
  items: { id: string; itemName: string; unitCost: number; quantity: number; totalCost: number }[];
  results: { id: string; resultNotes: string | null; fileName: string | null; receivedAt: string }[];
}

const STATUS_LABELS: Record<string, string> = {
  Draft: "مسودة",
  Sent: "مُرسل",
  InProgress: "جاري",
  ResultReceived: "نتيجة واردة",
  Completed: "مكتمل",
  Cancelled: "ملغى",
};

export default function LabOrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [order, setOrder] = useState<LabOrderDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState("");

  const load = () => {
    setLoading(true);
    api.get(`/lab/orders/${id}`).then((r) => {
      setOrder(r.data);
      setLoading(false);
    });
  };

  useEffect(load, [id]);

  const handleSend = async () => {
    await api.post(`/lab/orders/${id}/send`);
    load();
  };

  const handleComplete = async () => {
    await api.post(`/lab/orders/${id}/complete`);
    load();
  };

  const handleCancel = async () => {
    await api.post(`/lab/orders/${id}/cancel`, { reason: cancelReason });
    setShowCancelModal(false);
    load();
  };

  if (loading) return <div className="p-6 text-center text-gray-500">جاري التحميل...</div>;
  if (!order) return <div className="p-6 text-center text-red-500">لم يتم العثور على الطلب</div>;

  return (
    <div className="p-6 max-w-4xl mx-auto" dir="rtl">
      <button onClick={() => router.back()} className="text-sm text-blue-600 hover:underline mb-4">
        → رجوع
      </button>

      <div className="bg-white border rounded-xl p-6 mb-6">
        <div className="flex items-start justify-between mb-4">
          <div>
            <h1 className="text-xl font-bold text-gray-900">{order.orderNumber}</h1>
            <p className="text-sm text-gray-500 mt-1">{order.description}</p>
          </div>
          <span className="px-3 py-1 bg-blue-100 text-blue-700 rounded-full text-sm font-medium">
            {STATUS_LABELS[order.status] ?? order.status}
          </span>
        </div>

        <div className="grid grid-cols-3 gap-4 text-sm mb-6">
          <div>
            <p className="text-gray-500">إجمالي التكلفة</p>
            <p className="font-semibold">{order.totalCost.toFixed(2)} د.ل</p>
          </div>
          <div>
            <p className="text-gray-500">إجمالي الإيراد</p>
            <p className="font-semibold">{order.totalRevenue.toFixed(2)} د.ل</p>
          </div>
          <div>
            <p className="text-gray-500">تاريخ الإرسال</p>
            <p className="font-semibold">{order.sentAt ? new Date(order.sentAt).toLocaleDateString("ar-LY") : "—"}</p>
          </div>
        </div>

        <div className="flex gap-3">
          {order.status === "Draft" && (
            <button onClick={handleSend} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">
              إرسال للمختبر
            </button>
          )}
          {order.status === "ResultReceived" && (
            <button onClick={handleComplete} className="bg-green-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-green-700">
              اعتماد واكتمال
            </button>
          )}
          {!["Completed", "Cancelled"].includes(order.status) && (
            <button onClick={() => setShowCancelModal(true)} className="border border-red-300 text-red-600 px-4 py-2 rounded-lg text-sm hover:bg-red-50">
              إلغاء الطلب
            </button>
          )}
        </div>
      </div>

      {order.items.length > 0 && (
        <div className="bg-white border rounded-xl p-5 mb-6">
          <h2 className="font-semibold text-gray-700 mb-3">بنود الطلب</h2>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-gray-500 border-b">
                <th className="text-right pb-2">البند</th>
                <th className="text-right pb-2">الكمية</th>
                <th className="text-right pb-2">سعر الوحدة</th>
                <th className="text-right pb-2">الإجمالي</th>
              </tr>
            </thead>
            <tbody>
              {order.items.map((item) => (
                <tr key={item.id} className="border-b last:border-0">
                  <td className="py-2">{item.itemName}</td>
                  <td className="py-2">{item.quantity}</td>
                  <td className="py-2">{item.unitCost.toFixed(2)}</td>
                  <td className="py-2 font-medium">{item.totalCost.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {order.results.length > 0 && (
        <div className="bg-white border rounded-xl p-5">
          <h2 className="font-semibold text-gray-700 mb-3">نتائج المختبر</h2>
          {order.results.map((result) => (
            <div key={result.id} className="border-b last:border-0 pb-3 mb-3">
              <p className="text-sm text-gray-800">{result.resultNotes ?? "—"}</p>
              {result.fileName && (
                <p className="text-xs text-blue-600 mt-1">{result.fileName}</p>
              )}
              <p className="text-xs text-gray-400 mt-1">
                {new Date(result.receivedAt).toLocaleDateString("ar-LY")}
              </p>
            </div>
          ))}
        </div>
      )}

      {showCancelModal && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md" dir="rtl">
            <h2 className="text-lg font-semibold mb-4 text-red-600">إلغاء الطلب</h2>
            <textarea
              placeholder="سبب الإلغاء..."
              rows={3}
              className="w-full border rounded-lg p-2 text-sm"
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
            />
            <div className="flex gap-3 mt-4">
              <button onClick={handleCancel} disabled={!cancelReason.trim()} className="flex-1 bg-red-600 text-white py-2 rounded-lg text-sm hover:bg-red-700 disabled:opacity-50">
                تأكيد الإلغاء
              </button>
              <button onClick={() => setShowCancelModal(false)} className="flex-1 border py-2 rounded-lg text-sm">
                رجوع
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
