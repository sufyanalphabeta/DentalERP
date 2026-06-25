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
  daysLeft: number | null;
  unit: string;
}

interface LowStockAlert {
  itemId: string;
  itemCode: string;
  name: string;
  currentStock: number;
  reorderLevel: number;
  reorderQuantity: number;
}

interface ExpiryAlert {
  batchId: string;
  itemId: string;
  itemCode: string;
  itemName: string;
  warehouseName: string;
  quantity: number;
  expiryDate: string;
  daysLeft: number;
  severity: string;
}

interface StockAlertsDto {
  lowStockAlerts: LowStockAlert[];
  expiryAlerts: ExpiryAlert[];
}

const alertTypeAr: Record<string, string> = {
  LowStock:   "مخزون منخفض",
  OutOfStock: "نفاد كامل",
  NearExpiry: "قرب انتهاء الصلاحية",
  Expired:    "منتهي الصلاحية",
};

const alertColor: Record<string, string> = {
  LowStock:   "bg-amber-50 border-amber-200",
  OutOfStock: "bg-red-50 border-red-200",
  NearExpiry: "bg-orange-50 border-orange-200",
  Expired:    "bg-red-100 border-red-300",
};

const badgeColor: Record<string, string> = {
  LowStock:   "bg-amber-100 text-amber-800",
  OutOfStock: "bg-red-100 text-red-800",
  NearExpiry: "bg-orange-100 text-orange-800",
  Expired:    "bg-red-200 text-red-900",
};

const expiryPeriods = [
  { label: "منتهية",       maxDays: 0   },
  { label: "خلال أسبوع",  maxDays: 7   },
  { label: "خلال شهر",    maxDays: 30  },
  { label: "خلال 3 أشهر", maxDays: 90  },
  { label: "خلال 6 أشهر", maxDays: 180 },
  { label: "خلال سنة",    maxDays: 365 },
];

