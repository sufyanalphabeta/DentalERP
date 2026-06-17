# 01 — System Architecture
# وثيقة البنية النظامية — DentalERP

> **الإصدار:** V2-Final | **التاريخ:** 2026-06-16 | **الحالة:** مرجع تنفيذي معتمد

---

## 1. نظرة عامة على النظام

**DentalERP** هو نظام إدارة عيادات أسنان متكامل (Dental Practice Management System) مدمج مع ERP طبي خفيف (Medical ERP Lite). يُغطي دورة العمل الكاملة: استقبال المريض → التشخيص → خطة العلاج → التنفيذ السريري → الفواتير → التحصيل → المخزون → التقارير.

### 1.1 نوع النشر — Single Tenant (مستأجر واحد)

| القرار | التفاصيل |
|--------|----------|
| النموذج | كل عميل = قاعدة بيانات مستقلة + سيرفر مستقل + نشر مستقل |
| لا يوجد | `tenant_id` في أي جدول — مخطط DB نظيف وبسيط |
| السيرفر | Mini-PC محلي داخل شبكة العيادة (LAN) |
| الوصول | محلي بالكامل، لا يحتاج إنترنت للعمل اليومي |
| الوصول الخارجي | اختياري عبر Cloudflare Tunnel (مجاني) |
| SaaS | لا — ليس SaaS ولا Multi-tenant |

---

## 2. المبادئ المعمارية السبعة (P1–P7)

### P1 — Single Tenant (مستأجر واحد)
لا يوجد `tenant_id` في أي جدول. كل نشر = عيادة واحدة = قاعدة بيانات واحدة نظيفة. الاستعلامات مباشرة وسريعة بلا تصفية إضافية. عند الحاجة لتوسيع لسلسلة عيادات: نشر متعدد مستقل لكل فرع.

### P2 — Modular Monolith (مونوليث معياري)
نشر واحد + قاعدة بيانات واحدة + وحدات مستقلة. الوحدات تتواصل عبر **Domain Events** (MediatR) وليس عبر HTTP مباشرة. حدود الوحدات صارمة في الكود لكن النشر مُوحَّد. يمكن فك الوحدات إلى Microservices مستقبلاً بدون إعادة كتابة.

### P3 — Offline-Resilient (متحمل للانقطاع)
**المعمارية:** السيرفر المحلي داخل المركز هو مصدر الحقيقة الوحيد. لا Client-Side DB، لا IndexedDB، لا Browser Sync.
**التشغيل الطبيعي:** كل أجهزة العيادة تتصل بالسيرفر المحلي عبر LAN — لا إنترنت مطلوب لأي وظيفة من وظائف النظام.
**عند انقطاع الإنترنت:** النظام يعمل بالكامل بدون أي تأثير — يشمل المرضى، المواعيد، الإجراءات، الخزينة، المعمل، الأشعة، المخزون، التقارير.
**الوصول الخارجي (اختياري):** عبر Cloudflare Tunnel أو Reverse Proxy دون التأثير على التشغيل المحلي.

### P4 — Immutable Financial Data (البيانات المالية غير قابلة للحذف)
لا يُحذف أي سجل مالي فيزيائياً. العمليات المسموحة: **Void** أو **Cancel** فقط. الإلغاء يُنشئ سجل معاكس (Reversal Entry). السجلات التاريخية محمية للأبد.

### P5 — Event-Driven Cross-Module Communication
الوحدات تتواصل عبر **MediatR Domain Events**. مثال: `PaymentReceivedEvent` → تحديث رصيد المريض + حساب عمولة الطبيب + تسجيل الخزينة. لا استدعاء مباشر بين وحدات مختلفة.

### P6 — Vertical Slice per Feature
كل ميزة = Command/Query + Handler + Validator في ملف واحد (Vertical Slice). لا تبعيات أفقية معقدة. كل Slice مستقلة تماماً. يسهل الاختبار والصيانة.

### P7 — Audit Everything (تسجيل كل شيء)
كل عملية Create/Update/Delete تُسجَّل في `audit_logs` مع:
- المستخدم (user_id + username)
- عنوان IP
- Timestamp (TIMESTAMPTZ)
- القيم القديمة (old_values JSONB)
- القيم الجديدة (new_values JSONB)
- Module + Entity + Action

---

## 3. البنية المعمارية — Modular Monolith + Vertical Slice + CQRS

