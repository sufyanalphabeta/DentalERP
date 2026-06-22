"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";
import { useAuthStore } from "@/stores/authStore";

interface ExternalLab {
  id: string;
  name: string;
}

interface Patient {
  id: string;
  fullName: string;
  phone: string | null;
}

interface LabOrder {
  id: string;
  orderNumber: string;
  patientName: string;
  patientId: string;
  doctorName: string | null;
  externalLabName: string | null;
  status: string;
  description: string | null;
  requestDate: string;
  totalCost: number;
  totalRevenue: number;
}

interface LabOrdersResponse {
  items: LabOrder[];
  totalCount: number;
  page: number;
  pageSize: number;
}

interface Doctor {
  id: string;
  fullName: string;
  roles: string[];
}

interface OrderItem {
  itemName: string;
  unitCost: string;
  quantity: string;
}

const statusAr: Record<string, string> = {
  Draft: "مسودة",
  Sent: "مرسل",
  InProgress: "جاري",
  ResultReceived: "وصلت النتيجة",
  Completed: "مكتمل",
  Cancelled: "ملغى",
};

const statusCls: Record<string, string> = {
  Draft: "bg-gray-100 text-gray-600",
  Sent: "bg-blue-100 text-blue-700",
  InProgress: "bg-amber-100 text-amber-800",
  ResultReceived: "bg-purple-100 text-purple-700",
  Completed: "bg-green-100 text-green-700",
  Cancelled: "bg-red-100 text-red-600",
};

