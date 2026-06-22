"use client";

import { useEffect, useRef, useState, useCallback } from "react";
import { useParams, useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface Supplier { id: string; name: string; computedBalance: number; }
interface ItemSearchResult {
  id: string; itemCode: string; name: string;
  barcode: string | null; unitCost: number; salePrice: number; kind: string;
  currentStock: number;
}
interface ReturnLine {
  _key: string; itemId: string; itemCode: string; itemName: string;
  barcode: string; quantity: number; unitCost: number; lineTotal: number;
}
interface ReturnDetail {
  id: string; returnNumber: string; supplierId: string; supplierName: string;
  returnDate: string; reason: string; status: string; totalAmount: number;
  notes: string | null;
  items: Array<{ id: string; itemId: string; itemName: string; itemCode: string; quantity: number; unitCost: number; totalCost: number; }>;
}

const STATUS_AR: Record<string, string> = { Draft: "مسودة", Confirmed: "مؤكد", Completed: "مكتمل", Cancelled: "ملغى" };
const STATUS_CLS: Record<string, string> = {
  Draft: "text-amber-700 bg-amber-50", Confirmed: "text-green-700 bg-green-50",
  Completed: "text-blue-700 bg-blue-50", Cancelled: "text-red-600 bg-red-50 line-through",
};

const COL_QTY = 0, COL_COST = 1;

export default function PurchaseReturnPage() {
  const params = useParams<{ id?: string }>();
  const id = params?.id ?? "new";
  const router = useRouter();
  const isNew = id === "new";

  const [detail, setDetail] = useState<ReturnDetail | null>(null);
  const [supplierId, setSupplierId] = useState("");
  const [supplierBalance, setSupplierBalance] = useState(0);
  const [returnDate, setReturnDate] = useState(new Date().toISOString().split("T")[0]);
  const [reason, setReason] = useState("");
  const [notes, setNotes] = useState("");
  const [lines, setLines] = useState<ReturnLine[]>([]);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);

  // Item search above table
  const [itemQuery, setItemQuery] = useState("");
  const [itemResults, setItemResults] = useState<ItemSearchResult[]>([]);
  const [itemLoading, setItemLoading] = useState(false);
  const [itemDropOpen, setItemDropOpen] = useState(false);
  const searchContainerRef = useRef<HTMLDivElement>(null);
  const searchInputRef = useRef<HTMLInputElement>(null);

  // Cell refs for Enter-key navigation [row][col]
  const cellRefs = useRef<(HTMLInputElement | null)[][]>([]);
  const setCellRef = useCallback((el: HTMLInputElement | null, row: number, col: number) => {
    if (!cellRefs.current[row]) cellRefs.current[row] = [];
    cellRefs.current[row][col] = el;
  }, []);

  const [loading, setLoading] = useState(!isNew);
  const [saving, setSaving] = useState(false);
  const [confirming, setConfirming] = useState(false);
  const [cancelling, setCancelling] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const totalAmount = lines.reduce((s, l) => s + l.lineTotal, 0);

  useEffect(() => {
    api.get<{ suppliers: Supplier[] }>("/suppliers?pageSize=500&activeOnly=true")
      .then((r) => setSuppliers(r.data.suppliers ?? []))
      .catch(() => {});
  }, []);

  useEffect(() => { if (!isNew) loadExisting(); }, [id]);

  async function loadExisting() {
    setLoading(true);
    try {
      const r = await api.get<ReturnDetail>(`/purchasing/purchase-returns/${id}`);
      const d = r.data;
      setDetail(d); setSupplierId(d.supplierId);
      setReturnDate(d.returnDate); setReason(d.reason); setNotes(d.notes ?? "");
      setLines(d.items.map((item) => ({
        _key: crypto.randomUUID(), itemId: item.itemId, itemCode: item.itemCode,
        itemName: item.itemName, barcode: "", quantity: item.quantity,
        unitCost: item.unitCost, lineTotal: item.totalCost,
      })));
    } catch { setError("لم يتم العثور على المردود"); }
    finally { setLoading(false); }
  }

  useEffect(() => {
    if (!supplierId) { setSupplierBalance(0); return; }
    api.get<{ balance: number }>(`/suppliers/${supplierId}/balance`)
      .then((r) => setSupplierBalance(r.data.balance))
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

  // Close dropdown on click outside
  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (searchContainerRef.current && !searchContainerRef.current.contains(e.target as Node))
        setItemDropOpen(false);
    }
    document.addEventListener("mousedown", handleClick);
    return () => document.removeEventListener("mousedown", handleClick);
  }, []);

  function addItemFromSearch(item: ItemSearchResult) {
    const line: ReturnLine = {
      _key: crypto.randomUUID(), itemId: item.id, itemCode: item.itemCode,
      itemName: item.name, barcode: item.barcode ?? "",
      quantity: 1, unitCost: item.unitCost, lineTotal: item.unitCost,
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
    setItemQuery(""); setItemDropOpen(false);
  }

  function handleCellKey(e: React.KeyboardEvent, rowIdx: number, colIdx: number) {
    if (e.key !== "Enter") return;
    e.preventDefault();
    if (colIdx < COL_COST) {
      cellRefs.current[rowIdx]?.[colIdx + 1]?.focus();
      cellRefs.current[rowIdx]?.[colIdx + 1]?.select();
    } else {
      const nextRow = rowIdx + 1;
      if (cellRefs.current[nextRow]?.[COL_QTY]) {
        cellRefs.current[nextRow][COL_QTY]?.focus();
        cellRefs.current[nextRow][COL_QTY]?.select();
      } else {
        searchInputRef.current?.focus();
      }
    }
  }

  function updateLine(idx: number, field: keyof ReturnLine, value: string | number) {
    setLines((prev) => {
      const next = [...prev];
      const line = { ...next[idx], [field]: value };
      line.lineTotal = line.quantity * line.unitCost;
      next[idx] = line;
      return next;
    });
  }

  function removeLine(idx: number) { setLines((prev) => prev.filter((_, i) => i !== idx)); }

  async function save() {
    if (!supplierId) { setError("يرجى اختيار المورد"); return; }
    if (!reason.trim()) { setError("يرجى إدخال سبب المردود"); return; }
    const validLines = lines.filter((l) => l.itemId && l.quantity > 0);
    if (validLines.length === 0) { setError("أضف صنفاً واحداً على الأقل"); return; }
    setSaving(true); setError(null);
    try {
      const payload = {
        supplierId, returnDate, reason, notes: notes || null, createdById: null,
        items: validLines.map((l) => ({ itemId: l.itemId, quantity: l.quantity, unitCost: l.unitCost, batchId: null })),
      };
      const r = await api.post<{ id: string }>("/purchasing/purchase-returns", payload);
      setSuccess("تم حفظ المردود كمسودة");
      setTimeout(() => setSuccess(null), 3000);
      router.replace(`/purchasing/returns/${r.data.id}`);
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string; detail?: string } } };
      setError(err?.response?.data?.error ?? err?.response?.data?.detail ?? "حدث خطأ");
    } finally { setSaving(false); }
  }

  async function confirm() {
    if (!detail || detail.status !== "Draft") return;
    setConfirming(true); setError(null);
    try {
      await api.post(`/purchasing/purchase-returns/${id}/confirm`, { confirmedById: null });
      setSuccess("تم تأكيد المردود — تم خصم الكميات من المخزون وتعديل رصيد المورد");
      setTimeout(() => { setSuccess(null); loadExisting(); }, 3000);
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string; detail?: string } } };
      setError(err?.response?.data?.error ?? err?.response?.data?.detail ?? "حدث خطأ أثناء التأكيد");
    } finally { setConfirming(false); }
  }

  async function cancelReturn() {
    if (!detail) return;
    if (!window.confirm(`هل تريد إلغاء المردود ${detail.returnNumber}؟${detail.status === "Confirmed" ? "\n\nتحذير: سيتم عكس خصم المخزون." : ""}`)) return;
    setCancelling(true); setError(null);
    try {
      await api.post(`/purchasing/purchase-returns/${id}/cancel`, { cancelledById: null });
      setSuccess("تم إلغاء المردود بنجاح");
      setTimeout(() => { setSuccess(null); loadExisting(); }, 2500);
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ أثناء الإلغاء");
    } finally { setCancelling(false); }
  }

  async function deleteReturn() {
    if (!detail) return;
    if (!window.confirm(`هل تريد حذف المردود ${detail.returnNumber}؟ لا يمكن التراجع عن هذا الإجراء.`)) return;
    setDeleting(true); setError(null);
    try {
      await api.delete(`/purchasing/purchase-returns/${id}`);
      setSuccess("تم حذف المردود");
      setTimeout(() => router.push("/purchasing/returns"), 1500);
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ أثناء الحذف");
    } finally { setDeleting(false); }
  }

  const isDraft = isNew || detail?.status === "Draft";
  if (loading) return <div className="p-6 text-center text-gray-400">جاري التحميل...</div>;

  return (
    <div className="min-h-screen bg-gray-100" dir="rtl">
      {success && (
        <div className="fixed top-4 left-1/2 -translate-x-1/2 z-50 bg-green-600 text-white px-6 py-3 rounded-xl shadow-lg text-sm font-medium">
          ✓ {success}
        </div>
      )}

      {/* Header */}
      <div className="bg-white border-b shadow-sm px-6 py-3 flex items-center justify-between sticky top-0 z-10">
        <div className="flex items-center gap-4">
          <button onClick={() => router.push("/purchasing/returns")} className="text-gray-500 hover:text-gray-700 text-sm">← رجوع</button>
          <div>
            <div className="flex items-center gap-2">
              <span className="text-lg font-bold text-gray-800">
                {isNew ? "مردود مشتريات جديد" : (detail?.returnNumber ?? "...")}
              </span>
              {detail && (
                <span className={`text-xs px-2 py-0.5 rounded-full font-medium ${STATUS_CLS[detail.status] ?? ""}`}>
                  {STATUS_AR[detail.status] ?? detail.status}
                </span>
              )}
            </div>
            {supplierBalance > 0 && (
              <div className="text-xs text-gray-400 mt-0.5">
                رصيد المورد الحالي: <span className="text-red-500 font-medium">{supplierBalance.toFixed(2)} د.ل</span>
                {totalAmount > 0 && <span className="mr-2 text-green-600">• بعد المردود: {Math.max(0, supplierBalance - totalAmount).toFixed(2)} د.ل</span>}
              </div>
            )}
          </div>
        </div>
        <div className="flex gap-2">
          {isDraft && isNew && (
            <button onClick={save} disabled={saving} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">
              {saving ? "جاري الحفظ..." : "💾 حفظ مسودة"}
            </button>
          )}
          {!isNew && isDraft && (
            <button onClick={confirm} disabled={confirming || lines.filter((l) => l.itemId).length === 0}
              className="bg-green-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-green-700 disabled:opacity-50">
              {confirming ? "جاري التأكيد..." : "✓ تأكيد المردود"}
            </button>
          )}
          {!isNew && isDraft && (
            <button onClick={deleteReturn} disabled={deleting}
              className="bg-red-50 text-red-600 border border-red-200 px-3 py-2 rounded-lg text-sm hover:bg-red-100 disabled:opacity-50">
              {deleting ? "..." : "🗑 حذف"}
            </button>
          )}
          {detail?.status === "Confirmed" && (
            <>
              <span className="text-green-700 bg-green-50 px-4 py-2 rounded-lg text-sm font-medium border border-green-200">✓ تم خصم المخزون</span>
              <button onClick={cancelReturn} disabled={cancelling}
                className="bg-red-50 text-red-600 border border-red-200 px-3 py-2 rounded-lg text-sm hover:bg-red-100 disabled:opacity-50">
                {cancelling ? "جاري الإلغاء..." : "↩ إلغاء المردود"}
              </button>
            </>
          )}
          {detail?.status === "Cancelled" && (
            <span className="text-red-600 bg-red-50 px-4 py-2 rounded-lg text-sm font-medium border border-red-200">✗ ملغى</span>
          )}
        </div>
      </div>

      <div className="max-w-7xl mx-auto p-4 space-y-4">
        {error && <div className="bg-red-50 border border-red-200 text-red-700 text-sm rounded-xl px-4 py-3">{error}</div>}

        {/* Return header form */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <div className="col-span-2">
              <label className="block text-xs font-medium text-gray-500 mb-1">المورد *</label>
              <select value={supplierId} onChange={(e) => setSupplierId(e.target.value)} disabled={!isDraft || !isNew}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-50">
                <option value="">— اختر المورد —</option>
                {suppliers.map((s) => <option key={s.id} value={s.id}>{s.name}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1">تاريخ المردود</label>
              <input type="date" value={returnDate} onChange={(e) => setReturnDate(e.target.value)} disabled={!isDraft || !isNew}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-50" />
            </div>
            <div>
              <label className="block text-xs font-medium text-gray-500 mb-1">سبب المردود *</label>
              <input value={reason} onChange={(e) => setReason(e.target.value)} disabled={!isDraft || !isNew}
                placeholder="مثال: بضاعة تالفة"
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-50" />
            </div>
            <div className="col-span-2 md:col-span-4">
              <label className="block text-xs font-medium text-gray-500 mb-1">ملاحظات</label>
              <input value={notes} onChange={(e) => setNotes(e.target.value)} disabled={!isDraft || !isNew}
                placeholder="ملاحظات اختيارية..."
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400 disabled:bg-gray-50" />
            </div>
          </div>
        </div>

        {/* Items section */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100">
          <div className="flex items-center justify-between px-4 py-3 border-b bg-gray-50 rounded-t-xl">
            <span className="text-sm font-semibold text-gray-700">الأصناف المرتجعة ({lines.filter((l) => l.itemId).length})</span>
          </div>

          {/* Search bar above table */}
          {isDraft && isNew && (
            <div className="px-4 py-3 border-b bg-blue-50/40" ref={searchContainerRef} style={{ position: "relative" }}>
              <label className="block text-xs font-medium text-gray-500 mb-1.5">ابحث عن صنف لإضافته للمردود</label>
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
                            <div className="text-xs text-gray-400 mt-0.5">{item.itemCode}{item.barcode ? ` • ${item.barcode}` : ""}</div>
                          </div>
                          <div className="text-left shrink-0">
                            <div className="text-sm text-blue-700 font-bold whitespace-nowrap">{item.unitCost.toFixed(2)} د.ل</div>
                            <div className={`text-xs mt-0.5 ${item.currentStock > 0 ? "text-green-600" : "text-red-500"}`}>
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
              {isDraft && isNew ? "ابحث عن صنف أعلاه لإضافته" : "لا توجد أصناف"}
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full text-sm">
                <thead className="bg-gray-50 border-b">
                  <tr>
                    <th className="px-2 py-2 text-right text-xs text-gray-500 w-8">#</th>
                    <th className="px-2 py-2 text-right text-xs text-gray-500 min-w-[200px]">اسم الصنف</th>
                    <th className="px-2 py-2 text-center text-xs text-gray-500 w-28">الكمية</th>
                    <th className="px-2 py-2 text-center text-xs text-gray-500 w-28">سعر الوحدة</th>
                    <th className="px-2 py-2 text-center text-xs text-gray-500 w-28">الإجمالي</th>
                    {isDraft && isNew && <th className="px-2 py-2 w-8"></th>}
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {lines.map((line, idx) => (
                    <tr key={line._key} className="hover:bg-gray-50/50">
                      <td className="px-2 py-2 text-xs text-gray-400 text-center">{idx + 1}</td>
                      <td className="px-2 py-2">
                        <div className="text-sm font-medium text-gray-800">{line.itemName || "—"}</div>
                        {line.itemCode && <div className="text-xs text-gray-400 mt-0.5">{line.itemCode}</div>}
                      </td>

                      {/* Quantity — col 0 */}
                      <td className="px-2 py-2">
                        {isDraft && isNew ? (
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

                      {/* Unit cost — col 1 */}
                      <td className="px-2 py-2">
                        {isDraft && isNew ? (
                          <input type="number" min="0" step="0.01"
                            ref={(el) => setCellRef(el, idx, COL_COST)}
                            className="w-full border rounded px-2 py-1 text-sm text-center focus:outline-none focus:ring-2 focus:ring-blue-400"
                            value={line.unitCost}
                            onChange={(e) => updateLine(idx, "unitCost", parseFloat(e.target.value) || 0)}
                            onKeyDown={(e) => handleCellKey(e, idx, COL_COST)}
                          />
                        ) : (
                          <span className="text-sm text-gray-800 block text-center">{line.unitCost.toFixed(2)}</span>
                        )}
                      </td>

                      <td className="px-2 py-2 text-sm font-semibold text-gray-800 text-center">{line.lineTotal.toFixed(2)}</td>
                      {isDraft && isNew && (
                        <td className="px-2 py-2 text-center">
                          <button onClick={() => removeLine(idx)} className="text-red-400 hover:text-red-600 text-lg leading-none font-bold">×</button>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {/* Total */}
          <div className="border-t bg-gray-50 px-6 py-4 rounded-b-xl">
            <div className="flex justify-end">
              <div className="space-y-1.5 min-w-[260px]">
                <div className="flex justify-between text-base font-bold text-gray-900">
                  <span>إجمالي قيمة المردود</span>
                  <span className="text-red-600">{totalAmount.toFixed(2)} د.ل</span>
                </div>
                {supplierBalance > 0 && totalAmount > 0 && (
                  <div className="flex justify-between text-xs text-gray-400 border-t pt-1">
                    <span>رصيد المورد بعد التأكيد</span>
                    <span className="text-green-600 font-medium">{Math.max(0, supplierBalance - totalAmount).toFixed(2)} د.ل</span>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>

        {/* Bottom actions */}
        {isNew && (
          <div className="flex gap-3 pb-6">
            <button onClick={save} disabled={saving || !supplierId || !reason}
              className="flex-1 bg-blue-600 text-white py-3 rounded-xl text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
              {saving ? "جاري الحفظ..." : "💾 حفظ المردود"}
            </button>
            <button onClick={() => router.push("/purchasing/returns")} className="border px-6 py-3 rounded-xl text-sm text-gray-700 hover:bg-gray-50">إلغاء</button>
          </div>
        )}
        {!isNew && isDraft && (
          <div className="flex gap-3 pb-6">
            <button onClick={confirm} disabled={confirming || lines.filter((l) => l.itemId).length === 0}
              className="flex-1 bg-green-600 text-white py-3 rounded-xl text-sm font-medium hover:bg-green-700 disabled:opacity-50">
              {confirming ? "جاري التأكيد..." : "✓ تأكيد المردود وخصم المخزون"}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}
