"use client";

import { useEffect, useState } from "react";
import { api } from "@/lib/api";

interface VaultBalance {
  id: string;
  name: string;
  type: string;
  balance: number;
  currency: string;
  isActive: boolean;
}

const typeLabel: Record<string, string> = {
  cash: "Ù†Ù‚Ø¯ÙŠ",
  bank: "Ø¨Ù†Ùƒ",
  card: "Ø¨Ø·Ø§Ù‚Ø©",
  pos: "Ù†Ù‚Ø·Ø© Ø¨ÙŠØ¹",
};

export default function VaultsPage() {
  const [vaults, setVaults] = useState<VaultBalance[]>([]);
  const [loading, setLoading] = useState(true);
  const [total, setTotal] = useState(0);

  useEffect(() => { load(); }, []);

  async function load() {
    setLoading(true);
    try {
      const res = await api.get<VaultBalance[]>("/api/treasury/vaults/balances");
      setVaults(res.data);
      setTotal(res.data.reduce((sum: number, v: VaultBalance) => sum + v.balance, 0));
    } finally {
      setLoading(false);
    }
  }

  const balanceColor = (b: number) => b >= 0 ? "text-green-700" : "text-red-600";

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Ø§Ù„Ø®Ø²Ø§Ø¦Ù†</h1>
        <button onClick={load} className="text-sm text-blue-600 hover:text-blue-800">
          ØªØ­Ø¯ÙŠØ«
        </button>
      </div>

      {loading ? (
        <div className="text-center py-12 text-gray-500">Ø¬Ø§Ø±ÙŠ Ø§Ù„ØªØ­Ù…ÙŠÙ„...</div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
            {vaults.map((v) => (
              <div key={v.id} className="bg-white rounded-xl shadow p-5">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-sm text-gray-500">{typeLabel[v.type] ?? v.type}</span>
                  <span className={`text-xs px-2 py-0.5 rounded-full ${v.isActive ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"}`}>
                    {v.isActive ? "Ù†Ø´Ø·" : "ØºÙŠØ± Ù†Ø´Ø·"}
                  </span>
                </div>
                <div className="text-base font-semibold text-gray-800 mb-1">{v.name}</div>
                <div className={`text-2xl font-bold ${balanceColor(v.balance)}`}>
                  {v.balance.toFixed(2)} <span className="text-sm font-normal">{v.currency}</span>
                </div>
              </div>
            ))}
          </div>

          <div className="bg-blue-50 border border-blue-200 rounded-xl p-5 flex items-center justify-between">
            <span className="text-base font-semibold text-blue-800">Ø¥Ø¬Ù…Ø§Ù„ÙŠ Ø§Ù„Ø±ØµÙŠØ¯</span>
            <span className={`text-2xl font-bold ${balanceColor(total)}`}>
              {total.toFixed(2)} Ø¯.Ù„
            </span>
          </div>

          {vaults.length === 0 && (
            <div className="text-center py-12 text-gray-400">Ù„Ø§ ØªÙˆØ¬Ø¯ Ø®Ø²Ø§Ø¦Ù† Ù…Ø¶Ø§ÙØ©</div>
          )}
        </>
      )}
    </div>
  );
}