export default function LabOrdersPage() {
  const { user } = useAuthStore();
  const [data, setData] = useState<LabOrdersResponse | null>(null);
  const [labs, setLabs] = useState<ExternalLab[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState("");
  const [labFilter, setLabFilter] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [page, setPage] = useState(1);
  const [resultModal, setResultModal] = useState<LabOrder | null>(null);
  const [resultNotes, setResultNotes] = useState("");
  const [saving, setSaving] = useState(false);

  const [showCreate, setShowCreate] = useState(false);
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [patientSearch, setPatientSearch] = useState("");
  const [createForm, setCreateForm] = useState({
    patientId: "",
    doctorId: "",
    labId: "",
    description: "",
    expectedAt: "",
    notes: "",
    revenue: "",
  });
  const [items, setItems] = useState<OrderItem[]>([{ itemName: "", unitCost: "", quantity: "1" }]);
  const [createError, setCreateError] = useState<string | null>(null);

  useEffect(() => {
    api.get<ExternalLab[]>("/lab/external-labs").then((r) => setLabs(r.data)).catch(() => {});
  }, []);

  useEffect(() => { load(); }, [statusFilter, labFilter, from, to, page]);

  async function load() {
    setLoading(true);
    try {
      const params = new URLSearchParams({ page: String(page), pageSize: "25" });
      if (statusFilter) params.set("status", statusFilter);
      if (from) params.set("from", from);
      if (to) params.set("to", to);
      const r = await api.get<LabOrdersResponse>(`/lab/orders?${params}`);
      setData(r.data);
    } finally {
      setLoading(false);
    }
  }

  async function openCreate() {
    setShowCreate(true);
    setCreateError(null);
    setCreateForm({ patientId: "", doctorId: "", labId: "", description: "", expectedAt: "", notes: "", revenue: "" });
    setItems([{ itemName: "", unitCost: "", quantity: "1" }]);
    setPatientSearch("");
    const [usersRes, patientsRes] = await Promise.allSettled([
      api.get<{ items: Doctor[] }>("/users?pageSize=200"),
      api.get<{ items: Patient[] }>("/patients?pageSize=50"),
    ]);
    if (usersRes.status === "fulfilled")
      setDoctors((usersRes.value.data.items ?? []).filter((u) => u.roles?.includes("Doctor")));
    if (patientsRes.status === "fulfilled")
      setPatients(patientsRes.value.data.items ?? []);
  }

  async function searchPatients(q: string) {
    setPatientSearch(q);
    if (q.length < 2) return;
    try {
      const r = await api.get<{ items: Patient[] }>(`/patients?search=${encodeURIComponent(q)}&pageSize=20`);
      setPatients(r.data.items ?? []);
    } catch { /* ignore */ }
  }

  async function submitCreate() {
    setSaving(true);
    setCreateError(null);
    try {
      const validItems = items.filter((i) => i.itemName.trim());
      await api.post("/lab/orders", {
        patientId: createForm.patientId,
        doctorId: createForm.doctorId || null,
        labId: createForm.labId || null,
        clientId: null,
        procedureId: null,
        description: createForm.description || null,
        expectedAt: createForm.expectedAt || null,
        notes: createForm.notes || null,
        revenue: createForm.revenue ? parseFloat(createForm.revenue) : null,
        items: validItems.map((i) => ({
          itemName: i.itemName,
          unitCost: parseFloat(i.unitCost) || 0,
          quantity: parseInt(i.quantity) || 1,
        })),
        createdByUserId: user?.userId || null,
      });
      setShowCreate(false);
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string } } };
      setCreateError(err?.response?.data?.message ?? "حدث خطأ أثناء إنشاء الطلب");
    } finally {
      setSaving(false);
    }
  }

  async function sendOrder(id: string) {
    await api.post(`/lab/orders/${id}/send`, {});
    load();
  }

  async function submitResult() {
    if (!resultModal) return;
    setSaving(true);
    try {
      await api.post(`/lab/orders/${resultModal.id}/result`, { notes: resultNotes });
      setResultModal(null);
      setResultNotes("");
      load();
    } finally {
      setSaving(false);
    }
  }

  async function completeOrder(id: string) {
    await api.post(`/lab/orders/${id}/complete`, {});
    load();
  }

  function addItem() {
    setItems([...items, { itemName: "", unitCost: "", quantity: "1" }]);
  }

  function removeItem(idx: number) {
    setItems(items.filter((_, i) => i !== idx));
  }

  function updateItem(idx: number, field: keyof OrderItem, value: string) {
    setItems(items.map((it, i) => i === idx ? { ...it, [field]: value } : it));
  }

  const totalCostCalc = items.reduce((sum, it) => sum + (parseFloat(it.unitCost) || 0) * (parseInt(it.quantity) || 1), 0);
  const profitCalc = createForm.revenue ? parseFloat(createForm.revenue) - totalCostCalc : 0;
  const totalPages = data ? Math.ceil(data.totalCount / 25) : 1;
  const selectedPatient = patients.find((p) => p.id === createForm.patientId);
  const filteredPatients = patientSearch.length >= 2 ? patients : patients.slice(0, 10);

  const activeCount = (data?.items ?? []).filter((o) => ["Sent", "InProgress"].includes(o.status)).length;
  const totalProfit = (data?.items ?? []).reduce((s, o) => s + ((o.totalRevenue || 0) - (o.totalCost || 0)), 0);

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">طلبات المختبر</h1>
          {data && <p className="text-sm text-gray-500 mt-0.5">{data.totalCount} طلب إجمالاً</p>}
        </div>
        <div className="flex gap-2">
          <a href="/lab/external-labs" className="border px-3 py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-50">المختبرات الخارجية</a>
          <button onClick={openCreate} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">
            + طلب جديد
          </button>
        </div>
      </div>

      <div className="grid grid-cols-4 gap-3 mb-5">
        {[
          { label: "إجمالي الطلبات", value: data?.totalCount ?? 0, color: "text-blue-600", bg: "bg-blue-50" },
          { label: "نشطة", value: activeCount, color: "text-amber-600", bg: "bg-amber-50" },
          { label: "تكلفة المختبر", value: (data?.items ?? []).reduce((s, o) => s + (o.totalCost || 0), 0).toFixed(2) + " د.ل", color: "text-red-600", bg: "bg-red-50" },
          { label: "صافي الربح", value: totalProfit.toFixed(2) + " د.ل", color: totalProfit >= 0 ? "text-green-600" : "text-red-600", bg: totalProfit >= 0 ? "bg-green-50" : "bg-red-50" },
        ].map((s) => (
          <div key={s.label} className={`${s.bg} rounded-xl p-4 border border-gray-100`}>
            <div className={`text-lg font-bold ${s.color}`}>{loading ? "—" : s.value}</div>
            <div className="text-xs text-gray-500 mt-0.5">{s.label}</div>
          </div>
        ))}
      </div>

      <div className="flex flex-wrap gap-3 mb-4">
        <select value={statusFilter} onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm">
          <option value="">كل الحالات</option>
          {Object.entries(statusAr).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
        </select>
        <select value={labFilter} onChange={(e) => { setLabFilter(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm">
          <option value="">كل المختبرات</option>
          {labs.map((l) => <option key={l.id} value={l.id}>{l.name}</option>)}
        </select>
        <input type="date" value={from} onChange={(e) => { setFrom(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm" />
        <input type="date" value={to} onChange={(e) => { setTo(e.target.value); setPage(1); }} className="border rounded-lg px-3 py-2 text-sm" />
        <button onClick={() => { setStatusFilter(""); setLabFilter(""); setFrom(""); setTo(""); setPage(1); }} className="text-sm text-gray-500 border px-3 py-2 rounded-lg hover:bg-gray-50">إعادة تعيين</button>
      </div>

      <div className="bg-white rounded-xl shadow overflow-hidden">
        <table className="min-w-full divide-y divide-gray-200">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">رقم الطلب</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المريض</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">المختبر</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الوصف</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الحالة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">التكلفة</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">الربح</th>
              <th className="px-4 py-3 text-right text-xs font-medium text-gray-500">التاريخ</th>
              <th className="px-4 py-3"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {loading ? (
              <tr><td colSpan={9} className="text-center py-8 text-gray-400">جاري التحميل...</td></tr>
            ) : (data?.items ?? []).length === 0 ? (
              <tr><td colSpan={9} className="text-center py-8 text-gray-400">لا توجد طلبات</td></tr>
            ) : (data?.items ?? []).map((o) => {
              const profit = (o.totalRevenue || 0) - (o.totalCost || 0);
              return (
                <tr key={o.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3">
                    <Link href={`/lab/orders/${o.id}`} className="text-sm font-mono text-blue-600 hover:underline">{o.orderNumber}</Link>
                  </td>
                  <td className="px-4 py-3 text-sm font-medium text-gray-800">{o.patientName}</td>
                  <td className="px-4 py-3 text-sm text-gray-600">{o.externalLabName ?? "—"}</td>
                  <td className="px-4 py-3 text-sm text-gray-600 max-w-[160px] truncate">{o.description ?? "—"}</td>
                  <td className="px-4 py-3">
                    <span className={`text-xs px-2 py-0.5 rounded-full ${statusCls[o.status] ?? "bg-gray-100 text-gray-600"}`}>
                      {statusAr[o.status] ?? o.status}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-700">{(o.totalCost || 0).toFixed(2)}</td>
                  <td className={`px-4 py-3 text-sm font-medium ${profit > 0 ? "text-green-600" : profit < 0 ? "text-red-600" : "text-gray-400"}`}>
                    {o.totalRevenue ? profit.toFixed(2) : "—"}
                  </td>
                  <td className="px-4 py-3 text-xs text-gray-500">{new Date(o.requestDate).toLocaleDateString("ar")}</td>
                  <td className="px-4 py-3 flex gap-2">
                    {o.status === "Draft" && (
                      <button onClick={() => sendOrder(o.id)} className="text-xs text-blue-600 hover:underline">إرسال</button>
                    )}
                    {o.status === "Sent" && (
                      <button onClick={() => { setResultModal(o); setResultNotes(""); }} className="text-xs text-amber-600 hover:underline">نتيجة</button>
                    )}
                    {o.status === "ResultReceived" && (
                      <button onClick={() => completeOrder(o.id)} className="text-xs text-green-600 hover:underline">إكمال</button>
                    )}
                  </td>
                </tr>
              );
            })}
          </tbody>
        </table>
        {totalPages > 1 && (
          <div className="px-4 py-3 border-t flex justify-center gap-2">
            <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="px-3 py-1 rounded border text-sm disabled:opacity-40">السابق</button>
            <span className="px-3 py-1 text-sm text-gray-600">{page} / {totalPages}</span>
            <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="px-3 py-1 rounded border text-sm disabled:opacity-40">التالي</button>
          </div>
        )}
      </div>

      {resultModal && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-sm p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-1">تسجيل نتيجة</h2>
            <p className="text-sm text-gray-500 mb-4">{resultModal.patientName} — {resultModal.orderNumber}</p>
            <textarea className="w-full border rounded-lg px-3 py-2 text-sm" rows={4} value={resultNotes} onChange={(e) => setResultNotes(e.target.value)} />
            <div className="flex gap-3 mt-5">
              <button onClick={submitResult} disabled={saving} className="flex-1 bg-amber-600 text-white py-2 rounded-lg text-sm hover:bg-amber-700 disabled:opacity-50">{saving ? "جاري الحفظ..." : "تسجيل النتيجة"}</button>
              <button onClick={() => setResultModal(null)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}

      {showCreate && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-xl shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto p-6" dir="rtl">
            <h2 className="text-lg font-bold mb-4">طلب مختبر جديد</h2>
            {createError && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{createError}</div>}

            <div className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">المريض *</label>
                {selectedPatient ? (
                  <div className="flex items-center justify-between border rounded-lg px-3 py-2 bg-blue-50">
                    <div>
                      <div className="text-sm font-medium text-gray-800">{selectedPatient.fullName}</div>
                      {selectedPatient.phone && <div className="text-xs text-gray-500">{selectedPatient.phone}</div>}
                    </div>
                    <button onClick={() => { setCreateForm({ ...createForm, patientId: "" }); setPatientSearch(""); }} className="text-xs text-red-500 hover:text-red-700">تغيير</button>
                  </div>
                ) : (
                  <div>
                    <input type="text" placeholder="ابحث باسم المريض..." value={patientSearch} onChange={(e) => searchPatients(e.target.value)} className="w-full border rounded-lg px-3 py-2 text-sm" />
                    {filteredPatients.length > 0 && (
                      <div className="border rounded-lg mt-1 max-h-36 overflow-y-auto bg-white shadow">
                        {filteredPatients.map((p) => (
                          <button key={p.id} onClick={() => { setCreateForm({ ...createForm, patientId: p.id }); setPatientSearch(""); }} className="w-full text-right px-3 py-2 hover:bg-blue-50 text-sm border-b last:border-0">
                            <div className="font-medium text-gray-800">{p.fullName}</div>
                            {p.phone && <div className="text-xs text-gray-400">{p.phone}</div>}
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                )}
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">الطبيب</label>
                  <select className="w-full border rounded-lg px-3 py-2 text-sm" value={createForm.doctorId} onChange={(e) => setCreateForm({ ...createForm, doctorId: e.target.value })}>
                    <option value="">— اختر الطبيب —</option>
                    {doctors.map((d) => <option key={d.id} value={d.id}>{d.fullName}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">المختبر الخارجي</label>
                  <select className="w-full border rounded-lg px-3 py-2 text-sm" value={createForm.labId} onChange={(e) => setCreateForm({ ...createForm, labId: e.target.value })}>
                    <option value="">— داخلي —</option>
                    {labs.map((l) => <option key={l.id} value={l.id}>{l.name}</option>)}
                  </select>
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">الوصف / نوع العمل</label>
                  <input className="w-full border rounded-lg px-3 py-2 text-sm" placeholder="مثال: تاج زيركونيا..." value={createForm.description} onChange={(e) => setCreateForm({ ...createForm, description: e.target.value })} />
                </div>
                <div>
                  <label className="block text-sm font-medium text-gray-700 mb-1">تاريخ الاستلام المتوقع</label>
                  <input type="date" className="w-full border rounded-lg px-3 py-2 text-sm" value={createForm.expectedAt} onChange={(e) => setCreateForm({ ...createForm, expectedAt: e.target.value })} />
                </div>
              </div>

              <div>
                <div className="flex items-center justify-between mb-2">
                  <label className="text-sm font-medium text-gray-700">بنود العمل</label>
                  <button onClick={addItem} className="text-xs text-blue-600 hover:underline">+ إضافة بند</button>
                </div>
                <div className="space-y-2">
                  <div className="grid grid-cols-[1fr_auto_auto_auto] gap-2 text-xs text-gray-500 px-1">
                    <span>اسم البند</span>
                    <span className="w-24 text-center">تكلفة الوحدة</span>
                    <span className="w-16 text-center">الكمية</span>
                    <span className="w-5"></span>
                  </div>
                  {items.map((item, idx) => (
                    <div key={idx} className="grid grid-cols-[1fr_auto_auto_auto] gap-2 items-center">
                      <input placeholder="اسم البند / نوع العمل" value={item.itemName} onChange={(e) => updateItem(idx, "itemName", e.target.value)} className="border rounded-lg px-3 py-2 text-sm" />
                      <input placeholder="0.00" type="number" min="0" value={item.unitCost} onChange={(e) => updateItem(idx, "unitCost", e.target.value)} className="border rounded-lg px-3 py-2 text-sm w-24" />
                      <input placeholder="1" type="number" min="1" value={item.quantity} onChange={(e) => updateItem(idx, "quantity", e.target.value)} className="border rounded-lg px-3 py-2 text-sm w-16" />
                      {items.length > 1 && (
                        <button onClick={() => removeItem(idx)} className="text-red-400 hover:text-red-600 text-lg leading-none w-5">x</button>
                      )}
                    </div>
                  ))}
                </div>
              </div>

              <div className="bg-gray-50 rounded-lg p-4 border">
                <div className="grid grid-cols-3 gap-4 text-sm">
                  <div>
                    <div className="text-gray-500 text-xs mb-1">تكلفة المختبر</div>
                    <div className="font-semibold text-red-600">{totalCostCalc.toFixed(2)} د.ل</div>
                  </div>
                  <div>
                    <div className="text-gray-500 text-xs mb-1">سعر المريض</div>
                    <input type="number" min="0" placeholder="0.00" value={createForm.revenue} onChange={(e) => setCreateForm({ ...createForm, revenue: e.target.value })} className="border rounded-lg px-2 py-1 text-sm w-full" />
                  </div>
                  <div>
                    <div className="text-gray-500 text-xs mb-1">صافي الربح</div>
                    <div className={`font-semibold ${profitCalc >= 0 ? "text-green-600" : "text-red-600"}`}>
                      {createForm.revenue ? profitCalc.toFixed(2) + " د.ل" : "—"}
                    </div>
                  </div>
                </div>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
                <textarea className="w-full border rounded-lg px-3 py-2 text-sm" rows={2} value={createForm.notes} onChange={(e) => setCreateForm({ ...createForm, notes: e.target.value })} />
              </div>
            </div>

            <div className="flex gap-3 mt-5">
              <button onClick={submitCreate} disabled={saving || !createForm.patientId} className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">
                {saving ? "جاري الحفظ..." : "إنشاء الطلب"}
              </button>
              <button onClick={() => setShowCreate(false)} className="flex-1 border py-2 rounded-lg text-sm text-gray-700">إلغاء</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
