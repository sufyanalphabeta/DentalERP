"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface Category {
  id: string;
  name: string;
  nameAr: string | null;
  description: string | null;
}

const emptyForm = { name: "", nameAr: "" };

export default function InventoryCategoriesPage() {
  const [cats, setCats] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [showModal, setShowModal] = useState(false);
  const [editTarget, setEditTarget] = useState<Category | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const r = await api.get<Category[]>("/inventory/item-categories");
      setCats(r.data);
    } finally {
      setLoading(false);
    }
  }

  function openCreate() {
    setEditTarget(null);
    setForm(emptyForm);
    setError(null);
    setShowModal(true);
  }

  function openEdit(cat: Category) {
    setEditTarget(cat);
    setForm({ name: cat.name, nameAr: cat.nameAr ?? "" });
    setError(null);
    setShowModal(true);
  }

  async function save() {
    if (!form.name.trim()) { setError("اسم الفئة مطلوب"); return; }
    setSaving(true);
    setError(null);
    try {
      if (editTarget) {
        await api.put(`/inventory/item-categories/${editTarget.id}`, { name: form.name.trim(), nameAr: form.nameAr || null });
      } else {
        await api.post("/inventory/item-categories", { name: form.name.trim(), nameAr: form.nameAr || null, description: null });
      }
      setShowModal(false);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  }

  async function deleteCategory(id: string, name: string) {
    if (!confirm(`هل أنت متأكد من حذف الفئة "${name}"؟`)) return;
    try {
      await api.delete(`/inventory/item-categories/${id}`);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      alert(err?.response?.data?.error ?? "تعذر الحذف");
    }
  }

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">فئات المخزون</h1>
        <button onClick={openCreate} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">+ فئة جديدة</button>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        {loading ? (
          <div className="p-8 text-center text-gray-400">جاري التحميل...</div>
        ) : cats.length === 0 ? (
          <div className="p-8 text-center text-gray-400">لا توجد فئات</div>
        ) : (
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">اسم الفئة</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الاسم بالعربي</th>
                <th className="px-4 py-3"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {cats.map((c) => (
                <tr key={c.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 text-sm font-medium text-gray-800">{c.name}</td>
                  <td className="px-4 py-3 text-sm text-gray-500">{c.nameAr ?? "—"}</td>
                  <td className="px-4 py-3 flex gap-3 justify-end">
                    <button onClick={() => openEdit(c)} className="text-xs text-blue-600 hover:underline">تعديل</button>
                    <button onClick={() => deleteCategory(c.id, c.name)} className="text-xs text-red-500 hover:underline">حذف</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {showModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">{editTarget ? "تعديل الفئة" : "فئة جديدة"}</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">اسم الفئة (إنجليزي) *</label>
                <input
                  autoFocus
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الاسم بالعربي</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.nameAr}
                  onChange={(e) => setForm({ ...form, nameAr: e.target.value })}
                  placeholder="اختياري"
                />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button
                onClick={save}
                disabled={saving || !form.name.trim()}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50"
              >
                {saving ? "جاري الحفظ..." : editTarget ? "حفظ التعديل" : "إنشاء"}
              </button>
              <button onClick={() => setShowModal(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
