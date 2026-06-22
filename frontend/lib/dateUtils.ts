// Safe date formatting — never produces "Invalid Date"

export function fmtDate(iso: string | null | undefined, fallback = "—"): string {
  if (!iso) return fallback;
  const d = new Date(iso);
  if (isNaN(d.getTime())) return fallback;
  return d.toLocaleDateString("ar-LY", { year: "numeric", month: "short", day: "numeric" });
}

export function fmtDateTime(iso: string | null | undefined, fallback = "—"): string {
  if (!iso) return fallback;
  const d = new Date(iso);
  if (isNaN(d.getTime())) return fallback;
  return d.toLocaleString("ar-LY", { year: "numeric", month: "short", day: "numeric", hour: "2-digit", minute: "2-digit" });
}

export function fmtTime(iso: string | null | undefined, fallback = "—"): string {
  if (!iso) return fallback;
  const d = new Date(iso);
  if (isNaN(d.getTime())) return fallback;
  return d.toLocaleTimeString("ar-SA", { hour: "2-digit", minute: "2-digit" });
}

export function fmtAge(birthDate: string | null | undefined): string {
  if (!birthDate) return "—";
  const d = new Date(birthDate);
  if (isNaN(d.getTime())) return "—";
  const age = Math.floor((Date.now() - d.getTime()) / (365.25 * 24 * 60 * 60 * 1000));
  return `${age} سنة`;
}

// Translate common English status values to Arabic
const statusMap: Record<string, string> = {
  Scheduled: "مجدول",
  Confirmed: "مؤكد",
  InProgress: "جارٍ",
  Completed: "مكتمل",
  Cancelled: "ملغي",
  NoShow: "لم يحضر",
  Waiting: "انتظار",
  Called: "نودي",
  Skipped: "تم تخطيه",
  Draft: "مسودة",
  Paid: "مدفوعة",
  PartiallyPaid: "مدفوعة جزئياً",
  Void: "ملغاة",
  Issued: "صادرة",
  Approved: "معتمد",
  Sent: "مرسل",
  PartiallyReceived: "مستلم جزئياً",
  FullyReceived: "مستلم بالكامل",
  Pending: "قيد الانتظار",
  Resulted: "صدرت النتيجة",
  Submitted: "مقدّمة",
  Rejected: "مرفوضة",
  Active: "نشط",
  Inactive: "غير نشط",
  Disposed: "مستبعد",
  Overdue: "متأخر",
};

export function translateStatus(status: string | null | undefined): string {
  if (!status) return "—";
  return statusMap[status] ?? status;
}
