"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface Category { id: string; name: string; }
interface Unit { id: string; name: string; abbreviation: string; }

const emptyForm = {
  name: "",
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

export default function NewItemPage() {
  const router = useRouter();
  const [categories, setCategories] = useState<Category[]>([]);
  const [units, setUnits] = useState<Unit[]>([]);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([
      api.get<Category[]>("/inventory/item-categories"),
      api.get<Unit[]>("/inventory/units-of-measure"),
    ]).then(([catRes, unitRes]) => {
      setCategories(Array.isArray(catRes.data) ? catRes.data : []);
      setUnits(Array.isArray(unitRes.data) ? unitRes.data : []);
    }).catch(() => {});
  }, []);

  async function save() {
    if (!form.name.trim()) { setError("اسم الصنف مطلوب"); return; }
    setSaving(true);
    setError(null);
    try {
      await api.post("/inventory/items", {
        name: form.name.trim(),
        nameAr: form.nameAr || null,
        sku: form.sku || null,
        barcode: form.barcode || null,
        categoryId: form.categoryId || null,
        unitOfMeasureId: form.unitOfMeasureId || null,
        unitCost: parseFloat(form.unitCost) || 0,
        salePrice: parseFloat(form.salePrice) || 0,
        reorderLevel: parseInt(form.reorderLevel) || 0,
        reorderQuantity: parseInt(form.reorderQuantity) || 0,
        isExpiryTracked: form.isExpiryTracked,
        allowNegativeStock: form.allowNegativeStock,
        storageConditions: null,
        notes: form.notes || null,
        createdByUserId: null,
      });
      router.push("/inventory/items");
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string; message?: string; title?: string } } };
      setError(err?.response?.data?.error ?? err?.response?.data?.message ?? err?.response?.data?.title ?? "حدث خطأ");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="p-6 max-w-2xl mx-auto" dir="rtl">
      <div className="flex items-center gap-3 mb-6">
        <button onClick={() => router.push("/inventory/items")} className="text-gray-500 hover:text-gray-700 text-sm">← رجوع</button>
        <h1 className="text-2xl font-bold text-gray-900">إضافة صنف جديد</h1>
      </div>

      {error && <div className="mb-4 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-4 py-3">{error}</div>}

      <div className="bg-white rounded-xl shadow p-6 space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">اسم الصنف (عربي) *</label>
            <input
              autoFocus
              className="w-full border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-400 focus:outline-none"
              value={form.nameAr}
              onChange={(e) => setForm({ ...form, nameAr: e.target.value, name: e.target.value })}
              placeholder="مثال: حشو ضوئي"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">SKU / كود</label>
            <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.sku} onChange={(e) => setForm({ ...form, sku: e.target.value })} placeholder="اختياري" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">الباركود</label>
            <input className="w-full border rounded-lg px-3 py-2 text-sm" value={form.barcode} onChange={(e) => setForm({ ...form, barcode: e.target.value })} placeholder="اختياري" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">الفئة</label>
            <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.categoryId} onChange={(e) => setForm({ ...form, categoryId: e.target.value })}>
              <option value="">— بدون فئة —</option>
              {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">وحدة القياس</label>
            <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.unitOfMeasureId} onChange={(e) => setForm({ ...form, unitOfMeasureId: e.target.value })}>
              <option value="">— بدون وحدة —</option>
              {units.map((u) => <option key={u.id} value={u.id}>{u.name}{u.abbreviation ? ` (${u.abbreviation})` : ""}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">سعر الشراء</label>
            <input type="number" step="0.01" min="0" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.unitCost} onChange={(e) => setForm({ ...form, unitCost: e.target.value })} placeholder="0.00" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">سعر البيع</label>
            <input type="number" step="0.01" min="0" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.salePrice} onChange={(e) => setForm({ ...form, salePrice: e.target.value })} placeholder="0.00" />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">حد إعادة الطلب</label>
            <input type="number" min="0" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.reorderLevel} onChange={(e) => setForm({ ...form, reorderLevel: e.target.value })} />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">كمية إعادة الطلب</label>
            <input type="number" min="0" className="w-full border rounded-lg px-3 py-2 text-sm" value={form.reorderQuantity} onChange={(e) => setForm({ ...form, reorderQuantity: e.target.value })} />
          </div>
          <div className="col-span-2 flex gap-6">
            <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
              <input type="checkbox" checked={form.isExpiryTracked} onChange={(e) => setForm({ ...form, isExpiryTracked: e.target.checked })} className="rounded" />
              تتبع تاريخ الانتهاء
            </label>
            <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
              <input type="checkbox" checked={form.allowNegativeStock} onChange={(e) => setForm({ ...form, allowNegativeStock: e.target.checked })} className="rounded" />
              السماح بالمخزون السالب
            </label>
          </div>
          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
            <textarea rows={2} className="w-full border rounded-lg px-3 py-2 text-sm" value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} placeholder="اختياري" />
          </div>
        </div>

        <div className="flex gap-3 pt-2">
          <button
            onClick={save}
            disabled={saving || !form.name.trim()}
            className="flex-1 bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
          >
            {saving ? "جاري الحفظ..." : "حفظ الصنف"}
          </button>
          <button onClick={() => router.push("/inventory/items")} className="border px-6 py-2.5 rounded-lg text-sm text-gray-700 hover:bg-gray-50">
            إلغاء
          </button>
        </div>
      </div>
    </div>
  );
}
