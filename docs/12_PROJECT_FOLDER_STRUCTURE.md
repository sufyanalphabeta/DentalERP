# 12 — Project Folder Structure
# هيكل مجلدات المشروع الكامل — DentalERP

> **الإصدار:** V-Final | **التاريخ:** 2026-06-17 | **الحالة:** مرجع تنفيذي معتمد

---

## 1. الهيكل العام للمستودع

```
DentalERP/                              ← جذر المستودع
│
├── backend/                            ← .NET 8 Solution
├── frontend/                           ← Next.js 15 Project
├── docker/                             ← Docker Compose + Nginx
├── scripts/                            ← Backup + Deployment Scripts
├── docs/                               ← الوثائق (هذا المجلد)
├── secrets/                            ← JWT Keys (لا تُرفع على Git)
├── .env.example                        ← متغيرات البيئة (نموذج)
├── .gitignore
└── README.md
```

---

## 2. Backend — .NET 8 Solution

```
backend/
│
├── DentalERP.sln
│
├── src/
│   │
│   ├── DentalERP.Host/                         ← Entry Point
│   │   ├── Program.cs                          ← DI Registration + Middleware Pipeline
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── appsettings.Production.json
│   │   └── Dockerfile
│   │
│   ├── DentalERP.SharedKernel/                 ← Shared Abstractions (لا تبعيات على أي Module)
│   │   ├── Abstractions/
│   │   │   ├── BaseEntity.cs                   ← id, created_at, updated_at, deleted_at
│   │   │   ├── IAggregateRoot.cs
│   │   │   └── IDomainEvent.cs
│   │   ├── Behaviors/                          ← MediatR Pipeline Behaviors
│   │   │   ├── LoggingBehavior.cs
│   │   │   ├── ValidationBehavior.cs           ← FluentValidation
│   │   │   ├── AuthorizationBehavior.cs        ← Permission Check
│   │   │   └── TransactionBehavior.cs          ← DB Transaction
│   │   ├── Results/
│   │   │   └── Result.cs                       ← Result<T> pattern
│   │   ├── Interfaces/
│   │   │   ├── ICurrentUser.cs
│   │   │   └── IFileStorageService.cs          ← MinIO Abstraction
│   │   └── Extensions/
│   │       └── ServiceCollectionExtensions.cs
│   │
│   ├── DentalERP.Modules.IAM/                  ← Module 1: Identity & Access
│   │   ├── IAMModule.cs
│   │   ├── Features/
│   │   │   ├── Login/
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   ├── LoginCommandHandler.cs
│   │   │   │   └── LoginValidator.cs
│   │   │   ├── RefreshToken/
│   │   │   ├── Logout/
│   │   │   ├── Users/                          ← CRUD المستخدمين
│   │   │   └── Roles/                          ← CRUD الأدوار والصلاحيات
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   ├── Role.cs
│   │   │   │   └── Permission.cs
│   │   │   └── Events/
│   │   └── Infrastructure/
│   │       ├── Configurations/
│   │       │   └── UserConfiguration.cs
│   │       └── Services/
│   │           ├── JwtService.cs
│   │           └── AuditService.cs
│   │
│   ├── DentalERP.Modules.Clinic/               ← Module 2: إعدادات العيادة
│   │   ├── Features/
│   │   │   ├── Doctors/
│   │   │   ├── Services/                       ← medical_services
│   │   │   ├── Specialties/
│   │   │   ├── TreatmentLocations/
│   │   │   ├── Vaults/
│   │   │   ├── WorkflowSettings/
│   │   │   └── ClinicSettings/
│   │   └── Domain/Entities/
│   │       ├── Doctor.cs
│   │       ├── MedicalService.cs
│   │       └── WorkflowSetting.cs
│   │
│   ├── DentalERP.Modules.Patient/              ← Module 3: المرضى
│   │   ├── Features/
│   │   │   ├── CreatePatient/
│   │   │   │   ├── CreatePatientCommand.cs
│   │   │   │   ├── CreatePatientCommandHandler.cs
│   │   │   │   └── CreatePatientValidator.cs
│   │   │   ├── GetPatient/
│   │   │   ├── UpdatePatient/
│   │   │   ├── DeletePatient/
│   │   │   ├── PatientMedicalHistory/
│   │   │   └── PatientDocuments/
│   │   └── Domain/
│   │       ├── Entities/
│   │       │   ├── Patient.cs
│   │       │   └── PatientMedicalHistory.cs
│   │       └── Events/
│   │           └── PatientCreatedEvent.cs
│   │
│   ├── DentalERP.Modules.Scheduling/           ← Module 4: المواعيد والطابور
│   │   ├── Features/
│   │   │   ├── Appointments/
│   │   │   ├── Queue/
│   │   │   └── Calendar/
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── Appointment.cs
│   │   │   │   └── QueueEntry.cs
│   │   │   └── Events/
│   │   │       └── AppointmentCompletedEvent.cs
│   │   └── Infrastructure/
│   │       └── Hubs/
│   │           └── QueueHub.cs                 ← SignalR Hub
│   │
│   ├── DentalERP.Modules.Clinical/             ← Module 5: الوحدة السريرية
│   │   ├── Features/
│   │   │   ├── DentalChart/
│   │   │   ├── TreatmentPlans/
│   │   │   ├── Procedures/
│   │   │   │   ├── CreateProcedure/
│   │   │   │   ├── UpdateProcedure/            ← workflow-aware
│   │   │   │   ├── DeleteProcedure/            ← workflow-aware
│   │   │   │   └── ConfirmProcedure/           ← يُطلق ProcedureConfirmedEvent
│   │   │   ├── Media/
│   │   │   └── ApprovalRequests/
│   │   └── Domain/
│   │       ├── Entities/
│   │       │   ├── Procedure.cs
│   │       │   ├── TreatmentPlan.cs
│   │       │   ├── DentalChartEntry.cs
│   │       │   └── ApprovalRequest.cs
│   │       └── Events/
│   │           ├── ProcedureConfirmedEvent.cs
│   │           └── ApprovalRequestedEvent.cs
│   │
│   ├── DentalERP.Modules.Treasury/             ← Module 6: الخزينة والمالية
│   │   ├── Features/
│   │   │   ├── Invoices/
│   │   │   │   ├── CreateInvoice/
│   │   │   │   ├── CancelInvoice/              ← workflow-aware
│   │   │   │   └── AddPayment/                 ← يُطلق PaymentReceivedEvent
│   │   │   ├── VaultTransactions/
│   │   │   │   ├── AddTransaction/
│   │   │   │   └── ReverseTransaction/
│   │   │   ├── Installments/
│   │   │   ├── AdvancePayments/
│   │   │   ├── Insurance/
│   │   │   │   ├── Claims/
│   │   │   │   └── InsuranceCompanies/
│   │   │   ├── Commissions/
│   │   │   ├── DayClosures/
│   │   │   └── Payroll/
│   │   └── Domain/
│   │       ├── Entities/
│   │       │   ├── Invoice.cs
│   │       │   ├── Payment.cs
│   │       │   ├── VaultTransaction.cs
│   │       │   └── CommissionRecord.cs
│   │       └── Events/
│   │           └── PaymentReceivedEvent.cs
│   │
│   ├── DentalERP.Modules.Inventory/            ← Module 7: المخزون
│   │   ├── Features/
│   │   │   ├── StockItems/
│   │   │   ├── StockBatches/
│   │   │   ├── StockMovements/
│   │   │   │   ├── Consume/                    ← FEFO
│   │   │   │   ├── Issue/                      ← صرف يدوي
│   │   │   │   └── Waste/
│   │   │   ├── StockTake/
│   │   │   └── StockAlerts/
│   │   └── Domain/
│   │       ├── Entities/
│   │       │   ├── StockItem.cs
│   │       │   ├── StockBatch.cs
│   │       │   └── StockMovement.cs
│   │       └── Events/
│   │           └── LowStockEvent.cs
│   │
│   ├── DentalERP.Modules.Purchasing/           ← Module 8: المشتريات
│   │   ├── Features/
│   │   │   ├── PurchaseRequests/
│   │   │   ├── PurchaseOrders/
│   │   │   ├── PurchaseInvoices/
│   │   │   ├── Suppliers/
│   │   │   └── ExternalCustomers/
│   │   └── Domain/Events/
│   │       └── GoodsReceivedEvent.cs
│   │
│   ├── DentalERP.Modules.Expenses/             ← Module 9: المصروفات
│   │   ├── Features/
│   │   │   ├── ExpenseCategories/
│   │   │   └── Expenses/
│   │   └── Domain/Entities/
│   │       └── Expense.cs
│   │
│   ├── DentalERP.Modules.Laboratory/           ← Module 10: المعمل ★ Core V1
│   │   ├── LaboratoryModule.cs
│   │   ├── Features/
│   │   │   ├── LabOrders/
│   │   │   │   ├── CreateLabOrder/
│   │   │   │   │   ├── CreateLabOrderCommand.cs
│   │   │   │   │   ├── CreateLabOrderCommandHandler.cs
│   │   │   │   │   └── CreateLabOrderValidator.cs
│   │   │   │   ├── UpdateLabOrderStatus/
│   │   │   │   └── GetLabOrders/
│   │   │   ├── LabTechnicians/
│   │   │   │   ├── CreateLabTechnician/
│   │   │   │   ├── GetLabTechnicianAccount/    ← lab_technician_account_summary View
│   │   │   │   └── PayLabTechnician/
│   │   │   ├── LabCommissions/
│   │   │   │   ├── GetLabCommissions/
│   │   │   │   └── PayLabCommissions/
│   │   │   ├── LabExpenses/
│   │   │   └── LabOrderTypes/
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── LabOrder.cs
│   │   │   │   ├── LabTechnician.cs
│   │   │   │   ├── LabOrderType.cs
│   │   │   │   ├── LabExpense.cs
│   │   │   │   └── LabCommissionRecord.cs
│   │   │   └── Events/
│   │   │       └── LabOrderDeliveredEvent.cs
│   │   └── Infrastructure/Configurations/
│   │       ├── LabOrderConfiguration.cs
│   │       └── LabTechnicianConfiguration.cs
│   │
│   ├── DentalERP.Modules.Radiology/            ← Module 11: الأشعة ★ Core V1
│   │   ├── RadiologyModule.cs
│   │   ├── Features/
│   │   │   ├── RadiologyOrders/
│   │   │   │   ├── CreateRadiologyOrder/
│   │   │   │   │   ├── CreateRadiologyOrderCommand.cs
│   │   │   │   │   ├── CreateRadiologyOrderCommandHandler.cs
│   │   │   │   │   └── CreateRadiologyOrderValidator.cs
│   │   │   │   ├── UpdateRadiologyOrderStatus/
│   │   │   │   ├── UploadRadiologyImages/      ← MinIO Upload
│   │   │   │   └── GetRadiologyOrders/
│   │   │   ├── RadiologyTechnicians/
│   │   │   │   ├── CreateRadiologyTechnician/
│   │   │   │   ├── GetRadiologyTechnicianAccount/
│   │   │   │   └── PayRadiologyTechnician/
│   │   │   ├── RadiologyCommissions/
│   │   │   ├── RadiologyExpenses/
│   │   │   └── RadiologyTypes/
│   │   ├── Domain/
│   │   │   ├── Entities/
│   │   │   │   ├── RadiologyOrder.cs
│   │   │   │   ├── RadiologyTechnician.cs
│   │   │   │   ├── RadiologyType.cs
│   │   │   │   ├── RadiologyImage.cs
│   │   │   │   ├── RadiologyExpense.cs
│   │   │   │   └── RadiologyCommissionRecord.cs
│   │   │   └── Events/
│   │   │       └── RadiologyOrderCompletedEvent.cs
│   │   └── Infrastructure/
│   │       ├── Configurations/
│   │       └── Services/
│   │           └── MinioFileStorageService.cs
│   │
│   └── DentalERP.Modules.Reporting/            ← Module 12: التقارير
│       ├── Features/
│       │   ├── Financial/
│       │   ├── Patients/
│       │   ├── Inventory/
│       │   ├── Laboratory/                     ← ★ تقارير المعمل
│       │   ├── Radiology/                      ← ★ تقارير الأشعة
│       │   └── Dashboard/
│       └── Infrastructure/
│           ├── Reports/
│           │   ├── Templates/                  ← .frx files (FastReport)
│           │   └── ReportService.cs
│           └── Jobs/                           ← Hangfire
│               ├── DailyInventoryAlertJob.cs
│               └── MonthlyAuditPartitionJob.cs
│
├── tests/
│   ├── DentalERP.UnitTests/
│   │   ├── Modules/
│   │   │   ├── IAM/
│   │   │   ├── Clinical/
│   │   │   ├── Treasury/
│   │   │   ├── Laboratory/
│   │   │   └── Radiology/
│   │   └── SharedKernel/
│   └── DentalERP.IntegrationTests/
│       ├── Api/                                ← HTTP Integration Tests
│       └── Database/                           ← DB Integration Tests
│
└── migrations/                                 ← SQL Migration Files
    ├── 001_initial_schema.sql                  ← clinics, users, roles, permissions
    ├── 002_settings_tables.sql                 ← doctors, staff, services, locations
    ├── 003_workflow_settings.sql               ← workflow_settings + Seed
    ├── 004_permissions_seed.sql                ← 40 صلاحية
    ├── 005_patient_tables.sql                  ← patients, medical_history, documents
    ├── 006_scheduling_tables.sql               ← appointments, queue_entries
    ├── 007_clinical_tables.sql                 ← dental_chart, treatment_plans, procedures
    ├── 008_treasury_tables.sql                 ← invoices, payments, vault_transactions
    ├── 009_insurance_tables.sql                ← insurance_companies, claims
    ├── 010_inventory_tables.sql                ← stock_items, batches, movements
    ├── 011_purchasing_tables.sql               ← suppliers, purchase_orders, invoices
    ├── 012_laboratory_tables.sql               ← ★ lab_order_types, lab_technicians, lab_orders
    ├── 013_radiology_tables.sql                ← ★ radiology_types, radiology_orders, images
    ├── 014_computed_views.sql                  ← 6 Views محسوبة
    └── 015_indexes.sql                         ← Performance Indexes
```

