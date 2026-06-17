# 07 — Development Plan
# خطة التطوير المرحلية — DentalERP

> **الإصدار:** V-Final (مع التعديلين النهائيين) | **التاريخ:** 2026-06-16

---

## 1. مبادئ تنظيم الخطة

| المبدأ | التطبيق |
|--------|---------|
| Full-Stack Sprint | Frontend و Backend يسيران معاً في كل Phase |
| Vertical Slice | كل Phase وحدة عمل قابلة للتسليم المستقل |
| DoD محدّد | معيار اكتمال واضح في نهاية كل Phase |
| ترتيب الأولوية | البنية التحتية → استقبال → عيادة → مال → مخزون → تقارير |
| API Contracts | المرجع الإلزامي لكل Endpoint → [06_API_CONTRACTS.md](06_API_CONTRACTS.md) |

---

## 2. التعديلان النهائيان (مُدمجان في الخطة)

### التعديل الأول: Approval Workflow = اختياري

```
الإعداد الافتراضي: Approval Workflow = OFF (معطَّل)
→ كل عملية تعمل بـ Permission-Based فوراً + Audit Log

إن فعّل مدير النظام Approval لنوع معيّن:
→ نفس العملية تتحوّل لـ Pending بدل التنفيذ الفوري

3 إعدادات مستقلة (workflow_settings):
  ☐ تعديل إجراء طبي     [procedure_edit]
  ☐ حذف إجراء طبي       [procedure_delete]
  ☐ إلغاء فاتورة        [invoice_cancel]

Seed: كل القيم = false (مُعطَّلة)
```

### التعديل الثاني: Inventory Tracking = اختياري لكل خدمة

```
في جدول medical_services:
  has_inventory_tracking BOOLEAN NOT NULL DEFAULT false

إن false (الافتراضي):
  → لا مواد افتراضية، لا خصم تلقائي، لا تفاعل مع وحدة المخزون

إن true:
  → يمكن ربط service_default_materials
  → عند تأكيد الإجراء: خصم FEFO تلقائي
  → قسم المواد يظهر في S15 لتعديل الكميات يدوياً
```

---

## 3. ملخص الـ 8 Phases ★ محدَّث

| Phase | المحتوى | Gate |
|-------|---------|------|
| **1** | Auth, Users, Roles, Settings, Infrastructure | — |
| **2** | Patients, Reception, Appointments, Queue | — |
| **3** | Dental Chart, Treatment Plans, Procedures, Media, Approval Workflow | — |
| **4** | Invoices, Treasury, Installments, Insurance Claims, Commission Engine | **← MVP Gate** |
| **5** | Inventory, Purchasing, Supplier Accounts | — |
| **6** | Laboratory — أوامر المعمل، الفنيون، العمولات، المصروفات | — |
| **7** | Radiology — طلبات الأشعة، الصور، الفنيون، العمولات، المصروفات | **← V1 Complete** |
| **8** | Reports, Dashboards, PDF, Background Jobs | **← Delivery** |

```
MVP بعد Phase 4: مريض → طبيب → علاج → فاتورة → تحصيل نقدي
V1  بعد Phase 7: كل الوحدات بما فيها المعمل والأشعة
V1+ بعد Phase 8: تقارير + Dashboards + PDF + Background Jobs
```

**ملاحظة مهمة:** المعمل والأشعة = Core Modules (V1) — ليسا اختياريَّيْن ولا Feature Flags

---

## 4. Phase 1 — Auth / Users / Roles / Settings

**الهدف:** البنية التحتية الكاملة التي تعتمد عليها كل Phase لاحقة. لا يمكن بناء أي وحدة أخرى قبل اكتمال هذه المرحلة.

### Backend Tasks

#### DB Migrations
```
001_initial_schema.sql
    clinics, users, roles, permissions, role_permissions, user_roles, refresh_tokens

002_settings_tables.sql
    treatment_locations, vaults, cost_centers, medical_services, doctors,
    staff, service_categories, specialties

003_workflow_settings.sql
    workflow_settings + Seed (procedure_edit=false, procedure_delete=false, invoice_cancel=false)

004_permissions_seed.sql
    32 صلاحية (INSERT INTO permissions ...)
```

#### Auth Module
```
POST /api/auth/login     → JWT Access Token (15 min) + Refresh Token (7 days, RS256)
POST /api/auth/refresh   → دوران Refresh Token
POST /api/auth/logout    → إبطال Refresh Token
GET  /api/auth/me        → بيانات المستخدم + Flat List صلاحياته
```

#### Users Module
```
GET/POST           /api/admin/users
GET/PUT/DELETE     /api/admin/users/{id}
POST               /api/admin/users/{id}/reset-password
POST               /api/admin/users/{id}/roles        (assign/revoke)
```

#### Roles Module
```
GET/POST           /api/admin/roles
GET/PUT/DELETE     /api/admin/roles/{id}
PUT                /api/admin/roles/{id}/permissions   (تحديث صلاحيات الدور)
GET                /api/admin/permissions              (القائمة الكاملة 32 صلاحية)
```

#### Settings Module
```
CRUD /api/settings/clinic              ← بيانات العيادة الواحدة
CRUD /api/settings/doctors             ← شامل commission_method + default_value
CRUD /api/settings/staff
CRUD /api/settings/services            ← شامل has_inventory_tracking toggle
CRUD /api/settings/service-categories
CRUD /api/settings/vaults
CRUD /api/settings/cost-centers
CRUD /api/settings/treatment-locations ← Tree: clinic→room→chair (قيد Clinic واحد)
CRUD /api/settings/workflow-settings   ← تفعيل/تعطيل Approval لكل نوع
```

#### Infrastructure (إلزامية قبل أي Phase لاحقة)
```
AuthorizationMiddleware     ← يتحقق من Permission Code على كل Endpoint
AuditLogInterceptor         ← يسجّل تلقائياً قبل/بعد كل Command (MediatR Pipeline)
GlobalExceptionHandler      ← أخطاء موحّدة بالعربية (ErrorResponse)
HealthCheck /health         ← للـ Nginx + Docker
SignalR Hub /hubs/notifications ← يُنشأ هنا ويُستخدَم في كل Phase لاحقة
Hangfire Dashboard /hangfire    ← Background Jobs Management
Serilog → PostgreSQL + File     ← Application Logs
```