```
┌─────────────────────────────────────────────────────────────────┐
│                        Next.js 15 Frontend                      │
│              (App Router + TanStack Query + SignalR)             │
└───────────────────────────────┬─────────────────────────────────┘
                                │ HTTPS / REST + WebSocket
┌───────────────────────────────▼─────────────────────────────────┐
│                   ASP.NET Core 8 Web API                        │
│  ┌──────────────┐  ┌──────────────┐  ┌────────────────────────┐ │
│  │   MediatR    │  │ FluentValid. │  │  ASP.NET Core Identity │ │
│  │ Pipeline     │  │ Behaviors    │  │  JWT RS256             │ │
│  └──────┬───────┘  └──────────────┘  └────────────────────────┘ │
│         │                                                        │
│  ┌──────▼──────────────────────────────────────────────────┐    │
│  │                    Modules (Vertical Slices)             │    │
│  │  ┌──────┐ ┌────────┐ ┌─────────┐ ┌──────────────────┐  │    │
│  │  │ IAM  │ │Clinic  │ │ Patient │ │   Scheduling     │  │    │
│  │  └──────┘ └────────┘ └─────────┘ └──────────────────┘  │    │
│  │  ┌──────────┐ ┌─────────┐ ┌─────────┐ ┌────────────┐  │    │
│  │  │ Clinical │ │Treasury │ │Inventory│ │ Purchasing  │  │    │
│  │  └──────────┘ └─────────┘ └─────────┘ └────────────┘  │    │
│  │  ┌────────────┐ ┌──────────┐ ┌─────────┐ ┌──────────┐ │    │
│  │  │ Laboratory │ │Radiology │ │Expenses │ │Reporting │ │    │
│  │  └────────────┘ └──────────┘ └─────────┘ └──────────┘ │    │
│  └─────────────────────────────────────────────────────────┘    │
│         │ Domain Events (MediatR INotification)                  │
└─────────┼───────────────────────────────────────────────────────┘
          │
┌─────────▼───────────────────────────────────────────────────────┐
│                     Infrastructure Layer                         │
│  ┌─────────────┐  ┌──────────┐  ┌──────────┐  ┌─────────────┐  │
│  │ PostgreSQL  │  │  Redis   │  │ Hangfire │  │   MinIO     │  │
│  │    16       │  │   7      │  │  Worker  │  │  Storage    │  │
│  └─────────────┘  └──────────┘  └──────────┘  └─────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### 3.1 نمط CQRS مع MediatR

```
HTTP Request
    │
    ▼
Controller (thin) ──→ sends Command/Query via MediatR
    │
    ▼
Pipeline Behaviors (ordered):
    1. LoggingBehavior         ← يسجل الطلب
    2. ValidationBehavior      ← FluentValidation
    3. AuthorizationBehavior   ← Permission check
    4. TransactionBehavior     ← DB Transaction
    │
    ▼
Handler (Feature Logic)
    │
    ├──→ DbContext (EF Core 8)
    ├──→ Publish Domain Events
    └──→ Returns Result<T>
```

### 3.2 Vertical Slice — هيكل الملف

كل Feature Slice يحتوي على الملف الواحد أو المجلد:

```
Features/
└── Patients/
    ├── CreatePatient/
    │   ├── CreatePatientCommand.cs       ← Command + Response DTO
    │   ├── CreatePatientCommandHandler.cs ← Handler Logic
    │   ├── CreatePatientValidator.cs     ← FluentValidation Rules
    │   └── CreatePatientEndpoint.cs      ← Minimal API Endpoint
    ├── GetPatient/
    │   ├── GetPatientQuery.cs
    │   ├── GetPatientQueryHandler.cs
    │   └── GetPatientEndpoint.cs
    └── PatientEvents/
        ├── PatientCreatedEvent.cs
        └── PatientCreatedEventHandler.cs
