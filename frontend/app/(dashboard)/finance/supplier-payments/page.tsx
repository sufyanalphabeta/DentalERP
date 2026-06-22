"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface Supplier { id: string; name: string; supplierCode: string; computedBalance: number; }
interface Vault { id: string; name: string; currentBalance: number; }
interface Payment {
  id: string; paymentNumber: string; supplierId: string;
  vaultId: string; amount: number; paymentDate: string;
  referenceNumber: string | null; notes: string | null; createdAt: string;
}
interface PaymentsResponse { items: Payment[]; total: number; }

const emptyForm = {
  supplierId: "", vaultId: "", amount: "", paymentDate: new Date().toISOString().split("T")[0],
  referenceNumber: "", notes: "",
};

export default function SupplierPaymentsPage() {
  const [payments, setPayments] = useState<Payment[]>([]);
  const [total, setTotal] = useState(0);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [vaults, setVaults] = useState<Vault[]>([]);
  const [loading, setLoading] = useState(true);
  const [filterSupplier, setFilterSupplier] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 25;
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([
      api.get<{ suppliers: Supplier[] }>("/suppliers?pageSize=500&activeOnly=true"),
      api.get<Vault[]>("/treasury/vaults/balances"),
    ]).then(([s, v]) => {
      setSuppliers(s.data.suppliers ?? []);
      setVaults(v.data ?? []);
    }).catch(() => {});
  }, []);

  useEffect(() => { load(); }, [filterSupplier, page]);

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
      if (filterSupplier) params.set("supplierId", filterSupplier);
      const r = await api.get<PaymentsResponse>(`/purchasing/supplier-payments?${params}`);
      setPayments(r.data.items ?? []);
      setTotal(r.data.total ?? 0);
    } finally {
      setLoading(false);
    }
  }

  async function save() {
    if (!form.supplierId) { setError("يجب اختيار المورد"); return; }
    if (!form.vaultId) { setError("يجب اختيار الخزينة"); return; }
    if (!form.amount || parseFloat(form.amount) <= 0) { setError("يجب إدخال مبلغ صحيح"); return; }
    setSaving(true);
    setError(null);
    try {
      await api.post("/purchasing/supplier-payments", {
        supplierId: form.supplierId,
        vaultId: form.vaultId,
        amount: parseFloat(form.amount),
        paymentDate: form.paymentDate,
        referenceNumber: form.referenceNumber || null,
        notes: form.notes || null,
        paidById: null,
      });
      setShowCreate(false);
      setForm(emptyForm);
      setSuccess("تم تسجيل الدفعة بنجاح");
      setTimeout(() => setSuccess(null), 4000);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string; message?: string } } };
      setError(err?.response?.data?.error ?? err?.response?.data?.message ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  }

  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const supplierMap = Object.fromEntries(suppliers.map((s) => [s.id, s.name]));

  return (
    <div className="p-6" dir="rtl">
      {success && (
        <div className="fixed top-4 left-1/2 -translate-x-1/2 z-50 bg-green-600 text-white px-6 py-3 rounded-xl shadow-lg text-sm font-medium">
          ✓ {success}
        </div>
      )}

      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">مدفوعات الموردين</h1>
        <button
          onClick={() => { setShowCreate(true); setError(null); setForm(emptyForm); }}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
        >
          + دفعة جديدة
        </button>
      </div>

      <div className="flex gap-3 mb-4">
        <select
          value={filterSupplier}
          onChange={(e) => { setFilterSupplier(e.target.value); setPage(1); }}
          className="border rounded-lg px-3 py-2 text-sm"
        >
          <option value="">كل الموردين</option>
          {suppliers.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
        </select>
        <button onClick={() => { setFilterSupplier(""); setPage(1); }} className="text-sm text-gray-500 border px-3 py-2 rounded-lg hover:bg-gray-50">
          إعادة تعيين
        </button>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs text-gray-500">رقم الدفعة</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">المورد</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">المبلغ</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">تاريخ الدفع</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">مرجع</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">ملاحظات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={6} className="py-8 text-center text-gray-400">جاري التحميل...</td></tr>
            ) : payments.length === 0 ? (
              <tr><td colSpan={6} className="py-8 text-center text-gray-400">لا توجد دفعات</td></tr>
            ) : payments.map((p) => (
              <tr key={p.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 text-sm font-mono text-blue-700">{p.paymentNumber}</td>
                <td className="px-4 py-3 text-sm text-gray-800">{supplierMap[p.supplierId] ?? p.supplierId}</td>
                <td className="px-4 py-3 text-sm font-bold text-green-700">{p.amount.toFixed(2)} د.ل</td>
                <td className="px-4 py-3 text-xs text-gray-500">{p.paymentDate}</td>
                <td className="px-4 py-3 text-xs text-gray-500">{p.referenceNumber ?? "—"}</td>
                <td className="px-4 py-3 text-xs text-gray-400">{p.notes ?? "—"}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {totalPages > 1 && (
          <div className="px-4 py-3 border-t flex justify-center gap-2">
            <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-3 py-1 rounded border text-sm disabled:opacity-40">السابق</button>
            <span className="px-3 py-1 text-sm text-gray-600">{page} / {totalPages}</span>
            <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="px-3 py-1 rounded border text-sm disabled:opacity-40">التالي</button>
          </div>
        )}
      </div>

      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">دفعة جديدة للمورد</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">المورد *</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.supplierId} onChange={(e) => setForm({ ...form, supplierId: e.target.value })}>
                  <option value="">— اختر المورد —</option>
                  {suppliers.map((s) => (
                    <option key={s.id} value={s.id}>{s.name} {s.computedBalance > 0 ? `(مديون: ${s.computedBalance.toFixed(2)})` : ""}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الخزينة *</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.vaultId} onChange={(e) => setForm({ ...form, vaultId: e.target.value })}>
                  <option value="">— اختر الخزينة —</option>
                  {vaults.map((v) => <option key={v.id} value={v.id}>{v.name} ({v.currentBalance.toFixed(2)} د.ل)</option>)}
                </select>
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">المبلغ *</label>
                  <input type="number" step="0.01" min="0.01" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">تاريخ الدفع</label>
                  <input type="date" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.paymentDate} onChange={(e) => setForm({ ...form, paymentDate: e.target.value })} />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">رقم المرجع / الشيك</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.referenceNumber} onChange={(e) => setForm({ ...form, referenceNumber: e.target.value })} placeholder="اختياري" />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} placeholder="اختياري" />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={save} disabled={saving} className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">
                {saving ? "جاري الحفظ..." : "تسجيل الدفعة"}
              </button>
              <button onClick={() => setShowCreate(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
