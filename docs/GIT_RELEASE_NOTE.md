# GIT_RELEASE_NOTE — phase-1-approved

> **التاريخ:** 2026-06-17 | **Tag:** `phase-1-approved` | **Branch:** master

---

## معلومات الإصدار

| الحقل | القيمة |
|-------|--------|
| **Tag** | `phase-1-approved` |
| **Commit Hash** | `e0775571d5a5bf84f7c3003c3ea77b593b08b7e6` |
| **Branch** | `master` |
| **التاريخ** | 2026-06-17 |
| **الحالة** | ✅ Phase 1 Approved |

---

## Commits المُضمَّنة

| Hash | الرسالة |
|------|---------|
| `e077557` | fix: add frontend source files as regular tracked files (not submodule) |
| `854ff40` | feat: Phase 1 — Auth, Users, Roles, Settings (ASP.NET Core 9 + Next.js 15) |

---

## ما يتضمنه هذا الإصدار

### Backend (ASP.NET Core 9 / .NET 9)

- **IAM Module** — المصادقة والتفويض الكامل
  - JWT Access Token (60 دقيقة) + Refresh Token Rotation (7 أيام)
  - BCrypt.Net-Next 4.0.3 لتشفير كلمات المرور
  - 19 Endpoint: Auth (4) + Users (6) + Roles (5) + Permissions (1) + Settings (2) + Health (1)
- **CQRS** — MediatR 12 مع Pipeline Behaviors (Logging + Validation + Authorization)
- **Clean Architecture** — Modular Monolith + Vertical Slice
- **EF Core 8 + PostgreSQL 16** — 7 جداول

### Database Migrations

| # | الملف | المحتوى |
|---|-------|---------|
| 001 | `001_initial_schema.sql` | 7 جداول: users, roles, permissions, role_permissions, user_roles, refresh_tokens, system_settings |
| 002 | `002_permissions_seed.sql` | 40 صلاحية، 4 أدوار، مستخدم admin، 8 إعدادات |
| 003 | `003_audit_logs.sql` | جدول audit_logs (JSONB + 3 indexes) |

### Frontend (Next.js 15 + TypeScript)

- RTL كامل (`<html lang="ar" dir="rtl">`)
- **IBM Plex Sans Arabic** — Google Font عبر next/font
- صفحة Login مع Zod validation
- Dashboard Shell + Sidebar مع Permission-based navigation
- PermissionGate component + usePermission hook
- Zustand authStore مع persist middleware

### Tests

| النوع | العدد | النتيجة |
|-------|-------|---------|
| Unit Tests | 27 | ✅ Passed |
| Integration Tests | 5 | ✅ Passed |
| **المجموع** | **32** | **✅ All Passed** |

### Documentation

- `FRAMEWORK_VERIFICATION.md` — التحقق من .NET 9 SDK
- `PERMISSIONS_MATRIX.md` — 40 صلاحية × 4 أدوار
- `AUDIT_LOG_DESIGN.md` — تصميم سجلات المراجعة
- `FRONTEND_FOUNDATION_REPORT.md` — RTL + Arabic + Sidebar + PermissionGate

---

## الإحصائيات

| الإحصائية | القيمة |
|------------|--------|
| Files Changed | 144 |
| Lines of Code | +22,536 |
| Backend Files | ~90 |
| Frontend Files | ~30 |
| Docs Files | ~15 |

---

## ما هو مُستبعَد (Phase 2+)

- ❌ Patients Module
- ❌ Appointments Module
- ❌ Reception/Queue Module
- ❌ Treasury Module
- ❌ Inventory / Lab / Radiology

---

## الخطوة التالية

**Phase 2: Patients + Reception + Appointments**

- Database: patients, appointments, queue_entries tables
- Backend APIs: CRUD + Queue Management + Calendar
- Frontend: S03 Register Patient, S04 Calendar, S05 Book Appointment, S06 Queue Display

---

**الحالة: ✅ Phase 1 مكتملة ومعتمدة — جاهز لـ Phase 2**
