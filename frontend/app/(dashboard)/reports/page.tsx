"use client";

import Link from "next/link";

interface ReportGroup {
  label: string;
  icon: string;
  color: string;
  reports: { label: string; href: string; desc: string }[];
}

const REPORT_GROUPS: ReportGroup[] = [
  {
    label: "التقارير المالية",
    icon: "💰",
    color: "border-blue-200 bg-blue-50",
    reports: [
      { label: "تقرير التحصيلات", href: "/reports/collections", desc: "إجمالي التحصيلات النقدية حسب الخزينة وطريقة الدفع" },
      { label: "تقادم الذمم المدينة", href: "/reports/ar-aging", desc: "المبالغ المستحقة على المرضى مقسّمة حسب العمر" },
      { label: "تقرير الفواتير", href: "/finance/invoices", desc: "ملخص الفواتير والمدفوعات" },
      { label: "تقرير الأقساط", href: "/finance/installments", desc: "خطط التقسيط والمتأخرات" },
      { label: "حسابات الأطباء", href: "/finance/doctors", desc: "العمولات والمستحقات" },
    ],
  },
  {
    label: "تقارير الخزينة",
    icon: "🏦",
    color: "border-emerald-200 bg-emerald-50",
    reports: [
      { label: "أرصدة الخزائن", href: "/finance/treasury", desc: "رصيد كل خزينة نقدية" },
      { label: "حركة النقدية", href: "/treasury/movements", desc: "الوارد والصادر من الخزائن" },
      { label: "التحويلات", href: "/treasury/transfers", desc: "التحويلات بين الخزائن" },
    ],
  },
  {
    label: "تقارير المرضى",
    icon: "👥",
    color: "border-purple-200 bg-purple-50",
    reports: [
      { label: "قائمة المرضى", href: "/patients", desc: "إجمالي قاعدة المرضى" },
      { label: "المواعيد", href: "/appointments", desc: "المواعيد حسب الحالة والطبيب" },
    ],
  },
  {
    label: "تقارير المخزون",
    icon: "📦",
    color: "border-amber-200 bg-amber-50",
    reports: [
      { label: "تقرير المخزون", href: "/inventory/items", desc: "الأصناف والكميات" },
      { label: "تنبيهات المخزون", href: "/inventory/alerts", desc: "الأصناف المنخفضة ومنتهية الصلاحية" },
      { label: "حركة المخزون", href: "/inventory/movements", desc: "الوارد والصادر" },
    ],
  },
  {
    label: "تقارير المشتريات",
    icon: "🛒",
    color: "border-indigo-200 bg-indigo-50",
    reports: [
      { label: "تقرير فواتير المشتريات", href: "/reports/purchasing", desc: "عرض وطباعة فواتير المشتريات (للاطلاع فقط)" },
      { label: "الموردون", href: "/purchasing/suppliers", desc: "أرصدة الموردين والمعاملات" },
      { label: "مرتجعات الشراء", href: "/purchasing/returns", desc: "المرتجعات على الموردين" },
    ],
  },
  {
    label: "تقارير المصروفات",
    icon: "📝",
    color: "border-rose-200 bg-rose-50",
    reports: [
      { label: "تقرير المصروفات", href: "/reports/expenses", desc: "عرض وطباعة المصروفات (للاطلاع فقط)" },
    ],
  },
  {
    label: "تقارير الأصول",
    icon: "🏢",
    color: "border-orange-200 bg-orange-50",
    reports: [
      { label: "سجل الأصول", href: "/assets", desc: "الأصول الثابتة وحالتها" },
    ],
  },
  {
    label: "التأمين والمطالبات",
    icon: "🛡️",
    color: "border-teal-200 bg-teal-50",
    reports: [
      { label: "مطالبات التأمين", href: "/finance/insurance/claims", desc: "المطالبات والمدفوعات" },
      { label: "مستحقات التأمين", href: "/finance/insurance/receivables", desc: "المستحقات المعلقة" },
    ],
  },
  {
    label: "المختبر والأشعة",
    icon: "🔬",
    color: "border-cyan-200 bg-cyan-50",
    reports: [
      { label: "طلبات المختبر", href: "/lab/orders", desc: "الطلبات حسب الحالة" },
      { label: "طلبات الأشعة", href: "/radiology/orders", desc: "الطلبات حسب الحالة" },
    ],
  },
  {
    label: "التقارير التشغيلية الذكية",
    icon: "📊",
    color: "border-violet-200 bg-violet-50",
    reports: [
      { label: "التقارير الذكية", href: "/reports/operational", desc: "المرضى غير النشطين، الأقساط المتأخرة، إيرادات شهرية، أداء الأطباء" },
    ],
  },
];

export default function ReportsPage() {
  return (
    <div className="p-6" dir="rtl">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">مركز التقارير</h1>
        <p className="text-gray-500 text-sm mt-1">الوصول السريع لجميع تقارير وإحصاءات النظام</p>
      </div>

      <div className="space-y-6">
        {REPORT_GROUPS.map((group) => (
          <div key={group.label} className={`rounded-xl border p-5 ${group.color}`}>
            <div className="flex items-center gap-2 mb-4">
              <span className="text-xl">{group.icon}</span>
              <h2 className="font-bold text-gray-800">{group.label}</h2>
            </div>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
              {group.reports.map((r) => (
                <Link key={r.href} href={r.href}>
                  <div className="bg-white rounded-lg border border-white/80 p-4 hover:shadow-md transition-shadow">
                    <div className="text-sm font-semibold text-gray-800 mb-1">{r.label}</div>
                    <div className="text-xs text-gray-500">{r.desc}</div>
                  </div>
                </Link>
              ))}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