### Frontend Tasks

#### شاشات Phase 1

**S-Auth: صفحة تسجيل الدخول**
- شعار العيادة + حقلا Username/Password + زر دخول
- Validation عربية واضحة + حالة Loading + Toast خطأ
- `AuthContext` (JWT + currentUser + permissions as `Set<string>`)

**App Shell (يُبنى هنا — يُستخدَم في كل Phase)**
- Sidebar RTL + Topbar + Breadcrumb
- `PermissionGate` Component: `<PermissionGate code="Invoice.Cancel">`
- `usePermission(code: string): boolean` hook
- Sidebar يتغيّر تلقائياً حسب permissions المستخدم

**S51: المستخدمون والصلاحيات**
- جدول المستخدمين + Drawer إضافة/تعديل + تعيين أدوار
- جدول الأدوار + Drawer + Checklist صلاحيات (مجمّعة بالوحدة)

**S48: إعدادات العيادة**
- نموذج بيانات العيادة: اسم، شعار، هاتف، عنوان، ساعات العمل

**S49: الأطباء والموظفون**
- جدولان بـ Tabs + Drawer إضافة
- حقل Commission Method (Select) + Default Value في بطاقة الطبيب

**S50: الخدمات والأسعار**
- جدول + Drawer + Toggle "تتبّع المواد" (`has_inventory_tracking`)

**S64: مواقع العلاج**
- Tree View ثلاثي المستويات (Clinic→Room→Chair)
- إضافة Inline + تعطيل + قيد Clinic واحد ظاهر للمستخدم

**S45: مراكز التكلفة**
- جدول CRUD بسيط + منع تعطيل مركز له مصروفات في الشهر الجاري

**S66: إعدادات سير الموافقات**
- 3 Toggle Switches فقط (تعديل إجراء / حذف إجراء / إلغاء فاتورة)
- عند تفعيل أي منها: S53 يظهر تلقائياً في Sidebar

**S52: سجل التدقيق**
- جدول مع Diff View (قديم/جديد بـ JSON diff coloring)
- فلاتر: الوحدة + نوع الإجراء + المستخدم + الفترة

### Business Rules المُنفَّذة في Phase 1
- BR-PERM-01/02 (Granular RBAC)
- BR-AUD-01/02/03 (Audit Log إلزامي لكل عملية)
- BR-LOC-02 (قيد Clinic واحد في treatment_locations)
- BR-CC-01 (cost_center_id إلزامي — القيد يُبنى هنا)
- Workflow Settings Seed (كل Approval معطَّل افتراضياً)

### ✅ Definition of Done — Phase 1
```
□ تسجيل الدخول وتجديد Token يعمل بالكامل
□ مستخدم بدور معيّن يرى فقط عناصر Sidebar التي يملك صلاحيتها
□ محاولة الوصول لـ Endpoint بدون صلاحية → 403 برسالة عربية
□ كل CRUD في الإعدادات يُسجَّل في audit_logs (قديم + جديد)
□ Tree View مواقع العلاج يعمل (إضافة/تعطيل) + قيد Clinic واحد
□ Approval Workflow إعدادات (3 Toggles) تعمل
□ App Shell يعمل بـ RTL كامل على Chrome, Edge, Mobile Chrome
□ SignalR Hub يتصل ويفصل بشكل صحيح + Reconnect يعمل
```

---

## 5. Phase 2 — Patients / Reception / Appointments

**الهدف:** المسار اليومي الأساسي من لحظة وصول المريض حتى انتظاره لدى الطبيب. هذه Phase قابلة للتشغيل الفعلي في العيادة.  
**الاعتماد:** Phase 1 مكتملة (doctors, settings, permissions موجودة)

### Backend Tasks

#### Patients Module
```
POST /api/patients                          ← إنشاء + MRN تلقائي (DEN-YYYY-XXXXX)
GET  /api/patients/check-duplicate          ← فحص التكرار (phone + fullName)
GET  /api/patients                          ← Paginated + Search + Filters
GET  /api/patients/{id}                     ← ملف المريض الكامل + إحصائيات
PUT  /api/patients/{id}
DELETE /api/patients/{id}                   ← يفحص: لا سجل مالي (BR-PAT-06)
POST /api/patients/{id}/insurance-link      ← ربط بشركة تأمين
DELETE /api/patients/{id}/insurance-link/{linkId}
```

#### Queue Module (Real-time via SignalR)
```
POST   /api/queue/check-in                  ← تسجيل حضور (Walk-in أو بموعد)
PUT    /api/queue/{id}/status               ← waiting/called/with_doctor/done/left
DELETE /api/queue/{id}                      ← إزالة من الطابور
GET    /api/queue                           ← قائمة اليوم (يدعم SignalR)

SignalR Hub Events:
  QueueUpdated(queueItem)         → يصل لكل الموصّلين
  PatientCalled(queueId,doctorId) → يصل لشاشة الطبيب فقط
```

#### Appointments Module
```
GET/POST    /api/appointments
GET         /api/appointments/calendar?from=&to=&doctorId=
PUT         /api/appointments/{id}
PUT         /api/appointments/{id}/status    ← confirm/cancel/reschedule
POST        /api/appointments/{id}/add-to-queue ← تحويل موعد لطابور فوري
```

### Frontend Tasks

**S03: تسجيل مريض جديد**
- Section قابلة للطي: أساسية (إلزامية) + تفصيلية (اختيارية)
- Duplicate Detection: Toast يعرض المريض المطابق فوراً + زر "فتح الملف"
- Toast نجاح: MRN + زر "إضافة للانتظار مباشرة"

**S08: قائمة المرضى**
- DataTable + بحث متعدد (اسم/هاتف/MRN) + فلتر طبيب
- كل صف: زر "فتح الملف" + "حجز موعد" + "إضافة للانتظار"

