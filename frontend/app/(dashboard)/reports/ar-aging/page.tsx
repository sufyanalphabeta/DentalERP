"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface ARAgingPatient {
  patientId: string; patientName: string;
  current: number; days30: number; days60: number; days90: number; over90: number; total: number;
}
interface ARAgingReport {
  asOf: string; totalOutstanding: number; patients: ARAgingPatient[];
}

export default function ARAgingPage() {
  const [data, setData] = useState<ARAgingReport | null>(null);
  const [loading, setLoading] = useState(true);
  const [pdfLoading, setPdfLoading] = useState(false);
  const [search, setSearch] = useState("");

  useEffect(() => {
    api.get<ARAgingReport>("/analytics/ar-aging")
      .then((r) => setData(r.data))
      .finally(() => setLoading(false));
  }, []);

  async function downloadPdf() {
    setPdfLoading(true);
    try {
      const res = await api.get("/analytics/ar-aging/pdf", { responseType: "blob" });
      const url = URL.createObjectURL(new Blob([res.data], { type: "application/pdf" }));
      const a = document.createElement("a");
      a.href = url;
      a.download = "ar-aging.pdf";
      a.click();
      URL.revokeObjectURL(url);
    } finally {
      setPdfLoading(false);
    }
  }

  function downloadCsv() {
    if (!data) return;
    const rows = [
      ["المريض", "الحالي (0-30)", "31-60 يوم", "61-90 يوم", "91-120 يوم", "+120 يوم", "الإجمالي"],
      ...filtered.map((p) => [
        p.patientName,
        p.current.toFixed(2), p.days30.toFixed(2), p.days60.toFixed(2),
        p.days90.toFixed(2), p.over90.toFixed(2), p.total.toFixed(2),
      ]),
    ];
    const csv = rows.map((r) => r.join(",")).join("\n");
    const url = URL.createObjectURL(new Blob(["﻿" + csv], { type: "text/csv;charset=utf-8" }));
    const a = document.createElement("a");
    a.href = url;
    a.download = "ar-aging.csv";
    a.click();
    URL.revokeObjectURL(url);
  }

  const filtered = (data?.patients ?? []).filter((p) =>
    !search || p.patientName.includes(search)
  );

  const totals = filtered.reduce(
    (acc, p) => ({
      current: acc.current + p.current, days30: acc.days30 + p.days30,
      days60: acc.days60 + p.days60, days90: acc.days90 + p.days90,
      over90: acc.over90 + p.over90, total: acc.total + p.total,
    }),
    { current: 0, days30: 0, days60: 0, days90: 0, over90: 0, total: 0 }
  );

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">تقادم الذمم المدينة</h1>
          {!loading && data && (
            <p className="text-sm text-gray-500 mt-0.5">
              بتاريخ {new Date(data.asOf).toLocaleDateString("ar-LY")} — إجمالي: {data.totalOutstanding.toFixed(2)} د.ل
            </p>
          )}
        </div>
        <div className="flex gap-2">
          <button onClick={downloadPdf} disabled={pdfLoading || loading} className="bg-red-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-red-700 disabled:opacity-50">
            {pdfLoading ? "جاري..." : "📄 PDF"}
          </button>
          <button onClick={downloadCsv} disabled={loading || !data} className="bg-green-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-green-700 disabled:opacity-50">
            📊 CSV
          </button>
        </div>
      </div>

      <div className="mb-4">
        <input
          placeholder="بحث بالاسم..."
          className="border rounded-lg px-3 py-2 text-sm w-64"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {loading ? (
        <div className="text-center text-gray-400 py-12">جاري التحميل...</div>
      ) : !data ? (
        <div className="text-center text-red-500 py-12">تعذر تحميل البيانات</div>
      ) : (
        <div className="bg-white rounded-xl shadow overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200 text-sm">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-right text-xs text-gray-500">المريض</th>
                <th className="px-4 py-3 text-right text-xs text-gray-500">الحالي (0-30)</th>
                <th className="px-4 py-3 text-right text-xs text-gray-500">31-60 يوم</th>
                <th className="px-4 py-3 text-right text-xs text-gray-500">61-90 يوم</th>
                <th className="px-4 py-3 text-right text-xs text-gray-500">91-120 يوم</th>
                <th className="px-4 py-3 text-right text-xs text-gray-500">+120 يوم</th>
                <th className="px-4 py-3 text-right text-xs text-gray-500 font-bold">الإجمالي</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {filtered.map((p) => (
                <tr key={p.patientId} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-medium text-gray-900">{p.patientName}</td>
                  <td className="px-4 py-3 text-gray-600">{p.current > 0 ? p.current.toFixed(2) : "—"}</td>
                  <td className="px-4 py-3 text-amber-600">{p.days30 > 0 ? p.days30.toFixed(2) : "—"}</td>
                  <td className="px-4 py-3 text-orange-600">{p.days60 > 0 ? p.days60.toFixed(2) : "—"}</td>
                  <td className="px-4 py-3 text-red-500">{p.days90 > 0 ? p.days90.toFixed(2) : "—"}</td>
                  <td className="px-4 py-3 text-red-700 font-medium">{p.over90 > 0 ? p.over90.toFixed(2) : "—"}</td>
                  <td className="px-4 py-3 font-bold text-gray-900">{p.total.toFixed(2)}</td>
                </tr>
              ))}
              {filtered.length > 0 && (
                <tr className="bg-gray-100 font-bold">
                  <td className="px-4 py-3 text-gray-900">الإجمالي ({filtered.length} مريض)</td>
                  <td className="px-4 py-3 text-gray-900">{totals.current.toFixed(2)}</td>
                  <td className="px-4 py-3 text-gray-900">{totals.days30.toFixed(2)}</td>
                  <td className="px-4 py-3 text-gray-900">{totals.days60.toFixed(2)}</td>
                  <td className="px-4 py-3 text-gray-900">{totals.days90.toFixed(2)}</td>
                  <td className="px-4 py-3 text-gray-900">{totals.over90.toFixed(2)}</td>
                  <td className="px-4 py-3 text-gray-900">{totals.total.toFixed(2)}</td>
                </tr>
              )}
              {filtered.length === 0 && (
                <tr>
                  <td colSpan={7} className="px-4 py-8 text-center text-gray-400">لا توجد ذمم مستحقة</td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
