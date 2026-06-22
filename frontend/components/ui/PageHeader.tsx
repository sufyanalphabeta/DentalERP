import { ReactNode } from "react";
import { ChevronLeft } from "lucide-react";
import { cn } from "@/lib/utils";

interface Breadcrumb {
  label: string;
  href?: string;
}

interface PageHeaderProps {
  title: string;
  breadcrumbs?: Breadcrumb[];
  actions?: ReactNode;
  className?: string;
}

export function PageHeader({ title, breadcrumbs, actions, className }: PageHeaderProps) {
  return (
    <div
      className={cn(
        "flex items-start justify-between gap-4 mb-5",
        className
      )}
    >
      <div className="min-w-0">
        {breadcrumbs && breadcrumbs.length > 0 && (
          <nav className="flex items-center gap-1 mb-1" aria-label="breadcrumb">
            {breadcrumbs.map((crumb, i) => (
              <span key={i} className="flex items-center gap-1">
                {i > 0 && (
                  <ChevronLeft
                    size={12}
                    className="text-[var(--c-text-disabled)] rotate-180"
                  />
                )}
                {crumb.href ? (
                  <a
                    href={crumb.href}
                    className="text-[11px] text-[var(--c-brand)] hover:underline"
                  >
                    {crumb.label}
                  </a>
                ) : (
                  <span className="text-[11px] text-[var(--c-text-secondary)]">
                    {crumb.label}
                  </span>
                )}
              </span>
            ))}
          </nav>
        )}
        <h1 className="text-xl font-bold text-[var(--c-text-primary)] truncate">
          {title}
        </h1>
      </div>
      {actions && (
        <div className="flex items-center gap-2 shrink-0">{actions}</div>
      )}
    </div>
  );
}