**S09: ملف المريض (Hub)**
- الترويسة الثابتة: MRN + حساسية + فصيلة دم + شركة تأمين
- Tab ①: البيانات الأساسية (نفس S03 بوضع تعديل)
- Tab ②/③/④/⑤: Placeholders — تُكتمل في Phase 3/4

**S05: قائمة الانتظار (Real-time)**
- `useQueueHub` hook يستهلك SignalR
- جدول يتحدث لحظياً + RealtimeIndicator (نقطة وميض خضراء)
- أزرار: [استدعاء] [تفاصيل] [إزالة]

**S06: شاشة الاستدعاء (Display Screen)**
- `/queue/display` — مسار منفصل بلا Sidebar، بلا صلاحية مطلوبة
- Full-Screen: رقم الانتظار + اسم المريض + "توجّه للعيادة"
- تحديث لحظي عبر SignalR

**S18/S19/S20: التقويم والحجز**
- FullCalendar (locale: ar, direction: rtl)
- عرض يومي/أسبوعي/شهري + Drag & Drop لإعادة الجدولة
- Drawer حجز: مريض (Combobox بحث) + طبيب + وقت + نوع

### Business Rules المُنفَّذة في Phase 2
- BR-PAT-01 (Duplicate Detection — هاتف أو اسم + تاريخ ميلاد)
- BR-PAT-02 (MRN: DEN-YYYY-00001، Sequence مستقل لكل سنة)
- BR-PAT-03 (patient_doctor_assignments يُنشأ تلقائياً عند أول إجراء)
- BR-PAT-06 (منع الحذف إن وجد سجل مالي)
- BR-DOC-01 (الطبيب يرى مرضاه فقط في S08)

### ✅ Definition of Done — Phase 2
```
□ تسجيل مريض → MRN يُولَّد → إضافة للانتظار → يظهر فوراً في S05 بدون Refresh
□ Duplicate Detection: هاتف متطابق → تحذير فوري + بطاقة المريض المطابق
□ الطبيب يرى فقط مرضاه في S08 (فلترة تلقائية من Backend)
□ التقويم يعرض مواعيد بألوان الأطباء + Drag & Drop يعمل
□ SignalR: إضافة مريض → يظهر فوراً بمتصفحين منفصلين
□ شاشة Display تعمل بدون تسجيل دخول
```

---

## 6. Phase 3 — Dental Chart / Treatment Plans / Procedures / Media / Approval

**الهدف:** المسار السريري الكامل للطبيب. بنهاية هذه Phase: طبيب يستطيع استقبال مريض، تسجيل الخطة، تنفيذ الإجراء، رفع الصور، وإرسال الملف للخزينة.  
**الاعتماد:** Phase 2 (المريض موجود + قائمة الانتظار تعمل)

### Backend Tasks

#### Dental Chart Module
```
GET  /api/dental-chart/{patientId}               ← كل الأسنان (دائمة + لبنية)
PUT  /api/dental-chart/{patientId}/teeth/{toothNumber} ← تحديث حالة سن + BR-DOC-03
GET  /api/dental-chart/{patientId}/history       ← تاريخ التغييرات مرتّباً زمنياً
```

#### Treatment Plans Module
```
POST   /api/treatment-plans
GET    /api/treatment-plans/{id}                 ← + items + completion %
PUT    /api/treatment-plans/{id}
POST   /api/treatment-plans/{id}/items
PUT    /api/treatment-plans/{id}/items/{itemId}
DELETE /api/treatment-plans/{id}/items/{itemId}
```

#### Procedures Module
```
POST   /api/procedures
       Required: patient_id, doctor_id, service_id, treatment_location_id (BR-LOC-01)
       Optional: treatment_plan_item_id, lab_cost, discount

PUT    /api/procedures/{id}
       → يفحص workflow_settings[procedure_edit]:
         false → تعديل فوري + Audit
         true  → ينشئ approval_request + يُعيد 202

DELETE /api/procedures/{id}
       → نفس المنطق: يفحص workflow_settings[procedure_delete]

POST   /api/procedures/{id}/confirm
       → status → 'confirmed'
       → إن has_inventory_tracking=true:
           ينشئ stock_movements لكل مادة في service_default_materials
           (movement_type='consumption', FEFO التلقائي)
       → يُرسل SignalR event لـ Treasury: "إجراء جاهز للفوترة"

GET    /api/procedures/{id}/default-materials
       → يُعيد قائمة المواد الافتراضية + الكمية + الكمية الحالية في المخزون
         (فقط إن has_inventory_tracking=true)
```

#### Patient Assignments Module
```
POST /api/patient-assignments/{assignmentId}/close  ← can_edit=false
GET  /api/patient-assignments/{patientId}           ← قائمة الأطباء المشتركين
```

#### Media Library Module
```
POST   /api/patients/{id}/media       ← multipart/form-data (رفع ملف)
GET    /api/patients/{id}/media       ← + filters: type, date, doctor
DELETE /api/patients/{id}/media/{mediaId} ← Soft Delete فقط
```

**Storage:** ملفات محلية (Local Storage على الخادم) — مسار في `clinic-settings`

#### Approval Workflow Module (يُبنى هنا)
```
GET  /api/admin/approval-requests          ← pending فقط (للمدير)
PUT  /api/admin/approval-requests/{id}/review
     → approved: يُنفّذ الإجراء المؤجَّل (التعديل أو الحذف)
     → rejected: يُلغى الطلب + إشعار لمقدّمه عبر SignalR
```

### Frontend Tasks

**S11: قائمة انتظار الطبيب**
- مشابهة S05 لكن مفلترة على doctor_id تلقائياً
- زر "استدعاء" يفتح S12 مباشرة

**S12: الشاشة السريرية (Split View)**
- عمود ثابت أيمن: بيانات المريض + تحذيرات طبية + حالة Assignment
- عمود أيسر متغيّر: Tabs (Chart | خطة العلاج | إجراءات | صور | تاريخ)
- زر "إنهاء حالتي" → Confirmation Dialog → `POST /patient-assignments/close`

