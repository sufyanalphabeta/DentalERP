# 10 — Project Roadmap
# خارطة الطريق والمرجع السريع — DentalERP

> **الإصدار:** V-Final | **التاريخ:** 2026-06-16

---

## 1. حالة المشروع الحالية

### الوثائق المكتملة (جاهزة للتنفيذ)

| # | الوثيقة | المحتوى | الحالة |
|---|---------|---------|-------|
| 01 | [System Architecture](01_SYSTEM_ARCHITECTURE.md) | P1-P7، البنية الكاملة، ADRs | ✅ |
| 02 | [UX/UI Specification](02_UX_UI_SPECIFICATION.md) | 67 شاشة، Design System، RTL | ✅ |
| 03 | [Business Rules](03_BUSINESS_RULES_FINAL.md) | 40+ قاعدة، 32 صلاحية | ✅ |
| 04 | [ERD Final](04_ERD_FINAL.md) | DDL كامل، 42 جدول، 4 Views | ✅ |
| 05 | [Database Dictionary](05_DATABASE_DICTIONARY.md) | قاموس كل حقل | ✅ |
| 06 | [API Contracts](06_API_CONTRACTS.md) | 68 Endpoint + SignalR | ✅ |
| 07 | [Development Plan](07_DEVELOPMENT_PLAN.md) | 7 Phases + DoD | ✅ |
| 08 | [Deployment Architecture](08_DEPLOYMENT_ARCHITECTURE.md) | Docker، Nginx، Backup | ✅ |
| 09 | [Tech Stack](09_TECH_STACK.md) | كل المكتبات والإصدارات | ✅ |
| 10 | [Project Roadmap](10_PROJECT_ROADMAP.md) | هذا المستند | ✅ |
| 11 | [Module Dependencies](11_MODULE_DEPENDENCIES.md) | خريطة تبعيات الوحدات | ✅ |
| 12 | [Project Folder Structure](12_PROJECT_FOLDER_STRUCTURE.md) | هيكل المجلدات الكامل | ✅ |

**الخلاصة:** المشروع جاهز للانتقال إلى التطوير الفعلي. Documentation Freeze = TRUE.

---

## 2. نقطة البداية الموصى بها

```
أول خطوة: Phase 1 — Auth + Settings + Infrastructure

الملفات الأولى التي تُنشأ:
  1. DentalERP.sln
  2. src/DentalERP.Domain/
  3. src/DentalERP.Application/
  4. src/DentalERP.Infrastructure/ (+ DentalDbContext)
  5. src/DentalERP.API/ (+ Program.cs)
  6. Migrations: 001_initial_schema.sql
  7. frontend/ (Next.js 15 init)

أول شيء يُختبر:
  → POST /api/auth/login يُعيد JWT
  → App Shell يعرض Sidebar بـ RTL
```

---

## 3. MVP Gate — المسار الأساسي

```
Phase 1: Auth ────────────────────────────────────────────────┐
Phase 2: Patients + Queue ─────────────────────────────────────┤
Phase 3: Dental Chart + Procedures ────────────────────────────┤ → MVP
Phase 4: Invoices + Treasury + Commission Engine ──────────────┘

نهاية Phase 4 = نظام مكتمل وظيفياً للعيادة الأساسية:
مريض → طبيب → علاج → فاتورة → تحصيل نقدي → عمولة الطبيب
```

---

## 4. المسار الكامل (V1) ★ محدَّث

```
Phase 5: Inventory + Purchasing      ← مخزون + موردون
Phase 6: Laboratory                  ← معمل (Core — ليس اختيارياً)
Phase 7: Radiology                   ← أشعة (Core — ليس اختيارياً)
Phase 8: Reports + Dashboards + PDF  ← تقارير شاملة تغطي كل الوحدات

V1 Complete = بعد Phase 8
يشمل: مريض → طبيب → علاج → معمل → أشعة → فاتورة → تحصيل → تقارير
```

**ملاحظة:** المعمل والأشعة = Core Modules في V1 — لا Feature Flags، لا اختيارية

