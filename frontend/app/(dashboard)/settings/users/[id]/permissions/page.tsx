"use client";

import { useEffect, useMemo, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { api } from "@/lib/api";
import { useAuthStore } from "@/stores/authStore";
import { P } from "@/lib/permissions";

// ── Types ─────────────────────────────────────────────────────────────────────

interface PermissionDto {
  permissionId: string;
  name: string;
  displayName: string;
  module: string;
  screen: string | null;
  grantType: "Grant" | "Deny";
}

interface EffectivePermissionsDto {
  rolePermissions: PermissionDto[];
  additionalGrants: PermissionDto[];
  explicitDenies: PermissionDto[];
  effective: string[];
}

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

// ── Helpers ───────────────────────────────────────────────────────────────────

const MODULE_LABELS: Record<string, string> = {
  Dashboard: "لوحة القيادة", Patients: "المرضى", Appointments: "المواعيد",
  Clinical: "السريرية", Lab: "المختبر", Radiology: "الأشعة",
  Financial: "المالية", Insurance: "التأمين", Inventory: "المخزون",
  Purchasing: "المشتريات", Assets: "الأصول", Reports: "التقارير", IAM: "الإدارة",
};

// ── Page ──────────────────────────────────────────────────────────────────────

export default function UserEffectivePermissionsPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const { hasPermission } = useAuthStore();
  const canEdit = hasPermission(P.IAM.Users.Edit);

  const [data, setData] = useState<EffectivePermissionsDto | null>(null);
  const [allPerms, setAllPerms] = useState<PermissionGroup[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Override state: permissionId → "Grant" | "Deny" | null (null = remove override)
  const [overrides, setOverrides] = useState<Map<string, "Grant" | "Deny">>(new Map());
  const [dirty, setDirty] = useState(false);

  const userId = params.id;

  useEffect(() => {
    async function load() {
      setLoading(true);
      try {
        const [effRes, allRes] = await Promise.all([
          api.get<EffectivePermissionsDto>(`/users/${userId}/effective-permissions`),
          api.get<PermissionGroup[]>("/permissions"),
        ]);
        setData(effRes.data);
        setAllPerms(allRes.data);

        // Seed overrides from current data
        const map = new Map<string, "Grant" | "Deny">();
        effRes.data.additionalGrants.forEach((p) => map.set(p.permissionId, "Grant"));
        effRes.data.explicitDenies.forEach((p) => map.set(p.permissionId, "Deny"));
        setOverrides(map);
        setDirty(false);
      } finally {
        setLoading(false);
      }
    }
    load();
  }, [userId]);

  function setOverride(permId: string, type: "Grant" | "Deny" | null) {
    setOverrides((prev) => {
      const next = new Map(prev);
      if (type === null) next.delete(permId);
      else next.set(permId, type);
      return next;
    });
    setDirty(true);
  }

  async function save() {
    setSaving(true);
    setError(null);
    try {
      const payload = [...overrides.entries()].map(([permissionId, grantType]) => ({
        permissionId,
        grantType,
      }));
      await api.put(`/users/${userId}/permissions`, { overrides: payload });
      // Reload effective
      const effRes = await api.get<EffectivePermissionsDto>(`/users/${userId}/effective-permissions`);
      setData(effRes.data);
      setDirty(false);
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ أثناء الحفظ");
    } finally {
      setSaving(false);
    }
  }

  // Group all permissions by module for the matrix
  const groupedAll = useMemo(() => allPerms, [allPerms]);

  // Map from permId → is in role
  const rolePermIds = useMemo(
    () => new Set(data?.rolePermissions.map((p) => p.permissionId) ?? []),
    [data]
  );

  if (loading) {
    return (
      <div className="p-8 text-center text-gray-400 text-sm">جاري التحميل...</div>
    );
  }

  if (!data) {
    return (
      <div className="p-8 text-center text-red-500 text-sm">تعذر تحميل البيانات</div>
    );
  }

  return (
    <div className="p-6 min-h-screen bg-gray-50" dir="rtl">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <button
            onClick={() => router.back()}
            className="text-sm text-blue-600 hover:underline mb-1"
          >
            ← العودة للمستخدمين
          </button>
          <h1 className="text-xl font-bold text-gray-900">الصلاحيات الفعلية</h1>
          <p className="text-xs text-gray-500 mt-0.5">
            صلاحيات الدور + المنح الإضافية − المحجوبة = الصلاحيات الفعلية
          </p>
        </div>
        {canEdit && dirty && (
          <button
            onClick={save}
            disabled={saving}
            className="bg-blue-600 text-white px-5 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
          >
            {saving ? "جاري الحفظ..." : "حفظ التغييرات"}
          </button>
        )}
      </div>

      {error && (
        <div className="mb-4 text-sm text-red-600 bg-red-50 rounded-lg px-4 py-3 border border-red-200">
          {error}
        </div>
      )}

      {/* Summary cards */}
      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="bg-white rounded-xl shadow-sm p-4 border-r-4 border-blue-500">
          <div className="text-2xl font-bold text-blue-600">{data.rolePermissions.length}</div>
          <div className="text-xs text-gray-500 mt-0.5">صلاحيات الدور</div>
        </div>
        <div className="bg-white rounded-xl shadow-sm p-4 border-r-4 border-green-500">
          <div className="text-2xl font-bold text-green-600">{data.additionalGrants.length}</div>
          <div className="text-xs text-gray-500 mt-0.5">منح إضافية</div>
        </div>
        <div className="bg-white rounded-xl shadow-sm p-4 border-r-4 border-red-500">
          <div className="text-2xl font-bold text-red-600">{data.explicitDenies.length}</div>
          <div className="text-xs text-gray-500 mt-0.5">محجوبة صراحةً</div>
        </div>
      </div>

      {/* Effective permissions list */}
      <div className="bg-white rounded-xl shadow mb-6 overflow-hidden">
        <div className="px-5 py-3 border-b bg-gray-50">
          <span className="font-semibold text-gray-700 text-sm">
            الصلاحيات الفعلية ({data.effective.length})
          </span>
        </div>
        <div className="p-4 flex flex-wrap gap-2">
          {data.effective.length === 0 ? (
            <p className="text-sm text-gray-400">لا توجد صلاحيات فعلية</p>
          ) : (
            data.effective.map((name) => (
              <span key={name} className="text-xs bg-blue-50 text-blue-700 border border-blue-200 px-2 py-1 rounded-lg font-mono">
                {name}
              </span>
            ))
          )}
        </div>
      </div>

      {/* Override matrix */}
      <div className="bg-white rounded-xl shadow overflow-hidden">
        <div className="px-5 py-3 border-b bg-gray-50">
          <span className="font-semibold text-gray-700 text-sm">
            تعديل الصلاحيات الفردية
          </span>
          <p className="text-xs text-gray-400 mt-0.5">
            منح: إضافة صلاحية فوق دور المستخدم &nbsp;·&nbsp; حجب: إزالة صلاحية حتى لو كانت ضمن الدور
          </p>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-xs border-collapse">
            <thead>
              <tr className="bg-gray-50 border-b">
                <th className="text-right px-4 py-2.5 font-semibold text-gray-600 sticky right-0 bg-gray-50 z-10 border-l w-72">
                  الصلاحية
                </th>
                <th className="px-4 py-2.5 font-semibold text-gray-600 text-center w-24">من الدور</th>
                <th className="px-4 py-2.5 font-semibold text-gray-600 text-center w-24">منح</th>
                <th className="px-4 py-2.5 font-semibold text-gray-600 text-center w-24">حجب</th>
                <th className="px-4 py-2.5 font-semibold text-gray-600 text-center w-24">فعّال</th>
              </tr>
            </thead>
            <tbody>
              {groupedAll.map((group) => (
                <>
                  <tr key={`mod-${group.module}`} className="bg-blue-50 border-b border-blue-100">
                    <td colSpan={5} className="px-4 py-2 font-bold text-blue-800 text-[11px] uppercase tracking-wide">
                      {MODULE_LABELS[group.module] ?? group.module}
                    </td>
                  </tr>
                  {group.permissions.map((perm) => {
                    const inRole = rolePermIds.has(perm.id);
                    const override = overrides.get(perm.id) ?? null;
                    const isGranted = override === "Grant";
                    const isDenied = override === "Deny";
                    const isEffective = inRole
                      ? !isDenied
                      : isGranted;

                    return (
                      <tr
                        key={perm.id}
                        className={`border-b border-gray-100 hover:bg-gray-50 ${isEffective ? "" : "opacity-60"}`}
                      >
                        <td className="px-4 py-2 sticky right-0 bg-white hover:bg-gray-50 z-10 border-l border-gray-100">
                          <div className="ps-4">
                            <div className="text-gray-800">{perm.displayName}</div>
                            <div className="text-gray-400 font-mono text-[10px]">{perm.name}</div>
                          </div>
                        </td>
                        <td className="px-4 py-2 text-center">
                          {inRole ? (
                            <span className="inline-block w-4 h-4 bg-blue-500 rounded-full" title="ضمن الدور" />
                          ) : (
                            <span className="text-gray-200">—</span>
                          )}
                        </td>
                        <td className="px-4 py-2 text-center">
                          <input
                            type="checkbox"
                            checked={isGranted}
                            disabled={!canEdit}
                            onChange={(e) => {
                              if (e.target.checked) setOverride(perm.id, "Grant");
                              else setOverride(perm.id, null);
                            }}
                            className="w-3.5 h-3.5 rounded cursor-pointer accent-green-600"
                          />
                        </td>
                        <td className="px-4 py-2 text-center">
                          <input
                            type="checkbox"
                            checked={isDenied}
                            disabled={!canEdit}
                            onChange={(e) => {
                              if (e.target.checked) setOverride(perm.id, "Deny");
                              else setOverride(perm.id, null);
                            }}
                            className="w-3.5 h-3.5 rounded cursor-pointer accent-red-600"
                          />
                        </td>
                        <td className="px-4 py-2 text-center">
                          {isEffective ? (
                            <span className="text-green-600 font-bold">✓</span>
                          ) : (
                            <span className="text-gray-300">✗</span>
                          )}
                        </td>
                      </tr>
                    );
                  })}
                </>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
