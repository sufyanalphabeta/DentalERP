"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface VaultBalance {
  id: string;
  name: string;
  type: string;
  openingBalance: number;
  totalIn: number;
  totalOut: number;
  currentBalance: number;
}

const typeLabel: Record<string, { label: string; icon: string }> = {
  cash: { label: "نقدي", icon: "💵" },
  bank: { label: "بنك", icon: "🏦" },
  card: { label: "بطاقة", icon: "💳" },
  pos: { label: "نقطة بيع", icon: "🖨️" },
};

export default function TreasuryPage() {
  const [vaults, setVaults] = useState<VaultBalance[]>([]);
  const [loading, setLoading] = useState(true);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);
  const [showTransfer, setShowTransfer] = useState(false);
  const [transfer, setTransfer] = useState({
    fromVaultId: "",
    toVaultId: "",
    amount: 0,
    notes: "",
  });
  const [transferring, setTransferring] = useState(false);
  const [transferError, setTransferError] = useState<string | null>(null);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const res = await api.get<VaultBalance[]>("/treasury/vaults/balances");
      setVaults(res.data);
      setLastUpdated(new Date());
    } finally {
      setLoading(false);
    }
  }

  const handleTransfer = async () => {
    setTransferring(true);
    setTransferError(null);
    try {
      await api.post("/treasury/transfers", {
        fromVaultId: transfer.fromVaultId,
        toVaultId: transfer.toVaultId,
        amount: transfer.amount,
        notes: transfer.notes || null,
      });
      setShowTransfer(false);
      setTransfer({ fromVaultId: "", toVaultId: "", amount: 0, notes: "" });
      load();
    } catch (e: unknown) {
      const err = e as { response?: { data?: { message?: string; error?: string } } };
      setTransferError(err?.response?.data?.message ?? err?.response?.data?.error ?? "حدث خطأ");
    } finally {
      setTransferring(false);
    }
  };

  const total = vaults.reduce((sum, v) => sum + v.currentBalance, 0);

  const byType = vaults.reduce<Record<string, number>>((acc, v) => {
    acc[v.type] = (acc[v.type] ?? 0) + v.currentBalance;
    return acc;
  }, {});

  return (
    <div className="p-6" dir="rtl">
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">لوحة الخزينة</h1>
          {lastUpdated && (
            <p className="text-xs text-gray-400 mt-1">آخر تحديث: {lastUpdated.toLocaleTimeString("ar")}</p>
          )}
        </div>
        <div className="flex gap-3">
          <button
            onClick={() => { setShowTransfer(true); setTransferError(null); }}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
          >
            تحويل بين الخزائن
          </button>
          <button onClick={load} className="bg-white border border-gray-300 text-gray-600 px-4 py-2 rounded-lg hover:bg-gray-50 text-sm">
            تحديث
          </button>
        </div>
      </div>

      {showTransfer && (
        <div className="fixed inset-0 bg-black/40 flex items-center justify-center z-50">
          <div className="bg-white rounded-xl p-6 w-full max-w-md" dir="rtl">
            <h2 className="text-lg font-semibold mb-4">تحويل بين الخزائن</h2>
            {transferError && <div className="mb-3 text-sm text-red-600 bg-red-50 rounded-lg px-3 py-2">{transferError}</div>}
            <div className="space-y-3">
              <div>
                <label className="text-sm text-gray-600 block mb-1">من خزينة</label>
                <select
                  className="w-full border rounded-lg p-2 text-sm"
                  value={transfer.fromVaultId}
                  onChange={(e) => setTransfer({ ...transfer, fromVaultId: e.target.value })}
                >
                  <option value="">اختر الخزينة المصدر</option>
                  {vaults.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.name} ({v.currentBalance.toFixed(2)} د.ل)
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="text-sm text-gray-600 block mb-1">إلى خزينة</label>
                <select
                  className="w-full border rounded-lg p-2 text-sm"
                  value={transfer.toVaultId}
                  onChange={(e) => setTransfer({ ...transfer, toVaultId: e.target.value })}
                >
                  <option value="">اختر الخزينة الوجهة</option>
                  {vaults.filter((v) => v.id !== transfer.fromVaultId).map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.name}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="text-sm text-gray-600 block mb-1">المبلغ</label>
                <input
                  type="number"
                  min={1}
                  step="0.01"
                  className="w-full border rounded-lg p-2 text-sm"
                  value={transfer.amount || ""}
                  onChange={(e) => setTransfer({ ...transfer, amount: Number(e.target.value) })}
                />
              </div>
              <textarea
                placeholder="ملاحظات (اختياري)"
                rows={2}
                className="w-full border rounded-lg p-2 text-sm"
                value={transfer.notes}
                onChange={(e) => setTransfer({ ...transfer, notes: e.target.value })}
              />
            </div>
            <div className="flex gap-3 mt-5">
              <button
                onClick={handleTransfer}
                disabled={transferring || !transfer.fromVaultId || !transfer.toVaultId || transfer.amount <= 0}
                className="flex-1 bg-blue-600 text-white py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50"
              >
                {transferring ? "جاري التحويل..." : "تأكيد التحويل"}
              </button>
              <button onClick={() => setShowTransfer(false)} className="flex-1 border py-2 rounded-lg text-sm">
                إلغاء
              </button>
            </div>
          </div>
        </div>
      )}

      {loading ? (
        <div className="text-center py-12 text-gray-500">جاري التحميل...</div>
      ) : (
        <>
          <div className="bg-gradient-to-l from-blue-600 to-blue-700 rounded-2xl p-6 mb-6 text-white">
            <div className="text-sm opacity-80 mb-1">إجمالي الرصيد</div>
            <div className="text-4xl font-bold">{total.toFixed(2)} <span className="text-xl font-normal opacity-80">د.ل</span></div>
            <div className="mt-3 text-sm opacity-70">{vaults.length} خزائن</div>
          </div>

          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
            {Object.entries(byType).map(([type, balance]) => {
              const t = typeLabel[type] ?? { label: type, icon: "💰" };
              return (
                <div key={type} className="bg-white rounded-xl shadow p-4">
                  <div className="text-2xl mb-2">{t.icon}</div>
                  <div className="text-xs text-gray-500 mb-1">{t.label}</div>
                  <div className={`text-lg font-bold ${balance >= 0 ? "text-gray-800" : "text-red-600"}`}>
                    {balance.toFixed(2)}
                  </div>
                </div>
              );
            })}
          </div>

          <div className="bg-white rounded-xl shadow overflow-hidden">
            <div className="px-5 py-4 border-b">
              <h2 className="font-semibold text-gray-700">تفاصيل الخزائن</h2>
            </div>
            <div className="divide-y">
              {vaults.map((v) => {
                const t = typeLabel[v.type] ?? { label: v.type, icon: "💰" };
                const pct = total !== 0 ? Math.abs(v.currentBalance / total) * 100 : 0;
                return (
                  <div key={v.id} className="px-5 py-4">
                    <div className="flex items-center justify-between mb-2">
                      <div className="flex items-center gap-2">
                        <span className="text-lg">{t.icon}</span>
                        <div>
                          <div className="text-sm font-medium text-gray-800">{v.name}</div>
                          <div className="text-xs text-gray-400">{t.label}</div>
                        </div>
                      </div>
                      <div className={`text-base font-bold ${v.currentBalance >= 0 ? "text-gray-800" : "text-red-600"}`}>
                        {v.currentBalance.toFixed(2)} د.ل
                      </div>
                    </div>
                    <div className="grid grid-cols-3 gap-2 text-xs text-gray-400 mt-1 mb-2">
                      <span>افتتاحي: {v.openingBalance.toFixed(2)}</span>
                      <span className="text-green-600">وارد: +{v.totalIn.toFixed(2)}</span>
                      <span className="text-red-500">صادر: -{v.totalOut.toFixed(2)}</span>
                    </div>
                    <div className="w-full bg-gray-100 rounded-full h-1.5">
                      <div className="bg-blue-500 h-1.5 rounded-full" style={{ width: `${Math.min(100, pct)}%` }} />
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </>
      )}
    </div>
  );
}