**S13: Dental Chart التفاعلي (SVG Component مخصص)**
- 32 سن دائم (FDI 11-48) + 20 سن لبني (51-85)
- كل سن: 5 مناطق قابلة للنقر (M/D/B/L/O)
- Legend ألوان ثابت + Panel جانبي عند اختيار سن
- Segment Control (دائمة / لبنية / الاثنان)
- **Read-Only Mode** تلقائياً إن `can_edit=false` (حدود برتقالية)

**S14: خطة العلاج**
- جدول عناصر + شريط إنجاز % + زر "+ خدمة" → يفتح S15 مسبق التعبئة

**S15: تسجيل إجراء جديد**
- Select خدمة (يُحمَّل `has_inventory_tracking` تلقائياً)
- MultiSelect أسنان من Dental Chart (clickable)
- Select موقع العلاج (Clinic→Room→Chair) — إلزامي
- سعر تلقائي + حقل خصم (نسبة/مبلغ)
- حقل "تكلفة المعمل" (اختياري — لـ Net Commission)
- **قسم المواد (يظهر فقط إن `has_inventory_tracking=true`):**
  - جدول المواد الافتراضية + تعديل الكميات يدوياً
- زر "تأكيد وإرسال للخزينة"

**S16: رفع الصور (Drawer)**
- Dropzone + Preview + تحديد النوع (قبل/بعد/أشعة/OPG/CBCT/مستند)
- Upload Progress Bar + رفع متعدد

**S09 (مكتمل): مكتبة الوسائط + خطط العلاج**
- Tab ③: Timeline Grid بالتاريخ + Lightbox
- Tab ⑤: قائمة خطط علاج + نسبة إنجاز + رابط S14

**S53: صندوق الموافقات**
- يظهر في Sidebar فقط عند `requires_approval=true` لأي من الأنواع الثلاثة
- قائمة Pending + زر قبول/رفض + Modal سبب الرفض
- Badge بالعدد (Real-time via SignalR)

### Business Rules المُنفَّذة في Phase 3
- BR-PAT-04/05 (مريض مشترك + can_edit lock)
- BR-DOC-02/03 (حدود الرؤية: طبيب يرى Dental Chart كاملاً + إجراءاته فقط)
- BR-LOC-01 (`treatment_location_id` إلزامي في كل procedure)
- BR-INV-01 (FEFO في الخصم التلقائي — فقط إن has_inventory_tracking=true)
- BR-APR-01/02/03/04 (Approval Workflow)
- التعديل الثاني: `has_inventory_tracking` يتحكم في قسم المواد بالكامل
- التعديل الأول: `workflow_settings` تتحكم في سلوك Edit/Delete الإجراء

### ✅ Definition of Done — Phase 3
```
□ Dental Chart SVG: نقر على سن → Panel → حفظ → يتغيّر اللون فوراً
□ Read-Only Mode يعمل تلقائياً إن can_edit=false
□ إجراء (has_inventory=true) → تأكيد → صرف مخزون تلقائي FEFO
□ إجراء (has_inventory=false) → تأكيد → لا تفاعل مع المخزون إطلاقاً
□ رفع صورة → تُحفَظ → تظهر في Timeline بعد Refresh
□ زر "إرسال للخزينة" → SignalR event → يظهر في S21 بدون Refresh
□ Approval Workflow (عند تفعيله): طلب تعديل → Pending في S53 → مدير يقبل → يُنفَّذ
□ BR-APR-02: مقدّم الطلب لا يستطيع الموافقة على طلبه الخاص
```

---

## 7. Phase 4 — Invoices / Treasury / Insurance / Commission Engine

**الهدف:** المسار المالي الكامل: فاتورة → تحصيل → عمولة → مطالبة تأمين.  
**الاعتماد:** Phase 3 (الإجراءات تصل للخزينة عبر SignalR)

### Backend Tasks

#### Invoices Module
```
POST   /api/invoices
GET    /api/invoices                              ← + filters: status/doctor/date
GET    /api/invoices/{id}                         ← + items + payments + remaining
PUT    /api/invoices/{id}                         ← draft فقط
DELETE /api/invoices/{id}                         ← draft فقط (BR-FIN-06)
POST   /api/invoices/{id}/cancel
       → يفحص workflow_settings[invoice_cancel]:
         false → إلغاء فوري + Audit
         true  → approval_request + 202
GET    /api/invoices/{id}/print                   ← FastReport.NET PDF (RTL)
POST   /api/invoices/{id}/payments                ← تسجيل دفعة → PaymentReceivedEvent
```

#### Treasury Module
```
POST   /api/treasury/transactions
       → يقبل transaction_type + مبلغ + خزينة + الكيان المرتبط
       → Commission Engine يُشغَّل تلقائياً عند استلام من مريض

PUT    /api/treasury/transactions/{id}
       → مسموح: نفس اليوم + قبل Day Closure فقط

DELETE /api/treasury/transactions/{id}
       → مسموح: أقل من 24 ساعة + غير محجوبة

POST   /api/treasury/transactions/{id}/reverse
       → ينشئ حركة عكسية + reverse_transaction_links
       → يقبل correctedTransaction اختياري (حركة صحيحة بديلة)

GET    /api/treasury/vaults/balances
POST   /api/treasury/day-closure
       → يُقفل اليوم + يمنع Edit على حركاته مستقبلاً
```

#### Commission Engine (Service داخلي، يُشغَّل عند `PaymentReceivedEvent`)

```csharp
// CommissionCalculationService.Calculate(procedureId, paidAmount)
1. يقرأ doctor_service_commissions (override للخدمة المحددة)
   إن لم يوجد → يقرأ doctors.commission_method + default_commission_value

2. يُطبّق الصيغة:
   percentage_of_service:  amount = paidAmount × rate / 100
   fixed_amount:           amount = fixed_value (ثابت بغض النظر عن المبلغ)
   percentage_of_net:      net = paidAmount - lab_cost
                           amount = net × rate / 100

3. ينشئ commission_record { is_paid: false }
```

#### Installments & Advance Payments
```
POST /api/installments/plans         ← خطة أقساط: invoiceId + عدد + تاريخ بداية
PUT  /api/installments/{id}/pay      ← دفع قسط محدد
POST /api/advance-payments           ← دفعة مقدمة جديدة
POST /api/advance-payments/{id}/apply ← تطبيق على فاتورة
```

