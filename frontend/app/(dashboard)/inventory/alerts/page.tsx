"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";

interface StockAlert {
  id: string;
  itemId: string;
  itemName: string;
  alertType: string;
  warehouseName: string;
  currentQuantity: number;
  minimumQuantity: number | null;
  expiryDate: string | null;
  unit: string;
}

const alertTypeAr: Record<string, string> = {
  LowStock: "نفاد المخزون",
  OutOfStock: "نفاد كامل",
  NearExpiry: "قرب انتهاء الصلاحية",
  Expired: "منتهي الصلاحية",
};

const alertColor: Record<string, string> = {
  LowStock: "bg-amber-50 border-amber-200",
  OutOfStock: "bg-red-50 border-red-200",
  NearExpiry: "bg-orange-50 border-orange-200",
  Expired: "bg-red-100 border-red-300",
};

const badgeColor: Record<string, string> = {
  LowStock: "bg-amber-100 text-amber-800",
  OutOfStock: "bg-red-100 text-red-800",
  NearExpiry: "bg-orange-100 text-orange-800",
  Expired: "bg-red-200 text-red-900",
};

export default function InventoryAlertsPage() {
  const [alerts, setAlerts] = useState<StockAlert[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState("all");

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const r = await api.get<StockAlert[] | { items: StockAlert[] }>("/inventory/stock/alerts");
      const data = Array.isArray(r.data) ? r.data : (r.data as { items: StockAlert[] }).items ?? [];
      setAlerts(data);
    } finally {
      setLoading(false);
    }
  }

  const filtered = filter === "all" ? alerts : alerts.filter((a) => a.alertType === filter);

  const counts = {
    LowStock: alerts.filter((a) => a.alertType === "LowStock").length,
    OutOfStock: alerts.filter((a) => a.alertType === "OutOfStock").length,
    NearExpiry: alerts.filter((a) => a.alertType === "NearExpiry").length,
    Expired: alerts.filter((a) => a.alertType === "Expired").length,
  };

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">تنبيهات المخزون</h1>
          <p className="text-gray-500 text-sm">{alerts.length} تنبيه نشط</p>
        </div>
        <div className="flex gap-2">
          <button onClick={load} className="text-sm border px-3 py-1.5 rounded-lg text-gray-600 hover:bg-gray-50">تحديث</button>
          <Link href="/inventory/items" className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">إدارة الأصناف</Link>
        </div>
      </div>

      {/* Summary cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-6">
        {[
          { key: "OutOfStock", label: "نفاد كامل", color: "bg-red-600" },
          { key: "LowStock", label: "مخزون منخفض", color: "bg-amber-500" },
          { key: "Expired", label: "منتهي الصلاحية", color: "bg-red-800" },
          { key: "NearExpiry", label: "قرب انتهاء الصلاحية", color: "bg-orange-500" },
        ].map((s) => (
          <button
            key={s.key}
            onClick={() => setFilter(filter === s.key ? "all" : s.key)}
            className={`rounded-xl p-4 text-right border-2 transition-all ${filter === s.key ? "border-blue-500 shadow-md" : "border-transparent"} ${s.key === "OutOfStock" || s.key === "Expired" ? "bg-red-50" : "bg-amber-50"}`}
          >
            <div className={`text-2xl font-bold ${s.key === "OutOfStock" || s.key === "Expired" ? "text-red-700" : "text-amber-700"}`}>
              {loading ? "—" : counts[s.key as keyof typeof counts]}
            </div>
            <div className="text-xs text-gray-600 mt-0.5">{s.label}</div>
          </button>
        ))}
      </div>

      {loading ? (
        <div className="text-center py-12 text-gray-400">جاري التحميل...</div>
      ) : filtered.length === 0 ? (
        <div className="text-center py-12 bg-green-50 rounded-xl border border-green-200">
          <div className="text-4xl mb-3">✅</div>
          <div className="text-green-700 font-semibold">لا توجد تنبيهات</div>
          <div className="text-green-600 text-sm mt-1">المخزون في حالة جيدة</div>
        </div>
      ) : (
        <div className="space-y-3">
          {filtered.map((alert) => (
            <div key={alert.id} className={`rounded-xl border p-4 ${alertColor[alert.alertType] ?? "bg-gray-50 border-gray-200"}`}>
              <div className="flex items-start justify-between">
                <div>
                  <div className="flex items-center gap-2 mb-1">
                    <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${badgeColor[alert.alertType]}`}>
                      {alertTypeAr[alert.alertType] ?? alert.alertType}
                    </span>
                    <span className="text-xs text-gray-500">{alert.warehouseName}</span>
                  </div>
                  <div className="text-sm font-semibold text-gray-800">{alert.itemName}</div>
                  <div className="text-xs text-gray-600 mt-1">
                    الكمية الحالية: <strong>{alert.currentQuantity} {alert.unit}</strong>
                    {alert.minimumQuantity != null && ` — الحد الأدنى: ${alert.minimumQuantity} ${alert.unit}`}
                  </div>
                  {alert.expiryDate && (
                    <div className="text-xs text-red-600 mt-0.5">
                      تاريخ الانتهاء: {new Date(alert.expiryDate).toLocaleDateString("ar")}
                    </div>
                  )}
                </div>
                <Link href={`/inventory/items`} className="text-xs text-blue-600 hover:underline whitespace-nowrap">عرض الصنف</Link>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