---

## 5. المرجع السريع — Architecture Decisions

| القرار | الاختيار النهائي | المستند |
|--------|----------------|---------|
| Single Tenant | لا tenant_id | [01](01_SYSTEM_ARCHITECTURE.md) |
| ORM | EF Core 8 + LINQ (لا Dapper) | [09](09_TECH_STACK.md) |
| Auth | JWT RS256 (15 min access + 7d refresh) | [01](01_SYSTEM_ARCHITECTURE.md) |
| Approval Workflow | اختياري — default=OFF | [03](03_BUSINESS_RULES_FINAL.md) |
| Inventory Tracking | اختياري per service | [03](03_BUSINESS_RULES_FINAL.md) |
| Commission Engine | 3 طرق، Cash-Basis | [03](03_BUSINESS_RULES_FINAL.md) |
| Financial Immutability | لا Physical Delete للسجلات المالية | [03](03_BUSINESS_RULES_FINAL.md) |
| Dental Chart | SVG مخصص + FDI Notation | [02](02_UX_UI_SPECIFICATION.md) |
| PDF Generation | FastReport.NET (Arabic RTL Native) | [09](09_TECH_STACK.md) |
| Real-time | SignalR + Redis Backplane | [01](01_SYSTEM_ARCHITECTURE.md) |
| Background Jobs | Hangfire + PostgreSQL | [09](09_TECH_STACK.md) |
| Deployment | Docker Compose + Nginx + MinIO (On-Premise LAN) | [08](08_DEPLOYMENT_ARCHITECTURE.md) |
| Balance Calculation | Computed Views (لا حقول مخزّنة) | [04](04_ERD_FINAL.md) |
| Stock Algorithm | FEFO (First Expired First Out) | [03](03_BUSINESS_RULES_FINAL.md) |
| Audit | Partition شهرية + MediatR Pipeline | [01](01_SYSTEM_ARCHITECTURE.md) |
| Lab Module | Core V1 — ليس اختيارياً | [04](04_ERD_FINAL.md), [07](07_DEVELOPMENT_PLAN.md) |
| Radiology Module | Core V1 — MinIO لصور الأشعة | [04](04_ERD_FINAL.md), [09](09_TECH_STACK.md) |
| Offline Architecture | Local Server (LAN) — لا IndexedDB/PWA | [01](01_SYSTEM_ARCHITECTURE.md) |

---

## 6. المرجع السريع — الجداول الحرجة

| الجدول | الحرجية | ملاحظة |
|--------|---------|--------|
| `vault_transactions` | عالية | Immutable — لا حذف — 12 نوع |
| `procedures` | عالية | يتحكم في 3 workflows |
| `invoices` | عالية | Immutable بعد confirmed |
| `workflow_settings` | عالية | يغيّر سلوك Edit/Delete/Cancel |
| `audit_logs` | عالية | Partitioned شهرياً |
| `commission_records` | متوسطة | Cash-Basis — يُنشأ عند Payment |
| `claims` | متوسطة | base_price_used Snapshot إلزامي |
| `stock_batches` | متوسطة | FEFO = current_quantity > 0 |
| `treatment_locations` | منخفضة | قيد: clinic واحد فقط |
| `lab_orders` | متوسطة | مرتبط بمريض + طبيب + إجراء (اختياري) |
| `lab_commission_records` | متوسطة | Cash-Basis — يُنشأ عند status=delivered |
| `radiology_orders` | متوسطة | CONSTRAINT: internal/external صارم |
| `radiology_images` | متوسطة | MinIO URLs — Soft Delete فقط |

---

## 7. المرجع السريع — Endpoints الأكثر تعقيداً

