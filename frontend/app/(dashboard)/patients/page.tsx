"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import api from "@/lib/api";
import { PermissionGate } from "@/components/shared/PermissionGate";
import type { GetPatientsResponse, PatientSummary } from "@/types/patients";

export default function PatientsPage() {
  const [data, setData] = useState<GetPatientsResponse | null>(null);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const timeout = setTimeout(() => fetchPatients(), 300);
    return () => clearTimeout(timeout);
  }, [search, page]);

  async function fetchPatients() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: "20" });
      if (search) params.set("search", search);
      const res = await api.get<GetPatientsResponse>(`/api/patients?${params}`);
      setData(res.data);
    } finally {
      setLoading(false);
    }
  }

  const genderLabel = (g?: string) =>
    g === "Male" ? "ذكر" : g === "Female" ? "أنثى" : "—";

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">المرضى</h1>
        <PermissionGate permission="Patients.Create">
          <Link
            href="/patients/new"
            className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 transition"
          >
            + مريض جديد
          </Link>
        </PermissionGate>
      </div>

      <div className="mb-4">
        <input
          type="text"
          placeholder="بحث بالاسم، رقم الملف، الهاتف، أو رقم الهوية..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          className="w-full max-w-md border border-gray-300 rounded-lg px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-gray-600">
            <tr>
              <th className="px-4 py-3 text-start">رقم الملف</th>
              <th className="px-4 py-3 text-start">الاسم</th>
              <th className="px-4 py-3 text-start">الهاتف</th>
              <th className="px-4 py-3 text-start">الجنس</th>
              <th className="px-4 py-3 text-start">العمر</th>
              <th className="px-4 py-3 text-start">الحالة</th>
              <th className="px-4 py-3 text-start">الإجراءات</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={7} className="text-center py-8 text-gray-500">
                  جاري التحميل...
                </td>
              </tr>
            ) : data?.items.length === 0 ? (
              <tr>
                <td colSpan={7} className="text-center py-8 text-gray-400">
                  لا توجد نتائج
                </td>
              </tr>
            ) : (
              data?.items.map((p: PatientSummary) => (
                <tr key={p.id} className="border-t border-gray-100 hover:bg-gray-50">
                  <td className="px-4 py-3 font-mono text-blue-600">{p.fileNumber}</td>
                  <td className="px-4 py-3 font-medium">{p.fullName}</td>
                  <td className="px-4 py-3 text-gray-600">{p.phone}</td>
                  <td className="px-4 py-3 text-gray-600">{genderLabel(p.gender)}</td>
                  <td className="px-4 py-3 text-gray-600">{p.age ?? "—"}</td>
                  <td className="px-4 py-3">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                      p.isActive ? "bg-green-100 text-green-700" : "bg-red-100 text-red-700"
                    }`}>
                      {p.isActive ? "نشط" : "غير نشط"}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <Link href={`/patients/${p.id}`} className="text-blue-600 hover:underline me-3">
                      عرض
                    </Link>
                    <PermissionGate permission="Patients.Edit">
                      <Link href={`/patients/${p.id}/edit`} className="text-gray-600 hover:underline">
                        تعديل
                      </Link>
                    </PermissionGate>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>

        {data && data.totalPages > 1 && (
          <div className="flex items-center justify-between px-4 py-3 border-t border-gray-100">
            <span className="text-sm text-gray-500">
              {data.totalCount} مريض | صفحة {data.page} من {data.totalPages}
            </span>
            <div className="flex gap-2">
              <button
                disabled={page <= 1}
                onClick={() => setPage(p => p - 1)}
                className="px-3 py-1 text-sm border rounded disabled:opacity-40"
              >
                السابق
              </button>
              <button
                disabled={page >= data.totalPages}
                onClick={() => setPage(p => p + 1)}
                className="px-3 py-1 text-sm border rounded disabled:opacity-40"
              >
                التالي
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
