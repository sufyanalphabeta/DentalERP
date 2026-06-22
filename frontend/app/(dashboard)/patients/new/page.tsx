"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { api } from "@/lib/api";

const schema = z.object({
  fullName: z.string().min(2, "الاسم مطلوب"),
  phone: z.string().min(7, "رقم الهاتف مطلوب"),
  phone2: z.string().optional(),
  gender: z.enum(["Male", "Female", ""]).optional(),
  dateOfBirth: z.string().optional(),
  nationalId: z.string().optional(),
  bloodType: z.string().optional(),
  email: z.string().email("بريد إلكتروني غير صالح").or(z.literal("")).optional(),
  address: z.string().optional(),
  allergies: z.string().optional(),
  chronicDiseases: z.string().optional(),
  notes: z.string().optional(),
});

type FormValues = z.infer<typeof schema>;

const bloodTypes = ["A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-"];

export default function NewPatientPage() {
  const router = useRouter();
  const [error, setError] = useState("");

  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
  });

  async function onSubmit(data: FormValues) {
    setError("");
    try {
      await api.post("/patients", {
        ...data,
        gender: data.gender || undefined,
        dateOfBirth: data.dateOfBirth || undefined,
        email: data.email || undefined,
        bloodType: data.bloodType || undefined,
      });
      router.push("/patients");
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string } } };
      setError(err.response?.data?.message ?? "حدث خطأ، يرجى المحاولة مجدداً.");
    }
  }

  return (
    <div className="p-6 max-w-2xl mx-auto">
      <h1 className="text-2xl font-bold text-gray-900 mb-6">تسجيل مريض جديد</h1>

      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-700 rounded-lg text-sm">
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="bg-white rounded-xl shadow p-6 space-y-4">
        <div className="grid grid-cols-2 gap-4">
          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">
              الاسم الكامل <span className="text-red-500">*</span>
            </label>
            <input
              {...register("fullName")}
              className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.fullName && <p className="text-red-500 text-xs mt-1">{errors.fullName.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              الهاتف <span className="text-red-500">*</span>
            </label>
            <input
              {...register("phone")}
              className="w-full border rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            {errors.phone && <p className="text-red-500 text-xs mt-1">{errors.phone.message}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">هاتف بديل</label>
            <input {...register("phone2")} className="w-full border rounded-lg px-3 py-2" />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">الجنس</label>
            <select {...register("gender")} className="w-full border rounded-lg px-3 py-2">
              <option value="">اختر...</option>
              <option value="Male">ذكر</option>
              <option value="Female">أنثى</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">تاريخ الميلاد</label>
            <input type="date" {...register("dateOfBirth")} className="w-full border rounded-lg px-3 py-2" />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">رقم الهوية</label>
            <input {...register("nationalId")} className="w-full border rounded-lg px-3 py-2" />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">فصيلة الدم</label>
            <select {...register("bloodType")} className="w-full border rounded-lg px-3 py-2">
              <option value="">اختر...</option>
              {bloodTypes.map(bt => <option key={bt} value={bt}>{bt}</option>)}
            </select>
          </div>

          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">البريد الإلكتروني</label>
            <input type="email" {...register("email")} className="w-full border rounded-lg px-3 py-2" />
            {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email.message}</p>}
          </div>

          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">العنوان</label>
            <input {...register("address")} className="w-full border rounded-lg px-3 py-2" />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">الحساسيات</label>
            <textarea {...register("allergies")} rows={2} className="w-full border rounded-lg px-3 py-2" />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">الأمراض المزمنة</label>
            <textarea {...register("chronicDiseases")} rows={2} className="w-full border rounded-lg px-3 py-2" />
          </div>

          <div className="col-span-2">
            <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
            <textarea {...register("notes")} rows={2} className="w-full border rounded-lg px-3 py-2" />
          </div>
        </div>

        <div className="flex gap-3 pt-2">
          <button
            type="submit"
            disabled={isSubmitting}
            className="flex-1 bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition"
          >
            {isSubmitting ? "جاري الحفظ..." : "حفظ"}
          </button>
          <button
            type="button"
            onClick={() => router.back()}
            className="px-6 py-2 border rounded-lg hover:bg-gray-50"
          >
            إلغاء
          </button>
        </div>
      </form>
    </div>
  );
}
