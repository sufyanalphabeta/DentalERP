"use client";

import { useEffect, useRef, useState, useCallback } from "react";
import { useParams, useRouter, useSearchParams } from "next/navigation";
import { api } from "@/lib/api";

// ---- Types ----
interface Supplier { id: string; name: string; phone: string | null; computedBalance: number; }
interface Warehouse { id: string; name: string; }
interface Category { id: string; name: string; }
interface Unit { id: string; name: string; abbreviation: string; }
interface ItemSearchResult {
  id: string; itemCode: string; name: string;
  barcode: string | null; unitCost: number; salePrice: number; kind: string;
  currentStock: number;
}

interface InvLine {
  _key: string; itemId: string; itemCode: string; itemName: string;
  barcode: string; unitName: string; quantity: number;
  purchasePrice: number; salePrice: number; expiryDate: string;
  batchNumber: string; lineTotal: number;
}

interface PIDetail {
  id: string; invoiceNumber: string; invoiceDate: string;
  supplierId: string; supplierName: string; supplierPhone: string | null;
  warehouseId: string | null; warehouseName: string | null;
  status: string; subtotal: number; discount: number; netTotal: number;
  notes: string | null; createdAt: string; postedAt: string | null; cancelledAt: string | null;
  items: Array<{
    id: string; itemId: string; itemCode: string | null; itemName: string;
    barcode: string | null; unitName: string | null; quantity: number;
    purchasePrice: number; salePrice: number | null; lineTotal: number;
    expiryDate: string | null; batchNumber: string | null; sortOrder: number;
  }>;
}

interface SupplierBalanceResponse { supplierId: string; supplierName: string; balance: number; }
interface SuppliersResponse { suppliers: Supplier[]; }
interface WarehousesResponse { warehouses?: Warehouse[]; }

const emptyNewItemForm = {
  name: "", nameAr: "", sku: "", barcode: "", categoryId: "",
  unitOfMeasureId: "", unitCost: "", salePrice: "", reorderLevel: "0",
  reorderQuantity: "0", isExpiryTracked: false, allowNegativeStock: false, notes: "",
};

const newLine = (): InvLine => ({
  _key: crypto.randomUUID(), itemId: "", itemCode: "", itemName: "",
  barcode: "", unitName: "", quantity: 1, purchasePrice: 0, salePrice: 0,
  expiryDate: "", batchNumber: "", lineTotal: 0,
});

const STATUS_AR: Record<string, string> = { Draft: "مسودة", Posted: "مرحّلة", Cancelled: "ملغاة" };
const STATUS_CLS: Record<string, string> = {
  Draft: "text-amber-700 bg-amber-50", Posted: "text-green-700 bg-green-50", Cancelled: "text-red-600 bg-red-50",
};

// Column indices for keyboard navigation
const COL_QTY = 0, COL_PRICE = 1, COL_SALE = 2, COL_EXPIRY = 3;

