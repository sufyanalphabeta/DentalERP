"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";
import { useAuthStore } from "@/stores/authStore";

interface Appointment {
  id: string;
  patientName: string;
  patientId: string;
  startTime: string;
  status: string;
  notes: string | null;
}

interface Patient {
  id: string;
  fullName: string;
  dateOfBirth: string | null;
  phone: string | null;
  lastVisit: string | null;
}

export default function DoctorWorkspace() {
  const { user } = useAuthStore();
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null);
  const [loading, setLoading] = useState(true);
  const today = new Date().toISOString().split("T")[0];

  useEffect(() => { loadAll(); }, []);

  async function loadAll() {
    setLoading(true);
    await Promise.allSettled([
      api.get<{ items: Appointment[] }>(`/appointments?dateFrom=${today}&dateTo=${today}&pageSize=50`).then((r) => setAppointments(r.data.items ?? [])),
    ]);
    setLoading(false);
  }

  async function loadPatient(patientId: string) {
    try {
      const r = await api.get<Patient>(`/patients/${patientId}`);
      setSelectedPatient(r.data);
    } catch {
      setSelectedPatient(null);
    }
  }

  const stats = {
    total: appointments.length,
    waiting: appointments.filter((a) => a.status === "Confirmed" || a.status === "Scheduled").length,
    inProgress: appointments.filter((a) => a.status === "InProgress").length,
    completed: appointments.filter((a) => a.status === "Completed").length,
  };

  const statusAr: Record<string, string> = {
    Scheduled: "مجدول",
    Confirmed: "مؤكد",
    InProgress: "قيد المعالجة",
    Completed: "مكتمل",
    Cancelled: "ملغى",
    NoShow: "لم يحضر",
  };

  const statusCls: Record<string, string> = {
    Scheduled: "bg-blue-100 text-blue-700",
    Confirmed: "bg-purple-100 text-purple-700",
    InProgress: "bg-amber-100 text-amber-800",
    Completed: "bg-green-100 text-green-700",
    Cancelled: "bg-red-100 text-red-600",
    NoShow: "bg-orange-100 text-orange-700",
  };

  return (
    <div className="p-6 space-y-6" dir="rtl">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">مساحة الطبيب</h1>
          <p className="text-gray-500 text-sm">مرحباً {user?.fullName} — {new Date().toLocaleDateString("ar-LY", { weekday: "long", month: "long", day: "numeric" })}</p>
        </div>
        <button onClick={loadAll} className="text-sm border px-3 py-1.5 rounded-lg text-gray-600 hover:bg-gray-50">تحديث</button>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-4 gap-3">
        {[
          { label: "إجمالي المواعيد", value: stats.total, color: "bg-blue-600" },
          { label: "ينتظرون", value: stats.waiting, color: "bg-purple-600" },
          { label: "قيد العلاج", value: stats.inProgress, color: "bg-amber-500" },
          { label: "مكتمل", value: stats.completed, color: "bg-emerald-600" },
        ].map((s) => (
          <div key={s.label} className="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
            <div className={`w-8 h-8 ${s.color} rounded-lg mb-2`} />
            <div className="text-2xl font-bold text-gray-800">{loading ? "—" : s.value}</div>
            <div className="text-xs text-gray-500 mt-0.5">{s.label}</div>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
        {/* Today's patients */}
        <div className="lg:col-span-3 bg-white rounded-xl shadow-sm border border-gray-100">
          <div className="px-5 py-4 border-b">
            <h2 className="font-semibold text-gray-800">مرضى اليوم</h2>
          </div>
          {loading ? (
            <div className="p-8 text-center text-gray-400">جاري التحميل...</div>
          ) : appointments.length === 0 ? (
            <div className="p-8 text-center text-gray-400">لا توجد مواعيد اليوم</div>
          ) : (
            <div className="divide-y max-h-[480px] overflow-y-auto">
              {appointments.map((a) => (
                <button
                  key={a.id}
                  onClick={() => loadPatient(a.patientId)}
                  className="w-full flex items-center justify-between px-5 py-3 hover:bg-blue-50 text-right transition-colors"
                >
                  <div>
                    <div className="text-sm font-medium text-gray-800">{a.patientName}</div>
                    <div className="text-xs text-gray-400">{a.startTime ? new Date(a.startTime).toLocaleTimeString("ar", { hour: "2-digit", minute: "2-digit" }) : "—"}</div>
                    {a.notes && <div className="text-xs text-gray-500 mt-0.5">{a.notes}</div>}
                  </div>
                  <span className={`text-xs px-2 py-0.5 rounded-full ${statusCls[a.status] ?? "bg-gray-100 text-gray-600"}`}>
                    {statusAr[a.status] ?? a.status}
                  </span>
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Patient panel */}
        <div className="lg:col-span-2 space-y-4">
          {selectedPatient ? (
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
              <div className="flex items-center justify-between mb-4">
                <h2 className="font-semibold text-gray-800">ملف المريض</h2>
                <Link href={`/patients/${selectedPatient.id}`} className="text-xs text-blue-600 hover:underline">الملف الكامل</Link>
              </div>
              <div className="space-y-3">
                <div>
                  <div className="text-xs text-gray-400">الاسم الكامل</div>
                  <div className="text-sm font-semibold text-gray-800 mt-0.5">{selectedPatient.fullName}</div>
                </div>
                {selectedPatient.dateOfBirth && (
                  <div>
                    <div className="text-xs text-gray-400">تاريخ الميلاد</div>
                    <div className="text-sm text-gray-700 mt-0.5">{selectedPatient.dateOfBirth ? new Date(selectedPatient.dateOfBirth).toLocaleDateString("ar-LY") : "—"}</div>
                  </div>
                )}
                {selectedPatient.phone && (
                  <div>
                    <div className="text-xs text-gray-400">الهاتف</div>
                    <div className="text-sm text-gray-700 mt-0.5">{selectedPatient.phone}</div>
                  </div>
                )}
                {selectedPatient.lastVisit && (
                  <div>
                    <div className="text-xs text-gray-400">آخر زيارة</div>
                    <div className="text-sm text-gray-700 mt-0.5">{selectedPatient.lastVisit ? new Date(selectedPatient.lastVisit).toLocaleDateString("ar-LY") : "لا توجد زيارات"}</div>
                  </div>
                )}
              </div>
              <div className="mt-4 grid grid-cols-2 gap-2">
                <Link href={`/patients/${selectedPatient.id}`} className="text-center text-sm border border-blue-300 text-blue-700 px-3 py-2 rounded-lg hover:bg-blue-50">عرض الملف</Link>
                <Link href={`/finance/invoices?patientId=${selectedPatient.id}`} className="text-center text-sm border border-emerald-300 text-emerald-700 px-3 py-2 rounded-lg hover:bg-emerald-50">الفواتير</Link>
              </div>
            </div>
          ) : (
            <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-8 text-center text-gray-400 text-sm">
              اختر مريضاً من القائمة لعرض ملفه
            </div>
          )}

          {/* Quick access */}
          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4">
            <h3 className="text-sm font-semibold text-gray-700 mb-3">وصول سريع</h3>
            <div className="space-y-2">
              {[
                { href: "/lab/orders", label: "طلبات المختبر", icon: "🔬" },
                { href: "/radiology/orders", label: "طلبات الأشعة", icon: "📡" },
                { href: "/patients", label: "قائمة المرضى", icon: "👥" },
                { href: "/appointments", label: "المواعيد", icon: "📅" },
              ].map((l) => (
                <Link key={l.href} href={l.href} className="flex items-center gap-3 px-3 py-2 rounded-lg hover:bg-gray-50 text-sm text-gray-700">
                  <span>{l.icon}</span>
                  <span>{l.label}</span>
                </Link>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