| Endpoint | التعقيد | السبب |
|---------|---------|-------|
| `POST /api/procedures/{id}/confirm` | عالي | يُطلق Domain Events متعددة |
| `POST /api/treasury/transactions/{id}/reverse` | عالي | 3 سجلات مرتبطة + Audit |
| `POST /api/invoices/{id}/cancel` | متوسط | يفحص workflow_settings |
| `PUT /api/procedures/{id}` | متوسط | يفحص workflow_settings |
| `POST /api/invoices/{id}/payments` | متوسط | يُطلق PaymentReceivedEvent → Commission |
| `POST /api/inventory/stock-take` | متوسط | Variance > 5% → Approval مطلوب |
| `POST /api/insurance/claims` | متوسط | base_price_used يُحسَّب تلقائياً |

---

## 8. المرجع السريع — Business Rules الأكثر أهمية

### قواعد لا خطأ فيها (إن نُسيت تكسر النظام)

```
BR-FIN-06: لا Physical Delete لـ:
  invoices, payments, advance_payments, vault_transactions,
  expense_vouchers, purchase_invoices, commission_records, payroll_records

BR-FIN-07: Reverse Transaction:
  → is_reversed=true على الأصلية
  → is_reversal=true على العكسية
  → سجل في reverse_transaction_links
  → سبب لا يقل عن 10 أحرف

BR-COMM-04: Cash-Basis فقط:
  → العمولة تُحسب عند PaymentReceivedEvent فقط
  → لا تُحسب عند إنشاء الفاتورة

BR-INS-03 (base_price_used):
  → Snapshot يُحفَظ عند إنشاء المطالبة
  → لا يتغيّر أبداً حتى لو تغيّرت إعدادات الشركة

BR-INV-01 (FEFO):
  → ORDER BY expiry_date ASC NULLS LAST WHERE current_quantity > 0
  → لا استثناء بدون صلاحية مدير مخزون

BR-LOC-01:
  → treatment_location_id إلزامي في كل procedure جديد
```

---

## 9. الأسئلة المفتوحة (تحتاج قراراً قبل Phase محددة)

### قبل Phase 1
- [ ] نظام التشغيل للخادم: Ubuntu أم Windows Server؟
- [ ] هل SSL داخل الشبكة المحلية مطلوب؟

### قبل Phase 3
- [ ] هل تاريخ Dental Chart مطلوب (جدول `dental_chart_history`) أم آخر حالة فقط؟
- [ ] Local Storage للملفات أم MinIO Object Storage؟

### قبل Phase 4
- [ ] هل نافذة Reverse Transaction (30 يوم) قابلة للتهيئة من الإعدادات؟

### قبل Phase 5
- [ ] هل Inventory Alerts تُرسل بـ Email/SMS أم SignalR داخلي فقط؟

### قبل Phase 6 (Laboratory)
- [ ] هل MinIO bucket واحد `radiology` أم نُقسّم: `radiology-images` + `patient-documents`؟
- [ ] هل صور الأشعة تُضغَّط server-side لتوفير المساحة (للـ CBCT الكبيرة)؟

---

## 10. قائمة الـ Open Questions من كل المستندات

مجمَّعة من أقسام "⚠️ نقاط تحتاج توضيح" في المستندات 01-09:

