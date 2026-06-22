"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface ServiceCategory {
  id: string;
  name: string;
  sortOrder: number;
  isActive: boolean;
}

export default function ServiceCategoriesPage() {
  const [cats, setCats] = useState<ServiceCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState({ name: "", sortOrder: 0 });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Inline edit state
  const [editId, setEditId] = useState<string | null>(null);
  const [editForm, setEditForm] = useState({ name: "", sortOrder: 0 });
  const [editSaving, setEditSaving] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const r = await api.get<ServiceCategory[]>("/services/categories");
      setCats(r.data);
    } catch {
      setCats([]);
    } finally {
      setLoading(false);
    }
  }

  async function create() {
    if (!form.name.trim()) { setError("الاسم مطلوب"); return; }
    setSaving(true);
    setError(null);
    try {
      await api.post("/services/categories", { name: form.name, sortOrder: form.sortOrder });
      setShowCreate(false);
      setForm({ name: "", sortOrder: 0 });
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  }

  function startEdit(cat: ServiceCategory) {
    setEditId(cat.id);
    setEditForm({ name: cat.name, sortOrder: cat.sortOrder });
    setEditError(null);
  }

  async function saveEdit() {
    if (!editId || !editForm.name.trim()) return;
    setEditSaving(true);
    setEditError(null);
    try {
      await api.put(`/services/categories/${editId}`, { name: editForm.name, sortOrder: editForm.sortOrder });
      setEditId(null);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setEditError(err?.response?.data?.error ?? "حدث خطأ");
    } finally {
      setEditSaving(false);
    }
  }

  async function toggle(id: string) {
    try {
      await api.post(`/services/categories/${id}/toggle`, {});
      setCats((prev) => prev.map((c) => c.id === id ? { ...c, isActive: !c.isActive } : c));
    } catch {
      alert("حدث خطأ أثناء تغيير الحالة");
    }
  }

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">فئات الخدمات</h1>
        <button
          onClick={() => { setShowCreate(true); setError(null); setForm({ name: "", sortOrder: 0 }); }}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
        >
          + فئة جديدة
        </button>
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
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 w-20">الترتيب</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500 w-24">الحالة</th>
                <th className="px-4 py-3 w-36"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {cats.map((c) => (
                <tr key={c.id} className="hover:bg-gray-50">
                  {editId === c.id ? (
                    <>
                      <td className="px-4 py-2">
                        {editError && <div className="text-xs text-red-600 mb-1">{editError}</div>}
                        <input
                          className="w-full border rounded-lg px-2 py-1 text-sm focus:ring-1 focus:ring-blue-400"
                          value={editForm.name}
                          onChange={(e) => setEditForm({ ...editForm, name: e.target.value })}
                          autoFocus
                        />
                      </td>
                      <td className="px-4 py-2">
                        <input
                          type="number"
                          className="w-16 border rounded-lg px-2 py-1 text-sm text-center"
                          value={editForm.sortOrder}
                          onChange={(e) => setEditForm({ ...editForm, sortOrder: parseInt(e.target.value) || 0 })}
                        />
                      </td>
                      <td></td>
                      <td className="px-4 py-2">
                        <div className="flex gap-2">
                          <button
                            onClick={saveEdit}
                            disabled={editSaving}
                            className="text-xs bg-blue-600 text-white px-3 py-1 rounded hover:bg-blue-700 disabled:opacity-50"
                          >
                            {editSaving ? "..." : "حفظ"}
                          </button>
                          <button
                            onClick={() => setEditId(null)}
                            className="text-xs border px-3 py-1 rounded text-gray-600 hover:bg-gray-50"
                          >
                            إلغاء
                          </button>
                        </div>
                      </td>
                    </>
                  ) : (
                    <>
                      <td className="px-4 py-3 text-sm font-medium text-gray-800">{c.name}</td>
                      <td className="px-4 py-3 text-sm text-gray-500 text-center">{c.sortOrder}</td>
                      <td className="px-4 py-3">
                        <span className={`text-xs px-2 py-0.5 rounded-full ${c.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"}`}>
                          {c.isActive ? "نشط" : "غير نشط"}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex gap-2 justify-end">
                          <button
                            onClick={() => startEdit(c)}
                            className="text-xs text-blue-600 hover:underline"
                          >
                            تعديل
                          </button>
                          <button
                            onClick={() => toggle(c.id)}
                            className={`text-xs ${c.isActive ? "text-amber-600 hover:text-amber-700" : "text-green-600 hover:text-green-700"}`}
                          >
                            {c.isActive ? "إيقاف" : "تفعيل"}
                          </button>
                        </div>
                      </td>
                    </>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">فئة جديدة</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الاسم *</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.name}
                  onChange={(e) => setForm({ ...form, name: e.target.value })}
                  autoFocus
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الترتيب</label>
                <input
                  type="number"
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={form.sortOrder}
                  onChange={(e) => setForm({ ...form, sortOrder: parseInt(e.target.value) || 0 })}
                />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button
                onClick={create}
                disabled={saving || !form.name}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50"
              >
                {saving ? "جاري الحفظ..." : "إنشاء"}
              </button>
              <button onClick={() => setShowCreate(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
