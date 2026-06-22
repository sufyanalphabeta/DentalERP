"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface VaultBalance {
  id: string;
  name: string;
  type: string;
  openingBalance: number;
  totalIn: number;
  totalOut: number;
  currentBalance: number;
}

const typeLabel: Record<string, string> = {
  cash: "نقدي",
  bank: "بنك",
  card: "بطاقة",
  pos: "نقطة بيع",
};

const typeOptions = [
  { value: "cash", label: "نقدي" },
  { value: "bank", label: "بنك" },
  { value: "card", label: "بطاقة" },
  { value: "pos", label: "نقطة بيع" },
];

const emptyForm = { name: "", type: "cash", openingBalance: "" };

export default function VaultsPage() {
  const [vaults, setVaults] = useState<VaultBalance[]>([]);
  const [loading, setLoading] = useState(true);
  const [total, setTotal] = useState(0);
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Edit state
  const [editTarget, setEditTarget] = useState<VaultBalance | null>(null);
  const [editForm, setEditForm] = useState({ name: "", type: "cash" });
  const [editSaving, setEditSaving] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const res = await api.get<VaultBalance[]>("/treasury/vaults/balances");
      setVaults(res.data);
      setTotal(res.data.reduce((sum, v) => sum + v.currentBalance, 0));
    } finally {
      setLoading(false);
    }
  }

  async function create() {
    setSaving(true);
    setError(null);
    try {
      await api.post("/treasury/vaults", {
        name: form.name,
        type: form.type,
        openingBalance: parseFloat(form.openingBalance || "0"),
      });
      setShowCreate(false);
      setForm(emptyForm);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string; error?: string } } };
      setError(err?.response?.data?.message ?? err?.response?.data?.error ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  }

  function openEdit(v: VaultBalance) {
    setEditTarget(v);
    setEditForm({ name: v.name, type: v.type });
    setEditError(null);
  }

  async function updateVault() {
    if (!editTarget) return;
    setEditSaving(true);
    setEditError(null);
    try {
      await api.put(`/treasury/vaults/${editTarget.id}`, {
        name: editForm.name,
        type: editForm.type,
      });
      setEditTarget(null);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string; error?: string } } };
      setEditError(err?.response?.data?.message ?? err?.response?.data?.error ?? "حدث خطأ");
    } finally {
      setEditSaving(false);
    }
  }

  const balanceColor = (b: number) => b >= 0 ? "text-gray-800" : "text-red-600";

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">الخزائن</h1>
        <div className="flex gap-2">
          <button onClick={() => { setShowCreate(true); setError(null); setForm(emptyForm); }}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">
            + خزينة جديدة
          </button>
          <button onClick={load} className="border px-4 py-2 rounded-lg text-sm text-gray-600 hover:bg-gray-50">
            تحديث
          </button>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-12 text-gray-500">جاري التحميل...</div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
            {vaults.map((v) => (
              <div key={v.id} className="bg-white rounded-xl shadow p-5">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-xs px-2 py-0.5 rounded-full bg-blue-100 text-blue-700">
                    {typeLabel[v.type] ?? v.type}
                  </span>
                  <button
                    onClick={() => openEdit(v)}
                    className="text-xs text-gray-400 hover:text-blue-600 underline"
                  >
                    تعديل
                  </button>
                </div>
                <div className="text-base font-semibold text-gray-800 mb-1">{v.name}</div>
                <div className="text-xs text-gray-400 mb-3">رصيد افتتاحي: {v.openingBalance.toFixed(2)}</div>
                <div className="grid grid-cols-2 gap-2 text-xs text-gray-500 mb-3">
                  <div>وارد: <span className="text-green-700 font-medium">{v.totalIn.toFixed(2)}</span></div>
                  <div>صادر: <span className="text-red-600 font-medium">{v.totalOut.toFixed(2)}</span></div>
                </div>
                <div className={`text-2xl font-bold ${balanceColor(v.currentBalance)}`}>
                  {v.currentBalance.toFixed(2)} <span className="text-sm font-normal text-gray-500">د.ل</span>
                </div>
              </div>
            ))}
            {vaults.length === 0 && (
              <div className="col-span-4 text-center py-12 text-gray-400">لا توجد خزائن مضافة</div>
            )}
          </div>

          <div className="bg-blue-50 border border-blue-200 rounded-xl p-5 flex items-center justify-between">
            <span className="text-base font-semibold text-blue-800">إجمالي الرصيد الحالي</span>
            <span className={`text-2xl font-bold ${balanceColor(total)}`}>
              {total.toFixed(2)} د.ل
            </span>
          </div>
        </>
      )}

      {/* Create modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">خزينة جديدة</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">اسم الخزينة *</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  placeholder="مثال: الصندوق الرئيسي"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">نوع الخزينة *</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.type}
                  onChange={(e) => setForm({ ...form, type: e.target.value })}>
                  {typeOptions.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الرصيد الافتتاحي</label>
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  placeholder="0.00"
                  value={form.openingBalance}
                  onChange={(e) => setForm({ ...form, openingBalance: e.target.value })}
                />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button
                onClick={create}
                disabled={saving || !form.name}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50"
              >
                {saving ? "جاري الإنشاء..." : "إنشاء الخزينة"}
              </button>
              <button onClick={() => setShowCreate(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}

      {/* Edit modal */}
      {editTarget && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">تعديل الخزينة</h2>
            {editError && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{editError}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">اسم الخزينة *</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={editForm.name}
                  onChange={(e) => setEditForm({ ...editForm, name: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">نوع الخزينة *</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={editForm.type}
                  onChange={(e) => setEditForm({ ...editForm, type: e.target.value })}>
                  {typeOptions.map((t) => <option key={t.value} value={t.value}>{t.label}</option>)}
                </select>
              </div>
              <p className="text-xs text-gray-400">ملاحظة: تعديل الاسم والنوع فقط — الرصيد الافتتاحي لا يمكن تغييره بعد الإنشاء</p>
            </div>
            <div className="flex gap-3 mt-5">
              <button
                onClick={updateVault}
                disabled={editSaving || !editForm.name}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50"
              >
                {editSaving ? "جاري الحفظ..." : "حفظ التعديلات"}
              </button>
              <button onClick={() => setEditTarget(null)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
