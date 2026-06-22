"use client";

import { useEffect, useRef, ReactNode } from "react";
import { X, TriangleAlert } from "lucide-react";
import { Button } from "./Button";
import { cn } from "@/lib/utils";

/* ── Base overlay ──────────────────────────────────────────────────── */

interface DialogBaseProps {
  open: boolean;
  onClose: () => void;
  children: ReactNode;
  maxWidth?: string;
}

function DialogOverlay({ open, onClose, children, maxWidth = "max-w-[520px]" }: DialogBaseProps) {
  const panelRef = useRef<HTMLDivElement>(null);

  // Close on Escape
  useEffect(() => {
    if (!open) return;
    const handler = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    document.addEventListener("keydown", handler);
    return () => document.removeEventListener("keydown", handler);
  }, [open, onClose]);

  // Lock scroll
  useEffect(() => {
    if (open) {
      document.body.style.overflow = "hidden";
    } else {
      document.body.style.overflow = "";
    }
    return () => { document.body.style.overflow = ""; };
  }, [open]);

  // Focus first focusable on open
  useEffect(() => {
    if (open && panelRef.current) {
      const focusable = panelRef.current.querySelector<HTMLElement>(
        'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
      );
      focusable?.focus();
    }
  }, [open]);

  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center p-4"
      aria-modal="true"
      role="dialog"
    >
      {/* Overlay */}
      <div
        className="absolute inset-0 bg-black/40"
        onClick={onClose}
        aria-hidden="true"
      />
      {/* Panel */}
      <div
        ref={panelRef}
        className={cn(
          "relative z-10 w-full bg-[var(--c-surface)] rounded-xl shadow-2xl flex flex-col",
          "max-h-[calc(100vh-2rem)] animate-in fade-in zoom-in-95 duration-150",
          maxWidth
        )}
      >
        {children}
      </div>
    </div>
  );
}

/* ── Type A: Form Dialog ───────────────────────────────────────────── */

interface FormDialogProps {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  footer?: ReactNode;
  maxWidth?: string;
}

export function FormDialog({ open, onClose, title, children, footer, maxWidth }: FormDialogProps) {
  return (
    <DialogOverlay open={open} onClose={onClose} maxWidth={maxWidth}>
      {/* Header */}
      <div className="flex items-center justify-between px-5 py-4 border-b border-[var(--c-border)] shrink-0">
        <h2 className="text-base font-semibold text-[var(--c-text-primary)]">{title}</h2>
        <button
          onClick={onClose}
          className="w-7 h-7 flex items-center justify-center rounded-md text-[var(--c-text-secondary)] hover:bg-[var(--c-canvas)] hover:text-[var(--c-text-body)] transition-colors"
          aria-label="إغلاق"
        >
          <X size={16} />
        </button>
      </div>
      {/* Body */}
      <div className="px-5 py-4 overflow-y-auto flex-1">{children}</div>
      {/* Footer */}
      {footer && (
        <div className="px-5 py-4 border-t border-[var(--c-border)] flex items-center justify-end gap-3 shrink-0 bg-[var(--c-canvas)]/50">
          {footer}
        </div>
      )}
    </DialogOverlay>
  );
}

/* ── Type B: Confirm Dialog ────────────────────────────────────────── */

interface ConfirmDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: "danger" | "warning";
  loading?: boolean;
}

export function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  message,
  confirmLabel = "تأكيد",
  cancelLabel = "إلغاء",
  variant = "danger",
  loading = false,
}: ConfirmDialogProps) {
  return (
    <DialogOverlay open={open} onClose={onClose} maxWidth="max-w-[380px]">
      <div className="p-6 flex flex-col items-center text-center gap-4">
        <div
          className={cn(
            "w-12 h-12 rounded-full flex items-center justify-center",
            variant === "danger"
              ? "bg-[var(--c-danger-bg)] text-[var(--c-danger)]"
              : "bg-[var(--c-warning-bg)] text-[var(--c-warning)]"
          )}
        >
          <TriangleAlert size={22} />
        </div>
        <div>
          <h3 className="text-base font-semibold text-[var(--c-text-primary)] mb-1">
            {title}
          </h3>
          <p className="text-[13px] text-[var(--c-text-secondary)]">{message}</p>
        </div>
        <div className="flex gap-3 w-full">
          <Button
            variant={variant === "danger" ? "danger" : "primary"}
            size="md"
            className="flex-1"
            onClick={onConfirm}
            loading={loading}
          >
            {confirmLabel}
          </Button>
          <Button variant="secondary" size="md" className="flex-1" onClick={onClose}>
            {cancelLabel}
          </Button>
        </div>
      </div>
    </DialogOverlay>
  );
}

/* ── Type C: Detail Sheet ──────────────────────────────────────────── */

interface DetailDialogProps {
  open: boolean;
  onClose: () => void;
  title: string;
  subtitle?: string;
  badge?: ReactNode;
  children: ReactNode;
  actions?: ReactNode;
}

export function DetailDialog({
  open,
  onClose,
  title,
  subtitle,
  badge,
  children,
  actions,
}: DetailDialogProps) {
  return (
    <DialogOverlay open={open} onClose={onClose} maxWidth="max-w-[640px]">
      <div className="flex items-start justify-between px-5 py-4 border-b border-[var(--c-border)] shrink-0">
        <div className="min-w-0">
          <div className="flex items-center gap-2">
            <h2 className="text-base font-semibold text-[var(--c-text-primary)] truncate">
              {title}
            </h2>
            {badge}
          </div>
          {subtitle && (
            <p className="text-[12px] text-[var(--c-text-secondary)] mt-0.5">{subtitle}</p>
          )}
        </div>
        <button
          onClick={onClose}
          className="w-7 h-7 flex items-center justify-center rounded-md text-[var(--c-text-secondary)] hover:bg-[var(--c-canvas)] hover:text-[var(--c-text-body)] transition-colors shrink-0 mt-0.5"
          aria-label="إغلاق"
        >
          <X size={16} />
        </button>
      </div>
      <div className="px-5 py-4 overflow-y-auto flex-1">{children}</div>
      {actions && (
        <div className="px-5 py-4 border-t border-[var(--c-border)] flex items-center gap-3 shrink-0 bg-[var(--c-canvas)]/50">
          {actions}
        </div>
      )}
    </DialogOverlay>
  );
}

/* ── Key-value helper for Detail dialogs ───────────────────────────── */

export function DetailGrid({ children }: { children: ReactNode }) {
  return (
    <div className="grid grid-cols-2 gap-x-6 gap-y-4">{children}</div>
  );
}

export function DetailItem({
  label,
  value,
  full,
}: {
  label: string;
  value: ReactNode;
  full?: boolean;
}) {
  return (
    <div className={full ? "col-span-2" : undefined}>
      <dt className="text-[11px] font-medium text-[var(--c-text-secondary)] uppercase tracking-wide mb-0.5">
        {label}
      </dt>
      <dd className="text-[13px] text-[var(--c-text-body)]">{value ?? "—"}</dd>
    </div>
  );
}
