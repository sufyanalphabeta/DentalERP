# Frontend Run Guide
**DentalERP — Phase 6**
**Last Updated:** 2026-06-18

---

## Quick Start

```powershell
cd frontend
npm install    # first time or after pulling changes
npm run dev    # development server with hot reload
```

Frontend available at: **`http://localhost:3000`**

---

## Versions

| Tool | Required | Tested |
|---|---|---|
| Node.js | ≥ 20 | 24.16.0 |
| npm | ≥ 10 | 11.13.0 |
| Next.js | 16.2.9 | 16.2.9 |
| React | 19.2.4 | 19.2.4 |

Check with: `node --version && npm --version`

---

## Environment Variables

File: `frontend/.env.local` (already created — not committed to git)

```env
# Local dev: backend runs on port 5000
NEXT_PUBLIC_API_URL=http://localhost:5000
```

> **Important:** If `NEXT_PUBLIC_API_URL` is not set, the frontend defaults to `http://localhost:8080` (Docker port). You must set this for `dotnet run` local development.

---

## Commands

```powershell
# Install dependencies
npm install

# Development server (hot reload, fast refresh)
npm run dev

# Production build (TypeScript check + optimization)
npm run build

# Start production server (after build)
npm start

# Lint
npm run lint
```

---

## Application Structure

```
frontend/
├── app/
│   ├── layout.tsx                    ← Root layout (fonts, globals)
│   ├── page.tsx                      ← Root redirect
│   ├── (auth)/
│   │   └── login/
│   │       ├── page.tsx              ← Login form (Arabic, RTL)
│   │       └── layout.tsx
│   └── (dashboard)/
│       ├── layout.tsx                ← Dashboard layout with Sidebar
│       ├── page.tsx                  ← Dashboard home
│       ├── patients/                 ← Patient list, detail, chart, media
│       ├── appointments/             ← Appointment list, procedures
│       ├── queue/                    ← Reception queue
│       ├── finance/                  ← Invoices, payments, treasury, insurance
│       ├── lab/                      ← Lab orders
│       ├── radiology/                ← Radiology orders
│       └── settings/                 ← Services, vaults, insurance
├── components/
│   └── shared/
│       ├── Sidebar.tsx               ← Navigation sidebar (RTL)
│       └── PermissionGate.tsx        ← Permission-based rendering
├── lib/
│   └── api.ts                        ← Axios instance + interceptors
├── stores/
│   └── authStore.ts                  ← Zustand auth state (persisted)
├── types/
│   ├── auth.ts                       ← Login types
│   └── patients.ts                   ← Patient, Appointment, Queue types
└── hooks/
    └── usePermission.ts              ← Permission check hook
```

---

## Authentication Flow

1. User visits any route → middleware checks `authStore` (Zustand + localStorage)
2. If no `accessToken` → redirect to `/login`
3. Login form POSTs to `POST /api/auth/login` via Axios
4. On success → stores `accessToken`, `refreshToken`, user info in Zustand + localStorage
5. Subsequent API calls → `Authorization: Bearer <token>` header auto-injected by interceptor
6. On 401 → interceptor attempts refresh via `POST /api/auth/refresh-token`
7. If refresh fails → clears auth → redirects to `/login`

---

## Sidebar Navigation

The sidebar (`components/shared/Sidebar.tsx`) renders links based on user permissions:

| Label | Route | Permission Required |
|---|---|---|
| الرئيسية | `/` | None |
| المرضى | `/patients` | `Patients.View` |
| المواعيد | `/scheduling/calendar` | `Appointments.View` |
| الفواتير | `/finance/invoices` | `Treasury.View` |
| الخزينة | `/finance/treasury` | `Treasury.View` |
| التقسيط | `/finance/installments` | `Treasury.View` |
| المخزون | `/inventory` | `Inventory.View` |
| طلبات المختبر | `/lab/orders` | `Lab.View` |
| طلبات الأشعة | `/radiology/orders` | `Radiology.View` |
| مطالبات التأمين | `/finance/insurance/claims` | `Treasury.View` |
| مستحقات التأمين | `/finance/insurance/receivables` | `Treasury.View` |
| التقارير | `/reports` | `Reports.View` |
| المستخدمون | `/admin/users` | `Users.View` |
| الخدمات | `/settings/services` | `Settings.View` |
| الخزائن | `/settings/vaults` | `Settings.View` |
| شركات التأمين | `/settings/insurance` | `Settings.View` |
| الإعدادات | `/admin/clinic-settings` | `Settings.View` |

The `admin` user has ALL permissions — all items visible.

---

## Default Credentials

| Role | Username | Password |
|---|---|---|
| Administrator | `admin` | `Admin@123` |
| Receptionist | `reception` | `Admin@123` |
| Doctor | `doctor` | `Admin@123` |
| Accountant | `accountant` | `Admin@123` |

---

## Access URL

After `npm run dev`, open:

**`http://localhost:3000`**

You will be redirected to `http://localhost:3000/login`.

---

## Build Output (Production)

```
Route (app)                         Size    First Load JS
┌ ○ /                               ---
├ ○ /login                          3.2 kB
├ ○ /patients                       4.1 kB
├ ƒ /patients/[id]                  12.3 kB
├ ƒ /patients/[id]/chart            8.5 kB
...
○  (Static)   prerendered as static content
ƒ  (Dynamic)  server-rendered on demand
```

All 27 routes compile successfully. TypeScript type checking passes with 0 errors.
