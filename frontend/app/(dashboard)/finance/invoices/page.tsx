"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";

interface InvoiceSummary {
  id: string;
  invoiceNumber: string;
  patientName: string;
  doctorName: string;
  status: string;
  totalAmount: number;
  paidAmount: number;
  remaining: number;
  currency: string;
  createdAt: string;
}

interface InvoiceListResponse {
  items: InvoiceSummary[];
  total: number;
  page: number;
  pageSize: number;
}

const statusLabel: Record<string, { label: string; cls: string }> = {
  Draft: { label: "Ù…Ø³ÙˆØ¯Ø©", cls: "bg-gray-100 text-gray-600" },
  Confirmed: { label: "Ù…Ø¤ÙƒØ¯Ø©", cls: "bg-blue-100 text-blue-700" },
  PartiallyPaid: { label: "Ù…Ø¯ÙÙˆØ¹Ø© Ø¬Ø²Ø¦ÙŠØ§Ù‹", cls: "bg-yellow-100 text-yellow-700" },
  Paid: { label: "Ù…Ø¯ÙÙˆØ¹Ø©", cls: "bg-green-100 text-green-700" },
  Cancelled: { label: "Ù…Ù„ØºØ§Ø©", cls: "bg-red-100 text-red-600" },
};

export default function InvoicesPage() {
  const [data, setData] = useState<InvoiceListResponse | null>(null);
  const [status, setStatus] = useState("");
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);

  useEffect(() => { load(); }, [status, page]);

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: "20" });
      if (status) params.set("status", status);
      const res = await api.get<InvoiceListResponse>(`/api/invoices?${params}`);
      setData(res.data);
    } finally {
      setLoading(false);
    }
  }

  const totalPages = data ? Math.ceil(data.total / 20) : 1;

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Ø§Ù„ÙÙˆØ§ØªÙŠØ±</h1>
        <Link
          href="/finance/invoices/new"
          className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 text-sm font-medium"
        >
          + ÙØ§ØªÙˆØ±Ø© Ø¬Ø¯ÙŠØ¯Ø©
        </Link>
      </div>

      <div className="flex gap-2 mb-4 flex-wrap">
        {["", "Draft", "Confirmed", "PartiallyPaid", "Paid", "Cancelled"].map((s) => (
          <button
            key={s}
            onClick={() => { setStatus(s); setPage(1); }}
            className={`px-3 py-1.5 rounded-lg text-sm border transition ${status === s ? "bg-blue-600 text-white border-blue-600" : "bg-white text-gray-600 border-gray-300 hover:border-blue-400"}`}
          >
            {s ? statusLabel[s]?.label : "Ø§Ù„ÙƒÙ„"}
          </button>
        ))}
      </div>

      {loading ? (
        <div className="text-center py-12 text-gray-500">Ø¬Ø§Ø±ÙŠ Ø§Ù„ØªØ­Ù…ÙŠÙ„...</div>
      ) : (
        <div className="bg-white rounded-xl shadow overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Ø±Ù‚Ù… Ø§Ù„ÙØ§ØªÙˆØ±Ø©</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Ø§Ù„Ù…Ø±ÙŠØ¶</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Ø§Ù„Ø·Ø¨ÙŠØ¨</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Ø§Ù„Ø­Ø§Ù„Ø©</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Ø§Ù„Ø¥Ø¬Ù…Ø§Ù„ÙŠ</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Ø§Ù„Ù…ØªØ¨Ù‚ÙŠ</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase">Ø§Ù„ØªØ§Ø±ÙŠØ®</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {(data?.items ?? []).map((inv) => {
                const st = statusLabel[inv.status] ?? { label: inv.status, cls: "bg-gray-100 text-gray-600" };
                return (
                  <tr key={inv.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-sm font-mono text-gray-800">{inv.invoiceNumber}</td>
                    <td className="px-4 py-3 text-sm text-gray-700">{inv.patientName}</td>
                    <td className="px-4 py-3 text-sm text-gray-600">{inv.doctorName}</td>
                    <td className="px-4 py-3">
                      <span className={`inline-flex px-2 py-0.5 rounded-full text-xs font-medium ${st.cls}`}>{st.label}</span>
                    </td>
                    <td className="px-4 py-3 text-sm text-gray-900">{inv.totalAmount.toFixed(2)}</td>
                    <td className="px-4 py-3 text-sm font-medium text-red-600">{inv.remaining.toFixed(2)}</td>
                    <td className="px-4 py-3 text-sm text-gray-500">{new Date(inv.createdAt).toLocaleDateString("ar")}</td>
                    <td className="px-4 py-3 text-left">
                      <Link href={`/finance/invoices/${inv.id}`} className="text-blue-600 hover:text-blue-800 text-sm">
                        Ø¹Ø±Ø¶
                      </Link>
                    </td>
                  </tr>
                );
              })}
              {(data?.items.length ?? 0) === 0 && (
                <tr>
                  <td colSpan={8} className="px-6 py-12 text-center text-gray-400">Ù„Ø§ ØªÙˆØ¬Ø¯ ÙÙˆØ§ØªÙŠØ±</td>
                </tr>
              )}
            </tbody>
          </table>

          {totalPages > 1 && (
            <div className="px-4 py-3 border-t flex justify-center gap-2">
              <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-3 py-1 rounded border text-sm disabled:opacity-40">Ø§Ù„Ø³Ø§Ø¨Ù‚</button>
              <span className="px-3 py-1 text-sm text-gray-600">{page} / {totalPages}</span>
              <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="px-3 py-1 rounded border text-sm disabled:opacity-40">Ø§Ù„ØªØ§Ù„ÙŠ</button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
