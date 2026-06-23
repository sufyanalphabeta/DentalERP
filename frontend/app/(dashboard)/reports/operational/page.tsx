"use client";

import { useState, useEffect } from "react";
import { api } from "@/lib/api";
import { useAuthStore } from "@/stores/authStore";

interface InactivePatient {
  patientId: string;
  patientName: string;
  phone: string | null;
  lastVisit: string | null;
  monthsSinceLastVisit: number;
}

interface OverdueInstallment {
  installmentPlanId: string;
  invoiceNumber: string;
  patientId: string;
  patientName: string;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  overduePayments: number;
  oldestDueDate: string;
}

interface MonthlyRevenue {
  year: number;
  month: number;
  monthLabel: string;
  totalRevenue: number;
  totalPaid: number;
  totalOutstanding: number;
  invoiceCount: number;
}

interface DoctorPerformance {
  doctorId: string;
  doctorName: string;
  invoiceCount: number;
  totalRevenue: number;
  totalCommission: number;
  commissionRate: number;
}

type Section = "inactive" | "overdue" | "revenue" | "doctors";

export default function OperationalReportsPage() {
  const hasPermission = useAuthStore((s) => s.hasPermission);
  const [activeSection, setActiveSection] = useState<Section>("inactive");

  const [inactivePatients, setInactivePatients] = useState<InactivePatient[]>([]);
  const [overdueInstallments, setOverdueInstallments] = useState<OverdueInstallment[]>([]);
  const [monthlyRevenue, setMonthlyRevenue] = useState<MonthlyRevenue[]>([]);
  const [doctorPerformance, setDoctorPerformance] = useState<DoctorPerformance[]>([]);

  const [inactiveLoading, setInactiveLoading] = useState(false);
  const [overdueLoading, setOverdueLoading] = useState(false);
  const [revenueLoading, setRevenueLoading] = useState(false);
  const [doctorsLoading, setDoctorsLoading] = useState(false);

  const [inactiveMonths, setInactiveMonths] = useState(6);
  const [revenueMonths, setRevenueMonths] = useState(6);
  const [performanceMonths, setPerformanceMonths] = useState(3);

  const loadInactive = () => {
    setInactiveLoading(true);
    api.get(`/analytics/inactive-patients?months=${inactiveMonths}`)
      .then((r) => setInactivePatients(r.data ?? []))
      .finally(() => setInactiveLoading(false));
  };

  const loadOverdue = () => {
    setOverdueLoading(true);
    api.get("/analytics/overdue-installments")
      .then((r) => setOverdueInstallments(r.data ?? []))
      .finally(() => setOverdueLoading(false));
  };

  const loadRevenue = () => {
    setRevenueLoading(true);
    api.get(`/analytics/monthly-revenue?months=${revenueMonths}`)
      .then((r) => setMonthlyRevenue(r.data ?? []))
      .finally(() => setRevenueLoading(false));
  };

  const loadDoctors = () => {
    setDoctorsLoading(true);
    api.get(`/analytics/doctor-performance?months=${performanceMonths}`)
      .then((r) => setDoctorPerformance(r.data ?? []))
      .finally(() => setDoctorsLoading(false));
  };

  function exportCsv(filename: string, rows: string[][]) {
    const csv = rows.map((r) => r.join(",")).join("\n");
    const url = URL.createObjectURL(new Blob(["﻿" + csv], { type: "text/csv;charset=utf-8" }));
    const a = document.createElement("a");
    a.href = url; a.download = filename; a.click();
    URL.revokeObjectURL(url);
  }

  useEffect(() => {
    if (activeSection === "inactive") loadInactive();
    else if (activeSection === "overdue") loadOverdue();
    else if (activeSection === "revenue") loadRevenue();
    else if (activeSection === "doctors") loadDoctors();
  }, [activeSection]);

  const maxRevenue = Math.max(...monthlyRevenue.map((m) => m.totalRevenue), 1);

  const SECTIONS: { key: Section; label: string; icon: string }[] = [
    { key: "inactive", label: "المرضى غير النشطين", icon: "👥" },
    { key: "overdue", label: "الأقساط المتأخرة", icon: "⏰" },
    { key: "revenue", label: "الإيرادات الشهرية", icon: "📈" },
    { key: "doctors", label: "أداء الأطباء", icon: "🩺" },
  ];

  if (!hasPermission("Reports.Operational.View")) {
    return (
      <div className="p-12 text-center text-gray-400" dir="rtl">
        <p className="text-lg font-semibold">403 — غير مصرح</p>
        <p className="text-sm mt-1">ليس لديك صلاحية عرض التقارير التشغيلية</p>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-6xl mx-auto" dir="rtl">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">التقارير التشغيلية الذكية</h1>
        <p className="text-gray-500 text-sm mt-1">تحليلات متعمقة لأداء العيادة</p>
      </div>

      <div className="flex gap-2 mb-6 flex-wrap">
        {SECTIONS.map((s) => (
          <button
            key={s.key}
            onClick={() => setActiveSection(s.key)}
            className={`flex items-center gap-2 px-4 py-2 rounded-lg text-sm border transition-colors ${
              activeSection === s.key
                ? "bg-violet-600 text-white border-violet-600"
                : "bg-white text-gray-700 hover:bg-gray-50"
            }`}
          >
            <span>{s.icon}</span>
            {s.label}
          </button>
        ))}
      </div>

      {activeSection === "inactive" && (
        <div>
          <div className="flex items-center gap-4 mb-4">
            <h2 className="text-lg font-semibold text-gray-800">المرضى غير النشطين</h2>
            <div className="flex items-center gap-2">
              <span className="text-sm text-gray-500">لم يزوروا منذ أكثر من</span>
              <select
                value={inactiveMonths}
                onChange={(e) => setInactiveMonths(Number(e.target.value))}
                className="border rounded-lg px-2 py-1 text-sm"
              >
                {[3, 6, 9, 12].map((m) => (
                  <option key={m} value={m}>{m} أشهر</option>
                ))}
              </select>
              <button onClick={loadInactive} className="bg-violet-600 text-white px-3 py-1 rounded-lg text-sm hover:bg-violet-700">
                تحديث
              </button>
              <button
                onClick={() => exportCsv("inactive-patients.csv", [
                  ["المريض", "الهاتف", "آخر زيارة", "مدة الغياب (شهر)"],
                  ...inactivePatients.map((p) => [p.patientName, p.phone ?? "", p.lastVisit ?? "", String(p.monthsSinceLastVisit)]),
                ])}
                disabled={inactivePatients.length === 0}
                className="bg-green-600 text-white px-3 py-1 rounded-lg text-sm hover:bg-green-700 disabled:opacity-50"
              >
                📊 CSV
              </button>
            </div>
          </div>

          {inactiveLoading ? (
            <div className="text-center py-12 text-gray-500">جاري التحميل...</div>
          ) : inactivePatients.length === 0 ? (
            <div className="text-center py-12 text-gray-400">لا يوجد مرضى غير نشطين</div>
          ) : (
            <div className="bg-white border rounded-xl overflow-hidden">
              <div className="px-4 py-3 bg-violet-50 border-b flex justify-between items-center">
                <span className="text-sm font-medium text-violet-700">{inactivePatients.length} مريض</span>
              </div>
              <table className="w-full text-sm">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="text-right p-3 text-gray-600">المريض</th>
                    <th className="text-right p-3 text-gray-600">الهاتف</th>
                    <th className="text-right p-3 text-gray-600">آخر زيارة</th>
                    <th className="text-right p-3 text-gray-600">مدة الغياب</th>
                  </tr>
                </thead>
                <tbody>
                  {inactivePatients.map((p) => (
                    <tr key={p.patientId} className="border-t hover:bg-gray-50">
                      <td className="p-3 font-medium text-gray-800">{p.patientName}</td>
                      <td className="p-3 text-gray-600 font-mono">{p.phone ?? "—"}</td>
                      <td className="p-3 text-gray-500">
                        {p.lastVisit ? new Date(p.lastVisit).toLocaleDateString("ar-LY") : "لم يزر من قبل"}
                      </td>
                      <td className="p-3">
                        <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${
                          p.monthsSinceLastVisit >= 12
                            ? "bg-red-100 text-red-700"
                            : p.monthsSinceLastVisit >= 6
                            ? "bg-orange-100 text-orange-700"
                            : "bg-yellow-100 text-yellow-700"
                        }`}>
                          {p.monthsSinceLastVisit >= 999 ? "لم يزر" : `${p.monthsSinceLastVisit} شهر`}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {activeSection === "overdue" && (
        <div>
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-800">الأقساط المتأخرة</h2>
            <div className="flex gap-2">
              <button onClick={loadOverdue} className="bg-violet-600 text-white px-3 py-1 rounded-lg text-sm hover:bg-violet-700">
                تحديث
              </button>
              <button
                onClick={() => exportCsv("overdue-installments.csv", [
                  ["المريض", "رقم الفاتورة", "الإجمالي", "المتبقي", "أقساط متأخرة", "أقدم موعد"],
                  ...overdueInstallments.map((i) => [i.patientName, i.invoiceNumber, i.totalAmount.toFixed(2), i.remainingAmount.toFixed(2), String(i.overduePayments), i.oldestDueDate]),
                ])}
                disabled={overdueInstallments.length === 0}
                className="bg-green-600 text-white px-3 py-1 rounded-lg text-sm hover:bg-green-700 disabled:opacity-50"
              >
                📊 CSV
              </button>
            </div>
          </div>

          {overdueLoading ? (
            <div className="text-center py-12 text-gray-500">جاري التحميل...</div>
          ) : overdueInstallments.length === 0 ? (
            <div className="text-center py-12 text-gray-400">لا توجد أقساط متأخرة</div>
          ) : (
            <>
              <div className="grid grid-cols-3 gap-4 mb-4">
                <div className="bg-red-50 border border-red-200 rounded-xl p-4">
                  <p className="text-sm text-red-600">إجمالي الخطط المتأخرة</p>
                  <p className="text-2xl font-bold text-red-700">{overdueInstallments.length}</p>
                </div>
                <div className="bg-orange-50 border border-orange-200 rounded-xl p-4">
                  <p className="text-sm text-orange-600">إجمالي المتبقي</p>
                  <p className="text-2xl font-bold text-orange-700">
                    {overdueInstallments.reduce((s, i) => s + i.remainingAmount, 0).toFixed(2)} د.ل
                  </p>
                </div>
                <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-4">
                  <p className="text-sm text-yellow-600">عدد الأقساط المتأخرة</p>
                  <p className="text-2xl font-bold text-yellow-700">
                    {overdueInstallments.reduce((s, i) => s + i.overduePayments, 0)}
                  </p>
                </div>
              </div>

              <div className="bg-white border rounded-xl overflow-hidden">
                <table className="w-full text-sm">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="text-right p-3 text-gray-600">المريض</th>
                      <th className="text-right p-3 text-gray-600">رقم الفاتورة</th>
                      <th className="text-right p-3 text-gray-600">الإجمالي</th>
                      <th className="text-right p-3 text-gray-600">المتبقي</th>
                      <th className="text-right p-3 text-gray-600">أقساط متأخرة</th>
                      <th className="text-right p-3 text-gray-600">أقدم موعد</th>
                    </tr>
                  </thead>
                  <tbody>
                    {overdueInstallments.map((item) => (
                      <tr key={item.installmentPlanId} className="border-t hover:bg-gray-50">
                        <td className="p-3 font-medium text-gray-800">{item.patientName}</td>
                        <td className="p-3 text-blue-700">{item.invoiceNumber}</td>
                        <td className="p-3 text-gray-700">{item.totalAmount.toFixed(2)} د.ل</td>
                        <td className="p-3 text-red-600 font-medium">{item.remainingAmount.toFixed(2)} د.ل</td>
                        <td className="p-3">
                          <span className="px-2 py-0.5 bg-red-100 text-red-700 rounded-full text-xs font-medium">
                            {item.overduePayments} قسط
                          </span>
                        </td>
                        <td className="p-3 text-gray-500">
                          {new Date(item.oldestDueDate).toLocaleDateString("ar-LY")}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          )}
        </div>
      )}

      {activeSection === "revenue" && (
        <div>
          <div className="flex items-center gap-4 mb-4">
            <h2 className="text-lg font-semibold text-gray-800">الإيرادات الشهرية</h2>
            <div className="flex items-center gap-2">
              <select
                value={revenueMonths}
                onChange={(e) => setRevenueMonths(Number(e.target.value))}
                className="border rounded-lg px-2 py-1 text-sm"
              >
                {[3, 6, 9, 12].map((m) => (
                  <option key={m} value={m}>آخر {m} أشهر</option>
                ))}
              </select>
              <button onClick={loadRevenue} className="bg-violet-600 text-white px-3 py-1 rounded-lg text-sm hover:bg-violet-700">
                تحديث
              </button>
              <button
                onClick={() => exportCsv("monthly-revenue.csv", [
                  ["الشهر", "عدد الفواتير", "الإيرادات", "المحصل", "المتبقي"],
                  ...monthlyRevenue.map((m) => [m.monthLabel, String(m.invoiceCount), m.totalRevenue.toFixed(2), m.totalPaid.toFixed(2), m.totalOutstanding.toFixed(2)]),
                ])}
                disabled={monthlyRevenue.length === 0}
                className="bg-green-600 text-white px-3 py-1 rounded-lg text-sm hover:bg-green-700 disabled:opacity-50"
              >
                📊 CSV
              </button>
            </div>
          </div>

          {revenueLoading ? (
            <div className="text-center py-12 text-gray-500">جاري التحميل...</div>
          ) : (
            <>
              <div className="grid grid-cols-3 gap-4 mb-6">
                <div className="bg-green-50 border border-green-200 rounded-xl p-4">
                  <p className="text-sm text-green-600">إجمالي الإيرادات</p>
                  <p className="text-2xl font-bold text-green-700">
                    {monthlyRevenue.reduce((s, m) => s + m.totalRevenue, 0).toFixed(2)} د.ل
                  </p>
                </div>
                <div className="bg-blue-50 border border-blue-200 rounded-xl p-4">
                  <p className="text-sm text-blue-600">إجمالي المحصل</p>
                  <p className="text-2xl font-bold text-blue-700">
                    {monthlyRevenue.reduce((s, m) => s + m.totalPaid, 0).toFixed(2)} د.ل
                  </p>
                </div>
                <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-4">
                  <p className="text-sm text-yellow-600">إجمالي المتبقي</p>
                  <p className="text-2xl font-bold text-yellow-700">
                    {monthlyRevenue.reduce((s, m) => s + m.totalOutstanding, 0).toFixed(2)} د.ل
                  </p>
                </div>
              </div>

              <div className="bg-white border rounded-xl p-5 mb-4">
                <h3 className="text-sm font-medium text-gray-600 mb-4">مخطط الإيرادات الشهرية</h3>
                <div className="flex items-end gap-3 h-40">
                  {monthlyRevenue.map((m) => {
                    const heightPct = maxRevenue > 0 ? (m.totalRevenue / maxRevenue) * 100 : 0;
                    return (
                      <div key={`${m.year}-${m.month}`} className="flex-1 flex flex-col items-center gap-1">
                        <span className="text-xs text-gray-500">{m.totalRevenue > 0 ? m.totalRevenue.toFixed(0) : ""}</span>
                        <div className="w-full flex flex-col justify-end" style={{ height: "100px" }}>
                          <div
                            className="w-full bg-violet-500 rounded-t-md transition-all"
                            style={{ height: `${heightPct}%`, minHeight: m.invoiceCount > 0 ? "4px" : "0" }}
                          />
                        </div>
                        <span className="text-xs text-gray-500 text-center leading-tight">{(m.monthLabel ?? "").split(" ")[0]}</span>
                      </div>
                    );
                  })}
                </div>
              </div>

              <div className="bg-white border rounded-xl overflow-hidden">
                <table className="w-full text-sm">
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="text-right p-3 text-gray-600">الشهر</th>
                      <th className="text-right p-3 text-gray-600">عدد الفواتير</th>
                      <th className="text-right p-3 text-gray-600">الإيرادات</th>
                      <th className="text-right p-3 text-gray-600">المحصل</th>
                      <th className="text-right p-3 text-gray-600">المتبقي</th>
                    </tr>
                  </thead>
                  <tbody>
                    {[...monthlyRevenue].reverse().map((m) => (
                      <tr key={`${m.year}-${m.month}`} className="border-t hover:bg-gray-50">
                        <td className="p-3 font-medium text-gray-800">{m.monthLabel}</td>
                        <td className="p-3 text-gray-600">{m.invoiceCount}</td>
                        <td className="p-3 text-gray-800">{m.totalRevenue.toFixed(2)} د.ل</td>
                        <td className="p-3 text-green-700 font-medium">{m.totalPaid.toFixed(2)} د.ل</td>
                        <td className="p-3 text-red-600">{m.totalOutstanding.toFixed(2)} د.ل</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </>
          )}
        </div>
      )}

      {activeSection === "doctors" && (
        <div>
          <div className="flex items-center gap-4 mb-4">
            <h2 className="text-lg font-semibold text-gray-800">أداء الأطباء</h2>
            <div className="flex items-center gap-2">
              <select
                value={performanceMonths}
                onChange={(e) => setPerformanceMonths(Number(e.target.value))}
                className="border rounded-lg px-2 py-1 text-sm"
              >
                {[1, 3, 6, 12].map((m) => (
                  <option key={m} value={m}>آخر {m} {m === 1 ? "شهر" : "أشهر"}</option>
                ))}
              </select>
              <button onClick={loadDoctors} className="bg-violet-600 text-white px-3 py-1 rounded-lg text-sm hover:bg-violet-700">
                تحديث
              </button>
              <button
                onClick={() => exportCsv("doctor-performance.csv", [
                  ["الطبيب", "عدد الفواتير", "الإيرادات", "العمولة", "نسبة العمولة"],
                  ...doctorPerformance.map((d) => [d.doctorName, String(d.invoiceCount), d.totalRevenue.toFixed(2), d.totalCommission.toFixed(2), `${d.commissionRate.toFixed(1)}%`]),
                ])}
                disabled={doctorPerformance.length === 0}
                className="bg-green-600 text-white px-3 py-1 rounded-lg text-sm hover:bg-green-700 disabled:opacity-50"
              >
                📊 CSV
              </button>
            </div>
          </div>

          {doctorsLoading ? (
            <div className="text-center py-12 text-gray-500">جاري التحميل...</div>
          ) : doctorPerformance.length === 0 ? (
            <div className="text-center py-12 text-gray-400">لا توجد بيانات للفترة المحددة</div>
          ) : (
            <div className="bg-white border rounded-xl overflow-hidden">
              <table className="w-full text-sm">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="text-right p-3 text-gray-600">الطبيب</th>
                    <th className="text-right p-3 text-gray-600">عدد الفواتير</th>
                    <th className="text-right p-3 text-gray-600">إجمالي الإيرادات</th>
                    <th className="text-right p-3 text-gray-600">العمولة</th>
                    <th className="text-right p-3 text-gray-600">متوسط نسبة العمولة</th>
                  </tr>
                </thead>
                <tbody>
                  {doctorPerformance.map((d, idx) => (
                    <tr key={d.doctorId} className="border-t hover:bg-gray-50">
                      <td className="p-3">
                        <div className="flex items-center gap-2">
                          {idx === 0 && <span className="text-yellow-500 text-sm">🥇</span>}
                          {idx === 1 && <span className="text-gray-400 text-sm">🥈</span>}
                          {idx === 2 && <span className="text-amber-700 text-sm">🥉</span>}
                          <span className="font-medium text-gray-800">{d.doctorName}</span>
                        </div>
                      </td>
                      <td className="p-3 text-gray-600">{d.invoiceCount}</td>
                      <td className="p-3 font-medium text-gray-800">{d.totalRevenue.toFixed(2)} د.ل</td>
                      <td className="p-3 text-violet-700 font-medium">{d.totalCommission.toFixed(2)} د.ل</td>
                      <td className="p-3 text-gray-500">{d.commissionRate.toFixed(1)}%</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
