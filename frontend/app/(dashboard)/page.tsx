"use client";

import { useEffect, useState, useCallback } from "react";
import Link from "next/link";
import {
  Users,
  CalendarDays,
  ReceiptText,
  Landmark,
  Clock,
  FlaskConical,
  ScanLine,
  PackageX,
  CalendarPlus,
  UserPlus,
  FilePlus,
  Banknote,
  Monitor,
  ListOrdered,
  RefreshCw,
  TrendingUp,
  TrendingDown,
  ShieldCheck,
  Stethoscope,
  Wrench,
} from "lucide-react";
import { api } from "@/lib/api";
import { useAuthStore } from "@/stores/authStore";
import { useLangStore } from "@/stores/langStore";
import { StatCard } from "@/components/ui/StatCard";
import { Skeleton } from "@/components/ui/Skeleton";
import { AppointmentStatusBadge } from "@/components/shared/StatusBadge";

/* ── Types ──────────────────────────────────────────────────────── */

interface VaultBalance { id: string; name: string; currentBalance: number; }
interface AppointmentRow {
  id: string; patientName: string; scheduledAt: string; status: string; doctorName?: string;
}
interface DashboardData {
  waitingCount: number | null;
  appointmentsToday: number | null;
  unpaidInvoices: number | null;
  totalVaultBalance: number | null;
  overdueInstallments: number | null;
  pendingLab: number | null;
  pendingRadiology: number | null;
  lowStockAlerts: number | null;
  vaults: VaultBalance[];
  todayAppointments: AppointmentRow[];
}

interface RevenueSummary {
  invoicedThisMonth: number; collectedThisMonth: number;
  outstanding: number; collectionRate: number;
}
interface InsuranceSummary {
  submittedTotal: number; partiallyPaidBalance: number;
  totalOutstanding: number; claimsThisMonth: number;
}
interface OperationsSummary {
  appointmentsThisMonth: number; attended: number; noShow: number;
  utilizationRate: number; newPatientsThisMonth: number;
}
interface ExpenseTopCategory { category: string; amount: number; }
interface ExpenseSummary {
  thisMonth: number; lastMonth: number; deltaPct: number;
  topCategories: ExpenseTopCategory[];
}
interface AssetAlertSummary { underMaintenance: number; totalActive: number; }
interface ExecutiveDashboard {
  revenue: RevenueSummary; insurance: InsuranceSummary;
  operations: OperationsSummary; expenses: ExpenseSummary;
  assetAlerts: AssetAlertSummary;
}

interface MonthlyRevenue {
  year: number; month: number; label: string;
  invoiced: number; collected: number; outstanding: number; invoiceCount: number;
}
interface DoctorPerf {
  doctorId: string; doctorName: string;
  totalRevenue: number; commissions: number; netRevenue: number; procedureCount: number;
}

const EMPTY: DashboardData = {
  waitingCount: null, appointmentsToday: null, unpaidInvoices: null,
  totalVaultBalance: null, overdueInstallments: null, pendingLab: null,
  pendingRadiology: null, lowStockAlerts: null, vaults: [], todayAppointments: [],
};

/* ── Helpers ─────────────────────────────────────────────────────── */

