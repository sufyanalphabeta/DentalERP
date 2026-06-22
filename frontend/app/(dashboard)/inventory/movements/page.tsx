"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface Movement {
  id: string;
  itemId: string;
  itemName: string;
  movementType: string;
  quantity: number;
  unitName: string | null;
  warehouseName: string | null;
  reference: string | null;
  notes: string | null;
  createdAt: string;
  createdBy: string | null;
}

interface MovementsResponse {
  items: Movement[];
  totalCount: number;
  page: number;
  pageSize: number;
}

interface Item {
  id: string;
  name: string;
}

const typeAr: Record<string, string> = {
  In: "وارد",
  Out: "صادر",
  Adjustment: "تسوية",
  Transfer: "نقل",
  Return: "مرتجع",
  Purchase: "شراء",
  Issue: "صرف",
};

const typeCls: Record<string, string> = {
  In: "bg-green-100 text-green-700",
  Out: "bg-red-100 text-red-700",
  Adjustment: "bg-blue-100 text-blue-700",
  Transfer: "bg-purple-100 text-purple-700",
  Return: "bg-amber-100 text-amber-700",
  Purchase: "bg-emerald-100 text-emerald-700",
  Issue: "bg-orange-100 text-orange-700",
};

export default function InventoryMovementsPage() {
  const [data, setData] = useState<MovementsResponse | null>(null);
  const [items, setItems] = useState<Item[]>([]);
  const [loading, setLoading] = useState(true);
  const [itemFilter, setItemFilter] = useState("");
  const [typeFilter, setTypeFilter] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [page, setPage] = useState(1);

  useEffect(() => {
    api.get<{ items: Item[] }>("/inventory/items?pageSize=200").then((r) => setItems(r.data.items ?? [])).catch(() => {});
  }, []);

  useEffect(() => { load(); }, [itemFilter, typeFilter, from, to, page]);

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: "30" });
      if (itemFilter) params.set("itemId", itemFilter);
      if (typeFilter) params.set("movementType", typeFilter);
      if (from) params.set("from", from);
      if (to) params.set("to", to);
      const r = await api.get<MovementsResponse>(`/inventory/movements?${params}`);
      setData(r.data);
    } finally {
      setLoading(false);
    }
  }

  const totalPages = data ? Math.ceil(data.totalCount / 30) : 1;

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">حركة المخزون</h1>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-3 mb-4">
        <select value={itemFilter} onChange={(e) => { setItemFilter(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm">
          <option value="">كل الأصناف</option>
          {items.map((i) => <option key={i.id} value={i.id}>{i.name}</option>)}
        </select>
        <select value={typeFilter} onChange={(e) => { setTypeFilter(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm">
          <option value="">كل الحركات</option>
          {Object.entries(typeAr).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
        </select>
        <input type="date" value={from} onChange={(e) => { setFrom(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm" />
        <input type="date" value={to} onChange={(e) => { setTo(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm" />
        <button onClick={() => { setItemFilter(""); setTypeFilter(""); setFrom(""); setTo(""); setPage(1); }} className="text-sm text-gray-500 border px-3 py-2 rounded-lg hover:bg-gray-50">إعادة تعيين</button>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الصنف</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">نوع الحركة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الكمية</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المستودع</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المرجع</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">التاريخ</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">جاري التحميل...</td></tr>
            ) : (data?.items ?? []).length === 0 ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">لا توجد حركات</td></tr>
            ) : (data?.items ?? []).map((m) => (
              <tr key={m.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 text-sm font-medium text-gray-800">{m.itemName}</td>
                <td className="px-4 py-3">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${typeCls[m.movementType] ?? "bg-gray-100 text-gray-600"}`}>
                    {typeAr[m.movementType] ?? m.movementType}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <span className={`text-sm font-medium ${m.movementType === "Out" || m.movementType === "Issue" ? "text-red-600" : "text-green-700"}`}>
                    {m.movementType === "Out" || m.movementType === "Issue" ? "-" : "+"}{m.quantity} {m.unitName ?? ""}
                  </span>
                </td>
                <td className="px-4 py-3 text-sm text-gray-600">{m.warehouseName ?? "—"}</td>
                <td className="px-4 py-3 text-xs text-gray-500 font-mono">{m.reference ?? "—"}</td>
                <td className="px-4 py-3 text-xs text-gray-500">{new Date(m.createdAt).toLocaleDateString("ar")}</td>
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
