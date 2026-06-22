"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface VaultCollection { vaultId: string; vaultName: string; amount: number; }
interface MethodCollection { method: string; methodAr: string; amount: number; }
interface DailyCollection { date: string; amount: number; transactionCount: number; }
interface CollectionSummary {
  from: string; to: string; totalCollected: number;
  byVault: VaultCollection[]; byMethod: MethodCollection[]; daily: DailyCollection[];
}

export default function CollectionSummaryPage() {
  const today = new Date().toISOString().slice(0, 10);
  const firstOfMonth = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10);

  const [data, setData] = useState<CollectionSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [from, setFrom] = useState(firstOfMonth);
  const [to, setTo] = useState(today);
  const [pdfLoading, setPdfLoading] = useState(false);

  useEffect(() => { load(); }, [from, to]);

  async function load() {
    setLoading(true);
    try {
      const res = await api.get<CollectionSummary>(`/analytics/collection-summary?from=${from}&to=${to}`);
      setData(res.data);
    } finally {
      setLoading(false);
    }
  }

  async function downloadPdf() {
    setPdfLoading(true);
    try {
      const res = await api.get(`/analytics/collection-summary/pdf?from=${from}&to=${to}`, { responseType: "blob" });
      const url = URL.createObjectURL(new Blob([res.data], { type: "application/pdf" }));
      const a = document.createElement("a");
      a.href = url;
      a.download = `collection-summary-${from}-${to}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    } finally {
      setPdfLoading(false);
    }
  }

  function downloadCsv() {
    if (!data) return;
    const rows = [
      ["التاريخ", "عدد المعاملات", "المبلغ"],
      ...data.daily.map((d) => [d.date, String(d.transactionCount), d.amount.toFixed(2)]),
      ["الإجمالي", "", data.totalCollected.toFixed(2)],
    ];
    const csv = rows.map((r) => r.join(",")).join("\n");
    const url = URL.createObjectURL(new Blob(["﻿" + csv], { type: "text/csv;charset=utf-8" }));
    const a = document.createElement("a");
    a.href = url;
    a.download = `collection-${from}-${to}.csv`;
    a.click();
    URL.revokeObjectURL(url);
  }

  return (
    <div className="p-6 max-w-5xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">تقرير التحصيلات</h1>
          {!loading && data && (
            <p className="text-sm text-gray-500 mt-0.5">إجمالي: {data.totalCollected.toFixed(2)} د.ل</p>
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

      <div className="flex gap-3 mb-6 items-center flex-wrap">
        <div className="flex items-center gap-2">
          <label className="text-sm text-gray-600">من</label>
          <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} className="border rounded-lg px-3 py-2 text-sm" />
        </div>
        <div className="flex items-center gap-2">
          <label className="text-sm text-gray-600">إلى</label>
          <input type="date" value={to} onChange={(e) => setTo(e.target.value)} className="border rounded-lg px-3 py-2 text-sm" />
        </div>
        <button onClick={() => { setFrom(today); setTo(today); }} className="text-sm text-blue-600 border border-blue-200 px-3 py-2 rounded-lg hover:bg-blue-50">
          اليوم
        </button>
        <button onClick={() => { setFrom(firstOfMonth); setTo(today); }} className="text-sm text-blue-600 border border-blue-200 px-3 py-2 rounded-lg hover:bg-blue-50">
          هذا الشهر
        </button>
      </div>

      {loading ? (
        <div className="text-center text-gray-400 py-12">جاري التحميل...</div>
      ) : !data ? (
        <div className="text-center text-red-500 py-12">تعذر تحميل البيانات</div>
      ) : (
        <div className="space-y-6">
          {/* Total Card */}
          <div className="bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl p-6">
            <div className="text-sm opacity-80">إجمالي التحصيلات</div>
            <div className="text-4xl font-bold mt-1">{data.totalCollected.toFixed(2)} <span className="text-2xl">د.ل</span></div>
            <div className="text-sm opacity-70 mt-1">من {data.from} إلى {data.to}</div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {/* By Vault */}
            <div className="bg-white rounded-xl shadow p-5">
              <h2 className="font-semibold text-gray-700 mb-4">حسب الخزينة</h2>
              {data.byVault.length === 0 ? (
                <div className="text-gray-400 text-sm">لا توجد بيانات</div>
              ) : (
                <div className="space-y-3">
                  {data.byVault.map((v) => (
                    <div key={v.vaultId} className="flex justify-between items-center">
                      <span className="text-sm text-gray-700">{v.vaultName}</span>
                      <span className="font-semibold text-gray-900">{v.amount.toFixed(2)} د.ل</span>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* By Method */}
            <div className="bg-white rounded-xl shadow p-5">
              <h2 className="font-semibold text-gray-700 mb-4">حسب طريقة الدفع</h2>
              {data.byMethod.length === 0 ? (
                <div className="text-gray-400 text-sm">لا توجد بيانات</div>
              ) : (
                <div className="space-y-3">
                  {data.byMethod.map((m) => (
                    <div key={m.method} className="flex justify-between items-center">
                      <span className="text-sm text-gray-700">{m.methodAr}</span>
                      <span className="font-semibold text-gray-900">{m.amount.toFixed(2)} د.ل</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Daily Breakdown */}
          {data.daily.length > 0 && (
            <div className="bg-white rounded-xl shadow overflow-hidden">
              <div className="px-5 py-4 border-b">
                <h2 className="font-semibold text-gray-700">التفصيل اليومي</h2>
              </div>
              <table className="min-w-full divide-y divide-gray-200 text-sm">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-3 text-right text-xs text-gray-500">التاريخ</th>
                    <th className="px-4 py-3 text-center text-xs text-gray-500">عدد المعاملات</th>
                    <th className="px-4 py-3 text-right text-xs text-gray-500">المبلغ</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-200">
                  {data.daily.map((d) => (
                    <tr key={d.date} className="hover:bg-gray-50">
                      <td className="px-4 py-3 text-gray-800">{new Date(d.date + "T00:00:00").toLocaleDateString("ar-LY")}</td>
                      <td className="px-4 py-3 text-center text-gray-600">{d.transactionCount}</td>
                      <td className="px-4 py-3 font-medium text-gray-900">{d.amount.toFixed(2)} د.ل</td>
                    </tr>
                  ))}
                  <tr className="bg-gray-50 font-bold">
                    <td className="px-4 py-3 text-gray-900">الإجمالي</td>
                    <td className="px-4 py-3 text-center text-gray-900">{data.daily.reduce((s, d) => s + d.transactionCount, 0)}</td>
                    <td className="px-4 py-3 text-gray-900">{data.totalCollected.toFixed(2)} د.ل</td>
                  </tr>
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
