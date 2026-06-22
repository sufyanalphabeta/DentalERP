"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface AssetCategory {
  id: string;
  name: string;
  description: string | null;
  depreciationRate: number | null;
}

export default function AssetCategoriesPage() {
  const [cats, setCats] = useState<AssetCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState({ name: "", description: "", depreciationRate: "" });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const r = await api.get<AssetCategory[]>("/assets/categories");
      setCats(r.data);
    } finally {
      setLoading(false);
    }
  }

  async function create() {
    setSaving(true);
    setError(null);
    try {
      await api.post("/assets/categories", {
        name: form.name,
        description: form.description || null,
        depreciationRate: form.depreciationRate ? parseFloat(form.depreciationRate) : null,
      });
      setShowCreate(false);
      setForm({ name: "", description: "", depreciationRate: "" });
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">فئات الأصول الثابتة</h1>
        <button onClick={() => { setShowCreate(true); setError(null); }} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">+ فئة جديدة</button>
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
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الوصف</th>
                <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">معدل الإهلاك %</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {cats.map((c) => (
                <tr key={c.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 text-sm font-medium text-gray-800">{c.name}</td>
                  <td className="px-4 py-3 text-sm text-gray-500">{c.description ?? "—"}</td>
                  <td className="px-4 py-3 text-sm text-gray-600">{c.depreciationRate != null ? `${c.depreciationRate}%` : "—"}</td>
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
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الوصف</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">معدل الإهلاك السنوي %</label>
                <input type="number" step="0.01" min="0" max="100" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.depreciationRate} onChange={(e) => setForm({ ...form, depreciationRate: e.target.value })} />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={create} disabled={saving || !form.name} className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">{saving ? "جاري الحفظ..." : "إنشاء"}</button>
              <button onClick={() => setShowCreate(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
