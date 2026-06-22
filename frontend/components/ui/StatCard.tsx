import Link from "next/link";
import { type LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

export type StatVariant = "brand" | "success" | "warning" | "danger" | "neutral";

interface StatCardProps {
  label: string;
  value: string | number | null;
  icon: LucideIcon;
  variant?: StatVariant;
  href: string;
  subLabel?: string;
  loading?: boolean;
  className?: string;
}

const variantStyles: Record<StatVariant, { border: string; icon: string; value: string }> = {
  brand:   { border: "border-s-[3px] border-s-[var(--c-brand)]",   icon: "text-[var(--c-brand)]",   value: "text-[var(--c-text-primary)]" },
  success: { border: "border-s-[3px] border-s-[var(--c-success)]", icon: "text-[var(--c-success)]", value: "text-[var(--c-text-primary)]" },
  warning: { border: "border-s-[3px] border-s-[var(--c-warning)]", icon: "text-[var(--c-warning)]", value: "text-[var(--c-warning)]" },
  danger:  { border: "border-s-[3px] border-s-[var(--c-danger)]",  icon: "text-[var(--c-danger)]",  value: "text-[var(--c-danger)]" },
  neutral: { border: "border-s-[3px] border-s-[var(--c-neutral)]", icon: "text-[var(--c-neutral)]", value: "text-[var(--c-text-primary)]" },
};

export function StatCard({
  label,
  value,
  icon: Icon,
  variant = "neutral",
  href,
  subLabel,
  loading = false,
  className,
}: StatCardProps) {
  const styles = variantStyles[variant];

  return (
    <Link href={href} className={cn("block group", className)}>
      <div
        className={cn(
          "bg-[var(--c-surface)] rounded-lg border border-[var(--c-border)] px-4 py-3",
          "hover:shadow-sm hover:border-[var(--c-border-strong)] transition-all",
          styles.border
        )}
      >
        <div className="flex items-start justify-between gap-2">
          <div className="flex-1 min-w-0">
            <p className="text-[11px] font-medium text-[var(--c-text-secondary)] uppercase tracking-wide mb-1.5">
              {label}
            </p>
            {loading ? (
              <div className="h-7 w-16 bg-slate-200 animate-pulse rounded" />
            ) : (
              <p className={cn("text-2xl font-bold leading-none tabular-nums", styles.value)}>
                {value ?? "—"}
              </p>
            )}
            {subLabel && !loading && (
              <p className="text-[11px] text-[var(--c-text-disabled)] mt-1">{subLabel}</p>
            )}
          </div>
          <Icon
            size={18}
            className={cn("shrink-0 mt-0.5 opacity-70 group-hover:opacity-100 transition-opacity", styles.icon)}
          />
        </div>
      </div>
    </Link>
  );
}
