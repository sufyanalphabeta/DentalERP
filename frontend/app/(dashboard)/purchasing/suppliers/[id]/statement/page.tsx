"use client";

import { useEffect, useState } from "react";
import { useParams } from "next/navigation";
import { api } from "@/lib/api";

interface StatementRow {
  description: string;
  date: string | null;
  reference: string | null;
  debit: number;
  credit: number;
  balance: number;
  type: string;
}

interface StatementData {
  supplierId: string;
  supplierName: string;
  supplierCode: string;
  openingBalance: number;
  closingBalance: number;
  rows: StatementRow[];
}

const TYPE_LABEL: Record<string, string> = {
  opening: "رصيد افتتاحي",
  invoice: "فاتورة",
  payment: "دفعة",
  return: "مردود",
};

const TYPE_CLS: Record<string, string> = {
  opening: "bg-gray-50 text-gray-600",
  invoice: "text-red-700",
  payment: "text-green-700",
  return: "text-blue-700",
};

export default function SupplierStatementPage() {
  const { id } = useParams<{ id: string }>();
  const [data, setData] = useState<StatementData | null>(null);
  const [loading, setLoading] = useState(true);
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [pdfLoading, setPdfLoading] = useState(false);

  useEffect(() => { load(); }, [id, from, to]);

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams();
      if (from) params.set("from", from);
      if (to) params.set("to", to);
      const r = await api.get<StatementData>(`/purchasing/suppliers/${id}/statement?${params}`);
      setData(r.data);
    } finally {
      setLoading(false);
    }
  }

  async function handlePrint() {
    setPdfLoading(true);
    try {
      const params = new URLSearchParams();
      if (from) params.set("from", from);
      if (to) params.set("to", to);
      const res = await api.get(`/suppliers/${id}/statement/pdf?${params}`, { responseType: "blob" });
      const url = URL.createObjectURL(new Blob([res.data], { type: "application/pdf" }));
      const a = document.createElement("a");
      a.href = url;
      a.download = `supplier-statement-${id}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    } finally {
      setPdfLoading(false);
    }
  }

  if (loading) return <div className="p-6 text-center text-gray-400">جاري التحميل...</div>;
  if (!data) return <div className="p-6 text-center text-red-500">تعذر تحميل كشف الحساب</div>;

  return (
    <div className="p-6" dir="rtl">
      {/* Controls */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">كشف حساب مورد</h1>
          <p className="text-sm text-gray-500 mt-0.5">{data.supplierName} ({data.supplierCode})</p>
        </div>
        <div className="flex gap-2 items-center">
          <input type="date" value={from} onChange={(e) => setFrom(e.target.value)} className="border rounded-lg px-3 py-2 text-sm" placeholder="من" />
          <input type="date" value={to} onChange={(e) => setTo(e.target.value)} className="border rounded-lg px-3 py-2 text-sm" placeholder="إلى" />
          <button onClick={() => { setFrom(""); setTo(""); }} className="border px-3 py-2 rounded-lg text-sm text-gray-500 hover:bg-gray-50">
            كل الفترات
          </button>
          <button onClick={handlePrint} disabled={pdfLoading} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">
            {pdfLoading ? "جاري التحميل..." : "📄 تحميل PDF"}
          </button>
        </div>
      </div>

      <div>
        {/* Print header */}
        <div className="mb-4 pb-4 border-b">
          <div className="text-center mb-2">
            <h2 className="text-xl font-bold">كشف حساب مورد</h2>
            {(from || to) && (
              <p className="text-sm text-gray-500">
                الفترة: {from || "—"} إلى {to || "—"}
              </p>
            )}
          </div>
          <div className="flex justify-between text-sm text-gray-600">
            <div>
              <span className="font-medium">المورد: </span>{data.supplierName}
            </div>
            <div>
              <span className="font-medium">الكود: </span>{data.supplierCode}
            </div>
            <div>
              <span className="font-medium">تاريخ الطباعة: </span>
              {new Date().toLocaleDateString("ar")}
            </div>
          </div>
        </div>

        {/* Summary cards */}
        <div className="grid grid-cols-3 gap-4 mb-6 no-print">
          <div className="bg-gray-50 rounded-xl p-4 border">
            <div className="text-xs text-gray-500 mb-1">رصيد افتتاحي</div>
            <div className="text-lg font-bold text-gray-800">{data.openingBalance.toFixed(2)} <span className="text-sm font-normal">د.ل</span></div>
          </div>
          <div className="bg-red-50 rounded-xl p-4 border border-red-100">
            <div className="text-xs text-red-500 mb-1">إجمالي الفواتير</div>
            <div className="text-lg font-bold text-red-700">
              {data.rows.filter(r => r.type === "invoice").reduce((s, r) => s + r.debit, 0).toFixed(2)} <span className="text-sm font-normal">د.ل</span>
            </div>
          </div>
          <div className="bg-green-50 rounded-xl p-4 border border-green-100">
            <div className="text-xs text-green-600 mb-1">إجمالي المدفوعات</div>
            <div className="text-lg font-bold text-green-700">
              {data.rows.filter(r => r.type === "payment").reduce((s, r) => s + r.credit, 0).toFixed(2)} <span className="text-sm font-normal">د.ل</span>
            </div>
          </div>
        </div>

        {/* Statement table */}
        <div className="bg-white rounded-xl shadow overflow-hidden">
          <table className="min-w-full text-sm">
            <thead className="bg-gray-100 border-b">
              <tr>
                <th className="px-4 py-3 text-right text-xs text-gray-600">التاريخ</th>
                <th className="px-4 py-3 text-right text-xs text-gray-600">البيان</th>
                <th className="px-4 py-3 text-right text-xs text-gray-600">مرجع</th>
                <th className="px-4 py-3 text-right text-xs text-gray-600">مدين (علينا)</th>
                <th className="px-4 py-3 text-right text-xs text-gray-600">دائن (له)</th>
                <th className="px-4 py-3 text-right text-xs text-gray-600 font-bold">الرصيد</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {data.rows.map((row, i) => (
                <tr key={i} className={`hover:bg-gray-50 ${TYPE_CLS[row.type] ?? ""}`}>
                  <td className="px-4 py-2 text-xs text-gray-500">{row.date ?? "—"}</td>
                  <td className="px-4 py-2 text-sm">{row.description}</td>
                  <td className="px-4 py-2 text-xs text-gray-400">{row.reference ?? "—"}</td>
                  <td className="px-4 py-2 text-sm text-red-600 font-medium">{row.debit > 0 ? row.debit.toFixed(2) : "—"}</td>
                  <td className="px-4 py-2 text-sm text-green-600 font-medium">{row.credit > 0 ? row.credit.toFixed(2) : "—"}</td>
                  <td className={`px-4 py-2 text-sm font-bold ${row.balance >= 0 ? "text-red-700" : "text-green-700"}`}>
                    {Math.abs(row.balance).toFixed(2)} {row.balance >= 0 ? "دائن" : "مدين"}
                  </td>
                </tr>
              ))}
            </tbody>
            <tfoot className="bg-gray-50 border-t-2 border-gray-300">
              <tr>
                <td colSpan={5} className="px-4 py-3 text-sm font-bold text-gray-700">الرصيد الختامي</td>
                <td className={`px-4 py-3 text-base font-bold ${data.closingBalance >= 0 ? "text-red-700" : "text-green-700"}`}>
                  {Math.abs(data.closingBalance).toFixed(2)} {data.closingBalance >= 0 ? "دائن" : "مدين"}
                </td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>
    </div>
  );
}
