import { forwardRef, ButtonHTMLAttributes, ComponentType } from "react";
import { Spinner } from "./Spinner";
import { cn } from "@/lib/utils";

export type ButtonVariant = "primary" | "secondary" | "danger" | "ghost";
export type ButtonSize = "sm" | "md" | "lg";

type LucideIconProps = { size?: number; className?: string };
type IconType = ComponentType<LucideIconProps>;

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: ButtonVariant;
  size?: ButtonSize;
  loading?: boolean;
  iconStart?: IconType;
  iconEnd?: IconType;
}

const base =
  "inline-flex items-center justify-center gap-2 font-medium rounded-md transition-colors duration-150 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-[var(--c-brand-border)] focus-visible:ring-offset-1 disabled:opacity-50 disabled:cursor-not-allowed select-none";

const variants: Record<ButtonVariant, string> = {
  primary:
    "bg-[var(--c-brand)] text-white hover:bg-[var(--c-brand-dark)] active:bg-[var(--c-brand-dark)]",
  secondary:
    "bg-[var(--c-surface)] border border-[var(--c-border-strong)] text-[var(--c-text-body)] hover:bg-[var(--c-canvas)] active:bg-slate-100",
  danger:
    "bg-[var(--c-danger)] text-white hover:bg-red-700 active:bg-red-800",
  ghost:
    "bg-transparent text-[var(--c-brand)] hover:bg-[var(--c-brand-subtle)] active:bg-blue-100",
};

const sizes: Record<ButtonSize, string> = {
  sm: "px-3 py-1.5 text-[11px] h-[30px]",
  md: "px-4 py-2 text-[13px] h-9",
  lg: "px-5 py-2.5 text-[14px] h-10",
};

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  (
    {
      variant = "primary",
      size = "md",
      loading = false,
      iconStart,
      iconEnd,
      children,
      disabled,
      className,
      ...props
    },
    ref
  ) => {
    return (
      <button
        ref={ref}
        disabled={disabled || loading}
        className={cn(base, variants[variant], sizes[size], className)}
        {...props}
      >
        {loading ? (
          <Spinner size={size === "lg" ? "md" : "sm"} className="shrink-0" />
        ) : (
          iconStart && (
            <span className="shrink-0">
              {(() => { const Icon = iconStart; return <Icon size={size === "sm" ? 13 : size === "lg" ? 16 : 14} />; })()}
            </span>
          )
        )}
        {children}
        {!loading && iconEnd && (
          <span className="shrink-0">
            {(() => { const Icon = iconEnd; return <Icon size={size === "sm" ? 13 : size === "lg" ? 16 : 14} />; })()}
          </span>
        )}
      </button>
    );
  }
);

Button.displayName = "Button";