function fmtLYD(v: number | null | undefined): string {
  if (v == null) return "—";
  return v.toLocaleString("ar-LY", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " د.ل";
}
function fmtTime(iso: string): string {
  try { return new Date(iso).toLocaleTimeString("ar-LY", { hour: "2-digit", minute: "2-digit" }); }
  catch { return "—"; }
}
function fmtPct(v: number): string { return v.toFixed(1) + "%"; }

/* ── Quick action list ────────────────────────────────────────────── */

const QUICK_ACTIONS = [
  { icon: UserPlus,    label: "مريض جديد",    href: "/patients/new" },
  { icon: CalendarPlus,label: "موعد جديد",    href: "/appointments" },
  { icon: FilePlus,    label: "فاتورة جديدة", href: "/finance/invoices/new" },
  { icon: Banknote,    label: "مصروف جديد",   href: "/expenses" },
  { icon: Monitor,     label: "مساحة الصراف", href: "/finance/cashier" },
  { icon: ListOrdered, label: "طابور اليوم",  href: "/queue" },
];

/* ── Panel Shell ──────────────────────────────────────────────────── */
function Panel({ title, href, linkLabel, children }: {
  title: string; href?: string; linkLabel?: string; children: React.ReactNode;
}) {
  return (
    <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)] flex flex-col">
      <div className="px-4 py-3 border-b border-[var(--c-border)] flex items-center justify-between shrink-0">
        <h2 className="text-[12px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-wide">
          {title}
        </h2>
        {href && linkLabel && (
          <Link href={href} className="text-[11px] text-[var(--c-brand)] hover:underline">{linkLabel}</Link>
        )}
      </div>
      <div className="flex-1 overflow-hidden">{children}</div>
    </div>
  );
}

/* ── Component ────────────────────────────────────────────────────── */

