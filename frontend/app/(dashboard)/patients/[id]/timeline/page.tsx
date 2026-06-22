'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { api } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';

interface TimelineEvent {
  id: string;
  eventType: string;
  eventCategory: string;
  title: string;
  description?: string;
  actorName?: string;
  linkedEntityType?: string;
  eventAt: string;
}

interface TimelineResponse {
  patientId: string;
  totalCount: number;
  events: TimelineEvent[];
}

const categoryColors: Record<string, string> = {
  Clinical: 'bg-green-100 text-green-700',
  Financial: 'bg-yellow-100 text-yellow-700',
  Administrative: 'bg-blue-100 text-blue-700',
  Insurance: 'bg-cyan-100 text-cyan-700',
  Radiology: 'bg-purple-100 text-purple-700',
  Laboratory: 'bg-orange-100 text-orange-700',
};

const categoryAr: Record<string, string> = {
  Clinical: 'طبي', Financial: 'مالي', Administrative: 'إداري',
  Insurance: 'تأمين', Radiology: 'أشعة', Laboratory: 'مختبر',
};

const eventIcons: Record<string, string> = {
  'patient.registered': '👤',
  'patient.updated': '✏️',
  'appointment.scheduled': '🗓️',
  'appointment.confirmed': '✅',
  'appointment.completed': '✅',
  'appointment.cancelled': '❌',
  'appointment.noshow': '🚫',
  'queue.checkin': '📋',
  'queue.called': '📢',
  'queue.completed': '✓',
  'chart.updated': '🦷',
  'procedure.performed': '🦷',
  'treatment_plan.created': '📋',
  'treatment_plan.activated': '▶️',
  'treatment_plan.completed': '🏁',
  'media.uploaded': '📸',
  'doctor.assigned': '👨‍⚕️',
  'doctor.transferred': '🔄',
  'invoice.created': '💰',
  'invoice.paid': '💳',
  'insurance.claimed': '🛡️',
  'insurance.approved': '✅',
  'insurance.rejected': '❌',
};

function groupByMonth(events: TimelineEvent[]) {
  const groups: Record<string, TimelineEvent[]> = {};
  events.forEach((e) => {
    const month = new Date(e.eventAt).toLocaleDateString('ar-SA', { year: 'numeric', month: 'long' });
    if (!groups[month]) groups[month] = [];
    groups[month].push(e);
  });
  return groups;
}

export default function PatientTimelinePage() {
  const { id } = useParams<{ id: string }>();
  const token = useAuthStore((s) => s.accessToken);
  const [data, setData] = useState<TimelineResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [categoryFilter, setCategoryFilter] = useState('');
  const [page, setPage] = useState(1);

  useEffect(() => {
    if (!id || !token) return;
    const params = new URLSearchParams({ page: String(page), pageSize: '50' });
    if (categoryFilter) params.set('category', categoryFilter);

    api.get<TimelineResponse>(`/patients/${id}/timeline?${params}`)
      .then((r) => setData(r.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [id, token, categoryFilter, page]);

  if (loading) return <div className="p-6">جارٍ التحميل...</div>;

  const grouped = groupByMonth(data?.events ?? []);

  return (
    <div className="p-6 max-w-3xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold">سجل المريض الكامل</h1>
        {data && <span className="text-sm text-gray-500">{data.totalCount} حدث</span>}
      </div>

      {/* Category Filters */}
      <div className="flex gap-2 mb-6 flex-wrap">
        <button
          onClick={() => { setCategoryFilter(''); setPage(1); }}
          className={`text-xs px-3 py-1 rounded-full border ${!categoryFilter ? 'bg-blue-600 text-white border-blue-600' : 'border-gray-300'}`}
        >
          الكل
        </button>
        {Object.entries(categoryAr).map(([k, v]) => (
          <button
            key={k}
            onClick={() => { setCategoryFilter(k); setPage(1); }}
            className={`text-xs px-3 py-1 rounded-full border ${categoryFilter === k ? 'bg-blue-600 text-white border-blue-600' : 'border-gray-300'}`}
          >
            {v}
          </button>
        ))}
      </div>

      {/* Timeline Groups */}
      {Object.entries(grouped).map(([month, events]) => (
        <div key={month} className="mb-6">
          <div className="flex items-center gap-3 mb-3">
            <div className="w-3 h-3 rounded-full bg-blue-500" />
            <h2 className="font-semibold text-gray-700">{month}</h2>
          </div>
          <div className="mr-6 border-r-2 border-gray-200 pr-4 space-y-3">
            {events.map((event) => (
              <div key={event.id} className="bg-white rounded-xl border border-gray-100 p-4 hover:border-blue-200 transition-colors">
                <div className="flex items-start gap-3">
                  <span className="text-xl flex-shrink-0">{eventIcons[event.eventType] ?? '📌'}</span>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                      <h3 className="font-medium text-gray-900 text-sm">{event.title}</h3>
                      <span className={`text-xs px-1.5 py-0.5 rounded ${categoryColors[event.eventCategory]}`}>
                        {categoryAr[event.eventCategory]}
                      </span>
                    </div>
                    {event.description && (
                      <p className="text-sm text-gray-600 mt-1">{event.description}</p>
                    )}
                    <div className="flex items-center gap-3 mt-2 text-xs text-gray-400">
                      {event.actorName && <span>{event.actorName}</span>}
                      <span>
                        {new Date(event.eventAt).toLocaleDateString('ar-SA')}{' '}
                        {new Date(event.eventAt).toLocaleTimeString('ar-SA', { hour: '2-digit', minute: '2-digit' })}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      ))}

      {data?.totalCount === 0 && (
        <div className="bg-gray-50 rounded-xl p-8 text-center text-gray-500">
          لا توجد أحداث مسجلة
        </div>
      )}

      {/* Pagination */}
      {data && data.totalCount > 50 && (
        <div className="flex justify-center gap-3 mt-6">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page === 1}
            className="px-4 py-2 bg-gray-200 rounded-lg text-sm disabled:opacity-50"
          >
            السابق
          </button>
          <span className="px-4 py-2 text-sm text-gray-600">
            صفحة {page} / {Math.ceil(data.totalCount / 50)}
          </span>
          <button
            onClick={() => setPage((p) => p + 1)}
            disabled={page * 50 >= data.totalCount}
            className="px-4 py-2 bg-gray-200 rounded-lg text-sm disabled:opacity-50"
          >
            التالي
          </button>
        </div>
      )}
    </div>
  );
}
