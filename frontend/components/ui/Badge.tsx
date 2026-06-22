import { cn } from "@/lib/utils";

export type BadgeVariant =
  | "success"
  | "warning"
  | "danger"
  | "info"
  | "neutral"
  | "brand";

interface BadgeProps {
  variant?: BadgeVariant;
  children: React.ReactNode;
  className?: string;
}

const variants: Record<BadgeVariant, string> = {
  success: "bg-[var(--c-success-bg)] text-[var(--c-success)] ring-[var(--c-success)]/20",
  warning: "bg-[var(--c-warning-bg)] text-[var(--c-warning)] ring-[var(--c-warning)]/20",
  danger:  "bg-[var(--c-danger-bg)]  text-[var(--c-danger)]  ring-[var(--c-danger)]/20",
  info:    "bg-[var(--c-info-bg)]    text-[var(--c-info)]    ring-[var(--c-info)]/20",
  neutral: "bg-[var(--c-neutral-bg)] text-[var(--c-neutral)] ring-[var(--c-neutral)]/20",
  brand:   "bg-[var(--c-brand-subtle)] text-[var(--c-brand)] ring-[var(--c-brand)]/20",
};

export function Badge({ variant = "neutral", children, className }: BadgeProps) {
  return (
    <span
      className={cn(
        "inline-flex items-center px-2 py-0.5 rounded-full text-[11px] font-semibold ring-1 ring-inset whitespace-nowrap",
        variants[variant],
        className
      )}
    >
      {children}
    </span>
  );
}