export default function InventoryAlertsPage() {
  const [alerts, setAlerts] = useState<StockAlert[]>([]);
  const [loading, setLoading] = useState(true);
  const [typeFilter, setTypeFilter] = useState("all");
  const [expiryPeriod, setExpiryPeriod] = useState<number | null>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const r = await api.get<StockAlertsDto>("/inventory/stock/alerts");
      const dto = r.data;
      const unified: StockAlert[] = [
        ...(dto.lowStockAlerts ?? []).map((a) => ({
          id: a.itemId,
          itemId: a.itemId,
          itemName: a.name,
          alertType: a.currentStock === 0 ? "OutOfStock" : "LowStock",
          warehouseName: "—",
          currentQuantity: a.currentStock,
          minimumQuantity: a.reorderLevel,
          expiryDate: null,
          daysLeft: null,
          unit: "",
        })),
        ...(dto.expiryAlerts ?? []).map((a) => ({
          id: a.batchId,
          itemId: a.itemId,
          itemName: a.itemName,
          alertType: a.daysLeft <= 0 ? "Expired" : "NearExpiry",
          warehouseName: a.warehouseName,
          currentQuantity: a.quantity,
          minimumQuantity: null,
          expiryDate: a.expiryDate,
          daysLeft: a.daysLeft,
          unit: "",
        })),
      ];
      setAlerts(unified);
    } finally {
      setLoading(false);
    }
  }

  const counts = {
    LowStock:   alerts.filter((a) => a.alertType === "LowStock").length,
    OutOfStock: alerts.filter((a) => a.alertType === "OutOfStock").length,
    NearExpiry: alerts.filter((a) => a.alertType === "NearExpiry").length,
    Expired:    alerts.filter((a) => a.alertType === "Expired").length,
  };

  const filtered = alerts.filter((a) => {
    if (typeFilter !== "all" && a.alertType !== typeFilter) return false;
    if (expiryPeriod !== null) {
      if (a.daysLeft === null) return false;
      if (expiryPeriod === 0) return a.daysLeft <= 0;
      return a.daysLeft > 0 && a.daysLeft <= expiryPeriod;
    }
    return true;
  });

  const showExpiryFilter = typeFilter === "all" || typeFilter === "NearExpiry" || typeFilter === "Expired";

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
      <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-4">
        {[
          { key: "OutOfStock", label: "نفاد كامل",          bg: "bg-red-50",    text: "text-red-700"    },
          { key: "LowStock",   label: "مخزون منخفض",        bg: "bg-amber-50",  text: "text-amber-700"  },
          { key: "Expired",    label: "منتهي الصلاحية",      bg: "bg-red-100",   text: "text-red-800"    },
          { key: "NearExpiry", label: "قرب انتهاء الصلاحية", bg: "bg-orange-50", text: "text-orange-700" },
        ].map((s) => (
          <button
            key={s.key}
            onClick={() => {
              setTypeFilter(typeFilter === s.key ? "all" : s.key);
              setExpiryPeriod(null);
            }}
            className={`rounded-xl p-4 text-right border-2 transition-all ${
              typeFilter === s.key ? "border-blue-500 shadow-md" : "border-transparent"
            } ${s.bg}`}
          >
            <div className={`text-2xl font-bold ${s.text}`}>
              {loading ? "—" : counts[s.key as keyof typeof counts]}
            </div>
            <div className="text-xs text-gray-600 mt-0.5">{s.label}</div>
          </button>
        ))}
      </div>

      {/* Expiry period filter */}
      {showExpiryFilter && (counts.NearExpiry + counts.Expired) > 0 && (
        <div className="bg-white rounded-xl shadow p-4 mb-4">
          <div className="flex flex-wrap items-center gap-2">
            <span className="text-xs font-medium text-gray-500 ml-2">تصفية حسب انتهاء الصلاحية:</span>
            <button
              onClick={() => setExpiryPeriod(null)}
              className={`text-xs px-3 py-1.5 rounded-full border transition-colors ${
                expiryPeriod === null
                  ? "bg-blue-600 text-white border-blue-600"
                  : "border-gray-300 text-gray-600 hover:bg-gray-50"
              }`}
            >
              الكل
            </button>
            {expiryPeriods.map((p) => (
              <button
                key={p.maxDays}
                onClick={() => setExpiryPeriod(expiryPeriod === p.maxDays ? null : p.maxDays)}
                className={`text-xs px-3 py-1.5 rounded-full border transition-colors ${
                  expiryPeriod === p.maxDays
                    ? p.maxDays === 0
                      ? "bg-red-600 text-white border-red-600"
                      : p.maxDays <= 7
                      ? "bg-red-500 text-white border-red-500"
                      : p.maxDays <= 30
                      ? "bg-orange-500 text-white border-orange-500"
                      : "bg-amber-500 text-white border-amber-500"
                    : "border-gray-300 text-gray-600 hover:bg-gray-50"
                }`}
              >
                {p.label}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Results */}
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
                <div className="flex-1">
                  <div className="flex items-center gap-2 mb-1 flex-wrap">
                    <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${badgeColor[alert.alertType]}`}>
                      {alertTypeAr[alert.alertType] ?? alert.alertType}
                    </span>
                    {alert.warehouseName && alert.warehouseName !== "—" && (
                      <span className="text-xs text-gray-500 bg-gray-100 px-2 py-0.5 rounded-full">{alert.warehouseName}</span>
                    )}
                    {alert.daysLeft !== null && alert.daysLeft > 0 && (
                      <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                        alert.daysLeft <= 7 ? "bg-red-100 text-red-700" :
                        alert.daysLeft <= 30 ? "bg-orange-100 text-orange-700" :
                        "bg-amber-50 text-amber-700"
                      }`}>
                        متبقي {alert.daysLeft} يوم
                      </span>
                    )}
                    {alert.daysLeft !== null && alert.daysLeft <= 0 && (
                      <span className="text-xs px-2 py-0.5 rounded-full bg-red-200 text-red-800 font-medium">
                        منتهي الصلاحية
                      </span>
                    )}
                  </div>
                  <div className="text-sm font-semibold text-gray-800">{alert.itemName}</div>
                  <div className="text-xs text-gray-600 mt-1">
                    الكمية الحالية: <strong>{alert.currentQuantity}</strong>
                    {alert.minimumQuantity != null && ` — حد إعادة الطلب: ${alert.minimumQuantity}`}
                  </div>
                  {alert.expiryDate && (
                    <div className="text-xs text-red-600 mt-0.5">
                      تاريخ الانتهاء:{" "}
                      {new Date(alert.expiryDate).toLocaleDateString("ar-LY", {
                        year: "numeric", month: "long", day: "numeric",
                      })}
                    </div>
                  )}
                </div>
                <Link href="/inventory/items" className="text-xs text-blue-600 hover:underline whitespace-nowrap mr-4">
                  عرض الصنف
                </Link>
              </div>
            </div>
          ))}
          <div className="text-center pt-2 text-xs text-gray-400">
            عرض {filtered.length} من {alerts.length} تنبيه
          </div>
        </div>
      )}
    </div>
  );
}
