"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Sidebar } from "@/components/shared/Sidebar";
import { TopBar } from "@/components/shared/TopBar";
import { useAuthStore } from "@/stores/authStore";
import { useLangStore } from "@/stores/langStore";

export default function DashboardLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const user = useAuthStore((s) => s.user);
  const lang = useLangStore((s) => s.lang);
  const [hydrated, setHydrated] = useState(false);
  const [drawerOpen, setDrawerOpen] = useState(false);

  useEffect(() => { setHydrated(true); }, []);
  useEffect(() => { if (hydrated && !user) router.replace("/login"); }, [user, router, hydrated]);

  // Close drawer on route change (link click inside sidebar)
  useEffect(() => { setDrawerOpen(false); }, []);

  if (!hydrated || !user) return null;

  const isAr = lang === "ar";

  return (
    <div
      className="flex min-h-screen"
      style={{ background: "var(--c-canvas)" }}
      dir={isAr ? "rtl" : "ltr"}
    >
      {/* ── Desktop sidebar (in flow) ──────────────────────────────── */}
      <div className="hidden lg:flex lg:flex-shrink-0">
        <Sidebar />
      </div>

      {/* ── Mobile drawer overlay ──────────────────────────────────── */}
      {drawerOpen && (
        <>
          {/* Backdrop */}
          <div
            className="lg:hidden fixed inset-0 z-40 bg-black/40"
            onClick={() => setDrawerOpen(false)}
            aria-hidden="true"
          />
          {/* Sidebar panel */}
          <div className="lg:hidden fixed inset-y-0 end-0 z-50 flex">
            <Sidebar onClose={() => setDrawerOpen(false)} />
          </div>
        </>
      )}

      {/* ── Main content area ──────────────────────────────────────── */}
      <div className="flex-1 flex flex-col min-w-0">
        {/* Mobile TopBar */}
        <TopBar
          onMenuClick={() => setDrawerOpen(true)}
          className="lg:hidden sticky top-0 z-30"
        />

        <main className="flex-1 overflow-auto">
          {children}
        </main>
      </div>
    </div>
  );
}
