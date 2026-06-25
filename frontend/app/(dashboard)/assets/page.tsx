"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";

interface AssetCategory {
  id: string;
  name: string;
}

interface Asset {
  id: string;
  assetTag: string;
  name: string;
  categoryName: string | null;
  status: string;
  purchaseCost: number | null;
  location: string | null;
  serialNumber: string | null;
}

interface AssetsResponse {
  items: Asset[];
  totalCount: number;
  page: number;
  pageSize: number;
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

const emptyForm = {
  name: "",
  categoryId: "",
  purchaseDate: "",
  purchaseCost: "",
  location: "",
  serialNumber: "",
  notes: "",
};

export default function AssetsPage() {
  const [data, setData] = useState<AssetsResponse | null>(null);
  const [categories, setCategories] = useState<AssetCategory[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [catFilter, setCatFilter] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [page, setPage] = useState(1);
  const [showCreate, setShowCreate] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.get<AssetCategory[]>("/assets/categories").then((r) => setCategories(r.data)).catch(() => {});
  }, []);

  useEffect(() => { load(); }, [search, catFilter, statusFilter, page]);

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: "20" });
      if (search) params.set("search", search);
      if (catFilter) params.set("categoryId", catFilter);
      if (statusFilter) params.set("status", statusFilter);
      const r = await api.get<AssetsResponse>(`/assets?${params}`);
      setData(r.data);
    } finally {
      setLoading(false);
    }
  }

  async function create() {
    setSaving(true);
    setError(null);
    try {
      await api.post("/assets", {
        name: form.name,
        categoryId: form.categoryId || null,
        purchaseDate: form.purchaseDate || null,
        purchaseCost: form.purchaseCost ? parseFloat(form.purchaseCost) : null,
        location: form.location || null,
        serialNumber: form.serialNumber || null,
        notes: form.notes || null,
      });
      setShowCreate(false);
      setForm(emptyForm);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  }

  const totalPages = data ? Math.ceil(data.totalCount / 20) : 1;

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">سجل الأصول الثابتة</h1>
        <div className="flex gap-2">
          <Link href="/assets/categories" className="border px-3 py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-50">الفئات</Link>
          <button onClick={() => { setShowCreate(true); setError(null); setForm(emptyForm); }}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">
            + أصل جديد
          </button>
        </div>
      </div>

      <div className="flex flex-wrap gap-3 mb-4">
        <input type="text" placeholder="بحث..." value={search} onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          className="border rounded-lg px-3 py-2 text-sm w-52 focus:outline-none focus:ring-2 focus:ring-blue-400" />
        <select value={catFilter} onChange={(e) => { setCatFilter(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm">
          <option value="">كل الفئات</option>
          {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm">
          <option value="">كل الحالات</option>
          {Object.entries(statusAr).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
        </select>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الأصل</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الفئة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الموقع</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">تكلفة الشراء</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الحالة</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">جاري التحميل...</td></tr>
            ) : (data?.items ?? []).length === 0 ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">لا توجد أصول</td></tr>
            ) : (data?.items ?? []).map((asset) => (
              <tr key={asset.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <div className="text-sm font-medium text-gray-800">{asset.name}</div>
                  <div className="text-xs text-gray-400 font-mono">{asset.assetTag}</div>
                  {asset.serialNumber && <div className="text-xs text-gray-400">S/N: {asset.serialNumber}</div>}
                </td>
                <td className="px-4 py-3 text-sm text-gray-600">{asset.categoryName ?? "—"}</td>
                <td className="px-4 py-3 text-sm text-gray-600">{asset.location ?? "—"}</td>
                <td className="px-4 py-3 text-sm text-gray-700">{asset.purchaseCost != null ? `${asset.purchaseCost.toFixed(2)} د.ل` : "—"}</td>
                <td className="px-4 py-3">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${statusCls[asset.status] ?? "bg-gray-100 text-gray-600"}`}>
                    {statusAr[asset.status] ?? asset.status}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <Link href={`/assets/${asset.id}`} className="text-xs text-blue-600 hover:underline">تفاصيل</Link>
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

      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 py-6 overflow-y-auto">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-lg p-6 mx-4" dir="rtl">
            <h2 className="text-lg font-bold mb-4">أصل جديد</h2>
            {error && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{error}</div>}
            <div className="grid grid-cols-2 gap-3">
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">اسم الأصل *</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الرقم التسلسلي</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.serialNumber} onChange={(e) => setForm({ ...form, serialNumber: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الفئة</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: e.target.value })}>
                  <option value="">— بدون فئة —</option>
                  {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">تاريخ الشراء</label>
                <input type="date" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.purchaseDate} onChange={(e) => setForm({ ...form, purchaseDate: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">تكلفة الشراء (د.ل)</label>
                <input type="number" step="0.01" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.purchaseCost} onChange={(e) => setForm({ ...form, purchaseCost: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الموقع</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.location} onChange={(e) => setForm({ ...form, location: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
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
