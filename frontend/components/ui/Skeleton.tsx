import { CSSProperties } from "react";
import { cn } from "@/lib/utils";

interface SkeletonProps {
  className?: string;
  style?: CSSProperties;
}

export function Skeleton({ className, style }: SkeletonProps) {
  return (
    <div
      className={cn("animate-pulse rounded bg-slate-200", className)}
      style={style}
      aria-hidden="true"
    />
  );
}

export function SkeletonRow({ cols }: { cols: number[] }) {
  return (
    <tr aria-hidden="true">
      {cols.map((w, i) => (
        <td key={i} className="px-4 py-3">
          <Skeleton style={{ width: `${w}%` }} className="h-4" />
        </td>
      ))}
    </tr>
  );
}

export function TableSkeleton({
  rows = 5,
  cols,
}: {
  rows?: number;
  cols: number[];
}) {
  return (
    <>
      {Array.from({ length: rows }).map((_, i) => (
        <SkeletonRow key={i} cols={cols} />
      ))}
    </>
  );
}
