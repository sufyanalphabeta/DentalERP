# 09 — Tech Stack
# المكدس التقني النهائي — DentalERP

> **الإصدار:** V-Final | **التاريخ:** 2026-06-16 | **الحالة:** معتمد ومغلق

---

## 1. القرارات المعتمدة (لا تناقش مجدداً)

| القرار | الاختيار | السبب الرئيسي |
|--------|---------|--------------|
| Backend Framework | ASP.NET Core 8 (.NET 8 LTS) | دعم LTS حتى 2026 + توافق EF Core |
| Frontend Framework | Next.js 15 (App Router) | SSR + Offline PWA + TypeScript Native |
| Database | PostgreSQL 16 | Partitioning + JSONB + UUID Native |
| ORM | Entity Framework Core 8 | LINQ + Migrations + Query Tracking |
| Architecture | Modular Monolith + Vertical Slice + CQRS | بساطة التشغيل + قابلية التوسع |
| Auth | ASP.NET Identity + JWT RS256 | معيار صناعي + صلاحيات دقيقة |
| UI Components | shadcn/ui + Tailwind CSS v4 | RTL كامل + قابلية التخصيص |
| PDF | FastReport.NET | دعم عربي RTL Native |

---

## 2. Backend

### الإطار والبنية

| المكوّن | الإصدار | الاستخدام |
|---------|---------|----------|
| **.NET (C#)** | .NET 8 LTS / C# 12 | Runtime الأساسي |
| **ASP.NET Core** | 8.0 | Web API + SignalR |
| **Architecture** | Modular Monolith + Vertical Slice + CQRS | — |
| **MediatR** | 12.x | CQRS Commands/Queries/Domain Events |
| **FluentValidation** | 11.x | Validation لكل Command/Query |

### قاعدة البيانات

| المكوّن | الإصدار | الاستخدام |
|---------|---------|----------|
| **PostgreSQL** | 16 | قاعدة البيانات الأساسية |
| **EF Core** | 8.0 | ORM + Migrations |
| **Npgsql** | 8.x | PostgreSQL Driver لـ .NET |
| **Redis** | 7.x | Caching + Session + SignalR Backplane |

### المصادقة والأمان

| المكوّن | الإصدار | الاستخدام |
|---------|---------|----------|
| **ASP.NET Core Identity** | 8.0 | User Management + Password Hashing |
| **JWT** | RS256 | Access Token (15 min) + Refresh Token (7 days) |
| **Microsoft.IdentityModel** | — | JWT Validation |

### الوظائف الإضافية

| المكوّن | الإصدار | الاستخدام |
|---------|---------|----------|
| **SignalR** | ASP.NET Core 8 Built-in | Real-time: Queue + Notifications + Approvals |
| **Hangfire** | 1.8.x | Background Jobs: Inventory Alerts + Audit Partitions |
| **Serilog** | 3.x | Structured Logging → PostgreSQL + File |
| **FastReport.NET** | Latest | PDF Reports (Arabic RTL Native) |
| **ClosedXML** | 0.102.x | Excel Export |
| **AutoMapper** | 13.x | Entity → DTO Mapping |
| **Minio .NET SDK** | 6.x | Object Storage Client → صور الأشعة |

### Middleware Pipeline (الترتيب)

```csharp
app.UseHealthChecks("/health");
app.UseHttpsRedirection();      // في الإنتاج
app.UseStaticFiles();           // Uploads
app.UseRouting();
app.UseCors();
app.UseAuthentication();        // JWT Validation
app.UseAuthorization();         // Permission Check
app.UseGlobalExceptionHandler();// Error Handler موحّد
app.MapControllers();
app.MapHubs();                  // SignalR
app.MapHangfireDashboard("/hangfire");
```

---

## 3. Frontend

### الإطار والبنية

| المكوّن | الإصدار | الاستخدام |
|---------|---------|----------|
| **Next.js** | 15 (App Router) | React Framework مع SSR |
| **TypeScript** | 5.x | Type Safety |
| **React** | 19 (مع Next.js 15) | UI Library |

### التصميم والمكوّنات

| المكوّن | الإصدار | الاستخدام |
|---------|---------|----------|
| **Tailwind CSS** | v4 | Utility-First CSS |
| **shadcn/ui** | Latest | Component Library (RTL كامل) |
| **Tailwind RTL Plugin** | — | `rtl:` variants + Logical Properties |
| **Radix UI** | — | Primitives تحت shadcn/ui |
| **Lucide React** | — | Icons |

### إدارة البيانات

| المكوّن | الإصدار | الاستخدام |
|---------|---------|----------|
| **TanStack Query** | v5 | Server State + Caching |
| **Zustand** | 4.x | Client State (Auth + UI State) |
| **React Hook Form** | 7.x | Form Management |
| **Zod** | 3.x | Schema Validation |

### الوظائف المتخصصة

| المكوّن | الإصدار | الاستخدام |
|---------|---------|----------|
| **@microsoft/signalr** | 8.x | SignalR Client (Real-time) |
| **FullCalendar** | 6.x | تقويم المواعيد (RTL + locale: ar) |
| **Recharts** | 2.x | Charts (RTL Compatible) |
| **next-intl** | 3.x | i18n (AR فقط) |

### Dental Chart

```
مكوّن SVG مخصص (لا Library خارجية)
- 32 سن دائم (FDI 11-48) + 20 سن لبني (51-85)
- كل سن: 5 مناطق قابلة للنقر (M/D/B/L/O)
- Legend ألوان + Panel تعديل جانبي
- Read-Only Mode بحدود برتقالية
- Pure SVG = لا dependencies إضافية
```

---

## 4. بنية الحل (.NET Solution)

```
DentalERP.sln
│
├── src/
│   ├── DentalERP.API/              ← ASP.NET Core API Project
│   │   ├── Controllers/
│   │   ├── Hubs/                   ← SignalR Hubs
│   │   ├── Middleware/
│   │   └── Program.cs
│   │
│   ├── DentalERP.Application/      ← CQRS: Commands, Queries, Handlers
│   │   ├── Patients/
│   │   │   ├── Commands/
│   │   │   │   ├── CreatePatientCommand.cs
│   │   │   │   └── CreatePatientCommandHandler.cs
│   │   │   └── Queries/
│   │   │       └── GetPatientByIdQuery.cs
│   │   ├── Clinical/
│   │   ├── Invoicing/
│   │   ├── Treasury/
│   │   ├── Insurance/
│   │   ├── Inventory/
│   │   ├── Purchasing/
│   │   ├── Laboratory/             ← ★ Core V1
│   │   │   ├── Commands/
│   │   │   │   ├── CreateLabOrderCommand.cs
│   │   │   │   └── UpdateLabOrderStatusCommand.cs
│   │   │   └── Queries/
│   │   │       └── GetLabTechnicianAccountQuery.cs
│   │   ├── Radiology/              ← ★ Core V1
│   │   │   ├── Commands/
│   │   │   │   ├── CreateRadiologyOrderCommand.cs
│   │   │   │   └── UploadRadiologyImagesCommand.cs
│   │   │   └── Queries/
│   │   │       └── GetRadiologyOrderQuery.cs
│   │   └── Common/
│   │       ├── Behaviors/          ← ValidationBehavior, AuditBehavior
│   │       └── Interfaces/
│   │           └── IFileStorageService.cs  ← MinIO abstraction
│   │
│   ├── DentalERP.Domain/           ← Entities + Domain Events + Enums
│   │   ├── Entities/
│   │   │   ├── Patient.cs
│   │   │   ├── Procedure.cs
│   │   │   └── ...
│   │   └── Events/
│   │       ├── PaymentReceivedEvent.cs
│   │       └── ProcedureConfirmedEvent.cs
│   │
│   ├── DentalERP.Infrastructure/   ← EF Core, Redis, MinIO, External Services
│   │   ├── Data/
│   │   │   ├── DentalDbContext.cs
│   │   │   ├── Configurations/     ← EntityTypeConfiguration لكل Entity
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   ├── Reports/                ← FastReport templates + ReportService
│   │   ├── Storage/                ← MinIO Integration
│   │   │   └── MinioFileStorageService.cs   ← IFileStorageService implementation
│   │   └── BackgroundJobs/         ← Hangfire Jobs
│   │
│   └── DentalERP.Shared/           ← DTOs + Constants + Extensions
│
└── tests/
    ├── DentalERP.UnitTests/
    └── DentalERP.IntegrationTests/
```

---

## 5. بنية المشروع — Frontend (Next.js)

```
frontend/
├── app/                            ← App Router (Next.js 15)
│   ├── (auth)/
│   │   └── login/
│   │       └── page.tsx
│   ├── (dashboard)/
│   │   ├── layout.tsx              ← App Shell (Sidebar + Topbar)
│   │   ├── page.tsx                ← S01 لوحة التحكم
│   │   ├── patients/
│   │   │   ├── page.tsx            ← S08 قائمة المرضى
│   │   │   ├── new/page.tsx        ← S03 تسجيل مريض
│   │   │   └── [id]/page.tsx       ← S09 ملف المريض
│   │   ├── clinical/
│   │   ├── invoices/
│   │   ├── treasury/
│   │   ├── inventory/
│   │   ├── laboratory/             ← ★ Core V1
│   │   │   ├── page.tsx            ← S-LAB-01 قائمة أوامر المعمل
│   │   │   ├── new/page.tsx        ← S-LAB-02 إنشاء أمر
│   │   │   └── technicians/page.tsx ← S-LAB-03 كشوف الحسابات
│   │   ├── radiology/              ← ★ Core V1
│   │   │   ├── page.tsx            ← S-RAD-01 قائمة طلبات الأشعة
│   │   │   ├── new/page.tsx        ← S-RAD-02 إنشاء طلب
│   │   │   └── [id]/page.tsx       ← S-RAD-03 عارض الطلب + الصور
│   │   ├── reports/
│   │   └── admin/
│   └── queue/
│       └── display/page.tsx        ← S06 شاشة الاستدعاء (بلا Layout)
│
├── components/
│   ├── ui/                         ← shadcn/ui components
│   ├── dental-chart/               ← SVG Component مخصص
│   │   ├── DentalChart.tsx
│   │   ├── ToothSVG.tsx
│   │   └── types.ts
│   ├── financial/                  ← FinancialAccountPage (reusable)
│   └── shared/                     ← RealtimeIndicator, PermissionGate, etc.
│
├── hooks/
│   ├── usePermission.ts
│   ├── useQueueHub.ts              ← SignalR Queue Hook
│   └── useNotificationsHub.ts     ← SignalR Notifications Hook
│
├── lib/
│   ├── api.ts                      ← Axios Instance + Interceptors
│   ├── auth.ts                     ← Token Management
│   └── signalr.ts                  ← SignalR Connection Factory
│
├── stores/
│   ├── authStore.ts                ← Zustand (user + permissions)
│   └── uiStore.ts                  ← Zustand (sidebar, modals)
│
└── public/
    └── fonts/                      ← IBM Plex Sans Arabic (Self-hosted)
```

---

## 6. Domain Events Flow

```
عند حدث معيّن → MediatR يُطلق DomainEvent → Handler يُنفَّذ

مثال: تأكيد إجراء طبي

ProcedureConfirmedEvent
  ├── InventoryDeductionHandler    ← يخصم المواد (FEFO)
  ├── TreatmentPlanUpdateHandler   ← يُحدَّث حالة البند
  └── NotifyTreasuryHandler        ← يُرسَل SignalR إلى Treasury

PaymentReceivedEvent
  ├── CommissionCalculationHandler ← يحتسب عمولة الطبيب
  ├── InvoiceStatusUpdateHandler   ← يُحدَّث حالة الفاتورة
  └── InsuranceClaimHandler        ← (إن وُجدت مطالبة)

ApprovalDecidedEvent
  ├── ExecutePendingActionHandler  ← ينفّذ الإجراء المؤجَّل (إن approved)
  └── NotifyRequesterHandler       ← يُرسَل SignalR للمقدِّم

LabOrderDeliveredEvent             ← ★ Lab Module
  └── LabCommissionCalculationHandler ← يحتسب عمولة الفني

RadiologyOrderCompletedEvent       ← ★ Radiology Module
  └── RadiologyCommissionCalculationHandler ← يحتسب عمولة الفني
```

---

## 7. RTL Implementation

```typescript
// app/layout.tsx
export default function RootLayout({ children }) {
  return (
    <html lang="ar" dir="rtl">
      <body className="font-arabic">
        {children}
      </body>
    </html>
  )
}
```

```css
/* globals.css */
@import "tailwindcss";

@layer base {
  :root {
    font-family: 'IBM Plex Sans Arabic', 'Noto Sans Arabic', system-ui, sans-serif;
  }
}
```

```typescript
// Tailwind CSS v4 — RTL مدمج مع Logical Properties
// ms- (margin-start) = margin-right في RTL
// ps- (padding-start) = padding-right في RTL
// start-0 = right:0 في RTL
// border-s = border-right في RTL

// مثال:
<div className="ms-4 ps-2 border-s">
  Sidebar Item
</div>
```

---

## 8. MinIO Integration — Object Storage

**الهدف:** تخزين صور الأشعة + ملفات المرضى على MinIO داخل السيرفر المحلي.

```csharp
// IFileStorageService.cs (في Application Layer)
public interface IFileStorageService
{
    Task<string> UploadAsync(string bucket, string key, Stream data, string contentType);
    Task<string> GetPresignedUrlAsync(string bucket, string key, int expirySeconds = 3600);
    Task DeleteAsync(string bucket, string key);
}

// MinioFileStorageService.cs (في Infrastructure/Storage)
public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minio;
    // ... implementation using Minio .NET SDK
}
```

**Buckets الرسمية:**
| Bucket | الاستخدام |
|--------|----------|
| `radiology` | صور الأشعة (OPG, CBCT, X-Ray) |
| `patient-docs` | مستندات المرضى (صور، تقارير) |

**هيكل المسارات:**
```
radiology/{year}/{month}/{orderId}/{fileName}
patient-docs/{patientId}/{year}/{fileName}
```

**ملاحظة أمنية:** الوصول للصور يمر عبر Backend API — لا URL مباشر للـ MinIO من Frontend إلا عبر Presigned URL محدود الوقت.

---

## 9. Security Architecture

### JWT Flow

```
Login → RS256 Key Sign → Access Token (15 min) + Refresh Token (7 days)
                                    │
              ┌─────────────────────┘
              ▼
         كل Request
              │
         Bearer Token
              │
    AuthorizationMiddleware
              │
    يتحقق من: exp + sig + permissions
              │
    إن صالح: ينفّذ Request
    إن منتهي: 401 → Client يُجدّد بـ Refresh Token
```

### Permission Verification

```csharp
// على كل Endpoint:
[Authorize]
[RequirePermission("Invoice.Cancel")]
public async Task<IActionResult> CancelInvoice(...)

// أو في Application Layer (Command Handler):
public class CancelInvoiceCommandHandler
{
    public async Task<Result> Handle(CancelInvoiceCommand request, ...)
    {
        await _authorizationService.RequireAsync(request.UserId, "Invoice.Cancel");
        // ...
    }
}
```

### Audit Log (تلقائي عبر MediatR Pipeline)

```csharp
public class AuditBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, ...)
    {
        var result = await next();
        await _auditService.LogAsync(new AuditEntry {
            Module = request.Module,
            Action = request.AuditAction,
            EntityId = request.EntityId,
            OldValues = request.OldValues,
            NewValues = request.NewValues,
            UserId = _currentUser.Id,
            IpAddress = _httpContext.Connection.RemoteIpAddress
        });
        return result;
    }
}
```

---

## 10. Performance Guidelines

| الممارسة | التطبيق |
|---------|---------|
| Pagination | كل List Endpoint يدعم `page + pageSize` |
| Database Indexes | موثّقة في `014_indexes.sql` |
| Computed Views | الأرصدة كـ PostgreSQL Views (لا JOIN تكرارية) |
| Redis Caching | إعدادات العيادة + Permissions (TTL: 5 دقائق) |
| N+1 Prevention | EF Core `.Include()` في كل Query متعددة العلاقات |
| Async/Await | كل العمليات I/O بـ Async |
| audit_logs Partitioning | PostgreSQL Range Partition شهرية |

---

## ⚠️ نقاط تحتاج توضيح

1. **ClosedXML vs EPPlus:** لإنشاء Excel — ClosedXML مُختار (GPL) vs EPPlus (Commercial License). هل ClosedXML كافٍ للمتطلبات؟

2. **FastReport.NET License:** يتطلب ترخيصاً تجارياً لكل خادم إنتاج. هل هذا مُدرج في تكاليف المشروع؟

3. **next-pwa Status:** الحزمة شبه متوقفة (غير maintained). هل نستخدم `serwist` (Fork نشط) بدلاً منها؟

4. **IBM Plex Sans Arabic:** الخط يحتاج Google Fonts أو تحميل ذاتي. هل نحمّله على الخادم المحلي (لضمان عمله Offline)؟

5. **React Photo View (Lightbox):** الحزمة المقترحة لعرض الصور — هل هي مناسبة أم نستخدم بديلاً مثل `yet-another-react-lightbox`؟
