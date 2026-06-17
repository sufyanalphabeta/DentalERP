# 11 — Module Dependencies
# خريطة تبعيات الوحدات — DentalERP

> **الإصدار:** V-Final | **التاريخ:** 2026-06-17 | **الحالة:** مرجع تنفيذي معتمد

---

## 1. قائمة الوحدات الكاملة

| # | الوحدة | المشروع .NET | Phase |
|---|--------|-------------|-------|
| 1 | **IAM** — Identity & Access Management | `DentalERP.Modules.IAM` | Phase 1 |
| 2 | **Clinic** — إعدادات العيادة | `DentalERP.Modules.Clinic` | Phase 1 |
| 3 | **Patient** — المرضى | `DentalERP.Modules.Patient` | Phase 2 |
| 4 | **Scheduling** — المواعيد والطابور | `DentalERP.Modules.Scheduling` | Phase 2 |
| 5 | **Clinical** — الوحدة السريرية | `DentalERP.Modules.Clinical` | Phase 3 |
| 6 | **Treasury** — الخزينة والمالية | `DentalERP.Modules.Treasury` | Phase 4 |
| 7 | **Inventory** — المخزون | `DentalERP.Modules.Inventory` | Phase 5 |
| 8 | **Purchasing** — المشتريات | `DentalERP.Modules.Purchasing` | Phase 5 |
| 9 | **Expenses** — المصروفات | `DentalERP.Modules.Expenses` | Phase 4 |
| 10 | **Laboratory** — المعمل ★ Core V1 | `DentalERP.Modules.Laboratory` | Phase 6 |
| 11 | **Radiology** — الأشعة ★ Core V1 | `DentalERP.Modules.Radiology` | Phase 7 |
| 12 | **Reporting** — التقارير | `DentalERP.Modules.Reporting` | Phase 8 |

---

## 2. مخطط التبعيات الكلي

