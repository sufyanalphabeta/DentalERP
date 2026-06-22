"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface Category {
  id: string;
  name: string;
}

interface Unit {
  id: string;
  name: string;
  abbreviation: string;
}

interface Item {
  id: string;
  name: string;
  sku: string | null;
  categoryName: string | null;
  categoryId: string | null;
  unitName: string | null;
  unitId: string | null;
  sellingPrice: number | null;
  costPrice: number | null;
  minimumQuantity: number;
  currentStock: number;
  isActive: boolean;
}

interface ItemsResponse {
  items: Item[];
  totalCount: number;
  page: number;
  pageSize: number;
}

interface ItemDetail {
  id: string;
  name: string;
  nameAr: string | null;
  categoryId: string | null;
  unitOfMeasureId: string | null;
  unitCost: number;
  salePrice: number;
  reorderLevel: number;
  reorderQuantity: number;
  isExpiryTracked: boolean;
  allowNegativeStock: boolean;
  storageConditions: string | null;
  notes: string | null;
}

const emptyCreate = {
  nameAr: "",
  sku: "",
  barcode: "",
  categoryId: "",
  unitOfMeasureId: "",
  unitCost: "",
  salePrice: "",
  reorderLevel: "0",
  reorderQuantity: "0",
  isExpiryTracked: false,
  allowNegativeStock: false,
  notes: "",
};

type EditForm = {
  name: string;
  nameAr: string;
  categoryId: string;
  unitOfMeasureId: string;
  unitCost: string;
  salePrice: string;
  reorderLevel: string;
  reorderQuantity: string;
  isExpiryTracked: boolean;
  allowNegativeStock: boolean;
  storageConditions: string;
  notes: string;
};

