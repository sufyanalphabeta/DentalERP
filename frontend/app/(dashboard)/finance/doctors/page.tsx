"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";

interface Doctor {
  id: string;
  fullName: string;
  email: string | null;
  isActive: boolean;
  roles: string[];
}

export default function DoctorAccountsPage() {
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [loading, setLoading] = useState(true);
  const [dateFrom, setDateFrom] = useState(() => {
    const d = new Date();
    d.setMonth(d.getMonth() - 1);
    return d.toISOString().split("T")[0];
  });
  const [dateTo, setDateTo] = useState(new Date().toISOString().split("T")[0]);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const r = await api.get<{ items: Doctor[] }>("/users?pageSize=200");
      // Show all active users — doctors are identified by being assigned to appointments/invoices
      const docs = (r.data.items ?? []).filter((u) => u.isActive);
      setDoctors(docs);
    } catch {
      setDoctors([]);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">حسابات الأطباء</h1>
          <p className="text-gray-500 text-sm mt-0.5">العمولات والمستحقات</p>
        </div>
        <div className="flex items-center gap-2">
          <input type="date" value={dateFrom} onChange={(e) => setDateFrom(e.target.value)} className="border rounded-lg px-3 py-2 text-sm" />
          <span className="text-gray-400 text-sm">إلى</span>
          <input type="date" value={dateTo} onChange={(e) => setDateTo(e.target.value)} className="border rounded-lg px-3 py-2 text-sm" />
        </div>
      </div>

      {loading ? (
        <div className="text-center py-12 text-gray-400">جاري التحميل...</div>
      ) : doctors.length === 0 ? (
        <div className="text-center py-12 text-gray-400">لا يوجد أطباء نشطون</div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {doctors.map((doc) => (
            <Link key={doc.id} href={`/finance/doctors/${doc.id}/account?from=${dateFrom}&to=${dateTo}`}>
              <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5 hover:shadow-md transition-shadow">
                <div className="flex items-start justify-between mb-3">
                  <div className="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center text-blue-700 font-bold text-sm">
                    {doc.fullName.charAt(0)}
                  </div>
                  <span className="text-xs bg-green-100 text-green-700 px-2 py-0.5 rounded-full">نشط</span>
                </div>
                <div className="font-semibold text-gray-800">{doc.fullName}</div>
                {doc.email && <div className="text-sm text-gray-500 mt-1">{doc.email}</div>}
                <div className="mt-3 text-xs text-blue-600 font-medium">عرض الحساب ←</div>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
