"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import {
  LayoutDashboard,
  MonitorSmartphone,
  HeartPulse,
  FlaskConical,
  ScanLine,
  CreditCard,
  Landmark,
  FileText,
  Package,
  ShoppingCart,
  Building2,
  BarChart3,
  Settings,
  LogOut,
  ChevronDown,
  ChevronUp,
  type LucideIcon,
} from "lucide-react";
import { useAuthStore } from "@/stores/authStore";
import { useLangStore } from "@/stores/langStore";

interface NavItem {
  href: string;
  label: string;
  labelEn: string;
  permission?: string;
}

interface NavGroup {
  label: string;
  labelEn: string;
  icon: LucideIcon;
  permission?: string;
  section?: string;
  items: NavItem[];
}

/* Section labels for visual dividers */
const SECTIONS: Record<string, { ar: string; en: string }> = {
  operations: { ar: "العمليات", en: "OPERATIONS" },
  financial:  { ar: "المالية",  en: "FINANCIAL"  },
  logistics:  { ar: "اللوجستيات", en: "LOGISTICS" },
  admin:      { ar: "الإدارة",  en: "ADMIN"      },
};

const NAV: NavGroup[] = [
  /* ── Operations ─────────────────────────────────────────────── */
  {
    label: "الاستقبال", labelEn: "Reception",
    icon: MonitorSmartphone,
    section: "operations",
    items: [
      { href: "/reception",    label: "مساحة الاستقبال", labelEn: "Reception Desk",   permission: "Appointments.Queue.View" },
      { href: "/patients",     label: "المرضى",           labelEn: "Patients",         permission: "Patients.Patients.View" },
      { href: "/appointments", label: "المواعيد",          labelEn: "Appointments",     permission: "Appointments.Appointments.View" },
      { href: "/queue",        label: "طابور الانتظار",    labelEn: "Waiting Queue",    permission: "Appointments.Queue.View" },
    ],
  },
  {
    label: "السريرية", labelEn: "Clinical",
    icon: HeartPulse,
    section: "operations",
    permission: "Clinical.Workspace.View",
    items: [
      { href: "/clinical/workspace", label: "مساحة الطبيب", labelEn: "Doctor's Workspace", permission: "Clinical.Workspace.View" },
    ],
  },
  {
    label: "المختبر", labelEn: "Laboratory",
    icon: FlaskConical,
    section: "operations",
    permission: "Lab.Orders.View",
    items: [
      { href: "/lab/orders",        label: "طلبات المختبر",    labelEn: "Lab Orders",       permission: "Lab.Orders.View" },
      { href: "/lab/external-labs", label: "المختبرات الخارجية",labelEn: "External Labs",   permission: "Lab.ExternalLabs.View" },
    ],
  },
  {
    label: "الأشعة", labelEn: "Radiology",
    icon: ScanLine,
    section: "operations",
    permission: "Radiology.Orders.View",
    items: [
      { href: "/radiology/orders", label: "طلبات الأشعة", labelEn: "Radiology Orders", permission: "Radiology.Orders.View" },
    ],
  },

  /* ── Financial ───────────────────────────────────────────────── */
  {
    label: "الصندوق", labelEn: "Cashier",
    icon: CreditCard,
    section: "financial",
    permission: "Financial.Invoices.View",
    items: [
      { href: "/finance/cashier",      label: "مساحة الصراف", labelEn: "Cashier Desk",   permission: "Financial.CashierDesk.View" },
      { href: "/finance/invoices",     label: "الفواتير",     labelEn: "Invoices",       permission: "Financial.Invoices.View" },
      { href: "/finance/installments", label: "الأقساط",      labelEn: "Installments",   permission: "Financial.Installments.View" },
    ],
  },
  {
    label: "الخزينة", labelEn: "Treasury",
    icon: Landmark,
    section: "financial",
    permission: "Financial.Treasury.View",
    items: [
      { href: "/finance/treasury",    label: "أرصدة الخزائن",        labelEn: "Vault Balances",     permission: "Financial.Treasury.View" },
      { href: "/treasury/movements",  label: "حركة النقدية",         labelEn: "Cash Movements",     permission: "Financial.Treasury.View" },
      { href: "/treasury/transfers",  label: "التحويلات بين الخزائن",labelEn: "Vault Transfers",    permission: "Financial.Treasury.Transfer" },
      { href: "/expenses",            label: "المصروفات",            labelEn: "Expenses",           permission: "Financial.Expenses.View" },
      { href: "/expenses/categories", label: "فئات المصروفات",       labelEn: "Expense Categories", permission: "Financial.Expenses.View" },
    ],
  },
  {
    label: "الذمم", labelEn: "Receivables",
    icon: FileText,
    section: "financial",
    permission: "Financial.Doctors.View",
    items: [
      { href: "/finance/doctors",              label: "حسابات الأطباء",   labelEn: "Doctor Accounts",       permission: "Financial.Doctors.View" },
      { href: "/finance/supplier-payments",    label: "مدفوعات الموردين", labelEn: "Supplier Payments",     permission: "Purchasing.Invoices.View" },
      { href: "/finance/insurance/claims",     label: "مطالبات التأمين",  labelEn: "Insurance Claims",      permission: "Insurance.Claims.View" },
      { href: "/finance/insurance/receivables",label: "مستحقات التأمين",  labelEn: "Insurance Receivables", permission: "Insurance.Receivables.View" },
    ],
  },

  /* ── Logistics ───────────────────────────────────────────────── */
  {
    label: "المخزون", labelEn: "Inventory",
    icon: Package,
    section: "logistics",
    permission: "Inventory.Items.View",
    items: [
      { href: "/inventory/alerts",     label: "تنبيهات المخزون", labelEn: "Stock Alerts",    permission: "Inventory.Alerts.View" },
      { href: "/inventory/items",      label: "الأصناف",         labelEn: "Items",           permission: "Inventory.Items.View" },
      { href: "/inventory/movements",  label: "حركة المخزون",    labelEn: "Stock Movements", permission: "Inventory.Movements.View" },
      { href: "/inventory/warehouses", label: "المستودعات",      labelEn: "Warehouses",      permission: "Inventory.Items.View" },
      { href: "/inventory/categories", label: "الفئات",          labelEn: "Categories",      permission: "Inventory.Items.View" },
    ],
  },
  {
    label: "المشتريات", labelEn: "Purchasing",
    icon: ShoppingCart,
    section: "logistics",
    permission: "Purchasing.Suppliers.View",
    items: [
      { href: "/purchasing/suppliers", label: "الموردون",          labelEn: "Suppliers",        permission: "Purchasing.Suppliers.View" },
      { href: "/purchasing/invoices",  label: "فواتير المشتريات",  labelEn: "Purchase Invoices",permission: "Purchasing.Invoices.View" },
      { href: "/purchasing/returns",   label: "مردودات المشتريات", labelEn: "Purchase Returns", permission: "Purchasing.Returns.View" },
    ],
  },
  {
    label: "الأصول الثابتة", labelEn: "Fixed Assets",
    icon: Building2,
    section: "logistics",
    permission: "Assets.Assets.View",
    items: [
      { href: "/assets",            label: "سجل الأصول", labelEn: "Assets Registry", permission: "Assets.Assets.View" },
      { href: "/assets/categories", label: "الفئات",     labelEn: "Categories",      permission: "Assets.Categories.View" },
    ],
  },

  /* ── Admin ───────────────────────────────────────────────────── */
  {
    label: "التقارير", labelEn: "Reports",
    icon: BarChart3,
    section: "admin",
    permission: "Reports.Financial.View",
    items: [
      { href: "/reports",            label: "مركز التقارير",   labelEn: "Reports Center",    permission: "Reports.Financial.View" },
      { href: "/reports/operational",label: "التقرير التشغيلي", labelEn: "Operational",       permission: "Reports.Operational.View" },
      { href: "/reports/expenses",   label: "تقرير المصروفات",  labelEn: "Expenses Report",   permission: "Reports.Financial.View" },
      { href: "/reports/purchasing", label: "تقرير المشتريات",  labelEn: "Purchasing Report", permission: "Reports.Purchasing.View" },
    ],
  },
  {
    label: "الإعدادات", labelEn: "Settings",
    icon: Settings,
    section: "admin",
    permission: "IAM.Settings.View",
    items: [
      { href: "/settings/users",              label: "المستخدمون",        labelEn: "Users",                  permission: "IAM.Users.View" },
      { href: "/settings/roles",              label: "الأدوار والصلاحيات", labelEn: "Roles & Permissions",    permission: "IAM.Roles.View" },
      { href: "/settings/doctors",            label: "الأطباء",            labelEn: "Doctors",                permission: "IAM.Doctors.View" },
      { href: "/settings/services",           label: "الخدمات",            labelEn: "Services",               permission: "IAM.Services.View" },
      { href: "/settings/services/categories",label: "فئات الخدمات",       labelEn: "Service Categories",     permission: "IAM.Services.View" },
      { href: "/settings/insurance",          label: "شركات التأمين",      labelEn: "Insurance Companies",    permission: "IAM.Insurance.View" },
      { href: "/settings/vaults",             label: "الخزائن",            labelEn: "Vaults",                 permission: "IAM.Vaults.View" },
      { href: "/settings/system",             label: "إعدادات النظام",     labelEn: "System Settings",        permission: "IAM.Settings.View" },
    ],
  },
];