export default function PurchaseInvoicePage() {
  const params = useParams<{ id?: string }>();
  const id = params?.id ?? "new";
  const router = useRouter();
  const searchParams = useSearchParams();
  const isNew = id === "new";

  // Header state
  const [inv, setInv] = useState<PIDetail | null>(null);
  const [supplierId, setSupplierId] = useState("");
  const [supplierInfo, setSupplierInfo] = useState<{ name: string; phone: string | null; balance: number; } | null>(null);
  const [warehouseId, setWarehouseId] = useState("");
  const [invoiceDate, setInvoiceDate] = useState(new Date().toISOString().split("T")[0]);
  const [discount, setDiscount] = useState(0);
  const [notes, setNotes] = useState("");
  const [lines, setLines] = useState<InvLine[]>([]);

  // Lookup data
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [units, setUnits] = useState<Unit[]>([]);

  // Item search (above table)
  const [itemQuery, setItemQuery] = useState("");
  const [itemResults, setItemResults] = useState<ItemSearchResult[]>([]);
  const [itemLoading, setItemLoading] = useState(false);
  const [itemDropOpen, setItemDropOpen] = useState(false);
  const searchContainerRef = useRef<HTMLDivElement>(null);
  const searchInputRef = useRef<HTMLInputElement>(null);

  // Table cell refs for Enter-key navigation: [rowIdx][colIdx]
  const cellRefs = useRef<(HTMLInputElement | null)[][]>([]);
  const setCellRef = useCallback((el: HTMLInputElement | null, row: number, col: number) => {
    if (!cellRefs.current[row]) cellRefs.current[row] = [];
    cellRefs.current[row][col] = el;
  }, []);

  // New item modal
  const [newItemOpen, setNewItemOpen] = useState(false);
  const [newItemForm, setNewItemForm] = useState(emptyNewItemForm);
  const [newItemSaving, setNewItemSaving] = useState(false);
  const [newItemError, setNewItemError] = useState<string | null>(null);

  // UI state
  const [loading, setLoading] = useState(!isNew);
  const [saving, setSaving] = useState(false);
  const [posting, setPosting] = useState(false);
  const [cancelling, setCancelling] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [pdfLoading, setPdfLoading] = useState(false);

  const subtotal = lines.reduce((s, l) => s + l.lineTotal, 0);
  const netTotal = Math.max(0, subtotal - discount);

  // Load suppliers, warehouses, categories, units on mount
  useEffect(() => {
    Promise.all([
      api.get<SuppliersResponse>("/suppliers?pageSize=500&activeOnly=true"),
      api.get<WarehousesResponse | Warehouse[]>("/inventory/warehouses"),
      api.get<Category[]>("/inventory/item-categories"),
      api.get<Unit[]>("/inventory/units-of-measure"),
    ]).then(([supRes, whRes, catRes, unitRes]) => {
      setSuppliers(supRes.data.suppliers ?? []);
      const rawWh = whRes.data;
      const whs: Warehouse[] = Array.isArray(rawWh) ? rawWh : (rawWh as WarehousesResponse).warehouses ?? [];
      setWarehouses(whs);
      if (whs.length > 0 && !warehouseId) setWarehouseId(whs[0].id);
      setCategories(Array.isArray(catRes.data) ? catRes.data : []);
      setUnits(Array.isArray(unitRes.data) ? unitRes.data : []);
      const preSupplier = searchParams?.get("supplierId");
      if (isNew && preSupplier) setSupplierId(preSupplier);
    }).catch(() => {});
  }, []);

  useEffect(() => { if (!isNew) loadExisting(); }, [id]);

  async function loadExisting() {
    setLoading(true);
    try {
      const r = await api.get<PIDetail>(`/purchasing/invoices/${id}`);
      const detail = r.data;
      setInv(detail);
      setSupplierId(detail.supplierId);
      setSupplierInfo({ name: detail.supplierName, phone: detail.supplierPhone, balance: 0 });
      setWarehouseId(detail.warehouseId ?? "");
      setInvoiceDate(detail.invoiceDate);
      setDiscount(detail.discount);
      setNotes(detail.notes ?? "");
      setLines(detail.items.map((item) => ({
        _key: crypto.randomUUID(), itemId: item.itemId, itemCode: item.itemCode ?? "",
        itemName: item.itemName, barcode: item.barcode ?? "", unitName: item.unitName ?? "",
        quantity: item.quantity, purchasePrice: item.purchasePrice, salePrice: item.salePrice ?? 0,
        expiryDate: item.expiryDate ?? "", batchNumber: item.batchNumber ?? "", lineTotal: item.lineTotal,
      })));
    } catch { setError("لم يتم العثور على الفاتورة"); }
    finally { setLoading(false); }
  }

  useEffect(() => {
    if (!supplierId) { setSupplierInfo(null); return; }
    api.get<SupplierBalanceResponse>(`/suppliers/${supplierId}/balance`)
      .then((r) => setSupplierInfo({ name: r.data.supplierName, phone: null, balance: r.data.balance }))
      .catch(() => {});
  }, [supplierId]);

  // Item search — debounced
  useEffect(() => {
    if (!itemDropOpen) return;
    setItemLoading(true);
    const timer = setTimeout(async () => {
      try {
        const r = await api.get<ItemSearchResult[]>(
          `/purchasing/invoices/item-search?q=${encodeURIComponent(itemQuery)}&limit=30`
        );
        setItemResults(r.data);
      } catch { setItemResults([]); }
      finally { setItemLoading(false); }
    }, 150);
    return () => clearTimeout(timer);
  }, [itemQuery, itemDropOpen]);

  // Close search dropdown on click outside
  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (searchContainerRef.current && !searchContainerRef.current.contains(e.target as Node)) {
        setItemDropOpen(false);
      }
    }
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  // Add item from search bar to table
  function addItemFromSearch(item: ItemSearchResult) {
    const line: InvLine = {
      _key: crypto.randomUUID(), itemId: item.id, itemCode: item.itemCode, itemName: item.name,
      barcode: item.barcode ?? "", unitName: "", quantity: 1,
      purchasePrice: item.unitCost, salePrice: item.salePrice ?? 0,
      expiryDate: "", batchNumber: "", lineTotal: item.unitCost,
    };
    setLines((prev) => {
      const next = [...prev, line];
      const newIdx = next.length - 1;
      setTimeout(() => {
        cellRefs.current[newIdx]?.[COL_QTY]?.focus();
        cellRefs.current[newIdx]?.[COL_QTY]?.select();
      }, 30);
      return next;
    });
    setItemQuery("");
    setItemDropOpen(false);
  }

  // Enter-key navigation between table cells
  function handleCellKey(e: React.KeyboardEvent, rowIdx: number, colIdx: number) {
    if (e.key !== "Enter") return;
    e.preventDefault();
    if (colIdx < COL_EXPIRY) {
      cellRefs.current[rowIdx]?.[colIdx + 1]?.focus();
      cellRefs.current[rowIdx]?.[colIdx + 1]?.select();
    } else {
      // last column → move to next row qty, or focus search bar
      const nextRow = rowIdx + 1;
      if (cellRefs.current[nextRow]?.[COL_QTY]) {
        cellRefs.current[nextRow][COL_QTY]?.focus();
        cellRefs.current[nextRow][COL_QTY]?.select();
      } else {
        searchInputRef.current?.focus();
      }
    }
  }

  function updateLine(idx: number, field: keyof InvLine, value: string | number) {
    setLines((prev) => {
      const next = [...prev];
      const line = { ...next[idx], [field]: value };
      line.lineTotal = line.quantity * line.purchasePrice;
      next[idx] = line;
      return next;
    });
  }

  function removeLine(idx: number) { setLines((prev) => prev.filter((_, i) => i !== idx)); }

  // Build save payload
  function buildPayload(overrideLines?: InvLine[]) {
    const validLines = (overrideLines ?? lines).filter((l) => l.itemId && l.quantity > 0);
    return {
      supplierId, invoiceDate, warehouseId: warehouseId || null,
      discount, notes: notes || null, createdById: null,
      items: validLines.map((l, i) => ({
        itemId: l.itemId, itemName: l.itemName, itemCode: l.itemCode || null,
        barcode: l.barcode || null, unitName: l.unitName || null,
        quantity: l.quantity, purchasePrice: l.purchasePrice,
        salePrice: l.salePrice > 0 ? l.salePrice : null,
        expiryDate: l.expiryDate || null, batchNumber: l.batchNumber || null, sortOrder: i,
      })),
    };
  }

  async function save(): Promise<string | null> {
    if (!supplierId) { setError("يرجى اختيار المورد"); return null; }
    const validLines = lines.filter((l) => l.itemId && l.quantity > 0);
    if (validLines.length === 0) { setError("أضف صنفاً واحداً على الأقل"); return null; }
    setSaving(true); setError(null);
    try {
      if (isNew) {
        const r = await api.post<{ id: string }>("/purchasing/invoices", buildPayload());
        setSuccess("تم حفظ الفاتورة كمسودة");
        setTimeout(() => setSuccess(null), 3000);
        router.replace(`/purchasing/invoices/${r.data.id}`);
        return r.data.id;
      } else {
        await api.put(`/purchasing/invoices/${id}`, buildPayload());
        setSuccess("تم حفظ التعديلات");
        setTimeout(() => { setSuccess(null); loadExisting(); }, 2000);
        return id;
      }
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string; detail?: string; title?: string } } };
      setError(err?.response?.data?.error ?? err?.response?.data?.detail ?? err?.response?.data?.title ?? "حدث خطأ");
      return null;
    } finally { setSaving(false); }
  }

  async function postInvoice() {
    if (!inv || inv.status !== "Draft") return;
    setPosting(true); setError(null);
    try {
      // Save current lines first so new items added in this session are persisted before posting
      const validLines = lines.filter((l) => l.itemId && l.quantity > 0);
      if (validLines.length === 0) { setError("أضف صنفاً واحداً على الأقل"); return; }
      await api.put(`/purchasing/invoices/${id}`, buildPayload());
      await api.post(`/purchasing/invoices/${id}/post`, {});
      setSuccess("تم ترحيل الفاتورة بنجاح — تم تحديث المخزون ورصيد المورد");
      setTimeout(() => { setSuccess(null); loadExisting(); }, 3000);
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string; detail?: string } } };
      setError(err?.response?.data?.error ?? err?.response?.data?.detail ?? "حدث خطأ أثناء الترحيل");
    } finally { setPosting(false); }
  }

  async function cancelInvoice() {
    if (!inv) return;
    const msg = inv.status === "Posted"
      ? `هل تريد إلغاء الفاتورة ${inv.invoiceNumber}؟\n\nتحذير: سيتم عكس تأثير المخزون (خصم الكميات المُستلمة).`
      : `هل تريد إلغاء الفاتورة ${inv.invoiceNumber}؟`;
    if (!window.confirm(msg)) return;
    setCancelling(true); setError(null);
    try {
      await api.post(`/purchasing/invoices/${id}/cancel`, { cancelledById: null });
      setSuccess("تم إلغاء الفاتورة" + (inv.status === "Posted" ? " وعكس المخزون" : ""));
      setTimeout(() => { setSuccess(null); loadExisting(); }, 2500);
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ أثناء الإلغاء");
    } finally { setCancelling(false); }
  }

  async function deleteInvoice() {
    if (!inv) return;
    if (!window.confirm(`هل تريد حذف الفاتورة ${inv.invoiceNumber}؟ لا يمكن التراجع.`)) return;
    setDeleting(true); setError(null);
    try {
      await api.delete(`/purchasing/invoices/${id}`);
      setSuccess("تم حذف الفاتورة");
      setTimeout(() => router.push("/purchasing/invoices"), 1500);
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ أثناء الحذف");
    } finally { setDeleting(false); }
  }

  // Open new item modal — auto-save draft first if there are lines
  async function openNewItemModal() {
    const hasLines = lines.some((l) => l.itemId);
    if (hasLines && supplierId && !isNew) {
      // Already saved — just open modal
    } else if (hasLines && supplierId) {
      // Save as draft first to preserve data
      await save();
    }
    setNewItemForm(emptyNewItemForm);
    setNewItemError(null);
    setNewItemOpen(true);
  }

  async function saveNewItem() {
    if (!newItemForm.nameAr.trim() && !newItemForm.name.trim()) {
      setNewItemError("اسم الصنف مطلوب"); return;
    }
    setNewItemSaving(true); setNewItemError(null);
    try {
      const name = newItemForm.nameAr.trim() || newItemForm.name.trim();
      const r = await api.post<{ id: string }>("/inventory/items", {
        name, nameAr: newItemForm.nameAr || null, itemCode: newItemForm.sku || null,
        barcode: newItemForm.barcode || null, categoryId: newItemForm.categoryId || null,
        unitOfMeasureId: newItemForm.unitOfMeasureId || null,
        unitCost: parseFloat(newItemForm.unitCost) || 0,
        salePrice: parseFloat(newItemForm.salePrice) || 0,
        reorderLevel: parseInt(newItemForm.reorderLevel) || 0,
        reorderQuantity: parseInt(newItemForm.reorderQuantity) || 0,
        isExpiryTracked: newItemForm.isExpiryTracked,
        allowNegativeStock: newItemForm.allowNegativeStock,
        storageConditions: null, notes: newItemForm.notes || null, createdByUserId: null,
      });
      // Auto-add the new item to the invoice lines
      const newId = r.data.id;
      const unitCost = parseFloat(newItemForm.unitCost) || 0;
      const saleP = parseFloat(newItemForm.salePrice) || 0;
      const line: InvLine = {
        _key: crypto.randomUUID(), itemId: newId, itemCode: newItemForm.sku,
        itemName: name, barcode: newItemForm.barcode,
        unitName: "", quantity: 1, purchasePrice: unitCost, salePrice: saleP,
        expiryDate: "", batchNumber: "", lineTotal: unitCost,
      };
      setLines((prev) => {
        const next = [...prev, line];
        const newIdx = next.length - 1;
        setTimeout(() => {
          cellRefs.current[newIdx]?.[COL_QTY]?.focus();
          cellRefs.current[newIdx]?.[COL_QTY]?.select();
        }, 80);
        return next;
      });
      setNewItemOpen(false);
      setSuccess(`تمت إضافة الصنف "${name}" وأضيف للفاتورة`);
      setTimeout(() => setSuccess(null), 3000);
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string; message?: string; title?: string } } };
      setNewItemError(err?.response?.data?.error ?? err?.response?.data?.message ?? err?.response?.data?.title ?? "حدث خطأ");
    } finally { setNewItemSaving(false); }
  }

  const isDraft = isNew || inv?.status === "Draft";
  const isPosted = inv?.status === "Posted";

  if (loading) return <div className="p-6 text-center text-gray-400">جاري التحميل...</div>;

  return (
    <div className="min-h-screen bg-gray-100" dir="rtl">
      {success && (
        <div className="fixed top-4 left-1/2 -translate-x-1/2 z-50 bg-green-600 text-white px-6 py-3 rounded-xl shadow-lg text-sm font-medium">
          ✓ {success}
        </div>
      )}

      {/* Sticky header */}
      <div className="bg-white border-b shadow-sm px-6 py-3 flex items-center justify-between sticky top-0 z-10">
        <div className="flex items-center gap-4">
          <button onClick={() => router.push("/purchasing/invoices")} className="text-gray-500 hover:text-gray-700 text-sm">← رجوع</button>
          <div>
            <div className="flex items-center gap-2">
              <span className="text-lg font-bold text-gray-800">
                {isNew ? "فاتورة مشتريات جديدة" : (inv?.invoiceNumber ?? "...")}
              </span>
              {inv && (
                <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${STATUS_CLS[inv.status] ?? ""}`}>
                  {STATUS_AR[inv.status] ?? inv.status}
                </span>
              )}
            </div>
            {supplierInfo && (
              <div className="text-xs text-gray-400 mt-0.5">
                المورد: <span className="text-gray-600 font-medium">{supplierInfo.name}</span>
                {supplierInfo.balance > 0 && (
                  <span className="mr-2 text-red-500">• الرصيد الحالي: {supplierInfo.balance.toFixed(2)} د.ل</span>
                )}
              </div>
            )}
          </div>
        </div>
        <div className="flex gap-2 flex-wrap">
          {isDraft && (
            <>
              <button onClick={() => save()} disabled={saving} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">
                {saving ? "جاري الحفظ..." : "💾 حفظ"}
              </button>
              {!isNew && (
                <button onClick={postInvoice} disabled={posting || lines.filter((l) => l.itemId).length === 0}
                  className="bg-green-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-green-700 disabled:opacity-50">
                  {posting ? "جاري الترحيل..." : "✓ ترحيل"}
                </button>
              )}
              {!isNew && (
                <button onClick={cancelInvoice} disabled={cancelling}
                  className="bg-amber-50 text-amber-700 border border-amber-200 px-3 py-2 rounded-lg text-sm hover:bg-amber-100 disabled:opacity-50">
                  {cancelling ? "..." : "✗ إلغاء"}
                </button>
              )}
              {!isNew && (
                <button onClick={deleteInvoice} disabled={deleting}
                  className="bg-red-50 text-red-600 border border-red-200 px-3 py-2 rounded-lg text-sm hover:bg-red-100 disabled:opacity-50">
                  {deleting ? "..." : "🗑 حذف"}
                </button>
              )}
            </>
          )}
          {isPosted && (
            <>
              <span className="text-green-700 bg-green-50 px-4 py-2 rounded-lg text-sm font-medium border border-green-200">
                ✓ تم الترحيل {inv?.postedAt ? new Date(inv.postedAt).toLocaleDateString("ar-LY") : ""}
              </span>
              <button
                onClick={async () => {
                  setPdfLoading(true);
                  try {
                    const res = await api.get(`/purchasing/invoices/${id}/pdf`, { responseType: "blob" });
                    const url = URL.createObjectURL(new Blob([res.data], { type: "application/pdf" }));
                    const a = document.createElement("a");
                    a.href = url;
                    a.download = `purchase-invoice-${inv?.invoiceNumber ?? id}.pdf`;
                    a.click();
                    URL.revokeObjectURL(url);
                  } finally {
                    setPdfLoading(false);
                  }
                }}
                disabled={pdfLoading}
                className="border border-gray-300 text-gray-600 px-3 py-2 rounded-lg text-sm hover:bg-gray-50 disabled:opacity-50"
              >
                {pdfLoading ? "جاري التحميل..." : "📄 PDF"}
              </button>
              <button onClick={cancelInvoice} disabled={cancelling}
                className="bg-red-50 text-red-600 border border-red-200 px-3 py-2 rounded-lg text-sm hover:bg-red-100 disabled:opacity-50">
                {cancelling ? "جاري الإلغاء..." : "↩ إلغاء وعكس المخزون"}
              </button>
            </>
          )}
          {inv?.status === "Cancelled" && (
            <>
              <span className="text-red-600 bg-red-50 px-4 py-2 rounded-lg text-sm font-medium border border-red-200">✗ ملغاة</span>
              <button onClick={deleteInvoice} disabled={deleting}
                className="bg-gray-50 text-gray-600 border border-gray-200 px-3 py-2 rounded-lg text-sm hover:bg-gray-100 disabled:opacity-50">
                {deleting ? "..." : "🗑 حذف نهائي"}
              </button>
            </>
          )}
        </div>
      </div>

      <div className="max-w-7xl mx-auto p-4 space-y-4">
        {error && <div className="bg-red-50 border border-red-200 text-red-700 text-sm rounded-xl px-4 py-3">{error}</div>}

        {/* Invoice header form */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="col-span-2">
              <label className="block text-xs font-medium text-gray-500 mb-1">المورد *</label>
              <select value={supplierId} onChange={(e) => setSupplierId(e.target.value)} disabled={!isDraft}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-50">
                <option value="">— اختر المورد —</option>
                {suppliers.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1">تاريخ الفاتورة</label>
              <input type="date" value={invoiceDate} onChange={(e) => setInvoiceDate(e.target.value)} disabled={!isDraft}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-50" />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1">المستودع</label>
              <select value={warehouseId} onChange={(e) => setWarehouseId(e.target.value)} disabled={!isDraft}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-50">
                <option value="">— الافتراضي —</option>
                {warehouses.map((w) => <option key={w.id} value={w.id}>{w.name}</option>)}
              </select>
            </div>
            <div className="col-span-2 md:col-span-3">
              <label className="block text-xs font-medium text-gray-500 mb-1">ملاحظات</label>
              <input value={notes} onChange={(e) => setNotes(e.target.value)} disabled={!isDraft}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-50"
                placeholder="ملاحظات اختيارية..." />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1">الخصم (د.ل)</label>
              <input type="number" min="0" step="0.01" value={discount}
                onChange={(e) => setDiscount(parseFloat(e.target.value) || 0)} disabled={!isDraft}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-50" />
            </div>
          </div>
        </div>

        {/* Items section */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100">
          {/* Card header */}
          <div className="flex items-center justify-between px-4 py-3 border-b bg-gray-50 rounded-t-xl">
            <span className="text-sm font-semibold text-gray-700">الأصناف ({lines.filter((l) => l.itemId).length})</span>
            {isDraft && (
              <button
                onClick={openNewItemModal}
                className="text-xs font-medium bg-emerald-600 text-white rounded px-3 py-1.5 hover:bg-emerald-700"
                title="إضافة صنف جديد للمخزون بدون مغادرة الصفحة"
              >
                + صنف جديد
              </button>
            )}
          </div>

          {/* Search bar above table */}
          {isDraft && (
            <div className="px-4 py-3 border-b bg-blue-50/40" ref={searchContainerRef} style={{ position: "relative" }}>
              <label className="block text-xs font-medium text-gray-500 mb-1.5">ابحث عن صنف لإضافته إلى الفاتورة</label>
              <input
                ref={searchInputRef}
                type="text"
                value={itemQuery}
                onChange={(e) => { setItemQuery(e.target.value); setItemDropOpen(true); }}
                onFocus={() => setItemDropOpen(true)}
                placeholder="اكتب اسم الصنف أو الكود أو الباركود..."
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 bg-white"
                autoComplete="off"
              />
              {itemDropOpen && (
                <div className="absolute left-4 right-4 bg-white border border-gray-200 rounded-xl shadow-xl z-40 mt-1" style={{ top: "100%" }}>
                  {itemLoading ? (
                    <div className="py-4 text-center text-gray-400 text-sm">جاري البحث...</div>
                  ) : itemResults.length === 0 ? (
                    <div className="py-4 text-center text-gray-400 text-sm">
                      {itemQuery ? `لا توجد أصناف تطابق "${itemQuery}"` : "اكتب للبحث أو اضغط في الحقل لعرض الأصناف"}
                    </div>
                  ) : (
                    <div className="max-h-64 overflow-y-auto">
                      {itemResults.map((item) => (
                        <button key={item.id} type="button"
                          onMouseDown={(e) => { e.preventDefault(); addItemFromSearch(item); }}
                          className="w-full text-right px-4 py-2.5 hover:bg-blue-50 flex justify-between items-center gap-3 border-b border-gray-50 last:border-0">
                          <div className="min-w-0">
                            <div className="text-sm font-medium text-gray-800 truncate">{item.name}</div>
                            <div className="text-xs text-gray-400 mt-0.5">
                              {item.itemCode}{item.barcode ? ` • ${item.barcode}` : ""}
                            </div>
                          </div>
                          <div className="text-left shrink-0">
                            <div className="text-sm text-blue-700 font-bold whitespace-nowrap">{item.unitCost.toFixed(2)} د.ل</div>
                            <div className={`text-xs mt-0.5 ${item.currentStock > 0 ? "text-green-600" : "text-gray-400"}`}>
                              المتاح: {item.currentStock}
                            </div>
                          </div>
                        </button>
                      ))}
                    </div>
                  )}
                </div>
              )}
            </div>
          )}

          {/* Table */}
          {lines.length === 0 ? (
            <div className="py-12 text-center text-gray-400 text-sm">
              {isDraft ? "ابحث عن صنف أعلاه لإضافته" : "لا توجد أصناف"}
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead className="bg-gray-50 border-b">
                  <tr>
                    <th className="px-2 py-2 text-right text-xs text-gray-500 w-8">#</th>
                    <th className="px-2 py-2 text-right text-xs text-gray-500 min-w-[180px]">اسم الصنف</th>
                    <th className="px-2 py-2 text-center text-xs text-gray-500 w-24">الكمية</th>
                    <th className="px-2 py-2 text-center text-xs text-gray-500 w-28">سعر الشراء</th>
                    <th className="px-2 py-2 text-center text-xs text-gray-500 w-28">سعر البيع</th>
                    <th className="px-2 py-2 text-center text-xs text-gray-500 w-32">تاريخ الانتهاء</th>
                    <th className="px-2 py-2 text-center text-xs text-gray-500 w-24">الإجمالي</th>
                    {isDraft && <th className="px-2 py-2 w-8"></th>}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {lines.map((line, idx) => (
                    <tr key={line._key} className="hover:bg-gray-50/50">
                      <td className="px-2 py-2 text-xs text-gray-400 text-center">{idx + 1}</td>
                      <td className="px-2 py-2">
                        <div className="text-sm font-medium text-gray-800">{line.itemName || "—"}</div>
                        {line.itemCode && (
                          <div className="text-xs text-gray-400 mt-0.5">
                            {line.itemCode}{line.barcode ? ` • ${line.barcode}` : ""}
                          </div>
                        )}
                      </td>

                      {/* Quantity — col 0 */}
                      <td className="px-2 py-2">
                        {isDraft ? (
                          <input type="number" min="0.001" step="any"
                            ref={(el) => setCellRef(el, idx, COL_QTY)}
                            className="w-full border rounded px-2 py-1 text-sm text-center focus:outline-none focus:ring-2 focus:ring-blue-400"
                            value={line.quantity}
                            onChange={(e) => updateLine(idx, "quantity", parseFloat(e.target.value) || 0)}
                            onKeyDown={(e) => handleCellKey(e, idx, COL_QTY)}
                          />
                        ) : (
                          <span className="text-sm text-gray-800 block text-center">{line.quantity}</span>
                        )}
                      </td>

                      {/* Purchase price — col 1 */}
                      <td className="px-2 py-2">
                        {isDraft ? (
                          <input type="number" min="0" step="0.01"
                            ref={(el) => setCellRef(el, idx, COL_PRICE)}
                            className="w-full border rounded px-2 py-1 text-sm text-center focus:outline-none focus:ring-2 focus:ring-blue-400"
                            value={line.purchasePrice}
                            onChange={(e) => updateLine(idx, "purchasePrice", parseFloat(e.target.value) || 0)}
                            onKeyDown={(e) => handleCellKey(e, idx, COL_PRICE)}
                          />
                        ) : (
                          <span className="text-sm text-gray-800 block text-center">{line.purchasePrice.toFixed(2)}</span>
                        )}
                      </td>

                      {/* Sale price — col 2 */}
                      <td className="px-2 py-2">
                        {isDraft ? (
                          <div>
                            <input type="number" min="0" step="0.01"
                              ref={(el) => setCellRef(el, idx, COL_SALE)}
                              className={`w-full border rounded px-2 py-1 text-sm text-center focus:outline-none focus:ring-2 focus:ring-blue-400 ${
                                line.salePrice > 0 && line.salePrice < line.purchasePrice ? "border-amber-400 bg-amber-50" : ""
                              }`}
                              value={line.salePrice}
                              onChange={(e) => updateLine(idx, "salePrice", parseFloat(e.target.value) || 0)}
                              onKeyDown={(e) => handleCellKey(e, idx, COL_SALE)}
                            />
                            {line.salePrice > 0 && line.salePrice < line.purchasePrice && (
                              <div className="text-xs text-amber-600 mt-0.5 text-center">⚠ أقل من الشراء</div>
                            )}
                          </div>
                        ) : (
                          <span className="text-sm text-gray-800 block text-center">
                            {line.salePrice > 0 ? line.salePrice.toFixed(2) : "—"}
                          </span>
                        )}
                      </td>

                      {/* Expiry date — col 3 */}
                      <td className="px-2 py-2">
                        {isDraft ? (
                          <input type="date"
                            min={new Date().toISOString().split("T")[0]}
                            ref={(el) => setCellRef(el, idx, COL_EXPIRY)}
                            className="w-full border rounded px-2 py-1 text-xs focus:outline-none focus:ring-2 focus:ring-blue-400"
                            value={line.expiryDate}
                            onChange={(e) => updateLine(idx, "expiryDate", e.target.value)}
                            onKeyDown={(e) => handleCellKey(e, idx, COL_EXPIRY)}
                          />
                        ) : (
                          <span className="text-xs text-gray-500 block text-center">{line.expiryDate || "—"}</span>
                        )}
                      </td>

                      <td className="px-2 py-2 text-sm font-semibold text-gray-800 text-center">{line.lineTotal.toFixed(2)}</td>
                      {isDraft && (
                        <td className="px-2 py-2 text-center">
                          <button onClick={() => removeLine(idx)} className="text-red-400 hover:text-red-600 text-lg leading-none font-bold" title="حذف السطر">×</button>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Totals footer */}
          <div className="border-t bg-gray-50 px-6 py-4 rounded-b-xl">
            <div className="flex justify-end">
              <div className="space-y-1.5 min-w-[260px]">
                <div className="flex justify-between text-sm text-gray-600">
                  <span>المجموع الفرعي</span>
                  <span className="font-medium">{subtotal.toFixed(2)} د.ل</span>
                </div>
                <div className="flex justify-between text-sm text-gray-600">
                  <span>الخصم</span>
                  <span className="font-medium text-red-600">{discount > 0 ? `-${discount.toFixed(2)}` : "0.00"} د.ل</span>
                </div>
                <div className="flex justify-between text-base font-bold text-gray-900 border-t pt-1.5">
                  <span>الإجمالي الصافي</span>
                  <span className="text-blue-700">{netTotal.toFixed(2)} د.ل</span>
                </div>
                {supplierInfo && (
                  <div className="flex justify-between text-xs text-gray-400 border-t pt-1">
                    <span>رصيد المورد بعد الترحيل</span>
                    <span className="text-red-500 font-medium">{(supplierInfo.balance + netTotal).toFixed(2)} د.ل</span>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Bottom action buttons */}
        {isDraft && (
          <div className="flex gap-3 pb-6">
            <button onClick={() => save()} disabled={saving || !supplierId}
              className="flex-1 bg-blue-600 text-white py-3 rounded-xl text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
              {saving ? "جاري الحفظ..." : "💾 حفظ مسودة"}
            </button>
            {!isNew && (
              <button onClick={postInvoice} disabled={posting || !lines.some((l) => l.itemId)}
                className="flex-1 bg-green-600 text-white py-3 rounded-xl text-sm font-medium hover:bg-green-700 disabled:opacity-50">
                {posting ? "جاري الترحيل..." : "✓ ترحيل الفاتورة وتحديث المخزون"}
              </button>
            )}
            <button onClick={() => router.push("/purchasing/invoices")}
              className="border px-6 py-3 rounded-xl text-sm text-gray-700 hover:bg-gray-50">
              إلغاء
            </button>
          </div>
        )}
      </div>

      {/* ── New Item Modal ── */}
      {newItemOpen && (
        <div className="fixed inset-0 bg-black/60 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-2xl shadow-2xl w-full max-w-xl max-h-[90vh] overflow-y-auto" dir="rtl">
            <div className="flex items-center justify-between px-6 py-4 border-b sticky top-0 bg-white rounded-t-2xl">
              <h2 className="text-lg font-bold text-gray-900">إضافة صنف جديد للمخزون</h2>
              <button onClick={() => setNewItemOpen(false)} className="text-gray-400 hover:text-gray-600 text-2xl leading-none">×</button>
            </div>

            <div className="px-6 py-5 space-y-4">
              {newItemError && (
                <div className="text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{newItemError}</div>
              )}

              <div className="grid grid-cols-2 gap-4">
                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">اسم الصنف (عربي) *</label>
                  <input
                    autoFocus
                    className="w-full border rounded-lg px-3 py-2 text-sm focus:ring-2 focus:ring-blue-400 focus:outline-none"
                    value={newItemForm.nameAr}
                    onChange={(e) => setNewItemForm({ ...newItemForm, nameAr: e.target.value, name: e.target.value })}
                    placeholder="مثال: حشو ضوئي"
                  />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">SKU / كود</label>
                  <input className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={newItemForm.sku}
                    onChange={(e) => setNewItemForm({ ...newItemForm, sku: e.target.value })}
                    placeholder="اختياري" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">الباركود</label>
                  <input className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={newItemForm.barcode}
                    onChange={(e) => setNewItemForm({ ...newItemForm, barcode: e.target.value })}
                    placeholder="اختياري" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">الفئة</label>
                  <select className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={newItemForm.categoryId}
                    onChange={(e) => setNewItemForm({ ...newItemForm, categoryId: e.target.value })}>
                    <option value="">— بدون فئة —</option>
                    {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">وحدة القياس</label>
                  <select className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={newItemForm.unitOfMeasureId}
                    onChange={(e) => setNewItemForm({ ...newItemForm, unitOfMeasureId: e.target.value })}>
                    <option value="">— بدون وحدة —</option>
                    {units.map((u) => <option key={u.id} value={u.id}>{u.name}{u.abbreviation ? ` (${u.abbreviation})` : ""}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">سعر الشراء</label>
                  <input type="number" step="0.01" min="0" className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={newItemForm.unitCost}
                    onChange={(e) => setNewItemForm({ ...newItemForm, unitCost: e.target.value })}
                    placeholder="0.00" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">سعر البيع</label>
                  <input type="number" step="0.01" min="0" className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={newItemForm.salePrice}
                    onChange={(e) => setNewItemForm({ ...newItemForm, salePrice: e.target.value })}
                    placeholder="0.00" />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">حد إعادة الطلب</label>
                  <input type="number" min="0" className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={newItemForm.reorderLevel}
                    onChange={(e) => setNewItemForm({ ...newItemForm, reorderLevel: e.target.value })} />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">كمية إعادة الطلب</label>
                  <input type="number" min="0" className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={newItemForm.reorderQuantity}
                    onChange={(e) => setNewItemForm({ ...newItemForm, reorderQuantity: e.target.value })} />
                </div>
                <div className="col-span-2 flex gap-6">
                  <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
                    <input type="checkbox" checked={newItemForm.isExpiryTracked}
                      onChange={(e) => setNewItemForm({ ...newItemForm, isExpiryTracked: e.target.checked })} className="rounded" />
                    تتبع تاريخ الانتهاء
                  </label>
                  <label className="flex items-center gap-2 text-sm text-gray-700 cursor-pointer">
                    <input type="checkbox" checked={newItemForm.allowNegativeStock}
                      onChange={(e) => setNewItemForm({ ...newItemForm, allowNegativeStock: e.target.checked })} className="rounded" />
                    السماح بالمخزون السالب
                  </label>
                </div>
                <div className="col-span-2">
                  <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
                  <textarea rows={2} className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={newItemForm.notes}
                    onChange={(e) => setNewItemForm({ ...newItemForm, notes: e.target.value })}
                    placeholder="اختياري" />
                </div>
              </div>
            </div>

            <div className="flex gap-3 px-6 pb-6">
              <button
                onClick={saveNewItem}
                disabled={newItemSaving || (!newItemForm.nameAr.trim() && !newItemForm.name.trim())}
                className="flex-1 bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
              >
                {newItemSaving ? "جاري الحفظ..." : "حفظ الصنف وإضافته للفاتورة"}
              </button>
              <button onClick={() => setNewItemOpen(false)} className="border px-6 py-2.5 rounded-lg text-sm text-gray-700 hover:bg-gray-50">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