```

---

## 4. هيكل الحل (Solution Structure)

```
DentalERP.sln
├── src/
│   ├── DentalERP.Host/                         ← Entry Point (Program.cs + Startup)
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Production.json
│   │   └── Dockerfile
│   │
│   ├── DentalERP.SharedKernel/                 ← Shared Abstractions
│   │   ├── Abstractions/
│   │   │   ├── BaseEntity.cs                   ← id, created_at, updated_at, deleted_at
│   │   │   ├── IAggregateRoot.cs
│   │   │   └── IDomainEvent.cs
│   │   ├── Behaviors/
│   │   │   ├── LoggingBehavior.cs
│   │   │   ├── ValidationBehavior.cs
│   │   │   ├── AuthorizationBehavior.cs
│   │   │   └── TransactionBehavior.cs
│   │   ├── Results/
│   │   │   └── Result.cs                       ← Result<T> pattern
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   ├── DentalERP.Modules.IAM/                  ← Identity & Access Management
│   ├── DentalERP.Modules.Clinic/               ← Clinic Settings, Doctors, Specialties
│   ├── DentalERP.Modules.Patient/              ← Patients, Medical History, Files
│   ├── DentalERP.Modules.Scheduling/           ← Appointments, Queue, Calendar
│   ├── DentalERP.Modules.Clinical/             ← Dental Chart, Plans, Procedures, Media
│   ├── DentalERP.Modules.Treasury/             ← Invoices, Payments, Installments, Insurance
│   ├── DentalERP.Modules.Inventory/            ← Materials, Stock, Movements, FEFO
│   ├── DentalERP.Modules.Purchasing/           ← POs, Supplier Accounts, GRNs
│   ├── DentalERP.Modules.Expenses/             ← Petty Cash, Expense Categories
│   ├── DentalERP.Modules.Laboratory/           ← Lab Orders, Technicians, Commissions
│   ├── DentalERP.Modules.Radiology/            ← Radiology Orders, Images, Technicians
│   └── DentalERP.Modules.Reporting/            ← Reports, Dashboards, PDF Generation
│
├── tests/
│   ├── DentalERP.UnitTests/
│   └── DentalERP.IntegrationTests/
│
└── docker/
    ├── docker-compose.yml
    ├── docker-compose.override.yml
    └── nginx/
        └── nginx.conf
```

### 4.1 هيكل كل Module (نمط موحد)

```
DentalERP.Modules.{Name}/
├── {Name}Module.cs                  ← Module registration (IModule interface)
├── Features/                        ← Vertical Slices (Commands + Queries)
│   ├── Create{Entity}/
│   ├── Update{Entity}/
│   ├── Delete{Entity}/
│   └── Get{Entity}/
├── Domain/                          ← Entities + Domain Events + Value Objects
│   ├── Entities/
│   ├── Events/
│   └── ValueObjects/
├── Infrastructure/                  ← EF Core Configurations + Repository implementations
│   ├── {Name}DbContext.cs           ← (shared AppDbContext via DentalERP.Host)
│   └── Configurations/
│       └── {Entity}Configuration.cs ← IEntityTypeConfiguration<T>
└── Contracts/                       ← DTOs يُشاركها modules أخرى
    └── {Name}Contracts.cs