#### Insurance Claims (التكامل مع Treasury)
```
POST /api/insurance/claims                    ← base_price_used يُحسَّب تلقائياً (Snapshot)
POST /api/insurance/claims/{id}/collect
     → ينشئ vault_transaction (general_receipt + related_claim_id)
PUT  /api/insurance/claims/{id}/reject        ← رفض كلي/جزئي + سبب
GET  /api/insurance/claims                    ← + filters: company/status/date
```

#### Doctor Financial Account
```
GET  /api/treasury/doctors/{id}/account?from=&to=&locationId=
     → doctor_account_summary View (procedures + collected + commission_due + paid + remaining)
GET  /api/treasury/doctors/{id}/account/print    ← FastReport PDF
POST /api/treasury/commissions/{doctorId}/pay    ← دفع عمولة + vault_transaction
```

### Frontend Tasks

**S21: الفواتير المعلقة**
- DataTable + فلاتر + Badge حالة
- SignalR: إجراء جديد → صف يظهر بـ highlight أصفر لثانية
- زر [+دفعة] يفتح S24a كـ Drawer

**S22/S23: إنشاء فاتورة / تفاصيل فاتورة**
- S22: بنود + خصم إجمالي
- S23: عرض كامل + سجل دفعات + زر طباعة

**S24a: استلام دفعة (Drawer — النموذج الديناميكي الأساسي)**
- المبلغ + 4 طرق دفع (نقدي/مصرف/بطاقة/POS) + الخزينة
- Radio: دفع عادي / تطبيق دفعة مقدمة / قسط
- عند التأكيد: Toast + خيار طباعة السند

**S25: الدفعات المقدمة** — جدول + إضافة + [تطبيق على فاتورة]

**S26: الأقساط** — جدول مجمّع + أقساط متأخرة بالأحمر

**S65: حركة عكسية**
- بحث عن الحركة + تفاصيلها + حقل "سبب العكس" (إلزامي)
- Checkbox "إنشاء حركة صحيحة بديلة" → نموذج ثانٍ

**S30: أرصدة الخزائن** — 4 بطاقات KPI (نقدي/مصرف/بطاقة/POS)

**S32: إقفال اليوم** — ملخص شامل + زر إقفال + Confirm Dialog صارم

**S31: العمولات**
- Tabs: [مستحقة للأطباء] [مدفوعة]
- Tooltip يشرح طريقة الحساب لكل طبيب
- زر "دفع عمولة" → Drawer تأكيد

**S63: كشف حساب الطبيب**
- 4 KPIs + جدول تفصيلي + فلاتر + زر PDF + زر "دفع طبيب"

**S57: مطالبات التأمين** — جدول + تسجيل تحصيل (Drawer سريع)

**S09d: الحساب المالي للمريض (Tab مكتمل)**
- 4 KPIs + جدول أقساط + دفعات مقدمة نشطة + زر كشف حساب

### Business Rules المُنفَّذة في Phase 4
- BR-INV-FIN-01/02/03/04 (Invoice lifecycle + Immutability)
- BR-TRE-01/02/03/04 (Treasury: تعديل نفس اليوم + قبل Closure)
- BR-COMM-01/02/03 (Commission Engine 3 طرق)
- BR-FIN-04 (Cash-Basis Commission — فقط عند PaymentReceivedEvent)
- BR-FIN-06 (Immutability — لا حذف للحركات المقفلة)
- BR-FIN-07 (Reverse Transaction — 30 يوم + سبب إلزامي)
- BR-INS-01/02/03 (Claims + base_price_used Snapshot)
- التعديل الأول: Invoice.Cancel يفحص workflow_settings تلقائياً

### ✅ Definition of Done — Phase 4
```
□ إجراء (Phase 3) → فاتورة → دفعة → رصيد خزينة يتحدث Real-time
□ عمولة الطبيب: 3 طرق مختلفة تُختبر في بيئة test
□ Reverse Transaction: أصلية + عكسية + بديلة تظهر مرتبطة في السجل
□ إقفال اليوم: Edit على حركات المُقفَل → 403
□ base_price_used محفوظ ولا يتغيّر إن تغيّرت إعدادات الشركة لاحقاً
□ قسط متأخر → يظهر بالأحمر + يُحتسب في "المتبقي"
□ كشف حساب الطبيب: مدير يرى كل الأطباء / طبيب يرى نفسه فقط
```

---

## 8. Phase 5 — Inventory / Purchasing / Supplier Accounts

**الهدف:** إدارة المخزون الطبي الكامل (شراء → استهلاك → توالف) + الموردون.  
**الاعتماد:** Phase 4 (فواتير الشراء ترتبط بحركات الخزينة)

### Backend Tasks

#### Inventory Module
```
CRUD   /api/inventory/items
POST   /api/inventory/items/{id}/receive          ← استقبال دُفعة جديدة (stock_batches)
POST   /api/inventory/issue                       ← صرف يدوي (FEFO تلقائي)
POST   /api/inventory/waste                       ← توالف (movement_type=waste + reason إلزامي)
POST   /api/inventory/stock-take                  ← جرد دوري
POST   /api/inventory/stock-take/{id}/approve     ← Variance>5% يتطلب مراجعة مدير
GET    /api/inventory/alerts                      ← تحت الحد + قارب الانتهاء + منتهي
POST   /api/settings/service-default-materials    ← مسموح فقط إن has_inventory_tracking=true
GET    /api/inventory/items/{id}/supplier-comparison ← آخر أسعار كل مورد + أقل 6 أشهر
```

#### Purchasing Module
```
CRUD   /api/purchasing/requests
POST   /api/purchasing/requests/{id}/convert-to-order
CRUD   /api/purchasing/orders
POST   /api/purchasing/orders/{id}/approve        ← Purchase.Approve
POST   /api/purchasing/invoices                   ← استلام فاتورة + تحديث stock_batches
```

