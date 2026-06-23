"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";
import { useAuthStore } from "@/stores/authStore";

interface ExpenseCategory {
  id: string;
  name: string;
}

interface Vault {
  id: string;
  name: string;
  type: string;
  currentBalance: number;
}

interface Expense {
  id: string;
  description: string;
  amount: number;
  categoryId: string | null;
  categoryName: string | null;
  costCenter: string | null;
  expenseDate: string;
  notes: string | null;
  relatedModule: string | null;
  vaultId: string | null;
}

interface ExpensesResponse {
  items: Expense[];
  totalCount: number;
  page: number;
  pageSize: number;
}

const COST_CENTERS = [
  { value: "GENERAL", label: "عام" },
  { value: "CLINIC", label: "العيادة" },
  { value: "LABORATORY", label: "المختبر" },
  { value: "RADIOLOGY", label: "الأشعة" },
  { value: "TRAINING", label: "التدريب" },
  { value: "ADMINISTRATION", label: "الإدارة" },
];

const emptyForm = {
  description: "",
  amount: "",
  categoryId: "",
  costCenter: "GENERAL",
  expenseDate: new Date().toISOString().split("T")[0],
  notes: "",
  vaultId: "",
};

export default function ExpensesPage() {
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const [data, setData] = useState<ExpensesResponse | null>(null);
  const [categories, setCategories] = useState<ExpenseCategory[]>([]);
  const [vaults, setVaults] = useState<Vault[]>([]);
  const [loading, setLoading] = useState(true);
  const [catFilter, setCatFilter] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [page, setPage] = useState(1);
  const [showCreate, setShowCreate] = useState(false);
  const [editTarget, setEditTarget] = useState<Expense | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.get<ExpenseCategory[]>("/expenses/categories").then((r) => setCategories(r.data)).catch(() => {});
    api.get<Vault[]>("/treasury/vaults/balances").then((r) => setVaults(r.data ?? [])).catch(() => {});
  }, []);

  useEffect(() => { load(); }, [catFilter, from, to, page]);

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: "25" });
      if (catFilter) params.set("categoryId", catFilter);
      if (from) params.set("dateFrom", from);
      if (to) params.set("dateTo", to);
      const r = await api.get<ExpensesResponse>(`/expenses?${params}`);
      setData(r.data);
    } finally {
      setLoading(false);
    }
  }

  async function save() {
    if (!form.vaultId) { setError("يجب تحديد الخزينة"); return; }
    setSaving(true);
    setError(null);
    try {
      const payload = {
        description: form.description,
        amount: parseFloat(form.amount),
        categoryId: form.categoryId || null,
        costCenter: form.costCenter || "GENERAL",
        expenseDate: form.expenseDate,
        notes: form.notes || null,
        vaultId: form.vaultId || null,
        relatedModule: null,
        relatedEntityId: null,
        createdById: null,
      };
      if (editTarget) {
        await api.put(`/expenses/${editTarget.id}`, {
          description: payload.description,
          amount: payload.amount,
          categoryId: payload.categoryId,
          costCenter: payload.costCenter,
          expenseDate: payload.expenseDate,
          notes: payload.notes,
        });
      } else {
        await api.post("/expenses", payload);
      }
      setShowCreate(false);
      setEditTarget(null);
      setForm(emptyForm);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string; message?: string } } };
      setError(err?.response?.data?.error ?? err?.response?.data?.message ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  }

  async function deleteExpense(id: string) {
    if (!confirm("هل أنت متأكد من الحذف؟")) return;
    await api.delete(`/expenses/${id}`);
    load();
  }

  function openEdit(exp: Expense) {
    setEditTarget(exp);
    setForm({
      description: exp.description,
      amount: String(exp.amount),
      categoryId: exp.categoryId ?? "",
      costCenter: exp.costCenter ?? "GENERAL",
      expenseDate: (exp.expenseDate ?? "").split("T")[0],
      notes: exp.notes ?? "",
      vaultId: exp.vaultId ?? "",
    });
    setShowCreate(true);
    setError(null);
  }

  const totalPages = data ? Math.ceil(data.totalCount / 25) : 1;
  const totalAmount = (data?.items ?? []).reduce((s, e) => s + e.amount, 0);

  if (!hasPermission("Financial.Expenses.View")) {
    return (
      <div className="p-12 text-center text-gray-400" dir="rtl">
        <p className="text-lg font-semibold">403 — غير مصرح</p>
        <p className="text-sm mt-1">ليس لديك صلاحية عرض المصروفات</p>
      </div>
    );
  }

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">المصروفات</h1>
          {!loading && data && (
            <p className="text-sm text-gray-500 mt-0.5">إجمالي: {totalAmount.toFixed(2)} د.ل</p>
          )}
        </div>
        <div className="flex gap-2">
          <Link href="/expenses/categories" className="border px-3 py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-50">الفئات</Link>
          <button onClick={() => { setShowCreate(true); setEditTarget(null); setError(null); setForm(emptyForm); }}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">
            + مصروف جديد
          </button>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-3 mb-4">
        <select value={catFilter} onChange={(e) => { setCatFilter(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm">
          <option value="">كل الفئات</option>
          {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        <input type="date" value={from} onChange={(e) => { setFrom(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm" />
        <input type="date" value={to} onChange={(e) => { setTo(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm" />
        <button onClick={() => { setCatFilter(""); setFrom(""); setTo(""); setPage(1); }} className="text-sm text-gray-500 border px-3 py-2 rounded-lg hover:bg-gray-50">إعادة تعيين</button>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الوصف</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الفئة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الخزينة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">مركز التكلفة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المبلغ</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">التاريخ</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={7} className="text-center py-8 text-gray-400">جاري التحميل...</td></tr>
            ) : (data?.items ?? []).length === 0 ? (
              <tr><td colSpan={7} className="text-center py-8 text-gray-400">لا توجد مصروفات</td></tr>
            ) : (data?.items ?? []).map((exp) => (
              <tr key={exp.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <div className="text-sm font-medium text-gray-800">{exp.description}</div>
                  {exp.notes && <div className="text-xs text-gray-400">{exp.notes}</div>}
                </td>
                <td className="px-4 py-3 text-sm text-gray-600">{exp.categoryName ?? "—"}</td>
                <td className="px-4 py-3 text-sm text-gray-600">{vaults.find((v) => v.id === exp.vaultId)?.name ?? "—"}</td>
                <td className="px-4 py-3 text-sm text-gray-600">{exp.costCenter ?? "—"}</td>
                <td className="px-4 py-3 text-sm font-medium text-gray-800">{exp.amount.toFixed(2)} د.ل</td>
                <td className="px-4 py-3 text-xs text-gray-500">{new Date(exp.expenseDate).toLocaleDateString("ar")}</td>
                <td className="px-4 py-3 flex gap-2">
                  <button onClick={() => openEdit(exp)} className="text-xs text-blue-600 hover:underline">تعديل</button>
                  <button onClick={() => deleteExpense(exp.id)} className="text-xs text-red-500 hover:underline">حذف</button>
                </td>
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
            <h2 className="text-lg font-bold mb-4">{editTarget ? "تعديل المصروف" : "مصروف جديد"}</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الوصف *</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">المبلغ *</label>
                  <input type="number" step="0.01" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.amount} onChange={(e) => setForm({ ...form, amount: e.target.value })} />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">التاريخ *</label>
                  <input type="date" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.expenseDate} onChange={(e) => setForm({ ...form, expenseDate: e.target.value })} />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  الخزينة <span className="text-red-500">*</span>
                  {editTarget && <span className="text-xs text-gray-400 mr-1">(لا يمكن تغييرها بعد التسجيل)</span>}
                </label>
                {editTarget ? (
                  <div className="w-full border border-gray-200 rounded-lg px-3 py-2 text-sm bg-gray-50 text-gray-600">
                    {vaults.find((v) => v.id === form.vaultId)?.name ?? "—"}
                  </div>
                ) : (
                  <select
                    className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={form.vaultId}
                    onChange={(e) => setForm({ ...form, vaultId: e.target.value })}
                  >
                    <option value="">— اختر الخزينة —</option>
                    {vaults.map((v) => (
                      <option key={v.id} value={v.id}>
                        {v.name} (رصيد: {v.currentBalance.toFixed(2)} د.ل)
                      </option>
                    ))}
                  </select>
                )}
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الفئة</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: e.target.value })}>
                  <option value="">— بدون فئة —</option>
                  {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">مركز التكلفة</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.costCenter} onChange={(e) => setForm({ ...form, costCenter: e.target.value })}>
                  {COST_CENTERS.map((c) => <option key={c.value} value={c.value}>{c.label}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button
                onClick={save}
                disabled={saving || !form.description || !form.amount || (!editTarget && !form.vaultId)}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50"
              >
                {saving ? "جاري الحفظ..." : "حفظ"}
              </button>
              <button onClick={() => { setShowCreate(false); setEditTarget(null); }} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
