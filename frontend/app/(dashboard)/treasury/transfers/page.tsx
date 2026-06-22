"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface Vault {
  id: string;
  name: string;
  currentBalance: number;
}

interface Transfer {
  id: string;
  transferNumber: string;
  fromVaultName: string;
  toVaultName: string;
  amount: number;
  notes: string | null;
  transferDate: string;
}

interface TransfersResponse {
  items: Transfer[];
  totalCount: number;
  page: number;
  pageSize: number;
}

function formatDate(iso: string | null | undefined) {
  if (!iso) return "—";
  const d = new Date(iso);
  if (isNaN(d.getTime())) return "—";
  return d.toLocaleDateString("ar-LY", { year: "numeric", month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" });
}

export default function TreasuryTransfersPage() {
  const [data, setData] = useState<TransfersResponse | null>(null);
  const [vaults, setVaults] = useState<Vault[]>([]);
  const [loading, setLoading] = useState(true);
  const [page, setPage] = useState(1);
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState({ fromVaultId: "", toVaultId: "", amount: "", notes: "" });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => { loadAll(); }, [page]);

  async function loadAll() {
    setLoading(true);
    await Promise.allSettled([
      api.get<Vault[]>("/treasury/vaults/balances").then((r) => setVaults(r.data)).catch(() => {}),
      api.get<TransfersResponse>(`/treasury/transfers?page=${page}&pageSize=20`)
        .then((r) => setData(r.data))
        .catch(() => setData({ items: [], totalCount: 0, page: 1, pageSize: 20 })),
    ]);
    setLoading(false);
  }

  async function createTransfer() {
    setSaving(true);
    setError(null);
    try {
      await api.post("/treasury/transfers", {
        fromVaultId: form.fromVaultId,
        toVaultId: form.toVaultId,
        amount: parseFloat(form.amount),
        notes: form.notes || null,
      });
      setShowCreate(false);
      setForm({ fromVaultId: "", toVaultId: "", amount: "", notes: "" });
      loadAll();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string; message?: string } } };
      setError(err?.response?.data?.error ?? err?.response?.data?.message ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  }

  const totalPages = data ? Math.ceil(data.totalCount / 20) : 1;

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">التحويلات بين الخزائن</h1>
        <button
          onClick={() => { setShowCreate(true); setError(null); setForm({ fromVaultId: vaults[0]?.id ?? "", toVaultId: "", amount: "", notes: "" }); }}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
        >
          + تحويل جديد
        </button>
      </div>

      {/* Vault balances */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mb-6">
        {vaults.map((v) => (
          <div key={v.id} className="bg-white rounded-xl border border-gray-100 shadow-sm p-4">
            <div className="text-xs text-gray-500 mb-1">{v.name}</div>
            <div className={`text-lg font-bold ${v.currentBalance >= 0 ? "text-gray-800" : "text-red-600"}`}>
              {v.currentBalance.toFixed(2)} د.ل
            </div>
          </div>
        ))}
      </div>

      {/* Transfers list */}
      <div className="bg-white rounded-xl shadow overflow-hidden">
        <div className="px-5 py-4 border-b flex items-center justify-between">
          <h2 className="font-semibold text-gray-800">سجل التحويلات</h2>
          {data && <span className="text-sm text-gray-400">{data.totalCount} تحويل</span>}
        </div>
        {loading ? (
          <div className="p-8 text-center text-gray-400">جاري التحميل...</div>
        ) : (data?.items ?? []).length === 0 ? (
          <div className="p-8 text-center text-gray-400">لا توجد تحويلات بعد</div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">رقم التحويل</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">من</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">إلى</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المبلغ</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">ملاحظات</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">التاريخ</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {(data?.items ?? []).map((t) => (
                <tr key={t.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 text-xs font-mono text-gray-500">{t.transferNumber}</td>
                  <td className="px-4 py-3 text-sm font-medium text-gray-800">{t.fromVaultName}</td>
                  <td className="px-4 py-3 text-sm font-medium text-gray-800">{t.toVaultName}</td>
                  <td className="px-4 py-3 text-sm font-bold text-blue-700">{t.amount.toFixed(2)} د.ل</td>
                  <td className="px-4 py-3 text-sm text-gray-500">{t.notes ?? "—"}</td>
                  <td className="px-4 py-3 text-xs text-gray-500">{formatDate(t.transferDate)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        {totalPages > 1 && (
          <div className="px-4 py-3 border-t flex justify-center gap-2">
            <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-3 py-1 rounded border text-sm disabled:opacity-40">السابق</button>
            <span className="px-3 py-1 text-sm text-gray-600">{page} / {totalPages}</span>
            <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="px-3 py-1 rounded border text-sm disabled:opacity-40">التالي</button>
          </div>
        )}
      </div>

      {/* Create transfer modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">تحويل بين الخزائن</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">من الخزينة *</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.fromVaultId}
                  onChange={(e) => setForm({ ...form, fromVaultId: e.target.value })}>
                  <option value="">— اختر خزينة —</option>
                  {vaults.map((v) => (
                    <option key={v.id} value={v.id}>{v.name} ({v.currentBalance.toFixed(2)} د.ل)</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">إلى الخزينة *</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.toVaultId}
                  onChange={(e) => setForm({ ...form, toVaultId: e.target.value })}>
                  <option value="">— اختر خزينة —</option>
                  {vaults.filter((v) => v.id !== form.fromVaultId).map((v) => (
                    <option key={v.id} value={v.id}>{v.name}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">المبلغ *</label>
                <input type="number" step="0.01" min="0.01" className="w-full border rounded-lg px-3 py-2 text-sm"
                  placeholder="0.00" value={form.amount}
                  onChange={(e) => setForm({ ...form, amount: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={createTransfer}
                disabled={saving || !form.fromVaultId || !form.toVaultId || !form.amount}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">
                {saving ? "جاري التحويل..." : "تأكيد التحويل"}
              </button>
              <button onClick={() => setShowCreate(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
