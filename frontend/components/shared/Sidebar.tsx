"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuthStore } from "@/stores/authStore";

const navItems = [
  { href: "/", label: "الرئيسية", permission: null },
  { href: "/patients", label: "المرضى", permission: "Patients.View" },
  { href: "/scheduling/calendar", label: "المواعيد", permission: "Appointments.View" },
  { href: "/invoices", label: "الفواتير", permission: "Treasury.View" },
  { href: "/treasury", label: "الخزينة", permission: "Treasury.View" },
  { href: "/inventory", label: "المخزون", permission: "Inventory.View" },
  { href: "/laboratory", label: "المعمل", permission: "Lab.View" },
  { href: "/radiology", label: "الأشعة", permission: "Radiology.View" },
  { href: "/reports", label: "التقارير", permission: "Reports.View" },
  { href: "/admin/users", label: "المستخدمون", permission: "Users.View" },
  { href: "/admin/clinic-settings", label: "الإعدادات", permission: "Settings.View" },
];

export function Sidebar() {
  const pathname = usePathname();
  const { user, hasPermission, clearAuth } = useAuthStore();

  const filtered = navItems.filter(
    (item) => !item.permission || hasPermission(item.permission)
  );

  return (
    <aside className="w-64 min-h-screen bg-gray-900 text-white flex flex-col" dir="rtl">
      <div className="p-4 border-b border-gray-700">
        <h2 className="font-bold text-lg">🦷 DentalERP</h2>
        <p className="text-xs text-gray-400 mt-1">{user?.fullName}</p>
      </div>

      <nav className="flex-1 p-3 space-y-1 overflow-y-auto">
        {filtered.map((item) => {
          const active =
            item.href === "/" ? pathname === "/" : pathname.startsWith(item.href);
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex items-center gap-2 px-3 py-2 rounded-lg text-sm transition-colors ${
                active
                  ? "bg-blue-600 text-white"
                  : "text-gray-300 hover:bg-gray-700"
              }`}
            >
              {item.label}
            </Link>
          );
        })}
      </nav>

      <div className="p-3 border-t border-gray-700">
        <button
          onClick={() => {
            clearAuth();
            window.location.href = "/login";
          }}
          className="w-full text-sm text-gray-400 hover:text-white px-3 py-2 rounded-lg hover:bg-gray-700 transition-colors text-right"
        >
          تسجيل الخروج
        </button>
      </div>
    </aside>
  );
}
