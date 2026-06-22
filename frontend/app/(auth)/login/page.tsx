"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";
import { useAuthStore } from "@/stores/authStore";
import type { LoginResponse } from "@/types/auth";

interface UserListItem {
  id: string;
  username: string;
  fullName: string;
}

export default function LoginPage() {
  const router = useRouter();
  const setAuth = useAuthStore((s) => s.setAuth);

  const [users, setUsers] = useState<UserListItem[]>([]);
  const [selectedUsername, setSelectedUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [loadingUsers, setLoadingUsers] = useState(true);

  useEffect(() => {
    api
      .get<UserListItem[]>("/auth/users-list")
      .then((r) => setUsers(r.data))
      .catch(() => setError("تعذّر تحميل قائمة المستخدمين"))
      .finally(() => setLoadingUsers(false));
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedUsername) { setError("يرجى اختيار المستخدم"); return; }
    if (!password) { setError("يرجى إدخال رقم التعريف"); return; }
    if (!/^\d{4,8}$/.test(password)) {
      setError("رقم التعريف يجب أن يكون 4 إلى 8 أرقام فقط");
      return;
    }

    setError(null);
    setLoading(true);
    try {
      const res = await api.post<LoginResponse>("/auth/login", {
        username: selectedUsername,
        password,
      });
      const { userId, username, fullName, permissions, accessToken, refreshToken, mustChangePassword } = res.data;
      setAuth({ userId, username, fullName, permissions }, accessToken, refreshToken);

      if (mustChangePassword) {
        router.replace("/change-password");
      } else {
        router.replace("/");
      }
    } catch {
      setError("اسم المستخدم أو رقم التعريف غير صحيح");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50" dir="rtl">
      <div className="w-full max-w-sm bg-white rounded-2xl shadow-md p-8">
        <div className="text-center mb-8">
          <div className="w-16 h-16 bg-blue-600 rounded-2xl flex items-center justify-center text-white text-3xl mx-auto mb-4">
            🦷
          </div>
          <h1 className="text-2xl font-bold text-gray-800">نظام إدارة العيادة</h1>
          <p className="text-sm text-gray-500 mt-1">تسجيل الدخول</p>
        </div>

        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 text-sm rounded-lg p-3 mb-4">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              المستخدم
            </label>
            {loadingUsers ? (
              <div className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm text-gray-400 bg-gray-50">
                جارٍ التحميل...
              </div>
            ) : (
              <select
                value={selectedUsername}
                onChange={(e) => { setSelectedUsername(e.target.value); setError(null); }}
                className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white"
              >
                <option value="">— اختر اسمك —</option>
                {users.map((u) => (
                  <option key={u.id} value={u.username}>
                    {u.fullName}
                  </option>
                ))}
              </select>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              رقم التعريف (PIN)
            </label>
            <input
              type="password"
              inputMode="numeric"
              pattern="\d{4,8}"
              maxLength={8}
              value={password}
              onChange={(e) => {
                const val = e.target.value.replace(/\D/g, "").slice(0, 8);
                setPassword(val);
                setError(null);
              }}
              className="w-full border border-gray-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 tracking-widest text-center text-lg"
              placeholder="• • • • • •"
              autoComplete="current-password"
            />
            <p className="text-xs text-gray-400 mt-1 text-center">4 إلى 8 أرقام</p>
          </div>

          <button
            type="submit"
            disabled={loading || loadingUsers}
            className="w-full bg-blue-600 hover:bg-blue-700 disabled:opacity-50 text-white font-medium py-2.5 rounded-lg transition-colors"
          >
            {loading ? "جارٍ الدخول..." : "دخول"}
          </button>
        </form>

        <p className="text-center text-xs text-gray-400 mt-6">
          DentalERP — النظام المحلي للعيادات
        </p>
      </div>
    </div>
  );
}
