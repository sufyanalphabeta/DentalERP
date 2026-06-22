import { AlertCircle, Info, CheckCircle, TriangleAlert, X } from "lucide-react";
import { cn } from "@/lib/utils";

export type AlertVariant = "info" | "success" | "warning" | "danger";

interface AlertProps {
  variant?: AlertVariant;
  title?: string;
  children: React.ReactNode;
  onDismiss?: () => void;
  className?: string;
}

const config: Record<AlertVariant, { icon: React.ElementType; classes: string }> = {
  info:    { icon: Info,          classes: "bg-[var(--c-info-bg)]    text-[var(--c-info)]    border-[var(--c-info)]/30" },
  success: { icon: CheckCircle,   classes: "bg-[var(--c-success-bg)] text-[var(--c-success)] border-[var(--c-success)]/30" },
  warning: { icon: TriangleAlert, classes: "bg-[var(--c-warning-bg)] text-[var(--c-warning)] border-[var(--c-warning)]/30" },
  danger:  { icon: AlertCircle,   classes: "bg-[var(--c-danger-bg)]  text-[var(--c-danger)]  border-[var(--c-danger)]/30" },
};

export function Alert({ variant = "info", title, children, onDismiss, className }: AlertProps) {
  const { icon: Icon, classes } = config[variant];
  return (
    <div
      role="alert"
      className={cn(
        "flex gap-3 items-start px-4 py-3 rounded-lg border text-[13px]",
        classes,
        className
      )}
    >
      <Icon size={16} className="shrink-0 mt-0.5" />
      <div className="flex-1 min-w-0">
        {title && <p className="font-semibold mb-0.5">{title}</p>}
        <div>{children}</div>
      </div>
      {onDismiss && (
        <button
          onClick={onDismiss}
          className="shrink-0 opacity-60 hover:opacity-100 transition-opacity"
          aria-label="إغلاق"
        >
          <X size={14} />
        </button>
      )}
    </div>
  );
}
