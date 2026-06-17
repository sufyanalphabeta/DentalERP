# Frontend Foundation Report — DentalERP

> **التاريخ:** 2026-06-17 | **Framework:** Next.js 15 + TypeScript + Tailwind CSS

---

## 1. RTL Support ✅

### التطبيق
- `<html lang="ar" dir="rtl">` مضبوط في [app/layout.tsx](../frontend/app/layout.tsx)
- Tailwind CSS يدعم RTL natively بدون إضافات
- `dir="rtl"` على كل الـ layouts الرئيسية

### التحقق
```html
<!-- app/layout.tsx -->
<html lang="ar" dir="rtl" ...>
  <body>...</body>
</html>
```

---

## 2. Arabic Language Rendering ✅

### الخط المستخدم
**IBM Plex Sans Arabic** — خط Google مُحمَّل عبر `next/font/google`:

```typescript
const ibmPlexArabic = IBM_Plex_Sans_Arabic({
  subsets: ["arabic"],
  weight: ["300", "400", "500", "600", "700"],
  variable: "--font-arabic",
  display: "swap",
  preload: true,
});
```

### CSS Variable
```css
--font-arabic: 'IBM Plex Sans Arabic'
```

### التطبيق
```html
<body style="font-family: var(--font-arabic), system-ui, sans-serif">
```

### مميزات IBM Plex Sans Arabic
- يدعم كل الأحرف العربية بما فيها الحركات
- أوزان متعددة: 300، 400، 500، 600، 700
- مناسب لواجهات الأنظمة (ليس للزخرفة)
- مرخّص مفتوح المصدر (Apache 2.0)
- يعمل offline (محمَّل مع Next.js)

---

## 3. Sidebar Navigation ✅

### الموقع
[frontend/components/shared/Sidebar.tsx](../frontend/components/shared/Sidebar.tsx)

### الميزات
| الميزة | التطبيق |
|--------|---------|
| RTL Layout | `dir="rtl"` على `<aside>` |
| Permission-based filtering | يعرض فقط روابط لها صلاحية |
| Active link highlighting | `pathname.startsWith(item.href)` |
| User info | اسم المستخدم في الأعلى |
| Logout button | `clearAuth()` + redirect |
| Responsive | flex layout يمتد بالكامل |

### قائمة التنقل (مرتّبة بالصلاحيات)

| الرابط | الصلاحية المطلوبة |
|--------|------------------|
| الرئيسية | بلا |
| المرضى | `Patients.View` |
| المواعيد | `Appointments.View` |
| الفواتير | `Treasury.View` |
| الخزينة | `Treasury.View` |
| المخزون | `Inventory.View` |
| المعمل | `Lab.View` |
| الأشعة | `Radiology.View` |
| التقارير | `Reports.View` |
| المستخدمون | `Users.View` |
| الإعدادات | `Settings.View` |

---

## 4. Permission Guards ✅

### PermissionGate Component
الموقع: [frontend/components/shared/PermissionGate.tsx](../frontend/components/shared/PermissionGate.tsx)

```tsx
<PermissionGate permission="Users.Create">
  <button>إضافة مستخدم</button>
</PermissionGate>
```

- إذا لم تكن الصلاحية موجودة: لا يُعرض المحتوى
- يدعم `fallback` اختياري

### usePermission Hook
```typescript
const canCreate = usePermission("Users.Create");
const { canView, canEdit } = usePermissions(["Users.View", "Users.Edit"]);
```

### AuthStore (Zustand)
- `hasPermission(permission)` — فحص فوري من الذاكرة
- الصلاحيات مُخزَّنة في JWT وفي Zustand store
- `persist` middleware — تبقى عند إعادة تحميل الصفحة

---

## 5. Layout Responsiveness

### DashboardLayout
```
┌─────────────────────────────────────┐
│ Sidebar (w-64)  │  Main Content     │
│ bg-gray-900     │  bg-gray-100 flex │
│ min-h-screen    │  overflow-auto    │
└─────────────────────────────────────┘
```

| الشاشة | السلوك |
|--------|--------|
| Desktop (≥768px) | Sidebar ثابت على اليسار + محتوى |
| Mobile (<768px) | يحتاج Drawer — مجدول لـ Phase 2 |

### Protected Routes
```tsx
// DashboardLayout
useEffect(() => {
  if (!user) router.replace("/login");
}, [user, router]);
```

---

## 6. Auth Flow

```
/login → LoginPage → POST /api/auth/login
       → useAuthStore.setAuth()
       → router.replace("/")
       → DashboardLayout (Protected)
```

---

## 7. API Integration

### Axios Instance
[frontend/lib/api.ts](../frontend/lib/api.ts)

- Base URL: `NEXT_PUBLIC_API_URL`
- Auto-attach Bearer token
- Auto-refresh token on 401
- Redirect to `/login` on refresh failure

---

## 8. ما يحتاج تطوير في Phases لاحقة

| المطلوب | Phase |
|---------|-------|
| Mobile Sidebar (Drawer) | Phase 2 |
| `<QueryProvider>` (React Query) | Phase 2 |
| Notification System (SignalR) | Phase 4 |
| Dark Mode | بعد MVP |
| PWA (offline fallback page) | بعد MVP |

---

**الحالة: ✅ الأساس مكتمل — RTL + Arabic Font + Sidebar + PermissionGate + AuthFlow**
