"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";

interface Appointment {
  id: string;
  patientName: string;
  patientId: string;
  doctorName: string;
  startTime: string;
  endTime: string;
  status: string;
  notes: string | null;
}

interface QueueEntry {
  id: string;
  tokenNumber: number;
  patientId: string;
  patientName: string;
  patientPhone: string;
  doctorId: string | null;
  status: string;
  checkInAt: string;
  calledAt: string | null;
  startedAt: string | null;
  completedAt: string | null;
}

const apptStatusCls: Record<string, string> = {
  Scheduled: "bg-blue-100 text-blue-700",
  Confirmed: "bg-green-100 text-green-700",
  InProgress: "bg-amber-100 text-amber-700",
  Completed: "bg-gray-100 text-gray-600",
  Cancelled: "bg-red-100 text-red-600",
  NoShow: "bg-orange-100 text-orange-700",
};

const apptStatusAr: Record<string, string> = {
  Scheduled: "مجدول",
  Confirmed: "مؤكد",
  InProgress: "قيد المعالجة",
  Completed: "مكتمل",
  Cancelled: "ملغى",
  NoShow: "لم يحضر",
};

const queueStatusAr: Record<string, string> = {
  Waiting: "ينتظر",
  WithDoctor: "مع الطبيب",
  Done: "انتهى",
};

export default function ReceptionWorkspace() {
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [queue, setQueue] = useState<QueueEntry[]>([]);
  const [loading, setLoading] = useState(true);
  const today = new Date().toISOString().split("T")[0];

  useEffect(() => { loadAll(); }, []);

  async function loadAll() {
    setLoading(true);
    await Promise.allSettled([
      api.get<{ items: Appointment[] }>(`/appointments?dateFrom=${today}&dateTo=${today}&pageSize=50`).then((r) => setAppointments(r.data.items ?? [])),
      api.get<{ date: string; entries: QueueEntry[] }>(`/queue`).then((r) => setQueue(r.data.entries ?? [])).catch(() => {}),
    ]);
    setLoading(false);
  }

  const stats = {
    total: appointments.length,
    confirmed: appointments.filter((a) => a.status === "Confirmed" || a.status === "Scheduled").length,
    inProgress: appointments.filter((a) => a.status === "InProgress").length,
    completed: appointments.filter((a) => a.status === "Completed").length,
    cancelled: appointments.filter((a) => a.status === "Cancelled" || a.status === "NoShow").length,
    waiting: queue.filter((q) => q.status === "Waiting").length,
  };

  return (
    <div className="p-6 space-y-6" dir="rtl">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">مساحة الاستقبال</h1>
          <p className="text-gray-500 text-sm">{new Date().toLocaleDateString("ar-LY", { weekday: "long", year: "numeric", month: "long", day: "numeric" })}</p>
        </div>
        <div className="flex gap-2">
          <Link href="/patients/new" className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">+ مريض جديد</Link>
          <Link href="/appointments" className="bg-emerald-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-emerald-700">+ موعد جديد</Link>
          <Link href="/queue" className="bg-amber-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-amber-700">الطابور</Link>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-3 md:grid-cols-6 gap-3">
        {[
          { label: "إجمالي المواعيد", value: stats.total, color: "bg-blue-50 text-blue-800 border-blue-200" },
          { label: "المنتظرون", value: stats.confirmed, color: "bg-purple-50 text-purple-800 border-purple-200" },
          { label: "في الطابور", value: stats.waiting, color: "bg-amber-50 text-amber-800 border-amber-200" },
          { label: "قيد المعالجة", value: stats.inProgress, color: "bg-orange-50 text-orange-800 border-orange-200" },
          { label: "مكتمل", value: stats.completed, color: "bg-green-50 text-green-800 border-green-200" },
          { label: "ملغى/لم يحضر", value: stats.cancelled, color: "bg-red-50 text-red-800 border-red-200" },
        ].map((s) => (
          <div key={s.label} className={`rounded-xl border p-4 ${s.color}`}>
            <div className="text-2xl font-bold">{loading ? "—" : s.value}</div>
            <div className="text-xs mt-0.5">{s.label}</div>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-5 gap-6">
        {/* Appointments list */}
        <div className="lg:col-span-3 bg-white rounded-xl shadow-sm border border-gray-100">
          <div className="px-5 py-4 border-b flex items-center justify-between">
            <h2 className="font-semibold text-gray-800">مواعيد اليوم</h2>
            <Link href="/appointments" className="text-xs text-blue-600 hover:underline">عرض الكل</Link>
          </div>
          {loading ? (
            <div className="p-8 text-center text-gray-400">جاري التحميل...</div>
          ) : appointments.length === 0 ? (
            <div className="p-8 text-center text-gray-400">لا توجد مواعيد اليوم</div>
          ) : (
            <div className="divide-y max-h-[500px] overflow-y-auto">
              {appointments.map((a) => (
                <div key={a.id} className="flex items-center justify-between px-5 py-3 hover:bg-gray-50">
                  <div>
                    <div className="text-sm font-medium text-gray-800">{a.patientName}</div>
                    <div className="text-xs text-gray-400">{a.doctorName} — {new Date(a.startTime).toLocaleTimeString("ar", { hour: "2-digit", minute: "2-digit" })}</div>
                  </div>
                  <span className={`text-xs px-2 py-0.5 rounded-full ${apptStatusCls[a.status] ?? "bg-gray-100 text-gray-600"}`}>
                    {apptStatusAr[a.status] ?? a.status}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Queue panel */}
        <div className="lg:col-span-2 bg-white rounded-xl shadow-sm border border-gray-100">
          <div className="px-5 py-4 border-b flex items-center justify-between">
            <h2 className="font-semibold text-gray-800">طابور الانتظار</h2>
            <Link href="/queue" className="text-xs text-blue-600 hover:underline">إدارة الطابور</Link>
          </div>
          {queue.length === 0 ? (
            <div className="p-8 text-center text-gray-400 text-sm">الطابور فارغ</div>
          ) : (
            <div className="divide-y max-h-[500px] overflow-y-auto">
              {queue.map((q, i) => (
                <div key={q.id} className="flex items-center gap-3 px-4 py-3">
                  <div className="w-7 h-7 rounded-full bg-blue-100 text-blue-700 text-xs flex items-center justify-center font-bold">{q.tokenNumber}</div>
                  <div className="flex-1 min-w-0">
                    <div className="text-sm font-medium text-gray-800 truncate">{q.patientName}</div>
                    <div className="text-xs text-gray-400 truncate">{q.patientPhone}</div>
                  </div>
                  <span className={`text-xs px-2 py-0.5 rounded-full whitespace-nowrap ${q.status === "Waiting" ? "bg-amber-100 text-amber-700" : q.status === "WithDoctor" ? "bg-blue-100 text-blue-700" : "bg-green-100 text-green-700"}`}>
                    {queueStatusAr[q.status] ?? q.status}
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      {/* Quick links */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
        {[
          { href: "/patients", label: "قائمة المرضى", icon: "👥" },
          { href: "/appointments", label: "جدول المواعيد", icon: "📅" },
          { href: "/queue", label: "طابور الانتظار", icon: "🏥" },
          { href: "/finance/invoices", label: "الفواتير", icon: "💳" },
        ].map((l) => (
          <Link key={l.href} href={l.href} className="bg-white rounded-xl border border-gray-100 shadow-sm p-4 flex items-center gap-3 hover:shadow-md transition-shadow">
            <span className="text-2xl">{l.icon}</span>
            <span className="text-sm font-medium text-gray-700">{l.label}</span>
          </Link>
        ))}
      </div>
    </div>
  );
}
