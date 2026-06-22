"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface ExternalLab {
  id: string;
  name: string;
  contactName: string | null;
  phone: string | null;
  email: string | null;
  address: string | null;
  isActive: boolean;
}

export default function ExternalLabsPage() {
  const [labs, setLabs] = useState<ExternalLab[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState({ name: "", contactName: "", phone: "", email: "", address: "" });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const r = await api.get<ExternalLab[]>("/lab/external-labs");
      setLabs(r.data);
    } finally {
      setLoading(false);
    }
  }

  async function create() {
    setSaving(true);
    setError(null);
    try {
      await api.post("/lab/external-labs", {
        name: form.name,
        contactName: form.contactName || null,
        phone: form.phone || null,
        email: form.email || null,
        address: form.address || null,
      });
      setShowCreate(false);
      setForm({ name: "", contactName: "", phone: "", email: "", address: "" });
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
        <h1 className="text-2xl font-bold text-gray-900">المختبرات الخارجية</h1>
        <button onClick={() => { setShowCreate(true); setError(null); }} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">+ مختبر جديد</button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        {loading ? (
          <div className="col-span-3 text-center py-8 text-gray-400">جاري التحميل...</div>
        ) : labs.length === 0 ? (
          <div className="col-span-3 text-center py-8 text-gray-400">لا توجد مختبرات خارجية</div>
        ) : labs.map((lab) => (
          <div key={lab.id} className="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
            <div className="flex items-start justify-between mb-3">
              <div className="text-2xl">🔬</div>
              <span className={`text-xs px-2 py-0.5 rounded-full ${lab.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"}`}>
                {lab.isActive ? "نشط" : "غير نشط"}
              </span>
            </div>
            <div className="font-semibold text-gray-800 mb-2">{lab.name}</div>
            {lab.contactName && <div className="text-sm text-gray-600">{lab.contactName}</div>}
            {lab.phone && <div className="text-sm text-gray-500 mt-1">{lab.phone}</div>}
            {lab.email && <div className="text-sm text-gray-500">{lab.email}</div>}
          </div>
        ))}
      </div>

      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">مختبر خارجي جديد</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">اسم المختبر *</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">جهة الاتصال</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.contactName} onChange={(e) => setForm({ ...form, contactName: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الهاتف</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">البريد الإلكتروني</label>
                <input type="email" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
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
