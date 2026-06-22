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
} from "lucide-react";
import { api } from "@/lib/api";
import { useAuthStore } from "@/stores/authStore";
import { useLangStore } from "@/stores/langStore";
import { StatCard } from "@/components/ui/StatCard";
import { Skeleton } from "@/components/ui/Skeleton";
import { AppointmentStatusBadge } from "@/components/shared/StatusBadge";

/* ── Types ──────────────────────────────────────────────────────── */

interface VaultBalance {
  id: string;
  name: string;
  currentBalance: number;
}

interface AppointmentRow {
  id: string;
  patientName: string;
  scheduledAt: string;
  status: string;
  doctorName?: string;
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

const EMPTY: DashboardData = {
  waitingCount: null,
  appointmentsToday: null,
  unpaidInvoices: null,
  totalVaultBalance: null,
  overdueInstallments: null,
  pendingLab: null,
  pendingRadiology: null,
  lowStockAlerts: null,
  vaults: [],
  todayAppointments: [],
};

/* ── Helpers ─────────────────────────────────────────────────────── */

function fmtLYD(v: number | null): string {
  if (v === null) return "—";
  return (
    v.toLocaleString("ar-LY", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) +
    " د.ل"
  );
}

function fmtTime(iso: string): string {
  try {
    return new Date(iso).toLocaleTimeString("ar-LY", { hour: "2-digit", minute: "2-digit" });
  } catch {
    return "—";
  }
}

/* ── Quick action list ────────────────────────────────────────────── */

const QUICK_ACTIONS = [
  { icon: UserPlus,    label: "مريض جديد",    href: "/patients/new" },
  { icon: CalendarPlus,label: "موعد جديد",    href: "/appointments" },
  { icon: FilePlus,    label: "فاتورة جديدة", href: "/finance/invoices/new" },
  { icon: Banknote,    label: "مصروف جديد",   href: "/expenses" },
  { icon: Monitor,     label: "مساحة الصراف", href: "/finance/cashier" },
  { icon: ListOrdered, label: "طابور اليوم",  href: "/queue" },
];

/* ── Component ────────────────────────────────────────────────────── */

export default function OperationalDashboard() {
  const { user } = useAuthStore();
  const { lang } = useLangStore();
  const isAr = lang === "ar";

  const [loading, setLoading] = useState(true);
  const [data, setData] = useState<DashboardData>(EMPTY);

  const load = useCallback(async () => {
    setLoading(true);
    const today = new Date().toISOString().split("T")[0];

    const next = { ...EMPTY };

    await Promise.allSettled([
      /* Waiting patients (queue) */
      api
        .get<{ items?: unknown[]; totalCount?: number }>(`/appointments/queue?date=${today}`)
        .then((r) => {
          const waiting = Array.isArray(r.data?.items)
            ? (r.data.items as { status?: string }[]).filter((x) => x.status === "Waiting").length
            : (r.data?.totalCount ?? 0);
          next.waitingCount = waiting;
        })
        .catch(() => { next.waitingCount = 0; }),

      /* Today's appointments — count + list */
      api
        .get<{ items?: AppointmentRow[]; total?: number; totalCount?: number }>(
          `/appointments?page=1&pageSize=8&fromDate=${today}&toDate=${today}`
        )
        .then((r) => {
          next.appointmentsToday = r.data?.total ?? r.data?.totalCount ?? 0;
          next.todayAppointments = (r.data?.items ?? []).map((a) => ({
            id: a.id,
            patientName: (a as { patientName?: string; patient?: { fullName?: string } }).patientName
              ?? (a as { patient?: { fullName?: string } }).patient?.fullName
              ?? "—",
            scheduledAt: (a as { scheduledAt?: string; startTime?: string }).scheduledAt
              ?? (a as { startTime?: string }).startTime
              ?? "",
            status: a.status,
            doctorName: (a as { doctorName?: string; doctor?: { fullName?: string } }).doctorName
              ?? (a as { doctor?: { fullName?: string } }).doctor?.fullName,
          }));
        })
        .catch(() => { next.appointmentsToday = 0; }),

      /* Unpaid invoices (Posted = confirmed, not yet paid) */
      api
        .get<{ total?: number; totalCount?: number }>(
          "/invoices?page=1&pageSize=1&status=Confirmed"
        )
        .then((r) => { next.unpaidInvoices = r.data?.total ?? r.data?.totalCount ?? 0; })
        .catch(() => { next.unpaidInvoices = 0; }),

      /* Vault balances */
      api
        .get<VaultBalance[]>("/treasury/vaults/balances")
        .then((r) => {
          const list = Array.isArray(r.data) ? r.data : [];
          next.vaults = list;
          next.totalVaultBalance = list.reduce((s, v) => s + (v.currentBalance ?? 0), 0);
        })
        .catch(() => { next.totalVaultBalance = 0; }),

      /* Overdue installments */
      api
        .get<{ total?: number; totalCount?: number }>(
          "/installments/plans?status=Overdue&page=1&pageSize=1"
        )
        .then((r) => { next.overdueInstallments = r.data?.total ?? r.data?.totalCount ?? 0; })
        .catch(() => { next.overdueInstallments = 0; }),

      /* Pending lab orders */
      api
        .get<{ total?: number; totalCount?: number }>(
          "/lab/orders?page=1&pageSize=1&status=Sent"
        )
        .then((r) => { next.pendingLab = r.data?.total ?? r.data?.totalCount ?? 0; })
        .catch(() => { next.pendingLab = 0; }),

      /* Pending radiology */
      api
        .get<{ total?: number; totalCount?: number }>(
          "/radiology/orders?page=1&pageSize=1&status=Ordered"
        )
        .then((r) => { next.pendingRadiology = r.data?.total ?? r.data?.totalCount ?? 0; })
        .catch(() => { next.pendingRadiology = 0; }),

      /* Low stock alerts */
      api
        .get<unknown>("/inventory/stock/alerts")
        .then((r) => {
          const count = Array.isArray(r.data)
            ? (r.data as unknown[]).length
            : ((r.data as { items?: unknown[] })?.items?.length ?? 0);
          next.lowStockAlerts = count;
        })
        .catch(() => { next.lowStockAlerts = 0; }),
    ]);

    setData(next);
    setLoading(false);
  }, []);

  useEffect(() => { load(); }, [load]);

  const todayStr = new Date().toLocaleDateString(isAr ? "ar-LY" : "en-GB", {
    weekday: "long",
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  return (
    <div className="p-5 space-y-5" dir={isAr ? "rtl" : "ltr"}>

      {/* ── Header ─────────────────────────────────────────────────── */}
      <div className="flex items-center justify-between gap-4">
        <div>
          <h1 className="text-lg font-bold text-[var(--c-text-primary)]">لوحة العمليات</h1>
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
        <StatCard
          label="في الانتظار الآن"
          value={data.waitingCount}
          icon={Users}
          variant={!loading && (data.waitingCount ?? 0) > 0 ? "warning" : "neutral"}
          href="/queue"
          subLabel="مريض ينتظر"
          loading={loading}
        />
        <StatCard
          label="مواعيد اليوم"
          value={data.appointmentsToday}
          icon={CalendarDays}
          variant="brand"
          href={`/appointments?date=${new Date().toISOString().split("T")[0]}`}
          subLabel="موعد مجدول"
          loading={loading}
        />
        <StatCard
          label="فواتير غير مدفوعة"
          value={data.unpaidInvoices}
          icon={ReceiptText}
          variant={!loading && (data.unpaidInvoices ?? 0) > 0 ? "danger" : "neutral"}
          href="/finance/invoices?status=Confirmed"
          subLabel="تنتظر التحصيل"
          loading={loading}
        />
        <StatCard
          label="رصيد الخزائن"
          value={loading ? null : fmtLYD(data.totalVaultBalance)}
          icon={Landmark}
          variant="neutral"
          href="/finance/treasury"
          loading={loading}
        />
      </div>

      {/* ── Row 2: Action required ─────────────────────────────────── */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-3">
        <StatCard
          label="أقساط متأخرة"
          value={data.overdueInstallments}
          icon={Clock}
          variant={!loading && (data.overdueInstallments ?? 0) > 0 ? "danger" : "neutral"}
          href="/finance/installments"
          subLabel="تجاوزت تاريخ الاستحقاق"
          loading={loading}
        />
        <StatCard
          label="طلبات مختبر معلقة"
          value={data.pendingLab}
          icon={FlaskConical}
          variant={!loading && (data.pendingLab ?? 0) > 0 ? "warning" : "neutral"}
          href="/lab/orders"
          subLabel="في انتظار النتائج"
          loading={loading}
        />
        <StatCard
          label="طلبات أشعة معلقة"
          value={data.pendingRadiology}
          icon={ScanLine}
          variant={!loading && (data.pendingRadiology ?? 0) > 0 ? "warning" : "neutral"}
          href="/radiology/orders"
          subLabel="لم تكتمل بعد"
          loading={loading}
        />
        <StatCard
          label="تنبيهات المخزون"
          value={data.lowStockAlerts}
          icon={PackageX}
          variant={!loading && (data.lowStockAlerts ?? 0) > 0 ? "warning" : "neutral"}
          href="/inventory/alerts"
          subLabel="أصناف منخفضة أو منتهية"
          loading={loading}
        />
      </div>

      {/* ── Row 3: Panels ──────────────────────────────────────────── */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">

        {/* Quick Actions */}
        <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)]">
          <div className="px-4 py-3 border-b border-[var(--c-border)]">
            <h2 className="text-[12px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-wide">
              إجراءات سريعة
            </h2>
          </div>
          <div className="divide-y divide-[var(--c-border)]">
            {QUICK_ACTIONS.map(({ icon: Icon, label, href }) => (
              <Link
                key={href}
                href={href}
                className="flex items-center gap-3 px-4 h-10 text-[13px] text-[var(--c-text-body)] hover:bg-[var(--c-canvas)] transition-colors group"
              >
                <Icon size={14} className="text-[var(--c-brand)] shrink-0" />
                <span>{label}</span>
              </Link>
            ))}
          </div>
        </div>

        {/* Today's Appointments */}
        <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)]">
          <div className="px-4 py-3 border-b border-[var(--c-border)] flex items-center justify-between">
            <h2 className="text-[12px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-wide">
              مواعيد اليوم
            </h2>
            <Link
              href={`/appointments?date=${new Date().toISOString().split("T")[0]}`}
              className="text-[11px] text-[var(--c-brand)] hover:underline"
            >
              عرض الكل
            </Link>
          </div>
          <div className="divide-y divide-[var(--c-border)]">
            {loading ? (
              Array.from({ length: 5 }).map((_, i) => (
                <div key={i} className="px-4 h-10 flex items-center gap-3">
                  <Skeleton className="h-3 w-10" />
                  <Skeleton className="h-3 flex-1" />
                  <Skeleton className="h-5 w-14 rounded-full" />
                </div>
              ))
            ) : data.todayAppointments.length === 0 ? (
              <div className="px-4 py-8 text-center text-[12px] text-[var(--c-text-disabled)]">
                لا توجد مواعيد اليوم
              </div>
            ) : (
              data.todayAppointments.map((appt) => (
                <Link
                  key={appt.id}
                  href={`/appointments`}
                  className="flex items-center gap-3 px-4 h-10 hover:bg-[var(--c-canvas)] transition-colors"
                >
                  <span className="text-[11px] font-mono text-[var(--c-text-secondary)] shrink-0 w-11">
                    {appt.scheduledAt ? fmtTime(appt.scheduledAt) : "—"}
                  </span>
                  <span className="text-[12px] text-[var(--c-text-body)] flex-1 truncate">
                    {appt.patientName}
                  </span>
                  <AppointmentStatusBadge value={appt.status} />
                </Link>
              ))
            )}
          </div>
        </div>

        {/* Treasury Summary */}
        <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)]">
          <div className="px-4 py-3 border-b border-[var(--c-border)] flex items-center justify-between">
            <h2 className="text-[12px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-wide">
              ملخص الخزائن
            </h2>
            <Link
              href="/finance/treasury"
              className="text-[11px] text-[var(--c-brand)] hover:underline"
            >
              تفاصيل
            </Link>
          </div>
          <div className="divide-y divide-[var(--c-border)]">
            {loading ? (
              Array.from({ length: 3 }).map((_, i) => (
                <div key={i} className="px-4 h-10 flex items-center justify-between gap-3">
                  <Skeleton className="h-3 w-24" />
                  <Skeleton className="h-3 w-20" />
                </div>
              ))
            ) : data.vaults.length === 0 ? (
              <div className="px-4 py-8 text-center text-[12px] text-[var(--c-text-disabled)]">
                لا توجد خزائن
              </div>
            ) : (
              <>
                {data.vaults.map((v) => (
                  <div key={v.id} className="flex items-center justify-between px-4 h-10">
                    <span className="text-[12px] text-[var(--c-text-body)] truncate flex-1">
                      {v.name}
                    </span>
                    <span
                      className={`text-[12px] font-semibold tabular-nums shrink-0 ms-3 ${
                        v.currentBalance < 0
                          ? "text-[var(--c-danger)]"
                          : "text-[var(--c-text-body)]"
                      }`}
                    >
                      {fmtLYD(v.currentBalance)}
                    </span>
                  </div>
                ))}
                <div className="flex items-center justify-between px-4 h-10 bg-[var(--c-canvas)]">
                  <span className="text-[12px] font-semibold text-[var(--c-text-primary)]">
                    الإجمالي
                  </span>
                  <span className="text-[13px] font-bold tabular-nums text-[var(--c-brand)]">
                    {fmtLYD(data.totalVaultBalance)}
                  </span>
                </div>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
