"use client";

import { useEffect, useState, useCallback } from "react";
import { api } from "@/lib/api";
import type { GetQueueResponse, QueueEntryItem, QueueStatus } from "@/types/patients";

const STATUS_LABELS: Record<QueueStatus, string> = {
  Waiting: "انتظار",
  Called: "نودي",
  InProgress: "جارٍ",
  Completed: "انتهى",
  Skipped: "تم تخطيه",
};

const STATUS_COLORS: Record<QueueStatus, string> = {
  Waiting: "bg-blue-100 text-blue-800",
  Called: "bg-yellow-100 text-yellow-800",
  InProgress: "bg-green-100 text-green-800",
  Completed: "bg-gray-100 text-gray-600",
  Skipped: "bg-red-100 text-red-800",
};

interface Patient {
  id: string;
  fullName: string;
  phone: string | null;
}

interface Doctor {
  id: string;
  fullName: string;
  roles: string[];
}

export default function QueuePage() {
  const [queue, setQueue] = useState<GetQueueResponse | null>(null);
  const [today] = useState(new Date().toISOString().slice(0, 10));
  const [loading, setLoading] = useState(true);

  // Check-in modal
  const [showCheckin, setShowCheckin] = useState(false);
  const [patientSearch, setPatientSearch] = useState("");
  const [patients, setPatients] = useState<Patient[]>([]);
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null);
  const [selectedDoctorId, setSelectedDoctorId] = useState("");
  const [checkinNotes, setCheckinNotes] = useState("");
  const [checkingIn, setCheckingIn] = useState(false);
  const [checkinError, setCheckinError] = useState<string | null>(null);
  const [checkinSuccess, setCheckinSuccess] = useState<string | null>(null);

  const fetchQueue = useCallback(async () => {
    try {
      const res = await api.get<GetQueueResponse>(`/queue?date=${today}`);
      setQueue(res.data);
    } finally {
      setLoading(false);
    }
  }, [today]);

  useEffect(() => {
    fetchQueue();
    const interval = setInterval(fetchQueue, 15000);
    return () => clearInterval(interval);
  }, [fetchQueue]);

  async function updateStatus(id: string, status: QueueStatus) {
    await api.patch(`/queue/${id}/status`, { status });
    fetchQueue();
  }

  async function openCheckin() {
    setShowCheckin(true);
    setSelectedPatient(null);
    setPatientSearch("");
    setSelectedDoctorId("");
    setCheckinNotes("");
    setCheckinError(null);

    // Load doctors
    try {
      const r = await api.get<{ items: Doctor[] }>("/users?pageSize=200");
      setDoctors((r.data.items ?? []).filter((u) => u.roles?.includes("Doctor")));
    } catch { /* ignore */ }
  }

  async function searchPatients(q: string) {
    setPatientSearch(q);
    if (q.length < 2) { setPatients([]); return; }
    try {
      const r = await api.get<{ items: Patient[] }>(`/patients?search=${encodeURIComponent(q)}&pageSize=15`);
      setPatients(r.data.items ?? []);
    } catch { /* ignore */ }
  }

  async function checkIn() {
    if (!selectedPatient) return;
    setCheckingIn(true);
    setCheckinError(null);
    try {
      const r = await api.post<{ queueEntryId: string; tokenNumber: number }>("/queue/check-in", {
        patientId: selectedPatient.id,
        doctorId: selectedDoctorId || null,
        notes: checkinNotes || null,
      });
      setShowCheckin(false);
      fetchQueue();
      setCheckinSuccess(`تم تسجيل دخول ${selectedPatient.fullName} — رقم الدور: ${r.data.tokenNumber}`);
      setTimeout(() => setCheckinSuccess(null), 4000);
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string; message?: string } } };
      setCheckinError(err?.response?.data?.error ?? err?.response?.data?.message ?? "حدث خطأ");
    } finally {
      setCheckingIn(false);
    }
  }

  const active = queue?.entries.filter(e =>
    e.status === "Waiting" || e.status === "Called" || e.status === "InProgress"
  ) ?? [];

  const done = queue?.entries.filter(e =>
    e.status === "Completed" || e.status === "Skipped"
  ) ?? [];

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">قائمة الانتظار</h1>
        <div className="flex items-center gap-3">
          <span className="text-sm text-gray-500">{today}</span>
          <div className="flex gap-2 text-sm">
            <span className="bg-blue-100 text-blue-700 px-2 py-1 rounded-full">
              انتظار: {queue?.entries.filter(e => e.status === "Waiting").length ?? 0}
            </span>
            <span className="bg-green-100 text-green-700 px-2 py-1 rounded-full">
              مكتمل: {done.length}
            </span>
          </div>
          <button
            onClick={openCheckin}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
          >
            + تسجيل دخول مريض
          </button>
        </div>
      </div>

      {checkinSuccess && (
        <div className="mb-4 flex items-center gap-3 bg-green-50 border border-green-200 text-green-800 rounded-lg px-4 py-3 text-sm">
          <span className="font-medium">{checkinSuccess}</span>
        </div>
      )}

      {loading ? (
        <div className="text-center py-12 text-gray-500">جاري التحميل...</div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {active.map((entry: QueueEntryItem) => (
            <div
              key={entry.id}
              className={`bg-white rounded-xl shadow p-5 border-r-4 ${
                entry.status === "InProgress" ? "border-green-500" :
                entry.status === "Called" ? "border-yellow-500" :
                "border-blue-300"
              }`}
            >
              <div className="flex items-start justify-between mb-3">
                <div className="text-3xl font-bold text-gray-800">#{entry.tokenNumber}</div>
                <span className={`px-2 py-1 rounded-full text-xs font-medium ${STATUS_COLORS[entry.status]}`}>
                  {STATUS_LABELS[entry.status]}
                </span>
              </div>
              <div className="font-semibold text-gray-900 mb-1">{entry.patientName}</div>
              <div className="text-sm text-gray-500 mb-4">{entry.patientPhone}</div>

              <div className="flex gap-2 flex-wrap">
                {entry.status === "Waiting" && (
                  <button
                    onClick={() => updateStatus(entry.id, "Called")}
                    className="flex-1 bg-yellow-500 text-white py-1.5 text-sm rounded-lg hover:bg-yellow-600"
                  >
                    نادِ
                  </button>
                )}
                {(entry.status === "Called" || entry.status === "Waiting") && (
                  <button
                    onClick={() => updateStatus(entry.id, "InProgress")}
                    className="flex-1 bg-green-600 text-white py-1.5 text-sm rounded-lg hover:bg-green-700"
                  >
                    بدء
                  </button>
                )}
                {entry.status === "InProgress" && (
                  <button
                    onClick={() => updateStatus(entry.id, "Completed")}
                    className="flex-1 bg-gray-600 text-white py-1.5 text-sm rounded-lg hover:bg-gray-700"
                  >
                    انتهى
                  </button>
                )}
                {entry.status !== "Completed" && entry.status !== "Skipped" && (
                  <button
                    onClick={() => updateStatus(entry.id, "Skipped")}
                    className="px-3 py-1.5 text-sm border text-gray-600 rounded-lg hover:bg-gray-50"
                  >
                    تخطِّ
                  </button>
                )}
              </div>
            </div>
          ))}

          {active.length === 0 && (
            <div className="col-span-3 text-center py-16 text-gray-400">
              لا يوجد مرضى في قائمة الانتظار حالياً
            </div>
          )}
        </div>
      )}

      {done.length > 0 && (
        <div className="mt-8">
          <h2 className="text-lg font-semibold text-gray-700 mb-3">المنتهون اليوم ({done.length})</h2>
          <div className="bg-white rounded-xl shadow overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-2 text-start">الرقم</th>
                  <th className="px-4 py-2 text-start">المريض</th>
                  <th className="px-4 py-2 text-start">الحالة</th>
                  <th className="px-4 py-2 text-start">الانتهاء</th>
                </tr>
              </thead>
              <tbody>
                {done.map(e => (
                  <tr key={e.id} className="border-t border-gray-100">
                    <td className="px-4 py-2 font-mono">#{e.tokenNumber}</td>
                    <td className="px-4 py-2">{e.patientName}</td>
                    <td className="px-4 py-2">
                      <span className={`px-2 py-0.5 text-xs rounded-full ${STATUS_COLORS[e.status]}`}>
                        {STATUS_LABELS[e.status]}
                      </span>
                    </td>
                    <td className="px-4 py-2 text-gray-500 text-xs">
                      {e.completedAt ? new Date(e.completedAt).toLocaleTimeString("ar-SA") : "—"}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Check-in modal */}
      {showCheckin && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-md p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">تسجيل دخول مريض</h2>
            {checkinError && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{checkinError}</div>}

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
                    <button onClick={() => { setSelectedPatient(null); setPatientSearch(""); setPatients([]); }}
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
                      autoFocus
                    />
                    {patients.length > 0 && (
                      <div className="border rounded-lg mt-1 max-h-40 overflow-y-auto bg-white shadow">
                        {patients.map((p) => (
                          <button key={p.id} onClick={() => { setSelectedPatient(p); setPatients([]); setPatientSearch(""); }}
                            className="w-full text-right px-3 py-2 hover:bg-blue-50 text-sm border-b last:border-0">
                            <div className="font-medium text-gray-800">{p.fullName}</div>
                            {p.phone && <div className="text-xs text-gray-400">{p.phone}</div>}
                          </button>
                        ))}
                      </div>
                    )}
                    {patientSearch.length >= 2 && patients.length === 0 && (
                      <div className="text-xs text-gray-400 mt-1 px-1">لم يتم إيجاد مريض</div>
                    )}
                  </div>
                )}
              </div>

              {/* Doctor (optional) */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">الطبيب (اختياري)</label>
                <select className="w-full border rounded-lg px-3 py-2 text-sm" value={selectedDoctorId}
                  onChange={(e) => setSelectedDoctorId(e.target.value)}>
                  <option value="">— بدون تحديد طبيب —</option>
                  {doctors.map((d) => <option key={d.id} value={d.id}>{d.fullName}</option>)}
                </select>
              </div>

              {/* Notes */}
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
                <input className="w-full border rounded-lg px-3 py-2 text-sm"
                  placeholder="شكوى المريض أو ملاحظة..."
                  value={checkinNotes}
                  onChange={(e) => setCheckinNotes(e.target.value)} />
              </div>
            </div>

            <div className="flex gap-3 mt-5">
              <button
                onClick={checkIn}
                disabled={checkingIn || !selectedPatient}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50 font-medium"
              >
                {checkingIn ? "جاري التسجيل..." : "تسجيل الدخول"}
              </button>
              <button onClick={() => setShowCheckin(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
