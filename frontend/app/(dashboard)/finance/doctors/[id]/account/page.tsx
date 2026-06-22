"use client";

import { useEffect, useState } from "react";
import { useParams, useSearchParams } from "next/navigation";
import { api } from "@/lib/api";

interface CommissionRecord {
  id: string;
  invoiceId: string;
  invoiceNumber: string;
  paymentId: string;
  commissionMethod: string;
  baseAmount: number;
  commissionRate: number;
  commissionAmount: number;
  isPaid: boolean;
  paidAt: string | null;
  createdAt: string;
}

interface DoctorAccount {
  doctorId: string;
  doctorName: string;
  commissionMethod: string;
  defaultCommissionValue: number;
  totalUnpaid: number;
  totalPaid: number;
  commissions: CommissionRecord[];
}

interface Vault {
  id: string;
  name: string;
}

const methodAr: Record<string, string> = {
  percentage_of_service: "نسبة من الخدمة",
  fixed_amount: "مبلغ ثابت",
  percentage_of_net_service: "نسبة من صافي الخدمة",
};

export default function DoctorAccountPage() {
  const { id } = useParams<{ id: string }>();
  const searchParams = useSearchParams();
  const [account, setAccount] = useState<DoctorAccount | null>(null);
  const [vaults, setVaults] = useState<Vault[]>([]);
  const [loading, setLoading] = useState(true);
  const [payModal, setPayModal] = useState<string | null>(null);
  const [selectedVault, setSelectedVault] = useState("");
  const [paying, setPaying] = useState(false);
  const [tab, setTab] = useState<"unpaid" | "paid">("unpaid");

  useEffect(() => { load(); }, [id, searchParams]);

  async function load() {
    setLoading(true);
    const dateFrom = searchParams.get("dateFrom");
    const dateTo = searchParams.get("dateTo");
    const params = new URLSearchParams();
    if (dateFrom) params.set("from", dateFrom);
    if (dateTo) params.set("to", dateTo);
    const qs = params.toString();
    try {
      const [accRes, vaultRes] = await Promise.all([
        api.get<DoctorAccount>(`/treasury/doctors/${id}/account${qs ? `?${qs}` : ""}`),
        api.get<Vault[]>("/treasury/vaults/balances"),
      ]);
      setAccount(accRes.data);
      setVaults(vaultRes.data);
    } finally {
      setLoading(false);
    }
  }

  async function payCommission(commissionId: string) {
    setPaying(true);
    try {
      await api.post(`/treasury/commissions/${commissionId}/pay`, { vaultId: selectedVault });
      setPayModal(null);
      load();
    } finally {
      setPaying(false);
    }
  }

  if (loading) return <div className="p-6 text-center text-gray-500">جاري التحميل...</div>;
  if (!account) return <div className="p-6 text-center text-red-500">الحساب غير موجود</div>;

  const commissions = account.commissions ?? [];
  const filtered = commissions.filter((c) => tab === "unpaid" ? !c.isPaid : c.isPaid);

  return (
    <div className="p-6 max-w-4xl mx-auto">
      <h1 className="text-2xl font-bold text-gray-900 mb-2">{account.doctorName}</h1>
      <p className="text-sm text-gray-500 mb-6">
        طريقة العمولة: {methodAr[account.commissionMethod] ?? account.commissionMethod} — القيمة: {account.defaultCommissionValue}
      </p>

      <div className="grid grid-cols-2 gap-4 mb-6">
        <div className="bg-red-50 border border-red-200 rounded-xl p-4">
          <div className="text-xs text-red-500 mb-1">عمولات غير مدفوعة</div>
          <div className="text-2xl font-bold text-red-700">{account.totalUnpaid.toFixed(2)} <span className="text-sm font-normal">د.ل</span></div>
        </div>
        <div className="bg-green-50 border border-green-200 rounded-xl p-4">
          <div className="text-xs text-green-600 mb-1">إجمالي مدفوع</div>
          <div className="text-2xl font-bold text-green-700">{account.totalPaid.toFixed(2)} <span className="text-sm font-normal">د.ل</span></div>
        </div>
      </div>

      <div className="flex gap-2 mb-4">
        <button onClick={() => setTab("unpaid")} className={`px-4 py-2 rounded-lg text-sm font-medium border ${tab === "unpaid" ? "bg-blue-600 text-white border-blue-600" : "bg-white text-gray-600 border-gray-300"}`}>
          غير مدفوعة ({commissions.filter((c) => !c.isPaid).length})
        </button>
        <button onClick={() => setTab("paid")} className={`px-4 py-2 rounded-lg text-sm font-medium border ${tab === "paid" ? "bg-blue-600 text-white border-blue-600" : "bg-white text-gray-600 border-gray-300"}`}>
          مدفوعة ({commissions.filter((c) => c.isPaid).length})
        </button>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs text-gray-500">الفاتورة</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">طريقة العمولة</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">المبلغ الأساسي</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">النسبة / القيمة</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">العمولة</th>
              <th className="px-4 py-3 text-right text-xs text-gray-500">التاريخ</th>
              {tab === "unpaid" && <th className="px-4 py-3"></th>}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {filtered.map((c) => (
              <tr key={c.id} className="hover:bg-gray-50">
                <td className="px-4 py-3 text-sm font-mono text-gray-700">{c.invoiceNumber}</td>
                <td className="px-4 py-3 text-xs text-gray-500">{methodAr[c.commissionMethod] ?? c.commissionMethod}</td>
                <td className="px-4 py-3 text-sm text-gray-600">{c.baseAmount.toFixed(2)}</td>
                <td className="px-4 py-3 text-sm text-gray-600">{c.commissionRate}</td>
                <td className="px-4 py-3 text-sm font-bold text-blue-700">{c.commissionAmount.toFixed(2)}</td>
                <td className="px-4 py-3 text-xs text-gray-400">{new Date(c.createdAt).toLocaleDateString("ar")}</td>
                {tab === "unpaid" && (
                  <td className="px-4 py-3 text-left">
                    <button
                      onClick={() => { setSelectedVault(vaults[0]?.id ?? ""); setPayModal(c.id); }}
                      className="text-xs bg-blue-600 text-white px-3 py-1 rounded-lg hover:bg-blue-700"
                    >
                      صرف
                    </button>
                  </td>
                )}
              </tr>
            ))}
            {filtered.length === 0 && (
              <tr>
                <td colSpan={7} className="px-6 py-12 text-center text-gray-400">لا توجد سجلات</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {payModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6">
            <h2 className="text-lg font-bold mb-4">صرف العمولة</h2>
            <div className="mb-4">
              <label className="block text-sm font-medium text-gray-700 mb-1">الخزينة</label>
              <select className="w-full border rounded-lg px-3 py-2 text-sm" value={selectedVault} onChange={(e) => setSelectedVault(e.target.value)}>
                {vaults.map((v) => <option key={v.id} value={v.id}>{v.name}</option>)}
              </select>
            </div>
            <div className="flex gap-3">
              <button onClick={() => payCommission(payModal)} disabled={paying || !selectedVault} className="flex-1 bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 text-sm font-medium">
                {paying ? "جاري الصرف..." : "تأكيد الصرف"}
              </button>
              <button onClick={() => setPayModal(null)} className="flex-1 border border-gray-300 text-gray-700 py-2 rounded-lg hover:bg-gray-50 text-sm">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
