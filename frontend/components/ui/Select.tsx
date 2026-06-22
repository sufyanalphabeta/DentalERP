import { forwardRef, SelectHTMLAttributes } from "react";
import { cn } from "@/lib/utils";

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  error?: boolean;
}

const base =
  "w-full h-9 px-3 py-2 text-[13px] text-[var(--c-text-body)] bg-[var(--c-surface)] border rounded-md appearance-none transition-colors duration-150 focus:outline-none focus:ring-2 focus:ring-offset-0 disabled:bg-[var(--c-canvas)] disabled:text-[var(--c-text-disabled)] disabled:cursor-not-allowed";

// RTL-aware chevron — background-position on inline-start side
const chevronStyle = {
  backgroundImage: `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 24 24' fill='none' stroke='%2364748b' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='m6 9 6 6 6-6'/%3E%3C/svg%3E")`,
  backgroundRepeat: "no-repeat",
  backgroundPosition: "left 10px center",
  paddingLeft: "30px",
} as React.CSSProperties;

export const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ error, className, style, children, ...props }, ref) => (
    <select
      ref={ref}
      className={cn(
        base,
        error
          ? "border-[var(--c-danger)] focus:ring-red-200"
          : "border-[var(--c-border-strong)] focus:border-[var(--c-brand)] focus:ring-[var(--c-brand-border)]/40",
        className
      )}
      style={{ ...chevronStyle, ...style }}
      {...props}
    >
      {children}
    </select>
  )
);

Select.displayName = "Select";
