"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface Supplier { id: string; name: string; }
interface PurchaseInvoice {
  id: string; invoiceNumber: string; supplierName: string | null; supplierId: string;
  status: string; invoiceDate: string; totalAmount: number; notes: string | null;
}
interface InvoicesResponse { items: PurchaseInvoice[]; totalCount: number; page: number; pageSize: number; }

const STATUS_LABEL: Record<string, string> = {
  Draft: "مسودة", Posted: "مرحّلة", Cancelled: "ملغاة",
};
const STATUS_CLS: Record<string, string> = {
  Draft: "bg-yellow-100 text-yellow-800",
  Posted: "bg-green-100 text-green-700",
  Cancelled: "bg-red-100 text-red-700",
};

export default function PurchasingReportPage() {
  const [data, setData] = useState<InvoicesResponse | null>(null);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [loading, setLoading] = useState(true);
  const [supplierFilter, setSupplierFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [page, setPage] = useState(1);
  const [pdfLoading, setPdfLoading] = useState(false);

  useEffect(() => {
    api.get<{ suppliers: Supplier[] }>("/suppliers?pageSize=500").then((r) => setSuppliers(r.data.suppliers ?? [])).catch(() => {});
  }, []);

  useEffect(() => { load(); }, [supplierFilter, statusFilter, from, to, page]);

  async function downloadPdf() {
    setPdfLoading(true);
    try {
      const params = new URLSearchParams();
      if (supplierFilter) params.set("supplierId", supplierFilter);
      if (statusFilter) params.set("status", statusFilter);
      if (from) params.set("from", from);
      if (to) params.set("to", to);
      const res = await api.get(`/purchasing/invoices/report/pdf?${params}`, { responseType: "blob" });
      const url = URL.createObjectURL(new Blob([res.data], { type: "application/pdf" }));
      const a = document.createElement("a");
      a.href = url;
      a.download = "purchasing-report.pdf";
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
      if (supplierFilter) params.set("supplierId", supplierFilter);
      if (statusFilter) params.set("status", statusFilter);
      if (from) params.set("from", from);
      if (to) params.set("to", to);
      const r = await api.get<InvoicesResponse>(`/purchasing/invoices?${params}`);
      setData(r.data);
    } finally {
      setLoading(false);
    }
  }

  const totalPages = data ? Math.ceil(data.totalCount / 50) : 1;
  const totalAmount = (data?.items ?? []).reduce((s, i) => s + i.totalAmount, 0);

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">تقرير فواتير المشتريات</h1>
          {!loading && data && (
            <p className="text-sm text-gray-500 mt-0.5">إجمالي: {totalAmount.toFixed(2)} د.ل ({data.totalCount} فاتورة)</p>
          )}
        </div>
        <button onClick={downloadPdf} disabled={pdfLoading} className="bg-gray-700 text-white px-4 py-2 rounded-lg text-sm hover:bg-gray-800 disabled:opacity-50">
          {pdfLoading ? "جاري التحميل..." : "📄 تحميل PDF"}
        </button>
      </div>

      <div className="flex flex-wrap gap-3 mb-4 ">
        <select value={supplierFilter} onChange={(e) => { setSupplierFilter(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm">
          <option value="">كل الموردين</option>
          {suppliers.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
        </select>
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm">
          <option value="">كل الحالات</option>
          <option value="Draft">مسودة</option>
          <option value="Posted">مرحّلة</option>
          <option value="Cancelled">ملغاة</option>
        </select>
        <input type="date" value={from} onChange={(e) => { setFrom(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm" />
        <input type="date" value={to} onChange={(e) => { setTo(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm" />
        <button onClick={() => { setSupplierFilter(""); setStatusFilter(""); setFrom(""); setTo(""); setPage(1); }} className="text-sm text-gray-500 border px-3 py-2 rounded-lg hover:bg-gray-50">
          إعادة تعيين
        </button>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200 text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">رقم الفاتورة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المورد</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الحالة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">التاريخ</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الإجمالي</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">ملاحظات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">جاري التحميل...</td></tr>
            ) : (data?.items ?? []).length === 0 ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">لا توجد فواتير</td></tr>
            ) : (data?.items ?? []).map((inv) => (
              <tr key={inv.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 font-mono text-blue-700">{inv.invoiceNumber}</td>
                <td className="px-4 py-3 text-gray-800">{inv.supplierName ?? "—"}</td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${STATUS_CLS[inv.status] ?? "bg-gray-100 text-gray-600"}`}>
                    {STATUS_LABEL[inv.status] ?? inv.status}
                  </span>
                </td>
                <td className="px-4 py-3 text-xs text-gray-500">{new Date(inv.invoiceDate).toLocaleDateString("ar")}</td>
                <td className="px-4 py-3 font-medium text-gray-800">{inv.totalAmount.toFixed(2)} د.ل</td>
                <td className="px-4 py-3 text-xs text-gray-400">{inv.notes ?? "—"}</td>
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
