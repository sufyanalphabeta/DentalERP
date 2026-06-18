'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import { useAuthStore } from '@/stores/authStore';

interface PatientMedia {
  id: string;
  mediaType: string;
  fileName: string;
  title?: string;
  description?: string;
  toothId?: number;
  isRequired: boolean;
  isApproved: boolean;
  uploadedAt: string;
  fileSizeBytes?: number;
}

const mediaTypeAr: Record<string, string> = {
  Before: 'قبل العلاج', After: 'بعد العلاج',
  OPG: 'أشعة بانورامية', CBCT: 'أشعة CBCT',
  XRay: 'أشعة عادية', Document: 'وثيقة',
};

const mediaTypeColors: Record<string, string> = {
  Before: 'bg-orange-100 text-orange-700',
  After: 'bg-green-100 text-green-700',
  OPG: 'bg-blue-100 text-blue-700',
  CBCT: 'bg-purple-100 text-purple-700',
  XRay: 'bg-cyan-100 text-cyan-700',
  Document: 'bg-gray-100 text-gray-700',
};

function formatBytes(bytes?: number): string {
  if (!bytes) return '';
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / 1024 / 1024).toFixed(1)} MB`;
}

export default function MediaLibraryPage() {
  const { id } = useParams<{ id: string }>();
  const token = useAuthStore((s) => s.token);
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const [media, setMedia] = useState<PatientMedia[]>([]);
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState('');
  const [showAdd, setShowAdd] = useState(false);
  const [form, setForm] = useState({
    mediaType: 'XRay',
    title: '',
    description: '',
    toothId: '',
    isRequired: false,
    // In production: file upload to MinIO; here we simulate with a path
    fileName: '',
    filePath: '',
  });

  const load = () => {
    if (!id || !token) return;
    fetch(`/api/patients/${id}/media`, { headers: { Authorization: `Bearer ${token}` } })
      .then((r) => r.json())
      .then(setMedia)
      .catch(console.error)
      .finally(() => setLoading(false));
  };

  useEffect(load, [id, token]);

  const handleUpload = async () => {
    await fetch(`/api/patients/${id}/media`, {
      method: 'POST',
      headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json' },
      body: JSON.stringify({
        mediaType: form.mediaType,
        fileName: form.fileName,
        filePath: `patient-media/${id}/${form.fileName}`,
        title: form.title || null,
        description: form.description || null,
        toothId: form.toothId ? parseInt(form.toothId) : null,
        isRequired: form.isRequired,
      }),
    });
    setShowAdd(false);
    setLoading(true);
    load();
  };

  const filtered = filter ? media.filter((m) => m.mediaType === filter) : media;

  if (loading) return <div className="p-6">جارٍ التحميل...</div>;

  return (
    <div className="p-6 max-w-4xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-4">
        <h1 className="text-xl font-bold">مكتبة الوسائط</h1>
        {hasPermission('Patients.Edit') && (
          <button
            onClick={() => setShowAdd(true)}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm"
          >
            + رفع وسيط
          </button>
        )}
      </div>

      {/* Filters */}
      <div className="flex gap-2 mb-4 flex-wrap">
        <button
          onClick={() => setFilter('')}
          className={`text-xs px-3 py-1 rounded-full border ${!filter ? 'bg-blue-600 text-white border-blue-600' : 'border-gray-300'}`}
        >
          الكل ({media.length})
        </button>
        {Object.entries(mediaTypeAr).map(([k, v]) => {
          const count = media.filter((m) => m.mediaType === k).length;
          if (count === 0) return null;
          return (
            <button
              key={k}
              onClick={() => setFilter(k)}
              className={`text-xs px-3 py-1 rounded-full border ${filter === k ? 'bg-blue-600 text-white border-blue-600' : 'border-gray-300'}`}
            >
              {v} ({count})
            </button>
          );
        })}
      </div>

      {filtered.length === 0 ? (
        <div className="bg-gray-50 rounded-xl p-8 text-center text-gray-500">لا توجد ملفات</div>
      ) : (
        <div className="grid grid-cols-2 gap-4">
          {filtered.map((m) => (
            <div key={m.id} className="bg-white rounded-xl border border-gray-200 p-4">
              <div className="flex items-start justify-between mb-2">
                <span className={`text-xs px-2 py-0.5 rounded ${mediaTypeColors[m.mediaType]}`}>
                  {mediaTypeAr[m.mediaType]}
                </span>
                <div className="flex gap-1">
                  {m.isRequired && (
                    <span className="text-xs bg-red-50 text-red-600 px-1.5 py-0.5 rounded">مطلوب</span>
                  )}
                  {m.isApproved ? (
                    <span className="text-xs bg-green-50 text-green-600 px-1.5 py-0.5 rounded">✓ معتمد</span>
                  ) : (
                    <span className="text-xs bg-yellow-50 text-yellow-600 px-1.5 py-0.5 rounded">بانتظار الموافقة</span>
                  )}
                </div>
              </div>
              <p className="font-medium text-gray-800 text-sm truncate">{m.title || m.fileName}</p>
              {m.description && <p className="text-xs text-gray-500 mt-1">{m.description}</p>}
              <div className="flex justify-between mt-2 text-xs text-gray-400">
                {m.toothId && <span>سن: {m.toothId}</span>}
                {m.fileSizeBytes && <span>{formatBytes(m.fileSizeBytes)}</span>}
                <span>{new Date(m.uploadedAt).toLocaleDateString('ar-SA')}</span>
              </div>
            </div>
          ))}
        </div>
      )}

      {showAdd && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-96" dir="rtl">
            <h3 className="font-bold text-lg mb-4">رفع وسيط جديد</h3>
            <div className="space-y-3">
              <select
                value={form.mediaType}
                onChange={(e) => setForm((f) => ({ ...f, mediaType: e.target.value }))}
                className="w-full border rounded-lg p-2"
              >
                {Object.entries(mediaTypeAr).map(([k, v]) => (
                  <option key={k} value={k}>{v}</option>
                ))}
              </select>
              <input
                placeholder="اسم الملف *"
                value={form.fileName}
                onChange={(e) => setForm((f) => ({ ...f, fileName: e.target.value }))}
                className="w-full border rounded-lg p-2"
              />
              <input
                placeholder="العنوان"
                value={form.title}
                onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
                className="w-full border rounded-lg p-2"
              />
              <input
                type="number"
                placeholder="رقم السن (اختياري)"
                value={form.toothId}
                onChange={(e) => setForm((f) => ({ ...f, toothId: e.target.value }))}
                className="w-full border rounded-lg p-2"
              />
              <textarea
                placeholder="وصف"
                value={form.description}
                onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                className="w-full border rounded-lg p-2 h-16"
              />
              <label className="flex items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  checked={form.isRequired}
                  onChange={(e) => setForm((f) => ({ ...f, isRequired: e.target.checked }))}
                />
                هذا الوسيط مطلوب
              </label>
            </div>
            <div className="flex gap-2 mt-4">
              <button
                onClick={handleUpload}
                disabled={!form.fileName}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg disabled:opacity-50"
              >
                رفع
              </button>
              <button onClick={() => setShowAdd(false)} className="flex-1 bg-gray-200 py-2 rounded-lg">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
