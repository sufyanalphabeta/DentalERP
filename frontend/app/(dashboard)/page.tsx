"use client";

import { useAuthStore } from "@/stores/authStore";

export default function DashboardPage() {
  const user = useAuthStore((s) => s.user);

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-800 mb-2">
        مرحباً، {user?.fullName}
      </h1>
      <p className="text-gray-500 text-sm mb-8">نظام إدارة عيادة الأسنان</p>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <p className="text-sm text-gray-500">قيد التطوير — Phase 2</p>
          <p className="text-lg font-semibold text-gray-700 mt-1">المرضى اليوم</p>
        </div>
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <p className="text-sm text-gray-500">قيد التطوير — Phase 2</p>
          <p className="text-lg font-semibold text-gray-700 mt-1">المواعيد اليوم</p>
        </div>
        <div className="bg-white rounded-xl shadow-sm p-6 border border-gray-100">
          <p className="text-sm text-gray-500">قيد التطوير — Phase 4</p>
          <p className="text-lg font-semibold text-gray-700 mt-1">إيرادات اليوم</p>
        </div>
      </div>
    </div>
  );
}
