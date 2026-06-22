"use client";

import { useState, useEffect } from "react";
import { api } from "@/lib/api";

interface InsuranceCompany {
  id: string;
  name: string;
  nameAr: string | null;
  contactPerson: string | null;
  phone: string | null;
  email: string | null;
  defaultCoveragePercent: number;
  isActive: boolean;
}

const emptyForm = { name: "", nameAr: "", contactPerson: "", phone: "", email: "", defaultCoveragePercent: 80, isActive: true };

export default function InsuranceSettingsPage() {
  const [companies, setCompanies] = useState<InsuranceCompany[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = () => {
    api.get("/insurance/companies?activeOnly=false").then((r) => {
      setCompanies(r.data ?? []);
      setLoading(false);
    });
  };

  useEffect(load, []);

  function openCreate() {
    setEditId(null);
    setForm(emptyForm);
    setError(null);
    setShowForm(true);
  }

  function openEdit(c: InsuranceCompany) {
    setEditId(c.id);
    setForm({
      name: c.name,
      nameAr: c.nameAr ?? "",
      contactPerson: c.contactPerson ?? "",
      phone: c.phone ?? "",
      email: c.email ?? "",
      defaultCoveragePercent: c.defaultCoveragePercent,
      isActive: c.isActive,
    });
    setError(null);
    setShowForm(true);
  }

  const handleSave = async () => {
    if (!form.name.trim()) return;
    setSaving(true);
    setError(null);
    try {
      if (editId) {
        await api.put(`/insurance/companies/${editId}`, {
          ...form,
          nameAr: form.nameAr || null,
          contactPerson: form.contactPerson || null,
          phone: form.phone || null,
          email: form.email || null,
        });
      } else {
        await api.post("/insurance/companies", {
          ...form,
          nameAr: form.nameAr || null,
          contactPerson: form.contactPerson || null,
          phone: form.phone || null,
          email: form.email || null,
        });
      }
      setShowForm(false);
      setForm(emptyForm);
      setEditId(null);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string } } };
      setError(err?.response?.data?.message ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="p-6 max-w-4xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">شركات التأمين</h1>
        <button onClick={openCreate} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">
          + إضافة شركة تأمين
        </button>
      </div>

      {showForm && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md" dir="rtl">
            <h2 className="text-lg font-semibold mb-4">{editId ? "تعديل شركة التأمين" : "إضافة شركة تأمين جديدة"}</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="space-y-3">
              <input placeholder="اسم الشركة *" className="w-full border rounded-lg p-2 text-sm" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              <input placeholder="الاسم بالعربية" className="w-full border rounded-lg p-2 text-sm" value={form.nameAr} onChange={(e) => setForm({ ...form, nameAr: e.target.value })} />
              <input placeholder="اسم المسؤول" className="w-full border rounded-lg p-2 text-sm" value={form.contactPerson} onChange={(e) => setForm({ ...form, contactPerson: e.target.value })} />
              <div className="grid grid-cols-2 gap-3">
                <input placeholder="رقم الهاتف" className="w-full border rounded-lg p-2 text-sm" value={form.phone} onChange={(e) => setForm({ ...form, phone: e.target.value })} />
                <input placeholder="البريد الإلكتروني" type="email" className="w-full border rounded-lg p-2 text-sm" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} />
              </div>
              <div>
                <label className="text-sm text-gray-600 block mb-1">نسبة التغطية الافتراضية (%)</label>
                <input type="number" min={0} max={100} className="w-full border rounded-lg p-2 text-sm" value={form.defaultCoveragePercent} onChange={(e) => setForm({ ...form, defaultCoveragePercent: Number(e.target.value) })} />
              </div>
              {editId && (
                <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
                  <input type="checkbox" checked={form.isActive} onChange={(e) => setForm({ ...form, isActive: e.target.checked })} className="rounded" />
                  شركة نشطة
                </label>
              )}
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={handleSave} disabled={saving || !form.name.trim()} className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">
                {saving ? "جاري الحفظ..." : editId ? "حفظ التعديلات" : "إضافة"}
              </button>
              <button onClick={() => setShowForm(false)} className="flex-1 border py-2 rounded-lg text-sm">إلغاء</button>
            </div>
          </div>
        </div>
      )}

      {loading ? (
        <div className="text-center py-12 text-gray-500">جاري التحميل...</div>
      ) : companies.length === 0 ? (
        <div className="text-center py-12 text-gray-400">لا توجد شركات تأمين مسجلة</div>
      ) : (
        <div className="bg-white border rounded-xl overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50">
              <tr>
                <th className="text-right p-4 text-gray-600">الشركة</th>
                <th className="text-right p-4 text-gray-600">المسؤول</th>
                <th className="text-right p-4 text-gray-600">الهاتف</th>
                <th className="text-right p-4 text-gray-600">نسبة التغطية</th>
                <th className="text-right p-4 text-gray-600">الحالة</th>
                <th className="p-4"></th>
              </tr>
            </thead>
            <tbody>
              {companies.map((c) => (
                <tr key={c.id} className="border-t hover:bg-gray-50">
                  <td className="p-4">
                    <p className="font-medium text-gray-900">{c.nameAr ?? c.name}</p>
                    {c.nameAr && <p className="text-gray-500 text-xs">{c.name}</p>}
                  </td>
                  <td className="p-4 text-gray-600">{c.contactPerson ?? "—"}</td>
                  <td className="p-4 text-gray-600">{c.phone ?? "—"}</td>
                  <td className="p-4 text-gray-800 font-medium">{c.defaultCoveragePercent}%</td>
                  <td className="p-4">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${c.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"}`}>
                      {c.isActive ? "نشطة" : "غير نشطة"}
                    </span>
                  </td>
                  <td className="p-4 text-left">
                    <button onClick={() => openEdit(c)} className="text-xs text-blue-600 hover:text-blue-800 border border-blue-200 px-2 py-1 rounded-lg hover:bg-blue-50">
                      تعديل
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
