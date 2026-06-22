"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { api } from "@/lib/api";

const CATEGORIES = [
  { value: "Medical",   label: "طبي" },
  { value: "Equipment", label: "معدات" },
  { value: "General",   label: "عام" },
  { value: "Lab",       label: "مختبر" },
  { value: "Radiology", label: "أشعة" },
  { value: "Pharma",    label: "دوائي" },
];

interface SupplierDetail {
  id: string;
  supplierCode: string;
  name: string;
  nameAr: string | null;
  category: string | null;
  contactPerson: string | null;
  phone: string | null;
  email: string | null;
  address: string | null;
  paymentTermsDays: number;
  creditLimit: number;
  isActive: boolean;
  notes: string | null;
  openingBalance: number;
  balance: number;
  createdAt: string;
  updatedAt: string | null;
}

interface StatementDto {
  supplierId: string;
  supplierName: string;
  openingBalance: number;
  totalPurchases: number;
  totalPayments: number;
  totalReturns: number;
  closingBalance: number;
  lines: StatementLine[];
}

interface StatementLine {
  date: string;
  type: string;
  reference: string;
  debit: number;
  credit: number;
  runningBalance: number;
}

const TYPE_LABELS: Record<string, string> = {
  GoodsReceipt: "استلام بضاعة",
  Payment: "دفعة سداد",
  Return: "مرتجع",
};

