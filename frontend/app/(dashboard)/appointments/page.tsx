"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";
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

interface Doctor {
  id: string;
  fullName: string;
  roles: string[];
}

interface Patient {
  id: string;
  fullName: string;
  phone: string | null;
}

interface AppType {
  id: string;
  name: string;
  nameAr: string | null;
  defaultDurationMinutes: number;
}

function formatDateTime(iso: string | null | undefined) {
  if (!iso) return "—";
  const d = new Date(iso);
  if (isNaN(d.getTime())) return "—";
  return d.toLocaleString("ar-SA", { dateStyle: "short", timeStyle: "short" });
}

export default function AppointmentsPage() {
  const [data, setData] = useState<{ items: AppointmentItem[]; totalCount: number } | null>(null);
  const [fromDate, setFromDate] = useState(new Date().toISOString().slice(0, 10));
  const [doctorFilter, setDoctorFilter] = useState("");
  const [filterDoctors, setFilterDoctors] = useState<Doctor[]>([]);
  const [loading, setLoading] = useState(true);

  // Create appointment
  const [showCreate, setShowCreate] = useState(false);
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [apptTypes, setApptTypes] = useState<AppType[]>([]);
  const [patientSearch, setPatientSearch] = useState("");
  const [form, setForm] = useState({
    patientId: "",
    doctorId: "",
    scheduledAt: "",
    durationMinutes: "30",
    appointmentTypeId: "",
    chiefComplaint: "",
    notes: "",
  });
  const [saving, setSaving] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);

  useEffect(() => {
    api.get<{ items: Doctor[] }>("/users?pageSize=200").then((r) => {
      setFilterDoctors((r.data.items ?? []).filter((u) => u.roles?.includes("Doctor")));
    }).catch(() => {});
  }, []);

  useEffect(() => {
    fetchAppointments();
  }, [fromDate, doctorFilter]);

  async function fetchAppointments() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ fromDate, pageSize: "100" });
      if (doctorFilter) params.set("doctorId", doctorFilter);
      const res = await api.get<GetAppointmentsResponse>(`/appointments?${params}`);
      setData(res.data);
    } finally {
      setLoading(false);
    }
  }

  async function openCreate() {
    setShowCreate(true);
    setCreateError(null);
    setForm({ patientId: "", doctorId: "", scheduledAt: `${fromDate}T09:00`, durationMinutes: "30", appointmentTypeId: "", chiefComplaint: "", notes: "" });
    setPatientSearch("");

    const [usersRes, patientsRes] = await Promise.allSettled([
      api.get<{ items: Doctor[] }>("/users?pageSize=200"),
      api.get<{ items: Patient[] }>("/patients?pageSize=50"),
    ]);
    if (usersRes.status === "fulfilled") {
      setDoctors((usersRes.value.data.items ?? []).filter((u) => u.roles?.includes("Doctor")));
    }
    if (patientsRes.status === "fulfilled") {
      setPatients(patientsRes.value.data.items ?? []);
    }
    api.get<AppType[]>("/appointment-types").then((r) => setApptTypes(r.data ?? [])).catch(() => setApptTypes([]));
  }

  async function searchPatients(q: string) {
    setPatientSearch(q);
    if (q.length < 2) return;
    try {
      const r = await api.get<{ items: Patient[] }>(`/patients?search=${encodeURIComponent(q)}&pageSize=20`);
      setPatients(r.data.items ?? []);
    } catch { /* ignore */ }
  }

  async function createAppointment() {
    setSaving(true);
    setCreateError(null);
    try {
      await api.post("/appointments", {
        patientId: form.patientId,
        doctorId: form.doctorId,
        scheduledAt: new Date(form.scheduledAt).toISOString(),
        durationMinutes: parseInt(form.durationMinutes),
        appointmentTypeId: form.appointmentTypeId || null,
        chiefComplaint: form.chiefComplaint || null,
        notes: form.notes || null,
      });
      setShowCreate(false);
      fetchAppointments();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setCreateError(err?.response?.data?.error ?? "حدث خطأ أثناء إنشاء الموعد");
    } finally {
      setSaving(false);
    }
  }

  async function updateStatus(id: string, status: string, reason?: string) {
    await api.patch(`/appointments/${id}/status`, { status, cancellationReason: reason });
    fetchAppointments();
  }

  const filteredPatients = patientSearch.length >= 2 ? patients : patients.slice(0, 10);
  const selectedPatient = patients.find((p) => p.id === form.patientId);
  const selectedType = apptTypes.find((t) => t.id === form.appointmentTypeId);

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
          {filterDoctors.length > 0 && (
            <select value={doctorFilter} onChange={(e) => setDoctorFilter(e.target.value)} className="border rounded-lg px-3 py-2 text-sm">
              <option value="">كل الأطباء</option>
              {filterDoctors.map((d) => <option key={d.id} value={d.id}>{d.fullName}</option>)}
            </select>
          )}
          <PermissionGate permission="Appointments.Create">
            <button
              onClick={openCreate}
              className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
            >
              + موعد جديد
            </button>
          </PermissionGate>
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
                        <button onClick={() => updateStatus(apt.id, "NoShow")}
                          className="px-2 py-1 text-xs bg-orange-50 text-orange-700 rounded hover:bg-orange-100">
                          لم يحضر
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

      {/* Create Appointment Modal */}
      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-lg p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">موعد جديد</h2>
            {createError && (
              <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{createError}</div>
            )}
            <div className="space-y-3">
              {/* Patient search */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">المريض *</label>
                {selectedPatient ? (
                  <div className="flex items-center justify-between border rounded-lg px-3 py-2 bg-blue-50">
                    <div>
                      <div className="text-sm font-medium text-gray-800">{selectedPatient.fullName}</div>
                      {selectedPatient.phone && <div className="text-xs text-gray-500">{selectedPatient.phone}</div>}
                    </div>
                    <button onClick={() => { setForm({ ...form, patientId: "" }); setPatientSearch(""); }}
                      className="text-xs text-red-500 hover:text-red-700">تغيير</button>
                  </div>
                ) : (
                  <div>
                    <input
                      type="text"
                      placeholder="ابحث باسم المريض أو الهاتف..."
                      value={patientSearch}
                      onChange={(e) => searchPatients(e.target.value)}
                      className="w-full border rounded-lg px-3 py-2 text-sm"
                    />
                    {filteredPatients.length > 0 && (
                      <div className="border rounded-lg mt-1 max-h-36 overflow-y-auto bg-white shadow">
                        {filteredPatients.map((p) => (
                          <button key={p.id} onClick={() => { setForm({ ...form, patientId: p.id }); setPatientSearch(""); }}
                            className="w-full text-right px-3 py-2 hover:bg-blue-50 text-sm border-b last:border-0">
                            <div className="font-medium text-gray-800">{p.fullName}</div>
                            {p.phone && <div className="text-xs text-gray-400">{p.phone}</div>}
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </div>

              {/* Doctor */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الطبيب *</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.doctorId}
                  onChange={(e) => setForm({ ...form, doctorId: e.target.value })}>
                  <option value="">— اختر الطبيب —</option>
                  {doctors.map((d) => <option key={d.id} value={d.id}>{d.fullName}</option>)}
                </select>
              </div>

              {/* Date & time */}
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">التاريخ والوقت *</label>
                  <input type="datetime-local" className="w-full border rounded-lg px-3 py-2 text-sm"
                    value={form.scheduledAt} onChange={(e) => setForm({ ...form, scheduledAt: e.target.value })} />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">المدة (دقيقة)</label>
                  <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.durationMinutes}
                    onChange={(e) => setForm({ ...form, durationMinutes: e.target.value })}>
                    {[15, 20, 30, 45, 60, 90, 120].map((m) => <option key={m} value={m}>{m} دقيقة</option>)}
                  </select>
                </div>
              </div>

              {/* Type */}
              {apptTypes.length > 0 && (
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">نوع الموعد</label>
                  <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.appointmentTypeId}
                    onChange={(e) => {
                      const t = apptTypes.find((x) => x.id === e.target.value);
                      setForm({ ...form, appointmentTypeId: e.target.value, durationMinutes: t ? String(t.defaultDurationMinutes) : form.durationMinutes });
                    }}>
                    <option value="">— بدون نوع —</option>
                    {apptTypes.map((t) => <option key={t.id} value={t.id}>{t.nameAr ?? t.name}</option>)}
                  </select>
                </div>
              )}

              {/* Chief complaint */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الشكوى الرئيسية</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm" placeholder="وصف موجز للمشكلة..."
                  value={form.chiefComplaint} onChange={(e) => setForm({ ...form, chiefComplaint: e.target.value })} />
              </div>

              {/* Notes */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
                <textarea className="w-full border rounded-lg px-3 py-2 text-sm" rows={2}
                  value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
              </div>
            </div>
            <div className="flex gap-3 mt-5">
              <button onClick={createAppointment}
                disabled={saving || !form.patientId || !form.doctorId || !form.scheduledAt}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">
                {saving ? "جاري الحفظ..." : "إنشاء الموعد"}
              </button>
              <button onClick={() => setShowCreate(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
