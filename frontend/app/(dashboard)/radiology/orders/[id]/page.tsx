"use client";

import { useState, useEffect } from "react";
import { useParams, useRouter } from "next/navigation";
import { api } from "@/lib/api";

interface RadiologyOrderDetail {
  id: string;
  orderNumber: string;
  status: string;
  radiologyTypeName: string;
  price: number;
  isExternalPatient: boolean;
  externalPatientName: string | null;
  externalPatientPhone: string | null;
  patientId: string | null;
  doctorId: string | null;
  technicianId: string | null;
  invoiceId: string | null;
  doctorCommissionAmount: number;
  techCommissionAmount: number;
  notes: string | null;
  cancellationReason: string | null;
  orderDate: string;
  images: { id: string; fileName: string; fileSize: number; contentType: string | null; uploadedAt: string }[];
  report: { id: string; reportText: string; reportedById: string; reportedAt: string; updatedAt: string | null } | null;
}

const STATUS_LABELS: Record<string, string> = {
  Ordered: "مطلوب",
  Imaged: "تصوير مكتمل",
  ReportSaved: "تقرير محفوظ",
  Completed: "مكتمل",
  Cancelled: "ملغى",
};

export default function RadiologyOrderDetailPage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const [order, setOrder] = useState<RadiologyOrderDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [reportText, setReportText] = useState("");
  const [showReportEditor, setShowReportEditor] = useState(false);
  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState("");

  const load = () => {
    setLoading(true);
    api.get(`/radiology/orders/${id}`).then((r) => {
      setOrder(r.data);
      setReportText(r.data.report?.reportText ?? "");
      setLoading(false);
    });
  };

  useEffect(load, [id]);

  const handleMarkImaged = async () => {
    await api.post(`/radiology/orders/${id}/imaged`);
    load();
  };

  const handleSaveReport = async () => {
    await api.post(`/radiology/orders/${id}/report`, {
      reportText,
      reportedById: "00000000-0000-0000-0000-000000000001",
    });
    setShowReportEditor(false);
    load();
  };

  const handleComplete = async () => {
    await api.post(`/radiology/orders/${id}/complete`);
    load();
  };

  const handleCancel = async () => {
    await api.post(`/radiology/orders/${id}/cancel`, { reason: cancelReason });
    setShowCancelModal(false);
    load();
  };

  if (loading) return <div className="p-6 text-center text-gray-500">جاري التحميل...</div>;
  if (!order) return <div className="p-6 text-center text-red-500">لم يتم العثور على الطلب</div>;

  return (
    <div className="p-6 max-w-4xl mx-auto" dir="rtl">
      <button onClick={() => router.back()} className="text-sm text-blue-600 hover:underline mb-4">
        → رجوع
      </button>

      <div className="bg-white border rounded-xl p-6 mb-6">
        <div className="flex items-start justify-between mb-4">
          <div>
            <h1 className="text-xl font-bold text-gray-900">{order.orderNumber}</h1>
            <p className="text-gray-500 text-sm mt-1">{order.radiologyTypeName}</p>
            {order.isExternalPatient && (
              <p className="text-sm text-purple-600 mt-1">
                مريض خارجي: {order.externalPatientName} — {order.externalPatientPhone}
              </p>
            )}
          </div>
          <span className="px-3 py-1 bg-blue-100 text-blue-700 rounded-full text-sm font-medium">
            {STATUS_LABELS[order.status] ?? order.status}
          </span>
        </div>

        <div className="grid grid-cols-3 gap-4 text-sm mb-6">
          <div>
            <p className="text-gray-500">السعر</p>
            <p className="font-semibold">{order.price.toFixed(2)} د.ل</p>
          </div>
          <div>
            <p className="text-gray-500">عمولة الطبيب</p>
            <p className="font-semibold">{order.doctorCommissionAmount.toFixed(2)} د.ل</p>
          </div>
          <div>
            <p className="text-gray-500">عمولة الفني</p>
            <p className="font-semibold">{order.techCommissionAmount.toFixed(2)} د.ل</p>
          </div>
        </div>

        <div className="flex flex-wrap gap-3">
          {order.status === "Ordered" && (
            <button onClick={handleMarkImaged} className="bg-yellow-500 text-white px-4 py-2 rounded-lg text-sm hover:bg-yellow-600">
              تأكيد التصوير
            </button>
          )}
          {order.status === "Imaged" && (
            <button onClick={() => setShowReportEditor(true)} className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700">
              كتابة تقرير
            </button>
          )}
          {order.status === "ReportSaved" && (
            <>
              <button onClick={() => setShowReportEditor(true)} className="border border-blue-400 text-blue-600 px-4 py-2 rounded-lg text-sm hover:bg-blue-50">
                تعديل التقرير
              </button>
              <button onClick={handleComplete} className="bg-green-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-green-700">
                اعتماد واكتمال
              </button>
            </>
          )}
          {!["Completed", "Cancelled"].includes(order.status) && (
            <button onClick={() => setShowCancelModal(true)} className="border border-red-300 text-red-600 px-4 py-2 rounded-lg text-sm hover:bg-red-50">
              إلغاء
            </button>
          )}
        </div>
      </div>

      {order.images.length > 0 && (
        <div className="bg-white border rounded-xl p-5 mb-6">
          <h2 className="font-semibold text-gray-700 mb-3">الصور الإشعاعية ({order.images.length})</h2>
          <div className="space-y-2">
            {order.images.map((img) => (
              <div key={img.id} className="flex items-center gap-3 p-2 bg-gray-50 rounded-lg text-sm">
                <span className="text-gray-600">{img.fileName}</span>
                <span className="text-gray-400 text-xs">({(img.fileSize / 1024).toFixed(1)} KB)</span>
              </div>
            ))}
          </div>
        </div>
      )}

      {order.report && (
        <div className="bg-white border rounded-xl p-5">
          <h2 className="font-semibold text-gray-700 mb-3">التقرير الإشعاعي</h2>
          <p className="text-sm text-gray-800 whitespace-pre-wrap">{order.report.reportText}</p>
          <p className="text-xs text-gray-400 mt-2">
            {new Date(order.report.reportedAt).toLocaleDateString("ar-LY")}
            {order.report.updatedAt && ` • آخر تعديل: ${new Date(order.report.updatedAt).toLocaleDateString("ar-LY")}`}
          </p>
        </div>
      )}

      {showReportEditor && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-xl" dir="rtl">
            <h2 className="text-lg font-semibold mb-4">تقرير الأشعة</h2>
            <textarea
              rows={8}
              placeholder="اكتب التقرير الإشعاعي..."
              className="w-full border rounded-lg p-3 text-sm"
              value={reportText}
              onChange={(e) => setReportText(e.target.value)}
            />
            <div className="flex gap-3 mt-4">
              <button onClick={handleSaveReport} disabled={!reportText.trim()} className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50">
                حفظ التقرير
              </button>
              <button onClick={() => setShowReportEditor(false)} className="flex-1 border py-2 rounded-lg text-sm">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}

      {showCancelModal && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md" dir="rtl">
            <h2 className="text-lg font-semibold mb-4 text-red-600">إلغاء طلب الأشعة</h2>
            <textarea
              placeholder="سبب الإلغاء..."
              rows={3}
              className="w-full border rounded-lg p-2 text-sm"
              value={cancelReason}
              onChange={(e) => setCancelReason(e.target.value)}
            />
            <div className="flex gap-3 mt-4">
              <button onClick={handleCancel} disabled={!cancelReason.trim()} className="flex-1 bg-red-600 text-white py-2 rounded-lg text-sm disabled:opacity-50">
                تأكيد الإلغاء
              </button>
              <button onClick={() => setShowCancelModal(false)} className="flex-1 border py-2 rounded-lg text-sm">
                رجوع
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
