"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
import { useAuthStore } from "@/stores/authStore";

interface InstallmentPayment {
  id: string;
  installmentNum: number;
  dueDate: string;
  amount: number;
  status: string;
  paidAt: string | null;
  paymentMethod: string | null;
}

interface InstallmentPlan {
  id: string;
  invoiceId: string;
  invoiceNumber: string;
  patientId: string;
  patientName: string;
  totalAmount: number;
  installmentsCount: number;
  createdAt: string;
  installments: InstallmentPayment[];
}

interface Vault {
  id: string;
  name: string;
}

interface Invoice {
  id: string;
  invoiceNumber: string;
  patientName: string;
  patientId: string;
  totalAmount: number;
  remaining: number;
  status: string;
}

interface Patient {
  id: string;
  fullName: string;
}

const statusCls: Record<string, string> = {
  Pending: "bg-yellow-100 text-yellow-700",
  Paid:    "bg-green-100 text-green-700",
  Overdue: "bg-red-100 text-red-600",
};

const statusAr: Record<string, string> = {
  Pending: "قيد الانتظار",
  Paid:    "مدفوع",
  Overdue: "متأخر",
};

export default function InstallmentsPage() {
  const { user } = useAuthStore();
  const [plans, setPlans] = useState<InstallmentPlan[]>([]);
  const [vaults, setVaults] = useState<Vault[]>([]);
  const [loading, setLoading] = useState(true);
  const [expanded, setExpanded] = useState<string | null>(null);

  // Filters
  const [patientSearch, setPatientSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<"all" | "active" | "completed">("all");

  // Pay modal
  const [payModal, setPayModal] = useState<{ planId: string; num: number } | null>(null);
  const [payForm, setPayForm] = useState({ vaultId: "", paymentMethod: "cash" });
  const [paying, setPaying] = useState(false);

  // Create plan modal
  const [showCreate, setShowCreate] = useState(false);
  const [invoiceSearch, setInvoiceSearch] = useState("");
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null);
  const [createForm, setCreateForm] = useState({ installmentsCount: "3", startDate: "" });
  const [creating, setCreating] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const [plansRes, vaultRes] = await Promise.all([
        api.get<InstallmentPlan[]>("/installments/plans"),
        api.get<Vault[]>("/treasury/vaults/balances"),
      ]);
      setPlans(plansRes.data ?? []);
      setVaults(vaultRes.data ?? []);
    } finally {
      setLoading(false);
    }
  }

  async function payInstallment() {
    if (!payModal) return;
    setPaying(true);
    try {
      await api.post(`/installments/${payModal.planId}/pay/${payModal.num}`, {
        vaultId: payForm.vaultId,
        paymentMethod: payForm.paymentMethod,
      });
      setPayModal(null);
      load();
    } finally {
      setPaying(false);
    }
  }

  async function searchInvoices(q: string) {
    setInvoiceSearch(q);
    if (q.length < 2) return;
    try {
      const r = await api.get<{ items: Invoice[] }>(`/invoices?search=${encodeURIComponent(q)}&status=Confirmed&pageSize=15`);
      setInvoices(r.data.items ?? []);
    } catch { setInvoices([]); }
  }

  async function createPlan() {
    if (!selectedInvoice) return;
    setCreating(true);
    setCreateError(null);
    try {
      await api.post("/installments/plans", {
        invoiceId: selectedInvoice.id,
        patientId: selectedInvoice.patientId,
        totalAmount: selectedInvoice.remaining,
        installmentsCount: parseInt(createForm.installmentsCount),
        startDate: createForm.startDate ? new Date(createForm.startDate).toISOString() : new Date().toISOString(),
        createdByUserId: user?.userId ?? null,
      });
      setShowCreate(false);
      setSelectedInvoice(null);
      setInvoiceSearch("");
      setInvoices([]);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string } } };
      setCreateError(err?.response?.data?.message ?? "حدث خطأ أثناء إنشاء خطة التقسيط");
    } finally {
      setCreating(false);
    }
  }

  const filtered = plans.filter((plan) => {
    if (patientSearch && !plan.patientName.includes(patientSearch) && !plan.invoiceNumber.includes(patientSearch)) return false;
    if (statusFilter === "active") {
      const hasPending = plan.installments.some((i) => i.status !== "Paid");
      if (!hasPending) return false;
    }
    if (statusFilter === "completed") {
      const allPaid = plan.installments.every((i) => i.status === "Paid");
      if (!allPaid) return false;
    }
    return true;
  });

  if (loading) return <div className="p-6 text-center text-gray-500">جاري التحميل...</div>;

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">خطط التقسيط</h1>
        <button
          onClick={() => { setShowCreate(true); setSelectedInvoice(null); setInvoiceSearch(""); setInvoices([]); setCreateForm({ installmentsCount: "3", startDate: new Date().toISOString().slice(0, 10) }); setCreateError(null); }}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
        >
          + خطة تقسيط جديدة
        </button>
      </div>

      {/* Filters */}
      <div className="flex gap-3 mb-5 flex-wrap">
        <input
          type="text"
          placeholder="بحث باسم المريض أو رقم الفاتورة..."
          value={patientSearch}
          onChange={(e) => setPatientSearch(e.target.value)}
          className="border rounded-lg px-3 py-2 text-sm flex-1 min-w-[200px]"
        />
        <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value as typeof statusFilter)} className="border rounded-lg px-3 py-2 text-sm">
          <option value="all">كل الخطط</option>
          <option value="active">نشطة (بها أقساط متبقية)</option>
          <option value="completed">مكتملة</option>
        </select>
      </div>

      {filtered.length === 0 ? (
        <div className="text-center py-12 text-gray-400">لا توجد خطط تقسيط</div>
      ) : (
        <div className="space-y-4">
          {filtered.map((plan) => {
            const paid = plan.installments.filter((i) => i.status === "Paid").length;
            const isOpen = expanded === plan.id;
            const progress = plan.installmentsCount > 0 ? (paid / plan.installmentsCount) * 100 : 0;
            return (
              <div key={plan.id} className="bg-white rounded-xl shadow border border-gray-100">
                <button
                  onClick={() => setExpanded(isOpen ? null : plan.id)}
                  className="w-full px-5 py-4 flex items-center justify-between text-right"
                >
                  <div className="flex items-center gap-4">
                    <div>
                      <div className="text-sm font-semibold text-gray-800">{plan.patientName}</div>
                      <div className="text-xs text-gray-400">{plan.invoiceNumber} — {new Date(plan.createdAt).toLocaleDateString("ar-LY")}</div>
                    </div>
                  </div>
                  <div className="flex items-center gap-6">
                    <div className="text-right">
                      <div className="text-sm font-bold text-gray-700">{plan.totalAmount.toFixed(2)} د.ل</div>
                      <div className="text-xs text-gray-400">{paid}/{plan.installmentsCount} أقساط</div>
                    </div>
                    <div className="w-24 hidden sm:block">
                      <div className="h-1.5 bg-gray-100 rounded-full">
                        <div className="h-1.5 bg-blue-500 rounded-full" style={{ width: `${progress}%` }} />
                      </div>
                    </div>
                    <span className="text-gray-400">{isOpen ? "▲" : "▼"}</span>
                  </div>
                </button>

                {isOpen && (
                  <div className="border-t px-5 py-4">
                    <div className="space-y-2">
                      {plan.installments.map((inst) => (
                        <div key={inst.id} className="flex items-center justify-between py-2 border-b last:border-0">
                          <div className="flex items-center gap-3">
                            <span className="text-sm font-medium text-gray-700">القسط {inst.installmentNum}</span>
                            <span className={`text-xs px-2 py-0.5 rounded-full ${statusCls[inst.status] ?? "bg-gray-100 text-gray-600"}`}>
                              {statusAr[inst.status] ?? inst.status}
                            </span>
                          </div>
                          <div className="flex items-center gap-4">
                            <div className="text-right">
                              <div className="text-sm font-medium text-gray-800">{inst.amount.toFixed(2)} د.ل</div>
                              <div className="text-xs text-gray-400">{new Date(inst.dueDate).toLocaleDateString("ar")}</div>
                            </div>
                            {inst.status !== "Paid" && (
                              <button
                                onClick={() => { setPayForm({ vaultId: vaults[0]?.id ?? "", paymentMethod: "cash" }); setPayModal({ planId: plan.id, num: inst.installmentNum }); }}
                                className="text-xs bg-green-600 text-white px-3 py-1 rounded-lg hover:bg-green-700"
                              >
                                دفع
                              </button>
                            )}
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* Pay modal */}
      {payModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">تسديد القسط</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الخزينة</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={payForm.vaultId} onChange={(e) => setPayForm({ ...payForm, vaultId: e.target.value })}>
                  {vaults.map((v) => <option key={v.id} value={v.id}>{v.name}</option>)}
                </select>
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
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={payInstallment} disabled={paying || !payForm.vaultId} className="flex-1 bg-green-600 text-white py-2 rounded-lg hover:bg-green-700 disabled:opacity-50 text-sm font-medium">
                {paying ? "جاري التسديد..." : "تأكيد الدفع"}
              </button>
              <button onClick={() => setPayModal(null)} className="flex-1 border border-gray-300 text-gray-700 py-2 rounded-lg hover:bg-gray-50 text-sm">إلغاء</button>
            </div>
          </div>
        </div>
      )}

      {/* Create plan modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">خطة تقسيط جديدة</h2>
            {createError && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{createError}</div>}

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الفاتورة *</label>
                {selectedInvoice ? (
                  <div className="border rounded-lg px-3 py-2 bg-blue-50 flex items-center justify-between">
                    <div>
                      <div className="text-sm font-medium text-gray-800">{selectedInvoice.invoiceNumber} — {selectedInvoice.patientName}</div>
                      <div className="text-xs text-gray-500">المتبقي: {selectedInvoice.remaining.toFixed(2)} د.ل</div>
                    </div>
                    <button onClick={() => { setSelectedInvoice(null); setInvoiceSearch(""); setInvoices([]); }} className="text-xs text-red-500 hover:text-red-700">تغيير</button>
                  </div>
                ) : (
                  <div>
                    <input
                      type="text"
                      placeholder="ابحث برقم الفاتورة أو اسم المريض..."
                      value={invoiceSearch}
                      onChange={(e) => searchInvoices(e.target.value)}
                      className="w-full border rounded-lg px-3 py-2 text-sm"
                      autoFocus
                    />
                    {invoices.length > 0 && (
                      <div className="border rounded-lg mt-1 max-h-40 overflow-y-auto bg-white shadow">
                        {invoices.map((inv) => (
                          <button key={inv.id} onClick={() => { setSelectedInvoice(inv); setInvoiceSearch(""); setInvoices([]); }} className="w-full text-right px-3 py-2 hover:bg-blue-50 text-sm border-b last:border-0">
                            <div className="font-medium text-gray-800">{inv.invoiceNumber} — {inv.patientName}</div>
                            <div className="text-xs text-gray-500">المتبقي: {inv.remaining.toFixed(2)} د.ل</div>
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">عدد الأقساط *</label>
                  <select className="w-full border rounded-lg px-3 py-2 text-sm" value={createForm.installmentsCount} onChange={(e) => setCreateForm({ ...createForm, installmentsCount: e.target.value })}>
                    {[2, 3, 4, 5, 6, 8, 10, 12].map((n) => <option key={n} value={n}>{n} أقساط</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">تاريخ البداية *</label>
                  <input type="date" className="w-full border rounded-lg px-3 py-2 text-sm" value={createForm.startDate} onChange={(e) => setCreateForm({ ...createForm, startDate: e.target.value })} />
                </div>
              </div>

              {selectedInvoice && (
                <div className="bg-gray-50 rounded-lg p-3 text-sm space-y-1">
                  <div className="flex justify-between">
                    <span className="text-gray-500">إجمالي التقسيط</span>
                    <span className="font-semibold">{selectedInvoice.remaining.toFixed(2)} د.ل</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500">قيمة كل قسط (تقريباً)</span>
                    <span className="font-semibold">{(selectedInvoice.remaining / parseInt(createForm.installmentsCount || "1")).toFixed(2)} د.ل</span>
                  </div>
                </div>
              )}
            </div>

            <div className="flex gap-3 mt-5">
              <button
                onClick={createPlan}
                disabled={creating || !selectedInvoice || !createForm.startDate}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50 font-medium"
              >
                {creating ? "جاري الإنشاء..." : "إنشاء الخطة"}
              </button>
              <button onClick={() => setShowCreate(false)} className="flex-1 border border-gray-300 text-gray-700 py-2 rounded-lg hover:bg-gray-50 text-sm">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
