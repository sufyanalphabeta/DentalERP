"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface InvoiceItem {
  id: string;
  serviceName: string;
  serviceCode: string | null;
  quantity: number;
  unitPrice: number;
  discount: number;
  total: number;
}

interface InvoiceDetail {
  id: string;
  invoiceNumber: string;
  patientName: string;
  doctorName: string;
  status: string;
  subtotal: number;
  discountTotal: number;
  totalAmount: number;
  paidAmount: number;
  remaining: number;
  currency: string;
  notes: string | null;
  cancelledReason: string | null;
  createdAt: string;
  items: InvoiceItem[];
}

interface Vault {
  id: string;
  name: string;
  type: string;
}

const statusLabel: Record<string, { label: string; cls: string }> = {
  Draft: { label: "مسودة", cls: "bg-gray-100 text-gray-600" },
  Confirmed: { label: "مؤكدة", cls: "bg-blue-100 text-blue-700" },
  PartiallyPaid: { label: "مدفوعة جزئياً", cls: "bg-yellow-100 text-yellow-700" },
  Paid: { label: "مدفوعة", cls: "bg-green-100 text-green-700" },
  Cancelled: { label: "ملغاة", cls: "bg-red-100 text-red-600" },
};

export default function InvoiceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [invoice, setInvoice] = useState<InvoiceDetail | null>(null);
  const [vaults, setVaults] = useState<Vault[]>([]);
  const [loading, setLoading] = useState(true);
  const [showPayModal, setShowPayModal] = useState(false);
  const [payForm, setPayForm] = useState({ vaultId: "", amount: "", paymentMethod: "cash", notes: "" });
  const [paying, setPaying] = useState(false);
  const [payError, setPayError] = useState<string | null>(null);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState("");
  const [cancelling, setCancelling] = useState(false);

  useEffect(() => { load(); }, [id]);

  async function load() {
    setLoading(true);
    try {
      const [invRes, vaultRes] = await Promise.all([
        api.get<InvoiceDetail>(`/api/invoices/${id}`),
        api.get<Vault[]>("/api/treasury/vaults/balances"),
      ]);
      setInvoice(invRes.data);
      setVaults(vaultRes.data);
    } finally {
      setLoading(false);
    }
  }

  async function confirmInvoice() {
    await api.post(`/api/invoices/${id}/confirm`);
    load();
  }

  async function addPayment() {
    setPaying(true);
    setPayError(null);
    try {
      await api.post(`/api/invoices/${id}/payments`, {
        vaultId: payForm.vaultId,
        amount: parseFloat(payForm.amount),
        paymentMethod: payForm.paymentMethod,
        notes: payForm.notes || null,
      });
      setShowPayModal(false);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string } } };
      setPayError(err?.response?.data?.message ?? "حدث خطأ");
    } finally {
      setPaying(false);
    }
  }

  async function cancelInvoice() {
    setCancelling(true);
    try {
      await api.post(`/api/invoices/${id}/cancel`, { reason: cancelReason });
      setShowCancelModal(false);
      load();
    } finally {
      setCancelling(false);
    }
  }

  if (loading) return <div className="p-6 text-center text-gray-500">جاري التحميل...</div>;
  if (!invoice) return <div className="p-6 text-center text-red-500">الفاتورة غير موجودة</div>;

  const st = statusLabel[invoice.status] ?? { label: invoice.status, cls: "bg-gray-100 text-gray-600" };
  const canPay = invoice.status === "Confirmed" || invoice.status === "PartiallyPaid";
  const canCancel = invoice.status !== "Paid" && invoice.status !== "Cancelled";
  const canConfirm = invoice.status === "Draft";

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <div className="flex items-center justify-between mb-6">
        <div className="flex items-center gap-3">
          <button onClick={() => router.back()} className="text-gray-500 hover:text-gray-700">
            ← رجوع
          </button>
          <h1 className="text-2xl font-bold text-gray-900">{invoice.invoiceNumber}</h1>
          <span className={`inline-flex px-2 py-1 rounded-full text-xs font-medium ${st.cls}`}>{st.label}</span>
        </div>
        <div className="flex gap-2">
          {canConfirm && (
            <button onClick={confirmInvoice} className="bg-blue-600 text-white px-4 py-2 rounded-lg hover:bg-blue-700 text-sm">
              تأكيد الفاتورة
            </button>
          )}
          {canPay && (
            <button
              onClick={() => { setPayForm({ vaultId: vaults[0]?.id ?? "", amount: String(invoice.remaining), paymentMethod: "cash", notes: "" }); setShowPayModal(true); }}
              className="bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 text-sm"
            >
              تسجيل دفعة
            </button>
          )}
          {canCancel && (
            <button onClick={() => setShowCancelModal(true)} className="border border-red-500 text-red-600 px-4 py-2 rounded-lg hover:bg-red-50 text-sm">
              إلغاء الفاتورة
            </button>
          )}
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4 mb-6">
        <div className="bg-white rounded-xl shadow p-4">
          <div className="text-sm text-gray-500 mb-1">المريض</div>
          <div className="font-semibold text-gray-800">{invoice.patientName}</div>
        </div>
        <div className="bg-white rounded-xl shadow p-4">
          <div className="text-sm text-gray-500 mb-1">الطبيب</div>
          <div className="font-semibold text-gray-800">{invoice.doctorName}</div>
        </div>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden mb-6">
        <div className="px-5 py-4 border-b">
          <h2 className="font-semibold text-gray-700">بنود الفاتورة</h2>
        </div>
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs text-gray-500">الخدمة</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">الكمية</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">سعر الوحدة</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">الخصم</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">الإجمالي</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {invoice.items.map((item) => (
              <tr key={item.id}>
                <td className="px-4 py-3 text-sm text-gray-800">{item.serviceName}</td>
                <td className="px-4 py-3 text-sm text-gray-600">{item.quantity}</td>
                <td className="px-4 py-3 text-sm text-gray-600">{item.unitPrice.toFixed(2)}</td>
                <td className="px-4 py-3 text-sm text-gray-600">{item.discount.toFixed(2)}</td>
                <td className="px-4 py-3 text-sm font-medium text-gray-800">{item.total.toFixed(2)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="bg-white rounded-xl shadow p-5">
        <div className="space-y-2">
          <div className="flex justify-between text-sm text-gray-600">
            <span>المجموع الفرعي</span>
            <span>{invoice.subtotal.toFixed(2)} {invoice.currency}</span>
          </div>
          <div className="flex justify-between text-sm text-red-500">
            <span>إجمالي الخصم</span>
            <span>- {invoice.discountTotal.toFixed(2)} {invoice.currency}</span>
          </div>
          <div className="flex justify-between font-bold text-gray-900 border-t pt-2 text-base">
            <span>الإجمالي</span>
            <span>{invoice.totalAmount.toFixed(2)} {invoice.currency}</span>
          </div>
          <div className="flex justify-between text-sm text-green-700">
            <span>المدفوع</span>
            <span>{invoice.paidAmount.toFixed(2)} {invoice.currency}</span>
          </div>
          <div className="flex justify-between font-bold text-red-600 text-base">
            <span>المتبقي</span>
            <span>{invoice.remaining.toFixed(2)} {invoice.currency}</span>
          </div>
        </div>
      </div>

      {showPayModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6">
            <h2 className="text-lg font-bold mb-4">تسجيل دفعة</h2>
            {payError && <div className="mb-3 text-sm text-red-600 bg-red-50 p-3 rounded">{payError}</div>}
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الخزينة</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={payForm.vaultId} onChange={(e) => setPayForm({ ...payForm, vaultId: e.target.value })}>
                  {vaults.map((v) => <option key={v.id} value={v.id}>{v.name}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">المبلغ (المتبقي: {invoice.remaining.toFixed(2)})</label>
                <input type="number" min="0.01" step="0.01" max={invoice.remaining} className="w-full border rounded-lg px-3 py-2 text-sm" value={payForm.amount} onChange={(e) => setPayForm({ ...payForm, amount: e.target.value })} />
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">طريقة الدفع</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={payForm.paymentMethod} onChange={(e) => setPayForm({ ...payForm, paymentMethod: e.target.value })}>
                  <option value="cash">نقداً</option>
                  <option value="bank_transfer">تحويل بنكي</option>
                  <option value="card">بطاقة</option>
                  <option value="pos">نقطة بيع</option>
                  <option value="cheque">شيك</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" value={payForm.notes} onChange={(e) => setPayForm({ ...payForm, notes: e.target.value })} />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={addPayment} disabled={paying || !payForm.vaultId || !payForm.amount} className="flex-1 bg-green-600 text-white py-2 rounded-lg hover:bg-green-700 disabled:opacity-50 text-sm font-medium">
                {paying ? "جاري التسجيل..." : "تأكيد الدفع"}
              </button>
              <button onClick={() => setShowPayModal(false)} className="flex-1 border border-gray-300 text-gray-700 py-2 rounded-lg hover:bg-gray-50 text-sm">إلغاء</button>
            </div>
          </div>
        </div>
      )}

      {showCancelModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6">
            <h2 className="text-lg font-bold mb-4 text-red-600">إلغاء الفاتورة</h2>
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-1">سبب الإلغاء *</label>
              <textarea className="w-full border rounded-lg px-3 py-2 text-sm h-24 resize-none" value={cancelReason} onChange={(e) => setCancelReason(e.target.value)} />
            </div>
            <div className="flex gap-3">
              <button onClick={cancelInvoice} disabled={cancelling || !cancelReason.trim()} className="flex-1 bg-red-600 text-white py-2 rounded-lg hover:bg-red-700 disabled:opacity-50 text-sm font-medium">
                {cancelling ? "جاري الإلغاء..." : "تأكيد الإلغاء"}
              </button>
              <button onClick={() => setShowCancelModal(false)} className="flex-1 border border-gray-300 text-gray-700 py-2 rounded-lg hover:bg-gray-50 text-sm">تراجع</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
