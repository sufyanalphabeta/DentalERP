"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface InsuranceClaimDetail {
  id: string;
  claimNumber: string;
  status: string;
  insuranceCompanyId: string;
  insuranceCompanyName: string;
  patientId: string;
  invoiceId: string;
  claimedAmount: number;
  paidAmount: number;
  coveragePercent: number;
  rejectionReason: string | null;
  notes: string | null;
  claimDate: string;
  submittedAt: string | null;
  payments: { id: string; amount: number; referenceNumber: string | null; paymentDate: string }[];
}

const STATUS_LABELS: Record<string, string> = {
  Draft: "مسودة",
  Submitted: "مُقدَّم",
  PartiallyPaid: "مدفوع جزئياً",
  FullyPaid: "مدفوع بالكامل",
  Rejected: "مرفوض",
};

export default function InsuranceClaimDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [claim, setClaim] = useState<InsuranceClaimDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [showPayment, setShowPayment] = useState(false);
  const [showReject, setShowReject] = useState(false);
  const [payment, setPayment] = useState({ amount: 0, referenceNumber: "", notes: "" });
  const [rejectReason, setRejectReason] = useState("");

  const load = () => {
    setLoading(true);
    api.get(`/insurance/claims?page=1&pageSize=100`).then((r) => {
      const found = (r.data.items ?? []).find((c: InsuranceClaimDetail) => c.id === id);
      setClaim(found ?? null);
      setLoading(false);
    });
  };

  useEffect(load, [id]);

  const handleSubmit = async () => {
    await api.post(`/insurance/claims/${id}/submit`);
    load();
  };

  const handlePayment = async () => {
    await api.post(`/insurance/claims/${id}/payment`, {
      ...payment,
      receivedById: "00000000-0000-0000-0000-000000000001",
    });
    setShowPayment(false);
    load();
  };

  const handleReject = async () => {
    await api.post(`/insurance/claims/${id}/reject`, { reason: rejectReason });
    setShowReject(false);
    load();
  };

  if (loading) return <div className="p-6 text-center text-gray-500">جاري التحميل...</div>;
  if (!claim) return <div className="p-6 text-center text-red-500">لم يتم العثور على المطالبة</div>;

  const remaining = claim.claimedAmount - claim.paidAmount;

  return (
    <div className="p-6 max-w-4xl mx-auto" dir="rtl">
      <button onClick={() => router.back()} className="text-sm text-blue-600 hover:underline mb-4">
        → رجوع
      </button>

      <div className="bg-white border rounded-xl p-6 mb-6">
        <div className="flex items-start justify-between mb-4">
          <div>
            <h1 className="text-xl font-bold text-gray-900">{claim.claimNumber}</h1>
            <p className="text-sm text-gray-500 mt-1">{claim.insuranceCompanyName}</p>
          </div>
          <span className="px-3 py-1 bg-blue-100 text-blue-700 rounded-full text-sm font-medium">
            {STATUS_LABELS[claim.status] ?? claim.status}
          </span>
        </div>

        <div className="grid grid-cols-4 gap-4 text-sm mb-6">
          <div>
            <p className="text-gray-500">المبلغ المطالب</p>
            <p className="font-semibold text-lg">{claim.claimedAmount.toFixed(2)} د.ل</p>
          </div>
          <div>
            <p className="text-gray-500">المدفوع</p>
            <p className="font-semibold text-lg text-green-700">{claim.paidAmount.toFixed(2)} د.ل</p>
          </div>
          <div>
            <p className="text-gray-500">المتبقي</p>
            <p className="font-semibold text-lg text-red-600">{remaining.toFixed(2)} د.ل</p>
          </div>
          <div>
            <p className="text-gray-500">نسبة التغطية</p>
            <p className="font-semibold text-lg">{claim.coveragePercent}%</p>
          </div>
        </div>

        {claim.rejectionReason && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-3 mb-4">
            <p className="text-sm text-red-700">سبب الرفض: {claim.rejectionReason}</p>
          </div>
        )}

        <div className="flex flex-wrap gap-3">
          {claim.status === "Draft" && (
            <button onClick={handleSubmit} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">
              تقديم المطالبة
            </button>
          )}
          {(claim.status === "Submitted" || claim.status === "PartiallyPaid") && (
            <>
              <button onClick={() => setShowPayment(true)} className="bg-green-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-green-700">
                تسجيل دفعة
              </button>
              <button onClick={() => setShowReject(true)} className="border border-red-300 text-red-600 px-4 py-2 rounded-lg text-sm hover:bg-red-50">
                رفض المطالبة
              </button>
            </>
          )}
        </div>
      </div>

      {claim.payments.length > 0 && (
        <div className="bg-white border rounded-xl p-5">
          <h2 className="font-semibold text-gray-700 mb-3">سجل المدفوعات</h2>
          <table className="w-full text-sm">
            <thead>
              <tr className="text-gray-500 border-b">
                <th className="text-right pb-2">المبلغ</th>
                <th className="text-right pb-2">رقم المرجع</th>
                <th className="text-right pb-2">التاريخ</th>
              </tr>
            </thead>
            <tbody>
              {claim.payments.map((p) => (
                <tr key={p.id} className="border-b last:border-0">
                  <td className="py-2 font-medium text-green-700">{p.amount.toFixed(2)} د.ل</td>
                  <td className="py-2 text-gray-600">{p.referenceNumber ?? "—"}</td>
                  <td className="py-2 text-gray-500">{new Date(p.paymentDate).toLocaleDateString("ar-LY")}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {showPayment && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md" dir="rtl">
            <h2 className="text-lg font-semibold mb-4">تسجيل دفعة تأمين</h2>
            <div className="space-y-3">
              <input
                type="number"
                placeholder="المبلغ"
                className="w-full border rounded-lg p-2 text-sm"
                value={payment.amount}
                onChange={(e) => setPayment({ ...payment, amount: Number(e.target.value) })}
              />
              <input
                placeholder="رقم المرجع"
                className="w-full border rounded-lg p-2 text-sm"
                value={payment.referenceNumber}
                onChange={(e) => setPayment({ ...payment, referenceNumber: e.target.value })}
              />
              <textarea
                placeholder="ملاحظات"
                rows={2}
                className="w-full border rounded-lg p-2 text-sm"
                value={payment.notes}
                onChange={(e) => setPayment({ ...payment, notes: e.target.value })}
              />
            </div>
            <div className="flex gap-3 mt-4">
              <button onClick={handlePayment} disabled={!payment.amount} className="flex-1 bg-green-600 text-white py-2 rounded-lg text-sm disabled:opacity-50">
                تسجيل
              </button>
              <button onClick={() => setShowPayment(false)} className="flex-1 border py-2 rounded-lg text-sm">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}

      {showReject && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md" dir="rtl">
            <h2 className="text-lg font-semibold mb-4 text-red-600">رفض المطالبة</h2>
            <textarea
              placeholder="سبب الرفض..."
              rows={3}
              className="w-full border rounded-lg p-2 text-sm"
              value={rejectReason}
              onChange={(e) => setRejectReason(e.target.value)}
            />
            <div className="flex gap-3 mt-4">
              <button onClick={handleReject} disabled={!rejectReason.trim()} className="flex-1 bg-red-600 text-white py-2 rounded-lg text-sm disabled:opacity-50">
                تأكيد الرفض
              </button>
              <button onClick={() => setShowReject(false)} className="flex-1 border py-2 rounded-lg text-sm">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