---

## 3. Frontend — Next.js 15

```
frontend/
│
├── app/                                        ← App Router (Next.js 15)
│   ├── layout.tsx                              ← Root Layout (lang="ar" dir="rtl")
│   ├── (auth)/
│   │   └── login/
│   │       └── page.tsx                        ← S63 شاشة تسجيل الدخول
│   │
│   └── (dashboard)/
│       ├── layout.tsx                          ← App Shell (Sidebar + Topbar + PermissionGate)
│       ├── page.tsx                            ← S01 لوحة التحكم اليومية
│       │
│       ├── patients/
│       │   ├── page.tsx                        ← S08 قائمة المرضى
│       │   ├── new/page.tsx                    ← S03 تسجيل مريض
│       │   └── [id]/
│       │       ├── page.tsx                    ← S09 ملف المريض (Tab Layout)
│       │       ├── dental-chart/page.tsx       ← S12 خريطة الأسنان
│       │       ├── treatment-plans/page.tsx    ← S13 خطط العلاج
│       │       ├── procedures/page.tsx         ← S14 سجل الإجراءات
│       │       ├── financial/page.tsx          ← S10 الوضع المالي
│       │       └── media/page.tsx              ← S11 مكتبة الوسائط
│       │
│       ├── scheduling/
│       │   ├── calendar/page.tsx               ← S04 التقويم (FullCalendar)
│       │   ├── appointments/page.tsx           ← S07 قائمة المواعيد
│       │   └── new-appointment/page.tsx        ← S05 حجز موعد
│       │
│       ├── clinical/
│       │   ├── procedures/page.tsx             ← S15 الإجراءات اليومية
│       │   └── approval-requests/page.tsx      ← S16 طلبات الموافقة
│       │
│       ├── invoices/
│       │   ├── page.tsx                        ← S17 قائمة الفواتير
│       │   ├── new/page.tsx                    ← S18 فاتورة جديدة
│       │   └── [id]/page.tsx                   ← S19 تفاصيل الفاتورة
│       │
│       ├── treasury/
│       │   ├── page.tsx                        ← S22 لوحة الخزينة
│       │   ├── transactions/page.tsx           ← S23 حركات الخزينة
│       │   ├── commissions/page.tsx            ← S27 عمولات الأطباء
│       │   ├── installments/page.tsx           ← S20 الأقساط
│       │   └── insurance/
│       │       ├── page.tsx                    ← S28 شركات التأمين
│       │       └── claims/page.tsx             ← S29 المطالبات
│       │
│       ├── inventory/
│       │   ├── page.tsx                        ← S33 الأصناف
│       │   ├── [id]/page.tsx                   ← S34 تفاصيل صنف
│       │   ├── movements/page.tsx              ← S35 حركات المخزون
│       │   ├── issue/page.tsx                  ← S36 صرف مواد
│       │   ├── stock-take/page.tsx             ← S37 الجرد الدوري
│       │   └── alerts/page.tsx                 ← S38 تنبيهات المخزون
│       │
│       ├── purchasing/
│       │   ├── requests/page.tsx               ← S40 طلبات الشراء
│       │   ├── orders/page.tsx                 ← S41 أوامر الشراء
│       │   ├── invoices/page.tsx               ← S42 فواتير الشراء
│       │   ├── suppliers/
│       │   │   ├── page.tsx                    ← S39 الموردون
│       │   │   └── [id]/account/page.tsx       ← S59 كشف حساب مورد
│       │   └── comparison/page.tsx             ← S43 مقارنة موردين
│       │
│       ├── laboratory/                         ← ★ Core V1 — Module 10
│       │   ├── page.tsx                        ← S-LAB-01 قائمة أوامر المعمل
│       │   ├── new/page.tsx                    ← S-LAB-02 إنشاء أمر معمل
│       │   ├── [id]/page.tsx                   ← تفاصيل الأمر
│       │   ├── technicians/
│       │   │   ├── page.tsx                    ← قائمة فنيو المعمل
│       │   │   └── [id]/account/page.tsx       ← S-LAB-03 كشف حساب فني
│       │   └── dashboard/page.tsx              ← S-LAB-04 لوحة متابعة المعمل
│       │
│       ├── radiology/                          ← ★ Core V1 — Module 11
│       │   ├── page.tsx                        ← S-RAD-01 قائمة طلبات الأشعة
│       │   ├── new/page.tsx                    ← S-RAD-02 إنشاء طلب أشعة
│       │   ├── [id]/page.tsx                   ← S-RAD-03 عارض الطلب + الصور
│       │   ├── technicians/
│       │   │   ├── page.tsx                    ← قائمة فنيو الأشعة
│       │   │   └── [id]/account/page.tsx       ← S-RAD-05 كشف حساب فني
│       │   └── dashboard/page.tsx              ← S-RAD-04 إحصاءات الأشعة
│       │
│       ├── reports/
│       │   ├── page.tsx                        ← S46 مركز التقارير (Hub)
│       │   └── [category]/page.tsx             ← S47 عارض التقرير
│       │
│       └── admin/
│           ├── users/page.tsx                  ← S51 إدارة المستخدمين
│           ├── roles/page.tsx                  ← S52 الأدوار والصلاحيات
│           ├── clinic-settings/page.tsx        ← S48 إعدادات العيادة
│           ├── audit-logs/page.tsx             ← S54 سجل التدقيق
│           └── workflow-settings/page.tsx      ← S55 إعدادات الموافقة
│
├── queue/
│   └── display/page.tsx                        ← S06 شاشة الاستدعاء (بلا Layout، بلا Auth)
│
├── components/
│   ├── ui/                                     ← shadcn/ui components
│   ├── dental-chart/                           ← SVG Component مخصص (FDI)
│   │   ├── DentalChart.tsx                     ← الكومبوننت الرئيسي
│   │   ├── ToothSVG.tsx                        ← سن واحد (5 مناطق)
│   │   ├── ToothLegend.tsx                     ← دليل الألوان
│   │   └── types.ts
│   ├── financial/
│   │   └── FinancialAccountPage.tsx            ← Reusable (supplier/doctor/lab-tech)
│   ├── image-gallery/
│   │   └── RadiologyImageGallery.tsx           ← ★ Lightbox للصور
│   └── shared/
│       ├── PermissionGate.tsx                  ← يُخفي المحتوى بدون صلاحية
│       ├── RealtimeIndicator.tsx               ← مؤشر SignalR
│       ├── DataTable.tsx                       ← جدول موحّد مع Pagination
│       └── StatusBadge.tsx                     ← Badge بألوان موحّدة
│
├── hooks/
│   ├── usePermission.ts                        ← فحص الصلاحيات من Zustand
│   ├── useQueueHub.ts                          ← SignalR Queue Hook
│   ├── useNotificationsHub.ts                  ← SignalR Notifications Hook
│   └── useFileUpload.ts                        ← ★ رفع ملفات للـ API
│
├── lib/
│   ├── api.ts                                  ← Axios Instance + Interceptors
│   ├── auth.ts                                 ← Token Management (Access + Refresh)
│   └── signalr.ts                              ← SignalR Connection Factory
│
├── stores/
│   ├── authStore.ts                            ← Zustand: user + permissions + token
│   └── uiStore.ts                              ← Zustand: sidebar, modals, notifications
│
├── types/
│   ├── api.ts                                  ← Response types موحّدة
│   ├── patient.ts
│   ├── laboratory.ts                           ← ★
│   └── radiology.ts                            ← ★
│
└── public/
    └── fonts/
        └── IBMPlexSansArabic/                  ← Self-hosted (Offline مطلوب)
```

