'use client';

import { useEffect, useState } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { useAuthStore } from '@/stores/authStore';

interface PatientDetail {
  id: string;
  fileNumber: string;
  fullName: string;
  phone: string;
  phone2?: string;
  email?: string;
  dateOfBirth?: string;
  gender?: string;
  nationalId?: string;
  bloodType?: string;
  allergies?: string;
  chronicDiseases?: string;
  notes?: string;
  isActive: boolean;
  createdAt: string;
}

export default function PatientDetailPage() {
  const { id } = useParams<{ id: string }>();
  const token = useAuthStore((s) => s.token);
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const [patient, setPatient] = useState<PatientDetail | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!id || !token) return;
    fetch(`/api/patients/${id}`, {
      headers: { Authorization: `Bearer ${token}` },
    })
      .then((r) => r.json())
      .then(setPatient)
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [id, token]);

  if (loading) return <div className="p-6">جارٍ التحميل...</div>;
  if (!patient) return <div className="p-6 text-red-600">المريض غير موجود</div>;

  const navLinks = [
    { href: `/patients/${id}/chart`, label: 'مخطط الأسنان', icon: '🦷' },
    { href: `/patients/${id}/treatment-plans`, label: 'خطط العلاج', icon: '📋' },
    { href: `/patients/${id}/media`, label: 'الوسائط', icon: '📸' },
    { href: `/patients/${id}/timeline`, label: 'السجل الكامل', icon: '📅' },
  ];

  return (
    <div className="p-6 max-w-5xl mx-auto" dir="rtl">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{patient.fullName}</h1>
          <p className="text-gray-500 text-sm mt-1">
            {patient.fileNumber} •{' '}
            <span className={patient.isActive ? 'text-green-600' : 'text-red-500'}>
              {patient.isActive ? 'نشط' : 'غير نشط'}
            </span>
          </p>
        </div>
        {hasPermission('Patients.Edit') && (
          <Link
            href={`/patients/${id}/edit`}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
          >
            تعديل البيانات
          </Link>
        )}
      </div>

      {/* Quick Nav */}
      <div className="grid grid-cols-4 gap-3 mb-8">
        {navLinks.map((link) => (
          <Link
            key={link.href}
            href={link.href}
            className="bg-white border border-gray-200 rounded-xl p-4 text-center hover:border-blue-400 hover:shadow-md transition-all"
          >
            <div className="text-2xl mb-1">{link.icon}</div>
            <div className="text-sm font-medium text-gray-700">{link.label}</div>
          </Link>
        ))}
      </div>

      {/* Info Cards */}
      <div className="grid grid-cols-2 gap-6">
        {/* Basic Info */}
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="font-semibold text-gray-800 mb-4 border-b pb-2">البيانات الأساسية</h2>
          <dl className="space-y-3 text-sm">
            <div className="flex justify-between">
              <dt className="text-gray-500">الهاتف</dt>
              <dd className="font-medium">{patient.phone}</dd>
            </div>
            {patient.phone2 && (
              <div className="flex justify-between">
                <dt className="text-gray-500">هاتف بديل</dt>
                <dd className="font-medium">{patient.phone2}</dd>
              </div>
            )}
            {patient.email && (
              <div className="flex justify-between">
                <dt className="text-gray-500">البريد الإلكتروني</dt>
                <dd className="font-medium">{patient.email}</dd>
              </div>
            )}
            {patient.gender && (
              <div className="flex justify-between">
                <dt className="text-gray-500">الجنس</dt>
                <dd className="font-medium">{patient.gender === 'Male' ? 'ذكر' : 'أنثى'}</dd>
              </div>
            )}
            {patient.dateOfBirth && (
              <div className="flex justify-between">
                <dt className="text-gray-500">تاريخ الميلاد</dt>
                <dd className="font-medium">{new Date(patient.dateOfBirth).toLocaleDateString('ar-SA')}</dd>
              </div>
            )}
            {patient.nationalId && (
              <div className="flex justify-between">
                <dt className="text-gray-500">رقم الهوية</dt>
                <dd className="font-medium">{patient.nationalId}</dd>
              </div>
            )}
          </dl>
        </div>

        {/* Medical Info */}
        <div className="bg-white rounded-xl border border-gray-200 p-5">
          <h2 className="font-semibold text-gray-800 mb-4 border-b pb-2">المعلومات الطبية</h2>
          <dl className="space-y-3 text-sm">
            {patient.bloodType && (
              <div className="flex justify-between">
                <dt className="text-gray-500">فصيلة الدم</dt>
                <dd className="font-medium">{patient.bloodType}</dd>
              </div>
            )}
            {patient.allergies && (
              <div>
                <dt className="text-gray-500 mb-1">الحساسية</dt>
                <dd className="bg-red-50 text-red-800 rounded p-2">{patient.allergies}</dd>
              </div>
            )}
            {patient.chronicDiseases && (
              <div>
                <dt className="text-gray-500 mb-1">الأمراض المزمنة</dt>
                <dd className="bg-yellow-50 text-yellow-800 rounded p-2">{patient.chronicDiseases}</dd>
              </div>
            )}
            {patient.notes && (
              <div>
                <dt className="text-gray-500 mb-1">ملاحظات</dt>
                <dd className="text-gray-700">{patient.notes}</dd>
              </div>
            )}
          </dl>
        </div>
      </div>

      <p className="text-xs text-gray-400 mt-4 text-left">
        تسجيل: {new Date(patient.createdAt).toLocaleDateString('ar-SA')}
      </p>
    </div>
  );
}
