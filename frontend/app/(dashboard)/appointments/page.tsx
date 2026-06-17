"use client";

import { useEffect, useState } from "react";
import api from "@/lib/api";
import { PermissionGate } from "@/components/shared/PermissionGate";
import type { AppointmentItem, GetAppointmentsResponse } from "@/types/patients";

const STATUS_LABELS: Record<string, string> = {
  Scheduled: "مجدول",
  Confirmed: "مؤكد",
  InProgress: "جارٍ",
  Completed: "مكتمل",
  Cancelled: "ملغي",
  NoShow: "لم يحضر",
};

const STATUS_COLORS: Record<string, string> = {
  Scheduled: "bg-blue-100 text-blue-700",
  Confirmed: "bg-green-100 text-green-700",
  InProgress: "bg-yellow-100 text-yellow-700",
  Completed: "bg-gray-100 text-gray-700",
  Cancelled: "bg-red-100 text-red-700",
  NoShow: "bg-orange-100 text-orange-700",
};

function formatDateTime(iso: string) {
  return new Date(iso).toLocaleString("ar-SA", {
    dateStyle: "short",
    timeStyle: "short",
  });
}

export default function AppointmentsPage() {
  const [data, setData] = useState<{ items: AppointmentItem[]; totalCount: number } | null>(null);
  const [fromDate, setFromDate] = useState(new Date().toISOString().slice(0, 10));
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchAppointments();
  }, [fromDate]);

  async function fetchAppointments() {
    setLoading(true);
    try {
      const res = await api.get<GetAppointmentsResponse>(
        `/api/appointments?fromDate=${fromDate}&pageSize=100`
      );
      setData(res.data);
    } finally {
      setLoading(false);
    }
  }

  async function updateStatus(id: string, status: string, reason?: string) {
    await api.patch(`/api/appointments/${id}/status`, { status, cancellationReason: reason });
    fetchAppointments();
  }

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">المواعيد</h1>
        <div className="flex items-center gap-3">
          <input
            type="date"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
            className="border rounded-lg px-3 py-2 text-sm"
          />
        </div>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="w-full text-sm">
          <thead className="bg-gray-50 text-gray-600">
            <tr>
              <th className="px-4 py-3 text-start">الوقت</th>
              <th className="px-4 py-3 text-start">المريض</th>
              <th className="px-4 py-3 text-start">النوع</th>
              <th className="px-4 py-3 text-start">المدة</th>
              <th className="px-4 py-3 text-start">الحالة</th>
              <PermissionGate permission="Appointments.Edit">
                <th className="px-4 py-3 text-start">الإجراءات</th>
              </PermissionGate>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-500">جاري التحميل...</td></tr>
            ) : data?.items.length === 0 ? (
              <tr><td colSpan={6} className="text-center py-8 text-gray-400">لا توجد مواعيد لهذا اليوم</td></tr>
            ) : data?.items.map((apt) => (
              <tr key={apt.id} className="border-t border-gray-100 hover:bg-gray-50">
                <td className="px-4 py-3 font-medium">{formatDateTime(apt.scheduledAt)}</td>
                <td className="px-4 py-3">
                  <div className="font-medium">{apt.patientName}</div>
                  <div className="text-gray-500 text-xs">{apt.patientPhone}</div>
                </td>
                <td className="px-4 py-3">
                  {apt.typeColor ? (
                    <span className="flex items-center gap-1">
                      <span
                        className="w-2.5 h-2.5 rounded-full inline-block"
                        style={{ backgroundColor: apt.typeColor }}
                      />
                      {apt.typeNameAr ?? apt.typeName}
                    </span>
                  ) : "—"}
                </td>
                <td className="px-4 py-3 text-gray-600">{apt.durationMinutes} دقيقة</td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-1 rounded-full text-xs font-medium ${STATUS_COLORS[apt.status] ?? ""}`}>
                    {STATUS_LABELS[apt.status] ?? apt.status}
                  </span>
                </td>
                <PermissionGate permission="Appointments.Edit">
                  <td className="px-4 py-3">
                    <div className="flex gap-1 flex-wrap">
                      {apt.status === "Scheduled" && (
                        <button onClick={() => updateStatus(apt.id, "Confirmed")}
                          className="px-2 py-1 text-xs bg-green-50 text-green-700 rounded hover:bg-green-100">
                          تأكيد
                        </button>
                      )}
                      {(apt.status === "Confirmed" || apt.status === "Scheduled") && (
                        <button onClick={() => updateStatus(apt.id, "InProgress")}
                          className="px-2 py-1 text-xs bg-yellow-50 text-yellow-700 rounded hover:bg-yellow-100">
                          بدء
                        </button>
                      )}
                      {apt.status === "InProgress" && (
                        <button onClick={() => updateStatus(apt.id, "Completed")}
                          className="px-2 py-1 text-xs bg-gray-100 text-gray-700 rounded hover:bg-gray-200">
                          إتمام
                        </button>
                      )}
                      {(apt.status === "Scheduled" || apt.status === "Confirmed") && (
                        <button onClick={() => updateStatus(apt.id, "Cancelled")}
                          className="px-2 py-1 text-xs bg-red-50 text-red-700 rounded hover:bg-red-100">
                          إلغاء
                        </button>
                      )}
                    </div>
                  </td>
                </PermissionGate>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
