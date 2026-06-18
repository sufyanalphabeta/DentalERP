"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

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

const statusCls: Record<string, string> = {
  Pending: "bg-yellow-100 text-yellow-700",
  Paid: "bg-green-100 text-green-700",
  Overdue: "bg-red-100 text-red-600",
};

const statusAr: Record<string, string> = {
  Pending: "Ù‚ÙŠØ¯ Ø§Ù„Ø§Ù†ØªØ¸Ø§Ø±",
  Paid: "Ù…Ø¯ÙÙˆØ¹",
  Overdue: "Ù…ØªØ£Ø®Ø±",
};

export default function InstallmentsPage() {
  const [plans, setPlans] = useState<InstallmentPlan[]>([]);
  const [vaults, setVaults] = useState<Vault[]>([]);
  const [loading, setLoading] = useState(true);
  const [expanded, setExpanded] = useState<string | null>(null);
  const [payModal, setPayModal] = useState<{ planId: string; num: number } | null>(null);
  const [payForm, setPayForm] = useState({ vaultId: "", paymentMethod: "cash" });
  const [paying, setPaying] = useState(false);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const [plansRes, vaultRes] = await Promise.all([
        api.get<InstallmentPlan[]>("/api/installments/plans"),
        api.get<Vault[]>("/api/treasury/vaults/balances"),
      ]);
      setPlans(plansRes.data);
      setVaults(vaultRes.data);
    } finally {
      setLoading(false);
    }
  }

  async function payInstallment() {
    if (!payModal) return;
    setPaying(true);
    try {
      await api.post(`/api/installments/${payModal.planId}/pay/${payModal.num}`, {
        vaultId: payForm.vaultId,
        paymentMethod: payForm.paymentMethod,
      });
      setPayModal(null);
      load();
    } finally {
      setPaying(false);
    }
  }

  if (loading) return <div className="p-6 text-center text-gray-500">Ø¬Ø§Ø±ÙŠ Ø§Ù„ØªØ­Ù…ÙŠÙ„...</div>;

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Ø®Ø·Ø· Ø§Ù„ØªÙ‚Ø³ÙŠØ·</h1>
      </div>

      {plans.length === 0 ? (
        <div className="text-center py-12 text-gray-400">Ù„Ø§ ØªÙˆØ¬Ø¯ Ø®Ø·Ø· ØªÙ‚Ø³ÙŠØ·</div>
      ) : (
        <div className="space-y-4">
          {plans.map((plan) => {
            const paid = plan.installments.filter((i) => i.status === "Paid").length;
            const isOpen = expanded === plan.id;
            return (
              <div key={plan.id} className="bg-white rounded-xl shadow">
                <button
                  onClick={() => setExpanded(isOpen ? null : plan.id)}
                  className="w-full px-5 py-4 flex items-center justify-between text-right"
                >
                  <div className="flex items-center gap-4">
                    <div>
                      <div className="text-sm font-semibold text-gray-800">{plan.patientName}</div>
                      <div className="text-xs text-gray-400">{plan.invoiceNumber}</div>
                    </div>
                  </div>
                  <div className="flex items-center gap-6">
                    <div className="text-right">
                      <div className="text-sm font-bold text-gray-700">{plan.totalAmount.toFixed(2)} Ø¯.Ù„</div>
                      <div className="text-xs text-gray-400">{paid}/{plan.installmentsCount} Ø£Ù‚Ø³Ø§Ø·</div>
                    </div>
                    <span className="text-gray-400">{isOpen ? "â–²" : "â–¼"}</span>
                  </div>
                </button>

                {isOpen && (
                  <div className="border-t px-5 py-4">
                    <div className="space-y-2">
                      {plan.installments.map((inst) => (
                        <div key={inst.id} className="flex items-center justify-between py-2 border-b last:border-0">
                          <div className="flex items-center gap-3">
                            <span className="text-sm font-medium text-gray-700">Ø§Ù„Ù‚Ø³Ø· {inst.installmentNum}</span>
                            <span className={`text-xs px-2 py-0.5 rounded-full ${statusCls[inst.status]}`}>
                              {statusAr[inst.status]}
                            </span>
                          </div>
                          <div className="flex items-center gap-4">
                            <div className="text-right">
                              <div className="text-sm font-medium text-gray-800">{inst.amount.toFixed(2)} Ø¯.Ù„</div>
                              <div className="text-xs text-gray-400">{new Date(inst.dueDate).toLocaleDateString("ar")}</div>
                            </div>
                            {inst.status !== "Paid" && (
                              <button
                                onClick={() => { setPayForm({ vaultId: vaults[0]?.id ?? "", paymentMethod: "cash" }); setPayModal({ planId: plan.id, num: inst.installmentNum }); }}
                                className="text-xs bg-green-600 text-white px-3 py-1 rounded-lg hover:bg-green-700"
                              >
                                Ø¯ÙØ¹
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

      {payModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6">
            <h2 className="text-lg font-bold mb-4">ØªØ³Ø¯ÙŠØ¯ Ø§Ù„Ù‚Ø³Ø·</h2>
            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Ø§Ù„Ø®Ø²ÙŠÙ†Ø©</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={payForm.vaultId} onChange={(e) => setPayForm({ ...payForm, vaultId: e.target.value })}>
                  {vaults.map((v) => <option key={v.id} value={v.id}>{v.name}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Ø·Ø±ÙŠÙ‚Ø© Ø§Ù„Ø¯ÙØ¹</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={payForm.paymentMethod} onChange={(e) => setPayForm({ ...payForm, paymentMethod: e.target.value })}>
                  <option value="cash">Ù†Ù‚Ø¯Ø§Ù‹</option>
                  <option value="bank_transfer">ØªØ­ÙˆÙŠÙ„ Ø¨Ù†ÙƒÙŠ</option>
                  <option value="card">Ø¨Ø·Ø§Ù‚Ø©</option>
                  <option value="pos">Ù†Ù‚Ø·Ø© Ø¨ÙŠØ¹</option>
                  <option value="cheque">Ø´ÙŠÙƒ</option>
                </select>
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={payInstallment} disabled={paying || !payForm.vaultId} className="flex-1 bg-green-600 text-white py-2 rounded-lg hover:bg-green-700 disabled:opacity-50 text-sm font-medium">
                {paying ? "Ø¬Ø§Ø±ÙŠ Ø§Ù„ØªØ³Ø¯ÙŠØ¯..." : "ØªØ£ÙƒÙŠØ¯ Ø§Ù„Ø¯ÙØ¹"}
              </button>
              <button onClick={() => setPayModal(null)} className="flex-1 border border-gray-300 text-gray-700 py-2 rounded-lg hover:bg-gray-50 text-sm">Ø¥Ù„ØºØ§Ø¡</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
