# 03 — Business Rules Final
# وثيقة قواعد العمل النهائية — DentalERP

> **الإصدار:** V-Final | **التاريخ:** 2026-06-16 | **الحالة:** مرجع تنفيذي معتمد

---

## ⚠️ قرار معماري حاسم — Approval Workflow

**التعارض المُحلَّل:** وثيقة Business Rules V-Final تقول "الغاء Approval Workflow كلياً"، بينما خطة التطوير النهائية (الأحدث) تقول "اختياري، default=OFF، 3 أنواع فقط".

**القرار المعتمد (الأحدث):** خطة التطوير النهائية — **اختياري، يُضبط عبر `workflow_settings` table**.

```
Default = OFF لكل الأنواع الثلاثة
```

| الحالة | السلوك |
|--------|--------|
| workflow OFF (الافتراضي) | تنفيذ فوري + Audit Log إلزامي |
| workflow ON | ينشئ `approval_request` + إشعار للمدير |

---

## فهرس المحتويات

1. [قواعد المرضى (BR-PAT)](#1-قواعد-المرضى)
2. [قواعد وصول الأطباء (BR-DOC)](#2-قواعد-وصول-الأطباء)
3. [قواعد الفواتير (BR-INV-FIN)](#3-قواعد-الفواتير)
4. [قواعد الخزينة (BR-TRE)](#4-قواعد-الخزينة)
5. [قواعد الاقساط والدفعات المقدمة (BR-FIN)](#5-قواعد-مالية-عامة)
6. [محرك العمولات (BR-COMM)](#6-محرك-العمولات)
7. [قواعد التأمين (BR-INS)](#7-قواعد-التأمين)
8. [قواعد المخزون (BR-INV)](#8-قواعد-المخزون)
9. [قواعد الموافقات (BR-APR)](#9-قواعد-الموافقات)
10. [مراكز التكلفة (BR-CC)](#10-مراكز-التكلفة)
11. [مواقع العلاج (BR-LOC)](#11-مواقع-العلاج)
12. [قواعد التقارير (BR-REP)](#12-قواعد-التقارير)
13. [قواعد التدقيق والسلامة (BR-AUD)](#13-قواعد-التدقيق)
14. [نموذج الصلاحيات — 31 صلاحية (BR-PERM)](#14-نموذج-الصلاحيات)
15. [Workflow Settings — الضبط الاختياري](#15-workflow-settings)

---

## 1. قواعد المرضى

### BR-PAT-01 — كشف التكرار عند التسجيل

```
الشرط: تطابق (رقم الهاتف) OR (الاسم الكامل + تاريخ الميلاد)
الفعل: عرض تحذير + بطاقة المريض المطابق
       لا يُمنع التسجيل نهائياً — الاستقبال يقرر
الخيارات: [فتح الملف الموجود] [تسجيل كمريض جديد على أي حال]
```

**التنفيذ في API:**
```
GET /api/patients/check-duplicate?phone=...&name=...&dob=...
→ 200: { isDuplicate: false } | { isDuplicate: true, matchedPatient: {...} }
```

---

### BR-PAT-02 — توليد رقم الملف الطبي (MRN)

```
الصيغة:  DEN-{YYYY}-{Sequence 5 digits}
مثال:    DEN-2026-00001

التسلسل: يبدأ من 00001 مع كل سنة ميلادية جديدة (لا تراكم عبر السنوات)
```

**قيود صارمة:**
- MRN لا يُعاد توليده أبداً
- MRN لا يُعاد استخدامه حتى لو حُذف المريض (نادر)
- يُولَّد تلقائياً عند الحفظ، لا يدوياً

---

### BR-PAT-03 — ربط المريض بالطبيب (patient_doctor_assignments)

```
الحدث المُشغِّل: أول إجراء أو خطة علاج لطبيب على مريض
الفعل: إنشاء سجل assignment تلقائياً

الحالة الافتراضية:
  status:   'active'
  can_edit: true
```

---

### BR-PAT-04 — المريض المشترك بين أطباء متعددين

| ما يراه كل طبيب مُشارك | ما لا يراه |
|------------------------|-----------|
| Dental Chart الكامل (تنسيق سريري) | تفاصيل إجراءات طبيب آخر |
| الحساسيات، الأمراض المزمنة، فصيلة الدم | الأسعار وملاحظات طبيب آخر |
| التاريخ الطبي العام | المعلومات المالية لإجراءات طبيب آخر |

مريض واحد يمكن أن يمتلك عدة سجلات assignment (طبيب لكل تخصص أو خطة).

---

### BR-PAT-05 — قفل صلاحية الطبيب (can_edit = false)

**متى يحدث:**
- الطبيب يضغط "إنهاء حالتي" صريحاً في S12
- أو مدير يُغلق الـ assignment يدوياً من الإعدادات

**الأثر:**
```
الطبيب:  يبقى يرى السجل (read-only)
لا يمكن: إضافة/تعديل إجراءات أو خطط علاج جديدة لهذا الـ assignment
```

**إلغاء القفل:** يتطلب صلاحية "مدير النظام" صريحة فقط.

---

### BR-PAT-06 — حذف / أرشفة المريض

```
مريض له سجل مالي واحد على الأقل:
  → لا حذف فعلي → Soft Delete فقط (deleted_at, deleted_by)

مريض بلا أي سجل مالي/طبي:
  → حذف فعلي مسموح (خطأ تسجيل فوري فقط)
```

---

## 2. قواعد وصول الأطباء

### BR-DOC-01 — نطاق الرؤية الافتراضي

```
الطبيب يرى فقط:
  ✅ مرضاه (assignment نشط أو سابق له)
  ✅ بيانات المريض العامة (حساسية، دم، chart)

الطبيب لا يرى:
  ❌ قائمة كل مرضى العيادة
  ❌ فواتير غير مرضاه
  ❌ إجراءات طبيب آخر على نفس المريض بالتفصيل
```

---

### BR-DOC-02 — التاريخ المرضي العام مقابل الخاص

| النوع | من يراه |
|-------|--------|
| **عام** — حساسيات، أمراض مزمنة، فصيلة دم، Dental Chart الكامل | كل طبيب مُشارك |
| **خاص** — تفاصيل الإجراء، السعر، الصور، الملاحظات السريرية | الطبيب صاحب الـ assignment فقط |

---

### BR-DOC-03 — صلاحيات Dental Chart

| الحالة | الصلاحية |
|--------|---------|
| طبيب + assignment نشط (can_edit=true) | تعديل |
| طبيب + can_edit=false | عرض فقط |
| طبيب آخر بلا assignment | عرض فقط (يتطلب صلاحية خاصة من مدير لحالات نادرة) |

---

### BR-DOC-04 — كشف الحساب المالي للطبيب (S63)

```
كل طبيب: يرى كشف حسابه الخاص تلقائياً (بلا حاجة لصلاحية خاصة)
الطبيب:  لا يرى كشف حساب طبيب آخر حتى لو كان مديراً سريرياً
المدير + المحاسب فقط: يرون كشوف كل الأطباء
```

**التطبيق في API:**
```
GET /api/reports/doctor-statement
  → إن كان المستخدم طبيباً: WHERE doctor_id = current_user.doctor_id (forced)
  → إن كان مديراً/محاسباً: يقبل doctor_id كـ param
```

---

## 3. قواعد الفواتير

### BR-INV-FIN-01 — لا حظر مطلق على عمليات الفاتورة

```
كل عملية (إنشاء/تعديل/حذف/إلغاء/طباعة) تُنفَّذ فوراً إن توفّرت الصلاحية.
لا بوابة موافقة وسيطة (إلا إذا كان workflow_settings.requires_approval = true).
```

---

### BR-INV-FIN-02 — دورة حياة الفاتورة

```
draft → confirmed → partially_paid → paid
                 ↘
               cancelled  (حالة terminal — لا عودة)

قيود الانتقال:
  - لا انتقال من draft مباشرة إلى paid
  - draft يمكن حذفها فعلياً (لم يُسجَّل عليها دفع)
  - confirmed وما بعدها: Soft Cancel فقط
```

---

### BR-INV-FIN-03 — الحذف مقابل الإلغاء

| الإجراء | متى مسموح | الأثر |
|---------|-----------|-------|
| `Invoice.Delete` | فاتورة بحالة `draft` فقط (لم تُؤكَّد ولم يُدفع عليها) | حذف فعلي |
| `Invoice.Cancel` | أي حالة بعد draft | Soft (status=cancelled)، السجل يبقى |

**بعد أي دفعة واحدة: يُمنع الحذف الفعلي نهائياً.**

---

### BR-INV-FIN-04 — Audit إلزامي على كل عملية

يُسجَّل في `audit_logs`:
- المستخدم (user_id + username)
- التاريخ والوقت (TIMESTAMPTZ)
- نوع العملية (CREATE / UPDATE / DELETE / CANCEL / PRINT)
- القيمة القديمة (old_values JSONB كامل)
- القيمة الجديدة (new_values JSONB كامل)
- IP Address

حتى `Invoice.Print` يُسجَّل (من طبع؟ متى؟ أي فاتورة؟).

---

## 4. قواعد الخزينة

### BR-TRE-01 — صلاحيات الحركة المالية

```
Treasury.Add     — إضافة حركة جديدة
Treasury.Edit    — تعديل حركة (مقيّد زمنياً)
Treasury.Delete  — حذف حركة (مقيّد جداً)
Treasury.Reverse — عكس حركة (الخيار المحاسبي الرسمي)
Treasury.Print   — طباعة سند
```

تُمنح فردياً لكل دور من مدير النظام (S51).

---

### BR-TRE-02 — Treasury.Edit مقيّد زمنياً

```
مسموح:  خلال نفس يوم إنشاء الحركة وقبل إقفال اليوم (Day Closure)
بعد الإقفال: يُحظر Edit بالكامل → يبقى فقط Treasury.Reverse
```

---

### BR-TRE-03 — Treasury.Delete مقيّد جداً

```
مسموح فقط إذا:
  ✅ الحركة لم تُرحَّل لرصيد خزينة مُقفل
  ✅ لم يمرّ على الحركة أكثر من 24 ساعة

بعد أي من هذين الشرطين: يُحظر Delete → فقط Reverse
```

---

### BR-TRE-04 — ربط كل حركة خزينة بكيان مصدر

```
كل حركة خزينة MUST ترتبط بكيان مصدر واحد:
  استلام من مريض   → invoice / installment / advance_payment
  دفع لمريض        → credit_note / استرجاع
  دفع/استلام مورد  → supplier_invoice / purchase_invoice
  دفع طبيب          → commission_record / doctor_account
  دفع فني            → commission_record
  دفع موظف         → payroll_record
  استلام/صرف عام    → cost_center_id (إلزامي)
  تحويل بين خزائن  → vault → vault

لا حركة "عائمة" بلا ربط واضح.
```

---

## 5. قواعد مالية عامة

### BR-FIN-01 — الأقساط

```
القسط المتأخر (overdue):
  تاريخ الاستحقاق < اليوم AND status ≠ 'paid'

الدفعة المقدمة (advance):
  تُستخدم تلقائياً عند وجود فاتورة مستحقة الدفع (اختياري للمستخدم)
  أو يدوياً عبر "استخدام دفعة مقدمة" في S24a

استرجاع الدفعة المقدمة نقداً:
  فقط عبر "دفع لمريض" من S24b — يتطلب موافقة المدير
```

---

### BR-FIN-02 — عدم الحذف الفعلي (Immutability P4)

الجداول المحمية من physical delete:

```
invoices
payments
advance_payments
vault_transactions
expense_vouchers
purchase_invoices
commission_records
payroll_records
claims
```

**الإجراء البديل الوحيد:** Cancel/Void (حالة جديدة) **أو** Reverse Transaction.

---

### BR-FIN-03 — Reverse Transaction (القواعد الكاملة)

```
يُسمح بعكس حركة خزينة إذا:
  ✅ الحركة غير معكوسة مسبقاً
  ✅ ضمن النافذة الزمنية (افتراضي: 30 يوم من تاريخ الحركة)

بعد 30 يوم:
  يتطلب صلاحية "مدير النظام" صريحاً (تجاوز القيد)

ممنوع:
  ❌ عكس حركة سبق عكسها (لمنع حلقات متتالية)

إلزامي:
  سبب العكس (نص لا يقل عن 10 أحرف)

الأثر:
  الحركة الأصلية → حالة "معكوسة ↩️" (تبقى ظاهرة دائماً)
  حركة عكسية جديدة (مبلغ معكوس)
  كلتاهما مرتبطتان بـ original_ref مشترك
  3 سجلات في حالة إنشاء حركة بديلة صحيحة
```

---

### BR-FIN-04 — الحسابات المالية الموحّدة

```sql
-- معادلة موحّدة لكل أنواع الحسابات (مريض/مورد/عميل خارجي/طبيب)
الرصيد الحالي = الرصيد الافتتاحي + إجمالي المستحق - إجمالي المدفوع
```

**يُحسب Real-time (Computed View)** — لا يُخزَّن كحقل ثابت لتجنب عدم التزامن.

---

### BR-FIN-05 — الرواتب

```
راتب شهري ثابت لكل موظف
يُسجَّل كحركة "دفع موظف" مرة واحدة بالشهر
لا تكامل مع نظام حضور/انصراف (V2) — قيمة يدوية بالكامل
```

---

### BR-FIN-06 — الدقة المالية

```
المنازل العشرية: 2 (NUMERIC(15,2) في PostgreSQL)
طريقة التقريب: Round Half Up
العملة: دينار ليبي (LYD) — ثابتة، بلا تعدد عملات في V2
```

---

## 6. محرك العمولات

### BR-COMM-01 — طرق حساب العمولة (3 طرق)

#### الطريقة 1: Percentage From Service Amount
```
commission = service_price × commission_pct / 100
```

#### الطريقة 2: Fixed Amount
```
commission = fixed_value (ثابت لكل خدمة منفّذة — بصرف النظر عن السعر)
```

#### الطريقة 3: Percentage From Net Service
```
net_amount = service_price - lab_cost - material_cost
commission = net_amount × commission_pct / 100
```

---

### BR-COMM-02 — هرمية التخصيص (Override Hierarchy)

```
المستوى 1 (أعلى أولوية):
  doctor_service_commissions (تخصيص لطبيب × خدمة محددة)
  يتجاوز الافتراضي لهذه الخدمة وحدها

المستوى 2 (الافتراضي):
  doctors.commission_method + doctors.default_commission_value
  يُطبَّق على كل الخدمات بلا تخصيص
```

**في الكود:**
```csharp
var commission = await _db.DoctorServiceCommissions
    .FirstOrDefaultAsync(x => x.DoctorId == doctorId && x.ServiceId == serviceId)
    ?? new DoctorServiceCommission
    {
        CommissionMethod = doctor.CommissionMethod,
        CommissionValue = doctor.DefaultCommissionValue
    };
```

---

### BR-COMM-03 — Cash-Basis (لحظة الحساب)

```
العمولة تُحسب فقط عند:
  → استلام الدفعة فعلياً (PaymentReceivedEvent)
  → NOT عند إنشاء الفاتورة
  → NOT عند تسجيل الإجراء

النسبة تُطبَّق على المبلغ المُحصَّل فعلياً:
  إن دُفعت الفاتورة (500 د.ل) على دفعتين:
    300 د.ل الأولى → commission على 300
    200 د.ل الثانية → commission على 200

عند Reverse Transaction لدفعة:
  → تُعكَس العمولة المرتبطة تلقائياً
```

---

### BR-COMM-04 — Net Service وتكلفة المعمل

```
lab_cost مصدره:
  - procedures.lab_cost (حقل مسجَّل يدوياً)
  - أو مرتبط بـ purchase_invoice_item

إذا لم تُسجَّل تكلفة معمل:
  lab_cost = 0
  net_amount = service_price كاملاً
```

---

### BR-COMM-05 — قيد التضارب

```
commission_method IN ('percentage_of_service', 'fixed_amount', 'percentage_of_net_service')

لا يمكن الجمع بين طريقتين لنفس الطبيب لنفس الخدمة
UNIQUE(doctor_id, service_id) في doctor_service_commissions
```

---

## 7. قواعد التأمين

### BR-INS-01 — هرمية تسعير التأمين (3 مستويات)

```
المستوى 1 (أعلى أولوية): Custom Price
  insurance_covered_services.custom_price NOT NULL
  → يُستخدَم مباشرة بدون أي حساب

المستوى 2: نسبة خصم/زيادة على مستوى الشركة
  insurance_companies.price_discount_pct أو price_increase_pct
  → claim_base = original_price × (1 ± pct/100)

المستوى 3: السعر الأصلي
  لا قاعدة تسعير للشركة
  → claim_base = medical_services.price
```

**معادلة المطالبة:**
```
claim_base_price =
  COALESCE(custom_price,
    original_price * (1 + COALESCE(increase_pct,0)/100
                        - COALESCE(discount_pct,0)/100),
    original_price)

claim_amount = claim_base_price * coverage_pct / 100

-- تطبيق السقف:
IF coverage_cap IS NOT NULL AND claim_amount > coverage_cap
    claim_amount = coverage_cap
```

---

### BR-INS-02 — قيد التضارب (لا زيادة وخصم معاً)

```sql
ALTER TABLE insurance_companies ADD CONSTRAINT chk_no_both_increase_discount
    CHECK (NOT (price_increase_pct > 0 AND price_discount_pct > 0));
```

هذا القيد يُطبَّق على مستوى DB + Validation في طبقة التطبيق.

---

### BR-INS-03 — Snapshot السعر في المطالبة

```
claims.base_price_used = القيمة المحسوبة وقت الإنشاء (IMMUTABLE)
```

إذا تغيّرت إعدادات الشركة لاحقاً: المطالبات القديمة لا تتأثر.

---

### BR-INS-04 — إنشاء المطالبة التلقائي

```
الشروط الثلاثة لإنشاء مطالبة تلقائية عند تسجيل إجراء (S15):
  1. المريض مرتبط بشركة تأمين (patient_insurance_links)
  2. الخدمة مغطّاة (insurance_covered_services)
  3. Checkbox "مطالبة تأمين" مُفعَّل في S15 (ليس إلزامياً — اختياري)

claims.status الافتراضي عند الإنشاء: 'open'
```

---

### BR-INS-05 — تحصيل المطالبة

```
"تسجيل تحصيل من الشركة":
  → ينشئ vault_transaction نوع "استلام عام" مرتبط بـ claim_id
  → تحصيل جزئي مسموح (claim.status = 'partially_paid')
  → لا Workflow أو موافقة — يُنفَّذ فوراً بصلاحية Treasury.Add
```

---

### BR-INS-06 — رفض / تعديل مطالبة من الشركة

```
تسجيل يدوي فقط:
  claims.status = 'rejected' | 'partially_rejected'
  claims.rejection_reason (نص سبب الرفض)

لا تكامل EDI في V2 — يدوي بالكامل
```

---

## 8. قواعد المخزون

### BR-INV-01 — FEFO إلزامي (First Expired First Out)

```sql
-- كل صرف مخزون يستهلك الدُفعة الأقرب انتهاءً أولاً
SELECT * FROM stock_batches
WHERE stock_item_id = :item_id
  AND current_quantity > 0
  AND (expiry_date IS NULL OR expiry_date >= CURRENT_DATE)
ORDER BY expiry_date ASC NULLS LAST
LIMIT 1;
```

**استثناء:** اختيار دُفعة أخرى يدوياً يتطلب:
- صلاحية "مدير مخزون"
- سبب مسجَّل إلزامياً

---

### BR-INV-02 — التنبيهات الآلية

| النوع | الشرط | الأثر |
|-------|-------|-------|
| تحت الحد الأدنى | `current_quantity ≤ minimum_threshold` | تنبيه في S38 |
| قارب الانتهاء | `expiry_date - CURRENT_DATE ≤ 60 يوم` (قابل للتهيئة) | تنبيه في S38 |
| منتهي | `expiry_date < CURRENT_DATE` | يُحجب تلقائياً من الصرف |

---

### BR-INV-03 — المواد الافتراضية لكل خدمة (Auto-Deduction)

```
كل medical_service يمكن ربطها بـ service_default_materials:
  الخدمة "حشو ضوئي" → [Composite×1, Bond×1, Etch×1]

عند تأكيد الإجراء (S17 إرسال للخزينة):
  → تُخصَم تلقائياً جميع المواد الافتراضية (FEFO)
  → movement_type = 'consumption'
  → procedure_id = الإجراء المصدر

يمكن للطبيب تعديل الكميات الفعلية قبل التأكيد:
  مثال: استخدم 2 أنابيب Composite بدل 1
```

---

### BR-INV-04 — Consumption vs Waste (فصل صارم)

```
movement_type = 'consumption':
  - من إجراء طبي أو صرف يدوي
  - يحمل doctor_id + treatment_location_id
  - procedure_id إذا تلقائي
  - waste_reason = NULL (ممنوع)

movement_type = 'waste':
  - لا يرتبط بإجراء أو طبيب
  - waste_reason إلزامي: 'expired' | 'damaged' | 'lost' | 'broken'
  - يتطلب صلاحية Stock.Delete (أو Stock.Waste مستقبلاً)
  - Audit Log إلزامي (قيمة الفاقد قد تكون كبيرة)
```

**في التقارير:**
- S62 (استهلاك): WHERE movement_type = 'consumption' فقط
- تقرير التوالف: WHERE movement_type = 'waste' فقط

---

### BR-INV-05 — الجرد الدوري والتسويات

```
فرق الجرد (Variance) > 5% من الكمية النظرية:
  → يتطلب موافقة مدير مخزون قبل ترحيل التسوية للسجلات

movement_type = 'stock_take_adjustment' للتسويات
```

---

### BR-INV-06 — ربط الاستهلاك بالطبيب والموقع

```
صرف تلقائي: يحمل doctor_id + treatment_location_id من الإجراء المصدر (إلزامي)
صرف يدوي:   يطلب اختيارهما (اختياري لكن موصى به للتقارير)
```

---

## 9. قواعد الموافقات

### BR-APR-01 — الإجراءات الخاضعة للموافقة (3 أنواع فقط)

```
1. procedure_edit   — تعديل إجراء طبي
2. procedure_delete — حذف إجراء طبي
3. invoice_cancel   — إلغاء فاتورة

كل إجراء آخر (حركات الخزينة) → Reverse Transaction بلا موافقة
```

---

### BR-APR-02 — workflow_settings (المفتاح التحكمي)

```sql
CREATE TABLE workflow_settings (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    action_type   VARCHAR(30) NOT NULL UNIQUE
                  CHECK (action_type IN ('procedure_edit','procedure_delete','invoice_cancel')),
    requires_approval BOOLEAN NOT NULL DEFAULT false,  -- ← Default = OFF
    updated_by    UUID REFERENCES users(id),
    updated_at    TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Seed Data (الإدراج الأولي — كل الأنواع OFF)
INSERT INTO workflow_settings (action_type, requires_approval) VALUES
('procedure_edit',   false),
('procedure_delete', false),
('invoice_cancel',   false);
```

**المنطق في Handler:**
```csharp
var setting = await _db.WorkflowSettings
    .SingleAsync(x => x.ActionType == "invoice_cancel");

if (setting.RequiresApproval)
{
    // إنشاء approval_request + إشعار للمدير
    await _mediator.Send(new CreateApprovalRequestCommand { ... });
}
else
{
    // تنفيذ فوري + Audit Log
    invoice.Cancel(reason);
    await _auditLogger.LogAsync("Invoice", "Cancel", invoice.Id, userId);
}
```

---

### BR-APR-03 — من يملك صلاحية الموافقة

```
الدور المخوّل: مدير النظام (permission: approval.review)

Segregation of Duties (إلزامي):
  مقدّم الطلب لا يمكنه الموافقة على طلبه الخاص
  حتى لو كان مديراً — يجب مدير آخر
```

---

### BR-APR-04 — الإشعارات والمهل

```
عند إنشاء طلب:
  → إشعار فوري للمدير (SignalR + Badge يزداد في Sidebar)
  → حالة الكيان → "معلّق للموافقة" (badge أصفر)

تنبيه تذكير:
  إذا بقي الطلب معلّقاً > 24 ساعة → تنبيه يومي للمدير
  (Hangfire recurring job — يعمل كل يوم الساعة 8 صباحاً)

لا SLA صارم في V2 (لا انتهاء صلاحية تلقائي للطلب)

عند القرار (قبول/رفض):
  → إشعار فوري لمقدّم الطلب (نتيجة + سبب الرفض إن وُجد)
```

---

## 10. مراكز التكلفة

### BR-CC-01 — الربط الإلزامي

```
كل expense_voucher AND كل حركة خزينة من نوع "صرف عام":
  → MUST تحمل cost_center_id صالحاً وفعّالاً (is_active = true)

لا صرف "عائم" بلا تصنيف مركز تكلفة.
```

---

### BR-CC-02 — منع تعطيل مركز نشط

```
لا يمكن تعطيل مركز تكلفة له مصروفات في الشهر الجاري:
  → فحص: SELECT COUNT(*) FROM expense_vouchers
    WHERE cost_center_id = :id
      AND created_at >= DATE_TRUNC('month', NOW())
  → إذا COUNT > 0: يُرفض التعطيل مع رسالة واضحة
```

---

### BR-CC-03 — مراكز التكلفة الافتراضية

```
تُنشأ تلقائياً عند التهيئة الأولى (Seed):
  1. العيادة       — التشغيل العام
  2. التدريب       — مصروفات التدريب

تُضاف تلقائياً عند تفعيل وحدة:
  3. المعمل        — (عند تفعيل وحدة Laboratory)
  4. الأشعة        — (عند تفعيل وحدة Radiology)
```

---

## 11. مواقع العلاج

### BR-LOC-01 — إلزامية موقع العلاج

```
treatment_location_id: NOT NULL في جدول procedures
يُختار إلزامياً في S15 قبل إرسال الإجراء للخزينة
```

---

### BR-LOC-02 — البنية الهرمية (ثابتة)

```
Clinic (1 سجل فقط لكل قاعدة بيانات)
  └── Room (N)
        └── Chair (N)

قيد في DB:
CREATE UNIQUE INDEX uq_single_clinic_location
    ON treatment_locations (clinic_id)
    WHERE level = 'clinic';
-- يمنع وجود أكثر من كيان clinic واحد
```

---

### BR-LOC-03 — تعطيل الغرفة/الكرسي

```
Soft Disable فقط (is_active = false)
  → يمنع الاستخدام المستقبلي
  → السجلات التاريخية تبقى مرتبطة ولا تُحذف
  → التقارير التاريخية تبقى صحيحة
```

---

## 12. قواعد التقارير

### BR-REP-01 — صلاحيات الوصول حسب الفئة

| فئة التقرير | من يملك الوصول |
|------------|---------------|
| تقارير مالية شاملة | مدير، محاسب فقط |
| كشف حساب الطبيب الخاص | الطبيب نفسه + مدير + محاسب |
| تقارير المخزون/المشتريات | حسب صلاحية الوحدة المقابلة (Stock.*, Purchase.*) |
| التقرير التنفيذي | مدير فقط |

---

### BR-REP-02 — فلترة تلقائية حسب الدور

```
الطبيب يفتح أي تقرير:
  → يُفلتر تلقائياً: WHERE doctor_id = current_user.doctor_id
  → UI تُخفي خيار "الطبيب" لتغييره
  → API يرفض الطلب إذا حاول تغيير الفلتر
  (Double enforcement: UI + API)
```

---

### BR-REP-03 — الدقة والعملة

```
المنازل: 2 عشرية (NUMERIC(15,2))
التقريب: Round Half Up
العملة: LYD ثابتة — لا تعدد عملات في V2
```

---

### BR-REP-04 — التقارير الكبيرة (Async)

```
صغيرة (< 5,000 صف):  Sync — استجابة فورية (< 3 ثانية)
كبيرة (≥ 5,000 صف):  Hangfire background job
  → يُعاد HTTP 202 Accepted مع job_id
  → عند الاكتمال: SignalR يُبلّغ المستخدم ("التقرير جاهز")
  → ملف PDF/Excel يُحفظ في MinIO مؤقتاً
```

---

## 13. قواعد التدقيق

### BR-AUD-01 — ما يُسجَّل إلزامياً

| يُسجَّل | لا يُسجَّل |
|---------|-----------|
| كل Create/Update/Delete/Cancel على: invoices, payments, vault_transactions, procedures, patients, users, roles, permissions, claims | عمليات Read العادية |
| Print على الفواتير وكشوف الحسابات | تنقل الصفحات |
| Reverse Transaction | صفحة Dashboard (مجرد عرض) |
| Login/Logout/Failed Login | |

---

### BR-AUD-02 — الاحتفاظ بالبيانات

```
السجلات المالية:
  → Retention دائم (لا حذف أبداً)

audit_logs:
  → مُقسَّمة شهرياً (PARTITION BY RANGE created_at)
  → تُؤرشَف بعد 5 سنوات (لا تُحذف)
  → الأرشيف في MinIO (compressed JSON)
```

---

### BR-AUD-03 — السجلات المعكوسة

```
الحركة الأصلية المُعكوسة:
  → تبقى ظاهرة بحالة "معكوسة ↩️" في كل التقارير
  → رابط مباشر لحركة العكس المقابلة (reverse_transaction_ref)
  → لا يمكن إخفاؤها أو حذفها
```

---

## 14. نموذج الصلاحيات — 31 صلاحية

### كتالوج الصلاحيات الكامل

| الوحدة | الكود | الوصف |
|--------|-------|-------|
| **Patients (4)** | `Patient.Create` | تسجيل مريض جديد |
| | `Patient.Edit` | تعديل بيانات المريض |
| | `Patient.Delete` | حذف مريض (بشروط) |
| | `Patient.View` | عرض ملف المريض |
| **Clinical (5)** | `Procedure.Create` | تسجيل إجراء طبي |
| | `Procedure.Edit` | تعديل إجراء (+ workflow إن مُفعَّل) |
| | `Procedure.Delete` | حذف إجراء (+ workflow إن مُفعَّل) |
| | `TreatmentPlan.Create` | إنشاء خطة علاج |
| | `TreatmentPlan.Edit` | تعديل خطة علاج |
| **Invoicing (5)** | `Invoice.Create` | إنشاء فاتورة |
| | `Invoice.Edit` | تعديل فاتورة |
| | `Invoice.Delete` | حذف فاتورة draft فقط |
| | `Invoice.Cancel` | إلغاء فاتورة (+ workflow إن مُفعَّل) |
| | `Invoice.Print` | طباعة فاتورة |
| **Treasury (5)** | `Treasury.Add` | إضافة حركة خزينة |
| | `Treasury.Edit` | تعديل حركة (مقيّد زمنياً) |
| | `Treasury.Delete` | حذف حركة (مقيّد جداً) |
| | `Treasury.Reverse` | عكس حركة خزينة |
| | `Treasury.Print` | طباعة سند خزينة |
| **Inventory (4)** | `Stock.Add` | إضافة أصناف للمخزون |
| | `Stock.Edit` | تعديل بيانات صنف |
| | `Stock.Delete` | حذف/صرف مخزون (يشمل Waste) |
| | `Stock.Count` | تنفيذ جرد دوري |
| **Purchasing (4)** | `Purchase.Create` | إنشاء طلب/أمر شراء |
| | `Purchase.Edit` | تعديل طلب/أمر |
| | `Purchase.Delete` | حذف طلب/أمر |
| | `Purchase.Approve` | اعتماد أمر الشراء (Workflow الشراء) |
| **Reports (2)** | `Reports.View` | عرض التقارير |
| | `Reports.Export` | تصدير PDF/Excel |
| **Administration (3)** | `User.Manage` | إدارة المستخدمين |
| | `Role.Manage` | إدارة الأدوار والصلاحيات |
| | `System.Settings` | إعدادات النظام الكاملة |

**الإجمالي: 32 صلاحية** عبر 8 وحدات.

---

### BR-PERM-01 — Granular RBAC (مستقل تماماً)

```
كل Permission مستقل عن الآخر:
  منح Treasury.Add لا يمنح Treasury.Edit تلقائياً
  لا تجميع ضمني — كل صلاحية تُمنح فردياً
```

---

### BR-PERM-02 — Purchase.Approve ليس Approval Workflow

```
Workflow الشراء (طلب → أمر → فاتورة) يبقى كما هو:
  Purchase.Approve = اعتماد أمر شراء (Business Process طبيعي)
  ≠ نظام الموافقات الطبي/المالي (approval_requests)

هذا الفصل إلزامي ولا يُخلط بين الاثنين.
```

---

### BR-PERM-03 — تطبيق Permissions في ASP.NET Core

```csharp
// في Program.cs — تسجيل كل Permission كـ Policy
foreach (var permission in PermissionCatalog.All)
{
    options.AddPolicy(permission.Code, policy =>
        policy.Requirements.Add(new PermissionRequirement(permission.Code)));
}

// على Endpoint
[Authorize(Policy = "Invoice.Cancel")]
public async Task<IResult> CancelInvoice(...)

// في Handler — فحص Workflow بعد فحص Permission
```

---

### الأدوار الافتراضية المُقترحة

| الدور | الصلاحيات التقريبية |
|-------|-------------------|
| مدير النظام | كل الصلاحيات (32/32) |
| محاسب | Invoice.*, Treasury.*, Reports.*, Patient.View |
| موظف استقبال | Patient.Create/Edit/View، Scheduling.*, Invoice.Print |
| طبيب | Procedure.*, TreatmentPlan.*, Patient.View، Reports.View (محدود) |
| مخزن | Stock.*, Reports.View (محدود) |
| مشتريات | Purchase.*, Reports.View (محدود) |

---

## 15. Workflow Settings

### الضبط من S66 (إعدادات سير الموافقات)

```
الشاشة S66 تُظهر 3 toggles:
  [☐ تعديل إجراء طبي يتطلب موافقة]
  [☐ حذف إجراء طبي يتطلب موافقة]
  [☐ إلغاء الفاتورة يتطلب موافقة]

Default: كل الـ toggles = OFF
```

**API:**
```
GET  /api/settings/workflow      → قراءة الإعدادات الحالية
PUT  /api/settings/workflow/:type → تغيير toggle
  Permission: System.Settings
  Audit: تُسجَّل في audit_logs
```

---

### السلوك عند تفعيل Workflow لنوع معين

```
قبل أي تنفيذ لـ procedure_edit/delete أو invoice_cancel:
  1. API يقرأ workflow_settings
  2. إذا requires_approval = true:
     a. ينشئ approval_request بحالة 'pending'
     b. يُرجع HTTP 202 Accepted (لا تنفيذ فوري)
     c. يُرسل إشعار للمدير عبر SignalR
  3. إذا requires_approval = false:
     a. تنفيذ فوري
     b. تسجيل في audit_logs
     c. HTTP 200 OK
```

---

## ⚠️ نقاط تحتاج توضيح

1. **BR-PAT-06 — حذف المريض:** الوثيقة تقول "مريض بلا أي سجل مالي/طبي يمكن حذفه فعلياً" — هل يشمل "السجل الطبي" مجرد وجود appointment بلا إجراء؟ يحتاج تحديد حدود "السجل الطبي".

2. **BR-FIN-03 — النافذة الزمنية للـ Reverse:** "افتراضياً 30 يوم" — هل هذا القيد قابل للتهيئة من S66 أو إعدادات النظام؟ أم ثابت في الكود؟

3. **BR-AUD-01 — تسجيل Print:** هل "طباعة" تشمل "معاينة PDF" (بدون طباعة فعلية)؟ يؤثر على كثافة سجل التدقيق.

4. **BR-COMM-03 — Partial Payment:** عند دفع جزء من فاتورة متعددة الأطباء (فاتورة تجمع إجراءات لأطباء مختلفين) — كيف يُوزَّع المبلغ المدفوع على الإجراءات؟ FIFO للإجراءات؟ أم نسبي؟

5. **BR-INV-04 — Waste بدون صلاحية خاصة:** الوثيقة تقترح "Stock.Waste مستقبلاً" — هل نُضيفها الآن في V2 أم نستخدم Stock.Delete مؤقتاً؟

6. **BR-APR-04 — Segregation of Duties:** إذا كانت العيادة تمتلك مديراً واحداً فقط وهو نفسه طالب الموافقة — كيف يُحل هذا؟ هل يُعطَّل الـ Approval Workflow تلقائياً في هذه الحالة؟

---

*تفاصيل ERD وDDL الكاملة → [04_ERD_FINAL.md](04_ERD_FINAL.md)*
*قاموس قاعدة البيانات → [05_DATABASE_DICTIONARY.md](05_DATABASE_DICTIONARY.md)*
*عقود API لكل صلاحية → [06_API_CONTRACTS.md](06_API_CONTRACTS.md)*
