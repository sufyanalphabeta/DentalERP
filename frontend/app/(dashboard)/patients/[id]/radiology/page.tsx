"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface RadiologyOrderSummary {
  id: string;
  orderNumber: string;
  status: string;
  radiologyTypeName: string;
  price: number;
  isExternalPatient: boolean;
  externalPatientName: string | null;
  doctorId: string | null;
  orderDate: string;
}

interface RadiologyType {
  id: string;
  name: string;
  nameAr: string | null;
  basePrice: number;
}

const STATUS_LABELS: Record<string, string> = {
  Ordered: "مطلوب",
  Imaged: "تصوير مكتمل",
  ReportSaved: "تقرير محفوظ",
  Completed: "مكتمل",
  Cancelled: "ملغى",
};

const STATUS_COLORS: Record<string, string> = {
  Ordered: "bg-blue-100 text-blue-700",
  Imaged: "bg-yellow-100 text-yellow-700",
  ReportSaved: "bg-purple-100 text-purple-700",
  Completed: "bg-green-100 text-green-700",
  Cancelled: "bg-red-100 text-red-700",
};

export default function PatientRadiologyPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [orders, setOrders] = useState<RadiologyOrderSummary[]>([]);
  const [types, setTypes] = useState<RadiologyType[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [newOrder, setNewOrder] = useState({ radiologyTypeId: "", price: 0, notes: "" });

  const load = () => {
    api.get(`/radiology/orders?patientId=${id}`).then((r) => {
      setOrders(r.data.items ?? []);
      setLoading(false);
    });
  };

  useEffect(() => {
    Promise.all([
      api.get(`/radiology/orders?patientId=${id}`),
      api.get("/radiology/types"),
    ]).then(([ordersRes, typesRes]) => {
      setOrders(ordersRes.data.items ?? []);
      setTypes(typesRes.data ?? []);
      setLoading(false);
    });
  }, [id]);

  const handleCreate = async () => {
    await api.post("/radiology/orders", {
      patientId: id,
      isExternalPatient: false,
      radiologyTypeId: newOrder.radiologyTypeId,
      price: newOrder.price,
      notes: newOrder.notes || null,
    });
    setShowCreate(false);
    load();
  };

  const handleTypeSelect = (typeId: string) => {
    const type = types.find((t) => t.id === typeId);
    setNewOrder({ ...newOrder, radiologyTypeId: typeId, price: type?.basePrice ?? 0 });
  };

  return (
    <div className="p-6 max-w-5xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <button onClick={() => router.back()} className="text-sm text-blue-600 hover:underline mb-1">
            → رجوع
          </button>
          <h1 className="text-2xl font-bold text-gray-900">طلبات الأشعة</h1>
        </div>
        <button
          onClick={() => setShowCreate(true)}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 text-sm"
        >
          + طلب أشعة جديد
        </button>
      </div>

      {showCreate && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md" dir="rtl">
            <h2 className="text-lg font-semibold mb-4">طلب أشعة جديد</h2>
            <div className="space-y-3">
              <select
                className="w-full border rounded-lg p-2 text-sm"
                value={newOrder.radiologyTypeId}
                onChange={(e) => handleTypeSelect(e.target.value)}
              >
                <option value="">اختر نوع الأشعة</option>
                {types.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.nameAr ?? t.name}
                  </option>
                ))}
              </select>
              <input
                type="number"
                placeholder="السعر"
                className="w-full border rounded-lg p-2 text-sm"
                value={newOrder.price}
                onChange={(e) => setNewOrder({ ...newOrder, price: Number(e.target.value) })}
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
              <button
                onClick={handleCreate}
                disabled={!newOrder.radiologyTypeId}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50"
              >
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
        <div className="text-center py-12 text-gray-400">لا توجد طلبات أشعة</div>
      ) : (
        <div className="space-y-3">
          {orders.map((order) => (
            <div
              key={order.id}
              onClick={() => router.push(`/radiology/orders/${order.id}`)}
              className="bg-white border rounded-xl p-4 cursor-pointer hover:shadow-md transition-shadow"
            >
              <div className="flex items-center justify-between">
                <div>
                  <p className="font-semibold text-gray-900">{order.orderNumber}</p>
                  <p className="text-sm text-gray-500 mt-1">{order.radiologyTypeName}</p>
                </div>
                <div className="text-left">
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${STATUS_COLORS[order.status] ?? "bg-gray-100 text-gray-700"}`}>
                    {STATUS_LABELS[order.status] ?? order.status}
                  </span>
                  <p className="text-sm text-gray-500 mt-1">{order.price.toFixed(2)} د.ل</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
