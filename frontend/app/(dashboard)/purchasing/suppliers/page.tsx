"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";

interface Supplier {
  id: string;
  supplierCode: string;
  name: string;
  nameAr: string | null;
  category: string | null;
  phone: string | null;
  email: string | null;
  isActive: boolean;
  computedBalance: number;
}

interface SuppliersResult {
  suppliers: Supplier[];
  total: number;
}

const CATEGORIES = [
  { value: "Medical",    label: "طبي" },
  { value: "Equipment",  label: "معدات" },
  { value: "General",    label: "عام" },
  { value: "Lab",        label: "مختبر" },
  { value: "Radiology",  label: "أشعة" },
  { value: "Pharma",     label: "دوائي" },
];

const emptyForm = {
  name: "",
  nameAr: "",
  category: "" as string,
  contactPerson: "",
  phone: "",
  email: "",
  address: "",
  taxNumber: "",
  paymentTermsDays: "30",
  creditLimit: "0",
  openingBalance: "0",
};

export default function SuppliersPage() {
  const [data, setData] = useState<SuppliersResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 20;
  const [activeOnly, setActiveOnly] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  useEffect(() => { load(); }, [search, page, activeOnly]);

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
      if (search) params.set("search", search);
      if (activeOnly) params.set("activeOnly", "true");
      const r = await api.get<SuppliersResult>(`/suppliers?${params}`);
      setData(r.data);
    } finally {
      setLoading(false);
    }
  }

  async function deactivateSupplier(id: string, name: string) {
    if (!confirm(`هل تريد إيقاف المورد "${name}"؟\n\nسيتم إيقاف تشغيله ولن يظهر في القوائم.`)) return;
    setDeletingId(id);
    setDeleteError(null);
    try {
      const r = await api.delete<{ result: string }>(`/purchasing/suppliers/${id}`);
      const msg = r.data.result === "already_inactive" ? "المورد موقوف مسبقاً" : "تم إيقاف المورد";
      setSuccess(msg);
      setTimeout(() => setSuccess(null), 4000);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setDeleteError(err?.response?.data?.error ?? "لا يمكن الإيقاف");
      setTimeout(() => setDeleteError(null), 6000);
    } finally {
      setDeletingId(null);
    }
  }

  async function create() {
    setSaving(true);
    setError(null);
    try {
      await api.post("/suppliers", {
        name: form.name.trim(),
        nameAr: form.nameAr || null,
        category: form.category || null,
        contactPerson: form.contactPerson || null,
        phone: form.phone || null,
        email: form.email || null,
        address: form.address || null,
        paymentTermsDays: parseInt(form.paymentTermsDays) || 30,
        creditLimit: parseFloat(form.creditLimit) || 0,
        openingBalance: parseFloat(form.openingBalance) || 0,
        notes: form.taxNumber ? `رقم ضريبي: ${form.taxNumber}` : null,
      });
      setShowCreate(false);
      setForm(emptyForm);
      setSuccess("تم إضافة المورد بنجاح");
      setTimeout(() => setSuccess(null), 3500);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string; title?: string; detail?: string } } };
      setError(
        err?.response?.data?.error ??
        err?.response?.data?.detail ??
        err?.response?.data?.title ??
        "حدث خطأ أثناء الحفظ"
      );
    } finally {
      setSaving(false);
    }
  }

  const suppliers = data?.suppliers ?? [];
  const total = data?.total ?? 0;
  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  function balanceLabel(balance: number) {
    if (balance === 0) return { text: "مسوّى", cls: "text-green-700 bg-green-50" };
    return { text: `${balance.toFixed(2)} د.ل دائن`, cls: "text-red-700 bg-red-50" };
  }

  return (
    <div className="p-6" dir="rtl">
      {/* Success toast */}
      {success && (
        <div className="fixed top-4 left-1/2 -translate-x-1/2 z-50 bg-green-600 text-white px-6 py-3 rounded-xl shadow-lg text-sm font-medium">
          ✓ {success}
        </div>
      )}
      {deleteError && (
        <div className="fixed top-4 left-1/2 -translate-x-1/2 z-50 bg-red-600 text-white px-6 py-3 rounded-xl shadow-lg text-sm font-medium max-w-sm text-center">
          ✕ {deleteError}
        </div>
      )}

      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">الموردون</h1>
        <button
          onClick={() => { setShowCreate(true); setError(null); setForm(emptyForm); }}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
        >
          + مورد جديد
        </button>
      </div>

      <div className="flex flex-wrap gap-3 mb-4">
        <input
          type="text" placeholder="بحث باسم المورد أو الكود..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          className="border rounded-lg px-3 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-blue-400"
        />
        <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
          <input type="checkbox" checked={activeOnly} onChange={(e) => { setActiveOnly(e.target.checked); setPage(1); }} className="rounded" />
          نشط فقط
        </label>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الكود</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المورد</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الهاتف</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الرصيد</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={5} className="text-center py-8 text-gray-400">جاري التحميل...</td></tr>
            ) : suppliers.length === 0 ? (
              <tr><td colSpan={5} className="text-center py-8 text-gray-400">لا يوجد موردون</td></tr>
            ) : suppliers.map((s) => {
              const bal = balanceLabel(s.computedBalance ?? 0);
              return (
                <tr key={s.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 text-xs font-mono text-gray-500">{s.supplierCode}</td>
                  <td className="px-4 py-3">
                    <div className="text-sm font-medium text-gray-800">{s.name}</div>
                    {s.nameAr && <div className="text-xs text-gray-400">{s.nameAr}</div>}
                    {s.email && <div className="text-xs text-gray-400">{s.email}</div>}
                    {s.category && (
                      <span className="text-xs bg-blue-50 text-blue-600 px-1.5 py-0.5 rounded mt-0.5 inline-block">
                        {CATEGORIES.find(c => c.value === s.category)?.label ?? s.category}
                      </span>
                    )}
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-600">{s.phone ?? "—"}</td>
                  <td className="px-4 py-3">
                    <span className={`text-xs font-semibold px-2 py-1 rounded-lg ${bal.cls}`}>
                      {bal.text}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <div className="flex items-center gap-2">
                      <Link href={`/purchasing/suppliers/${s.id}`} className="text-xs text-blue-600 hover:underline">
                        التفاصيل
                      </Link>
                      <button
                        onClick={() => deactivateSupplier(s.id, s.name)}
                        disabled={deletingId === s.id || !s.isActive}
                        className="text-xs text-orange-500 hover:text-orange-700 disabled:opacity-40"
                      >
                        {deletingId === s.id ? "..." : s.isActive ? "إيقاف" : "موقوف"}
                      </button>
                    </div>
                  </td>
                </tr>
              );
            })}
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

      {/* Create Modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-lg p-6 max-h-[90vh] overflow-y-auto" dir="rtl">
            <h2 className="text-lg font-bold mb-4">مورد جديد</h2>

            <div className="bg-blue-50 border border-blue-100 rounded-lg px-3 py-2 mb-4 text-xs text-blue-700">
              💡 رصيد المورد يُسجَّل تلقائياً كـ <strong>دائن</strong> عند استلام البضاعة ويُخفَّض عند سداد الدفعات
            </div>

            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{error}</div>}

            <div className="grid grid-cols-2 gap-3">
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">اسم المورد *</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  placeholder="مثال: شركة الأدوية الوطنية"
                  autoFocus
                />
              </div>
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">الاسم بالعربي</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.nameAr}
                  onChange={(e) => setForm({ ...form, nameAr: e.target.value })}
                  placeholder="اختياري"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الفئة</label>
                <select
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.category}
                  onChange={(e) => setForm({ ...form, category: e.target.value })}
                >
                  <option value="">— بدون فئة —</option>
                  {CATEGORIES.map(c => (
                    <option key={c.value} value={c.value}>{c.label}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">جهة الاتصال</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.contactPerson}
                  onChange={(e) => setForm({ ...form, contactPerson: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الهاتف</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.phone}
                  onChange={(e) => setForm({ ...form, phone: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">البريد الإلكتروني</label>
                <input
                  type="email"
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.email}
                  onChange={(e) => setForm({ ...form, email: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">شروط الدفع (أيام)</label>
                <input
                  type="number" min="1"
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.paymentTermsDays}
                  onChange={(e) => setForm({ ...form, paymentTermsDays: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">حد الائتمان</label>
                <input
                  type="number" min="0" step="0.01"
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.creditLimit}
                  onChange={(e) => setForm({ ...form, creditLimit: e.target.value })}
                />
              </div>
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">العنوان</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.address}
                  onChange={(e) => setForm({ ...form, address: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الرقم الضريبي</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.taxNumber}
                  onChange={(e) => setForm({ ...form, taxNumber: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الرصيد الافتتاحي (د.ل)</label>
                <input
                  type="number" min="0" step="0.01"
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.openingBalance}
                  onChange={(e) => setForm({ ...form, openingBalance: e.target.value })}
                  placeholder="0.00"
                />
                <p className="text-xs text-gray-400 mt-0.5">المبلغ المستحق للمورد قبل بدء التشغيل</p>
              </div>
            </div>

            <div className="flex gap-3 mt-5">
              <button
                onClick={create}
                disabled={saving || !form.name.trim()}
                className="flex-1 bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
              >
                {saving ? "جاري الحفظ..." : "حفظ المورد"}
              </button>
              <button
                onClick={() => { setShowCreate(false); setError(null); }}
                className="flex-1 border py-2.5 rounded-lg text-sm text-gray-700 hover:bg-gray-50"
              >
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
