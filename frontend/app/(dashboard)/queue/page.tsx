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

export default function QueuePage() {
  const [queue, setQueue] = useState<GetQueueResponse | null>(null);
  const [today] = useState(new Date().toISOString().slice(0, 10));
  const [loading, setLoading] = useState(true);

  const fetchQueue = useCallback(async () => {
    try {
      const res = await api.get<GetQueueResponse>(`/api/queue?date=${today}`);
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
    await api.patch(`/api/queue/${id}/status`, { status });
    fetchQueue();
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
        </div>
      </div>

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
    </div>
  );
}