```
┌─────────────────────────────────────────────────────────────────────┐
│                    SHARED KERNEL (لا تبعيات)                         │
│  BaseEntity | Result<T> | IDomainEvent | Behaviors | ICurrentUser   │
└──────────────────────────────┬──────────────────────────────────────┘
                               │ يعتمد عليها الجميع
                               ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      LAYER 0: Foundation                             │
│  ┌─────────────────┐  ┌──────────────────────────────────────────┐  │
│  │   IAM Module    │  │           Clinic Module                  │  │
│  │ users, roles,   │  │  doctors, services, locations, settings  │  │
│  │ permissions,    │  │  workflow_settings, specialties          │  │
│  │ audit_logs      │  │                                          │  │
│  └────────┬────────┘  └──────────────────────┬───────────────────┘  │
└───────────┼──────────────────────────────────┼─────────────────────┘
            │ يعتمد عليها                       │
            ▼                                   ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      LAYER 1: Core Domain                            │
│  ┌──────────────┐   ┌──────────────┐                               │
│  │   Patient    │   │  Scheduling  │                               │
│  │  (المرضى)    │   │  (المواعيد)  │                               │
│  └──────┬───────┘   └──────┬───────┘                               │
└─────────┼──────────────────┼───────────────────────────────────────┘
          │                  │
          ▼                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      LAYER 2: Clinical                               │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                     Clinical Module                           │   │
│  │         dental_chart, treatment_plans, procedures            │   │
│  │              approval_requests, patient_documents            │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
          │ Domain Events
          ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      LAYER 3: Financial + Ops                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────┐  │
│  │   Treasury   │  │  Inventory   │  │       Purchasing         │  │
│  │  invoices,   │  │  stock_items │  │  suppliers, POs,         │  │
│  │  payments,   │  │  batches,    │  │  purchase_invoices       │  │
│  │  commissions │  │  movements   │  │                          │  │
│  └──────┬───────┘  └──────┬───────┘  └──────────────────────────┘  │
│         │                 │                                          │
│  ┌──────▼───────┐         │                                          │
│  │   Expenses   │         └── يُغذّي stock_batches                  │
│  └──────────────┘                                                    │
└─────────────────────────────────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      LAYER 4: Auxiliary Services                     │
│  ┌──────────────────────┐    ┌──────────────────────────────────┐   │
│  │    Laboratory         │    │          Radiology               │   │
│  │  lab_orders           │    │  radiology_orders                │   │
│  │  lab_technicians      │    │  radiology_images (MinIO)        │   │
│  │  lab_commissions      │    │  radiology_technicians           │   │
│  │  lab_expenses         │    │  radiology_commissions           │   │
│  └──────────────────────┘    └──────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
          │ يقرأ من كل الطبقات
          ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      LAYER 5: Reporting (Read-Only)                  │
│  ┌──────────────────────────────────────────────────────────────┐   │
│  │                    Reporting Module                           │   │
│  │     FastReport.NET | ClosedXML | Dashboard Endpoints         │   │
│  │     يقرأ من كل الوحدات — لا يكتب إلى أي منها               │   │
│  └──────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 3. تبعيات كل وحدة (المدخلات والمخرجات)

### IAM Module

| النوع | الوصف |
|-------|-------|
| **يعتمد على** | لا شيء (Foundation) |
| **تعتمد عليه** | جميع الوحدات (Authentication + Authorization) |
| **Domain Events المُصدَرة** | — |
| **Domain Events المُستقبَلة** | — |
| **جداول مشتركة** | `users`, `roles`, `permissions` ← تقرأها كل الوحدات |

---

### Clinic Module

| النوع | الوصف |
|-------|-------|
| **يعتمد على** | IAM (users) |
| **تعتمد عليه** | Clinical, Scheduling, Treasury, Laboratory, Radiology |
| **Domain Events المُصدَرة** | — |
| **Domain Events المُستقبَلة** | `PaymentReceivedEvent` → `CalculateCommissionHandler` |
| **جداول مشتركة** | `doctors`, `medical_services`, `treatment_locations`, `vaults`, `workflow_settings` |

---

### Patient Module

| النوع | الوصف |
|-------|-------|
| **يعتمد على** | IAM, Clinic |
| **تعتمد عليه** | Scheduling, Clinical, Treasury, Laboratory, Radiology |
| **Domain Events المُصدَرة** | `PatientCreatedEvent` |
| **Domain Events المُستقبَلة** | — |
| **جداول مشتركة** | `patients`, `patient_medical_history`, `patient_documents` |

---

### Scheduling Module

| النوع | الوصف |
|-------|-------|
| **يعتمد على** | IAM, Clinic, Patient |
| **تعتمد عليه** | Clinical (فتح الجلسة السريرية) |
| **Domain Events المُصدَرة** | `AppointmentCompletedEvent` |
| **Domain Events المُستقبَلة** | — |
| **Real-time** | SignalR `QueueUpdated` → كل الأجهزة المتصلة |

---

### Clinical Module

| النوع | الوصف |
|-------|-------|
| **يعتمد على** | IAM, Clinic, Patient, Scheduling |
| **تعتمد عليه** | Treasury, Inventory, Laboratory, Radiology |
| **Domain Events المُصدَرة** | `ProcedureConfirmedEvent`, `ApprovalRequestedEvent` |
| **Domain Events المُستقبَلة** | `AppointmentCompletedEvent` |
| **قاعدة Workflow** | يفحص `workflow_settings` قبل كل Edit/Delete → إما فوري أو Pending |

---

### Treasury Module

| النوع | الوصف |
|-------|-------|
| **يعتمد على** | IAM, Clinic, Patient, Clinical |
| **تعتمد عليه** | Laboratory, Radiology (vault_transactions) |
| **Domain Events المُصدَرة** | `PaymentReceivedEvent` |
| **Domain Events المُستقبَلة** | `ProcedureConfirmedEvent` → جاهز للفوترة |
| **Immutability** | invoices, payments, vault_transactions = لا Physical Delete |

---

### Inventory Module

| النوع | الوصف |
|-------|-------|
| **يعتمد على** | IAM, Clinic |
| **تعتمد عليه** | Purchasing (تحديث stock_batches) |
| **Domain Events المُصدَرة** | `LowStockEvent` |
| **Domain Events المُستقبَلة** | `ProcedureConfirmedEvent` → خصم FEFO |
| **قاعدة FEFO** | `ORDER BY expiry_date ASC NULLS LAST WHERE current_quantity > 0` |

---

### Purchasing Module

| النوع | الوصف |
|-------|-------|
| **يعتمد على** | IAM, Inventory |
| **تعتمد عليه** | — |
| **Domain Events المُصدَرة** | `GoodsReceivedEvent` |
| **Domain Events المُستقبَلة** | — |
| **الرصيد** | `supplier_account_summary` View — لا حقل مخزَّن |

---

### Expenses Module

| النوع | الوصف |
|-------|-------|
| **يعتمد على** | IAM, Treasury (vault_transactions) |
| **تعتمد عليه** | — |
| **Domain Events** | — |

---

### Laboratory Module ★ Core V1

| النوع | الوصف |
|-------|-------|
| **يعتمد على** | IAM, Clinic (doctors), Patient, Clinical (procedures) |
| **يكتب إلى** | `vault_transactions` (نوع: payment_to_lab, lab_income) |
| **تعتمد عليه** | Reporting |
| **Domain Events المُصدَرة** | `LabOrderDeliveredEvent` |
| **Domain Events المُستقبَلة** | — |
| **العمولة** | Cash-Basis عند `status = delivered` |
| **الرصيد** | `lab_technician_account_summary` View — لا حقل مخزَّن |

**تدفق lab_order:**
```
إنشاء أمر → pending
الفني يستلم → in_progress
الفني ينهي → completed
الطبيب يستلم → delivered → LabOrderDeliveredEvent → Commission Calculation
```

---

### Radiology Module ★ Core V1

| النوع | الوصف |
|-------|-------|
| **يعتمد على** | IAM, Clinic (doctors), Patient, Clinical (procedures) |
| **يكتب إلى** | `vault_transactions` (نوع: radiology_income), MinIO (صور) |
| **تعتمد عليه** | Reporting |
| **Domain Events المُصدَرة** | `RadiologyOrderCompletedEvent` |
| **Domain Events المُستقبَلة** | — |
| **العمولة** | Cash-Basis عند `status = completed` |
| **MinIO** | صور الأشعة في bucket: `radiology/{year}/{month}/{orderId}/` |

**تدفق radiology_order:**
```
إنشاء طلب → pending
الفني يبدأ الفحص → in_progress
الفني ينتهي + يرفع الصور → completed
  → RadiologyOrderCompletedEvent → Commission Calculation
  → (external patient) → vault_transaction: radiology_income