---

## 4. Docker & Infrastructure

```
docker/
│
├── docker-compose.yml                          ← Production Config
├── docker-compose.override.yml                 ← Development Overrides
│
├── nginx/
│   ├── nginx.conf                              ← Reverse Proxy + SignalR + MinIO
│   └── ssl/                                   ← SSL Certificates (إن كانت مفعّلة)
│
└── minio/
    └── init-buckets.sh                         ← إنشاء buckets عند أول تشغيل
```

```
scripts/
│
├── backup.sh                                   ← DB + Uploads + MinIO Backup
├── restore.sh                                  ← استعادة النسخة الاحتياطية
├── deploy.sh                                   ← سحب كود جديد + بناء + تشغيل
└── seed-admin.sh                               ← إنشاء مدير النظام الأول
```

---

## 5. ملف البيئة (.env)

```env
# .env.example
# Database
DB_PASSWORD=change_me_strong_password

# Redis
REDIS_PASSWORD=change_me_redis_password

# MinIO
MINIO_USER=minioadmin
MINIO_PASSWORD=change_me_minio_password

# JWT Keys (يُنشآن بـ openssl — لا تُعدَّل يدوياً)
# secrets/jwt_private_key.pem
# secrets/jwt_public_key.pem
```

---

## 6. ترتيب إنشاء الملفات (Onboarding Order)

للمطور الجديد — ترتيب إنشاء المشروع من الصفر:

```
1. git clone → cd DentalERP
2. cp .env.example .env → عدّل كلمات المرور
3. mkdir -p secrets
   openssl genrsa -out secrets/jwt_private_key.pem 2048
   openssl rsa -in secrets/jwt_private_key.pem -pubout -out secrets/jwt_public_key.pem
4. docker compose up -d postgres redis minio
5. (انتظر 10 ثوانٍ حتى تبدأ PostgreSQL)
6. docker compose exec backend dotnet ef database update
7. docker compose exec backend dotnet run --seed-admin
8. docker compose up -d --build
9. افتح: http://localhost  ← النظام جاهز
   افتح: http://localhost:9001 ← MinIO Console
   افتح: http://localhost/hangfire ← Hangfire Dashboard (من LAN فقط)
```

---

*هذا المستند هو الدليل العملي لإعداد بيئة التطوير.*  
*للتبعيات بين الوحدات → [11_MODULE_DEPENDENCIES.md](11_MODULE_DEPENDENCIES.md)*  
*لبنية النشر الكاملة → [08_DEPLOYMENT_ARCHITECTURE.md](08_DEPLOYMENT_ARCHITECTURE.md)*
