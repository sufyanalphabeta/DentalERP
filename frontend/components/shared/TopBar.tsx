"use client";

import { Menu, HeartPulse } from "lucide-react";
import { cn } from "@/lib/utils";

interface TopBarProps {
  onMenuClick: () => void;
  className?: string;
}

export function TopBar({ onMenuClick, className }: TopBarProps) {
  return (
    <header
      className={cn(
        "h-12 flex items-center justify-between px-4 shrink-0",
        className
      )}
      style={{ background: "var(--c-sidebar-bg)" }}
    >
      {/* Brand */}
      <div className="flex items-center gap-2">
        <div
          className="w-7 h-7 rounded-lg flex items-center justify-center"
          style={{ background: "var(--c-sidebar-active)" }}
        >
          <HeartPulse size={14} className="text-white" />
        </div>
        <span className="text-[13px] font-bold text-white">DentalERP</span>
      </div>

      {/* Hamburger */}
      <button
        onClick={onMenuClick}
        className="w-8 h-8 flex items-center justify-center rounded-lg transition-colors"
        style={{ color: "var(--c-sidebar-text)" }}
        onMouseEnter={(e) =>
          ((e.currentTarget as HTMLElement).style.background = "var(--c-sidebar-hover)")
        }
        onMouseLeave={(e) =>
          ((e.currentTarget as HTMLElement).style.background = "transparent")
        }
        aria-label="فتح القائمة"
      >
        <Menu size={18} />
      </button>
    </header>
  );
}