#### Supplier & External Customer Accounts
```
CRUD /api/suppliers
GET  /api/suppliers/{id}/account?from=&to=       ← supplier_account_summary View
GET  /api/suppliers/{id}/account/ledger          ← حركة كاملة
GET  /api/suppliers/{id}/account/print           ← FastReport PDF

CRUD /api/external-customers
POST /api/external-customers/{id}/transactions
GET  /api/external-customers/{id}/account
GET  /api/external-customers/{id}/account/print
```

### Frontend Tasks

**S33: قائمة الأصناف** — Badge ملوّن للحالة: 🟢 جيد / 🟡 قارب الانتهاء / 🔴 تحت الحد

**S34: تفاصيل صنف + الدُفعات**
- بطاقة رئيسية: الكمية الكلية + متوسط التكلفة
- جدول الدُفعات مرتّب FEFO بصرياً

**S37: الجرد الدوري** — Inline Edit + Variance تلقائي + أحمر إن > 5%

**S38: لوحة التنبيهات** — Card لكل تنبيه + [إنشاء طلب شراء مباشرة]

**S39: الموردون** — جدول + "عرض الحساب" → S59

**S40/S41/S42: Workflow الشراء (3 شاشات)**
- Status Stepper: طلب شراء → أمر شراء → فاتورة شراء
- Conversion Button بين المراحل

**S43: مقارنة الموردين**
- Select الصنف → مورد | آخر سعر | أقل سعر (6 أشهر) | التوفير%
- Highlight السطر الأفضل بالأخضر

**S59/S60/S61: كشوف الحسابات (Pattern موحّد)**
- `<FinancialAccountPage variant="supplier|customer">`
- 4 KPIs + كشف حركة + PDF

**S36: صرف مواد يدوي** — Select الصنف + الكمية + النوع + الطبيب

### Business Rules المُنفَّذة في Phase 5
- BR-INV-01 (FEFO في كل صرف)
- BR-INV-02 (تنبيهات: Hangfire Job يومياً 08:00)
- BR-INV-03/04/05 (أنواع الصرف + التسويات + ربط الاستهلاك بالطبيب/الموقع)
- BR-FIN-08 (Computed View — لا تخزين رصيد ثابت للمورد)
- التعديل الثاني: `service_default_materials` يُقيَّد بـ `has_inventory_tracking=true`

### ✅ Definition of Done — Phase 5
```
□ إجراء (has_inventory=true) → تأكيد → يظهر في حركة المخزون فوراً
□ FEFO: النظام يصرف الدُفعة الأقرب انتهاءً (test بدُفعتين مختلفتي التاريخ)
□ توالف يظهر منفصلاً عن الاستهلاك في S38
□ مقارنة موردين: آخر 6 أشهر صحيحة
□ كشف مورد: رصيد = opening_balance + مشتريات - مدفوع (اختبار حسابي)
□ تنبيهات المخزون: Job يعمل يومياً + Badge في Sidebar يتحدث
```

---

## 9. Phase 6 — Laboratory Module ★ Core V1

**الهدف:** وحدة المعمل الكاملة — أوامر، فنيون، كشوف حسابات، عمولات، مصروفات، تقارير.  
**الاعتماد:** Phase 1-4 (المرضى، الإجراءات، الخزينة جاهزة)

### Backend Tasks

#### DB Migration
```
012_laboratory_module.sql
    lab_order_types, lab_technicians, lab_orders,
    lab_expense_categories, lab_expenses, lab_commission_records
    ALTER TABLE vault_transactions ADD COLUMNS (related_lab_order_id)
    ALTER TABLE vault_transactions ADD transaction_type: 'lab_income','payment_to_lab'
```

#### Lab Module Features
```
CRUD /api/lab/orders
    → إنشاء أمر معمل + ربط بمريض/طبيب/إجراء
    → تحديث status (pending→in_progress→completed→delivered)
    → حساب عمولة الفني عند status=delivered

CRUD /api/lab/technicians
CRUD /api/lab/order-types
GET  /api/lab/technicians/{id}/account   ← lab_technician_account_summary View
POST /api/lab/technicians/{id}/pay       ← vault_transaction: payment_to_lab
GET  /api/lab/commissions/{id}
POST /api/lab/commissions/{id}/pay
CRUD /api/lab/expenses
```

#### Lab Commission Engine
```
عند status = 'delivered':
  1. اقرأ commission_method + default_commission_value من lab_technicians
  2. احسب العمولة:
     - percentage_of_service: cost × rate / 100
     - fixed_amount: commission_value مباشرة
  3. أنشئ lab_commission_records { is_paid: false }
```

#### Lab Account Logic
```
رصيد الفني = opening_balance + Σ(lab_orders.cost) - Σ(vault_transactions.amount WHERE type='payment_to_lab')
← مبني على lab_technician_account_summary View
```

### Frontend Tasks

**S-LAB-01: قائمة أوامر المعمل**
- جدول فلترة بالحالة / الطبيب / الفني / التاريخ
- Badge ملوّن للحالة: 🔵 معلّق / 🟡 جارٍ / 🟢 مكتمل / ✅ مُسلَّم
- عمود "أيام متبقية" للمواعيد القادمة (محسوب من expected_date)

**S-LAB-02: إنشاء/تعديل أمر معمل**
- Select مريض + طبيب + نوع الشغل + فني المعمل
- رسم أسنان FDI تفاعلي للاختيار (يُعيد مصفوفة tooth_numbers)
- حقلا التكلفة والسعر مع ملاحظة الفرق (هامش)
- التاريخ المتوقع مع تنبيه لو أقل من 3 أيام

**S-LAB-03: كشف حساب فني المعمل**
- 4 KPIs: إجمالي الشغل / إجمالي التكاليف / المدفوع / المتبقي
- جدول الأوامر بالتفاصيل + [دفع] في كل صف
- Modal دفع: المبلغ + الخزينة + تحديد الأوامر

**S-LAB-04: لوحة متابعة المعمل**
- أوامر متأخرة (expected_date < today + status NOT delivered)
- إحصاء شهري: أوامر / تكاليف / إيرادات المعمل

### Business Rules المُنفَّذة في Phase 6
- عمولة الفني: Cash-Basis — تُحسَب عند تغيير status إلى `delivered`
- رصيد الفني: Computed View — لا حقل مخزَّن
- مدفوعات المعمل: Immutable — Soft Delete فقط (lab_expenses)