interface SidebarProps {
  onClose?: () => void;
}

export function Sidebar({ onClose }: SidebarProps) {
  const pathname = usePathname();
  const { user, hasPermission, clearAuth } = useAuthStore();
  const { lang, setLang } = useLangStore();
  const isAr = lang === "ar";

  const lbl = (ar: string, en: string) => (isAr ? ar : en);

  const [openGroups, setOpenGroups] = useState<Record<string, boolean>>(() => {
    const defaults: Record<string, boolean> = {};
    NAV.forEach((g) => {
      if (g.items.some((i) => pathname === i.href || pathname.startsWith(i.href + "/"))) {
        defaults[g.label] = true;
      }
    });
    return defaults;
  });

  const canSeeItem = (item: NavItem) =>
    !item.permission || hasPermission(item.permission);

  const canSeeGroup = (group: NavGroup) =>
    (!group.permission || hasPermission(group.permission)) &&
    group.items.some(canSeeItem);

  const toggle = (label: string) =>
    setOpenGroups((prev) => ({ ...prev, [label]: !prev[label] }));

  const isActive = (href: string) =>
    href === "/" ? pathname === "/" : pathname === href || pathname.startsWith(href + "/");

  /* Build ordered list with section dividers */
  const visibleGroups = NAV.filter(canSeeGroup);
  const renderedSections = new Set<string>();

  return (
    <aside
      className="w-60 min-h-screen flex flex-col flex-shrink-0"
      style={{ background: "var(--c-sidebar-bg)" }}
      dir="rtl"
    >
      {/* ── Logo ─────────────────────────────────────────────────── */}
      <div
        className="px-4 py-3.5 border-b flex items-center gap-3"
        style={{ borderColor: "var(--c-sidebar-hover)" }}
      >
        <div
          className="w-8 h-8 rounded-lg flex items-center justify-center shrink-0"
          style={{ background: "var(--c-sidebar-active)" }}
        >
          <HeartPulse size={16} className="text-white" />
        </div>
        <div className="min-w-0">
          <div className="text-[13px] font-bold text-white leading-tight">DentalERP</div>
          <div
            className="text-[11px] leading-tight truncate"
            style={{ color: "var(--c-sidebar-text-dim)" }}
          >
            {user?.fullName}
          </div>
        </div>
      </div>

      {/* ── Dashboard ────────────────────────────────────────────── */}
      {hasPermission("Dashboard.Overview.View") && (
        <div className="px-2.5 pt-2.5">
          <Link
            href="/"
            onClick={onClose}
            className="flex items-center gap-2.5 px-3 py-2 rounded-lg text-[13px] font-medium transition-colors"
            style={{
              color: pathname === "/" ? "white" : "var(--c-sidebar-text)",
              background: pathname === "/" ? "var(--c-sidebar-active)" : "transparent",
            }}
            onMouseEnter={(e) => {
              if (pathname !== "/")
                (e.currentTarget as HTMLElement).style.background = "var(--c-sidebar-hover)";
            }}
            onMouseLeave={(e) => {
              if (pathname !== "/")
                (e.currentTarget as HTMLElement).style.background = "transparent";
            }}
          >
            <LayoutDashboard size={15} className="shrink-0" />
            <span>{lbl("لوحة القيادة", "Dashboard")}</span>
          </Link>
        </div>
      )}

      {/* ── Nav Groups ───────────────────────────────────────────── */}
      <nav className="flex-1 px-2.5 pb-2 overflow-y-auto mt-1 space-y-0.5">
        {visibleGroups.map((group) => {
          const visibleItems = group.items.filter(canSeeItem);
          const isOpen = openGroups[group.label] ?? false;
          const hasActive = visibleItems.some((i) => isActive(i.href));
          const Icon = group.icon;

          /* Section divider */
          let divider: React.ReactNode = null;
          const sec = group.section;
          if (sec && !renderedSections.has(sec)) {
            renderedSections.add(sec);
            divider = (
              <div
                key={`sec-${sec}`}
                className="px-3 pt-3 pb-1 text-[10px] font-semibold uppercase tracking-widest"
                style={{ color: "var(--c-sidebar-text-dim)" }}
              >
                {lbl(SECTIONS[sec].ar, SECTIONS[sec].en)}
              </div>
            );
          }

          return (
            <div key={group.label}>
              {divider}
              {/* Group header */}
              <button
                onClick={() => toggle(group.label)}
                className="w-full flex items-center justify-between px-3 py-2 rounded-lg transition-colors text-[12px] font-semibold"
                style={{
                  color: hasActive ? "white" : "var(--c-sidebar-text)",
                  background: hasActive ? "var(--c-sidebar-hover)" : "transparent",
                }}
                onMouseEnter={(e) => {
                  if (!hasActive)
                    (e.currentTarget as HTMLElement).style.background = "var(--c-sidebar-hover)";
                }}
                onMouseLeave={(e) => {
                  if (!hasActive)
                    (e.currentTarget as HTMLElement).style.background = "transparent";
                }}
              >
                <div className="flex items-center gap-2.5">
                  <Icon size={15} className="shrink-0" />
                  <span>{lbl(group.label, group.labelEn)}</span>
                </div>
                {isOpen ? (
                  <ChevronUp size={12} style={{ color: "var(--c-sidebar-text-dim)" }} />
                ) : (
                  <ChevronDown size={12} style={{ color: "var(--c-sidebar-text-dim)" }} />
                )}
              </button>

              {/* Items */}
              {isOpen && (
                <div
                  className="mt-0.5 ms-4 space-y-0.5 border-s ps-2"
                  style={{ borderColor: "var(--c-sidebar-hover)" }}
                >
                  {visibleItems.map((item) => {
                    const active = isActive(item.href);
                    return (
                      <Link
                        key={item.href}
                        href={item.href}
                        onClick={onClose}
                        className="block px-3 py-1.5 rounded-md text-[12px] transition-colors"
                        style={{
                          color: active ? "white" : "var(--c-sidebar-text)",
                          background: active ? "var(--c-sidebar-active)" : "transparent",
                          fontWeight: active ? 500 : 400,
                        }}
                        onMouseEnter={(e) => {
                          if (!active)
                            (e.currentTarget as HTMLElement).style.background = "var(--c-sidebar-hover)";
                        }}
                        onMouseLeave={(e) => {
                          if (!active)
                            (e.currentTarget as HTMLElement).style.background = "transparent";
                        }}
                      >
                        {lbl(item.label, item.labelEn)}
                      </Link>
                    );
                  })}
                </div>
              )}
            </div>
          );
        })}
      </nav>

      {/* ── Footer ───────────────────────────────────────────────── */}
      <div
        className="px-2.5 py-2.5 border-t space-y-0.5"
        style={{ borderColor: "var(--c-sidebar-hover)" }}
      >
        {/* Language toggle */}
        <div
          className="flex items-center gap-1 px-3 py-1.5 text-[11px]"
          style={{ color: "var(--c-sidebar-text-dim)" }}
        >
          <span className="flex-1">{lbl("اللغة", "Lang")}</span>
          <button
            onClick={() => setLang("ar")}
            className="px-2 py-0.5 rounded transition-colors"
            style={{
              background: lang === "ar" ? "var(--c-sidebar-active)" : "transparent",
              color: lang === "ar" ? "white" : "var(--c-sidebar-text-dim)",
            }}
          >
            عربي
          </button>
          <button
            onClick={() => setLang("en")}
            className="px-2 py-0.5 rounded transition-colors"
            style={{
              background: lang === "en" ? "var(--c-sidebar-active)" : "transparent",
              color: lang === "en" ? "white" : "var(--c-sidebar-text-dim)",
            }}
          >
            EN
          </button>
        </div>

        {/* Sign out */}
        <button
          onClick={() => { clearAuth(); window.location.href = "/login"; }}
          className="w-full flex items-center gap-2.5 px-3 py-2 rounded-lg text-[12px] transition-colors text-start"
          style={{ color: "var(--c-sidebar-text-dim)" }}
          onMouseEnter={(e) => {
            (e.currentTarget as HTMLElement).style.background = "var(--c-sidebar-hover)";
            (e.currentTarget as HTMLElement).style.color = "white";
          }}
          onMouseLeave={(e) => {
            (e.currentTarget as HTMLElement).style.background = "transparent";
            (e.currentTarget as HTMLElement).style.color = "var(--c-sidebar-text-dim)";
          }}
        >
          <LogOut size={14} className="shrink-0" />
          <span>{lbl("تسجيل الخروج", "Sign Out")}</span>
        </button>
      </div>
    </aside>
  );
}
