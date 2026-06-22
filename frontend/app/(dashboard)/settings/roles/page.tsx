"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface PermissionItem {
  id: string;
  name: string;
  displayName: string;
}

interface PermissionGroup {
  module: string;
  permissions: PermissionItem[];
}

interface RoleSummary {
  id: string;
  name: string;
  description: string | null;
  isSystem: boolean;
  permissionCount: number;
}

interface RoleDetail {
  id: string;
  name: string;
  description: string | null;
  isSystem: boolean;
  permissions: string[];
}

export default function RolesPage() {
  const [roles, setRoles] = useState<RoleSummary[]>([]);
  const [permGroups, setPermGroups] = useState<PermissionGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<RoleDetail | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [newRoleName, setNewRoleName] = useState("");
  const [newRoleDesc, setNewRoleDesc] = useState("");
  const [newRolePerms, setNewRolePerms] = useState<string[]>([]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => { loadAll(); }, []);

  async function loadAll() {
    setLoading(true);
    try {
      const [rolesRes, permsRes] = await Promise.all([
        api.get<RoleSummary[]>("/roles"),
        api.get<PermissionGroup[]>("/permissions"),
      ]);
      setRoles(rolesRes.data);
      setPermGroups(permsRes.data);
    } finally {
      setLoading(false);
    }
  }

  async function selectRole(id: string) {
    const res = await api.get<RoleDetail>(`/roles/${id}`);
    setSelected(res.data);
  }

  async function createRole() {
    setSaving(true);
    setError(null);
    try {
      await api.post("/roles", {
        name: newRoleName,
        description: newRoleDesc || null,
        permissionIds: newRolePerms,
      });
      setShowCreate(false);
      setNewRoleName("");
      setNewRoleDesc("");
      setNewRolePerms([]);
      loadAll();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  }

  async function deleteRole(id: string) {
    if (!confirm("هل أنت متأكد من حذف هذا الدور؟")) return;
    await api.delete(`/roles/${id}`);
    setSelected(null);
    loadAll();
  }

  const allPermIds = permGroups.flatMap((g) => g.permissions.map((p) => p.id));

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">الأدوار والصلاحيات</h1>
        <button
          onClick={() => { setShowCreate(true); setError(null); }}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
        >
          + دور جديد
        </button>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Roles list */}
        <div className="lg:col-span-1">
          <div className="bg-white rounded-xl shadow overflow-hidden">
            <div className="px-4 py-3 border-b bg-gray-50">
              <h2 className="font-semibold text-gray-700 text-sm">الأدوار ({roles.length})</h2>
            </div>
            {loading ? (
              <div className="p-4 text-center text-gray-400 text-sm">جاري التحميل...</div>
            ) : roles.length === 0 ? (
              <div className="p-4 text-center text-gray-400 text-sm">لا توجد أدوار</div>
            ) : (
              <div className="divide-y">
                {roles.map((role) => (
                  <button
                    key={role.id}
                    onClick={() => selectRole(role.id)}
                    className={`w-full text-right px-4 py-3 hover:bg-blue-50 transition-colors ${selected?.id === role.id ? "bg-blue-50 border-r-2 border-blue-600" : ""}`}
                  >
                    <div className="flex items-center justify-between">
                      <div>
                        <div className="text-sm font-medium text-gray-800">{role.name}</div>
                        {role.description && <div className="text-xs text-gray-400">{role.description}</div>}
                      </div>
                      <div className="flex items-center gap-2">
                        <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded-full">{role.permissionCount} صلاحية</span>
                        {role.isSystem && <span className="text-xs bg-amber-100 text-amber-700 px-2 py-0.5 rounded-full">نظام</span>}
                      </div>
                    </div>
                  </button>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Role detail */}
        <div className="lg:col-span-2">
          {selected ? (
            <div className="bg-white rounded-xl shadow p-5">
              <div className="flex items-center justify-between mb-5">
                <div>
                  <h2 className="text-lg font-bold text-gray-800">{selected.name}</h2>
                  {selected.description && <p className="text-sm text-gray-500">{selected.description}</p>}
                </div>
                {!selected.isSystem && (
                  <button
                    onClick={() => deleteRole(selected.id)}
                    className="text-sm text-red-600 border border-red-300 px-3 py-1 rounded-lg hover:bg-red-50"
                  >
                    حذف الدور
                  </button>
                )}
              </div>
              <div className="space-y-4">
                {permGroups.map((group) => {
                  const granted = group.permissions.filter((p) => selected.permissions.includes(p.name));
                  if (granted.length === 0) return null;
                  return (
                    <div key={group.module}>
                      <h3 className="text-xs font-semibold text-gray-500 uppercase mb-2">{group.module}</h3>
                      <div className="flex flex-wrap gap-2">
                        {granted.map((p) => (
                          <span key={p.id} className="text-xs bg-blue-100 text-blue-700 px-2 py-1 rounded-lg">{p.displayName}</span>
                        ))}
                      </div>
                    </div>
                  );
                })}
                {selected.permissions.length === 0 && (
                  <p className="text-sm text-gray-400">لا توجد صلاحيات مخصصة لهذا الدور</p>
                )}
              </div>
            </div>
          ) : (
            <div className="bg-white rounded-xl shadow p-8 text-center text-gray-400">
              اختر دوراً لعرض صلاحياته
            </div>
          )}
        </div>
      </div>

      {/* Create role modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-lg p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">دور جديد</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">اسم الدور *</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={newRoleName} onChange={(e) => setNewRoleName(e.target.value)} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الوصف</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={newRoleDesc} onChange={(e) => setNewRoleDesc(e.target.value)} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-2">الصلاحيات</label>
                <div className="space-y-3 max-h-64 overflow-y-auto border rounded-lg p-3">
                  {permGroups.map((group) => (
                    <div key={group.module}>
                      <div className="flex items-center gap-2 mb-1">
                        <span className="text-xs font-semibold text-gray-500 uppercase">{group.module}</span>
                      </div>
                      <div className="grid grid-cols-2 gap-1">
                        {group.permissions.map((p) => (
                          <label key={p.id} className="flex items-center gap-2 cursor-pointer">
                            <input
                              type="checkbox"
                              checked={newRolePerms.includes(p.id)}
                              onChange={(e) => setNewRolePerms(
                                e.target.checked ? [...newRolePerms, p.id] : newRolePerms.filter((id) => id !== p.id)
                              )}
                              className="rounded"
                            />
                            <span className="text-xs text-gray-700">{p.displayName}</span>
                          </label>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
                <div className="flex gap-2 mt-2">
                  <button onClick={() => setNewRolePerms(allPermIds)} className="text-xs text-blue-600 hover:underline">تحديد الكل</button>
                  <span className="text-gray-300">|</span>
                  <button onClick={() => setNewRolePerms([])} className="text-xs text-gray-500 hover:underline">إلغاء التحديد</button>
                </div>
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button
                onClick={createRole}
                disabled={saving || !newRoleName}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
              >
                {saving ? "جاري الحفظ..." : "إنشاء"}
              </button>
              <button onClick={() => setShowCreate(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-50">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
