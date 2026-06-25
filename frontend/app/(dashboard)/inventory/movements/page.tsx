"use client";

import { useEffect, useState, Suspense } from "react";
import { useSearchParams } from "next/navigation";
import { api } from "@/lib/api";

interface Movement {
  id: string;
  movementNumber: string;
  itemName: string;
  itemCode: string;
  movementType: string;
  direction: string;
  quantity: number;
  unitCost: number | null;
  totalCost: number | null;
  warehouseName: string | null;
  destinationType: string | null;
  destinationId: string | null;
  isNegativeStock: boolean;
  notes: string | null;
  createdAt: string;
}

interface MovementsResponse {
  movements: Movement[];
  total: number;
}

interface Item {
  id: string;
  name: string;
}

const typeAr: Record<string, string> = {
  PurchaseReceipt:      "استلام شراء",
  ManualIssue:          "صرف يدوي",
  LabConsumption:       "استهلاك مختبر",
  RadiologyConsumption: "استهلاك أشعة",
  Adjustment:           "تسوية",
  WriteOff:             "شطب",
  SupplierReturn:       "إرجاع للمورد",
  Transfer:             "نقل",
};

const typeCls: Record<string, string> = {
  PurchaseReceipt:      "bg-green-100 text-green-700",
  ManualIssue:          "bg-orange-100 text-orange-700",
  LabConsumption:       "bg-orange-100 text-orange-700",
  RadiologyConsumption: "bg-orange-100 text-orange-700",
  Adjustment:           "bg-blue-100 text-blue-700",
  WriteOff:             "bg-red-100 text-red-700",
  SupplierReturn:       "bg-amber-100 text-amber-700",
  Transfer:             "bg-purple-100 text-purple-700",
};

