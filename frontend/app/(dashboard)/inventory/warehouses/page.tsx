"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface Warehouse {
  id: string;
  name: string;
  location: string | null;
  isActive: boolean;
}

export default function WarehousesPage() {
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState({ name: "", location: "" });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const r = await api.get<Warehouse[]>("/inventory/warehouses");
      setWarehouses(r.data);
    } finally {
      setLoading(false);
    }
  }

  async function create() {
    setSaving(true);
    setError(null);
    try {
      await api.post("/inventory/warehouses", { name: form.name, location: form.location || null });
      setShowCreate(false);
      setForm({ name: "", location: "" });
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
        <h1 className="text-2xl font-bold text-gray-900">المستودعات</h1>
        <button onClick={() => { setShowCreate(true); setError(null); }} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">+ مستودع جديد</button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {loading ? (
          <div className="col-span-3 text-center py-8 text-gray-400">جاري التحميل...</div>
        ) : warehouses.length === 0 ? (
          <div className="col-span-3 text-center py-8 text-gray-400">لا توجد مستودعات</div>
        ) : warehouses.map((w) => (
          <div key={w.id} className="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
            <div className="flex items-start justify-between mb-2">
              <div className="text-2xl">🏭</div>
              <span className={`text-xs px-2 py-0.5 rounded-full ${w.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"}`}>
                {w.isActive ? "نشط" : "غير نشط"}
              </span>
            </div>
            <div className="font-semibold text-gray-800">{w.name}</div>
            {w.location && <div className="text-sm text-gray-500 mt-1">{w.location}</div>}
          </div>
        ))}
      </div>

      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">مستودع جديد</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">اسم المستودع *</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الموقع</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.location} onChange={(e) => setForm({ ...form, location: e.target.value })} />
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