export default function SupplierDetailPage() {
  const { id } = useParams<{ id: string }>();
  const [supplier, setSupplier] = useState<SupplierDetail | null>(null);
  const [statement, setStatement] = useState<StatementDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [showEdit, setShowEdit] = useState(false);
  const [editForm, setEditForm] = useState<{
    name: string; nameAr: string; category: string; contactPerson: string;
    phone: string; email: string; address: string; notes: string;
    paymentTermsDays: string; creditLimit: string; openingBalance: string;
  } | null>(null);
  const [saving, setSaving] = useState(false);
  const [editError, setEditError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  const [from, setFrom] = useState(() => {
    const d = new Date();
    d.setMonth(d.getMonth() - 3);
    return d.toISOString().split("T")[0];
  });
  const [to, setTo] = useState(new Date().toISOString().split("T")[0]);

  useEffect(() => { load(); }, [id]);
  useEffect(() => { loadStatement(); }, [id, from, to]);

  async function load() {
    setLoading(true);
    try {
      const r = await api.get<SupplierDetail>(`/suppliers/${id}`);
      setSupplier(r.data);
    } finally {
      setLoading(false);
    }
  }

  async function loadStatement() {
    try {
      const r = await api.get<StatementDto>(`/suppliers/${id}/statement?from=${from}&to=${to}`);
      setStatement(r.data);
    } catch {
      setStatement(null);
    }
  }

  function openEdit() {
    if (!supplier) return;
    setEditForm({
      name: supplier.name,
      nameAr: supplier.nameAr ?? "",
      category: supplier.category ?? "",
      contactPerson: supplier.contactPerson ?? "",
      phone: supplier.phone ?? "",
      email: supplier.email ?? "",
      address: supplier.address ?? "",
      notes: supplier.notes ?? "",
      paymentTermsDays: String(supplier.paymentTermsDays),
      creditLimit: String(supplier.creditLimit),
      openingBalance: String(supplier.openingBalance),
    });
    setEditError(null);
    setShowEdit(true);
  }

  async function saveEdit() {
    if (!editForm) return;
    setSaving(true);
    setEditError(null);
    try {
      await api.put(`/suppliers/${id}`, {
        name: editForm.name.trim(),
        nameAr: editForm.nameAr || null,
        category: editForm.category || null,
        contactPerson: editForm.contactPerson || null,
        phone: editForm.phone || null,
        email: editForm.email || null,
        address: editForm.address || null,
        paymentTermsDays: parseInt(editForm.paymentTermsDays) || 30,
        creditLimit: parseFloat(editForm.creditLimit) || 0,
        openingBalance: parseFloat(editForm.openingBalance) || 0,
        notes: editForm.notes || null,
      });
      setShowEdit(false);
      setSuccess("تم حفظ التعديلات");
      setTimeout(() => setSuccess(null), 3000);
      load();
      loadStatement();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string; detail?: string; title?: string } } };
      setEditError(
        err?.response?.data?.error ??
        err?.response?.data?.detail ??
        err?.response?.data?.title ??
        "حدث خطأ أثناء الحفظ"
      );
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <div className="p-6 text-center text-gray-400">جاري التحميل...</div>;
  if (!supplier) return <div className="p-6 text-red-600">المورد غير موجود</div>;

  return (
    <div className="p-6" dir="rtl">
      {success && (
        <div className="fixed top-4 left-1/2 -translate-x-1/2 z-50 bg-green-600 text-white px-6 py-3 rounded-xl shadow-lg text-sm font-medium">
          ✓ {success}
        </div>
      )}

      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{supplier.name}</h1>
          <div className="flex items-center gap-2 mt-1">
            <span className="text-xs font-mono text-gray-400 bg-gray-100 px-2 py-0.5 rounded">{supplier.supplierCode}</span>
            {supplier.category && (
              <span className="text-xs bg-blue-50 text-blue-600 px-2 py-0.5 rounded">
                {CATEGORIES.find(c => c.value === supplier.category)?.label ?? supplier.category}
              </span>
            )}
            <span className={`text-xs px-2 py-0.5 rounded-full ${supplier.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"}`}>
              {supplier.isActive ? "نشط" : "غير نشط"}
            </span>
          </div>
        </div>
        <div className="flex gap-2">
          <Link href="/purchasing/suppliers" className="border px-3 py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-50">رجوع</Link>
          <button onClick={openEdit} className="border border-blue-300 text-blue-700 px-4 py-2 rounded-lg text-sm hover:bg-blue-50">تعديل البيانات</button>
          <Link href={`/purchasing/suppliers/${id}/statement`} className="border border-gray-300 text-gray-700 px-4 py-2 rounded-lg text-sm hover:bg-gray-50">كشف الحساب</Link>
          <Link href={`/purchasing/invoices/new?supplierId=${id}`} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">+ فاتورة مشتريات</Link>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-4 mb-6">
        {/* Info */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5 space-y-3">
          <h2 className="font-semibold text-gray-700 text-sm border-b pb-2">بيانات التواصل</h2>
          {supplier.contactPerson && <Row label="جهة الاتصال" value={supplier.contactPerson} />}
          {supplier.phone && <Row label="الهاتف" value={supplier.phone} />}
          {supplier.email && <Row label="البريد" value={supplier.email} />}
          {supplier.address && <Row label="العنوان" value={supplier.address} />}
          <Row label="شروط الدفع" value={`${supplier.paymentTermsDays} يوم`} />
          {supplier.creditLimit > 0 && <Row label="حد الائتمان" value={`${supplier.creditLimit.toFixed(2)} د.ل`} />}
          {supplier.notes && <Row label="ملاحظات" value={supplier.notes} />}
        </div>

        {/* Balance cards */}
        <div className="bg-white rounded-xl shadow-sm border border-blue-100 p-5 flex flex-col justify-center">
          <div className="text-xs text-gray-400 mb-1">الرصيد الافتتاحي</div>
          <div className="text-2xl font-bold text-blue-700">{supplier.openingBalance.toFixed(2)}</div>
          <div className="text-xs text-gray-400">د.ل</div>
        </div>
        <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5 flex flex-col justify-center">
          <div className="text-xs text-gray-400 mb-1">إجمالي المشتريات</div>
          <div className="text-2xl font-bold text-gray-700">{statement ? statement.totalPurchases.toFixed(2) : "—"}</div>
          <div className="text-xs text-gray-400">د.ل</div>
        </div>
        <div className="bg-white rounded-xl shadow-sm border border-red-100 p-5 flex flex-col justify-center">
          <div className="text-xs text-gray-400 mb-1">الرصيد المستحق (دائن)</div>
          <div className={`text-2xl font-bold ${supplier.balance > 0 ? "text-red-600" : "text-green-700"}`}>
            {supplier.balance.toFixed(2)}
          </div>
          <div className="text-xs text-gray-400">د.ل</div>
        </div>
      </div>

      {/* Statement */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100">
        <div className="px-5 py-4 border-b flex items-center justify-between flex-wrap gap-3">
          <h2 className="font-semibold text-gray-800">كشف الحساب</h2>
          <div className="flex gap-2">
            <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} className="border rounded-lg px-2 py-1 text-sm" />
            <span className="text-gray-400 self-center text-sm">إلى</span>
            <input type="date" value={to} onChange={(e) => setTo(e.target.value)} className="border rounded-lg px-2 py-1 text-sm" />
          </div>
        </div>

        {(!statement || statement.lines.length === 0) ? (
          <div className="p-8 text-center text-gray-400">لا توجد حركات في الفترة المحددة</div>
        ) : (
          <>
            {statement.openingBalance !== 0 && (
              <div className="px-5 py-2 bg-blue-50 border-b text-sm text-blue-700 font-medium">
                رصيد افتتاحي: {statement.openingBalance.toFixed(2)} د.ل
              </div>
            )}
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">التاريخ</th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">النوع</th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المرجع</th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">مدين</th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">دائن</th>
                  <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الرصيد</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {statement.lines.map((line, i) => (
                  <tr key={i} className="hover:bg-gray-50">
                    <td className="px-4 py-3 text-xs text-gray-500">{new Date(line.date).toLocaleDateString("ar-LY")}</td>
                    <td className="px-4 py-3 text-xs">
                      <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                        line.type === "Payment" ? "bg-green-100 text-green-700" :
                        line.type === "Return"  ? "bg-amber-100 text-amber-700" :
                                                   "bg-blue-100 text-blue-700"
                      }`}>
                        {TYPE_LABELS[line.type] ?? line.type}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-xs font-mono text-gray-500">{line.reference}</td>
                    <td className="px-4 py-3 text-sm text-red-600">{line.debit > 0 ? line.debit.toFixed(2) : "—"}</td>
                    <td className="px-4 py-3 text-sm text-green-700">{line.credit > 0 ? line.credit.toFixed(2) : "—"}</td>
                    <td className="px-4 py-3 text-sm font-semibold text-gray-800">{line.runningBalance.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
              <tfoot className="bg-gray-50">
                <tr>
                  <td colSpan={5} className="px-4 py-3 text-sm font-bold text-gray-700 text-left">الرصيد الختامي</td>
                  <td className={`px-4 py-3 text-sm font-bold ${statement.closingBalance > 0 ? "text-red-600" : "text-green-700"}`}>
                    {statement.closingBalance.toFixed(2)} د.ل
                  </td>
                </tr>
              </tfoot>
            </table>
          </>
        )}
      </div>

      {/* Edit Modal */}
      {showEdit && editForm && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-lg p-6 max-h-[90vh] overflow-y-auto" dir="rtl">
            <h2 className="text-lg font-bold mb-4">تعديل بيانات المورد</h2>

            {editError && (
              <div className="mb-3 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-3 py-2">{editError}</div>
            )}

            <div className="grid grid-cols-2 gap-3">
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">الاسم *</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={editForm.name}
                  onChange={(e) => setEditForm({ ...editForm, name: e.target.value })} />
              </div>
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">الاسم بالعربي</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={editForm.nameAr}
                  onChange={(e) => setEditForm({ ...editForm, nameAr: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الفئة</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={editForm.category}
                  onChange={(e) => setEditForm({ ...editForm, category: e.target.value })}>
                  <option value="">— بدون —</option>
                  {CATEGORIES.map(c => <option key={c.value} value={c.value}>{c.label}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">جهة الاتصال</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={editForm.contactPerson}
                  onChange={(e) => setEditForm({ ...editForm, contactPerson: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الهاتف</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={editForm.phone}
                  onChange={(e) => setEditForm({ ...editForm, phone: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">البريد الإلكتروني</label>
                <input type="email" className="w-full border rounded-lg px-3 py-2 text-sm" value={editForm.email}
                  onChange={(e) => setEditForm({ ...editForm, email: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">شروط الدفع (أيام)</label>
                <input type="number" min="1" className="w-full border rounded-lg px-3 py-2 text-sm" value={editForm.paymentTermsDays}
                  onChange={(e) => setEditForm({ ...editForm, paymentTermsDays: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">حد الائتمان</label>
                <input type="number" min="0" step="0.01" className="w-full border rounded-lg px-3 py-2 text-sm" value={editForm.creditLimit}
                  onChange={(e) => setEditForm({ ...editForm, creditLimit: e.target.value })} />
              </div>
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">العنوان</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={editForm.address}
                  onChange={(e) => setEditForm({ ...editForm, address: e.target.value })} />
              </div>
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  الرصيد الافتتاحي (د.ل)
                  <span className="text-xs text-amber-600 font-normal mr-2">— المبلغ المستحق قبل بدء التشغيل</span>
                </label>
                <input type="number" min="0" step="0.01" className="w-full border border-amber-300 rounded-lg px-3 py-2 text-sm bg-amber-50"
                  value={editForm.openingBalance}
                  onChange={(e) => setEditForm({ ...editForm, openingBalance: e.target.value })} />
              </div>
              <div className="col-span-2">
                <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
                <textarea className="w-full border rounded-lg px-3 py-2 text-sm" rows={2} value={editForm.notes}
                  onChange={(e) => setEditForm({ ...editForm, notes: e.target.value })} />
              </div>
            </div>

            <div className="flex gap-3 mt-5">
              <button onClick={saveEdit} disabled={saving || !editForm.name.trim()}
                className="flex-1 bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
                {saving ? "جاري الحفظ..." : "حفظ التعديلات"}
              </button>
              <button onClick={() => setShowEdit(false)}
                className="flex-1 border py-2.5 rounded-lg text-sm text-gray-700 hover:bg-gray-50">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function Row({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <div className="text-xs text-gray-400">{label}</div>
      <div className="text-sm text-gray-700">{value}</div>
    </div>
  );
}
