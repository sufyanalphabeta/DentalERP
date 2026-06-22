"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { api } from "@/lib/api";

interface Asset {
  id: string;
  name: string;
  assetCode: string | null;
  categoryName: string | null;
  status: string;
  purchaseDate: string | null;
  purchaseCost: number | null;
  currentValue: number | null;
  location: string | null;
  serialNumber: string | null;
  description: string | null;
}

interface Maintenance {
  id: string;
  maintenanceDate: string;
  cost: number | null;
  description: string;
  nextMaintenanceDate: string | null;
}

const statusAr: Record<string, string> = {
  Active: "نشط",
  UnderMaintenance: "تحت الصيانة",
  Disposed: "مستبعد",
  Inactive: "غير نشط",
};

const statusCls: Record<string, string> = {
  Active: "bg-green-100 text-green-700",
  UnderMaintenance: "bg-amber-100 text-amber-700",
  Disposed: "bg-gray-100 text-gray-500",
  Inactive: "bg-red-100 text-red-600",
};

export default function AssetDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [asset, setAsset] = useState<Asset | null>(null);
  const [maintenances, setMaintenances] = useState<Maintenance[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAddMaint, setShowAddMaint] = useState(false);
  const [maintForm, setMaintForm] = useState({ description: "", cost: "", maintenanceDate: new Date().toISOString().split("T")[0], nextMaintenanceDate: "" });
  const [saving, setSaving] = useState(false);

  useEffect(() => { loadAll(); }, [id]);

  async function loadAll() {
    setLoading(true);
    await Promise.allSettled([
      api.get<Asset>(`/assets/${id}`).then((r) => setAsset(r.data)),
      api.get<Maintenance[]>(`/assets/${id}/maintenances`).then((r) => setMaintenances(r.data)).catch(() => setMaintenances([])),
    ]);
    setLoading(false);
  }

  async function addMaintenance() {
    setSaving(true);
    try {
      await api.post(`/assets/${id}/maintenances`, {
        description: maintForm.description,
        cost: maintForm.cost ? parseFloat(maintForm.cost) : null,
        maintenanceDate: maintForm.maintenanceDate,
        nextMaintenanceDate: maintForm.nextMaintenanceDate || null,
      });
      setShowAddMaint(false);
      setMaintForm({ description: "", cost: "", maintenanceDate: new Date().toISOString().split("T")[0], nextMaintenanceDate: "" });
      loadAll();
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
          {asset.assetCode && <p className="text-gray-400 text-sm font-mono">{asset.assetCode}</p>}
        </div>
        <div className="flex gap-2">
          <Link href="/assets" className="border px-3 py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-50">رجوع</Link>
          <button onClick={() => setShowAddMaint(true)} className="bg-amber-600 text-white px-3 py-2 rounded-lg text-sm hover:bg-amber-700">+ صيانة</button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Asset info */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
          <h2 className="font-semibold text-gray-800 mb-4">بيانات الأصل</h2>
          <div className="space-y-3">
            <div><div className="text-xs text-gray-400">الفئة</div><div className="text-sm text-gray-700">{asset.categoryName ?? "—"}</div></div>
            <div><div className="text-xs text-gray-400">الحالة</div>
              <span className={`text-xs px-2 py-0.5 rounded-full inline-block mt-0.5 ${statusCls[asset.status] ?? "bg-gray-100 text-gray-600"}`}>
                {statusAr[asset.status] ?? asset.status}
              </span>
            </div>
            <div><div className="text-xs text-gray-400">الموقع</div><div className="text-sm text-gray-700">{asset.location ?? "—"}</div></div>
            {asset.serialNumber && <div><div className="text-xs text-gray-400">الرقم التسلسلي</div><div className="text-sm font-mono text-gray-700">{asset.serialNumber}</div></div>}
            {asset.purchaseDate && <div><div className="text-xs text-gray-400">تاريخ الشراء</div><div className="text-sm text-gray-700">{new Date(asset.purchaseDate).toLocaleDateString("ar")}</div></div>}
            {asset.purchaseCost != null && <div><div className="text-xs text-gray-400">تكلفة الشراء</div><div className="text-sm font-medium text-gray-800">{asset.purchaseCost.toFixed(2)} د.ل</div></div>}
            {asset.currentValue != null && <div><div className="text-xs text-gray-400">القيمة الحالية</div><div className="text-sm font-medium text-gray-800">{asset.currentValue.toFixed(2)} د.ل</div></div>}
            {asset.description && <div><div className="text-xs text-gray-400">الوصف</div><div className="text-sm text-gray-700">{asset.description}</div></div>}
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
                    <div>
                      <div className="text-sm font-medium text-gray-800">{m.description}</div>
                      {m.nextMaintenanceDate && (
                        <div className="text-xs text-amber-600 mt-0.5">الصيانة القادمة: {new Date(m.nextMaintenanceDate).toLocaleDateString("ar")}</div>
                      )}
                    </div>
                    <div className="text-right">
                      <div className="text-xs text-gray-400">{new Date(m.maintenanceDate).toLocaleDateString("ar")}</div>
                      {m.cost != null && <div className="text-sm font-medium text-gray-700 mt-0.5">{m.cost.toFixed(2)} د.ل</div>}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {showAddMaint && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">إضافة سجل صيانة</h2>
            <div className="space-y-3">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الوصف *</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={maintForm.description} onChange={(e) => setMaintForm({ ...maintForm, description: e.target.value })} />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">تاريخ الصيانة</label>
                  <input type="date" className="w-full border rounded-lg px-3 py-2 text-sm" value={maintForm.maintenanceDate} onChange={(e) => setMaintForm({ ...maintForm, maintenanceDate: e.target.value })} />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">التكلفة</label>
                  <input type="number" step="0.01" className="w-full border rounded-lg px-3 py-2 text-sm" value={maintForm.cost} onChange={(e) => setMaintForm({ ...maintForm, cost: e.target.value })} />
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">تاريخ الصيانة القادمة</label>
                <input type="date" className="w-full border rounded-lg px-3 py-2 text-sm" value={maintForm.nextMaintenanceDate} onChange={(e) => setMaintForm({ ...maintForm, nextMaintenanceDate: e.target.value })} />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={addMaintenance} disabled={saving || !maintForm.description} className="flex-1 bg-amber-600 text-white py-2 rounded-lg text-sm hover:bg-amber-700 disabled:opacity-50">{saving ? "جاري الحفظ..." : "إضافة"}</button>
              <button onClick={() => setShowAddMaint(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
