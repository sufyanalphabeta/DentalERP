"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface Patient {
  id: string;
  fileNumber: string;
  fullName: string;
  phone: string;
  phone2: string | null;
  email: string | null;
  dateOfBirth: string | null;
  gender: string | null;
  nationalId: string | null;
  bloodType: string | null;
  allergies: string | null;
  chronicDiseases: string | null;
  notes: string | null;
  isActive: boolean;
}

export default function PatientEditPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [patient, setPatient] = useState<Patient | null>(null);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState<Partial<Patient>>({});
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    api.get<Patient>(`/patients/${id}`).then((r) => {
      setPatient(r.data);
      setForm(r.data);
    }).catch(() => setError("تعذر تحميل بيانات المريض"))
      .finally(() => setLoading(false));
  }, [id]);

  async function save() {
    setSaving(true);
    setError(null);
    try {
      await api.put(`/patients/${id}`, {
        fullName: form.fullName,
        phone: form.phone,
        phone2: form.phone2 || null,
        email: form.email || null,
        dateOfBirth: form.dateOfBirth || null,
        gender: form.gender || null,
        nationalId: form.nationalId || null,
        bloodType: form.bloodType || null,
        allergies: form.allergies || null,
        chronicDiseases: form.chronicDiseases || null,
        notes: form.notes || null,
      });
      router.push(`/patients/${id}`);
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ أثناء الحفظ");
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <div className="p-6 text-center text-gray-400">جاري التحميل...</div>;
  if (!patient) return <div className="p-6 text-red-600">المريض غير موجود</div>;

  const f = (k: keyof Patient) => form[k] as string ?? "";
  const set = (k: keyof Patient, v: string) => setForm((prev) => ({ ...prev, [k]: v }));

  return (
    <div className="p-6 max-w-2xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">تعديل بيانات المريض</h1>
          <p className="text-gray-500 text-sm mt-0.5">{patient.fileNumber}</p>
        </div>
        <button onClick={() => router.back()} className="text-sm text-gray-600 border px-3 py-1.5 rounded-lg hover:bg-gray-50">رجوع</button>
      </div>

      {error && <div className="mb-4 text-sm text-red-600 bg-red-50 rounded-lg px-4 py-3">{error}</div>}

      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 space-y-4">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">الاسم الكامل *</label>
            <input className="w-full border rounded-lg px-3 py-2 text-sm" value={f("fullName")} onChange={(e) => set("fullName", e.target.value)} />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">رقم الهاتف *</label>
            <input className="w-full border rounded-lg px-3 py-2 text-sm" value={f("phone")} onChange={(e) => set("phone", e.target.value)} />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">هاتف 2</label>
            <input className="w-full border rounded-lg px-3 py-2 text-sm" value={f("phone2")} onChange={(e) => set("phone2", e.target.value)} />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">البريد الإلكتروني</label>
            <input type="email" className="w-full border rounded-lg px-3 py-2 text-sm" value={f("email")} onChange={(e) => set("email", e.target.value)} />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">تاريخ الميلاد</label>
            <input type="date" className="w-full border rounded-lg px-3 py-2 text-sm" value={f("dateOfBirth")} onChange={(e) => set("dateOfBirth", e.target.value)} />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">الجنس</label>
            <select className="w-full border rounded-lg px-3 py-2 text-sm" value={f("gender")} onChange={(e) => set("gender", e.target.value)}>
              <option value="">— اختر —</option>
              <option value="Male">ذكر</option>
              <option value="Female">أنثى</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">رقم الهوية</label>
            <input className="w-full border rounded-lg px-3 py-2 text-sm" value={f("nationalId")} onChange={(e) => set("nationalId", e.target.value)} />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">فصيلة الدم</label>
            <select className="w-full border rounded-lg px-3 py-2 text-sm" value={f("bloodType")} onChange={(e) => set("bloodType", e.target.value)}>
              <option value="">— غير محدد —</option>
              {["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"].map((b) => <option key={b} value={b}>{b}</option>)}
            </select>
          </div>
          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">الحساسيات</label>
            <textarea className="w-full border rounded-lg px-3 py-2 text-sm" rows={2} value={f("allergies")} onChange={(e) => set("allergies", e.target.value)} />
          </div>
          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">الأمراض المزمنة</label>
            <textarea className="w-full border rounded-lg px-3 py-2 text-sm" rows={2} value={f("chronicDiseases")} onChange={(e) => set("chronicDiseases", e.target.value)} />
          </div>
          <div className="md:col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
            <textarea className="w-full border rounded-lg px-3 py-2 text-sm" rows={3} value={f("notes")} onChange={(e) => set("notes", e.target.value)} />
          </div>
        </div>

        <div className="flex gap-3 pt-2">
          <button onClick={save} disabled={saving || !form.fullName || !form.phone}
            className="flex-1 bg-blue-600 text-white py-2.5 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
            {saving ? "جاري الحفظ..." : "حفظ التعديلات"}
          </button>
          <button onClick={() => router.push(`/patients/${id}`)} className="flex-1 border py-2.5 rounded-lg text-sm text-gray-700 hover:bg-gray-50">
            إلغاء
          </button>
        </div>
      </div>
    </div>
  );
}
