"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface ReturnListItem {
  id: string;
  returnNumber: string;
  supplierName: string;
  status: string;
  returnDate: string;
  totalAmount: number;
  reason: string;
  createdAt: string;
}

interface ReturnsResult {
  returns: ReturnListItem[];
  total: number;
}

interface Supplier {
  id: string;
  name: string;
}

const STATUS_AR: Record<string, string> = {
  Draft: "مسودة",
  Confirmed: "مؤكد",
  Completed: "مكتمل",
  Cancelled: "ملغى",
};
const STATUS_CLS: Record<string, string> = {
  Draft: "bg-amber-100 text-amber-700",
  Confirmed: "bg-green-100 text-green-700",
  Completed: "bg-blue-100 text-blue-700",
  Cancelled: "bg-red-100 text-red-600",
};

export default function PurchaseReturnsPage() {
  const router = useRouter();
  const [data, setData] = useState<ReturnsResult | null>(null);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [loading, setLoading] = useState(true);
  const [supplierFilter, setSupplierFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 20;

  useEffect(() => {
    api
      .get<{ suppliers: Supplier[] }>("/suppliers?pageSize=500&activeOnly=true")
      .then((r) => setSuppliers(r.data.suppliers ?? []))
      .catch(() => {});
  }, []);

  useEffect(() => {
    load();
  }, [supplierFilter, statusFilter, page]);

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({
        page: String(page),
        pageSize: String(pageSize),
      });
      if (supplierFilter) params.set("supplierId", supplierFilter);
      if (statusFilter) params.set("status", statusFilter);
      const r = await api.get<ReturnsResult>(
        `/purchasing/purchase-returns?${params}`
      );
      setData(r.data);
    } finally {
      setLoading(false);
    }
  }

  const returns = data?.returns ?? [];
  const totalPages = Math.max(1, Math.ceil((data?.total ?? 0) / pageSize));

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">مردودات المشتريات</h1>
        <button
          onClick={() => router.push("/purchasing/returns/new")}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
        >
          + مردود جديد
        </button>
      </div>

      <div className="flex flex-wrap gap-3 mb-4">
        <select
          value={supplierFilter}
          onChange={(e) => {
            setSupplierFilter(e.target.value);
            setPage(1);
          }}
          className="border rounded-lg px-3 py-2 text-sm"
        >
          <option value="">كل الموردين</option>
          {suppliers.map((s) => (
            <option key={s.id} value={s.id}>
              {s.name}
            </option>
          ))}
        </select>
        <select
          value={statusFilter}
          onChange={(e) => {
            setStatusFilter(e.target.value);
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
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">
                رقم المردود
              </th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">
                التاريخ
              </th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">
                المورد
              </th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">
                السبب
              </th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">
                الإجمالي
              </th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">
                الحالة
              </th>
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
            ) : returns.length === 0 ? (
              <tr>
                <td colSpan={7} className="text-center py-8 text-gray-400">
                  لا توجد مردودات
                </td>
              </tr>
            ) : (
              returns.map((r) => (
                <tr
                  key={r.id}
                  className="hover:bg-gray-50 cursor-pointer"
                  onClick={() => router.push(`/purchasing/returns/${r.id}`)}
                >
                  <td className="px-4 py-3 text-sm font-mono font-semibold text-blue-700">
                    {r.returnNumber}
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-700">
                    {r.returnDate}
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-800">
                    {r.supplierName}
                  </td>
                  <td className="px-4 py-3 text-xs text-gray-500 max-w-[160px] truncate">
                    {r.reason}
                  </td>
                  <td className="px-4 py-3 text-sm font-semibold text-gray-800">
                    {r.totalAmount.toFixed(2)} د.ل
                  </td>
                  <td className="px-4 py-3">
                    <span
                      className={`text-xs px-2 py-0.5 rounded-full font-medium ${STATUS_CLS[r.status] ?? "bg-gray-100 text-gray-600"}`}
                    >
                      {STATUS_AR[r.status] ?? r.status}
                    </span>
                  </td>
                  <td
                    className="px-4 py-3 text-xs text-blue-600 hover:underline"
                    onClick={(e) => {
                      e.stopPropagation();
                      router.push(`/purchasing/returns/${r.id}`);
                    }}
                  >
                    فتح
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