```

---

## 5. الوحدات (Domain Modules) — كتالوج شامل

### وحدة 1 — IAM (Identity & Access Management)
**المسؤولية:** المستخدمون، الأدوار، الصلاحيات، تسجيل الدخول، الجلسات، سجل التدقيق.

| الكيانات الرئيسية | الوصف |
|-------------------|-------|
| `users` | بيانات المستخدم + كلمة المرور المُشفرة |
| `roles` | الأدوار (مدير، طبيب، موظف استقبال...) |
| `role_permissions` | الصلاحيات المُعيَّنة لكل دور |
| `refresh_tokens` | Refresh Token Rotation |
| `audit_logs` | كل العمليات مع IP + User + القيم |

**31 صلاحية** موزعة على 8 وحدات (تفاصيل في 03_BUSINESS_RULES_FINAL.md).

---

### وحدة 2 — Clinic (إعدادات العيادة)
**المسؤولية:** بيانات العيادة، الأطباء، التخصصات، الخدمات، الغرف، workflow_settings.

| الكيانات الرئيسية | الوصف |
|-------------------|-------|
| `clinic_settings` | اسم العيادة، شعار، عملة، ساعات العمل |
| `doctors` | بيانات الطبيب + نسبة عمولة افتراضية |
| `specialties` | التخصصات الطبية |
| `medical_services` | الخدمات + السعر + `has_inventory_tracking` |
| `service_categories` | تصنيفات الخدمات |
| `rooms` | غرف العلاج |
| `workflow_settings` | إعدادات سير موافقة العمل (اختياري، default=OFF) |
| `doctor_service_commissions` | تجاوز عمولة طبيب × خدمة محددة |

---

### وحدة 3 — Patient (المرضى)
**المسؤولية:** تسجيل المرضى، التاريخ الطبي، ملفات المريض، جهة الاتصال.

| الكيانات الرئيسية | الوصف |
|-------------------|-------|
| `patients` | البيانات الأساسية + emergency_contact (JSONB) |
| `patient_medical_history` | الأمراض المزمنة، الأدوية، الحساسية |
| `patient_files` | مرفقات (صور، وثائق) |
| `patient_insurance` | تغطية التأمين للمريض |

---

### وحدة 4 — Scheduling (الجدولة والحجز)
**المسؤولية:** المواعيد، طابور الانتظار، التقويم، الإشعارات الفورية.

| الكيانات الرئيسية | الوصف |
|-------------------|-------|
| `appointments` | الموعد + الطبيب + المريض + الغرفة + الحالة |
| `appointment_statuses` | (مُحجوز، في الانتظار، جارٍ، مكتمل، ملغي) |
| `queue_entries` | الطابور الفعلي للانتظار (real-time) |
| `working_hours` | ساعات عمل الطبيب |
| `appointment_blocks` | حجب وقت معين |

**Real-time:** SignalR يُبثّ تحديثات الطابور لشاشة الاستقبال والأطباء.

---

### وحدة 5 — Clinical (الوحدة السريرية)
**المسؤولية:** خريطة الأسنان، خطط العلاج، الإجراءات، صور الأسنان، الوسائط.

| الكيانات الرئيسية | الوصف |
|-------------------|-------|
| `dental_charts` | خريطة سن لكل مريض (FDI notation, 32 سن) |
| `tooth_conditions` | حالة كل سن (سليم، تسوس، مُعالَج، مفقود...) |
| `treatment_plans` | خطة العلاج الكاملة |
| `treatment_plan_items` | خدمات ضمن الخطة (مرتبطة بسن + طبيب) |
| `procedures` | الإجراءات المُنفَّذة فعلياً |
| `procedure_materials` | المواد المستخدمة في كل إجراء |
| `clinical_media` | صور + ملاحظات سريرية |
| `approval_requests` | طلبات الموافقة (عندما workflow_settings=ON) |

**خريطة الأسنان:** مُكوَّن SVG مخصص، FDI Numbering، Adult/Pediatric toggle.

---

### وحدة 6 — Treasury (الخزينة والمالية)
**المسؤولية:** الفواتير، المدفوعات، الأقساط، حسابات الخزينة، التأمين، عمولات الأطباء.

| الكيانات الرئيسية | الوصف |
|-------------------|-------|
| `invoices` | الفاتورة الرئيسية (immutable بعد الإصدار) |
| `invoice_items` | بنود الفاتورة (الخدمات) |
| `payments` | المدفوعات + طريقة الدفع |
| `payment_methods` | (نقد، بطاقة، تحويل، شيك) |
| `installment_plans` | خطط تقسيط |
| `installment_payments` | دفعات الأقساط |
| `vault_accounts` | حسابات الخزينة (نقدية + بنكية) |
| `vault_transactions` | كل حركة خزينة |
| `vault_snapshots` | لقطة رصيد يومية (JSONB) |
| `insurance_companies` | شركات التأمين |
| `insurance_claims` | المطالبات + base_price_used snapshot |
| `doctor_commissions` | العمولات المحسوبة (Cash-Basis) |
| `patient_credit_notes` | رصيد آجل للمريض |

**مبدأ P4:** الفواتير والمدفوعات لا تُحذف — Void فقط مع سجل إلغاء.

---

### وحدة 7 — Inventory (المخزون)
**المسؤولية:** المواد، المخزون، الحركات، FEFO، ربط الإجراءات بالمواد.

| الكيانات الرئيسية | الوصف |
|-------------------|-------|
| `inventory_items` | المادة + الوحدة + الحد الأدنى |
| `inventory_batches` | دفعات المخزون + تاريخ انتهاء |
| `inventory_movements` | كل حركة (in/out/consumption/waste) |
| `service_default_materials` | المواد الافتراضية لكل خدمة (auto-deduction) |

**FEFO:** `ORDER BY expiry_date ASC NULLS LAST` — أقدم تاريخ انتهاء يُستهلك أولاً.
**Inventory Tracking اختياري:** `has_inventory_tracking BOOLEAN` على `medical_services` — default=false.

---

### وحدة 8 — Purchasing (المشتريات)
**المسؤولية:** أوامر الشراء، الموردون، حسابات الموردين، استلام البضائع.

| الكيانات الرئيسية | الوصف |
|-------------------|-------|
| `suppliers` | بيانات المورد |
| `purchase_orders` | أمر الشراء |
| `purchase_order_items` | بنود أمر الشراء |
| `goods_receipt_notes` | وصل استلام (GRN) |
| `supplier_invoices` | فاتورة المورد (accounts payable) |
| `supplier_payments` | مدفوعات للمورد |

**رصيد المورد:** View محسوبة (ليس حقل مخزَّن) = Σ(فواتير) - Σ(مدفوعات).

---

### وحدة 9 — Expenses (المصروفات)
**المسؤولية:** المصروفات النثرية، فئات المصروفات، ربط الخزينة.

| الكيانات الرئيسية | الوصف |
|-------------------|-------|
| `expense_categories` | تصنيفات المصروفات (كهرباء، إيجار...) |
| `expenses` | المصروف + المبلغ + الفئة + المسؤول |

**ملاحظة:** كل مصروف = سحب من vault_account (يُنشئ vault_transaction تلقائياً).

---

### وحدة 10 — Laboratory (المعمل)
**المسؤولية:** أوامر المعمل، فنيو المعمل (داخليون وخارجيون)، تتبع الإيرادات والمصروفات، عمولات الفنيين.

| الكيانات الرئيسية | الوصف |
|-------------------|-------|
| `lab_order_types` | أنواع أشغال المعمل (تركيبات، تقويم، أطقم...) |
| `lab_technicians` | فنيو المعمل + بيانات العمولة + الرصيد الافتتاحي |
| `lab_orders` | أوامر المعمل مرتبطة بالمريض والطبيب والإجراء |
| `lab_expense_categories` | تصنيفات مصروفات المعمل |
| `lab_expenses` | مصروفات المعمل التشغيلية |
| `lab_commission_records` | عمولات فنيي المعمل (Cash-Basis) |

**ملاحظات معمارية:**
- أوامر المعمل مرتبطة بـ `procedure_id` (اختياري — قد تكون مستقلة)
- مدفوعات المعمل تمر عبر `vault_transactions` (نوع: `payment_to_lab`)
- إيرادات المعمل (أشغال للخارج) تُسجَّل في `vault_transactions` (نوع: `lab_income`)
- عمولات الفنيين: Cash-Basis عند تحصيل الأمر

---

### وحدة 11 — Radiology (الأشعة)
**المسؤولية:** طلبات الأشعة (مرضى داخليون وخارجيون)، إدارة الصور (MinIO)، فنيو الأشعة، تتبع الإيرادات والمصروفات.

| الكيانات الرئيسية | الوصف |
|-------------------|-------|
| `radiology_types` | أنواع الأشعة (OPG, CBCT, Periapical X-Ray...) |
| `radiology_technicians` | فنيو الأشعة + بيانات العمولة |
| `radiology_orders` | طلبات الأشعة (internal/external patient) |
| `radiology_images` | مسارات الصور في MinIO |
| `radiology_expense_categories` | تصنيفات مصروفات الأشعة |
| `radiology_expenses` | مصروفات الأشعة التشغيلية |
| `radiology_commission_records` | عمولات فنيي الأشعة (Cash-Basis) |

**ملاحظات معمارية:**
- المريض الداخلي: مرتبط بـ `patient_id`
- المريض الخارجي: `external_patient_name` + `external_patient_phone` (لا حساب في النظام)
- الصور: تُخزَّن في MinIO، الجدول يحفظ URL فقط
- CONSTRAINT يضمن: internal → patient_id NOT NULL، external → external_patient_name NOT NULL
- إيرادات الأشعة من المرضى الخارجيين: `vault_transactions` (نوع: `radiology_income`)

---

### وحدة 12 — Reporting (التقارير)
**المسؤولية:** تقارير PDF، لوحة تحكم، مؤشرات أداء، تصدير Excel.

- **PDF:** FastReport.NET (ملفات .frx للقوالب)، دعم RTL عربي أصيل
- **Excel:** ClosedXML
- **تقارير صغيرة (< 5k صف):** sync (استجابة فورية)
- **تقارير كبيرة:** Hangfire background job + إشعار عند الاكتمال
- **تقارير المعمل والأشعة** مدمجة ضمن تقارير النظام

---

## 6. تدفق Domain Events بين الوحدات

```
┌─────────────┐     AppointmentCompletedEvent     ┌──────────────────┐
│  Scheduling │ ──────────────────────────────▶  │   Clinical       │
└─────────────┘                                   │ (فتح جلسة سريرية)│
                                                  └──────────────────┘

