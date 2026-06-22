import { ChevronRight, ChevronLeft } from "lucide-react";
import { cn } from "@/lib/utils";

interface PaginationProps {
  page: number;
  pageSize: number;
  total: number;
  onPage: (page: number) => void;
  className?: string;
}

export function Pagination({ page, pageSize, total, onPage, className }: PaginationProps) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  if (total === 0) return null;

  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, total);

  // Page number windows: always show first, last, and up to 3 around current
  const pages: (number | "...")[] = [];
  if (totalPages <= 7) {
    for (let i = 1; i <= totalPages; i++) pages.push(i);
  } else {
    pages.push(1);
    if (page > 3) pages.push("...");
    for (let i = Math.max(2, page - 1); i <= Math.min(totalPages - 1, page + 1); i++) {
      pages.push(i);
    }
    if (page < totalPages - 2) pages.push("...");
    pages.push(totalPages);
  }

  return (
    <div
      className={cn(
        "flex items-center justify-between px-4 py-2.5 border-t border-[var(--c-border)] bg-[var(--c-canvas)]/50 text-[12px]",
        className
      )}
    >
      <span className="text-[var(--c-text-secondary)]">
        عرض <span className="font-medium text-[var(--c-text-body)]">{from}</span>–
        <span className="font-medium text-[var(--c-text-body)]">{to}</span> من{" "}
        <span className="font-medium text-[var(--c-text-body)]">{total}</span> نتيجة
      </span>

      <div className="flex items-center gap-1">
        {/* RTL: "previous" is visually on the right — ChevronRight */}
        <button
          onClick={() => onPage(page - 1)}
          disabled={page === 1}
          className="w-7 h-7 flex items-center justify-center rounded border border-[var(--c-border)] bg-[var(--c-surface)] text-[var(--c-text-secondary)] hover:bg-[var(--c-canvas)] disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          aria-label="الصفحة السابقة"
        >
          <ChevronRight size={14} />
        </button>

        {pages.map((p, i) =>
          p === "..." ? (
            <span key={`dots-${i}`} className="px-1 text-[var(--c-text-disabled)]">
              …
            </span>
          ) : (
            <button
              key={p}
              onClick={() => onPage(p as number)}
              className={cn(
                "w-7 h-7 flex items-center justify-center rounded border text-[12px] font-medium transition-colors",
                p === page
                  ? "bg-[var(--c-brand)] text-white border-[var(--c-brand)]"
                  : "bg-[var(--c-surface)] border-[var(--c-border)] text-[var(--c-text-body)] hover:bg-[var(--c-canvas)]"
              )}
            >
              {p}
            </button>
          )
        )}

        <button
          onClick={() => onPage(page + 1)}
          disabled={page === totalPages}
          className="w-7 h-7 flex items-center justify-center rounded border border-[var(--c-border)] bg-[var(--c-surface)] text-[var(--c-text-secondary)] hover:bg-[var(--c-canvas)] disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          aria-label="الصفحة التالية"
        >
          <ChevronLeft size={14} />
        </button>
      </div>
    </div>
  );
}
