"use client";

import { useEffect, useMemo, useState } from "react";
import { api } from "@/lib/api";
import { useAuthStore } from "@/stores/authStore";
import { P } from "@/lib/permissions";

// ── Types ────────────────────────────────────────────────────────────────────

interface PermissionItem {
  id: string;
  name: string;
  displayName: string;
  screen: string | null;
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
  permissions: Array<{ id: string; name: string; displayName: string; module: string; screen: string | null }>;
}

// ── Matrix columns ────────────────────────────────────────────────────────────

const ACTIONS = [
  "View", "Create", "Edit", "Delete",
  "Print", "ExportPdf", "ExportExcel",
  "Approve", "Cancel", "Transfer", "Upload", "Confirm",
] as const;
type Action = typeof ACTIONS[number];

const ACTION_LABELS: Record<Action, string> = {
  View: "عرض", Create: "إنشاء", Edit: "تعديل", Delete: "حذف",
  Print: "طباعة", ExportPdf: "PDF", ExportExcel: "Excel",
  Approve: "اعتماد", Cancel: "إلغاء", Transfer: "تحويل",
  Upload: "رفع", Confirm: "تأكيد",
};

// ── Screen row key ────────────────────────────────────────────────────────────

function rowKey(module: string, screen: string | null): string {
  return screen ? `${module}.${screen}` : module;
}

function rowLabel(module: string, screen: string | null): string {
  return screen ?? module;
}

// ── Module display names ──────────────────────────────────────────────────────

const MODULE_LABELS: Record<string, string> = {
  Dashboard: "لوحة القيادة", Patients: "المرضى", Appointments: "المواعيد",
  Clinical: "السريرية", Lab: "المختبر", Radiology: "الأشعة",
  Financial: "المالية", Insurance: "التأمين", Inventory: "المخزون",
  Purchasing: "المشتريات", Assets: "الأصول", Reports: "التقارير", IAM: "الإدارة",
};

// ── Helpers ───────────────────────────────────────────────────────────────────