export default function OperationalDashboard() {
  const { user, hasPermission } = useAuthStore();
  const { lang } = useLangStore();
  const isAr = lang === "ar";

  const [loading, setLoading] = useState(true);
  const [data, setData] = useState<DashboardData>(EMPTY);
  const [exec, setExec] = useState<ExecutiveDashboard | null>(null);
  const [revenue6, setRevenue6] = useState<MonthlyRevenue[]>([]);
  const [doctors, setDoctors] = useState<DoctorPerf[]>([]);

  const load = useCallback(async () => {
    setLoading(true);
    const today = new Date().toISOString().split("T")[0];
    const next = { ...EMPTY };

    await Promise.allSettled([
      /* Waiting patients */
      api.get<{ items?: unknown[]; totalCount?: number }>(`/queue?date=${today}`)
        .then((r) => {
          const waiting = Array.isArray(r.data?.items)
            ? (r.data.items as { status?: string }[]).filter((x) => x.status === "Waiting").length
            : (r.data?.totalCount ?? 0);
          next.waitingCount = waiting;
        }).catch(() => { next.waitingCount = 0; }),

      /* Today appointments */
      api.get<{ items?: AppointmentRow[]; total?: number; totalCount?: number }>(
        `/appointments?page=1&pageSize=8&fromDate=${today}&toDate=${today}`)
        .then((r) => {
          next.appointmentsToday = r.data?.total ?? r.data?.totalCount ?? 0;
          next.todayAppointments = (r.data?.items ?? []).map((a) => ({
            id: a.id,
            patientName: (a as { patientName?: string; patient?: { fullName?: string } }).patientName
              ?? (a as { patient?: { fullName?: string } }).patient?.fullName ?? "—",
            scheduledAt: (a as { scheduledAt?: string; startTime?: string }).scheduledAt
              ?? (a as { startTime?: string }).startTime ?? "",
            status: a.status,
            doctorName: (a as { doctorName?: string; doctor?: { fullName?: string } }).doctorName
              ?? (a as { doctor?: { fullName?: string } }).doctor?.fullName,
          }));
        }).catch(() => { next.appointmentsToday = 0; }),

      /* Unpaid invoices */
      api.get<{ total?: number; totalCount?: number }>("/invoices?page=1&pageSize=1&status=Confirmed")
        .then((r) => { next.unpaidInvoices = r.data?.total ?? r.data?.totalCount ?? 0; })
        .catch(() => { next.unpaidInvoices = 0; }),

      /* Vault balances */
      api.get<VaultBalance[]>("/treasury/vaults/balances")
        .then((r) => {
          const list = Array.isArray(r.data) ? r.data : [];
          next.vaults = list;
          next.totalVaultBalance = list.reduce((s, v) => s + (v.currentBalance ?? 0), 0);
        }).catch(() => { next.totalVaultBalance = 0; }),

      /* Overdue installments */
      api.get<{ total?: number; totalCount?: number }>("/installments/plans?status=Overdue&page=1&pageSize=1")
        .then((r) => { next.overdueInstallments = r.data?.total ?? r.data?.totalCount ?? 0; })
        .catch(() => { next.overdueInstallments = 0; }),

      /* Pending lab */
      api.get<{ total?: number; totalCount?: number }>("/lab/orders?page=1&pageSize=1&status=Sent")
        .then((r) => { next.pendingLab = r.data?.total ?? r.data?.totalCount ?? 0; })
        .catch(() => { next.pendingLab = 0; }),

      /* Pending radiology */
      api.get<{ total?: number; totalCount?: number }>("/radiology/orders?page=1&pageSize=1&status=Ordered")
        .then((r) => { next.pendingRadiology = r.data?.total ?? r.data?.totalCount ?? 0; })
        .catch(() => { next.pendingRadiology = 0; }),

      /* Low stock */
      api.get<unknown>("/inventory/stock/alerts")
        .then((r) => {
          const count = Array.isArray(r.data)
            ? (r.data as unknown[]).length
            : ((r.data as { items?: unknown[] })?.items?.length ?? 0);
          next.lowStockAlerts = count;
        }).catch(() => { next.lowStockAlerts = 0; }),

      /* Executive KPIs */
      api.get<ExecutiveDashboard>("/analytics/executive-dashboard")
        .then((r) => setExec(r.data))
        .catch(() => {}),

      /* 6-month revenue trend */
      api.get<MonthlyRevenue[]>("/analytics/monthly-revenue?months=6")
        .then((r) => setRevenue6(Array.isArray(r.data) ? r.data : []))
        .catch(() => {}),

      /* Top doctors this month */
      api.get<DoctorPerf[]>("/analytics/doctor-performance?months=1")
        .then((r) => setDoctors(Array.isArray(r.data) ? r.data.slice(0, 5) : []))
        .catch(() => {}),
    ]);

    setData(next);
    setLoading(false);
  }, []);

  useEffect(() => { load(); }, [load]);

  const todayStr = new Date().toLocaleDateString(isAr ? "ar-LY" : "en-GB", {
    weekday: "long", year: "numeric", month: "long", day: "numeric",
  });

  const maxRevenue = Math.max(...revenue6.map((m) => Math.max(m.invoiced, m.collected)), 1);

  if (!hasPermission("Dashboard.Overview.View")) {
    return (
      <div className="p-12 text-center text-gray-400" dir="rtl">
        <p className="text-lg font-semibold">403 — غير مصرح</p>
        <p className="text-sm mt-1">ليس لديك صلاحية عرض لوحة القيادة</p>
      </div>
    );
  }

  return (
    <div className="p-5 space-y-5" dir={isAr ? "rtl" : "ltr"}>

      {/* ── Header ─────────────────────────────────────────────────── */}
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-lg font-bold text-[var(--c-text-primary)]">لوحة التحكم</h1>
          <p className="text-[12px] text-[var(--c-text-secondary)] mt-0.5">
            {user?.fullName && <span className="font-medium">{user.fullName}</span>}
            {user?.fullName && " · "}
            {todayStr}
          </p>
        </div>
        <button
          onClick={load}
          disabled={loading}
          className="flex items-center gap-1.5 text-[12px] text-[var(--c-brand)] border border-[var(--c-brand-border)] px-3 py-1.5 rounded-md hover:bg-[var(--c-brand-subtle)] transition-colors disabled:opacity-50"
        >
          <RefreshCw size={12} className={loading ? "animate-spin" : ""} />
          تحديث
        </button>
      </div>

      {/* ── Row 1: Operational pulse ───────────────────────────────── */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        <StatCard label="في الانتظار الآن" value={data.waitingCount} icon={Users}
          variant={!loading && (data.waitingCount ?? 0) > 0 ? "warning" : "neutral"}
          href="/queue" subLabel="مريض ينتظر" loading={loading} />
        <StatCard label="مواعيد اليوم" value={data.appointmentsToday} icon={CalendarDays}
          variant="brand" href={`/appointments?date=${new Date().toISOString().split("T")[0]}`}
          subLabel="موعد مجدول" loading={loading} />
        <StatCard label="فواتير غير مدفوعة" value={data.unpaidInvoices} icon={ReceiptText}
          variant={!loading && (data.unpaidInvoices ?? 0) > 0 ? "danger" : "neutral"}
          href="/finance/invoices?status=Confirmed" subLabel="تنتظر التحصيل" loading={loading} />
        <StatCard label="رصيد الخزائن" value={loading ? null : fmtLYD(data.totalVaultBalance)}
          icon={Landmark} variant="neutral" href="/finance/treasury" loading={loading} />
      </div>

      {/* ── Row 2: Action required ─────────────────────────────────── */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        <StatCard label="أقساط متأخرة" value={data.overdueInstallments} icon={Clock}
          variant={!loading && (data.overdueInstallments ?? 0) > 0 ? "danger" : "neutral"}
          href="/finance/installments" subLabel="تجاوزت تاريخ الاستحقاق" loading={loading} />
        <StatCard label="طلبات مختبر معلقة" value={data.pendingLab} icon={FlaskConical}
          variant={!loading && (data.pendingLab ?? 0) > 0 ? "warning" : "neutral"}
          href="/lab/orders" subLabel="في انتظار النتائج" loading={loading} />
        <StatCard label="طلبات أشعة معلقة" value={data.pendingRadiology} icon={ScanLine}
          variant={!loading && (data.pendingRadiology ?? 0) > 0 ? "warning" : "neutral"}
          href="/radiology/orders" subLabel="لم تكتمل بعد" loading={loading} />
        <StatCard label="تنبيهات المخزون" value={data.lowStockAlerts} icon={PackageX}
          variant={!loading && (data.lowStockAlerts ?? 0) > 0 ? "warning" : "neutral"}
          href="/inventory/alerts" subLabel="أصناف منخفضة أو منتهية" loading={loading} />
      </div>

      {/* ── Row 3: Classic panels ──────────────────────────────────── */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">

        {/* Quick Actions */}
        <Panel title="إجراءات سريعة">
          <div className="divide-y divide-[var(--c-border)]">
            {QUICK_ACTIONS.map(({ icon: Icon, label, href }) => (
              <Link key={href} href={href}
                className="flex items-center gap-3 px-4 h-10 text-[13px] text-[var(--c-text-body)] hover:bg-[var(--c-canvas)] transition-colors">
                <Icon size={14} className="text-[var(--c-brand)] shrink-0" />
                <span>{label}</span>
              </Link>
            ))}
          </div>
        </Panel>

        {/* Today's Appointments */}
        <Panel title="مواعيد اليوم" href={`/appointments?date=${new Date().toISOString().split("T")[0]}`} linkLabel="عرض الكل">
          <div className="divide-y divide-[var(--c-border)]">
            {loading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <div key={i} className="px-4 h-10 flex items-center gap-3">
                  <Skeleton className="h-3 w-10" /><Skeleton className="h-3 flex-1" /><Skeleton className="h-5 w-14 rounded-full" />
                </div>
              ))
            ) : data.todayAppointments.length === 0 ? (
              <div className="px-4 py-8 text-center text-[12px] text-[var(--c-text-disabled)]">لا توجد مواعيد اليوم</div>
            ) : (
              data.todayAppointments.map((appt) => (
                <Link key={appt.id} href="/appointments"
                  className="flex items-center gap-3 px-4 h-10 hover:bg-[var(--c-canvas)] transition-colors">
                  <span className="text-[11px] font-mono text-[var(--c-text-secondary)] shrink-0 w-11">
                    {appt.scheduledAt ? fmtTime(appt.scheduledAt) : "—"}
                  </span>
                  <span className="text-[12px] text-[var(--c-text-body)] flex-1 truncate">{appt.patientName}</span>
                  <AppointmentStatusBadge value={appt.status} />
                </Link>
              ))
            )}
          </div>
        </Panel>

        {/* Treasury Summary */}
        <Panel title="ملخص الخزائن" href="/finance/treasury" linkLabel="تفاصيل">
          <div className="divide-y divide-[var(--c-border)]">
            {loading ? (
              Array.from({ length: 3 }).map((_, i) => (
                <div key={i} className="px-4 h-10 flex items-center justify-between gap-3">
                  <Skeleton className="h-3 w-24" /><Skeleton className="h-3 w-20" />
                </div>
              ))
            ) : data.vaults.length === 0 ? (
              <div className="px-4 py-8 text-center text-[12px] text-[var(--c-text-disabled)]">لا توجد خزائن</div>
            ) : (
              <>
                {data.vaults.map((v) => (
                  <div key={v.id} className="flex items-center justify-between px-4 h-10">
                    <span className="text-[12px] text-[var(--c-text-body)] truncate flex-1">{v.name}</span>
                    <span className={`text-[12px] font-semibold tabular-nums shrink-0 ms-3 ${v.currentBalance < 0 ? "text-[var(--c-danger)]" : "text-[var(--c-text-body)]"}`}>
                      {fmtLYD(v.currentBalance)}
                    </span>
                  </div>
                ))}
                <div className="flex items-center justify-between px-4 h-10 bg-[var(--c-canvas)]">
                  <span className="text-[12px] font-semibold text-[var(--c-text-primary)]">الإجمالي</span>
                  <span className="text-[13px] font-bold tabular-nums text-[var(--c-brand)]">{fmtLYD(data.totalVaultBalance)}</span>
                </div>
              </>
            )}
          </div>
        </Panel>
      </div>

      {/* ── Section title ──────────────────────────────────────────── */}
      <div className="flex items-center gap-3 pt-1">
        <div className="h-px flex-1 bg-[var(--c-border)]" />
        <span className="text-[11px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-widest">
          ملخص تنفيذي — هذا الشهر
        </span>
        <div className="h-px flex-1 bg-[var(--c-border)]" />
      </div>

      {/* ── Row 4: Revenue + Insurance ─────────────────────────────── */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">

        {/* Revenue Panel */}
        <Panel title="الإيرادات" href="/reports/collections" linkLabel="تقرير التحصيلات">
          {loading || !exec ? (
            <div className="p-4 space-y-2">
              {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-4 w-full" />)}
            </div>
          ) : (
            <div className="p-4 space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <div className="bg-blue-50 rounded-lg p-3">
                  <div className="text-[11px] text-blue-600 mb-0.5">تم إصداره</div>
                  <div className="text-[15px] font-bold text-blue-800 tabular-nums">{fmtLYD(exec.revenue.invoicedThisMonth)}</div>
                </div>
                <div className="bg-emerald-50 rounded-lg p-3">
                  <div className="text-[11px] text-emerald-600 mb-0.5">تم تحصيله</div>
                  <div className="text-[15px] font-bold text-emerald-800 tabular-nums">{fmtLYD(exec.revenue.collectedThisMonth)}</div>
                </div>
                <div className="bg-amber-50 rounded-lg p-3">
                  <div className="text-[11px] text-amber-600 mb-0.5">متأخر السداد</div>
                  <div className="text-[15px] font-bold text-amber-800 tabular-nums">{fmtLYD(exec.revenue.outstanding)}</div>
                </div>
                <div className="bg-purple-50 rounded-lg p-3">
                  <div className="text-[11px] text-purple-600 mb-0.5">نسبة التحصيل</div>
                  <div className="text-[15px] font-bold text-purple-800">{fmtPct(exec.revenue.collectionRate)}</div>
                </div>
              </div>

              {/* 6-month sparkline bars */}
              {revenue6.length > 0 && (
                <div>
                  <div className="text-[11px] text-[var(--c-text-secondary)] mb-2">الإيرادات — 6 أشهر</div>
                  <div className="flex items-end gap-1 h-14">
                    {revenue6.map((m) => (
                      <div key={`${m.year}-${m.month}`} className="flex-1 flex flex-col items-center gap-0.5">
                        <div className="w-full flex flex-col justify-end gap-px" style={{ height: "44px" }}>
                          <div
                            className="w-full bg-blue-200 rounded-sm"
                            style={{ height: `${Math.round((m.invoiced / maxRevenue) * 44)}px` }}
                            title={`إصدار: ${fmtLYD(m.invoiced)}`}
                          />
                        </div>
                        <div
                          className="w-full bg-emerald-400 rounded-sm"
                          style={{ height: `${Math.round((m.collected / maxRevenue) * 10)}px` }}
                          title={`تحصيل: ${fmtLYD(m.collected)}`}
                        />
                        <div className="text-[9px] text-[var(--c-text-secondary)] truncate w-full text-center leading-none">
                          {(m.label ?? "").split(" ")[0]}
                        </div>
                      </div>
                    ))}
                  </div>
                  <div className="flex gap-3 mt-1">
                    <span className="flex items-center gap-1 text-[10px] text-[var(--c-text-secondary)]">
                      <span className="inline-block w-3 h-2 bg-blue-200 rounded-sm" />إصدار
                    </span>
                    <span className="flex items-center gap-1 text-[10px] text-[var(--c-text-secondary)]">
                      <span className="inline-block w-3 h-2 bg-emerald-400 rounded-sm" />تحصيل
                    </span>
                  </div>
                </div>
              )}
            </div>
          )}
        </Panel>

        {/* Insurance + Operations stacked */}
        <div className="flex flex-col gap-4">
          {/* Insurance Panel */}
          <Panel title="مستحقات التأمين" href="/finance/insurance/claims" linkLabel="المطالبات">
            {loading || !exec ? (
              <div className="p-3 space-y-2">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-4 w-full" />)}</div>
            ) : (
              <div className="divide-y divide-[var(--c-border)]">
                <div className="flex justify-between items-center px-4 h-9">
                  <span className="text-[12px] text-[var(--c-text-secondary)]">مطالبات مقدمة</span>
                  <span className="text-[12px] font-semibold tabular-nums text-[var(--c-text-body)]">{fmtLYD(exec.insurance.submittedTotal)}</span>
                </div>
                <div className="flex justify-between items-center px-4 h-9">
                  <span className="text-[12px] text-[var(--c-text-secondary)]">مدفوعة جزئياً</span>
                  <span className="text-[12px] font-semibold tabular-nums text-amber-600">{fmtLYD(exec.insurance.partiallyPaidBalance)}</span>
                </div>
                <div className="flex justify-between items-center px-4 h-9 bg-[var(--c-canvas)]">
                  <span className="text-[12px] font-semibold text-[var(--c-text-primary)]">إجمالي المستحقات</span>
                  <span className="text-[13px] font-bold tabular-nums text-[var(--c-brand)]">{fmtLYD(exec.insurance.totalOutstanding)}</span>
                </div>
                <div className="flex justify-between items-center px-4 h-9">
                  <span className="text-[12px] text-[var(--c-text-secondary)]">مطالبات هذا الشهر</span>
                  <span className="text-[12px] font-medium text-[var(--c-text-body)]">{exec.insurance.claimsThisMonth} مطالبة</span>
                </div>
              </div>
            )}
          </Panel>

          {/* Operations Panel */}
          <Panel title="العمليات" href="/appointments" linkLabel="المواعيد">
            {loading || !exec ? (
              <div className="p-3 space-y-2">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-4 w-full" />)}</div>
            ) : (
              <div className="divide-y divide-[var(--c-border)]">
                <div className="flex justify-between items-center px-4 h-9">
                  <span className="text-[12px] text-[var(--c-text-secondary)]">مواعيد الشهر</span>
                  <span className="text-[12px] font-semibold text-[var(--c-text-body)]">{exec.operations.appointmentsThisMonth}</span>
                </div>
                <div className="flex justify-between items-center px-4 h-9">
                  <span className="text-[12px] text-[var(--c-text-secondary)]">حضر / غياب</span>
                  <span className="text-[12px] tabular-nums text-[var(--c-text-body)]">
                    <span className="text-emerald-600 font-semibold">{exec.operations.attended}</span>
                    {" / "}
                    <span className="text-red-500 font-semibold">{exec.operations.noShow}</span>
                  </span>
                </div>
                <div className="flex justify-between items-center px-4 h-9">
                  <span className="text-[12px] text-[var(--c-text-secondary)]">نسبة الاستثمار</span>
                  <span className={`text-[12px] font-semibold ${exec.operations.utilizationRate >= 70 ? "text-emerald-600" : exec.operations.utilizationRate >= 40 ? "text-amber-600" : "text-red-500"}`}>
                    {fmtPct(exec.operations.utilizationRate)}
                  </span>
                </div>
                <div className="flex justify-between items-center px-4 h-9">
                  <span className="text-[12px] text-[var(--c-text-secondary)]">مرضى جدد</span>
                  <span className="text-[12px] font-medium text-[var(--c-brand)]">{exec.operations.newPatientsThisMonth} مريض</span>
                </div>
              </div>
            )}
          </Panel>
        </div>
      </div>

      {/* ── Row 5: Doctors + Expenses + Assets ────────────────────── */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">

        {/* Doctor Performance */}
        <Panel title="أداء الأطباء" href="/reports/operational" linkLabel="التفاصيل">
          {loading ? (
            <div className="p-4 space-y-2">{Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-8 w-full" />)}</div>
          ) : doctors.length === 0 ? (
            <div className="px-4 py-8 text-center text-[12px] text-[var(--c-text-disabled)]">لا توجد بيانات</div>
          ) : (
            <div className="divide-y divide-[var(--c-border)]">
              {doctors.map((d, idx) => (
                <div key={d.doctorId} className="flex items-center gap-3 px-4 py-2.5">
                  <span className="text-[11px] font-bold text-[var(--c-text-secondary)] w-4 shrink-0">{idx + 1}</span>
                  <div className="flex-1 min-w-0">
                    <div className="text-[12px] font-medium text-[var(--c-text-body)] truncate">{d.doctorName}</div>
                    <div className="text-[10px] text-[var(--c-text-secondary)]">{d.procedureCount} إجراء</div>
                  </div>
                  <div className="text-right shrink-0">
                    <div className="text-[12px] font-semibold text-[var(--c-text-body)] tabular-nums">{fmtLYD(d.totalRevenue)}</div>
                    <div className="text-[10px] text-emerald-600 tabular-nums">صافي {fmtLYD(d.netRevenue)}</div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </Panel>

        {/* Financial Health — Expenses */}
        <Panel title="الصحة المالية" href="/reports/expenses" linkLabel="تقرير المصروفات">
          {loading || !exec ? (
            <div className="p-4 space-y-2">{Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-4 w-full" />)}</div>
          ) : (
            <div className="p-4 space-y-4">
              {/* This month vs last month */}
              <div className="flex items-center justify-between">
                <div>
                  <div className="text-[11px] text-[var(--c-text-secondary)]">مصروفات هذا الشهر</div>
                  <div className="text-[18px] font-bold text-[var(--c-text-primary)] tabular-nums mt-0.5">{fmtLYD(exec.expenses.thisMonth)}</div>
                </div>
                <div className="flex items-center gap-1">
                  {exec.expenses.deltaPct > 0 ? (
                    <TrendingUp size={16} className="text-red-500" />
                  ) : exec.expenses.deltaPct < 0 ? (
                    <TrendingDown size={16} className="text-emerald-500" />
                  ) : null}
                  <span className={`text-[13px] font-semibold ${exec.expenses.deltaPct > 0 ? "text-red-500" : exec.expenses.deltaPct < 0 ? "text-emerald-500" : "text-[var(--c-text-secondary)]"}`}>
                    {exec.expenses.deltaPct > 0 ? "+" : ""}{exec.expenses.deltaPct.toFixed(1)}%
                  </span>
                </div>
              </div>
              <div className="text-[11px] text-[var(--c-text-secondary)]">
                الشهر الماضي: <span className="font-medium text-[var(--c-text-body)] tabular-nums">{fmtLYD(exec.expenses.lastMonth)}</span>
              </div>

              {/* Top categories */}
              {exec.expenses.topCategories.length > 0 && (
                <div className="space-y-1.5">
                  <div className="text-[11px] text-[var(--c-text-secondary)]">أكثر الفئات إنفاقاً</div>
                  {exec.expenses.topCategories.map((cat, i) => {
                    const total = exec.expenses.thisMonth || 1;
                    const pct = Math.round((cat.amount / total) * 100);
                    return (
                      <div key={i} className="space-y-0.5">
                        <div className="flex justify-between text-[11px]">
                          <span className="text-[var(--c-text-body)] truncate">{cat.category}</span>
                          <span className="text-[var(--c-text-secondary)] tabular-nums shrink-0 ms-2">{fmtLYD(cat.amount)}</span>
                        </div>
                        <div className="h-1.5 bg-[var(--c-border)] rounded-full overflow-hidden">
                          <div className="h-full bg-[var(--c-brand)] rounded-full" style={{ width: `${pct}%` }} />
                        </div>
                      </div>
                    );
                  })}
                </div>
              )}
            </div>
          )}
        </Panel>

        {/* Asset Alerts */}
        <Panel title="تنبيهات الأصول" href="/assets" linkLabel="سجل الأصول">
          {loading || !exec ? (
            <div className="p-4 space-y-2">{Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-8 w-full" />)}</div>
          ) : (
            <div className="p-4 space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <div className={`rounded-lg p-3 border ${exec.assetAlerts.underMaintenance > 0 ? "bg-amber-50 border-amber-200" : "bg-gray-50 border-gray-200"}`}>
                  <Wrench size={18} className={exec.assetAlerts.underMaintenance > 0 ? "text-amber-500" : "text-gray-400"} />
                  <div className="mt-2 text-[20px] font-bold text-[var(--c-text-primary)]">{exec.assetAlerts.underMaintenance}</div>
                  <div className="text-[11px] text-[var(--c-text-secondary)]">تحت الصيانة</div>
                </div>
                <div className="rounded-lg p-3 border bg-emerald-50 border-emerald-200">
                  <Stethoscope size={18} className="text-emerald-500" />
                  <div className="mt-2 text-[20px] font-bold text-[var(--c-text-primary)]">{exec.assetAlerts.totalActive}</div>
                  <div className="text-[11px] text-[var(--c-text-secondary)]">أصول نشطة</div>
                </div>
              </div>

              {exec.assetAlerts.underMaintenance > 0 && (
                <div className="flex items-start gap-2 bg-amber-50 border border-amber-200 rounded-lg p-3">
                  <ShieldCheck size={14} className="text-amber-500 mt-0.5 shrink-0" />
                  <p className="text-[11px] text-amber-700">
                    {exec.assetAlerts.underMaintenance} أصل قيد الصيانة حالياً — يُنصح بالمتابعة للتأكد من إعادة تشغيلها.
                  </p>
                </div>
              )}

              {exec.assetAlerts.underMaintenance === 0 && (
                <div className="flex items-start gap-2 bg-emerald-50 border border-emerald-200 rounded-lg p-3">
                  <ShieldCheck size={14} className="text-emerald-500 mt-0.5 shrink-0" />
                  <p className="text-[11px] text-emerald-700">جميع الأصول تعمل بشكل طبيعي.</p>
                </div>
              )}
            </div>
          )}
        </Panel>
      </div>

    </div>
  );
}
