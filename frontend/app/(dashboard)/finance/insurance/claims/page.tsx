"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface InsuranceClaimSummary {
  id: string;
  claimNumber: string;
  status: string;
  insuranceCompanyName: string;
  patientId: string;
  patientName: string;
  claimedAmount: number;
  paidAmount: number;
  coveragePercent: number;
  claimDate: string;
}

interface InsuranceCompany {
  id: string;
  name: string;
}

interface InvoiceSummary {
  id: string;
  invoiceNumber: string;
  patientName: string;
  totalAmount: number;
  patientId: string;
}

const STATUS_LABELS: Record<string, string> = {
  Draft: "مسودة",
  Submitted: "مُقدَّم",
  PartiallyPaid: "مدفوع جزئياً",
  FullyPaid: "مدفوع بالكامل",
  Rejected: "مرفوض",
};

const STATUS_COLORS: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-700",
  Submitted: "bg-blue-100 text-blue-700",
  PartiallyPaid: "bg-yellow-100 text-yellow-700",
  FullyPaid: "bg-green-100 text-green-700",
  Rejected: "bg-red-100 text-red-700",
};

const STATUSES = ["Draft", "Submitted", "PartiallyPaid", "FullyPaid", "Rejected"];

const emptyForm = {
  invoiceId: "",
  insuranceCompanyId: "",
  patientId: "",
  claimedAmount: "",
  coveragePercent: "",
  notes: "",
};

