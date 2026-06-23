"use client";

import { useEffect, useState, useCallback, useRef } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import {
  Phone,
  CalendarDays,
  FilePlus,
  UserCog,
  AlertTriangle,
  HeartPulse,
  Droplets,
  CreditCard,
  FlaskConical,
  ScanLine,
  FolderOpen,
  Clock,
  ChevronRight,
  Stethoscope,
  ListOrdered,
} from "lucide-react";
import { api } from "@/lib/api";
import { useAuthStore } from "@/stores/authStore";
import { Skeleton } from "@/components/ui/Skeleton";
import { Badge } from "@/components/ui/Badge";
import {
  InvoiceStatusBadge,
  AppointmentStatusBadge,
} from "@/components/shared/StatusBadge";

/* ── Types ──────────────────────────────────────────────────────── */

interface Patient {
  id: string; fileNumber: string; fullName: string; phone: string;
  phone2?: string; email?: string; dateOfBirth?: string; gender?: string;
  nationalId?: string; bloodType?: string; allergies?: string;
  chronicDiseases?: string; notes?: string; isActive: boolean; createdAt: string;
}
interface Appointment {
  id: string; scheduledAt?: string; startTime?: string;
  status: string; doctorName?: string; typeName?: string; typeNameAr?: string;
}
interface Invoice {
  id: string; invoiceNumber: string; totalAmount: number;
  paidAmount: number; remaining: number; status: string; createdAt: string;
}
interface TreatmentPlanItem {
  id: string; procedureName: string; toothId?: number;
  surface?: string; quantity: number; unitPrice: number;
  discountPercent: number; totalPrice: number; status: string;
}
interface TreatmentPlan {
  id: string; title: string; priority: string; status: string;
  estimatedCost: number; totalCost: number; actualCost: number;
  paidAmount: number; createdAt: string; items?: TreatmentPlanItem[];
}
interface TimelineEvent {
  id: string; eventType: string; eventCategory: string;
  title: string; description?: string; actorName?: string; eventAt: string;
}
interface LabOrder {
  id: string; orderNumber: string; status: string;
  externalLabName: string | null; totalCost: number; isExternal: boolean; createdAt: string;
}
interface RadiologyOrder {
  id: string; orderNumber: string; status: string;
  radiologyTypeName: string; price: number; orderDate: string;
}

type Tab = "overview" | "chart" | "plans" | "invoices" | "orders" | "timeline" | "media";

const TABS: { id: Tab; label: string; icon: React.ComponentType<{ size?: number; className?: string }> }[] = [
  { id: "overview",  label: "نظرة عامة",     icon: Stethoscope  },
  { id: "chart",     label: "المخطط السني",  icon: HeartPulse   },
  { id: "plans",     label: "خطط العلاج",    icon: ListOrdered  },
  { id: "invoices",  label: "الفواتير",       icon: CreditCard   },
  { id: "orders",    label: "الأوامر",        icon: FlaskConical },
  { id: "timeline",  label: "السجل",          icon: Clock        },
  { id: "media",     label: "الملفات",        icon: FolderOpen   },
];

/* ── Helpers ─────────────────────────────────────────────────────── */

