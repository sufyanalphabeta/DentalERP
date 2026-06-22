"use client";

import { ReactNode, ThHTMLAttributes, TdHTMLAttributes } from "react";
import { ChevronUp, ChevronDown } from "lucide-react";
import { TableSkeleton } from "./Skeleton";
import { cn } from "@/lib/utils";

/* ── Wrappers ──────────────────────────────────────────────────────── */

export function Table({
  children,
  className,
}: {
  children: ReactNode;
  className?: string;
}) {
  return (
    <div className={cn("bg-[var(--c-surface)] rounded-xl shadow-sm border border-[var(--c-border)] overflow-hidden", className)}>
      <div className="overflow-x-auto">
        <table className="min-w-full">{children}</table>
      </div>
    </div>
  );
}

export function Thead({ children }: { children: ReactNode }) {
  return (
    <thead className="bg-[var(--c-canvas)] border-b border-[var(--c-border)]">
      {children}
    </thead>
  );
}

export function Tbody({ children }: { children: ReactNode }) {
  return <tbody className="divide-y divide-[var(--c-border)]">{children}</tbody>;
}

/* ── Th ────────────────────────────────────────────────────────────── */

type SortDir = "asc" | "desc" | null;

interface ThProps extends ThHTMLAttributes<HTMLTableCellElement> {
  sortDir?: SortDir;
  onSort?: () => void;
}

export function Th({ children, sortDir, onSort, className, ...props }: ThProps) {
  return (
    <th
      className={cn(
        "px-4 py-2.5 text-start text-[11px] font-semibold text-[var(--c-text-secondary)] uppercase tracking-wide whitespace-nowrap",
        onSort && "cursor-pointer select-none hover:text-[var(--c-text-body)]",
        className
      )}
      onClick={onSort}
      {...props}
    >
      <span className="inline-flex items-center gap-1">
        {children}
        {onSort && (
          <span className="opacity-50">
            {sortDir === "asc" ? (
              <ChevronUp size={12} />
            ) : sortDir === "desc" ? (
              <ChevronDown size={12} />
            ) : (
              <ChevronDown size={12} className="opacity-0 group-hover:opacity-50" />
            )}
          </span>
        )}
      </span>
    </th>
  );
}

/* ── Td ────────────────────────────────────────────────────────────── */

interface TdProps extends TdHTMLAttributes<HTMLTableCellElement> {
  mono?: boolean;
  amount?: boolean;
}

export function Td({ children, mono, amount, className, ...props }: TdProps) {
  return (
    <td
      className={cn(
        "px-4 py-2.5 text-[13px] text-[var(--c-text-body)] whitespace-nowrap",
        mono && "font-mono text-[var(--c-brand)] text-[12px]",
        amount && "text-end font-semibold tabular-nums",
        className
      )}
      {...props}
    >
      {children}
    </td>
  );
}

/* ── Row ───────────────────────────────────────────────────────────── */

export function Tr({
  children,
  onClick,
  selected,
  className,
}: {
  children: ReactNode;
  onClick?: () => void;
  selected?: boolean;
  className?: string;
}) {
  return (
    <tr
      onClick={onClick}
      className={cn(
        "transition-colors",
        onClick && "cursor-pointer hover:bg-[var(--c-canvas)]",
        selected && "bg-[var(--c-brand-subtle)] border-e-2 border-e-[var(--c-brand)]",
        !selected && "bg-[var(--c-surface)]",
        className
      )}
    >
      {children}
    </tr>
  );
}

/* ── Empty ─────────────────────────────────────────────────────────── */

export function TableEmpty({
  colSpan,
  filtered = false,
  message,
}: {
  colSpan: number;
  filtered?: boolean;
  message?: string;
}) {
  return (
    <tr>
      <td colSpan={colSpan} className="py-16 text-center">
        <div className="flex flex-col items-center gap-2 text-[var(--c-text-secondary)]">
          <span className="text-2xl opacity-30">
            {filtered ? "🔍" : "📋"}
          </span>
          <p className="text-[13px]">
            {message ?? (filtered ? "لا توجد نتائج مطابقة للفلتر" : "لا توجد بيانات")}
          </p>
          {filtered && (
            <p className="text-[11px] text-[var(--c-text-disabled)]">
              حاول تغيير معايير البحث أو الفلتر
            </p>
          )}
        </div>
      </td>
    </tr>
  );
}

/* ── Loading ───────────────────────────────────────────────────────── */

export function TableLoading({
  colSpan,
  rows = 5,
  colWidths,
}: {
  colSpan: number;
  rows?: number;
  colWidths?: number[];
}) {
  const widths = colWidths ?? Array(colSpan).fill(70);
  return <TableSkeleton rows={rows} cols={widths} />;
}
