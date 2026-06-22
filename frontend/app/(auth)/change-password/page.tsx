"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";
import { useAuthStore } from "@/stores/authStore";

export default function ChangePasswordPage() {
  const router = useRouter();
  const { user, clearAuth } = useAuthStore();

  const [newPin, setNewPin] = useState("");
  const [confirmPin, setConfirmPin] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!/^\d{4,8}$/.test(newPin)) {
      setError("رقم التعريف يجب أن يكون 4 إلى 8 أرقام فقط");
      return;
    }
    if (newPin !== confirmPin) {
      setError("رقم التعريف وتأكيده غير متطابقين");
      return;
    }

    setSaving(true);
    try {
      await api.post("/auth/force-change-password", {
        newPassword: newPin,
        confirmPassword: confirmPin,
      });
      clearAuth();
      router.replace("/login");
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { detail?: string; title?: string } } })
        ?.response?.data?.detail ??
        (err as { response?: { data?: { title?: string } } })?.response?.data?.title ??
        "حدث خطأ، حاول مجدداً";
      setError(msg);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50" dir="rtl">
      <div className="w-full max-w-sm bg-white rounded-2xl shadow-md p-8">
        <div className="text-center mb-6">
          <div className="w-14 h-14 bg-amber-500 rounded-2xl flex items-center justify-center text-white text-2xl mx-auto mb-3">
            🔑
          </div>
          <h1 className="text-xl font-bold text-gray-800">تغيير رقم التعريف</h1>
          {user && (
            <p className="text-sm text-gray-500 mt-1">
              مرحباً <strong>{user.fullName}</strong> — يجب عليك تعيين رقم تعريف جديد
            </p>
          )}
        </div>

        <div className="bg-amber-50 border border-amber-200 rounded-lg p-3 mb-5 text-sm text-amber-800">
          ⚠️ رقم التعريف يجب أن يكون 4 إلى 8 أرقام فقط. احتفظ به في مكان آمن.
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg p-3 mb-4">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              رقم التعريف الجديد
            </label>
            <input
              type="password"
              inputMode="numeric"
              maxLength={8}
              value={newPin}
              onChange={(e) => {
                setNewPin(e.target.value.replace(/\D/g, "").slice(0, 8));
                setError(null);
              }}
              className="w-full border border-gray-300 rounded-lg px-3 py-2.5 text-center text-xl tracking-widest focus:outline-none focus:ring-2 focus:ring-blue-500"
              placeholder="• • • •"
              autoFocus
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              تأكيد رقم التعريف
            </label>
            <input
              type="password"
              inputMode="numeric"
              maxLength={8}
              value={confirmPin}
              onChange={(e) => {
                setConfirmPin(e.target.value.replace(/\D/g, "").slice(0, 8));
                setError(null);
              }}
              className={`w-full border rounded-lg px-3 py-2.5 text-center text-xl tracking-widest focus:outline-none focus:ring-2 focus:ring-blue-500 ${
                confirmPin && confirmPin !== newPin ? "border-red-400 bg-red-50" : "border-gray-300"
              }`}
              placeholder="• • • •"
            />
            {confirmPin && newPin && confirmPin !== newPin && (
              <p className="text-xs text-red-500 mt-1">الأرقام غير متطابقة</p>
            )}
          </div>

          <div className="flex gap-1 justify-center pt-1">
            {Array.from({ length: 8 }).map((_, i) => (
              <div
                key={i}
                className={`h-1.5 rounded-full flex-1 transition-colors ${
                  i < newPin.length ? "bg-blue-500" : "bg-gray-200"
                }`}
              />
            ))}
          </div>

          <button
            type="submit"
            disabled={saving || newPin.length < 4}
            className="w-full bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white font-medium py-2.5 rounded-lg transition-colors mt-2"
          >
            {saving ? "جارٍ الحفظ..." : "حفظ وتسجيل الدخول"}
          </button>
        </form>
      </div>
    </div>
  );
}
