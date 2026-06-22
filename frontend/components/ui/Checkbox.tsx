import { forwardRef, InputHTMLAttributes } from "react";
import { cn } from "@/lib/utils";

interface CheckboxProps extends Omit<InputHTMLAttributes<HTMLInputElement>, "type"> {
  label?: string;
  description?: string;
}

export const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
  ({ label, description, className, id, ...props }, ref) => {
    const inputId = id ?? `chk-${Math.random().toString(36).slice(2, 7)}`;
    return (
      <label
        htmlFor={inputId}
        className={cn(
          "inline-flex items-start gap-2.5 cursor-pointer select-none",
          props.disabled && "opacity-50 cursor-not-allowed",
          className
        )}
      >
        <input
          ref={ref}
          id={inputId}
          type="checkbox"
          className="mt-0.5 w-4 h-4 rounded border-[var(--c-border-strong)] accent-[var(--c-brand)] shrink-0 cursor-pointer disabled:cursor-not-allowed"
          {...props}
        />
        {(label || description) && (
          <span className="flex flex-col">
            {label && (
              <span className="text-[13px] text-[var(--c-text-body)] leading-snug">
                {label}
              </span>
            )}
            {description && (
              <span className="text-[11px] text-[var(--c-text-secondary)] leading-snug mt-0.5">
                {description}
              </span>
            )}
          </span>
        )}
      </label>
    );
  }
);

Checkbox.displayName = "Checkbox";
