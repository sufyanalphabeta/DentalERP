"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface Role {
  id: string;
  name: string;
}

interface UserSummary {
  id: string;
  username: string;
  fullName: string;
  email: string | null;
  phone: string | null;
  isActive: boolean;
  lastLoginAt: string | null;
  roles: string[];
}

interface UsersResponse {
  items: UserSummary[];
  totalCount: number;
  page: number;
  pageSize: number;
}

const emptyCreate = {
  username: "",
  fullName: "",
  email: "",
  phone: "",
  roleIds: [] as string[],
};

export default function UsersPage() {
  const [data, setData] = useState<UsersResponse | null>(null);
  const [roles, setRoles] = useState<Role[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);

  // Create
  const [showCreate, setShowCreate] = useState(false);
  const [createForm, setCreateForm] = useState(emptyCreate);
  const [createSaving, setCreateSaving] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  // Edit
  const [editUser, setEditUser] = useState<UserSummary | null>(null);
  const [editForm, setEditForm] = useState({ fullName: "", email: "", phone: "", roleIds: [] as string[] });
  const [editSaving, setEditSaving] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);

  useEffect(() => { loadAll(); }, [search, page]);

  async function loadAll() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: "20" });
      if (search) params.set("search", search);
      const [usersRes, rolesRes] = await Promise.all([
        api.get<UsersResponse>(`/users?${params}`),
        api.get<Role[]>("/roles"),
      ]);
      setData(usersRes.data);
      setRoles(rolesRes.data);
    } finally {
      setLoading(false);
    }
  }

  async function createUser() {
    setCreateSaving(true);
    setCreateError(null);
    try {
      await api.post("/users", {
        username: createForm.username,
        fullName: createForm.fullName,
        email: createForm.email || null,
        phone: createForm.phone || null,
        roleIds: createForm.roleIds,
      });
      setShowCreate(false);
      setCreateForm(emptyCreate);
      loadAll();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setCreateError(err?.response?.data?.error ?? "حدث خطأ أثناء الإنشاء");
    } finally {
      setCreateSaving(false);
    }
  }

  function openEdit(u: UserSummary) {
    setEditUser(u);
    setEditError(null);
    // Map role names back to IDs
    const currentRoleIds = roles
      .filter((r) => u.roles.includes(r.name))
      .map((r) => r.id);
    setEditForm({ fullName: u.fullName, email: u.email ?? "", phone: u.phone ?? "", roleIds: currentRoleIds });
  }

  async function saveEdit() {
    if (!editUser) return;
    setEditSaving(true);
    setEditError(null);
    try {
      await api.put(`/users/${editUser.id}`, {
        fullName: editForm.fullName,
        email: editForm.email || null,
        phone: editForm.phone || null,
        roleIds: editForm.roleIds,
      });
      setEditUser(null);
      loadAll();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setEditError(err?.response?.data?.error ?? "حدث خطأ أثناء الحفظ");
    } finally {
      setEditSaving(false);
    }
  }

  async function toggleUser(id: string, isActive: boolean) {
    await api.patch(`/users/${id}/toggle`, { isActive: !isActive });
    loadAll();
  }

  async function resetPassword(id: string, name: string) {
    if (!confirm(`إعادة تعيين كلمة مرور "${name}" إلى 123456؟ سيُطلب منه تغييرها عند أول دخول.`)) return;
    await api.post(`/users/${id}/reset-password`);
    alert("تم إعادة تعيين كلمة المرور إلى 123456 بنجاح");
  }

  function fmtDate(iso: string | null) {
    if (!iso) return "لم يسجل بعد";
    const d = new Date(iso);
    if (isNaN(d.getTime())) return "—";
    return d.toLocaleDateString("ar-LY", { year: "numeric", month: "short", day: "numeric" });
  }

  const totalPages = data ? Math.ceil(data.totalCount / 20) : 1;

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">إدارة المستخدمين</h1>
        <button
          onClick={() => { setShowCreate(true); setCreateError(null); setCreateForm(emptyCreate); }}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
        >
          + مستخدم جديد
        </button>
      </div>

      <div className="mb-4">
        <input
          type="text"
          placeholder="بحث بالاسم أو اسم المستخدم..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          className="w-full max-w-md border border-gray-300 rounded-lg px-4 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المستخدم</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">اسم الدخول</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الأدوار</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">آخر دخول</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الحالة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الإجراءات</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">جاري التحميل...</td></tr>
            ) : (data?.items ?? []).length === 0 ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">لا يوجد مستخدمون</td></tr>
            ) : (data?.items ?? []).map((u) => (
              <tr key={u.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <div className="text-sm font-medium text-gray-800">{u.fullName}</div>
                  {u.email && <div className="text-xs text-gray-400">{u.email}</div>}
                  {u.phone && <div className="text-xs text-gray-400">{u.phone}</div>}
                </td>
                <td className="px-4 py-3 text-sm font-mono text-gray-600">{u.username}</td>
                <td className="px-4 py-3">
                  <div className="flex flex-wrap gap-1">
                    {u.roles.map((r) => (
                      <span key={r} className="text-xs bg-blue-100 text-blue-700 px-2 py-0.5 rounded-full">{r}</span>
                    ))}
                  </div>
                </td>
                <td className="px-4 py-3 text-xs text-gray-500">{fmtDate(u.lastLoginAt)}</td>
                <td className="px-4 py-3">
                  <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${u.isActive ? "bg-green-100 text-green-700" : "bg-red-100 text-red-600"}`}>
                    {u.isActive ? "نشط" : "موقوف"}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <div className="flex items-center gap-2 flex-wrap">
                    <button
                      onClick={() => openEdit(u)}
                      className="text-xs px-3 py-1 rounded-lg border border-gray-300 text-gray-600 hover:bg-gray-50"
                    >
                      تعديل
                    </button>
                    <button
                      onClick={() => resetPassword(u.id, u.fullName)}
                      className="text-xs px-3 py-1 rounded-lg border border-amber-300 text-amber-700 hover:bg-amber-50"
                      title="إعادة تعيين كلمة المرور إلى 123456"
                    >
                      🔑 إعادة كلمة المرور
                    </button>
                    <button
                      onClick={() => toggleUser(u.id, u.isActive)}
                      className={`text-xs px-3 py-1 rounded-lg border ${u.isActive ? "border-red-300 text-red-600 hover:bg-red-50" : "border-green-300 text-green-600 hover:bg-green-50"}`}
                    >
                      {u.isActive ? "إيقاف" : "تفعيل"}
                    </button>
                  </div>
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

      {/* Create Modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">مستخدم جديد</h2>
            {createError && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{createError}</div>}
            <div className="bg-amber-50 border border-amber-200 rounded-lg px-3 py-2 mb-3 text-xs text-amber-800">
              🔑 سيتم تعيين رقم التعريف الافتراضي <strong>123456</strong> — سيُطلب من المستخدم تغييره عند أول دخول
            </div>
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الاسم الكامل *</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={createForm.fullName} onChange={(e) => setCreateForm({ ...createForm, fullName: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">اسم الدخول *</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm font-mono" value={createForm.username} onChange={(e) => setCreateForm({ ...createForm, username: e.target.value })} />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">البريد الإلكتروني</label>
                  <input type="email" className="w-full border rounded-lg px-3 py-2 text-sm" value={createForm.email} onChange={(e) => setCreateForm({ ...createForm, email: e.target.value })} />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">الهاتف</label>
                  <input className="w-full border rounded-lg px-3 py-2 text-sm" value={createForm.phone} onChange={(e) => setCreateForm({ ...createForm, phone: e.target.value })} />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الأدوار</label>
                <div className="border rounded-lg p-3 space-y-2 max-h-40 overflow-y-auto">
                  {roles.map((r) => (
                    <label key={r.id} className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={createForm.roleIds.includes(r.id)}
                        onChange={(e) => setCreateForm({
                          ...createForm,
                          roleIds: e.target.checked
                            ? [...createForm.roleIds, r.id]
                            : createForm.roleIds.filter((id) => id !== r.id),
                        })}
                        className="rounded"
                      />
                      <span className="text-sm text-gray-700">{r.name}</span>
                    </label>
                  ))}
                  {roles.length === 0 && <p className="text-xs text-gray-400">لا توجد أدوار</p>}
                </div>
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button
                onClick={createUser}
                disabled={createSaving || !createForm.username || !createForm.fullName}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
              >
                {createSaving ? "جاري الحفظ..." : "إنشاء"}
              </button>
              <button onClick={() => setShowCreate(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-50">إلغاء</button>
            </div>
          </div>
        </div>
      )}

      {/* Edit Modal */}
      {editUser && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-1">تعديل المستخدم</h2>
            <p className="text-sm text-gray-500 mb-4 font-mono">{editUser.username}</p>
            {editError && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{editError}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الاسم الكامل *</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={editForm.fullName}
                  onChange={(e) => setEditForm({ ...editForm, fullName: e.target.value })}
                />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">البريد الإلكتروني</label>
                  <input
                    type="email"
                    className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={editForm.email}
                    onChange={(e) => setEditForm({ ...editForm, email: e.target.value })}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">الهاتف</label>
                  <input
                    className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={editForm.phone}
                    onChange={(e) => setEditForm({ ...editForm, phone: e.target.value })}
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الأدوار</label>
                <div className="border rounded-lg p-3 space-y-2 max-h-40 overflow-y-auto">
                  {roles.map((r) => (
                    <label key={r.id} className="flex items-center gap-2 cursor-pointer">
                      <input
                        type="checkbox"
                        checked={editForm.roleIds.includes(r.id)}
                        onChange={(e) => setEditForm({
                          ...editForm,
                          roleIds: e.target.checked
                            ? [...editForm.roleIds, r.id]
                            : editForm.roleIds.filter((id) => id !== r.id),
                        })}
                        className="rounded"
                      />
                      <span className="text-sm text-gray-700">{r.name}</span>
                    </label>
                  ))}
                  {roles.length === 0 && <p className="text-xs text-gray-400">لا توجد أدوار</p>}
                </div>
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button
                onClick={saveEdit}
                disabled={editSaving || !editForm.fullName}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
              >
                {editSaving ? "جاري الحفظ..." : "حفظ التعديلات"}
              </button>
              <button onClick={() => setEditUser(null)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-50">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