┌──────────────┐     ProcedureCompletedEvent       ┌──────────────────┐
│   Clinical   │ ──────────────────────────────▶  │   Treasury       │
└──────────────┘                                   │ (جاهز للفوترة)   │
                                                  └──────────────────┘

┌──────────────┐     ProcedureCompletedEvent       ┌──────────────────┐
│   Clinical   │ ──────────────────────────────▶  │   Inventory      │
└──────────────┘                                   │ (خصم المواد)     │
                                                  └──────────────────┘

┌──────────────┐     PaymentReceivedEvent          ┌──────────────────┐
│   Treasury   │ ──────────────────────────────▶  │   Clinic         │
└──────────────┘                                   │ (حساب العمولة)   │
                                                  └──────────────────┘

┌──────────────┐     PaymentReceivedEvent          ┌──────────────────┐
│   Treasury   │ ──────────────────────────────▶  │   Reporting      │
└──────────────┘                                   │ (تحديث Dashboard)│
                                                  └──────────────────┘

┌──────────────┐     GoodsReceivedEvent            ┌──────────────────┐
│  Purchasing  │ ──────────────────────────────▶  │   Inventory      │
└──────────────┘                                   │ (إضافة للمخزون)  │
                                                  └──────────────────┘

┌──────────────┐     LowStockEvent                 ┌──────────────────┐
│  Inventory   │ ──────────────────────────────▶  │   Reporting      │
└──────────────┘                                   │ (تنبيه مخزون)    │
                                                  └──────────────────┘

