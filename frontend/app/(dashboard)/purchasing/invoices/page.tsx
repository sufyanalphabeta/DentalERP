"use client";
import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface PIListItem {
  id: string;
  invoiceNumber: string;
  invoiceDate: string;
  supplierId: string;
  supplierName: string;
  status: string;
  netTotal: number;
  itemCount: number;
  createdAt: string;
  postedAt: string | null;
}

interface PIResult {
  invoices: PIListItem[];
  total: number;
}

const STATUS_AR: Record<string, string> = {
  Draft: "مسودة",
  Posted: "مرحّلة",
  Cancelled: "ملغاة",
};
const STATUS_CLS: Record<string, string> = {
  Draft: "bg-amber-100 text-amber-700",
  Posted: "bg-green-100 text-green-700",
  Cancelled: "bg-red-100 text-red-600",
};

export default function PurchaseInvoicesPage() {
  const router = useRouter();
  const [data, setData] = useState<PIResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [status, setStatus] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 20;

  useEffect(() => {
    load();
  }, [search, status, page]);

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
      if (search) params.set("search", search);
      if (status) params.set("status", status);
      const r = await api.get<PIResult>(`/purchasing/invoices?${params}`);
      setData(r.data);
    } finally {
      setLoading(false);
    }
  }

  const invoices = data?.invoices ?? [];
  const totalPages = Math.max(1, Math.ceil((data?.total ?? 0) / pageSize));

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">فواتير المشتريات</h1>
        <button
          onClick={() => router.push("/purchasing/invoices/new")}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
        >
          + فاتورة جديدة
        </button>
      </div>

      <div className="flex flex-wrap gap-3 mb-4">
        <input
          type="text"
          placeholder="بحث برقم الفاتورة..."
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
          className="border rounded-lg px-3 py-2 text-sm w-52 focus:outline-none focus:ring-2 focus:ring-blue-400"
        />
        <select
          value={status}
          onChange={(e) => {
            setStatus(e.target.value);
            setPage(1);
          }}
          className="border rounded-lg px-3 py-2 text-sm"
        >
          <option value="">كل الحالات</option>
          {Object.entries(STATUS_AR).map(([k, v]) => (
            <option key={k} value={k}>
              {v}
            </option>
          ))}
        </select>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">رقم الفاتورة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">التاريخ</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المورد</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الأصناف</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الإجمالي</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الحالة</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr>
                <td colSpan={7} className="text-center py-8 text-gray-400">
                  جاري التحميل...
                </td>
              </tr>
            ) : invoices.length === 0 ? (
              <tr>
                <td colSpan={7} className="text-center py-8 text-gray-400">
                  لا توجد فواتير
                </td>
              </tr>
            ) : (
              invoices.map((inv) => (
                <tr
                  key={inv.id}
                  className="hover:bg-gray-50 cursor-pointer"
                  onClick={() => router.push(`/purchasing/invoices/${inv.id}`)}
                >
                  <td className="px-4 py-3 text-sm font-mono font-semibold text-blue-700">
                    {inv.invoiceNumber}
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-700">{inv.invoiceDate}</td>
                  <td className="px-4 py-3 text-sm text-gray-800">{inv.supplierName}</td>
                  <td className="px-4 py-3 text-xs text-gray-500">{inv.itemCount} صنف</td>
                  <td className="px-4 py-3 text-sm font-semibold text-gray-800">
                    {inv.netTotal.toFixed(2)} د.ل
                  </td>
                  <td className="px-4 py-3">
                    <span
                      className={`text-xs px-2 py-0.5 rounded-full font-medium ${
                        STATUS_CLS[inv.status] ?? "bg-gray-100 text-gray-600"
                      }`}
                    >
                      {STATUS_AR[inv.status] ?? inv.status}
                    </span>
                  </td>
                  <td
                    className="px-4 py-3 text-xs text-blue-600 hover:underline"
                    onClick={(e) => e.stopPropagation()}
                  >
                    <Link href={`/purchasing/invoices/${inv.id}`}>فتح</Link>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
        {totalPages > 1 && (
          <div className="px-4 py-3 border-t flex justify-center gap-2">
            <button
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              disabled={page === 1}
              className="px-3 py-1 rounded border text-sm disabled:opacity-40"
            >
              السابق
            </button>
            <span className="px-3 py-1 text-sm text-gray-600">
              {page} / {totalPages}
            </span>
            <button
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
              disabled={page === totalPages}
              className="px-3 py-1 rounded border text-sm disabled:opacity-40"
            >
              التالي
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