### ✅ Definition of Done — Phase 6
```
□ إنشاء أمر معمل → تحديث status → حساب عمولة تلقائية عند delivered
□ كشف حساب الفني: الرصيد = opening + Σcost - Σpaid (اختبار حسابي)
□ دفع للمعمل: vault_transaction نوع payment_to_lab يُنشأ تلقائياً
□ مصروفات المعمل: تُخصَم من الخزينة + تظهر في التقارير
□ أوامر متأخرة: التنبيه يظهر في S-LAB-04
□ صلاحية Lab.Create: موظف بدونها لا يرى وحدة المعمل
```

---

## 10. Phase 7 — Radiology Module ★ Core V1

**الهدف:** وحدة الأشعة الكاملة — طلبات داخلية وخارجية، صور MinIO، فنيون، عمولات، مصروفات.  
**الاعتماد:** Phase 1-4 + MinIO جاهز

### Backend Tasks

#### DB Migration
```
013_radiology_module.sql
    radiology_types, radiology_technicians, radiology_orders,
    radiology_images, radiology_expense_categories, radiology_expenses,
    radiology_commission_records
    ALTER TABLE vault_transactions ADD COLUMNS (related_radiology_order_id)
    ALTER TABLE vault_transactions ADD transaction_type: 'radiology_income'
```

#### Radiology Module Features
```
CRUD /api/radiology/orders
    → internal: مريض من النظام + يُدرَج في فاتورته
    → external: بيانات زائر + دفع فوري → vault_transaction: radiology_income

POST /api/radiology/orders/{id}/images
    → رفع صور → MinIO → حفظ URL في radiology_images
GET  /api/radiology/orders/{id}/images
    → عرض صور الطلب

CRUD /api/radiology/technicians
CRUD /api/radiology/types
POST /api/radiology/orders/{id}/status  → يُحدَّث عند اكتمال الفحص
GET  /api/radiology/commissions/{id}
POST /api/radiology/commissions/{id}/pay
CRUD /api/radiology/expenses
```

#### MinIO Integration
```csharp
// IFileStorageService → MinioFileStorageService
// Upload: PutObjectAsync(bucket: "radiology", key: "2026/06/{orderId}/{fileName}")
// URL: MinIO presigned URL أو Nginx proxy لـ /radiology/{path}
// Max file size: 50MB (للـ CBCT) — تُضغَّط الأحجام الكبيرة اختيارياً
```

#### Radiology Commission Engine
```
عند status = 'completed':
  1. اقرأ commission_method + default_commission_value من radiology_technicians
  2. احسب العمولة على price
  3. أنشئ radiology_commission_records { is_paid: false }
```

### Frontend Tasks

**S-RAD-01: قائمة طلبات الأشعة**
- جدول فلترة بالنوع / الحالة / نوع المريض (داخلي/خارجي) / التاريخ
- Badge: 🔵 جديد / 🟡 جارٍ / 🟢 مكتمل / ⛔ ملغي

**S-RAD-02: إنشاء طلب أشعة**
- Radio buttons: داخلي / خارجي → يُغيّر Form تلقائياً
- داخلي: Select مريض من النظام
- خارجي: حقل اسم + هاتف + حقول دفع فوري (مبلغ + خزينة + طريقة)
- Select نوع الأشعة + الفني + الطبيب الطالب (اختياري)

**S-RAD-03: عارض الطلب + الصور**
- بيانات الطلب + Image Gallery (lightbox) للصور
- زر "رفع صور" (multi-file) → تُعرَض فوراً
- حقل ملاحظات الفني (report_notes) قابل للتعديل

**S-RAD-04: إحصاءات الأشعة**
- اليوم: داخليون / خارجيون / إجمالي إيرادات / محصَّل
- Chart شهري (Recharts BarChart)

**S-RAD-05: كشف حساب فني الأشعة**
- 4 KPIs + جدول الطلبات + Modal دفع العمولة

### Business Rules المُنفَّذة في Phase 7
- CONSTRAINT ck_radiology_patient: internal/external صارم
- external patient = دفع فوري → vault_transaction: radiology_income
- صور MinIO: لا تُحذف فيزيائياً (Soft Delete على radiology_images)
- عمولة الفني: Cash-Basis عند status=completed

### ✅ Definition of Done — Phase 7
```
□ طلب داخلي: يرتبط بالمريض + يُدرَج في فاتورته (أو يُدفع مستقلاً)
□ طلب خارجي: يُنشئ vault_transaction radiology_income فوراً
□ رفع 3 صور بتنسيقات مختلفة → تُعرَض في Gallery
□ CONSTRAINT ck_radiology_patient: external بدون patient_id → يُرفَض بـ DB
□ عمولة الفني تظهر في S-RAD-05 عند completed
□ صلاحية Radiology.Create: موظف بدونها لا يرى وحدة الأشعة
```

---

## 11. Phase 8 — Reports / Dashboards / PDF / Background Jobs

**الهدف:** كل التقارير التحليلية + لوحات التحكم + PDF احترافية عربية.  
**الاعتماد:** كل Phase السابقة (البيانات يجب أن تكون موجودة لاختبار التقارير)

### Backend Tasks

#### FastReport.NET Engine
```csharp
// ReportService.Generate(templateName, parameters) → byte[] PDF
// كل تقرير = ملف .frx template مُصمَّم مع خط Arabic Typesetting
// كل PDF: RTL كامل + Header بشعار العيادة + أرقام عربية
```