┌──────────────┐     LabOrderCompletedEvent        ┌──────────────────┐
│ Laboratory   │ ──────────────────────────────▶  │   Treasury       │
└──────────────┘                                   │ (تحديث تكلفة     │
                                                  │  الإجراء)        │
                                                  └──────────────────┘

┌──────────────┐     RadiologyOrderPaidEvent       ┌──────────────────┐
│  Radiology   │ ──────────────────────────────▶  │   Treasury       │
└──────────────┘                                   │ (تسجيل إيراد    │
                                                  │  أشعة خارجي)    │
                                                  └──────────────────┘
```

### 6.1 تنفيذ Domain Event (نمط موحد)

```csharp
// الحدث
public record PaymentReceivedEvent(
    Guid PaymentId,
    Guid InvoiceId,
    Guid DoctorId,
    decimal AmountPaid,
    DateTime PaidAt
) : IDomainEvent;

// المُعالج في Clinic Module
public class CalculateCommissionOnPaymentHandler
    : INotificationHandler<PaymentReceivedEvent>
{
    public async Task Handle(PaymentReceivedEvent notification,
        CancellationToken cancellationToken)
    {
        // حساب العمولة وحفظها
    }
}
```

---

## 7. بنية الأمان (Security Architecture)

```
┌──────────────────────────────────────────────────────────┐
│                    Security Layers                        │
│                                                          │
│  Layer 1: Transport ── HTTPS (TLS 1.2/1.3) via Nginx    │
│                                                          │
│  Layer 2: Authentication ──  JWT RS256                   │
│           • Access Token: 15 دقيقة                      │
│           • Refresh Token: 7 أيام (Rotation)             │
│           • Redis Blacklist للـ logout                   │
│                                                          │
│  Layer 3: Authorization ── Policy-Based RBAC             │
│           • 31 Permission Code                           │
│           • كل Permission = Policy في ASP.NET Core      │
│           • [Authorize(Policy = "patients.create")]      │
│                                                          │
│  Layer 4: Input Validation ── FluentValidation           │
│           • كل Command/Query لها Validator               │
│                                                          │
│  Layer 5: Audit ── AuditLog لكل CUD operation            │
│           • old_values + new_values (JSONB)              │
│           • IP + User + Timestamp                        │
│                                                          │
│  Layer 6: Rate Limiting ── تحديد محاولات الدخول          │
│           • 5 محاولات فاشلة → قفل الحساب مؤقتاً        │
└──────────────────────────────────────────────────────────┘
```

### 7.1 Auth Flow

```
Client ──POST /api/auth/login──▶ IAM Module
    ◀── AccessToken (15min) + RefreshToken (7d) ──

Client ──GET /api/patients [Authorization: Bearer {AT}]──▶
    │
    ├── JWT Validation (RS256 public key)
    ├── Redis Blacklist check (is token revoked?)
    ├── Permission Policy check ([Authorize(Policy="patients.read")])
    └── Handler executes

Client ──POST /api/auth/refresh──▶ IAM Module
    [body: { refreshToken }]
    ◀── New AccessToken + New RefreshToken (old RT invalidated)
```

---

## 8. بنية الـ Offline — Local Server Architecture

**المبدأ:** السيرفر المحلي داخل المركز الطبي هو مصدر الحقيقة الوحيد. لا يوجد Client-Side Database ولا Browser Sync.

### بنية الشبكة الداخلية

```
┌─────────────────────────────────────────────────────────────┐
│                   Medical Center (LAN)                       │
│                                                             │
│  [استقبال]  [طبيب 1]  [طبيب 2]  [خزينة]  [معمل]  [أشعة]  │
│       │          │          │         │        │        │   │
│       └──────────┴──────────┴─────────┴────────┴────────┘   │
│                            │                                 │
│                   [Switch / Router]                          │
│                            │                                 │
│         ┌──────────────────▼──────────────────┐             │
│         │           Local Server               │             │
│         │  ASP.NET Core + PostgreSQL           │             │
│         │  + Redis + MinIO                     │             │
│         └─────────────────────────────────────┘             │
└─────────────────────────────────────────────────────────────┘
```

### سيناريو انقطاع الإنترنت

```
Internet: ✗ DOWN
LAN:      ✓ UP