export default function InventoryItemsPage() {
  const router = useRouter();
  const [data, setData] = useState<ItemsResponse | null>(null);
  const [categories, setCategories] = useState<Category[]>([]);
  const [units, setUnits] = useState<Unit[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const [catFilter, setCatFilter] = useState("");
  const [lowStock, setLowStock] = useState(false);
  const [page, setPage] = useState(1);

  const [showCreate, setShowCreate] = useState(false);
  const [createForm, setCreateForm] = useState(emptyCreate);

  const [showEdit, setShowEdit] = useState<string | null>(null); // item id
  const [editForm, setEditForm] = useState<EditForm>({
    name: "", nameAr: "", categoryId: "", unitOfMeasureId: "",
    unitCost: "", salePrice: "", reorderLevel: "0", reorderQuantity: "0",
    isExpiryTracked: false, allowNegativeStock: false,
    storageConditions: "", notes: "",
  });
  const [editLoading, setEditLoading] = useState(false);

  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => { loadMeta(); }, []);
  useEffect(() => { loadItems(); }, [search, catFilter, lowStock, page]);

  async function loadMeta() {
    await Promise.allSettled([
      api.get<Category[]>("/inventory/item-categories").then((r) => setCategories(r.data)),
      api.get<Unit[]>("/inventory/units-of-measure").then((r) => setUnits(r.data)),
    ]);
  }

  async function loadItems() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: "25" });
      if (search) params.set("search", search);
      if (catFilter) params.set("categoryId", catFilter);
      if (lowStock) params.set("lowStock", "true");
      const r = await api.get<ItemsResponse>(`/inventory/items?${params}`);
      setData(r.data);
    } finally {
      setLoading(false);
    }
  }

  async function createItem() {
    if (!createForm.nameAr.trim()) { setError("اسم الصنف مطلوب"); return; }
    setSaving(true);
    setError(null);
    try {
      await api.post("/inventory/items", {
        name: createForm.nameAr.trim(),
        nameAr: createForm.nameAr.trim(),
        sku: createForm.sku || null,
        barcode: createForm.barcode || null,
        categoryId: createForm.categoryId || null,
        unitOfMeasureId: createForm.unitOfMeasureId || null,
        unitCost: parseFloat(createForm.unitCost) || 0,
        salePrice: parseFloat(createForm.salePrice) || 0,
        reorderLevel: parseInt(createForm.reorderLevel) || 0,
        reorderQuantity: parseInt(createForm.reorderQuantity) || 0,
        isExpiryTracked: createForm.isExpiryTracked,
        allowNegativeStock: createForm.allowNegativeStock,
        storageConditions: null,
        notes: createForm.notes || null,
        createdByUserId: null,
      });
      setShowCreate(false);
      setCreateForm(emptyCreate);
      loadItems();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  }

  async function openEdit(itemId: string) {
    setShowEdit(itemId);
    setError(null);
    setEditLoading(true);
    try {
      const r = await api.get<ItemDetail>(`/inventory/items/${itemId}`);
      const d = r.data;
      setEditForm({
        name: d.name,
        nameAr: d.nameAr ?? d.name,
        categoryId: d.categoryId ?? "",
        unitOfMeasureId: d.unitOfMeasureId ?? "",
        unitCost: String(d.unitCost),
        salePrice: String(d.salePrice),
        reorderLevel: String(d.reorderLevel),
        reorderQuantity: String(d.reorderQuantity),
        isExpiryTracked: d.isExpiryTracked,
        allowNegativeStock: d.allowNegativeStock,
        storageConditions: d.storageConditions ?? "",
        notes: d.notes ?? "",
      });
    } catch {
      setError("تعذّر تحميل بيانات الصنف");
    } finally {
      setEditLoading(false);
    }
  }

  async function updateItem() {
    if (!showEdit) return;
    if (!editForm.name.trim()) { setError("اسم الصنف مطلوب"); return; }
    setSaving(true);
    setError(null);
    try {
      await api.put(`/inventory/items/${showEdit}`, {
        name: editForm.name.trim(),
        nameAr: editForm.nameAr.trim() || null,
        categoryId: editForm.categoryId || null,
        unitOfMeasureId: editForm.unitOfMeasureId || null,
        unitCost: parseFloat(editForm.unitCost) || undefined,
        salePrice: parseFloat(editForm.salePrice) || undefined,
        reorderLevel: parseFloat(editForm.reorderLevel) || 0,
        reorderQuantity: parseFloat(editForm.reorderQuantity) || 0,
        isExpiryTracked: editForm.isExpiryTracked,
        allowNegativeStock: editForm.allowNegativeStock,
        storageConditions: editForm.storageConditions || null,
        notes: editForm.notes || null,
      });
      setShowEdit(null);
      loadItems();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ أثناء الحفظ");
    } finally {
      setSaving(false);
    }
  }

  const totalPages = data ? Math.ceil(data.totalCount / 25) : 1;

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">الأصناف</h1>
        <button onClick={() => { setShowCreate(true); setError(null); setCreateForm(emptyCreate); }}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">
          + صنف جديد
        </button>
      </div>

      {/* Filters */}
      <div className="flex flex-wrap gap-3 mb-4">
        <input type="text" placeholder="بحث..." value={search} onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          className="border rounded-lg px-3 py-2 text-sm w-52 focus:outline-none focus:ring-2 focus:ring-blue-400" />
        <select value={catFilter} onChange={(e) => { setCatFilter(e.target.value); setPage(1); }}
          className="border rounded-lg px-3 py-2 text-sm focus:outline-none">
          <option value="">كل الفئات</option>
          {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
        </select>
        <label className="flex items-center gap-2 cursor-pointer text-sm text-gray-700">
          <input type="checkbox" checked={lowStock} onChange={(e) => { setLowStock(e.target.checked); setPage(1); }} className="rounded" />
          مخزون منخفض فقط
        </label>
      </div>

      {/* Table */}
      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الصنف</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الفئة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الوحدة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المخزون الحالي</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">سعر البيع</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الحالة</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={7} className="text-center py-8 text-gray-400">جاري التحميل...</td></tr>
            ) : (data?.items ?? []).length === 0 ? (
              <tr><td colSpan={7} className="text-center py-8 text-gray-400">لا توجد أصناف</td></tr>
            ) : (data?.items ?? []).map((item) => (
              <tr key={item.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <div className="text-sm font-medium text-gray-800">{item.name}</div>
                  {item.sku && <div className="text-xs text-gray-400 font-mono">{item.sku}</div>}
                </td>
                <td className="px-4 py-3 text-sm text-gray-600">{item.categoryName ?? "—"}</td>
                <td className="px-4 py-3 text-sm text-gray-600">{item.unitName ?? "—"}</td>
                <td className="px-4 py-3">
                  <span className={`text-sm font-medium ${item.currentStock <= item.minimumQuantity ? "text-red-600" : "text-gray-800"}`}>
                    {item.currentStock}
                  </span>
                  {item.currentStock <= item.minimumQuantity && (
                    <span className="mr-1 text-xs text-red-500">⚠</span>
                  )}
                </td>
                <td className="px-4 py-3 text-sm text-gray-800">
                  {item.sellingPrice != null ? `${item.sellingPrice.toFixed(2)} د.ل` : "—"}
                </td>
                <td className="px-4 py-3">
                  <span className={`text-xs px-2 py-0.5 rounded-full ${item.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"}`}>
                    {item.isActive ? "نشط" : "غير نشط"}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <div className="flex gap-2">
                    <button
                      onClick={() => openEdit(item.id)}
                      className="text-xs text-blue-600 border border-blue-200 px-2 py-1 rounded hover:bg-blue-50"
                    >
                      تعديل
                    </button>
                    <button
                      onClick={() => router.push(`/inventory/movements?itemId=${item.id}`)}
                      className="text-xs text-gray-600 border border-gray-200 px-2 py-1 rounded hover:bg-gray-50"
                    >
                      الحركات
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

      {/* ── Create Modal ─────────────────────────────────────────────────── */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-xl max-h-[90vh] overflow-y-auto" dir="rtl">
            <div className="sticky top-0 bg-white border-b px-5 py-4 flex items-center justify-between rounded-t-2xl z-10">
              <h2 className="text-base font-bold text-gray-900">صنف جديد</h2>
              <button onClick={() => setShowCreate(false)} className="text-gray-400 hover:text-gray-700 text-xl font-bold leading-none">×</button>
            </div>
            <div className="p-5 space-y-4">
              {error && <div className="text-sm text-red-600 bg-red-50 border border-red-100 rounded-lg px-3 py-2">{error}</div>}
              <div className="grid grid-cols-2 gap-3">
                <div className="col-span-2">
                  <label className="block text-xs font-medium text-gray-500 mb-1">اسم الصنف *</label>
                  <input autoFocus
                    className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                    value={createForm.nameAr}
                    onChange={(e) => setCreateForm({ ...createForm, nameAr: e.target.value })}
                    placeholder="مثال: حشو ضوئي" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 mb-1">SKU / كود</label>
                  <input className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 font-mono"
                    value={createForm.sku} onChange={(e) => setCreateForm({ ...createForm, sku: e.target.value })} placeholder="اختياري" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 mb-1">الباركود</label>
                  <input className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                    value={createForm.barcode} onChange={(e) => setCreateForm({ ...createForm, barcode: e.target.value })} placeholder="اختياري" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 mb-1">الفئة</label>
                  <select className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                    value={createForm.categoryId} onChange={(e) => setCreateForm({ ...createForm, categoryId: e.target.value })}>
                    <option value="">— بدون فئة —</option>
                    {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 mb-1">وحدة القياس</label>
                  <select className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                    value={createForm.unitOfMeasureId} onChange={(e) => setCreateForm({ ...createForm, unitOfMeasureId: e.target.value })}>
                    <option value="">— بدون وحدة —</option>
                    {units.map((u) => <option key={u.id} value={u.id}>{u.name}{u.abbreviation ? ` (${u.abbreviation})` : ""}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 mb-1">سعر الشراء</label>
                  <input type="number" step="0.01" min="0"
                    className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                    value={createForm.unitCost} onChange={(e) => setCreateForm({ ...createForm, unitCost: e.target.value })} placeholder="0.00" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 mb-1">سعر البيع</label>
                  <input type="number" step="0.01" min="0"
                    className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                    value={createForm.salePrice} onChange={(e) => setCreateForm({ ...createForm, salePrice: e.target.value })} placeholder="0.00" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 mb-1">حد إعادة الطلب</label>
                  <input type="number" min="0"
                    className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                    value={createForm.reorderLevel} onChange={(e) => setCreateForm({ ...createForm, reorderLevel: e.target.value })} />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-500 mb-1">كمية إعادة الطلب</label>
                  <input type="number" min="0"
                    className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                    value={createForm.reorderQuantity} onChange={(e) => setCreateForm({ ...createForm, reorderQuantity: e.target.value })} />
                </div>
                <div className="col-span-2 flex gap-6 py-1">
                  <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
                    <input type="checkbox" checked={createForm.isExpiryTracked}
                      onChange={(e) => setCreateForm({ ...createForm, isExpiryTracked: e.target.checked })} className="rounded" />
                    تتبع تاريخ الانتهاء
                  </label>
                  <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
                    <input type="checkbox" checked={createForm.allowNegativeStock}
                      onChange={(e) => setCreateForm({ ...createForm, allowNegativeStock: e.target.checked })} className="rounded" />
                    السماح بالمخزون السالب
                  </label>
                </div>
                <div className="col-span-2">
                  <label className="block text-xs font-medium text-gray-500 mb-1">ملاحظات</label>
                  <textarea rows={2} className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                    value={createForm.notes} onChange={(e) => setCreateForm({ ...createForm, notes: e.target.value })} placeholder="اختياري" />
                </div>
              </div>
              <div className="flex gap-3 pt-1">
                <button onClick={createItem} disabled={saving || !createForm.nameAr.trim()}
                  className="flex-1 bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
                  {saving ? "جاري الحفظ..." : "حفظ الصنف"}
                </button>
                <button onClick={() => setShowCreate(false)} className="border px-5 py-2.5 rounded-lg text-sm text-gray-700 hover:bg-gray-50">إلغاء</button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* ── Edit Modal (Master Data only — no stock quantities) ────────────── */}
      {showEdit && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-xl max-h-[90vh] overflow-y-auto" dir="rtl">
            <div className="sticky top-0 bg-white border-b px-5 py-4 flex items-center justify-between rounded-t-2xl z-10">
              <h2 className="text-base font-bold text-gray-900">تعديل بيانات الصنف</h2>
              <button onClick={() => { setShowEdit(null); setError(null); }} className="text-gray-400 hover:text-gray-700 text-xl font-bold leading-none">×</button>
            </div>
            <div className="p-5 space-y-4">
              {error && <div className="text-sm text-red-600 bg-red-50 border border-red-100 rounded-lg px-3 py-2">{error}</div>}
              {editLoading ? (
                <div className="text-center py-8 text-gray-400 text-sm">جاري التحميل...</div>
              ) : (
                <>
                  <div className="grid grid-cols-2 gap-3">
                    <div className="col-span-2">
                      <label className="block text-xs font-medium text-gray-500 mb-1">اسم الصنف *</label>
                      <input autoFocus
                        className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                        value={editForm.name}
                        onChange={(e) => setEditForm({ ...editForm, name: e.target.value, nameAr: e.target.value })} />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-500 mb-1">الفئة</label>
                      <select className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                        value={editForm.categoryId} onChange={(e) => setEditForm({ ...editForm, categoryId: e.target.value })}>
                        <option value="">— بدون فئة —</option>
                        {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                      </select>
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-500 mb-1">وحدة القياس</label>
                      <select className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                        value={editForm.unitOfMeasureId} onChange={(e) => setEditForm({ ...editForm, unitOfMeasureId: e.target.value })}>
                        <option value="">— بدون وحدة —</option>
                        {units.map((u) => <option key={u.id} value={u.id}>{u.name}{u.abbreviation ? ` (${u.abbreviation})` : ""}</option>)}
                      </select>
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-500 mb-1">سعر الشراء</label>
                      <input type="number" step="0.01" min="0"
                        className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                        value={editForm.unitCost} onChange={(e) => setEditForm({ ...editForm, unitCost: e.target.value })} />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-500 mb-1">سعر البيع</label>
                      <input type="number" step="0.01" min="0"
                        className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                        value={editForm.salePrice} onChange={(e) => setEditForm({ ...editForm, salePrice: e.target.value })} />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-500 mb-1">حد إعادة الطلب</label>
                      <input type="number" min="0"
                        className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                        value={editForm.reorderLevel} onChange={(e) => setEditForm({ ...editForm, reorderLevel: e.target.value })} />
                    </div>
                    <div>
                      <label className="block text-xs font-medium text-gray-500 mb-1">كمية إعادة الطلب</label>
                      <input type="number" min="0"
                        className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                        value={editForm.reorderQuantity} onChange={(e) => setEditForm({ ...editForm, reorderQuantity: e.target.value })} />
                    </div>
                    <div className="col-span-2 flex gap-6 py-1">
                      <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
                        <input type="checkbox" checked={editForm.isExpiryTracked}
                          onChange={(e) => setEditForm({ ...editForm, isExpiryTracked: e.target.checked })} className="rounded" />
                        تتبع تاريخ الانتهاء
                      </label>
                      <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
                        <input type="checkbox" checked={editForm.allowNegativeStock}
                          onChange={(e) => setEditForm({ ...editForm, allowNegativeStock: e.target.checked })} className="rounded" />
                        السماح بالمخزون السالب
                      </label>
                    </div>
                    <div className="col-span-2">
                      <label className="block text-xs font-medium text-gray-500 mb-1">ملاحظات</label>
                      <textarea rows={2} className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
                        value={editForm.notes} onChange={(e) => setEditForm({ ...editForm, notes: e.target.value })} />
                    </div>
                  </div>
                  <div className="flex gap-3 pt-1">
                    <button onClick={updateItem} disabled={saving || !editForm.name.trim()}
                      className="flex-1 bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
                      {saving ? "جاري الحفظ..." : "حفظ التعديلات"}
                    </button>
                    <button onClick={() => { setShowEdit(null); setError(null); }} className="border px-5 py-2.5 rounded-lg text-sm text-gray-700 hover:bg-gray-50">إلغاء</button>
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
