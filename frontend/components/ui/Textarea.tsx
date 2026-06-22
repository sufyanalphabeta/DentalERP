import { forwardRef, TextareaHTMLAttributes } from "react";
import { cn } from "@/lib/utils";

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
  error?: boolean;
}

const base =
  "w-full px-3 py-2 text-[13px] text-[var(--c-text-body)] bg-[var(--c-surface)] border rounded-md resize-y transition-colors duration-150 placeholder:text-[var(--c-text-disabled)] focus:outline-none focus:ring-2 focus:ring-offset-0 disabled:bg-[var(--c-canvas)] disabled:text-[var(--c-text-disabled)] disabled:cursor-not-allowed min-h-[72px]";

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(
  ({ error, className, ...props }, ref) => (
    <textarea
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

Textarea.displayName = "Textarea";