→ النظام يعمل بالكامل:
  ✓ المرضى والمواعيد
  ✓ الإجراءات السريرية
  ✓ الفواتير والتحصيل
  ✓ الخزينة والمصروفات
  ✓ المعمل والأشعة
  ✓ المخزون والمشتريات
  ✓ التقارير والإحصاءات
```

### الوصول الخارجي (اختياري)

```
[مدير/طبيب خارج المركز]
           │
           ▼ Internet
  [Cloudflare Tunnel]
    أو [Reverse Proxy]
           │
           ▼
  [Local Server داخل المركز]
```

Cloudflare Tunnel: مجاني، بدون port forwarding، بدون IP ثابت، TLS تلقائي.

---

## 9. هيكل الـ API

### 9.1 التجميع حسب Module

```
/api/v1/
├── auth/           ← IAM: login, refresh, logout
├── users/          ← IAM: إدارة المستخدمين
├── roles/          ← IAM: الأدوار والصلاحيات
├── doctors/        ← Clinic: الأطباء
├── services/       ← Clinic: الخدمات
├── patients/       ← Patient: المرضى
├── appointments/   ← Scheduling: المواعيد
├── queue/          ← Scheduling: الطابور (+ SignalR hub)
├── dental-chart/   ← Clinical: خريطة الأسنان
├── treatment-plans/← Clinical: خطط العلاج
├── procedures/     ← Clinical: الإجراءات
├── invoices/       ← Treasury: الفواتير
├── payments/       ← Treasury: المدفوعات
├── installments/   ← Treasury: الأقساط
├── vault/          ← Treasury: الخزينة
├── insurance/      ← Treasury: التأمين
├── inventory/      ← Inventory: المخزون
├── purchases/      ← Purchasing: المشتريات
├── suppliers/      ← Purchasing: الموردون
├── expenses/       ← Expenses: المصروفات
├── lab/            ← Laboratory: أوامر المعمل
├── lab-technicians/← Laboratory: فنيو المعمل
├── radiology/      ← Radiology: طلبات الأشعة
├── radiology-types/← Radiology: أنواع الأشعة
├── reports/        ← Reporting: التقارير
└── settings/       ← Clinic: الإعدادات
```

### 9.2 معيار الاستجابة الموحد

```json
{
  "success": true,
  "data": { ... },
  "error": null,
  "meta": {
    "page": 1,
    "pageSize": 20,
    "total": 150,
    "totalPages": 8
  }
}
```

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "PATIENT_NOT_FOUND",
    "message": "المريض غير موجود",
    "details": []
  }
}
```

---

## 10. قرارات البنية (Architecture Decision Records)

### ADR-001 — ASP.NET Core 8 بدلاً من NestJS
**السبب:** FastReport.NET هو المكتبة الوحيدة التي تدعم RTL عربي أصيلاً في PDF. FastReport.NET = .NET فقط.
**التبعات:** الفريق يتعلم C# (أبسط من TypeScript في Backend patterns) + أداء أعلى + ORM أقوى (EF Core).

### ADR-002 — Modular Monolith بدلاً من Microservices
**السبب:** العيادة تعمل على Mini-PC واحد. الموارد محدودة. Microservices overhead غير مبرر.
**التبعات:** تواصل عبر Domain Events داخل نفس العملية. أسرع، أبسط، أسهل للديباغ. قابل للتوسيع لاحقاً.

### ADR-003 — Vertical Slice بدلاً من Clean Architecture التقليدية
**السبب:** كل Feature مستقلة = أسهل للاختبار والصيانة. لا cascade changes عند تعديل Feature.
**التبعات:** بعض تكرار في الكود (مقبول ومتعمد). لا abstraction زائدة.

### ADR-004 — Single Tenant (لا Multi-tenant)
**السبب:** كل عيادة سيرفر مستقل. DB نظيف بلا tenant_id. أمان أعلى (بيانات عيادة لا تختلط).
**التبعات:** النشر لكل عميل مستقل. السعر محسوب على أساس per-deployment.

### ADR-005 — PostgreSQL بدلاً من SQL Server
**السبب:** مجاني، مفتوح المصدر، JSONB قوي، partitioning ممتاز للـ audit_logs، أداء ممتاز.
**التبعات:** استخدام `gen_random_uuid()` للـ UUID. `TIMESTAMPTZ` للتوقيتات. `JSONB` للبيانات المرنة.

