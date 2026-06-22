"use client";
import { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function PurchaseOrdersRedirect() {
  const router = useRouter();
  useEffect(() => { router.replace("/purchasing/invoices"); }, []);
  return <div className="p-6 text-center text-gray-400">جاري إعادة التوجيه...</div>;
}
