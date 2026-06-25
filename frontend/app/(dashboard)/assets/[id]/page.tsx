"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { api } from "@/lib/api";

interface Asset {
  id: string;
  assetTag: string;
  name: string;
  categoryName: string | null;
  status: string;
  purchaseDate: string | null;
  purchaseCost: number | null;
  location: string | null;
  serialNumber: string | null;
  notes: string | null;
}

interface Maintenance {
  id: string;
  maintenanceDate: string;
  cost: number;
  description: string;
  vendor: string | null;
  nextMaintenanceDate: string | null;
  expenseId: string | null;
}

interface Vault {
  id: string;
  name: string;
  currentBalance: number;
}

const statusAr: Record<string, string> = {
  Active: "نشط",
  UnderMaintenance: "تحت الصيانة",
  Disposed: "مستبعد",
};

const statusCls: Record<string, string> = {
  Active: "bg-green-100 text-green-700",
  UnderMaintenance: "bg-amber-100 text-amber-700",
  Disposed: "bg-gray-100 text-gray-500",
};

const emptyMaint = {
  description: "",
  cost: "",
  maintenanceDate: new Date().toISOString().split("T")[0],
  nextMaintenanceDate: "",
  vendor: "",
  vaultId: "",
};

export default function AssetDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [asset, setAsset] = useState<Asset | null>(null);
  const [maintenances, setMaintenances] = useState<Maintenance[]>([]);
  const [vaults, setVaults] = useState<Vault[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAddMaint, setShowAddMaint] = useState(false);
  const [maintForm, setMaintForm] = useState(emptyMaint);
  const [saving, setSaving] = useState(false);
  const [maintError, setMaintError] = useState<string | null>(null);

  useEffect(() => { loadAll(); }, [id]);

  async function loadAll() {
    setLoading(true);
    await Promise.allSettled([
      api.get<Asset>(`/assets/${id}`).then((r) => setAsset(r.data)),
      api.get<Maintenance[]>(`/assets/${id}/maintenances`).then((r) => setMaintenances(r.data)).catch(() => setMaintenances([])),
    ]);
    setLoading(false);
  }

  async function openMaintModal() {
    setMaintError(null);
    setMaintForm(emptyMaint);
    if (vaults.length === 0) {
      try {
        const r = await api.get<Vault[]>("/treasury/vaults/balances");
        setVaults(r.data);
      } catch {
        setVaults([]);
      }
    }
    setShowAddMaint(true);
  }

  async function addMaintenance() {
    if (!maintForm.description) return;
    setSaving(true);
    setMaintError(null);
    try {
      await api.post(`/assets/${id}/maintenances`, {
        description: maintForm.description,
        cost: maintForm.cost ? parseFloat(maintForm.cost) : 0,
        maintenanceDate: maintForm.maintenanceDate,
        nextMaintenanceDate: maintForm.nextMaintenanceDate || null,
        vendor: maintForm.vendor || null,
        vaultId: maintForm.vaultId || null,
      });
      setShowAddMaint(false);
      setMaintForm(emptyMaint);
      loadAll();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setMaintError(err?.response?.data?.error ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <div className="p-6 text-center text-gray-400">جاري التحميل...</div>;
  if (!asset) return <div className="p-6 text-red-600">الأصل غير موجود</div>;

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-xl font-bold text-gray-900">{asset.name}</h1>
          <p className="text-gray-400 text-sm font-mono">{asset.assetTag}</p>
        </div>
        <div className="flex gap-2">
          <Link href="/assets" className="border px-3 py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-50">رجوع</Link>
          <button onClick={openMaintModal} className="bg-amber-600 text-white px-3 py-2 rounded-lg text-sm hover:bg-amber-700">+ صيانة</button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Asset info */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
          <h2 className="font-semibold text-gray-800 mb-4">بيانات الأصل</h2>
          <div className="space-y-3">
            <div><div className="text-xs text-gray-400">الفئة</div><div className="text-sm text-gray-700">{asset.categoryName ?? "—"}</div></div>
            <div>
              <div className="text-xs text-gray-400">الحالة</div>
              <span className={`text-xs px-2 py-0.5 rounded-full inline-block mt-0.5 ${statusCls[asset.status] ?? "bg-gray-100 text-gray-600"}`}>
                {statusAr[asset.status] ?? asset.status}
              </span>
            </div>
            <div><div className="text-xs text-gray-400">الموقع</div><div className="text-sm text-gray-700">{asset.location ?? "—"}</div></div>
            {asset.serialNumber && (
              <div><div className="text-xs text-gray-400">الرقم التسلسلي</div><div className="text-sm font-mono text-gray-700">{asset.serialNumber}</div></div>
            )}
            {asset.purchaseDate && (
              <div><div className="text-xs text-gray-400">تاريخ الشراء</div><div className="text-sm text-gray-700">{new Date(asset.purchaseDate).toLocaleDateString("ar-LY")}</div></div>
            )}
            {asset.purchaseCost != null && (
              <div><div className="text-xs text-gray-400">تكلفة الشراء</div><div className="text-sm font-medium text-gray-800">{asset.purchaseCost.toFixed(2)} د.ل</div></div>
            )}
            {asset.notes && (
              <div><div className="text-xs text-gray-400">ملاحظات</div><div className="text-sm text-gray-700">{asset.notes}</div></div>
            )}
          </div>
        </div>

        {/* Maintenance log */}
        <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-gray-100">
          <div className="px-5 py-4 border-b flex items-center justify-between">
            <h2 className="font-semibold text-gray-800">سجل الصيانة</h2>
            <span className="text-xs text-gray-400">{maintenances.length} سجل</span>
          </div>
          {maintenances.length === 0 ? (
            <div className="p-8 text-center text-gray-400">لا توجد سجلات صيانة</div>
          ) : (
            <div className="divide-y max-h-96 overflow-y-auto">
              {maintenances.map((m) => (
                <div key={m.id} className="px-5 py-4">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="text-sm font-medium text-gray-800">{m.description}</div>
                      {m.vendor && <div className="text-xs text-gray-500 mt-0.5">المورد: {m.vendor}</div>}
                      {m.nextMaintenanceDate && (
                        <div className="text-xs text-amber-600 mt-0.5">
                          الصيانة القادمة:{" "}
                          {new Date(m.nextMaintenanceDate).toLocaleDateString("ar-LY", { year: "numeric", month: "long", day: "numeric" })}
                        </div>
                      )}
                      {m.expenseId && (
                        <div className="text-xs text-green-600 mt-0.5">تم تسجيل المصروف</div>
                      )}
                    </div>
                    <div className="text-right ml-4">
                      <div className="text-xs text-gray-400">
                        {new Date(m.maintenanceDate).toLocaleDateString("ar-LY", { year: "numeric", month: "short", day: "numeric" })}
                      </div>
                      {m.cost > 0 && (
                        <div className="text-sm font-medium text-gray-700 mt-0.5">{m.cost.toFixed(2)} د.ل</div>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Add Maintenance modal */}
      {showAddMaint && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 overflow-y-auto py-6">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md p-6 mx-4" dir="rtl">
            <h2 className="text-lg font-bold mb-4">إضافة سجل صيانة</h2>
            {maintError && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{maintError}</div>}
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الوصف *</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={maintForm.description}
                  onChange={(e) => setMaintForm({ ...maintForm, description: e.target.value })}
                />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">تاريخ الصيانة</label>
                  <input
                    type="date"
                    className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={maintForm.maintenanceDate}
                    onChange={(e) => setMaintForm({ ...maintForm, maintenanceDate: e.target.value })}
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">التكلفة (د.ل)</label>
                  <input
                    type="number"
                    step="0.01"
                    min="0"
                    className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={maintForm.cost}
                    onChange={(e) => setMaintForm({ ...maintForm, cost: e.target.value })}
                  />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">تاريخ الصيانة القادمة</label>
                <input
                  type="date"
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={maintForm.nextMaintenanceDate}
                  onChange={(e) => setMaintForm({ ...maintForm, nextMaintenanceDate: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">المورد / الشركة</label>
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={maintForm.vendor}
                  onChange={(e) => setMaintForm({ ...maintForm, vendor: e.target.value })}
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الخزينة (صرف التكلفة)</label>
                <select
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={maintForm.vaultId}
                  onChange={(e) => setMaintForm({ ...maintForm, vaultId: e.target.value })}
                >
                  <option value="">— لا تسجيل مصروف —</option>
                  {vaults.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.name} — {v.currentBalance.toFixed(2)} د.ل
                    </option>
                  ))}
                </select>
                {maintForm.vaultId && (
                  <p className="text-xs text-amber-600 mt-1">سيتم خصم التكلفة من الخزينة المحددة تلقائياً</p>
                )}
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button
                onClick={addMaintenance}
                disabled={saving || !maintForm.description}
                className="flex-1 bg-amber-600 text-white py-2 rounded-lg text-sm hover:bg-amber-700 disabled:opacity-50"
              >
                {saving ? "جاري الحفظ..." : "إضافة"}
              </button>
              <button
                onClick={() => setShowAddMaint(false)}
                className="flex-1 border py-2 rounded-lg text-sm text-gray-700"
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
