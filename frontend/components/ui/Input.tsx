import { forwardRef, InputHTMLAttributes } from "react";
import { cn } from "@/lib/utils";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  error?: boolean;
}

const base =
  "w-full h-9 px-3 py-2 text-[13px] text-[var(--c-text-body)] bg-[var(--c-surface)] border rounded-md transition-colors duration-150 placeholder:text-[var(--c-text-disabled)] focus:outline-none focus:ring-2 focus:ring-offset-0 disabled:bg-[var(--c-canvas)] disabled:text-[var(--c-text-disabled)] disabled:cursor-not-allowed";

export const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ error, className, ...props }, ref) => (
    <input
      ref={ref}
      className={cn(
        base,
        error
          ? "border-[var(--c-danger)] focus:ring-red-200"
          : "border-[var(--c-border-strong)] focus:border-[var(--c-brand)] focus:ring-[var(--c-brand-border)]/40",
        className
      )}
      {...props}
    />
  )
);

Input.displayName = "Input";