export default function InsuranceClaimsPage() {
  const router = useRouter();
  const [claims, setClaims] = useState<InsuranceClaimSummary[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState("");
  const [companyFilter, setCompanyFilter] = useState("");
  const [loading, setLoading] = useState(true);
  const [companies, setCompanies] = useState<InsuranceCompany[]>([]);
  const [showCreate, setShowCreate] = useState(false);
  const [invoices, setInvoices] = useState<InvoiceSummary[]>([]);
  const [invoiceSearch, setInvoiceSearch] = useState("");
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);

  const loadClaims = () => {
    setLoading(true);
    const params = new URLSearchParams({ page: String(page), pageSize: "20" });
    if (statusFilter) params.set("status", statusFilter);
    if (companyFilter) params.set("insuranceCompanyId", companyFilter);
    api.get(`/insurance/claims?${params}`).then((r) => {
      setClaims(r.data.items ?? []);
      setTotal(r.data.totalCount ?? 0);
      setLoading(false);
    });
  };

  useEffect(() => {
    api.get("/insurance/companies").then((r) => setCompanies(r.data ?? []));
  }, []);

  useEffect(() => {
    if (showCreate) {
      api.get("/invoices?page=1&pageSize=100&status=Confirmed").then((r) =>
        setInvoices(r.data.items ?? [])
      );
    }
  }, [showCreate]);

  useEffect(loadClaims, [page, statusFilter, companyFilter]);

  const filteredInvoices = invoices.filter(
    (inv) =>
      !invoiceSearch ||
      inv.invoiceNumber.toLowerCase().includes(invoiceSearch.toLowerCase()) ||
      inv.patientName.toLowerCase().includes(invoiceSearch.toLowerCase())
  );

  const handleInvoiceSelect = (inv: InvoiceSummary) => {
    setForm((f) => ({
      ...f,
      invoiceId: inv.id,
      patientId: inv.patientId,
      claimedAmount: String(inv.totalAmount),
    }));
    setInvoiceSearch(inv.invoiceNumber);
  };

  const handleCreate = async () => {
    if (!form.invoiceId || !form.insuranceCompanyId || !form.claimedAmount) return;
    setSaving(true);
    try {
      await api.post("/insurance/claims", {
        invoiceId: form.invoiceId,
        insuranceCompanyId: form.insuranceCompanyId,
        patientId: form.patientId,
        claimedAmount: Number(form.claimedAmount),
        coveragePercent: Number(form.coveragePercent) || 0,
        notes: form.notes || null,
      });
      setShowCreate(false);
      setForm(emptyForm);
      setInvoiceSearch("");
      loadClaims();
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="p-6 max-w-6xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">مطالبات التأمين</h1>
        <button
          onClick={() => setShowCreate(true)}
          className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
        >
          + مطالبة جديدة
        </button>
      </div>

      <div className="flex gap-3 mb-5 flex-wrap items-center">
        <select
          value={companyFilter}
          onChange={(e) => { setCompanyFilter(e.target.value); setPage(1); }}
          className="border rounded-lg px-3 py-1.5 text-sm bg-white"
        >
          <option value="">كل الشركات</option>
          {companies.map((c) => (
            <option key={c.id} value={c.id}>{c.name}</option>
          ))}
        </select>

        <div className="flex gap-2 flex-wrap">
          <button
            onClick={() => { setStatusFilter(""); setPage(1); }}
            className={`px-3 py-1.5 rounded-lg text-sm border ${statusFilter === "" ? "bg-blue-600 text-white border-blue-600" : "bg-white text-gray-700"}`}
          >
            الكل ({total})
          </button>
          {STATUSES.map((s) => (
            <button
              key={s}
              onClick={() => { setStatusFilter(s); setPage(1); }}
              className={`px-3 py-1.5 rounded-lg text-sm border ${statusFilter === s ? "bg-blue-600 text-white border-blue-600" : "bg-white text-gray-700"}`}
            >
              {STATUS_LABELS[s]}
            </button>
          ))}
        </div>
      </div>

      {loading ? (
        <div className="text-center py-12 text-gray-500">جاري التحميل...</div>
      ) : claims.length === 0 ? (
        <div className="text-center py-12 text-gray-400">لا توجد مطالبات</div>
      ) : (
        <div className="bg-white border rounded-xl overflow-hidden">
          <table className="w-full text-sm">
            <thead className="bg-gray-50">
              <tr>
                <th className="text-right p-4 text-gray-600">رقم المطالبة</th>
                <th className="text-right p-4 text-gray-600">المريض</th>
                <th className="text-right p-4 text-gray-600">شركة التأمين</th>
                <th className="text-right p-4 text-gray-600">المطالب</th>
                <th className="text-right p-4 text-gray-600">المدفوع</th>
                <th className="text-right p-4 text-gray-600">التغطية</th>
                <th className="text-right p-4 text-gray-600">الحالة</th>
                <th className="text-right p-4 text-gray-600">التاريخ</th>
              </tr>
            </thead>
            <tbody>
              {claims.map((claim) => (
                <tr
                  key={claim.id}
                  onClick={() => router.push(`/finance/insurance/claims/${claim.id}`)}
                  className="border-t hover:bg-gray-50 cursor-pointer"
                >
                  <td className="p-4 font-medium text-blue-700">{claim.claimNumber}</td>
                  <td className="p-4 text-gray-700">{claim.patientName}</td>
                  <td className="p-4 text-gray-700">{claim.insuranceCompanyName}</td>
                  <td className="p-4 text-gray-800">{claim.claimedAmount.toFixed(2)} د.ل</td>
                  <td className="p-4 text-green-700 font-medium">{claim.paidAmount.toFixed(2)} د.ل</td>
                  <td className="p-4 text-gray-600">{claim.coveragePercent}%</td>
                  <td className="p-4">
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${STATUS_COLORS[claim.status] ?? "bg-gray-100 text-gray-700"}`}>
                      {STATUS_LABELS[claim.status] ?? claim.status}
                    </span>
                  </td>
                  <td className="p-4 text-gray-500">
                    {new Date(claim.claimDate).toLocaleDateString("ar-LY")}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          {total > 20 && (
            <div className="flex items-center justify-center gap-4 p-4 border-t text-sm">
              <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="border px-3 py-1 rounded disabled:opacity-50">
                السابق
              </button>
              <span className="text-gray-500">صفحة {page}</span>
              <button onClick={() => setPage((p) => p + 1)} disabled={claims.length < 20} className="border px-3 py-1 rounded disabled:opacity-50">
                التالي
              </button>
            </div>
          )}
        </div>
      )}

      {showCreate && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl p-6 w-full max-w-lg" dir="rtl">
            <h2 className="text-lg font-semibold mb-4">مطالبة تأمين جديدة</h2>
            <div className="space-y-3">
              <div>
                <label className="block text-sm text-gray-600 mb-1">الفاتورة</label>
                <input
                  placeholder="ابحث برقم الفاتورة أو اسم المريض..."
                  className="w-full border rounded-lg p-2 text-sm"
                  value={invoiceSearch}
                  onChange={(e) => { setInvoiceSearch(e.target.value); setForm((f) => ({ ...f, invoiceId: "", patientId: "" })); }}
                />
                {invoiceSearch && !form.invoiceId && filteredInvoices.length > 0 && (
                  <div className="border rounded-lg mt-1 max-h-40 overflow-y-auto bg-white shadow-lg">
                    {filteredInvoices.slice(0, 8).map((inv) => (
                      <button
                        key={inv.id}
                        onClick={() => handleInvoiceSelect(inv)}
                        className="w-full text-right px-3 py-2 text-sm hover:bg-blue-50 border-b last:border-0"
                      >
                        <span className="font-medium">{inv.invoiceNumber}</span>
                        <span className="text-gray-500 mr-2">{inv.patientName}</span>
                        <span className="text-gray-400 mr-1">— {inv.totalAmount.toFixed(2)} د.ل</span>
                      </button>
                    ))}
                  </div>
                )}
              </div>

              <div>
                <label className="block text-sm text-gray-600 mb-1">شركة التأمين</label>
                <select
                  className="w-full border rounded-lg p-2 text-sm"
                  value={form.insuranceCompanyId}
                  onChange={(e) => setForm((f) => ({ ...f, insuranceCompanyId: e.target.value }))}
                >
                  <option value="">اختر شركة...</option>
                  {companies.map((c) => (
                    <option key={c.id} value={c.id}>{c.name}</option>
                  ))}
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm text-gray-600 mb-1">المبلغ المطالب</label>
                  <input
                    type="number"
                    className="w-full border rounded-lg p-2 text-sm"
                    value={form.claimedAmount}
                    onChange={(e) => setForm((f) => ({ ...f, claimedAmount: e.target.value }))}
                  />
                </div>
                <div>
                  <label className="block text-sm text-gray-600 mb-1">نسبة التغطية %</label>
                  <input
                    type="number"
                    min={0}
                    max={100}
                    className="w-full border rounded-lg p-2 text-sm"
                    value={form.coveragePercent}
                    onChange={(e) => setForm((f) => ({ ...f, coveragePercent: e.target.value }))}
                  />
                </div>
              </div>

              <div>
                <label className="block text-sm text-gray-600 mb-1">ملاحظات</label>
                <textarea
                  rows={2}
                  className="w-full border rounded-lg p-2 text-sm"
                  value={form.notes}
                  onChange={(e) => setForm((f) => ({ ...f, notes: e.target.value }))}
                />
              </div>
            </div>

            <div className="flex gap-3 mt-5">
              <button
                onClick={handleCreate}
                disabled={saving || !form.invoiceId || !form.insuranceCompanyId || !form.claimedAmount}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm disabled:opacity-50 hover:bg-blue-700"
              >
                {saving ? "جاري الحفظ..." : "إنشاء المطالبة"}
              </button>
              <button
                onClick={() => { setShowCreate(false); setForm(emptyForm); setInvoiceSearch(""); }}
                className="flex-1 border py-2 rounded-lg text-sm"
              >
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