function buildMatrix(permGroups: PermissionGroup[]) {
  // Returns: moduleRows → { module, rows: [{ screen, perms: Map<action, permId> }] }
  const modules: Array<{
    module: string;
    rows: Array<{ screen: string | null; perms: Map<Action, string> }>;
  }> = [];

  for (const group of permGroups) {
    const screenMap = new Map<string | null, Map<Action, string>>();
    for (const p of group.permissions) {
      const parts = p.name.split(".");
      const actionRaw = parts[parts.length - 1];
      const action = ACTIONS.find((a) => a.toLowerCase() === actionRaw.toLowerCase());
      if (!action) continue;
      const key = p.screen ?? null;
      if (!screenMap.has(key)) screenMap.set(key, new Map());
      screenMap.get(key)!.set(action, p.id);
    }
    const rows = [...screenMap.entries()].map(([screen, perms]) => ({ screen, perms }));
    rows.sort((a, b) => (a.screen ?? "").localeCompare(b.screen ?? ""));
    modules.push({ module: group.module, rows });
  }

  return modules;
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function RolesPage() {
  const { hasPermission } = useAuthStore();
  const canEdit = hasPermission(P.IAM.Roles.Edit);

  const [roles, setRoles] = useState<RoleSummary[]>([]);
  const [permGroups, setPermGroups] = useState<PermissionGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<RoleDetail | null>(null);
  const [checkedIds, setCheckedIds] = useState<Set<string>>(new Set());
  const [dirty, setDirty] = useState(false);
  const [saving, setSaving] = useState(false);
  const [showCreate, setShowCreate] = useState(false);
  const [newName, setNewName] = useState("");
  const [newDesc, setNewDesc] = useState("");
  const [creating, setCreating] = useState(false);
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
    const role = res.data;
    setSelected(role);
    setCheckedIds(new Set(role.permissions.map((p) => p.id)));
    setDirty(false);
  }

  function toggle(id: string) {
    if (!canEdit || selected?.isSystem) return;
    setCheckedIds((prev) => {
      const next = new Set(prev);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
    setDirty(true);
  }

  // Select/deselect all in a row (screen)
  function toggleRow(rowPermIds: string[]) {
    if (!canEdit || selected?.isSystem) return;
    const allChecked = rowPermIds.every((id) => checkedIds.has(id));
    setCheckedIds((prev) => {
      const next = new Set(prev);
      rowPermIds.forEach((id) => allChecked ? next.delete(id) : next.add(id));
      return next;
    });
    setDirty(true);
  }

  // Select/deselect all in a module
  function toggleModule(modulePermIds: string[]) {
    if (!canEdit || selected?.isSystem) return;
    const allChecked = modulePermIds.every((id) => checkedIds.has(id));
    setCheckedIds((prev) => {
      const next = new Set(prev);
      modulePermIds.forEach((id) => allChecked ? next.delete(id) : next.add(id));
      return next;
    });
    setDirty(true);
  }

  // Select/deselect all permissions for an action column
  function toggleColumn(action: Action) {
    if (!canEdit || selected?.isSystem) return;
    const colIds = permGroups.flatMap((g) =>
      g.permissions
        .filter((p) => p.name.endsWith(`.${action}`))
        .map((p) => p.id)
    );
    const allChecked = colIds.every((id) => checkedIds.has(id));
    setCheckedIds((prev) => {
      const next = new Set(prev);
      colIds.forEach((id) => allChecked ? next.delete(id) : next.add(id));
      return next;
    });
    setDirty(true);
  }

  async function save() {
    if (!selected || !dirty) return;
    setSaving(true);
    try {
      await api.put(`/roles/${selected.id}`, {
        name: selected.name,
        description: selected.description,
        permissionIds: [...checkedIds],
      });
      setDirty(false);
      loadAll();
    } finally {
      setSaving(false);
    }
  }

  async function createRole() {
    if (!newName.trim()) return;
    setCreating(true);
    setError(null);
    try {
      await api.post("/roles", { name: newName.trim(), description: newDesc || null, permissionIds: [] });
      setShowCreate(false);
      setNewName(""); setNewDesc("");
      await loadAll();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ");
    } finally {
      setCreating(false);
    }
  }

  async function deleteRole(id: string) {
    if (!confirm("هل أنت متأكد من حذف هذا الدور؟")) return;
    await api.delete(`/roles/${id}`);
    setSelected(null);
    loadAll();
  }

  const matrix = useMemo(() => buildMatrix(permGroups), [permGroups]);

  // Which actions actually appear in the data (to hide empty columns)
  const usedActions = useMemo<Action[]>(() => {
    const used = new Set<Action>();
    permGroups.forEach((g) =>
      g.permissions.forEach((p) => {
        const a = p.name.split(".").at(-1);
        const found = ACTIONS.find((x) => x.toLowerCase() === a?.toLowerCase());
        if (found) used.add(found);
      })
    );
    return ACTIONS.filter((a) => used.has(a));
  }, [permGroups]);

  return (
    <div className="p-6 min-h-screen bg-gray-50" dir="rtl">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">الأدوار والصلاحيات</h1>
        {hasPermission(P.IAM.Roles.Create) && (
          <button
            onClick={() => { setShowCreate(true); setError(null); }}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700"
          >
            + دور جديد
          </button>
        )}
      </div>

      <div className="flex gap-5" style={{ alignItems: "flex-start" }}>
        {/* Roles list */}
        <div className="w-64 shrink-0 bg-white rounded-xl shadow overflow-hidden">
          <div className="px-4 py-3 border-b bg-gray-50">
            <span className="font-semibold text-gray-700 text-sm">الأدوار ({roles.length})</span>
          </div>
          {loading ? (
            <div className="p-4 text-center text-gray-400 text-sm">جاري التحميل...</div>
          ) : (
            <div className="divide-y max-h-[calc(100vh-12rem)] overflow-y-auto">
              {roles.map((role) => (
                <button
                  key={role.id}
                  onClick={() => selectRole(role.id)}
                  className={`w-full text-right px-4 py-3 hover:bg-blue-50 transition-colors ${selected?.id === role.id ? "bg-blue-50 border-r-2 border-blue-600" : ""}`}
                >
                  <div className="flex items-start justify-between gap-2">
                    <div className="min-w-0">
                      <div className="text-sm font-medium text-gray-800 truncate">{role.name}</div>
                      <div className="text-xs text-gray-400">{role.permissionCount} صلاحية</div>
                    </div>
                    {role.isSystem && (
                      <span className="text-[10px] bg-amber-100 text-amber-700 px-1.5 py-0.5 rounded shrink-0">نظام</span>
                    )}
                  </div>
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Matrix panel */}
        <div className="flex-1 min-w-0">
          {selected ? (
            <div className="bg-white rounded-xl shadow overflow-hidden">
              {/* Role header */}
              <div className="px-5 py-4 border-b flex items-center justify-between">
                <div>
                  <h2 className="text-base font-bold text-gray-800">{selected.name}</h2>
                  {selected.description && <p className="text-xs text-gray-500 mt-0.5">{selected.description}</p>}
                  {selected.isSystem && (
                    <span className="inline-block mt-1 text-[11px] bg-amber-100 text-amber-700 px-2 py-0.5 rounded">
                      دور نظام — القراءة فقط
                    </span>
                  )}
                </div>
                <div className="flex items-center gap-3">
                  {canEdit && dirty && !selected.isSystem && (
                    <button
                      onClick={save}
                      disabled={saving}
                      className="bg-blue-600 text-white px-4 py-1.5 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
                    >
                      {saving ? "جاري الحفظ..." : "حفظ التغييرات"}
                    </button>
                  )}
                  {canEdit && hasPermission(P.IAM.Roles.Delete) && !selected.isSystem && (
                    <button
                      onClick={() => deleteRole(selected.id)}
                      className="text-red-600 border border-red-300 px-3 py-1.5 rounded-lg text-sm hover:bg-red-50"
                    >
                      حذف
                    </button>
                  )}
                </div>
              </div>

              {/* Matrix */}
              <div className="overflow-x-auto">
                <table className="w-full text-xs border-collapse">
                  <thead>
                    <tr className="bg-gray-50 border-b">
                      <th className="text-right px-4 py-2.5 font-semibold text-gray-600 w-44 sticky right-0 bg-gray-50 z-10 border-l">
                        الوحدة / الشاشة
                      </th>
                      {usedActions.map((action) => (
                        <th key={action} className="px-2 py-2.5 font-semibold text-gray-600 text-center min-w-[52px]">
                          <button
                            title={`تحديد/إلغاء عمود ${action}`}
                            onClick={() => toggleColumn(action)}
                            className="hover:text-blue-600 transition-colors"
                            disabled={!canEdit || selected.isSystem}
                          >
                            {ACTION_LABELS[action]}
                          </button>
                        </th>
                      ))}
                    </tr>
                  </thead>
                  <tbody>
                    {matrix.map(({ module, rows }) => {
                      const modulePermIds = rows.flatMap((r) => [...r.perms.values()]);
                      const moduleAllChecked = modulePermIds.length > 0 && modulePermIds.every((id) => checkedIds.has(id));
                      const moduleSomeChecked = modulePermIds.some((id) => checkedIds.has(id));

                      return (
                        <>
                          {/* Module header row */}
                          <tr key={`mod-${module}`} className="bg-blue-50 border-b border-blue-100">
                            <td className="px-4 py-2 sticky right-0 bg-blue-50 z-10 border-l border-blue-100">
                              <div className="flex items-center gap-2">
                                <input
                                  type="checkbox"
                                  checked={moduleAllChecked}
                                  ref={(el) => { if (el) el.indeterminate = !moduleAllChecked && moduleSomeChecked; }}
                                  onChange={() => toggleModule(modulePermIds)}
                                  disabled={!canEdit || selected.isSystem}
                                  className="rounded w-3.5 h-3.5"
                                />
                                <span className="font-bold text-blue-800 text-[11px] uppercase tracking-wide">
                                  {MODULE_LABELS[module] ?? module}
                                </span>
                              </div>
                            </td>
                            {usedActions.map((action) => (
                              <td key={action} className="px-2 py-2 text-center" />
                            ))}
                          </tr>

                          {/* Screen rows */}
                          {rows.map(({ screen, perms }) => {
                            const rowPermIds = [...perms.values()];
                            const rowAllChecked = rowPermIds.length > 0 && rowPermIds.every((id) => checkedIds.has(id));
                            const rowSomeChecked = rowPermIds.some((id) => checkedIds.has(id));

                            return (
                              <tr key={rowKey(module, screen)} className="border-b border-gray-100 hover:bg-gray-50">
                                <td className="px-4 py-2 sticky right-0 bg-white hover:bg-gray-50 z-10 border-l border-gray-100">
                                  <div className="flex items-center gap-2 ps-4">
                                    <input
                                      type="checkbox"
                                      checked={rowAllChecked}
                                      ref={(el) => { if (el) el.indeterminate = !rowAllChecked && rowSomeChecked; }}
                                      onChange={() => toggleRow(rowPermIds)}
                                      disabled={!canEdit || selected.isSystem}
                                      className="rounded w-3.5 h-3.5"
                                    />
                                    <span className="text-gray-700">{rowLabel(module, screen)}</span>
                                  </div>
                                </td>
                                {usedActions.map((action) => {
                                  const permId = perms.get(action);
                                  return (
                                    <td key={action} className="px-2 py-2 text-center">
                                      {permId ? (
                                        <input
                                          type="checkbox"
                                          checked={checkedIds.has(permId)}
                                          onChange={() => toggle(permId)}
                                          disabled={!canEdit || selected.isSystem}
                                          className="rounded w-3.5 h-3.5 cursor-pointer"
                                        />
                                      ) : (
                                        <span className="text-gray-200">—</span>
                                      )}
                                    </td>
                                  );
                                })}
                              </tr>
                            );
                          })}
                        </>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>
          ) : (
            <div className="bg-white rounded-xl shadow p-12 text-center text-gray-400">
              اختر دوراً من القائمة لعرض وتعديل صلاحياته
            </div>
          )}
        </div>
      </div>

      {/* Create role modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">دور جديد</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">اسم الدور *</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                  placeholder="مثال: مدير الفرع"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الوصف</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  value={newDesc}
                  onChange={(e) => setNewDesc(e.target.value)}
                  placeholder="اختياري"
                />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button
                onClick={createRole}
                disabled={creating || !newName.trim()}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
              >
                {creating ? "جاري الإنشاء..." : "إنشاء"}
              </button>
              <button
                onClick={() => setShowCreate(false)}
                className="flex-1 border py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-50"
              >
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
