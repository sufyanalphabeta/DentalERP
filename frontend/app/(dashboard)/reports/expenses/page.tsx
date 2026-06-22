"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface ExpenseCategory { id: string; name: string; }
interface Vault { id: string; name: string; currentBalance: number; }
interface Expense {
  id: string; description: string; amount: number;
  categoryName: string | null; costCenter: string | null;
  expenseDate: string; notes: string | null; vaultId: string | null;
}
interface ExpensesResponse { items: Expense[]; totalCount: number; page: number; pageSize: number; }

export default function ExpensesReportPage() {
  const [data, setData] = useState<ExpensesResponse | null>(null);
  const [categories, setCategories] = useState<ExpenseCategory[]>([]);
  const [vaults, setVaults] = useState<Vault[]>([]);
  const [loading, setLoading] = useState(true);
  const [catFilter, setCatFilter] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [page, setPage] = useState(1);
  const [pdfLoading, setPdfLoading] = useState(false);

  useEffect(() => {
    api.get<ExpenseCategory[]>("/expenses/categories").then((r) => setCategories(r.data)).catch(() => {});
    api.get<Vault[]>("/treasury/vaults/balances").then((r) => setVaults(r.data ?? [])).catch(() => {});
  }, []);

  useEffect(() => { load(); }, [catFilter, from, to, page]);

  async function downloadPdf() {
    setPdfLoading(true);
    try {
      const params = new URLSearchParams();
      params.set("dateFrom", from || "2000-01-01");
      params.set("dateTo", to || new Date().toISOString().slice(0, 10));
      if (catFilter) params.set("categoryId", catFilter);
      const res = await api.get(`/expenses/report/pdf?${params}`, { responseType: "blob" });
      const url = URL.createObjectURL(new Blob([res.data], { type: "application/pdf" }));
      const a = document.createElement("a");
      a.href = url;
      a.download = `expenses-report.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    } finally {
      setPdfLoading(false);
    }
  }

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: "50" });
      if (catFilter) params.set("categoryId", catFilter);
      if (from) params.set("dateFrom", from);
      if (to) params.set("dateTo", to);
      const r = await api.get<ExpensesResponse>(`/expenses?${params}`);
      setData(r.data);
    } finally {
      setLoading(false);
    }
  }

  const totalPages = data ? Math.ceil(data.totalCount / 50) : 1;
  const totalAmount = (data?.items ?? []).reduce((s, e) => s + e.amount, 0);

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">تقرير المصروفات</h1>
          {!loading && data && (
            <p className="text-sm text-gray-500 mt-0.5">إجمالي: {totalAmount.toFixed(2)} د.ل ({data.totalCount} مصروف)</p>
          )}
        </div>
        <button onClick={downloadPdf} disabled={pdfLoading} className="bg-gray-700 text-white px-4 py-2 rounded-lg text-sm hover:bg-gray-800 disabled:opacity-50">
          {pdfLoading ? "جاري التحميل..." : "📄 تحميل PDF"}
        </button>
      </div>

      <div className="flex flex-wrap gap-3 mb-4 ">
        <select value={catFilter} onChange={(e) => { setCatFilter(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm">
          <option value="">كل الفئات</option>
          {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        <input type="date" value={from} onChange={(e) => { setFrom(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm" />
        <input type="date" value={to} onChange={(e) => { setTo(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm" />
        <button onClick={() => { setCatFilter(""); setFrom(""); setTo(""); setPage(1); }} className="text-sm text-gray-500 border px-3 py-2 rounded-lg hover:bg-gray-50">
          إعادة تعيين
        </button>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200 text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الوصف</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الفئة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الخزينة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">مركز التكلفة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المبلغ</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">التاريخ</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">جاري التحميل...</td></tr>
            ) : (data?.items ?? []).length === 0 ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">لا توجد مصروفات</td></tr>
            ) : (data?.items ?? []).map((exp) => (
              <tr key={exp.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <div className="font-medium text-gray-800">{exp.description}</div>
                  {exp.notes && <div className="text-xs text-gray-400">{exp.notes}</div>}
                </td>
                <td className="px-4 py-3 text-gray-600">{exp.categoryName ?? "—"}</td>
                <td className="px-4 py-3 text-gray-600">{vaults.find((v) => v.id === exp.vaultId)?.name ?? "—"}</td>
                <td className="px-4 py-3 text-gray-600">{exp.costCenter ?? "—"}</td>
                <td className="px-4 py-3 font-medium text-gray-800">{exp.amount.toFixed(2)} د.ل</td>
                <td className="px-4 py-3 text-xs text-gray-500">{new Date(exp.expenseDate).toLocaleDateString("ar")}</td>
              </tr>
            ))}
          </tbody>
          {data && data.totalCount > 0 && (
            <tfoot className="bg-gray-50 border-t-2 border-gray-300">
              <tr>
                <td colSpan={4} className="px-4 py-3 text-sm font-bold text-gray-700">الإجمالي</td>
                <td className="px-4 py-3 text-sm font-bold text-gray-900">{totalAmount.toFixed(2)} د.ل</td>
                <td />
              </tr>
            </tfoot>
          )}
        </table>
        {totalPages > 1 && (
          <div className="px-4 py-3 border-t flex justify-center gap-2 ">
            <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-3 py-1 rounded border text-sm disabled:opacity-40">السابق</button>
            <span className="px-3 py-1 text-sm text-gray-600">{page} / {totalPages}</span>
            <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="px-3 py-1 rounded border text-sm disabled:opacity-40">التالي</button>
          </div>
        )}
      </div>
    </div>
  );
}
