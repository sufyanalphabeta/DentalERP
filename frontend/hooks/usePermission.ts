import { useAuthStore } from "@/stores/authStore";

export function usePermission(permission: string): boolean {
  return useAuthStore((s) => s.hasPermission(permission));
}

export function usePermissions(permissions: string[]): Record<string, boolean> {
  const hasPermission = useAuthStore((s) => s.hasPermission);
  return Object.fromEntries(permissions.map((p) => [p, hasPermission(p)]));
}