#### Reports Endpoints
```
GET /api/reports/financial/daily-vault?date=
GET /api/reports/financial/revenue?from=&to=&doctorId=&locationId=
GET /api/reports/financial/uncollected?from=&to=
GET /api/reports/financial/advance-payments?status=
GET /api/reports/patients/new?from=&to=
GET /api/reports/patients/active?from=&to=
GET /api/reports/inventory/consumption?from=&to=&doctorId=&itemId=
GET /api/reports/inventory/waste?from=&to=&reason=
GET /api/reports/inventory/expiry?within_days=
GET /api/reports/purchasing/supplier-comparison
GET /api/reports/cost-centers?centerId=&from=&to=
GET /api/reports/insurance/claims?companyId=&status=&from=&to=
GET /api/reports/admin/doctor-productivity?from=&to=&locationId=
GET /api/reports/admin/revenue-by-location?from=&to=
-- ★ تقارير المعمل والأشعة (مُضافة في Phase 8)
GET /api/reports/lab/orders?from=&to=&technicianId=&status=
GET /api/reports/lab/technician-account?technicianId=&from=&to=
GET /api/reports/radiology/orders?from=&to=&type=internal|external
GET /api/reports/radiology/revenue?from=&to=&patientType=

→ كل endpoint يُعيد JSON للعرض + {endpoint}/export يُعيد PDF أو Excel
```

#### Dashboard Data Endpoints
```
GET /api/dashboard/operational    ← لكل الأدوار (S01)
    → مرضى اليوم، انتظار حالي، مواعيد متبقية، آخر إشعارات، تنبيهات مخزون

GET /api/dashboard/executive      ← مدير النظام فقط (S02)
    → Revenue YTD/MTD/Today، مرضى جدد، top doctors، top services،
      uncollected، open claims، inventory alerts summary
```

#### Hangfire Background Jobs
```
DailyInventoryAlertJob     → 08:00 كل يوم → تنبيهات + SignalR push
MonthlyAuditPartitionJob   → 1 كل شهر → ينشئ Partition جديد لـ audit_logs
WeeklyExecutiveReportJob   → (اختياري) ملخص أسبوعي
```

### Frontend Tasks

**S01: لوحة التحكم اليومية**
- 6 KPI بطاقات: مرضى اليوم، انتظار الآن، مواعيد متبقية، تنبيهات مخزون، إيرادات اليوم (بصلاحية), أقساط متأخرة
- آخر 5 إشعارات + Shortcuts (+ مريض / + موعد / + سند)
- Real-time عبر SignalR

**S02: لوحة الإدارة التنفيذية**
- Filter الفترة (اليوم/أسبوع/شهر/مخصص)
- Row 1: 5 KPIs مالية
- Row 2: رسم خطي (إيرادات 30 يوم) + دائري (توزيع الخدمات) — **Recharts**
- Row 3: أفضل 5 أطباء + أفضل شركات تأمين
- Row 4: تنبيهات (غير محصَّل، مطالبات مفتوحة، مخزون)

**S46: مركز التقارير (Hub)**
- Grid of Cards مجمّعة بالفئة: مالية / مرضى / مخزون / مشتريات / مراكز تكلفة / تأمين / إدارية

**S47: عارض التقرير**
- Panel فلاتر ديناميكية + DataTable نتائج
- إجماليات أسفل الجدول + [تصدير PDF] [تصدير Excel]
- PDF Preview مدمج (iframe من FastReport)

**S62: تقرير الاستهلاك** — جدول + مخطط شريطي (Recharts)

### Business Rules المُنفَّذة في Phase 6
- BR-REP-01 (Reports.View / Reports.Export على كل Endpoint)
- BR-REP-02 (الطبيب يرى تقاريره فقط)
- BR-REP-03 (دقة عشريتين في كل الأرقام المالية)

### ✅ Definition of Done — Phase 6
```
□ كل التقارير: نتيجة PDF = نتيجة JSON (اختبار تطابق الأرقام)
□ PDF عربي: نص RTL صحيح + خط عربي + أرقام سليمة (لا Bidi artifacts)
□ Executive Dashboard: الأرقام تنعكس فوراً بعد أي عملية مالية
□ Reports.View بدون Export: الجدول يعمل + زر Export مخفي (PermissionGate)
□ Hangfire Jobs: يعمل + يُسجَّل في Hangfire Dashboard
□ طبيب يفتح تقرير إيرادات: يرى خدماته فقط (test بمستخدمين مختلفين)
```

---

## 12. Migration Strategy

```
001_initial_schema.sql       ← clinics, users, roles, permissions, refresh_tokens
002_settings_tables.sql      ← doctors, staff, services, treatment_locations, vaults
003_workflow_settings.sql    ← workflow_settings + Seed (all false)
004_permissions_seed.sql     ← 32 permissions
005_patients_appointments.sql← patients, appointments, queue_entries, dental_chart
006_clinical_tables.sql      ← treatment_plans, procedures, patient_documents
007_financial_tables.sql     ← invoices, payments, vault_transactions, commissions
008_installments.sql         ← installment_plans, installment_payments, advance_payments
009_insurance_tables.sql     ← insurance_companies, covered_services, claims
010_inventory_tables.sql     ← stock_items, stock_batches, stock_movements
011_purchasing_tables.sql    ← suppliers, purchase_requests, purchase_orders, invoices
012_audit_partitions.sql     ← audit_logs PARTITION BY RANGE + partitions 2026
013_views.sql                ← supplier_account_summary, doctor_account_summary, etc.
014_indexes.sql              ← كل الـ Performance Indexes
```

**أداة Migration:** `dotnet ef migrations` (EF Core 8)

---

## ⚠️ نقاط تحتاج توضيح

1. **Timeline الأسبوعية:** الخطة تُحدَّد Phases بدون أسابيع محددة. هل نحتاج خطة زمنية بالأسابيع لمتابعة التقدم؟

2. **Testing Strategy:** هل يُكتب Unit Tests + Integration Tests بالتوازي مع كل Phase؟ أم تُؤجَّل لمرحلة منفصلة قبل الإطلاق؟

3. **Lab/Radiology Module Scope:** المعمل والأشعة وحدات Core داخل نفس Monolith — لا Feature Flags، لا DLL منفصل.

4. **MinIO:** الملفات (صور الأشعة + ملفات المرضى) تُخزَّن في MinIO Object Storage داخل السيرفر المحلي.

---

*هذه الخطة تُكمِّل [06_API_CONTRACTS.md](06_API_CONTRACTS.md) بتحديد أولوية ومرحلة كل Endpoint.*  
*بنية النشر والخوادم المطلوبة → [08_DEPLOYMENT_ARCHITECTURE.md](08_DEPLOYMENT_ARCHITECTURE.md)*
