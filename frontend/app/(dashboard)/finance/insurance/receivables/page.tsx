"use client";

import { useState, useEffect } from "react";
import { api } from "@/lib/api";

interface InsuranceClaimSummary {
  id: string;
  claimNumber: string;
  status: string;
  insuranceCompanyName: string;
  patientId: string;
  claimedAmount: number;
  paidAmount: number;
  coveragePercent: number;
  claimDate: string;
}

export default function InsuranceReceivablesPage() {
  const [claims, setClaims] = useState<InsuranceClaimSummary[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      api.get("/insurance/claims?status=Submitted&pageSize=100"),
      api.get("/insurance/claims?status=PartiallyPaid&pageSize=100"),
    ]).then(([submitted, partial]) => {
      const all = [
        ...(submitted.data.items ?? []),
        ...(partial.data.items ?? []),
      ];
      setClaims(all);
      setLoading(false);
    });
  }, []);

  const totalOutstanding = claims.reduce((sum, c) => sum + (c.claimedAmount - c.paidAmount), 0);
  const totalClaimed = claims.reduce((sum, c) => sum + c.claimedAmount, 0);
  const totalPaid = claims.reduce((sum, c) => sum + c.paidAmount, 0);

  return (
    <div className="p-6 max-w-6xl mx-auto" dir="rtl">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">مستحقات التأمين</h1>

      <div className="grid grid-cols-3 gap-4 mb-6">
        <div className="bg-white border rounded-xl p-5">
          <p className="text-sm text-gray-500">إجمالي المطالب</p>
          <p className="text-2xl font-bold text-gray-900 mt-1">{totalClaimed.toFixed(2)} د.ل</p>
        </div>
        <div className="bg-white border rounded-xl p-5">
          <p className="text-sm text-gray-500">المحصّل</p>
          <p className="text-2xl font-bold text-green-700 mt-1">{totalPaid.toFixed(2)} د.ل</p>
        </div>
        <div className="bg-red-50 border border-red-200 rounded-xl p-5">
          <p className="text-sm text-red-600">المستحق</p>
          <p className="text-2xl font-bold text-red-700 mt-1">{totalOutstanding.toFixed(2)} د.ل</p>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-12 text-gray-500">جاري التحميل...</div>
      ) : claims.length === 0 ? (
        <div className="text-center py-12 text-gray-400">لا توجد مستحقات معلقة</div>
      ) : (
        <div className="bg-white border rounded-xl overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50">
              <tr>
                <th className="text-right p-4 text-gray-600">رقم المطالبة</th>
                <th className="text-right p-4 text-gray-600">شركة التأمين</th>
                <th className="text-right p-4 text-gray-600">المطالب</th>
                <th className="text-right p-4 text-gray-600">المدفوع</th>
                <th className="text-right p-4 text-gray-600">المستحق</th>
                <th className="text-right p-4 text-gray-600">الحالة</th>
                <th className="text-right p-4 text-gray-600">تاريخ التقديم</th>
              </tr>
            </thead>
            <tbody>
              {claims.map((claim) => (
                <tr key={claim.id} className="border-t hover:bg-gray-50">
                  <td className="p-4 font-medium text-blue-700">{claim.claimNumber}</td>
                  <td className="p-4 text-gray-700">{claim.insuranceCompanyName}</td>
                  <td className="p-4">{claim.claimedAmount.toFixed(2)} د.ل</td>
                  <td className="p-4 text-green-700">{claim.paidAmount.toFixed(2)} د.ل</td>
                  <td className="p-4 font-semibold text-red-600">
                    {(claim.claimedAmount - claim.paidAmount).toFixed(2)} د.ل
                  </td>
                  <td className="p-4">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${
                      claim.status === "PartiallyPaid"
                        ? "bg-yellow-100 text-yellow-700"
                        : "bg-blue-100 text-blue-700"
                    }`}>
                      {claim.status === "PartiallyPaid" ? "مدفوع جزئياً" : "مُقدَّم"}
                    </span>
                  </td>
                  <td className="p-4 text-gray-500">
                    {new Date(claim.claimDate).toLocaleDateString("ar-LY")}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