### ADR-006 — Next.js 15 (App Router) بدلاً من React 18 SPA
**السبب:** Server Components للأداء، App Router للتنقل المتقدم، PWA متكامل، SEO (غير مهم هنا لكن البنية أقوى).
**التبعات:** `use client` directive للمكونات التفاعلية. TanStack Query للـ data fetching.

### ADR-007 — shadcn/ui + Tailwind CSS v4 بدلاً من MUI
**السبب:** دعم RTL أفضل (Tailwind logical properties). حجم bundle أصغر. تخصيص أسهل. المكونات في المشروع مباشرة.
**التبعات:** كل مكون shadcn/ui يُضاف لـ `src/components/ui/`. Tailwind RTL plugin مطلوب.

### ADR-008 — Approval Workflow اختياري (default=OFF)
**السبب:** معظم العيادات الصغيرة لا تحتاجه. إضافته كـ toggle يُبسط التنفيذ الأولي.
**التبعات:** `workflow_settings` table تتحكم في 3 أنواع: `procedure_edit`, `procedure_delete`, `invoice_cancel`.
عندما OFF: تنفيذ فوري + Audit Log. عندما ON: ينشئ `approval_request`.

---

## 11. الوحدات الاختيارية (V2)

| الوحدة | الوصف | يتطلب |
|--------|-------|--------|
| Laboratory | إدارة المختبرات الخارجية، الطلبات، التسليم | Treasury (للدفع) |
| Radiology | الصور الشعاعية، DICOM viewer بسيط | Patient (للربط) |
| Multi-Branch | إدارة فروع متعددة من لوحة تحكم مركزية | نشر مستقل لكل فرع |

---

## 12. تكامل Real-Time (SignalR)

**الـ Hub:** `/hubs/dental-erp`

| الحدث | المُرسِل | المُستقبِل | الاستخدام |
|-------|---------|-----------|----------|
| `QueueUpdated` | Scheduling | شاشة الاستقبال | تحديث الطابور |
| `PatientCalled` | موظف الاستقبال | شاشة انتظار المرضى | "يرجى التوجه للغرفة X" |
| `ProcedureReadyForBilling` | Clinical | Treasury | إجراء جاهز للفوترة |
| `NotificationReceived` | أي Module | المستخدم المحدد | إشعار عام |
| `ReportReady` | Reporting | المستخدم الطالب | التقرير الكبير جاهز |

---

## 13. مؤشرات الأداء المستهدفة

| المؤشر | الهدف |
|--------|-------|
| API Response (P95) | < 200ms |
| First Contentful Paint | < 1.5s (LAN) |
| Audit Log Write | Async (لا يُبطئ الطلب) |
| Report Generation (صغير) | < 3s |
| Report Generation (كبير) | Hangfire async + notify |
| DB Query (indexed) | < 50ms |
| Max Concurrent Users | 20 (عيادة واحدة) |

---

## ⚠️ نقاط تحتاج توضيح

1. **Multi-Branch في V1 أم V2؟** المستندات تذكر "V2 Optional" — هل يوجد طلب محدد من عيادة تريد إدارة فروع متعددة؟ يؤثر على نمط النشر.

2. **الوحدات الاختيارية (Laboratory / Radiology):** لم تُحدَّد متطلبات وظيفية تفصيلية. هل تريد إضافة screens للـ Lab في V2 أو تأجيلها؟

3. **PWA Scope:** ما العمليات المحددة التي يجب أن تعمل في المستوى الثاني (offline)؟ المستندات تذكر "عمليات محدودة" لكن لم تحددها بدقة. مقترح: عرض المرضى فقط + عرض الجدول.

4. **Cloudflare Tunnel vs VPN:** هل Cloudflare Tunnel كافٍ أم يحتاج بعض العملاء VPN مؤسسي؟ يؤثر على 08_DEPLOYMENT_ARCHITECTURE.

---

*التوثيق التقني التفصيلي (Tech Stack, Docker, Nginx, CI/CD) → [09_TECH_STACK.md](09_TECH_STACK.md) و [08_DEPLOYMENT_ARCHITECTURE.md](08_DEPLOYMENT_ARCHITECTURE.md)*
*قواعد العمل التفصيلية → [03_BUSINESS_RULES_FINAL.md](03_BUSINESS_RULES_FINAL.md)*
*مخطط قاعدة البيانات → [04_ERD_FINAL.md](04_ERD_FINAL.md)*
