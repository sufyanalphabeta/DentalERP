"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";

interface Invoice {
  id: string;
  invoiceNumber: string;
  patientName: string;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  status: string;
  createdAt: string;
}

interface VaultBalance {
  id: string;
  name: string;
  type: string;
  currentBalance: number;
}

const statusCls: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Confirmed: "bg-blue-100 text-blue-700",
  PartiallyPaid: "bg-amber-100 text-amber-700",
  Paid: "bg-green-100 text-green-700",
  Cancelled: "bg-red-100 text-red-600",
};

const statusAr: Record<string, string> = {
  Draft: "مسودة",
  Confirmed: "مؤكدة",
  PartiallyPaid: "مدفوعة جزئياً",
  Paid: "مدفوعة",
  Cancelled: "ملغاة",
};

export default function CashierWorkspace() {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [vaults, setVaults] = useState<VaultBalance[]>([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState("");
  const today = new Date().toISOString().split("T")[0];

  useEffect(() => { loadAll(); }, []);

  async function loadAll() {
    setLoading(true);
    await Promise.allSettled([
      api.get<{ items: Invoice[] }>(`/invoices?dateFrom=${today}&dateTo=${today}&pageSize=50`).then((r) => setInvoices(r.data.items ?? [])).catch(() => {}),
      api.get<VaultBalance[]>("/treasury/vaults/balances").then((r) => setVaults(r.data)).catch(() => {}),
    ]);
    setLoading(false);
  }

  const filtered = invoices.filter((inv) =>
    !search || inv.patientName.includes(search) || inv.invoiceNumber.includes(search)
  );

  const stats = {
    totalInvoices: invoices.length,
    totalAmount: invoices.reduce((s, i) => s + i.totalAmount, 0),
    collectedToday: invoices.reduce((s, i) => s + i.paidAmount, 0),
    pending: invoices.filter((i) => i.status === "Confirmed" || i.status === "PartiallyPaid").length,
  };

  return (
    <div className="p-6 space-y-6" dir="rtl">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">مساحة الصراف</h1>
          <p className="text-gray-500 text-sm">{new Date().toLocaleDateString("ar-LY", { weekday: "long", month: "long", day: "numeric" })}</p>
        </div>
        <Link href="/finance/invoices" className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">+ فاتورة جديدة</Link>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {[
          { label: "فواتير اليوم", value: stats.totalInvoices, icon: "📄", color: "bg-blue-600" },
          { label: "إجمالي المطلوب", value: `${stats.totalAmount.toFixed(2)} د.ل`, icon: "💰", color: "bg-purple-600" },
          { label: "المحصّل اليوم", value: `${stats.collectedToday.toFixed(2)} د.ل`, icon: "✅", color: "bg-emerald-600" },
          { label: "فواتير معلقة", value: stats.pending, icon: "⏳", color: stats.pending > 0 ? "bg-amber-500" : "bg-gray-400" },
        ].map((s) => (
          <div key={s.label} className="bg-white rounded-xl shadow-sm border border-gray-100 p-5">
            <div className={`w-10 h-10 ${s.color} rounded-lg flex items-center justify-center text-white text-lg mb-3`}>{s.icon}</div>
            <div className="text-xl font-bold text-gray-800">{loading ? "—" : s.value}</div>
            <div className="text-xs text-gray-500 mt-0.5">{s.label}</div>
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-4 gap-6">
        {/* Invoices */}
        <div className="lg:col-span-3 bg-white rounded-xl shadow-sm border border-gray-100">
          <div className="px-5 py-4 border-b flex items-center justify-between gap-3">
            <h2 className="font-semibold text-gray-800 whitespace-nowrap">فواتير اليوم</h2>
            <input
              type="text"
              placeholder="بحث بالاسم أو رقم الفاتورة..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="flex-1 border border-gray-200 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
            <Link href="/finance/invoices" className="text-xs text-blue-600 hover:underline whitespace-nowrap">كل الفواتير</Link>
          </div>
          {loading ? (
            <div className="p-8 text-center text-gray-400">جاري التحميل...</div>
          ) : filtered.length === 0 ? (
            <div className="p-8 text-center text-gray-400">لا توجد فواتير</div>
          ) : (
            <div className="divide-y max-h-[480px] overflow-y-auto">
              {filtered.map((inv) => (
                <div key={inv.id} className="flex items-center justify-between px-5 py-3 hover:bg-gray-50">
                  <div>
                    <div className="text-sm font-medium text-gray-800">{inv.patientName}</div>
                    <div className="text-xs text-gray-400">{inv.invoiceNumber} — {new Date(inv.createdAt).toLocaleTimeString("ar", { hour: "2-digit", minute: "2-digit" })}</div>
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="text-right">
                      <div className="text-sm font-bold text-gray-800">{inv.totalAmount.toFixed(2)} د.ل</div>
                      {inv.remainingAmount > 0 && <div className="text-xs text-red-600">متبقي: {inv.remainingAmount.toFixed(2)}</div>}
                    </div>
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusCls[inv.status] ?? "bg-gray-100 text-gray-600"}`}>
                      {statusAr[inv.status] ?? inv.status}
                    </span>
                    <Link href={`/finance/invoices/${inv.id}`} className="text-xs text-blue-600 hover:underline">فتح</Link>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Vaults */}
        <div className="lg:col-span-1 space-y-4">
          <div className="bg-white rounded-xl shadow-sm border border-gray-100">
            <div className="px-4 py-3 border-b">
              <h2 className="font-semibold text-gray-800 text-sm">أرصدة الخزائن</h2>
            </div>
            <div className="divide-y">
              {vaults.map((v) => (
                <div key={v.id} className="flex items-center justify-between px-4 py-3">
                  <span className="text-sm text-gray-700">{v.name}</span>
                  <span className={`text-sm font-bold ${v.currentBalance >= 0 ? "text-gray-800" : "text-red-600"}`}>
                    {v.currentBalance.toFixed(2)}
                  </span>
                </div>
              ))}
            </div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-4 space-y-2">
            <h3 className="text-sm font-semibold text-gray-700 mb-1">روابط سريعة</h3>
            {[
              { href: "/finance/invoices", label: "كل الفواتير", icon: "📄" },
              { href: "/finance/installments", label: "خطط التقسيط", icon: "📋" },
              { href: "/treasury/transfers", label: "تحويل خزينة", icon: "🏦" },
            ].map((l) => (
              <Link key={l.href} href={l.href} className="flex items-center gap-2 px-3 py-2 rounded-lg hover:bg-gray-50 text-sm text-gray-700">
                <span>{l.icon}</span>
                <span>{l.label}</span>
              </Link>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
