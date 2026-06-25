"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface SystemSetting {
  key: string;
  value: string;
  group: string;
  description: string | null;
  // Legacy alias — may be undefined from API
  label?: string;
  type?: string;
}

const groupAr: Record<string, string> = {
  company:       "هوية الشركة والمستندات",
  clinic:        "بيانات العيادة",
  financial:     "الإعدادات المالية",
  notifications: "الإشعارات",
  system:        "إعدادات النظام",
};

// Group sort order — company first
const groupOrder = ["company", "clinic", "financial", "notifications", "system"];

function settingLabel(s: SystemSetting): string {
  return s.label || s.description || s.key;
}

function isMultiline(key: string): boolean {
  return key.endsWith("termsAndConditions") || key.endsWith("footerNotes");
}

function isBooleanField(s: SystemSetting): boolean {
  return s.type === "boolean" || s.value === "true" || s.value === "false";
}

export default function SystemSettingsPage() {
  const [settings, setSettings] = useState<Record<string, SystemSetting[]>>({});
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState<string | null>(null);
  const [editedValues, setEditedValues] = useState<Record<string, string>>({});
  const [savedKeys, setSavedKeys] = useState<string[]>([]);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const r = await api.get<SystemSetting[]>("/settings");
      const grouped: Record<string, SystemSetting[]> = {};
      for (const s of r.data) {
        if (!grouped[s.group]) grouped[s.group] = [];
        grouped[s.group].push(s);
      }
      setSettings(grouped);
      const vals: Record<string, string> = {};
      for (const s of r.data) vals[s.key] = s.value;
      setEditedValues(vals);
    } finally {
      setLoading(false);
    }
  }

  async function saveSetting(key: string) {
    setSaving(key);
    try {
      await api.put(`/settings/${key}`, { value: editedValues[key] });
      setSavedKeys((prev) => [...prev, key]);
      setTimeout(() => setSavedKeys((prev) => prev.filter((k) => k !== key)), 2000);
    } finally {
      setSaving(null);
    }
  }

  if (loading) return <div className="p-6 text-center text-gray-400">جاري التحميل...</div>;

  return (
    <div className="p-6" dir="rtl">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900">إعدادات النظام</h1>
        <p className="text-gray-500 text-sm mt-1">تخصيص إعدادات العيادة والنظام</p>
      </div>

      {Object.keys(settings).length === 0 ? (
        <div className="text-center py-12 text-gray-400">لا توجد إعدادات قابلة للتعديل</div>
      ) : (
        <div className="space-y-6">
          {[...groupOrder, ...Object.keys(settings).filter((g) => !groupOrder.includes(g))]
            .filter((g) => g in settings)
            .map((group) => {
              const items = settings[group];
              return (
                <div key={group} className="bg-white rounded-xl shadow-sm border border-gray-100">
                  <div className="px-5 py-4 border-b bg-gray-50 rounded-t-xl flex items-center gap-2">
                    {group === "company" && (
                      <span className="text-blue-600">
                        <svg className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                          <path strokeLinecap="round" strokeLinejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                        </svg>
                      </span>
                    )}
                    <h2 className="font-semibold text-gray-800">{groupAr[group] ?? group}</h2>
                    {group === "company" && (
                      <span className="text-xs text-blue-600 bg-blue-50 px-2 py-0.5 rounded-full mr-auto">
                        تُستخدم في المستندات المطبوعة
                      </span>
                    )}
                  </div>
                  <div className="divide-y">
                    {items.map((s) => (
                      <div key={s.key} className={`px-5 py-4 gap-4 ${isMultiline(s.key) ? "flex-col flex" : "flex items-center justify-between"}`}>
                        <div className="min-w-0 flex-1">
                          <div className="text-sm font-medium text-gray-800">{settingLabel(s)}</div>
                          <div className="text-xs text-gray-400 font-mono mt-0.5">{s.key}</div>
                        </div>
                        <div className={`flex gap-3 ${isMultiline(s.key) ? "items-end" : "items-center flex-shrink-0"}`}>
                          {isBooleanField(s) ? (
                            <select
                              className="border rounded-lg px-3 py-1.5 text-sm"
                              value={editedValues[s.key] ?? s.value}
                              onChange={(e) => setEditedValues({ ...editedValues, [s.key]: e.target.value })}
                            >
                              <option value="true">نعم</option>
                              <option value="false">لا</option>
                            </select>
                          ) : isMultiline(s.key) ? (
                            <textarea
                              rows={4}
                              className="border rounded-lg px-3 py-2 text-sm w-full font-mono"
                              value={editedValues[s.key] ?? s.value}
                              onChange={(e) => setEditedValues({ ...editedValues, [s.key]: e.target.value })}
                              placeholder={s.description ?? ""}
                            />
                          ) : (
                            <input
                              type={s.type === "number" ? "number" : "text"}
                              className="border rounded-lg px-3 py-1.5 text-sm w-56"
                              value={editedValues[s.key] ?? s.value}
                              onChange={(e) => setEditedValues({ ...editedValues, [s.key]: e.target.value })}
                              placeholder={s.description ?? ""}
                            />
                          )}
                          <button
                            onClick={() => saveSetting(s.key)}
                            disabled={saving === s.key}
                            className={`text-xs px-3 py-1.5 rounded-lg border transition-colors whitespace-nowrap ${
                              savedKeys.includes(s.key)
                                ? "bg-green-100 text-green-700 border-green-200"
                                : "bg-blue-600 text-white border-blue-600 hover:bg-blue-700"
                            } disabled:opacity-50`}
                          >
                            {saving === s.key ? "..." : savedKeys.includes(s.key) ? "تم ✓" : "حفظ"}
                          </button>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              );
            })}
        </div>
      )}
    </div>
  );
}