| # | السؤال | المستند | الأثر |
|---|--------|---------|-------|
| 1 | تاريخ Dental Chart كامل؟ | 04, 05 | جدول إضافي إن نعم |
| 2 | Walk-in بدون موعد مسموح؟ | 04 | appointment_id nullable — مؤكد |
| 3 | Patient Credit Notes مستقلة؟ | 05 | invoice_id nullable — مؤكد |
| 4 | Payroll items تفصيلية؟ | 05 | جدول payroll_items إن نعم |
| 5 | stock_batches.current_quantity: Trigger أم App Layer؟ | 05 | قرار تقني |
| 6 | installment overdue: Hangfire Job أم CHECK عند القراءة؟ | 05 | قرار تقني |
| 7 | inter_vault_transfer يحتاج جدول ربط؟ | 05 | بسيط vs مربوط |
| 8 | Pagination على Dental Chart (32 سن فقط)؟ | 06 | لا pagination موصى |
| 9 | Media Upload: Local vs Cloud؟ | 06, 08 | يؤثر على `file_url` |
| 10 | /me/account للطبيب أم endpoint موحّد؟ | 06 | UX decision |
| 11 | Soft Delete يُعيد 404 أم 410؟ | 06 | API convention |
| 12 | Timeline الأسابيع لكل Phase؟ | 07 | Project Management |
| 13 | Testing Strategy: بالتوازي أم بعد MVP؟ | 07 | Quality decision |
| 14 | Lab Module: Separate DLL أم داخل Monolith؟ | 07 | Architecture |
| 15 | SSL داخل LAN؟ | 08 | Self-Signed أم CA خاص |
| 16 | Backup خارج الموقع (USB/Cloud)؟ | 08 | Ops decision |
| 17 | Multiple Branches سيناريو؟ | 08 | V3 scope |
| 18 | ClosedXML vs EPPlus (GPL vs Commercial)؟ | 09 | License decision |
| 19 | FastReport.NET License cost؟ | 09 | Budget decision |
| 20 | IBM Plex Sans Arabic: Google Fonts vs Self-hosted؟ | 09 | محسوم: Self-hosted (لأن LAN) |
| 21 | MinIO bucket structure: واحد أم متعدد؟ | 08 | Ops decision |
| 22 | صور الأشعة: ضغط server-side؟ | 07 | Storage decision |

---

## 11. مسارد المصطلحات

| المصطلح | المعنى |
|---------|--------|
| MRN | Medical Record Number — رقم الملف الطبي (DEN-YYYY-XXXXX) |
| FEFO | First Expired First Out — يستهلك الأقرب انتهاءً أولاً |
| FDI | Fédération Dentaire Internationale — نظام ترقيم الأسنان |
| CQRS | Command Query Responsibility Segregation |
| Cash-Basis | العمولة تُحسب عند الدفع الفعلي (لا عند الإنشاء) |
| Snapshot | نسخة القيمة وقت الإنشاء (لا تتغيّر لاحقاً) |
| Soft Delete | حذف منطقي (deleted_at ≠ NULL) مع بقاء البيانات |
| Computed View | PostgreSQL VIEW تحسب الأرصدة Real-time |
| Domain Event | حدث يُطلق بعد اكتمال عملية (مثال: PaymentReceivedEvent) |
| Vertical Slice | Feature كاملة (DB → API → UI) في نفس الـ Sprint |
| ADR | Architecture Decision Record — قرار معماري مُوثَّق |
| LAN | Local Area Network — الشبكة المحلية للعيادة |
| PWA | Progressive Web App — يعمل Offline كـ App |

---

## 12. روابط المستندات

```
docs/
├── 01_SYSTEM_ARCHITECTURE.md      ← البنية + المبادئ + ADRs (12 وحدة)
├── 02_UX_UI_SPECIFICATION.md      ← 67+ شاشة + Design System + UX Flows
├── 03_BUSINESS_RULES_FINAL.md     ← 40+ قاعدة + 40 صلاحية
├── 04_ERD_FINAL.md                ← DDL الكامل + 56 جدول + 6 Views
├── 05_DATABASE_DICTIONARY.md      ← قاموس كل حقل (13 وحدة)
├── 06_API_CONTRACTS.md            ← 85 Endpoint + SignalR
├── 07_DEVELOPMENT_PLAN.md         ← 8 Phases + DoD لكل Phase
├── 08_DEPLOYMENT_ARCHITECTURE.md  ← Docker + Nginx + MinIO + Backup
├── 09_TECH_STACK.md               ← كل المكتبات + بنية المشروع
├── 10_PROJECT_ROADMAP.md          ← هذا المستند (خارطة الطريق)
├── 11_MODULE_DEPENDENCIES.md      ← خريطة تبعيات الوحدات
└── 12_PROJECT_FOLDER_STRUCTURE.md ← هيكل المجلدات الكامل
```

---

*هذا المستند هو الدليل التوجيهي للمشروع. يجب مراجعته في بداية كل Phase.*
