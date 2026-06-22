import { ReactNode } from "react";
import { cn } from "@/lib/utils";

interface FormFieldProps {
  label?: string;
  required?: boolean;
  error?: string;
  hint?: string;
  children: ReactNode;
  className?: string;
  labelFor?: string;
}

export function FormField({
  label,
  required,
  error,
  hint,
  children,
  className,
  labelFor,
}: FormFieldProps) {
  return (
    <div className={cn("flex flex-col gap-1", className)}>
      {label && (
        <label
          htmlFor={labelFor}
          className="block text-[12px] font-medium text-[var(--c-text-secondary)]"
        >
          {label}
          {required && (
            <span className="text-[var(--c-danger)] ms-0.5" aria-hidden>
              *
            </span>
          )}
        </label>
      )}
      {children}
      {error && (
        <p className="text-[11px] text-[var(--c-danger)]" role="alert">
          {error}
        </p>
      )}
      {!error && hint && (
        <p className="text-[11px] text-[var(--c-text-secondary)]">{hint}</p>
      )}
    </div>
  );
}
