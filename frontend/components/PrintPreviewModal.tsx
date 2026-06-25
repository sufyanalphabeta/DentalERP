"use client";

import { useEffect, useRef, useState } from "react";
import { fetchBlob } from "@/lib/api";

export type PrintFormat = "A4" | "A4L";

interface PrintPreviewModalProps {
  /** API path to the PDF endpoint (relative, e.g. "/purchasing/invoices/{id}/pdf") */
  pdfPath: string;
  /** Default paper format */
  defaultFormat?: PrintFormat;
  /** Called when the user closes the modal */
  onClose: () => void;
  /** Document title shown in the modal toolbar */
  title?: string;
}

const FORMAT_LABELS: Record<PrintFormat, string> = {
  A4:  "A4 عمودي",
  A4L: "A4 أفقي",
};

export default function PrintPreviewModal({
  pdfPath,
  defaultFormat = "A4",
  onClose,
  title = "معاينة المستند",
}: PrintPreviewModalProps) {
  const [format, setFormat]       = useState<PrintFormat>(defaultFormat);
  const [blobUrl, setBlobUrl]     = useState<string | null>(null);
  const [loading, setLoading]     = useState(true);
  const [error, setError]         = useState<string | null>(null);
  const iframeRef                 = useRef<HTMLIFrameElement>(null);
  const prevBlobRef               = useRef<string | null>(null);

  // Load PDF whenever format changes
  useEffect(() => {
    loadPdf();
    return () => {
      if (prevBlobRef.current) URL.revokeObjectURL(prevBlobRef.current);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [format, pdfPath]);

  async function loadPdf() {
    setLoading(true);
    setError(null);

    // Revoke previous blob to avoid memory leaks
    if (prevBlobRef.current) {
      URL.revokeObjectURL(prevBlobRef.current);
      prevBlobRef.current = null;
    }

    const landscape = format === "A4L";
    const sep = pdfPath.includes("?") ? "&" : "?";
    const url = `${pdfPath}${sep}landscape=${landscape}`;

    try {
      const response = await fetchBlob(url);
      const blob = new Blob([response.data], { type: "application/pdf" });
      const objUrl = URL.createObjectURL(blob);
      prevBlobRef.current = objUrl;
      setBlobUrl(objUrl);
    } catch {
      setError("تعذّر تحميل المستند. تأكد من صلاحياتك وحاول مجدداً.");
    } finally {
      setLoading(false);
    }
  }

  function handlePrint() {
    if (iframeRef.current?.contentWindow) {
      iframeRef.current.contentWindow.focus();
      iframeRef.current.contentWindow.print();
    }
  }

  function handleDownload() {
    if (!blobUrl) return;
    const a = document.createElement("a");
    a.href = blobUrl;
    a.download = `document-${Date.now()}.pdf`;
    a.click();
  }

  // Trap Escape key
  useEffect(() => {
    function onKey(e: KeyboardEvent) { if (e.key === "Escape") onClose(); }
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [onClose]);

  return (
    <div className="fixed inset-0 z-[9999] flex flex-col bg-gray-900/80 backdrop-blur-sm" dir="rtl">
      {/* ── Toolbar ─────────────────────────────────────────────────────── */}
      <div className="flex items-center justify-between bg-gray-800 px-4 py-2.5 shadow-lg flex-shrink-0">
        {/* Right side */}
        <div className="flex items-center gap-3">
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-white transition-colors p-1.5 rounded hover:bg-gray-700"
            title="إغلاق (Esc)"
          >
            <svg className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
          <span className="text-white font-medium text-sm">{title}</span>
        </div>

        {/* Center: format picker */}
        <div className="flex items-center gap-1 bg-gray-700 rounded-lg p-1">
          {(Object.keys(FORMAT_LABELS) as PrintFormat[]).map((f) => (
            <button
              key={f}
              onClick={() => setFormat(f)}
              className={`px-3 py-1 rounded text-xs font-medium transition-all ${
                format === f
                  ? "bg-blue-600 text-white shadow"
                  : "text-gray-300 hover:text-white hover:bg-gray-600"
              }`}
            >
              {FORMAT_LABELS[f]}
            </button>
          ))}
        </div>

        {/* Left side: actions */}
        <div className="flex items-center gap-2">
          <button
            onClick={loadPdf}
            disabled={loading}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-gray-700 hover:bg-gray-600 text-gray-200 rounded text-sm transition-colors disabled:opacity-40"
            title="إعادة تحميل"
          >
            <svg className={`w-4 h-4 ${loading ? "animate-spin" : ""}`} fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
            </svg>
            تحديث
          </button>

          <button
            onClick={handleDownload}
            disabled={!blobUrl || loading}
            className="flex items-center gap-1.5 px-3 py-1.5 bg-gray-700 hover:bg-gray-600 text-gray-200 rounded text-sm transition-colors disabled:opacity-40"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4" />
            </svg>
            تنزيل PDF
          </button>

          <button
            onClick={handlePrint}
            disabled={!blobUrl || loading}
            className="flex items-center gap-1.5 px-4 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded text-sm font-medium transition-colors disabled:opacity-40 shadow"
          >
            <svg className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" d="M17 17h2a2 2 0 002-2v-4a2 2 0 00-2-2H5a2 2 0 00-2 2v4a2 2 0 002 2h2m2 4h6a2 2 0 002-2v-4a2 2 0 00-2-2H9a2 2 0 00-2 2v4a2 2 0 002 2zm8-12V5a2 2 0 00-2-2H9a2 2 0 00-2 2v4h10z" />
            </svg>
            طباعة
          </button>
        </div>
      </div>

      {/* ── Preview area ────────────────────────────────────────────────── */}
      <div className="flex-1 overflow-hidden bg-gray-700 p-4 flex items-center justify-center">
        {loading && (
          <div className="flex flex-col items-center gap-3 text-gray-300">
            <svg className="w-10 h-10 animate-spin" fill="none" stroke="currentColor" strokeWidth={1.5} viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" d="M16.023 9.348h4.992v-.001M2.985 19.644v-4.992m0 0h4.992m-4.993 0l3.181 3.183a8.25 8.25 0 0013.803-3.7M4.031 9.865a8.25 8.25 0 0113.803-3.7l3.181 3.182m0-4.991v4.99" />
            </svg>
            <span className="text-sm">جاري تحميل المستند...</span>
          </div>
        )}

        {!loading && error && (
          <div className="bg-red-900/50 border border-red-500 rounded-xl p-8 text-center max-w-md">
            <svg className="w-12 h-12 text-red-400 mx-auto mb-3" fill="none" stroke="currentColor" strokeWidth={1.5} viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
            </svg>
            <p className="text-red-300 font-medium mb-2">خطأ في تحميل المستند</p>
            <p className="text-red-400 text-sm">{error}</p>
            <button
              onClick={loadPdf}
              className="mt-4 px-4 py-2 bg-red-700 hover:bg-red-600 text-white rounded-lg text-sm"
            >
              إعادة المحاولة
            </button>
          </div>
        )}

        {!loading && !error && blobUrl && (
          <iframe
            ref={iframeRef}
            src={blobUrl}
            className="w-full h-full rounded shadow-2xl bg-white"
            style={{ maxWidth: format === "A4L" ? "100%" : "850px" }}
            title={title}
          />
        )}
      </div>
    </div>
  );
}
