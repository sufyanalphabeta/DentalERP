"use client";

import { useEffect, useState, useRef } from "react";
import { useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface Patient {
  id: string;
  fullName: string;
  fileNumber: string | null;
}

interface Doctor {
  id: string;
  fullName: string;
}

interface ServiceOption {
  id: string;
  name: string;
  code: string | null;
  price: number;
  categoryName: string | null;
}

interface InvoiceItem {
  serviceId: string | null;
  serviceName: string;
  unitPrice: string;
  quantity: number;
  discount: string;
  // search state per row
  search: string;
  showDropdown: boolean;
  options: ServiceOption[];
}

const emptyItem = (): InvoiceItem => ({
  serviceId: null,
  serviceName: "",
  unitPrice: "",
  quantity: 1,
  discount: "0",
  search: "",
  showDropdown: false,
  options: [],
});

export default function NewInvoicePage() {
  const router = useRouter();
  const [patients, setPatients] = useState<Patient[]>([]);
  const [doctors, setDoctors] = useState<Doctor[]>([]);
  const [patientSearch, setPatientSearch] = useState("");
  const [filteredPatients, setFilteredPatients] = useState<Patient[]>([]);
  const [form, setForm] = useState({ patientId: "", doctorId: "", notes: "" });
  const [items, setItems] = useState<InvoiceItem[]>([emptyItem()]);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const searchTimers = useRef<Record<number, ReturnType<typeof setTimeout>>>({});

  useEffect(() => {
    api.get<{ items: Patient[] }>("/patients?pageSize=200").then((r) => setPatients(r.data.items ?? [])).catch(() => {});
    api.get<{ items: (Doctor & { roles: string[] })[] }>("/users?pageSize=200")
      .then((r) => setDoctors((r.data.items ?? []).filter((u) => u.roles?.includes("Doctor"))))
      .catch(() => {});
  }, []);

  useEffect(() => {
    if (!patientSearch.trim()) { setFilteredPatients([]); return; }
    const q = patientSearch.toLowerCase();
    setFilteredPatients(patients.filter((p) =>
      p.fullName.toLowerCase().includes(q) || (p.fileNumber ?? "").includes(q)
    ).slice(0, 8));
  }, [patientSearch, patients]);

  function searchServices(rowIdx: number, text: string) {
    clearTimeout(searchTimers.current[rowIdx]);
    if (!text.trim()) {
      updateItem(rowIdx, { search: text, showDropdown: false, options: [], serviceId: null, serviceName: "" });
      return;
    }
    updateItem(rowIdx, { search: text, serviceId: null, serviceName: text });
    searchTimers.current[rowIdx] = setTimeout(async () => {
      try {
        const res = await api.get<ServiceOption[]>(`/services?search=${encodeURIComponent(text)}&activeOnly=true`);
        setItems((prev) => {
          const updated = [...prev];
          updated[rowIdx] = { ...updated[rowIdx], options: res.data ?? [], showDropdown: true };
          return updated;
        });
      } catch { /* ignore */ }
    }, 250);
  }

  function selectService(rowIdx: number, svc: ServiceOption) {
    setItems((prev) => {
      const updated = [...prev];
      updated[rowIdx] = {
        ...updated[rowIdx],
        serviceId: svc.id,
        serviceName: svc.name,
        unitPrice: String(svc.price),
        search: svc.name,
        showDropdown: false,
        options: [],
      };
      return updated;
    });
  }

  function updateItem(rowIdx: number, patch: Partial<InvoiceItem>) {
    setItems((prev) => {
      const updated = [...prev];
      updated[rowIdx] = { ...updated[rowIdx], ...patch };
      return updated;
    });
  }

  function addItem() { setItems((prev) => [...prev, emptyItem()]); }

  function removeItem(i: number) { setItems((prev) => prev.filter((_, idx) => idx !== i)); }

  const total = items.reduce((sum, it) => {
    const price = parseFloat(it.unitPrice) || 0;
    const disc = parseFloat(it.discount) || 0;
    return sum + Math.max(0, price - disc) * it.quantity;
  }, 0);

  async function submit() {
    if (!form.patientId || !form.doctorId) { setError("يرجى اختيار المريض والطبيب"); return; }
    const validItems = items.filter((it) => it.serviceName.trim() && parseFloat(it.unitPrice) > 0);
    if (validItems.length === 0) { setError("يرجى إضافة بند واحد على الأقل بسعر صحيح"); return; }
    setSaving(true);
    setError(null);
    try {
      const res = await api.post<{ id: string }>("/invoices", {
        patientId: form.patientId,
        doctorId: form.doctorId,
        notes: form.notes || null,
        items: validItems.map((it) => ({
          serviceName: it.serviceName,
          unitPrice: parseFloat(it.unitPrice),
          quantity: it.quantity,
          discount: parseFloat(it.discount) || 0,
        })),
      });
      router.push(`/finance/invoices/${res.data.id}`);
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } };
      setError(err?.response?.data?.error ?? "حدث خطأ أثناء إنشاء الفاتورة");
    } finally {
      setSaving(false);
    }
  }

  const selectedPatient = patients.find((p) => p.id === form.patientId);

  return (
    <div className="p-6 max-w-3xl mx-auto" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">فاتورة جديدة</h1>
          <p className="text-gray-500 text-sm mt-0.5">إنشاء فاتورة خدمات</p>
        </div>
        <button onClick={() => router.back()} className="border px-4 py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-50">رجوع</button>
      </div>

      {error && <div className="mb-4 text-sm text-red-600 bg-red-50 border border-red-200 rounded-lg px-4 py-3">{error}</div>}

      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 space-y-5 mb-6">
        <h2 className="font-semibold text-gray-800">بيانات الفاتورة</h2>

        {/* Patient search */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">المريض *</label>
          {selectedPatient ? (
            <div className="flex items-center justify-between border rounded-lg px-3 py-2 bg-blue-50">
              <span className="text-sm font-medium text-blue-800">{selectedPatient.fullName}</span>
              <button onClick={() => { setForm({ ...form, patientId: "" }); setPatientSearch(""); }} className="text-xs text-blue-600 hover:underline">تغيير</button>
            </div>
          ) : (
            <div className="relative">
              <input
                className="w-full border rounded-lg px-3 py-2 text-sm"
                placeholder="ابحث عن المريض بالاسم أو رقم الملف..."
                value={patientSearch}
                onChange={(e) => setPatientSearch(e.target.value)}
              />
              {filteredPatients.length > 0 && (
                <div className="absolute z-10 w-full mt-1 bg-white border rounded-lg shadow-lg max-h-48 overflow-y-auto">
                  {filteredPatients.map((p) => (
                    <button key={p.id} className="w-full text-right px-4 py-2 hover:bg-blue-50 text-sm" onClick={() => { setForm({ ...form, patientId: p.id }); setPatientSearch(""); setFilteredPatients([]); }}>
                      <span className="font-medium">{p.fullName}</span>
                      {p.fileNumber && <span className="text-gray-400 text-xs mr-2">#{p.fileNumber}</span>}
                    </button>
                  ))}
                </div>
              )}
            </div>
          )}
        </div>

        {/* Doctor */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">الطبيب *</label>
          <select className="w-full border rounded-lg px-3 py-2 text-sm" value={form.doctorId} onChange={(e) => setForm({ ...form, doctorId: e.target.value })}>
            <option value="">— اختر الطبيب —</option>
            {doctors.map((d) => <option key={d.id} value={d.id}>{d.fullName}</option>)}
          </select>
        </div>

        {/* Notes */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">ملاحظات</label>
          <textarea className="w-full border rounded-lg px-3 py-2 text-sm" rows={2} value={form.notes} onChange={(e) => setForm({ ...form, notes: e.target.value })} />
        </div>
      </div>

      {/* Items */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 p-6 mb-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="font-semibold text-gray-800">البنود</h2>
          <button onClick={addItem} className="text-sm text-blue-600 hover:underline">+ إضافة بند</button>
        </div>

        {/* Column headers */}
        <div className="grid grid-cols-12 gap-2 mb-1 text-xs text-gray-400">
          <div className="col-span-5">الخدمة / الصنف (اسم أو كود)</div>
          <div className="col-span-2">السعر</div>
          <div className="col-span-2">الكمية</div>
          <div className="col-span-2">الخصم</div>
          <div className="col-span-1"></div>
        </div>

        <div className="space-y-3">
          {items.map((item, i) => (
            <div key={i} className="grid grid-cols-12 gap-2 items-start">
              {/* Service search */}
              <div className="col-span-5 relative">
                <input
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  placeholder="ابحث بالاسم أو الكود..."
                  value={item.search}
                  onChange={(e) => searchServices(i, e.target.value)}
                  onBlur={() => setTimeout(() => updateItem(i, { showDropdown: false }), 150)}
                  onFocus={() => { if (item.options.length > 0) updateItem(i, { showDropdown: true }); }}
                />
                {item.showDropdown && item.options.length > 0 && (
                  <div className="absolute z-20 w-full mt-1 bg-white border rounded-lg shadow-lg max-h-52 overflow-y-auto">
                    {item.options.map((svc) => (
                      <button
                        key={svc.id}
                        onMouseDown={(e) => { e.preventDefault(); selectService(i, svc); }}
                        className="w-full text-right px-3 py-2 hover:bg-blue-50 border-b last:border-0 text-sm"
                      >
                        <div className="flex justify-between items-center">
                          <div>
                            <span className="font-medium text-gray-800">{svc.name}</span>
                            {svc.code && <span className="text-gray-400 text-xs mr-2 font-mono">{svc.code}</span>}
                            {svc.categoryName && <span className="text-gray-400 text-xs mr-1">— {svc.categoryName}</span>}
                          </div>
                          <span className="text-green-700 font-semibold text-xs">{svc.price.toFixed(2)} د.ل</span>
                        </div>
                      </button>
                    ))}
                  </div>
                )}
              </div>

              <div className="col-span-2">
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  placeholder="0.00"
                  value={item.unitPrice}
                  onChange={(e) => updateItem(i, { unitPrice: e.target.value })}
                />
              </div>
              <div className="col-span-2">
                <input
                  type="number"
                  min="1"
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  value={item.quantity}
                  onChange={(e) => updateItem(i, { quantity: parseInt(e.target.value) || 1 })}
                />
              </div>
              <div className="col-span-2">
                <input
                  type="number"
                  step="0.01"
                  min="0"
                  className="w-full border rounded-lg px-3 py-2 text-sm"
                  placeholder="0"
                  value={item.discount}
                  onChange={(e) => updateItem(i, { discount: e.target.value })}
                />
              </div>
              <div className="col-span-1 flex items-center justify-center pt-2">
                {items.length > 1 && (
                  <button onClick={() => removeItem(i)} className="text-red-400 hover:text-red-600 text-lg leading-none">×</button>
                )}
              </div>

              {/* Row total */}
              {item.unitPrice && (
                <div className="col-span-12 text-left text-xs text-gray-500 -mt-1">
                  = {(Math.max(0, (parseFloat(item.unitPrice) || 0) - (parseFloat(item.discount) || 0)) * item.quantity).toFixed(2)} د.ل
                </div>
              )}
            </div>
          ))}
        </div>

        <div className="flex justify-end mt-4 pt-4 border-t">
          <div className="text-lg font-bold text-gray-800">
            الإجمالي: <span className="text-blue-600">{total.toFixed(2)} د.ل</span>
          </div>
        </div>
      </div>

      <div className="flex justify-end gap-3">
        <button onClick={() => router.back()} className="border px-6 py-2 rounded-lg text-sm text-gray-700 hover:bg-gray-50">إلغاء</button>
        <button onClick={submit} disabled={saving} className="bg-blue-600 text-white px-6 py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">
          {saving ? "جاري الإنشاء..." : "إنشاء الفاتورة"}
        </button>
      </div>
    </div>
  );
}
