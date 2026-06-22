"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface Movement {
  id: string;
  vaultName: string;
  direction: string;
  amount: number;
  transactionType: string;
  referenceNumber: string | null;
  notes: string | null;
  createdAt: string;
}

interface MovementsResponse {
  items: Movement[];
  totalCount: number;
  page: number;
  pageSize: number;
}

interface Vault {
  id: string;
  name: string;
  currentBalance: number;
}

const dirAr: Record<string, string> = {
  In: "وارد",
  Out: "صادر",
};

const dirCls: Record<string, string> = {
  In: "bg-green-100 text-green-700",
  Out: "bg-red-100 text-red-700",
};

const txTypeAr: Record<string, string> = {
  receipt_from_patient: "تحصيل من مريض",
  payment_to_doctor: "صرف للطبيب",
  general_receipt: "إيراد عام",
  general_payment: "مصروف عام",
  inter_vault_transfer: "تحويل بين خزائن",
};

function formatDate(iso: string | null | undefined) {
  if (!iso) return "—";
  const d = new Date(iso);
  if (isNaN(d.getTime())) return "—";
  return d.toLocaleDateString("ar-LY", { year: "numeric", month: "short", day: "numeric" });
}

export default function TreasuryMovementsPage() {
  const [data, setData] = useState<MovementsResponse | null>(null);
  const [vaults, setVaults] = useState<Vault[]>([]);
  const [loading, setLoading] = useState(true);
  const [vaultFilter, setVaultFilter] = useState("");
  const [dirFilter, setDirFilter] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [page, setPage] = useState(1);

  useEffect(() => {
    api.get<Vault[]>("/treasury/vaults/balances").then((r) => setVaults(r.data)).catch(() => {});
  }, []);

  useEffect(() => { load(); }, [vaultFilter, dirFilter, from, to, page]);

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: "30" });
      if (vaultFilter) params.set("vaultId", vaultFilter);
      if (dirFilter) params.set("direction", dirFilter);
      if (from) params.set("dateFrom", new Date(from).toISOString());
      if (to) params.set("dateTo", new Date(to).toISOString());
      const r = await api.get<MovementsResponse>(`/treasury/movements?${params}`);
      setData(r.data);
    } catch {
      setData({ items: [], totalCount: 0, page: 1, pageSize: 30 });
    } finally {
      setLoading(false);
    }
  }

  const totalPages = data ? Math.ceil(data.totalCount / 30) : 1;

  const totals = (data?.items ?? []).reduce(
    (acc, m) => {
      if (m.direction === "In") acc.in += m.amount;
      else acc.out += m.amount;
      return acc;
    },
    { in: 0, out: 0 }
  );

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">حركة النقدية</h1>
      </div>

      {/* Summary cards */}
      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="bg-green-50 border border-green-200 rounded-xl p-4">
          <div className="text-xs text-green-600 mb-1">إجمالي الوارد</div>
          <div className="text-xl font-bold text-green-700">+{totals.in.toFixed(2)} د.ل</div>
        </div>
        <div className="bg-red-50 border border-red-200 rounded-xl p-4">
          <div className="text-xs text-red-600 mb-1">إجمالي الصادر</div>
          <div className="text-xl font-bold text-red-700">-{totals.out.toFixed(2)} د.ل</div>
        </div>
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-4">
          <div className="text-xs text-blue-600 mb-1">الصافي</div>
          <div className={`text-xl font-bold ${totals.in - totals.out >= 0 ? "text-blue-700" : "text-red-700"}`}>
            {(totals.in - totals.out).toFixed(2)} د.ل
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-3 mb-4">
        <select value={vaultFilter} onChange={(e) => { setVaultFilter(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm">
          <option value="">كل الخزائن</option>
          {vaults.map((v) => <option key={v.id} value={v.id}>{v.name}</option>)}
        </select>
        <select value={dirFilter} onChange={(e) => { setDirFilter(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm">
          <option value="">الوارد والصادر</option>
          <option value="In">وارد</option>
          <option value="Out">صادر</option>
        </select>
        <input type="date" value={from} onChange={(e) => { setFrom(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm" />
        <input type="date" value={to} onChange={(e) => { setTo(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm" />
        <button onClick={() => { setVaultFilter(""); setDirFilter(""); setFrom(""); setTo(""); setPage(1); }} className="text-sm text-gray-500 border px-3 py-2 rounded-lg hover:bg-gray-50">إعادة تعيين</button>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الخزينة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الاتجاه</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المبلغ</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">نوع العملية</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">البيان</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">التاريخ</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">جاري التحميل...</td></tr>
            ) : (data?.items ?? []).length === 0 ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">لا توجد حركات في هذه الفترة</td></tr>
            ) : (data?.items ?? []).map((m) => (
              <tr key={m.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 text-sm font-medium text-gray-800">{m.vaultName}</td>
                <td className="px-4 py-3">
                  <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${dirCls[m.direction] ?? "bg-gray-100 text-gray-600"}`}>
                    {dirAr[m.direction] ?? m.direction}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <span className={`text-sm font-bold ${m.direction === "In" ? "text-green-700" : "text-red-600"}`}>
                    {m.direction === "In" ? "+" : "-"}{m.amount.toFixed(2)} د.ل
                  </span>
                </td>
                <td className="px-4 py-3 text-xs text-gray-500">{txTypeAr[m.transactionType] ?? m.transactionType}</td>
                <td className="px-4 py-3 text-sm text-gray-600 max-w-xs truncate">{m.notes ?? m.referenceNumber ?? "—"}</td>
                <td className="px-4 py-3 text-xs text-gray-500">{formatDate(m.createdAt)}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {totalPages > 1 && (
          <div className="px-4 py-3 border-t flex items-center justify-between">
            <span className="text-sm text-gray-500">{data?.totalCount} حركة</span>
            <div className="flex gap-2">
              <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-3 py-1 rounded border text-sm disabled:opacity-40">السابق</button>
              <span className="px-3 py-1 text-sm text-gray-600">{page} / {totalPages}</span>
              <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="px-3 py-1 rounded border text-sm disabled:opacity-40">التالي</button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
