"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

const DOCTOR_ROLE_ID = "00000000-0000-0000-0000-000000000003";

interface UserSummary {
  id: string;
  username: string;
  fullName: string;
  email: string | null;
  phone: string | null;
  isActive: boolean;
  roles: string[];
}

interface UsersResponse {
  items: UserSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

const emptyForm = { username: "", fullName: "", password: "", email: "", phone: "" };

export default function SettingsDoctorsPage() {
  const [doctors, setDoctors] = useState<UserSummary[]>([]);
  const [all, setAll] = useState<UserSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => { load(); }, []);

  useEffect(() => {
    const q = search.toLowerCase();
    setDoctors(
      q
        ? all.filter((u) => u.fullName.toLowerCase().includes(q) || u.username.toLowerCase().includes(q))
        : all
    );
  }, [search, all]);

  async function load() {
    setLoading(true);
    try {
      const r = await api.get<UsersResponse>("/users?pageSize=200");
      const docs = (r.data.items ?? []).filter((u) => u.roles.includes("Doctor"));
      setAll(docs);
      setDoctors(docs);
    } catch {
      setAll([]);
      setDoctors([]);
    } finally {
      setLoading(false);
    }
  }

  async function create() {
    if (!form.username || !form.fullName || !form.password) {
      setError("الاسم ومعرف المستخدم وكلمة المرور مطلوبة");
      return;
    }
    setSaving(true);
    setError(null);
    try {
      await api.post("/users", {
        username: form.username,
        fullName: form.fullName,
        password: form.password,
        email: form.email || null,
        phone: form.phone || null,
        roleIds: [DOCTOR_ROLE_ID],
      });
      setShowCreate(false);
      setForm(emptyForm);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ أثناء إنشاء الطبيب");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">الأطباء</h1>
        <button onClick={() => { setShowCreate(true); setError(null); setForm(emptyForm); }}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">
          + طبيب جديد
        </button>
      </div>

      <div className="mb-4">
        <input type="text" placeholder="بحث بالاسم..." value={search} onChange={(e) => setSearch(e.target.value)}
          className="border rounded-lg px-3 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-blue-400" />
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الطبيب</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">اسم المستخدم</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الهاتف</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الحالة</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={4} className="text-center py-8 text-gray-400">جاري التحميل...</td></tr>
            ) : doctors.length === 0 ? (
              <tr><td colSpan={4} className="text-center py-8 text-gray-400">لا يوجد أطباء</td></tr>
            ) : doctors.map((doc) => (
              <tr key={doc.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <div className="text-sm font-medium text-gray-800">{doc.fullName}</div>
                  {doc.email && <div className="text-xs text-gray-400">{doc.email}</div>}
                </td>
                <td className="px-4 py-3 text-sm text-gray-600 font-mono">{doc.username}</td>
                <td className="px-4 py-3 text-sm text-gray-600">{doc.phone ?? "—"}</td>
                <td className="px-4 py-3">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${doc.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"}`}>
                    {doc.isActive ? "نشط" : "غير نشط"}
                  </span>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">طبيب جديد</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="grid grid-cols-2 gap-3">
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">الاسم الكامل *</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.fullName} onChange={(e) => setForm({ ...form, fullName: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">اسم المستخدم *</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.username} onChange={(e) => setForm({ ...form, username: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">كلمة المرور *</label>
                <input type="password" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} />
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
              <button onClick={create} disabled={saving || !form.fullName || !form.username || !form.password}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">
                {saving ? "جاري الإنشاء..." : "إنشاء"}
              </button>
              <button onClick={() => setShowCreate(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
