import { Badge } from "@/components/ui/Badge";

type BadgeVariant = "success" | "warning" | "danger" | "info" | "neutral" | "brand";

function status(label: string, variant: BadgeVariant) {
  return { label, variant } as const;
}

/* ── Invoice / Payment ────────────────────────────────────────────── */

const INVOICE_STATUS = {
  Draft: status("مسودة", "neutral"),
  Posted: status("مُرحَّل", "brand"),
  Paid: status("مدفوع", "success"),
  PartiallyPaid: status("مدفوع جزئياً", "warning"),
  Cancelled: status("ملغى", "danger"),
  Overdue: status("متأخر", "danger"),
} as const;

const PAYMENT_STATUS = {
  Pending: status("معلق", "warning"),
  Completed: status("مكتمل", "success"),
  Cancelled: status("ملغى", "danger"),
  Reversed: status("معكوس", "neutral"),
} as const;

/* ── Appointment ──────────────────────────────────────────────────── */

const APPOINTMENT_STATUS = {
  Scheduled: status("مجدول", "brand"),
  Confirmed: status("مؤكد", "info"),
  InProgress: status("جارٍ", "warning"),
  Completed: status("مكتمل", "success"),
  NoShow: status("لم يحضر", "danger"),
  Cancelled: status("ملغى", "neutral"),
} as const;

/* ── Patient ──────────────────────────────────────────────────────── */

const PATIENT_STATUS = {
  Active: status("نشط", "success"),
  Inactive: status("غير نشط", "neutral"),
  Blocked: status("محظور", "danger"),
} as const;

/* ── Expense ──────────────────────────────────────────────────────── */

const EXPENSE_STATUS = {
  Draft: status("مسودة", "neutral"),
  Posted: status("مُرحَّل", "success"),
  Cancelled: status("ملغى", "danger"),
} as const;

/* ── Purchase Invoice ─────────────────────────────────────────────── */

const PURCHASE_INVOICE_STATUS = {
  Draft: status("مسودة", "neutral"),
  Posted: status("مُرحَّل", "brand"),
  Paid: status("مدفوع", "success"),
  PartiallyPaid: status("مدفوع جزئياً", "warning"),
  Cancelled: status("ملغى", "danger"),
} as const;

/* ── Purchase Return ──────────────────────────────────────────────── */

const PURCHASE_RETURN_STATUS = {
  Draft: status("مسودة", "neutral"),
  Confirmed: status("مؤكد", "info"),
  Completed: status("مكتمل", "success"),
  Cancelled: status("ملغى", "danger"),
} as const;

/* ── Supplier ─────────────────────────────────────────────────────── */

const SUPPLIER_STATUS = {
  Active: status("نشط", "success"),
  Inactive: status("غير نشط", "neutral"),
} as const;

/* ── Inventory Item ───────────────────────────────────────────────── */

const ITEM_STATUS = {
  Active: status("نشط", "success"),
  Inactive: status("غير نشط", "neutral"),
  LowStock: status("مخزون منخفض", "warning"),
  OutOfStock: status("نفذ المخزون", "danger"),
} as const;

/* ── Asset ────────────────────────────────────────────────────────── */

const ASSET_STATUS = {
  Active: status("فعّال", "success"),
  UnderMaintenance: status("تحت الصيانة", "warning"),
  Disposed: status("مُخصَّص", "neutral"),
  Retired: status("متقاعد", "danger"),
} as const;

/* ── Insurance Claim ──────────────────────────────────────────────── */

const INSURANCE_CLAIM_STATUS = {
  Pending: status("معلق", "warning"),
  Submitted: status("مُقدَّم", "info"),
  Approved: status("موافق عليه", "success"),
  Rejected: status("مرفوض", "danger"),
  PartiallyApproved: status("موافق جزئي", "warning"),
} as const;

/* ── Installment Plan ─────────────────────────────────────────────── */

const INSTALLMENT_STATUS = {
  Active: status("نشط", "brand"),
  Completed: status("مكتمل", "success"),
  Defaulted: status("متعثر", "danger"),
  Cancelled: status("ملغى", "neutral"),
} as const;

/* ── Vault / Treasury ─────────────────────────────────────────────── */

const VAULT_TRANSACTION_TYPE = {
  Deposit: status("إيداع", "success"),
  Withdrawal: status("سحب", "danger"),
  Transfer: status("تحويل", "info"),
} as const;

/* ── Lab Order ────────────────────────────────────────────────────── */

const LAB_ORDER_STATUS = {
  Sent: status("مُرسَل", "info"),
  InProgress: status("جارٍ", "warning"),
  Ready: status("جاهز", "success"),
  Delivered: status("مُسلَّم", "brand"),
  Cancelled: status("ملغى", "neutral"),
} as const;

/* ── Generic fallback ─────────────────────────────────────────────── */

function unknown(value: string) {
  return status(value, "neutral");
}

/* ── Badge components ─────────────────────────────────────────────── */

function makeBadge<T extends Record<string, { label: string; variant: BadgeVariant }>>(
  map: T
) {
  return function StatusBadgeComponent({ value }: { value: keyof T | string }) {
    const entry = map[value as keyof T] ?? unknown(String(value));
    return <Badge variant={entry.variant}>{entry.label}</Badge>;
  };
}

export const InvoiceStatusBadge = makeBadge(INVOICE_STATUS);
export const PaymentStatusBadge = makeBadge(PAYMENT_STATUS);
export const AppointmentStatusBadge = makeBadge(APPOINTMENT_STATUS);
export const PatientStatusBadge = makeBadge(PATIENT_STATUS);
export const ExpenseStatusBadge = makeBadge(EXPENSE_STATUS);
export const PurchaseInvoiceStatusBadge = makeBadge(PURCHASE_INVOICE_STATUS);
export const PurchaseReturnStatusBadge = makeBadge(PURCHASE_RETURN_STATUS);
export const SupplierStatusBadge = makeBadge(SUPPLIER_STATUS);
export const ItemStatusBadge = makeBadge(ITEM_STATUS);
export const AssetStatusBadge = makeBadge(ASSET_STATUS);
export const InsuranceClaimStatusBadge = makeBadge(INSURANCE_CLAIM_STATUS);
export const InstallmentStatusBadge = makeBadge(INSTALLMENT_STATUS);
export const VaultTransactionTypeBadge = makeBadge(VAULT_TRANSACTION_TYPE);
export const LabOrderStatusBadge = makeBadge(LAB_ORDER_STATUS);