function fmtD(iso?: string | null) {
  if (!iso) return "—";
  const d = new Date(iso);
  return isNaN(d.getTime()) ? "—" : d.toLocaleDateString("ar-LY", { year: "numeric", month: "short", day: "numeric" });
}
function fmtDT(iso?: string | null) {
  if (!iso) return "—";
  const d = new Date(iso);
  return isNaN(d.getTime()) ? "—" : d.toLocaleString("ar-LY", { month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" });
}
function calcAge(dob?: string | null) {
  if (!dob) return null;
  const d = new Date(dob);
  if (isNaN(d.getTime())) return null;
  return Math.floor((Date.now() - d.getTime()) / (365.25 * 864e5)) + " سنة";
}
function fmtLYD(v: number | null | undefined): string {
  if (v == null) return "—";
  return v.toLocaleString("ar-LY", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " د.ل";
}
function apptTime(a: Appointment) {
  return fmtDT(a.scheduledAt ?? a.startTime);
}

const PLAN_STATUS_AR: Record<string, string> = {
  Draft: "مسودة", Active: "نشطة", Completed: "مكتملة", Cancelled: "ملغاة",
};
const PLAN_PRIORITY_AR: Record<string, string> = {
  Low: "منخفض", Normal: "عادي", High: "عالي", Urgent: "عاجل",
};
const TIMELINE_CAT_AR: Record<string, string> = {
  Clinical: "طبي", Financial: "مالي", Administrative: "إداري",
  Insurance: "تأمين", Radiology: "أشعة", Laboratory: "مختبر",
};
const LAB_STATUS_AR: Record<string, string> = {
  Draft: "مسودة", Sent: "مُرسل", InProgress: "جاري",
  ResultReceived: "نتيجة واردة", Completed: "مكتمل", Cancelled: "ملغى",
};
const RAD_STATUS_AR: Record<string, string> = {
  Ordered: "مطلوب", Imaged: "تصوير مكتمل", ReportSaved: "تقرير محفوظ",
  Completed: "مكتمل", Cancelled: "ملغى",
};

/* ── Section heading ─────────────────────────────────────────────── */

function SectionTitle({ children }: { children: React.ReactNode }) {
  return (
    <h3 className="text-[11px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-wide mb-2">
      {children}
    </h3>
  );
}

/* ── Empty state ─────────────────────────────────────────────────── */

function Empty({ message }: { message: string }) {
  return (
    <div className="py-10 text-center text-[12px] text-[var(--c-text-disabled)]">{message}</div>
  );
}

/* ══════════════════════════════════════════════════════════════════
   TAB: Overview
═══════════════════════════════════════════════════════════════════ */

function OverviewTab({
  patient, appointments, invoices, outstanding, events,
}: {
  patient: Patient;
  appointments: Appointment[];
  invoices: Invoice[];
  outstanding: number;
  events: TimelineEvent[];
}) {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">

      {/* Appointments */}
      <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)]">
        <div className="px-4 py-3 border-b border-[var(--c-border)] flex items-center justify-between">
          <SectionTitle>المواعيد الأخيرة</SectionTitle>
          <Link href={`/appointments?patientId=${patient.id}`} className="text-[11px] text-[var(--c-brand)] hover:underline">عرض الكل</Link>
        </div>
        <div className="divide-y divide-[var(--c-border)]">
          {appointments.length === 0 ? (
            <Empty message="لا توجد مواعيد" />
          ) : appointments.map((a) => (
            <div key={a.id} className="flex items-center justify-between gap-3 px-4 py-2.5">
              <div className="min-w-0">
                <p className="text-[12px] font-medium text-[var(--c-text-body)] truncate">
                  {a.typeNameAr ?? a.typeName ?? "موعد"}
                </p>
                <p className="text-[11px] text-[var(--c-text-secondary)]">{apptTime(a)}</p>
                {a.doctorName && <p className="text-[11px] text-[var(--c-text-disabled)]">{a.doctorName}</p>}
              </div>
              <AppointmentStatusBadge value={a.status} />
            </div>
          ))}
        </div>
      </div>

      {/* Invoices */}
      <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)]">
        <div className="px-4 py-3 border-b border-[var(--c-border)] flex items-center justify-between">
          <SectionTitle>الفواتير الأخيرة</SectionTitle>
          <Link href={`/finance/invoices?patientId=${patient.id}`} className="text-[11px] text-[var(--c-brand)] hover:underline">عرض الكل</Link>
        </div>
        <div className="divide-y divide-[var(--c-border)]">
          {invoices.length === 0 ? (
            <Empty message="لا توجد فواتير" />
          ) : invoices.map((inv) => (
            <Link key={inv.id} href={`/finance/invoices/${inv.id}`}
              className="flex items-center justify-between gap-3 px-4 py-2.5 hover:bg-[var(--c-canvas)] transition-colors">
              <div className="min-w-0">
                <p className="text-[12px] font-mono text-[var(--c-brand)]">{inv.invoiceNumber}</p>
                <p className="text-[11px] text-[var(--c-text-secondary)]">{fmtD(inv.createdAt)}</p>
                {inv.remaining > 0 && (
                  <p className="text-[11px] text-[var(--c-danger)]">متبقي: {fmtLYD(inv.remaining)}</p>
                )}
              </div>
              <div className="text-end shrink-0">
                <p className="text-[12px] font-semibold tabular-nums text-[var(--c-text-body)]">{fmtLYD(inv.totalAmount)}</p>
                <InvoiceStatusBadge value={inv.status} />
              </div>
            </Link>
          ))}
        </div>
        {outstanding > 0 && (
          <div className="flex items-center justify-between px-4 py-2.5 border-t border-[var(--c-border)] bg-[var(--c-danger-bg)]">
            <span className="text-[12px] font-medium text-[var(--c-danger)]">إجمالي المستحق</span>
            <span className="text-[13px] font-bold tabular-nums text-[var(--c-danger)]">{fmtLYD(outstanding)}</span>
          </div>
        )}
      </div>

      {/* Medical info + Timeline */}
      <div className="space-y-4">
        {/* Medical info */}
        <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)] px-4 py-3 space-y-2">
          <SectionTitle>المعلومات الطبية</SectionTitle>
          {patient.bloodType && (
            <div className="flex items-center justify-between text-[12px]">
              <span className="flex items-center gap-1.5 text-[var(--c-text-secondary)]">
                <Droplets size={12} /> فصيلة الدم
              </span>
              <span className="font-bold text-[var(--c-brand)] bg-[var(--c-brand-subtle)] px-2 py-0.5 rounded text-[11px]">
                {patient.bloodType}
              </span>
            </div>
          )}
          {patient.nationalId && (
            <div className="flex items-center justify-between text-[12px]">
              <span className="text-[var(--c-text-secondary)]">رقم الهوية</span>
              <span className="font-medium text-[var(--c-text-body)]">{patient.nationalId}</span>
            </div>
          )}
          {patient.dateOfBirth && (
            <div className="flex items-center justify-between text-[12px]">
              <span className="text-[var(--c-text-secondary)]">تاريخ الميلاد</span>
              <span className="text-[var(--c-text-body)]">{fmtD(patient.dateOfBirth)}</span>
            </div>
          )}
          {patient.notes && (
            <div>
              <p className="text-[11px] text-[var(--c-text-secondary)] mb-1">ملاحظات</p>
              <p className="text-[12px] text-[var(--c-text-body)] bg-[var(--c-canvas)] rounded p-2">{patient.notes}</p>
            </div>
          )}
          {!patient.bloodType && !patient.nationalId && !patient.notes && !patient.dateOfBirth && (
            <p className="text-[12px] text-[var(--c-text-disabled)]">لا توجد معلومات إضافية</p>
          )}
        </div>

        {/* Recent events */}
        <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)]">
          <div className="px-4 py-3 border-b border-[var(--c-border)] flex items-center justify-between">
            <SectionTitle>آخر الأحداث</SectionTitle>
          </div>
          <div className="divide-y divide-[var(--c-border)]">
            {events.length === 0 ? (
              <Empty message="لا توجد أحداث" />
            ) : events.map((e) => (
              <div key={e.id} className="flex items-start gap-3 px-4 py-2.5">
                <div className="w-1.5 h-1.5 rounded-full bg-[var(--c-brand)] mt-1.5 shrink-0" />
                <div className="min-w-0 flex-1">
                  <p className="text-[12px] text-[var(--c-text-body)] truncate">{e.title}</p>
                  <div className="flex items-center gap-2 mt-0.5">
                    <span className="text-[10px] text-[var(--c-text-disabled)]">{fmtDT(e.eventAt)}</span>
                    <Badge variant="neutral">{TIMELINE_CAT_AR[e.eventCategory] ?? e.eventCategory}</Badge>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════
   TAB: Dental Chart (placeholder — Gate 3D)
═══════════════════════════════════════════════════════════════════ */

function ChartTab({ patientId }: { patientId: string }) {
  return (
    <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)] p-8 text-center">
      <HeartPulse size={32} className="text-[var(--c-brand)] mx-auto mb-3 opacity-40" />
      <p className="text-[14px] font-medium text-[var(--c-text-primary)] mb-1">مخطط الأسنان قيد التطوير</p>
      <p className="text-[12px] text-[var(--c-text-secondary)] mb-4 max-w-sm mx-auto">
        المخطط السني التفاعلي مع تدوين FDI وتتبع الإجراءات سيكون متاحاً في Gate 3D
      </p>
      <Link
        href={`/patients/${patientId}/chart`}
        className="inline-flex items-center gap-2 text-[12px] text-[var(--c-brand)] border border-[var(--c-brand-border)] px-4 py-2 rounded-md hover:bg-[var(--c-brand-subtle)] transition-colors"
      >
        <span>فتح صفحة المخطط</span>
        <ChevronRight size={14} className="rotate-180" />
      </Link>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════
   TAB: Treatment Plans
═══════════════════════════════════════════════════════════════════ */

function PlansTab({ patientId }: { patientId: string }) {
  const [plans, setPlans] = useState<TreatmentPlan[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get<{ items: TreatmentPlan[] }>(`/patients/${patientId}/treatment-plans`)
      .then((r) => setPlans(r.data.items ?? []))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [patientId]);

  if (loading) return (
    <div className="space-y-2">
      {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-16 w-full" />)}
    </div>
  );

  return (
    <div className="space-y-3">
      {plans.length === 0 ? (
        <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)] p-8 text-center text-[12px] text-[var(--c-text-disabled)]">
          لا توجد خطط علاج
        </div>
      ) : plans.map((plan) => (
        <div key={plan.id} className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)]">
          <div className="px-4 py-3 flex items-center justify-between gap-3 border-b border-[var(--c-border)]">
            <div className="flex items-center gap-3 min-w-0">
              <p className="text-[13px] font-semibold text-[var(--c-text-primary)] truncate">{plan.title}</p>
              <Badge variant={plan.status === "Active" ? "brand" : plan.status === "Completed" ? "success" : "neutral"}>
                {PLAN_STATUS_AR[plan.status] ?? plan.status}
              </Badge>
              <Badge variant={plan.priority === "Urgent" ? "danger" : plan.priority === "High" ? "warning" : "neutral"}>
                {PLAN_PRIORITY_AR[plan.priority] ?? plan.priority}
              </Badge>
            </div>
            <div className="text-end shrink-0">
              <p className="text-[12px] font-semibold tabular-nums text-[var(--c-text-body)]">{fmtLYD(plan.totalCost)}</p>
              {plan.paidAmount > 0 && (
                <p className="text-[11px] text-[var(--c-success)]">مدفوع: {fmtLYD(plan.paidAmount)}</p>
              )}
            </div>
          </div>
          {plan.items && plan.items.length > 0 && (
            <div className="divide-y divide-[var(--c-border)]">
              {plan.items.slice(0, 5).map((item) => (
                <div key={item.id} className="flex items-center justify-between gap-3 px-4 py-2">
                  <div className="flex items-center gap-2 min-w-0">
                    {item.toothId && (
                      <span className="text-[10px] font-mono bg-[var(--c-brand-subtle)] text-[var(--c-brand)] px-1.5 py-0.5 rounded shrink-0">
                        {item.toothId}
                      </span>
                    )}
                    <p className="text-[12px] text-[var(--c-text-body)] truncate">{item.procedureName}</p>
                  </div>
                  <div className="flex items-center gap-3 shrink-0">
                    <span className="text-[12px] tabular-nums text-[var(--c-text-body)]">{fmtLYD(item.totalPrice)}</span>
                    <Badge variant={item.status === "Completed" ? "success" : item.status === "InProgress" ? "warning" : "neutral"}>
                      {item.status === "Completed" ? "مكتمل" : item.status === "InProgress" ? "جاري" : "معلق"}
                    </Badge>
                  </div>
                </div>
              ))}
            </div>
          )}
          <div className="px-4 py-2 text-[11px] text-[var(--c-text-disabled)]">{fmtD(plan.createdAt)}</div>
        </div>
      ))}
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════
   TAB: Invoices (paginated)
═══════════════════════════════════════════════════════════════════ */

function InvoicesTab({ patientId }: { patientId: string }) {
  const [items, setItems] = useState<Invoice[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const PAGE_SIZE = 15;

  const load = useCallback((p: number) => {
    setLoading(true);
    api.get<{ items: Invoice[]; total?: number; totalCount?: number }>(
      `/invoices?patientId=${patientId}&page=${p}&pageSize=${PAGE_SIZE}`
    )
      .then((r) => {
        setItems(r.data.items ?? []);
        setTotal(r.data.total ?? r.data.totalCount ?? 0);
      })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [patientId]);

  useEffect(() => { load(1); }, [load]);

  const totalPages = Math.max(1, Math.ceil(total / PAGE_SIZE));

  return (
    <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)] overflow-hidden">
      <div className="overflow-x-auto">
        <table className="min-w-full">
          <thead className="bg-[var(--c-canvas)] border-b border-[var(--c-border)]">
            <tr>
              <th className="px-4 py-2.5 text-start text-[11px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-wide">رقم الفاتورة</th>
              <th className="px-4 py-2.5 text-start text-[11px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-wide">التاريخ</th>
              <th className="px-4 py-2.5 text-start text-[11px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-wide">الحالة</th>
              <th className="px-4 py-2.5 text-end text-[11px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-wide">الإجمالي</th>
              <th className="px-4 py-2.5 text-end text-[11px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-wide">المدفوع</th>
              <th className="px-4 py-2.5 text-end text-[11px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-wide">المتبقي</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-[var(--c-border)]">
            {loading ? (
              Array.from({ length: 6 }).map((_, i) => (
                <tr key={i}>
                  {Array.from({ length: 6 }).map((__, j) => (
                    <td key={j} className="px-4 py-2.5">
                      <Skeleton className="h-3" style={{ width: j === 0 ? "80px" : "60px" }} />
                    </td>
                  ))}
                </tr>
              ))
            ) : items.length === 0 ? (
              <tr>
                <td colSpan={6} className="py-12 text-center text-[12px] text-[var(--c-text-disabled)]">
                  لا توجد فواتير
                </td>
              </tr>
            ) : items.map((inv) => (
              <tr key={inv.id} className="hover:bg-[var(--c-canvas)] transition-colors">
                <td className="px-4 py-2.5">
                  <Link href={`/finance/invoices/${inv.id}`} className="text-[12px] font-mono text-[var(--c-brand)] hover:underline">
                    {inv.invoiceNumber}
                  </Link>
                </td>
                <td className="px-4 py-2.5 text-[12px] text-[var(--c-text-secondary)]">{fmtD(inv.createdAt)}</td>
                <td className="px-4 py-2.5"><InvoiceStatusBadge value={inv.status} /></td>
                <td className="px-4 py-2.5 text-end text-[12px] font-semibold tabular-nums text-[var(--c-text-body)]">{fmtLYD(inv.totalAmount)}</td>
                <td className="px-4 py-2.5 text-end text-[12px] tabular-nums text-[var(--c-success)]">{fmtLYD(inv.paidAmount)}</td>
                <td className="px-4 py-2.5 text-end text-[12px] tabular-nums font-medium"
                  style={{ color: inv.remaining > 0 ? "var(--c-danger)" : "var(--c-text-disabled)" }}>
                  {fmtLYD(inv.remaining)}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      {totalPages > 1 && !loading && (
        <div className="flex items-center justify-between px-4 py-2.5 border-t border-[var(--c-border)] bg-[var(--c-canvas)] text-[12px]">
          <span className="text-[var(--c-text-secondary)]">
            {total} فاتورة · صفحة {page} من {totalPages}
          </span>
          <div className="flex gap-2">
            <button
              disabled={page === 1}
              onClick={() => { setPage(page - 1); load(page - 1); }}
              className="px-3 py-1 rounded border border-[var(--c-border)] hover:bg-[var(--c-surface)] disabled:opacity-40 text-[var(--c-text-body)]"
            >
              السابق
            </button>
            <button
              disabled={page === totalPages}
              onClick={() => { setPage(page + 1); load(page + 1); }}
              className="px-3 py-1 rounded border border-[var(--c-border)] hover:bg-[var(--c-surface)] disabled:opacity-40 text-[var(--c-text-body)]"
            >
              التالي
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════
   TAB: Orders (Lab + Radiology)
═══════════════════════════════════════════════════════════════════ */

function OrdersTab({ patientId }: { patientId: string }) {
  const [labOrders, setLabOrders] = useState<LabOrder[]>([]);
  const [radOrders, setRadOrders] = useState<RadiologyOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [subTab, setSubTab] = useState<"lab" | "rad">("lab");

  useEffect(() => {
    Promise.allSettled([
      api.get<{ items: LabOrder[] }>(`/lab/orders?patientId=${patientId}&pageSize=20`)
        .then((r) => setLabOrders(r.data.items ?? [])),
      api.get<{ items: RadiologyOrder[] }>(`/radiology/orders?patientId=${patientId}&pageSize=20`)
        .then((r) => setRadOrders(r.data.items ?? [])),
    ]).finally(() => setLoading(false));
  }, [patientId]);

  const tabCls = (active: boolean) =>
    `px-4 py-2 text-[12px] font-medium border-b-2 transition-colors ${
      active
        ? "border-[var(--c-brand)] text-[var(--c-brand)]"
        : "border-transparent text-[var(--c-text-secondary)] hover:text-[var(--c-text-body)]"
    }`;

  return (
    <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)] overflow-hidden">
      <div className="flex border-b border-[var(--c-border)]">
        <button className={tabCls(subTab === "lab")} onClick={() => setSubTab("lab")}>
          <span className="flex items-center gap-1.5"><FlaskConical size={13} /> المختبر ({labOrders.length})</span>
        </button>
        <button className={tabCls(subTab === "rad")} onClick={() => setSubTab("rad")}>
          <span className="flex items-center gap-1.5"><ScanLine size={13} /> الأشعة ({radOrders.length})</span>
        </button>
      </div>
      {loading ? (
        <div className="p-4 space-y-2">
          {Array.from({ length: 4 }).map((_, i) => <Skeleton key={i} className="h-10 w-full" />)}
        </div>
      ) : subTab === "lab" ? (
        <div className="divide-y divide-[var(--c-border)]">
          {labOrders.length === 0 ? (
            <Empty message="لا توجد طلبات مختبر" />
          ) : labOrders.map((o) => (
            <Link key={o.id} href={`/lab/orders/${o.id}`}
              className="flex items-center justify-between gap-3 px-4 py-2.5 hover:bg-[var(--c-canvas)] transition-colors">
              <div>
                <p className="text-[12px] font-mono text-[var(--c-brand)]">{o.orderNumber}</p>
                <p className="text-[11px] text-[var(--c-text-secondary)]">
                  {o.isExternal ? `مختبر خارجي: ${o.externalLabName ?? "—"}` : "مختبر داخلي"}
                </p>
              </div>
              <div className="text-end shrink-0">
                <p className="text-[11px] text-[var(--c-text-body)]">{fmtD(o.createdAt)}</p>
                <Badge variant="neutral">{LAB_STATUS_AR[o.status] ?? o.status}</Badge>
              </div>
            </Link>
          ))}
        </div>
      ) : (
        <div className="divide-y divide-[var(--c-border)]">
          {radOrders.length === 0 ? (
            <Empty message="لا توجد طلبات أشعة" />
          ) : radOrders.map((o) => (
            <Link key={o.id} href={`/radiology/orders/${o.id}`}
              className="flex items-center justify-between gap-3 px-4 py-2.5 hover:bg-[var(--c-canvas)] transition-colors">
              <div>
                <p className="text-[12px] font-mono text-[var(--c-brand)]">{o.orderNumber}</p>
                <p className="text-[11px] text-[var(--c-text-secondary)]">{o.radiologyTypeName}</p>
              </div>
              <div className="text-end shrink-0">
                <p className="text-[11px] text-[var(--c-text-body)]">{fmtD(o.orderDate)}</p>
                <Badge variant="neutral">{RAD_STATUS_AR[o.status] ?? o.status}</Badge>
              </div>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════
   TAB: Timeline
═══════════════════════════════════════════════════════════════════ */

function TimelineTab({ patientId }: { patientId: string }) {
  const [events, setEvents] = useState<TimelineEvent[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    api.get<{ events: TimelineEvent[] }>(`/patients/${patientId}/timeline?pageSize=50`)
      .then((r) => setEvents(r.data.events ?? []))
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [patientId]);

  if (loading) return (
    <div className="space-y-2">
      {Array.from({ length: 5 }).map((_, i) => <Skeleton key={i} className="h-12 w-full" />)}
    </div>
  );

  return (
    <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)] overflow-hidden">
      {events.length === 0 ? (
        <Empty message="لا توجد أحداث" />
      ) : (
        <div className="divide-y divide-[var(--c-border)]">
          {events.map((e) => (
            <div key={e.id} className="flex items-start gap-3 px-4 py-3">
              <div className="w-1.5 h-1.5 rounded-full bg-[var(--c-brand)] mt-1.5 shrink-0" />
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 flex-wrap">
                  <p className="text-[12px] font-medium text-[var(--c-text-body)]">{e.title}</p>
                  <Badge variant="neutral">{TIMELINE_CAT_AR[e.eventCategory] ?? e.eventCategory}</Badge>
                </div>
                {e.description && (
                  <p className="text-[11px] text-[var(--c-text-secondary)] mt-0.5">{e.description}</p>
                )}
                <div className="flex items-center gap-3 mt-0.5 text-[11px] text-[var(--c-text-disabled)]">
                  <span>{fmtDT(e.eventAt)}</span>
                  {e.actorName && <span>· {e.actorName}</span>}
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════
   TAB: Media
═══════════════════════════════════════════════════════════════════ */

function MediaTab({ patientId }: { patientId: string }) {
  return (
    <div className="bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)] p-8 text-center">
      <FolderOpen size={32} className="text-[var(--c-neutral)] mx-auto mb-3 opacity-40" />
      <p className="text-[14px] font-medium text-[var(--c-text-primary)] mb-1">ملفات المريض</p>
      <p className="text-[12px] text-[var(--c-text-secondary)] mb-4 max-w-sm mx-auto">
        صور الأشعة، الموافقات، والوثائق الطبية
      </p>
      <Link
        href={`/patients/${patientId}/media`}
        className="inline-flex items-center gap-2 text-[12px] text-[var(--c-brand)] border border-[var(--c-brand-border)] px-4 py-2 rounded-md hover:bg-[var(--c-brand-subtle)] transition-colors"
      >
        <span>فتح صفحة الملفات</span>
        <ChevronRight size={14} className="rotate-180" />
      </Link>
    </div>
  );
}

/* ══════════════════════════════════════════════════════════════════
   MAIN: Patient Workspace
═══════════════════════════════════════════════════════════════════ */

export default function PatientWorkspacePage() {
  const { id } = useParams<{ id: string }>();
  const hasPermission = useAuthStore((s) => s.hasPermission);

  const [patient, setPatient] = useState<Patient | null>(null);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [outstanding, setOutstanding] = useState(0);
  const [events, setEvents] = useState<TimelineEvent[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<Tab>("overview");

  /* Tab URL sync */
  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const t = params.get("tab") as Tab | null;
    if (t && TABS.some((tab) => tab.id === t)) setActiveTab(t);
  }, []);

  function changeTab(t: Tab) {
    setActiveTab(t);
    const url = new URL(window.location.href);
    url.searchParams.set("tab", t);
    window.history.pushState({}, "", url.toString());
  }

  /* Eager data load */
  useEffect(() => {
    if (!id) return;
    setLoading(true);
    Promise.allSettled([
      api.get<Patient>(`/patients/${id}`).then((r) => setPatient(r.data)),
      api
        .get<{ items: Appointment[] }>(`/appointments?patientId=${id}&pageSize=8`)
        .then((r) => setAppointments(r.data.items ?? [])),
      api
        .get<{ items: Invoice[] }>(`/invoices?patientId=${id}&pageSize=10`)
        .then((r) => {
          const items = r.data.items ?? [];
          setInvoices(items);
          setOutstanding(
            items
              .filter((i) => i.status !== "Paid" && i.status !== "Cancelled")
              .reduce((s, i) => s + i.remaining, 0)
          );
        }),
      api
        .get<{ events: TimelineEvent[] }>(`/patients/${id}/timeline?pageSize=8`)
        .then((r) => setEvents(r.data.events ?? []))
        .catch(() => {}),
    ]).finally(() => setLoading(false));
  }, [id]);

  if (!id) return null;

  /* ── Loading skeleton ─────────────────────────────────────────── */
  if (loading) return (
    <div dir="rtl">
      <div className="bg-[var(--c-surface)] border-b border-[var(--c-border)] px-5 py-4 space-y-2">
        <div className="flex items-center gap-3">
          <Skeleton className="w-10 h-10 rounded-lg" />
          <div className="space-y-1.5">
            <Skeleton className="h-4 w-40" />
            <Skeleton className="h-3 w-60" />
          </div>
        </div>
      </div>
      <div className="p-5 space-y-3">
        {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-20 w-full rounded-lg" />)}
      </div>
    </div>
  );

  if (!patient) return (
    <div className="p-6 text-center text-[var(--c-danger)] text-[13px]" dir="rtl">
      المريض غير موجود
    </div>
  );

  const age = calcAge(patient.dateOfBirth);
  const genderAr = patient.gender === "Male" ? "ذكر" : patient.gender === "Female" ? "أنثى" : null;
  const today = new Date().toISOString().split("T")[0];
  const todayAppt = appointments.find((a) => (a.scheduledAt ?? a.startTime ?? "").startsWith(today));

  return (
    <div dir="rtl">
      {/* ── Sticky header + tabs ──────────────────────────────────── */}
      <div className="sticky top-0 z-20 bg-[var(--c-surface)] border-b border-[var(--c-border)] shadow-sm">

        {/* Patient info row */}
        <div className="px-5 py-3 flex items-start justify-between gap-4">
          <div className="flex items-start gap-3 min-w-0">
            {/* Avatar */}
            <div
              className="w-10 h-10 rounded-lg flex items-center justify-center text-white text-[15px] font-bold shrink-0"
              style={{ background: "var(--c-brand)" }}
            >
              {patient.fullName.charAt(0)}
            </div>

            {/* Identity */}
            <div className="min-w-0">
              <div className="flex items-center gap-2 flex-wrap">
                <h1 className="text-[15px] font-bold text-[var(--c-text-primary)]">{patient.fullName}</h1>
                <span className="text-[10px] font-mono bg-[var(--c-canvas)] border border-[var(--c-border)] px-1.5 py-0.5 rounded text-[var(--c-text-secondary)]">
                  #{patient.fileNumber}
                </span>
                <Badge variant={patient.isActive ? "success" : "danger"}>
                  {patient.isActive ? "نشط" : "موقوف"}
                </Badge>
                {outstanding > 0 && (
                  <span className="inline-flex items-center gap-1 text-[11px] font-semibold text-[var(--c-danger)] bg-[var(--c-danger-bg)] border border-[var(--c-danger)]/30 px-2 py-0.5 rounded-full">
                    <AlertTriangle size={11} />
                    {fmtLYD(outstanding)} مستحق
                  </span>
                )}
              </div>
              <div className="flex items-center gap-3 mt-0.5 text-[11px] text-[var(--c-text-secondary)] flex-wrap">
                {age && <span>{age}</span>}
                {genderAr && <span>{genderAr}</span>}
                {patient.bloodType && (
                  <span className="flex items-center gap-1">
                    <Droplets size={10} /> {patient.bloodType}
                  </span>
                )}
                <span className="flex items-center gap-1">
                  <Phone size={10} /> {patient.phone}
                </span>
                {patient.phone2 && <span className="flex items-center gap-1"><Phone size={10} /> {patient.phone2}</span>}
                {todayAppt && (
                  <span className="flex items-center gap-1 text-[var(--c-brand)] font-medium">
                    <CalendarDays size={10} /> موعد اليوم: {apptTime(todayAppt)}
                  </span>
                )}
              </div>
            </div>
          </div>

          {/* Actions */}
          <div className="flex items-center gap-2 shrink-0">
            <Link
              href={`/finance/invoices/new?patientId=${id}`}
              className="flex items-center gap-1.5 text-[11px] font-medium text-white bg-[var(--c-brand)] hover:bg-[var(--c-brand-dark)] px-2.5 py-1.5 rounded-md transition-colors"
            >
              <FilePlus size={13} /> فاتورة
            </Link>
            <Link
              href="/appointments"
              className="flex items-center gap-1.5 text-[11px] font-medium text-[var(--c-text-body)] bg-[var(--c-canvas)] border border-[var(--c-border-strong)] hover:bg-[var(--c-surface)] px-2.5 py-1.5 rounded-md transition-colors"
            >
              <CalendarDays size={13} /> موعد
            </Link>
            {hasPermission("Patients.Edit") && (
              <Link
                href={`/patients/${id}/edit`}
                className="flex items-center gap-1.5 text-[11px] font-medium text-[var(--c-text-body)] bg-[var(--c-canvas)] border border-[var(--c-border-strong)] hover:bg-[var(--c-surface)] px-2.5 py-1.5 rounded-md transition-colors"
              >
                <UserCog size={13} /> تعديل
              </Link>
            )}
          </div>
        </div>

        {/* Medical alerts row */}
        {(patient.allergies || patient.chronicDiseases) && (
          <div className="px-5 pb-2.5 flex items-center gap-2 flex-wrap">
            {patient.allergies && (
              <div className="flex items-center gap-1.5 text-[11px] text-[var(--c-danger)] bg-[var(--c-danger-bg)] border border-[var(--c-danger)]/30 px-2.5 py-1 rounded-md">
                <AlertTriangle size={11} />
                <strong>حساسية:</strong>&nbsp;{patient.allergies}
              </div>
            )}
            {patient.chronicDiseases && (
              <div className="flex items-center gap-1.5 text-[11px] text-[var(--c-warning)] bg-[var(--c-warning-bg)] border border-[var(--c-warning)]/30 px-2.5 py-1 rounded-md">
                <HeartPulse size={11} />
                <strong>أمراض مزمنة:</strong>&nbsp;{patient.chronicDiseases}
              </div>
            )}
          </div>
        )}

        {/* Tab bar */}
        <div className="flex overflow-x-auto border-t border-[var(--c-border)] px-2">
          {TABS.map(({ id: tabId, label, icon: Icon }) => (
            <button
              key={tabId}
              onClick={() => changeTab(tabId)}
              className={`flex items-center gap-1.5 px-3 py-2.5 text-[12px] font-medium border-b-2 whitespace-nowrap transition-colors ${
                activeTab === tabId
                  ? "border-[var(--c-brand)] text-[var(--c-brand)]"
                  : "border-transparent text-[var(--c-text-secondary)] hover:text-[var(--c-text-body)] hover:border-[var(--c-border-strong)]"
              }`}
            >
              <Icon size={13} />
              {label}
            </button>
          ))}
        </div>
      </div>

      {/* ── Tab content ───────────────────────────────────────────── */}
      <div className="p-5">
        {activeTab === "overview" && (
          <OverviewTab
            patient={patient}
            appointments={appointments}
            invoices={invoices}
            outstanding={outstanding}
            events={events}
          />
        )}
        {activeTab === "chart"    && <ChartTab patientId={id} />}
        {activeTab === "plans"    && <PlansTab patientId={id} />}
        {activeTab === "invoices" && <InvoicesTab patientId={id} />}
        {activeTab === "orders"   && <OrdersTab patientId={id} />}
        {activeTab === "timeline" && <TimelineTab patientId={id} />}
        {activeTab === "media"    && <MediaTab patientId={id} />}
      </div>
    </div>
  );
}