```

---

### Reporting Module

| النوع | الوصف |
|-------|-------|
| **يعتمد على** | كل الوحدات السابقة (Read-Only) |
| **تعتمد عليه** | — (طرف النهاية) |
| **Domain Events** | — (يقرأ فقط) |
| **التقارير** | مالية + مرضى + مخزون + معمل + أشعة + إدارية |

---

## 4. Domain Events Registry (سجل شامل)

| الحدث | المُطلِق | المُستقبِل | الأثر |
|-------|---------|-----------|-------|
| `AppointmentCompletedEvent` | Scheduling | Clinical | فتح جلسة سريرية |
| `ProcedureConfirmedEvent` | Clinical | Inventory | خصم مواد FEFO |
| `ProcedureConfirmedEvent` | Clinical | Treasury | الإجراء جاهز للفوترة |
| `PaymentReceivedEvent` | Treasury | Clinic | حساب عمولة الطبيب |
| `PaymentReceivedEvent` | Treasury | Reporting | تحديث Dashboard |
| `GoodsReceivedEvent` | Purchasing | Inventory | إضافة للمخزون |
| `LowStockEvent` | Inventory | Reporting | SignalR تنبيه مخزون |
| `ApprovalRequestedEvent` | Clinical | IAM | SignalR `ApprovalRequest` |
| `ApprovalDecidedEvent` | IAM | Clinical | تنفيذ الإجراء المؤجَّل |
| `LabOrderDeliveredEvent` | Laboratory | Laboratory | حساب عمولة فني المعمل |
| `RadiologyOrderCompletedEvent` | Radiology | Radiology | حساب عمولة فني الأشعة |

---

## 5. الجداول المشتركة (Cross-Module Tables)

جداول يقرأها أو يكتب إليها أكثر من وحدة:

| الجدول | الوحدة المالكة | الوحدات القارئة |
|--------|--------------|----------------|
| `users` | IAM | جميع الوحدات |
| `doctors` | Clinic | Clinical, Treasury, Laboratory, Radiology, Reporting |
| `patients` | Patient | Scheduling, Clinical, Treasury, Laboratory, Radiology |
| `procedures` | Clinical | Treasury, Inventory, Laboratory, Radiology |
| `vault_transactions` | Treasury | Laboratory (كتابة), Radiology (كتابة), Expenses (كتابة) |
| `medical_services` | Clinic | Clinical, Inventory, Treasury |
| `treatment_locations` | Clinic | Clinical, Scheduling, Reporting |
| `vaults` | Treasury | Expenses, Laboratory, Radiology |
| `cost_centers` | Clinic | Treasury, Expenses, Reporting |
| `audit_logs` | IAM | الكتابة من كل الوحدات عبر AuditBehavior |

---

## 6. قواعد الاستقلالية بين الوحدات

```
✅ مسموح: Module A → يُطلق Domain Event → Module B يعالجه
✅ مسموح: Module A → يقرأ View/Table مملوكة لـ Module B
✅ مسموح: Module A → يكتب في Shared Table (vault_transactions, audit_logs)

❌ ممنوع: Module A → يستدعي Service/Repository مملوكة لـ Module B مباشرة
❌ ممنوع: Module A → يُعدّل Entity جوهرية لـ Module B
❌ ممنوع: Cross-Module Transaction بدون Domain Event
```

**استثناء:** `vault_transactions` = جدول مشترك مقصود — كل المالية تمر عبره.

---

## 7. مسار طلب API النموذجي

```
Browser → Nginx → ASP.NET Core API
                      │
                  Controller
                      │
                  MediatR Send(Command)
                      │
              Pipeline Behaviors:
                 LoggingBehavior
                 ValidationBehavior    ← FluentValidation
                 AuthorizationBehavior ← Permission Check
                 TransactionBehavior   ← DB Transaction
                      │
                  Command Handler
                      │
              ┌───────┴──────────┐
              │                  │
          DbContext          Publish Events
              │                  │
         PostgreSQL           Event Handlers
                                  │
                           Side Effects:
                           - Commission Calculation
                           - Inventory Deduction
                           - SignalR Notification
                           - Audit Log
```

---

*هذا المستند يُكمِّل [01_SYSTEM_ARCHITECTURE.md](01_SYSTEM_ARCHITECTURE.md) بتفصيل التبعيات.*  
*للتبعيات على مستوى قاعدة البيانات → [04_ERD_FINAL.md](04_ERD_FINAL.md)*