function MovementsContent() {
  const searchParams = useSearchParams();
  const initialItemId = searchParams.get("itemId") ?? "";

  const [data, setData] = useState<MovementsResponse | null>(null);
  const [items, setItems] = useState<Item[]>([]);
  const [loading, setLoading] = useState(true);
  const [itemFilter, setItemFilter] = useState(initialItemId);
  const [typeFilter, setTypeFilter] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [page, setPage] = useState(1);

  useEffect(() => {
    api.get<{ items: Item[] }>("/inventory/items?pageSize=500").then((r) => setItems(r.data.items ?? [])).catch(() => {});
  }, []);

  useEffect(() => { load(); }, [itemFilter, typeFilter, from, to, page]);

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: "30" });
      if (itemFilter) params.set("itemId", itemFilter);
      if (typeFilter) params.set("movementType", typeFilter);
      if (from) params.set("from", from + "T00:00:00.000Z");
      if (to)   params.set("to",   to   + "T23:59:59.999Z");
      const r = await api.get<MovementsResponse>(`/inventory/movements?${params}`);
      setData(r.data);
    } finally {
      setLoading(false);
    }
  }

  function reset() {
    setItemFilter("");
    setTypeFilter("");
    setFrom("");
    setTo("");
    setPage(1);
  }

  function setQuickDate(days: number) {
    const now = new Date();
    const start = new Date();
    start.setDate(now.getDate() - days);
    setFrom(start.toISOString().slice(0, 10));
    setTo(now.toISOString().slice(0, 10));
    setPage(1);
  }

  const totalPages = data ? Math.ceil(data.total / 30) : 1;
  const selectedItemName = items.find((i) => i.id === itemFilter)?.name;

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">حركة المخزون</h1>
          {selectedItemName && (
            <p className="text-sm text-blue-600 mt-0.5">الصنف: {selectedItemName}</p>
          )}
        </div>
        {data && (
          <div className="text-sm text-gray-500">{data.total} حركة</div>
        )}
      </div>

      {/* Filters */}
      <div className="bg-white rounded-xl shadow p-4 mb-4 space-y-3">
        <div className="flex flex-wrap gap-3">
          {/* Item filter */}
          <select
            value={itemFilter}
            onChange={(e) => { setItemFilter(e.target.value); setPage(1); }}
            className="border rounded-lg px-3 py-2 text-sm min-w-[180px] focus:outline-none focus:ring-2 focus:ring-blue-400"
          >
            <option value="">كل الأصناف</option>
            {items.map((i) => <option key={i.id} value={i.id}>{i.name}</option>)}
          </select>

          {/* Movement type filter */}
          <select
            value={typeFilter}
            onChange={(e) => { setTypeFilter(e.target.value); setPage(1); }}
            className="border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          >
            <option value="">كل أنواع الحركات</option>
            {Object.entries(typeAr).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
          </select>

          {/* Date range */}
          <div className="flex items-center gap-2">
            <span className="text-xs text-gray-500">من</span>
            <input
              type="date"
              value={from}
              onChange={(e) => { setFrom(e.target.value); setPage(1); }}
              className="border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
            <span className="text-xs text-gray-500">إلى</span>
            <input
              type="date"
              value={to}
              onChange={(e) => { setTo(e.target.value); setPage(1); }}
              className="border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
          </div>

          <button
            onClick={reset}
            className="text-sm text-gray-500 border px-3 py-2 rounded-lg hover:bg-gray-50"
          >
            إعادة تعيين
          </button>
        </div>

        {/* Quick date shortcuts */}
        <div className="flex flex-wrap gap-2">
          <span className="text-xs text-gray-400 self-center">فترة سريعة:</span>
          {[
            { label: "اليوم", days: 0 },
            { label: "7 أيام", days: 7 },
            { label: "30 يوم", days: 30 },
            { label: "90 يوم", days: 90 },
            { label: "6 أشهر", days: 180 },
            { label: "سنة", days: 365 },
          ].map((q) => {
            const now = new Date();
            const start = new Date();
            start.setDate(now.getDate() - q.days);
            const qFrom = q.days === 0 ? now.toISOString().slice(0, 10) : start.toISOString().slice(0, 10);
            const qTo = now.toISOString().slice(0, 10);
            const active = from === qFrom && to === qTo;
            return (
              <button
                key={q.label}
                onClick={() => { setFrom(qFrom); setTo(qTo); setPage(1); }}
                className={`text-xs px-3 py-1 rounded-full border transition-colors ${
                  active
                    ? "bg-blue-600 text-white border-blue-600"
                    : "border-gray-300 text-gray-600 hover:bg-gray-50"
                }`}
              >
                {q.label}
              </button>
            );
          })}
        </div>
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الصنف</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">نوع الحركة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الكمية</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المستودع</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">رقم الحركة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">التاريخ</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">جاري التحميل...</td></tr>
            ) : (data?.movements ?? []).length === 0 ? (
              <tr>
                <td colSpan={6} className="text-center py-12 text-gray-400">
                  <div className="text-3xl mb-2">📦</div>
                  <div>لا توجد حركات مخزون</div>
                </td>
              </tr>
            ) : (data?.movements ?? []).map((m) => (
              <tr key={m.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <div className="text-sm font-medium text-gray-800">{m.itemName}</div>
                  <div className="text-xs text-gray-400 font-mono">{m.itemCode}</div>
                </td>
                <td className="px-4 py-3">
                  <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${typeCls[m.movementType] ?? "bg-gray-100 text-gray-600"}`}>
                    {typeAr[m.movementType] ?? m.movementType}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <span className={`text-sm font-semibold ${m.direction === "out" ? "text-red-600" : "text-green-700"}`}>
                    {m.direction === "out" ? "−" : "+"}{m.quantity}
                  </span>
                  {m.totalCost != null && m.totalCost > 0 && (
                    <div className="text-xs text-gray-400">{m.totalCost.toFixed(2)} د.ل</div>
                  )}
                </td>
                <td className="px-4 py-3 text-sm text-gray-600">{m.warehouseName ?? "—"}</td>
                <td className="px-4 py-3 text-xs text-gray-500 font-mono">{m.movementNumber}</td>
                <td className="px-4 py-3 text-xs text-gray-500">
                  {new Date(m.createdAt).toLocaleDateString("ar-LY", { year: "numeric", month: "short", day: "numeric" })}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {totalPages > 1 && (
          <div className="px-4 py-3 border-t flex justify-center gap-2">
            <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-3 py-1 rounded border text-sm disabled:opacity-40">السابق</button>
            <span className="px-3 py-1 text-sm text-gray-600">{page} / {totalPages}</span>
            <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="px-3 py-1 rounded border text-sm disabled:opacity-40">التالي</button>
          </div>
        )}
      </div>
    </div>
  );
}

export default function InventoryMovementsPage() {
  return (
    <Suspense fallback={<div className="p-6 text-center text-gray-400">جاري التحميل...</div>}>
      <MovementsContent />
    </Suspense>
  );
}
