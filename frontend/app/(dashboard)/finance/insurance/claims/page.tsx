"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface InsuranceClaimSummary {
  id: string;
  claimNumber: string;
  status: string;
  insuranceCompanyName: string;
  patientId: string;
  claimedAmount: number;
  paidAmount: number;
  coveragePercent: number;
  claimDate: string;
}

const STATUS_LABELS: Record<string, string> = {
  Draft: "مسودة",
  Submitted: "مُقدَّم",
  PartiallyPaid: "مدفوع جزئياً",
  FullyPaid: "مدفوع بالكامل",
  Rejected: "مرفوض",
};

const STATUS_COLORS: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-700",
  Submitted: "bg-blue-100 text-blue-700",
  PartiallyPaid: "bg-yellow-100 text-yellow-700",
  FullyPaid: "bg-green-100 text-green-700",
  Rejected: "bg-red-100 text-red-700",
};

const STATUSES = ["Draft", "Submitted", "PartiallyPaid", "FullyPaid", "Rejected"];

export default function InsuranceClaimsPage() {
  const router = useRouter();
  const [claims, setClaims] = useState<InsuranceClaimSummary[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    const params = new URLSearchParams({ page: String(page), pageSize: "20" });
    if (statusFilter) params.set("status", statusFilter);
    api.get(`/insurance/claims?${params}`).then((r) => {
      setClaims(r.data.items ?? []);
      setTotal(r.data.totalCount ?? 0);
      setLoading(false);
    });
  }, [page, statusFilter]);

  return (
    <div className="p-6 max-w-6xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">مطالبات التأمين</h1>
      </div>

      <div className="flex gap-2 mb-5 flex-wrap">
        <button
          onClick={() => { setStatusFilter(""); setPage(1); }}
          className={`px-3 py-1.5 rounded-lg text-sm border ${statusFilter === "" ? "bg-blue-600 text-white border-blue-600" : "bg-white text-gray-700"}`}
        >
          الكل ({total})
        </button>
        {STATUSES.map((s) => (
          <button
            key={s}
            onClick={() => { setStatusFilter(s); setPage(1); }}
            className={`px-3 py-1.5 rounded-lg text-sm border ${statusFilter === s ? "bg-blue-600 text-white border-blue-600" : "bg-white text-gray-700"}`}
          >
            {STATUS_LABELS[s]}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="text-center py-12 text-gray-500">جاري التحميل...</div>
      ) : claims.length === 0 ? (
        <div className="text-center py-12 text-gray-400">لا توجد مطالبات</div>
      ) : (
        <div className="bg-white border rounded-xl overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50">
              <tr>
                <th className="text-right p-4 text-gray-600">رقم المطالبة</th>
                <th className="text-right p-4 text-gray-600">شركة التأمين</th>
                <th className="text-right p-4 text-gray-600">المبلغ المطالب</th>
                <th className="text-right p-4 text-gray-600">المدفوع</th>
                <th className="text-right p-4 text-gray-600">التغطية</th>
                <th className="text-right p-4 text-gray-600">الحالة</th>
                <th className="text-right p-4 text-gray-600">التاريخ</th>
              </tr>
            </thead>
            <tbody>
              {claims.map((claim) => (
                <tr
                  key={claim.id}
                  onClick={() => router.push(`/finance/insurance/claims/${claim.id}`)}
                  className="border-t hover:bg-gray-50 cursor-pointer"
                >
                  <td className="p-4 font-medium text-blue-700">{claim.claimNumber}</td>
                  <td className="p-4 text-gray-700">{claim.insuranceCompanyName}</td>
                  <td className="p-4 text-gray-800">{claim.claimedAmount.toFixed(2)} د.ل</td>
                  <td className="p-4 text-green-700 font-medium">{claim.paidAmount.toFixed(2)} د.ل</td>
                  <td className="p-4 text-gray-600">{claim.coveragePercent}%</td>
                  <td className="p-4">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${STATUS_COLORS[claim.status] ?? "bg-gray-100 text-gray-700"}`}>
                      {STATUS_LABELS[claim.status] ?? claim.status}
                    </span>
                  </td>
                  <td className="p-4 text-gray-500">
                    {new Date(claim.claimDate).toLocaleDateString("ar-LY")}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {total > 20 && (
            <div className="flex items-center justify-center gap-4 p-4 border-t text-sm">
              <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1} className="border px-3 py-1 rounded disabled:opacity-50">
                السابق
              </button>
              <span className="text-gray-500">صفحة {page}</span>
              <button onClick={() => setPage(p => p + 1)} disabled={claims.length < 20} className="border px-3 py-1 rounded disabled:opacity-50">
                التالي
              </button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
