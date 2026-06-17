# 05 — Database Dictionary
# قاموس قاعدة البيانات — DentalERP

> **الإصدار:** V-Final | **التاريخ:** 2026-06-16 | **المرجع:** [04_ERD_FINAL.md](04_ERD_FINAL.md)

---

## اصطلاحات القاموس

| الرمز | المعنى |
|-------|--------|
| PK | Primary Key |
| FK | Foreign Key |
| NN | NOT NULL |
| UQ | UNIQUE |
| DEF | Default Value |
| IDX | مُفهرَس |
| ★ | جديد أو معدَّل في V2/V-Final |

---

## MODULE 1: IAM — المستخدمون والصلاحيات

### جدول: `users`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK, NN, DEF gen_random_uuid() | معرف فريد |
| `username` | VARCHAR(50) | NN, UQ | اسم المستخدم للدخول — حروف/أرقام/_ فقط |
| `email` | VARCHAR(150) | UQ, NULL | البريد الإلكتروني (اختياري) |
| `password_hash` | VARCHAR(255) | NN | BCrypt hash للكلمة السرية |
| `full_name` | VARCHAR(150) | NN | الاسم الكامل للعرض في الواجهة |
| `is_active` | BOOLEAN | NN, DEF true | false = لا يستطيع الدخول |
| `doctor_id` | UUID | FK → doctors(id), NULL | إذا كان المستخدم طبيباً — ربط الحسابات |
| `staff_id` | UUID | FK → staff(id), NULL | إذا كان المستخدم موظفاً إدارياً |
| `failed_attempts` | SMALLINT | NN, DEF 0 | عدد محاولات الدخول الفاشلة المتتالية |
| `locked_until` | TIMESTAMPTZ | NULL | حساب مقفل حتى هذا الوقت (بعد 5 محاولات) |
| `created_at` | TIMESTAMPTZ | NN, DEF now() | وقت الإنشاء |
| `created_by` | UUID | FK → users(id), NULL | من أنشأ السجل (NULL للمستخدم الأول) |
| `updated_at` | TIMESTAMPTZ | NULL | آخر تعديل |
| `updated_by` | UUID | FK → users(id), NULL | من عدّل آخر مرة |
| `deleted_at` | TIMESTAMPTZ | NULL | Soft Delete — NULL = موجود |
| `deleted_by` | UUID | FK → users(id), NULL | من حذف |

**قواعد:**
- `failed_attempts` يُصفَّر عند نجاح الدخول
- القفل: بعد 5 محاولات → `locked_until = now() + 15 minutes`
- لا يمكن حذف المستخدم الوحيد في النظام

---

### جدول: `roles`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK, NN | معرف الدور |
| `name` | VARCHAR(50) | NN, UQ | اسم الدور (مثال: مدير، طبيب، موظف استقبال) |
| `description` | VARCHAR(255) | NULL | وصف اختياري |
| `is_system` | BOOLEAN | NN, DEF false | true = دور افتراضي لا يمكن حذفه |
| `created_at` | TIMESTAMPTZ | NN | وقت الإنشاء |

**الأدوار الافتراضية (is_system = true):**
- `Admin` — كل الصلاحيات
- `Doctor` — Clinical + Invoice.Print + Patient.View
- `Receptionist` — Patient.Create/Edit + Appointments + Invoice.Create + Treasury.Add

---

### جدول: `permissions`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK, NN | معرف الصلاحية |
| `code` | VARCHAR(50) | NN, UQ | كود فريد (مثال: Invoice.Cancel) |
| `module` | VARCHAR(30) | NN | الوحدة المرتبطة (مثال: Invoicing) |
| `label` | VARCHAR(100) | NULL | وصف بالعربية للعرض في شاشة S51 |

**الـ 32 كود المعتمدة:** موثقة في [03_BUSINESS_RULES_FINAL.md](03_BUSINESS_RULES_FINAL.md) قسم §7

---

### جدول: `user_roles`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `user_id` | UUID | PK, FK → users(id) ON DELETE CASCADE | المستخدم |
| `role_id` | UUID | PK, FK → roles(id) ON DELETE CASCADE | الدور |

**ملاحظة:** مستخدم واحد يمكنه امتلاك عدة أدوار.

---

### جدول: `role_permissions`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `role_id` | UUID | PK, FK → roles(id) ON DELETE CASCADE | الدور |
| `permission_id` | UUID | PK, FK → permissions(id) ON DELETE CASCADE | الصلاحية |

---

### جدول: `refresh_tokens`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `user_id` | UUID | NN, FK → users(id) ON DELETE CASCADE | المستخدم |
| `token_hash` | VARCHAR(255) | NN, UQ | SHA-256 hash للـ token (لا يُخزَّن النص الحقيقي) |
| `expires_at` | TIMESTAMPTZ | NN | انتهاء الصلاحية (7 أيام من الإصدار) |
| `is_revoked` | BOOLEAN | NN, DEF false | إلغاء مبكر (Logout) |
| `created_at` | TIMESTAMPTZ | NN | وقت الإنشاء |
| `device_info` | VARCHAR(255) | NULL | معلومات الجهاز (User-Agent) |

---

### جدول: `workflow_settings` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `action_type` | VARCHAR(30) | NN, UQ, CHECK | نوع الإجراء: `procedure_edit` \| `procedure_delete` \| `invoice_cancel` |
| `requires_approval` | BOOLEAN | NN, DEF false | تفعيل سير الموافقة لهذا الإجراء |
| `updated_by` | UUID | FK → users(id), NULL | آخر من عدّل |
| `updated_at` | TIMESTAMPTZ | NN, DEF now() | وقت آخر تعديل |

---

## MODULE 2: Clinic — إعدادات العيادة

### جدول: `clinics`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف العيادة |
| `name` | VARCHAR(150) | NN | اسم العيادة (يظهر في الطباعة) |
| `logo_url` | VARCHAR(500) | NULL | مسار ملف الشعار |
| `address` | TEXT | NULL | العنوان الكامل |
| `phone` | VARCHAR(20) | NULL | رقم الهاتف الرئيسي |
| `email` | VARCHAR(150) | NULL | البريد الإلكتروني |
| `tax_number` | VARCHAR(50) | NULL | الرقم الضريبي |
| `currency_code` | CHAR(3) | NN, DEF 'LYD' | كود العملة (ISO 4217) |
| `working_hours` | JSONB | NULL | ساعات العمل لكل يوم — `{monday: {open, close, is_open}}` |
| `created_at` | TIMESTAMPTZ | NN | وقت الإنشاء |

**ملاحظة Single Tenant:** يحتوي الجدول دائماً على سجل واحد فقط.

---

### جدول: `specialties`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `name` | VARCHAR(100) | NN, UQ | اسم التخصص (مثال: تقويم، زراعة، تجميل) |
| `created_at` | TIMESTAMPTZ | NN | وقت الإنشاء |

---

### جدول: `doctors`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف الطبيب |
| `full_name` | VARCHAR(150) | NN | الاسم الكامل |
| `specialty_id` | UUID | FK → specialties(id), NULL | التخصص |
| `phone` | VARCHAR(20) | NULL | رقم الهاتف |
| `email` | VARCHAR(150) | NULL | البريد الإلكتروني |
| `license_number` | VARCHAR(50) | NULL | رقم الترخيص المهني |
| `commission_method` ★ | VARCHAR(30) | NN, DEF 'percentage_of_service', CHECK | طريقة احتساب العمولة: `percentage_of_service` \| `fixed_amount` \| `percentage_of_net_service` |
| `default_commission_value` ★ | NUMERIC(10,2) | NN, DEF 0 | القيمة الافتراضية (نسبة أو مبلغ) — تُطبَّق على كل الخدمات غير المحددة |
| `is_active` | BOOLEAN | NN, DEF true | false = لا يظهر في القوائم |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |
| `updated_at` | TIMESTAMPTZ | NULL | |
| `updated_by` | UUID | FK → users(id) | |
| `deleted_at` | TIMESTAMPTZ | NULL | Soft Delete |
| `deleted_by` | UUID | FK → users(id) | |

---

### جدول: `doctor_service_commissions` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `doctor_id` | UUID | NN, FK → doctors(id) | الطبيب |
| `service_id` | UUID | NN, FK → medical_services(id) | الخدمة المحددة |
| `commission_method` | VARCHAR(30) | NN, CHECK | طريقة العمولة (تغلب على default_commission_method) |
| `commission_value` | NUMERIC(10,2) | NN, CHECK >= 0 | القيمة المخصصة لهذه الخدمة |
| — | — | UNIQUE(doctor_id, service_id) | طبيب × خدمة مرة واحدة فقط |

**منطق الأولوية:** doctor_service_commissions (خدمة محددة) > doctors.default_commission_value

---

### جدول: `staff`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف الموظف |
| `full_name` | VARCHAR(150) | NN | الاسم الكامل |
| `role_title` | VARCHAR(100) | NULL | المسمى الوظيفي (مثال: موظف استقبال) |
| `phone` | VARCHAR(20) | NULL | رقم الهاتف |
| `base_salary` | NUMERIC(10,2) | NN, DEF 0 | الراتب الأساسي الشهري |
| `is_active` | BOOLEAN | NN, DEF true | |
| `created_at` | TIMESTAMPTZ | NN | |
| `deleted_at` | TIMESTAMPTZ | NULL | Soft Delete |
| `deleted_by` | UUID | FK → users(id) | |

---

### جدول: `service_categories`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `name` | VARCHAR(100) | NN, UQ | اسم التصنيف |
| `sort_order` | SMALLINT | NN, DEF 0 | ترتيب العرض في القوائم |
| `created_at` | TIMESTAMPTZ | NN | |

---

### جدول: `medical_services`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف الخدمة |
| `category_id` | UUID | FK → service_categories(id), NULL | تصنيف الخدمة |
| `name` | VARCHAR(200) | NN | اسم الخدمة بالعربية |
| `code` | VARCHAR(30) | UQ, NULL | رمز الخدمة (اختياري) |
| `price` | NUMERIC(10,2) | NN, CHECK >= 0 | السعر الأساسي |
| `has_inventory_tracking` ★ | BOOLEAN | NN, DEF false | true = تُخصَم مواد مخزون عند تأكيد الإجراء |
| `requires_images` | BOOLEAN | NN, DEF false | true = يُنبّه على إضافة صور قبل/بعد |
| `is_active` | BOOLEAN | NN, DEF true | |
| `created_at` | TIMESTAMPTZ | NN | |
| `updated_at` | TIMESTAMPTZ | NULL | |
| `updated_by` | UUID | FK → users(id) | |

---

### جدول: `service_default_materials` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `service_id` | UUID | NN, FK → medical_services(id) | الخدمة |
| `stock_item_id` | UUID | NN, FK → stock_items(id) | الصنف المستهلَك |
| `default_quantity` | NUMERIC(10,2) | NN, DEF 1, CHECK > 0 | الكمية الافتراضية |
| — | — | UNIQUE(service_id, stock_item_id) | |

**الاستخدام:** عند `POST /api/procedures/{id}/confirm` يُجلب هذا الجدول ويُنشأ حركات مخزون تلقائياً.

---

### جدول: `treatment_locations` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `clinic_id` | UUID | NN, FK → clinics(id) | العيادة |
| `parent_id` | UUID | FK → treatment_locations(id), NULL | الأب — NULL للعيادة نفسها |
| `level` | VARCHAR(10) | NN, CHECK | المستوى: `clinic` \| `room` \| `chair` |
| `name` | VARCHAR(100) | NN | اسم الموقع (مثال: غرفة 1، كرسي A) |
| `assigned_doctor_id` | UUID | FK → doctors(id), NULL | الطبيب الافتراضي لهذا الموقع |
| `is_active` | BOOLEAN | NN, DEF true | |
| `created_at` | TIMESTAMPTZ | NN | |

**الهيكل الهرمي:**
```
العيادة (clinic) [جذر — لا parent]
  └── غرفة 1 (room) [parent = العيادة]
        └── كرسي A (chair) [parent = غرفة 1]
        └── كرسي B (chair)
```
**قيد خاص:** `UNIQUE INDEX uq_single_clinic_location WHERE level = 'clinic'` — سجل عيادة واحد فقط.

---

### جدول: `vaults`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `name` | VARCHAR(100) | NN, UQ | اسم الخزينة (مثال: الخزينة الرئيسية) |
| `type` | VARCHAR(20) | NN, CHECK | النوع: `cash` \| `bank` \| `card` \| `pos` |
| `bank_name` | VARCHAR(100) | NULL | اسم البنك (للـ bank فقط) |
| `account_number` | VARCHAR(50) | NULL | رقم الحساب البنكي |
| `opening_balance` | NUMERIC(12,2) | NN, DEF 0 | الرصيد الافتتاحي |
| `is_active` | BOOLEAN | NN, DEF true | |
| `created_at` | TIMESTAMPTZ | NN | |

**الرصيد الحالي:** محسوب = opening_balance + SUM(in) - SUM(out) من vault_transactions

---

## MODULE 3: Patient — المرضى

### جدول: `patients`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف المريض |
| `mrn` | VARCHAR(20) | NN, UQ, IDX | رقم المريض الطبي — DEN-YYYY-XXXXX |
| `full_name` | VARCHAR(200) | NN, IDX | الاسم الرباعي |
| `phone` | VARCHAR(20) | NN, IDX | الجوال الرئيسي (يُستخدَم للبحث وفحص المكرر) |
| `phone2` | VARCHAR(20) | NULL | جوال بديل |
| `national_id` | VARCHAR(30) | NULL | الرقم الوطني |
| `date_of_birth` | DATE | NULL | تاريخ الميلاد (لحساب العمر) |
| `gender` | VARCHAR(10) | NULL, CHECK | `male` \| `female` |
| `address` | TEXT | NULL | العنوان |
| `blood_type` | VARCHAR(5) | NULL, CHECK | `A+` \| `A-` \| `B+` ... الخ |
| `allergies` | TEXT[] | NULL | قائمة مفصولة كـ Array: `['بنسلين', 'لاتكس']` |
| `chronic_diseases` | TEXT[] | NULL | الأمراض المزمنة |
| `emergency_contact` | JSONB | NULL | `{name, phone, relation}` |
| `referral_source` | VARCHAR(50) | NULL | مصدر الإحالة (مريض آخر، إعلان، وسائل التواصل...) |
| `notes` | TEXT | NULL | ملاحظات عامة |
| `created_at` | TIMESTAMPTZ | NN, DEF now() | |
| `created_by` | UUID | FK → users(id) | |
| `updated_at` | TIMESTAMPTZ | NULL | |
| `updated_by` | UUID | FK → users(id) | |
| `deleted_at` | TIMESTAMPTZ | NULL | Soft Delete |
| `deleted_by` | UUID | FK → users(id) | |

**منطق MRN:**
```
DEN-{YEAR}-{SEQUENCE_5_DIGITS}
مثال: DEN-2026-00001
```
Sequence مستقلة لكل سنة تبدأ من 1.

**فحص المكرر:** يُبحَث عن نفس `phone` + `full_name` قبل الإنشاء.

---

### جدول: `patient_doctor_assignments`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `patient_id` | UUID | NN, FK → patients(id) | المريض |
| `doctor_id` | UUID | NN, FK → doctors(id) | الطبيب |
| `status` | VARCHAR(20) | NN, DEF 'active', CHECK | `active` \| `closed` |
| `can_edit` | BOOLEAN | NN, DEF true | false = طبيب آخر أقفل الملف |
| `notes` | TEXT | NULL | سبب الإحالة أو الإسناد |
| `created_at` | TIMESTAMPTZ | NN | |
| `closed_at` | TIMESTAMPTZ | NULL | وقت الإغلاق |
| `closed_by` | UUID | FK → users(id) | من أغلق |
| — | — | UNIQUE(patient_id, doctor_id) | طبيب واحد لكل مريض مرة |

---

### جدول: `patient_documents`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `patient_id` | UUID | NN, FK → patients(id) | المريض |
| `procedure_id` | UUID | FK → procedures(id), NULL | الإجراء المرتبط (NULL = وثيقة عامة) |
| `type` | VARCHAR(20) | NN, CHECK | `before` \| `after` \| `xray` \| `opg` \| `cbct` \| `document` \| `other` |
| `file_url` | VARCHAR(500) | NN | المسار الكامل للملف (Local Storage أو URL) |
| `file_name` | VARCHAR(255) | NULL | اسم الملف الأصلي |
| `file_size` | INTEGER | NULL | الحجم بالـ Bytes |
| `mime_type` | VARCHAR(100) | NULL | نوع الملف (image/jpeg, application/pdf...) |
| `notes` | TEXT | NULL | وصف إضافي |
| `uploaded_by` | UUID | NN, FK → users(id) | من رفع الملف |
| `uploaded_at` | TIMESTAMPTZ | NN, DEF now() | وقت الرفع |

---

### جدول: `patient_insurance_links` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `patient_id` | UUID | NN, FK → patients(id) | المريض |
| `insurance_company_id` | UUID | NN, FK → insurance_companies(id) | شركة التأمين |
| `policy_number` | VARCHAR(50) | NULL | رقم البوليصة/التأمين |
| `is_active` | BOOLEAN | NN, DEF true | تفعيل/إيقاف |
| `created_at` | TIMESTAMPTZ | NN | |

---

## MODULE 4: Scheduling — المواعيد

### جدول: `appointments`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `patient_id` | UUID | NN, FK → patients(id) | المريض |
| `doctor_id` | UUID | NN, FK → doctors(id) | الطبيب |
| `treatment_location_id` | UUID | FK → treatment_locations(id), NULL | موقع العلاج |
| `scheduled_at` | TIMESTAMPTZ | NN | تاريخ ووقت الموعد |
| `duration_minutes` | SMALLINT | NN, DEF 30 | مدة الموعد المتوقعة |
| `type` | VARCHAR(30) | NN, DEF 'checkup', CHECK | `checkup` \| `followup` \| `procedure` \| `emergency` |
| `status` | VARCHAR(20) | NN, DEF 'scheduled', CHECK | `scheduled` \| `confirmed` \| `arrived` \| `in_progress` \| `completed` \| `cancelled` \| `no_show` |
| `notes` | TEXT | NULL | |
| `cancelled_reason` | TEXT | NULL | سبب الإلغاء |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |
| `updated_at` | TIMESTAMPTZ | NULL | |
| `updated_by` | UUID | FK → users(id) | |

---

### جدول: `queue_entries`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `patient_id` | UUID | NN, FK → patients(id) | المريض |
| `doctor_id` | UUID | NN, FK → doctors(id) | الطبيب |
| `appointment_id` | UUID | FK → appointments(id), NULL | الموعد المرتبط (NULL = Walk-in) |
| `queue_number` | SMALLINT | NN | رقم الطابور اليومي (يُعاد تسلسله كل يوم) |
| `status` | VARCHAR(20) | NN, DEF 'waiting', CHECK | `waiting` \| `called` \| `with_doctor` \| `done` \| `left` |
| `visit_type` | VARCHAR(30) | NULL | نوع الزيارة (يُنقَل من الموعد) |
| `checked_in_at` | TIMESTAMPTZ | NN, DEF now() | وقت تسجيل الحضور |
| `called_at` | TIMESTAMPTZ | NULL | وقت النداء للدخول |
| `done_at` | TIMESTAMPTZ | NULL | وقت انتهاء الكشف |
| `queue_date` | DATE | NN, DEF CURRENT_DATE | تاريخ اليوم (للفلترة) — IDX مع doctor_id |

---

## MODULE 5: Clinical — السريرية

### جدول: `dental_chart_entries`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `patient_id` | UUID | NN, FK → patients(id) | المريض |
| `tooth_number` | SMALLINT | NN, CHECK 11-85 | رقم السن — FDI notation |
| `surface` | VARCHAR(5) | NULL | السطح: `M`(Mesial) \| `D`(Distal) \| `B`(Buccal) \| `L`(Lingual) \| `O`(Occlusal) |
| `condition` | VARCHAR(30) | NN | الحالة: `healthy` \| `decay` \| `filling` \| `crown` \| `veneer` \| `missing` \| `extracted` \| `implant` \| `bridge` \| `rct` \| `fracture` |
| `mobility_grade` | SMALLINT | NULL, CHECK 0-3 | درجة الحركة (0=طبيعي، 3=شديد) |
| `notes` | TEXT | NULL | ملاحظات إضافية |
| `recorded_by` | UUID | NN, FK → users(id) | من سجل |
| `recorded_at` | TIMESTAMPTZ | NN, DEF now() | وقت التسجيل |

**FDI Range:**
- الأسنان الدائمة: 11-18, 21-28, 31-38, 41-48
- أسنان اللبن: 51-55, 61-65, 71-75, 81-85

---

### جدول: `treatment_plans`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `patient_id` | UUID | NN, FK → patients(id) | المريض |
| `doctor_id` | UUID | NN, FK → doctors(id) | الطبيب الواضع للخطة |
| `title` | VARCHAR(200) | NN | عنوان الخطة (مثال: خطة تقويم كاملة) |
| `status` | VARCHAR(20) | NN, DEF 'active', CHECK | `active` \| `completed` \| `cancelled` |
| `notes` | TEXT | NULL | ملاحظات الخطة |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |
| `updated_at` | TIMESTAMPTZ | NULL | |
| `updated_by` | UUID | FK → users(id) | |

---

### جدول: `treatment_plan_items`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `plan_id` | UUID | NN, FK → treatment_plans(id) | الخطة |
| `service_id` | UUID | NN, FK → medical_services(id) | الخدمة |
| `tooth_number` | SMALLINT | NULL | رقم السن المحدد |
| `estimated_price` | NUMERIC(10,2) | NN | السعر التقديري وقت الخطة |
| `status` | VARCHAR(20) | NN, DEF 'pending', CHECK | `pending` \| `in_progress` \| `completed` \| `skipped` |
| `sort_order` | SMALLINT | NN, DEF 0 | ترتيب التنفيذ |
| `notes` | TEXT | NULL | |

---

### جدول: `procedures`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `patient_id` | UUID | NN, FK → patients(id), IDX | المريض |
| `doctor_id` | UUID | NN, FK → doctors(id), IDX | الطبيب المنفذ |
| `service_id` | UUID | NN, FK → medical_services(id) | الخدمة |
| `treatment_plan_id` | UUID | FK → treatment_plans(id), NULL | الخطة العلاجية |
| `treatment_plan_item_id` | UUID | FK → treatment_plan_items(id), NULL | بند الخطة |
| `treatment_location_id` ★ | UUID | NN, FK → treatment_locations(id) | موقع العلاج (غرفة/كرسي) — إلزامي |
| `tooth_numbers` | SMALLINT[] | NULL | قائمة أسنان: `[16, 17]` |
| `base_price` | NUMERIC(10,2) | NN | سعر الخدمة وقت تسجيل الإجراء (Snapshot) |
| `discount_type` | VARCHAR(10) | NULL, CHECK | `percentage` \| `fixed` |
| `discount_value` | NUMERIC(10,2) | NN, DEF 0 | قيمة الخصم |
| `final_price` | NUMERIC(10,2) | NN | السعر النهائي بعد الخصم |
| `lab_cost` ★ | NUMERIC(10,2) | NN, DEF 0 | تكلفة المعمل الخارجي (لحساب percentage_of_net) |
| `clinical_notes` | TEXT | NULL | ملاحظات سريرية |
| `status` | VARCHAR(20) | NN, DEF 'draft', CHECK | `draft` \| `confirmed` \| `billed` \| `cancelled` |
| `confirmed_at` | TIMESTAMPTZ | NULL | وقت التأكيد |
| `confirmed_by` | UUID | FK → users(id) | من أكد |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |
| `updated_at` | TIMESTAMPTZ | NULL | |
| `updated_by` | UUID | FK → users(id) | |

**قواعد الحالة:**
- `draft` → `confirmed`: يُنشئ حركات مخزون تلقائياً (إن has_inventory_tracking = true)
- `confirmed` → `billed`: عند إضافة الإجراء لفاتورة
- `confirmed/billed` → `cancelled`: يحتاج approval (إن workflow ON)

---

### جدول: `approval_requests`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `request_type` | VARCHAR(30) | NN, CHECK | `procedure_edit` \| `procedure_delete` \| `invoice_cancel` |
| `entity_id` | UUID | NN | معرف الكيان المطلوب تعديله |
| `entity_type` | VARCHAR(30) | NN | نوع الكيان: `procedure` \| `invoice` |
| `reason` | TEXT | NN | سبب الطلب (مطلوب) |
| `change_data` | JSONB | NULL | التغييرات المطلوبة للـ edit (قبل/بعد) |
| `status` | VARCHAR(20) | NN, DEF 'pending', CHECK | `pending` \| `approved` \| `rejected` |
| `requested_by` | UUID | NN, FK → users(id) | مقدم الطلب |
| `reviewed_by` | UUID | FK → users(id), NULL | من راجع |
| `review_notes` | TEXT | NULL | ملاحظات المراجع |
| `requested_at` | TIMESTAMPTZ | NN, DEF now() | وقت الطلب |
| `reviewed_at` | TIMESTAMPTZ | NULL | وقت المراجعة |

---

## MODULE 6: Treasury — الخزينة

### جدول: `invoices`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `invoice_number` | VARCHAR(30) | NN, UQ | الرقم التسلسلي — INV-YYYY-XXXXXX |
| `patient_id` | UUID | NN, FK → patients(id), IDX | المريض |
| `doctor_id` | UUID | NN, FK → doctors(id) | الطبيب |
| `status` | VARCHAR(20) | NN, DEF 'draft', CHECK | `draft` \| `confirmed` \| `partially_paid` \| `paid` \| `cancelled` |
| `subtotal` | NUMERIC(12,2) | NN, DEF 0 | المجموع قبل الخصم |
| `discount_total` | NUMERIC(12,2) | NN, DEF 0 | إجمالي الخصومات |
| `total_amount` | NUMERIC(12,2) | NN, DEF 0 | الإجمالي النهائي |
| `paid_amount` | NUMERIC(12,2) | NN, DEF 0 | المدفوع فعلياً |
| `remaining` | NUMERIC(12,2) | GENERATED AS (total_amount - paid_amount) STORED | المتبقي — محسوب |
| `currency` | CHAR(3) | NN, DEF 'LYD' | |
| `notes` | TEXT | NULL | |
| `cancelled_reason` | TEXT | NULL | سبب الإلغاء (مطلوب عند الإلغاء) |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |
| `updated_at` | TIMESTAMPTZ | NULL | |
| `updated_by` | UUID | FK → users(id) | |

**قواعد:**
- `draft` فقط قابل للحذف الفيزيائي
- `confirmed` فصاعداً: Immutable — يُلغى فقط بـ Cancel + Reversal

---

### جدول: `invoice_items`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `invoice_id` | UUID | NN, FK → invoices(id) | الفاتورة |
| `procedure_id` | UUID | NN, FK → procedures(id) | الإجراء المصدر |
| `service_name` | VARCHAR(200) | NN | اسم الخدمة وقت الإنشاء (Snapshot — يحمي من تغيير الاسم لاحقاً) |
| `quantity` | SMALLINT | NN, DEF 1 | الكمية |
| `unit_price` | NUMERIC(10,2) | NN | السعر الفردي (من procedures.final_price) |
| `discount` | NUMERIC(10,2) | NN, DEF 0 | خصم إضافي على مستوى البند |
| `total` | NUMERIC(10,2) | NN | المجموع = (unit_price × quantity) - discount |

---

### جدول: `payments`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `invoice_id` | UUID | NN, FK → invoices(id) | الفاتورة |
| `vault_id` | UUID | NN, FK → vaults(id) | الخزينة |
| `amount` | NUMERIC(10,2) | NN, CHECK > 0 | المبلغ المدفوع |
| `payment_method` | VARCHAR(20) | NN, CHECK | `cash` \| `bank_transfer` \| `card` \| `pos` \| `cheque` |
| `reference_number` | VARCHAR(50) | NULL | رقم الشيك أو الحوالة |
| `notes` | TEXT | NULL | |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |

**تأثير:** كل payment يُطلق `PaymentReceivedEvent` → يُحسب Commission للطبيب

---

### جدول: `installment_plans`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `invoice_id` | UUID | NN, FK → invoices(id) | الفاتورة |
| `patient_id` | UUID | NN, FK → patients(id) | |
| `total_amount` | NUMERIC(10,2) | NN | إجمالي التقسيط |
| `installments_count` | SMALLINT | NN | عدد الأقساط |
| `notes` | TEXT | NULL | |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |

---

### جدول: `installment_payments`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `plan_id` | UUID | NN, FK → installment_plans(id) | خطة التقسيط |
| `installment_num` | SMALLINT | NN | رقم القسط (1، 2، 3...) |
| `due_date` | DATE | NN | تاريخ الاستحقاق |
| `amount` | NUMERIC(10,2) | NN | مبلغ القسط |
| `status` | VARCHAR(20) | NN, DEF 'pending', CHECK | `pending` \| `paid` \| `overdue` |
| `paid_at` | TIMESTAMPTZ | NULL | وقت السداد الفعلي |
| `vault_id` | UUID | FK → vaults(id), NULL | الخزينة عند السداد |
| `payment_method` | VARCHAR(20) | NULL | طريقة السداد |

---

### جدول: `advance_payments`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `patient_id` | UUID | NN, FK → patients(id) | المريض |
| `vault_id` | UUID | NN, FK → vaults(id) | |
| `amount` | NUMERIC(10,2) | NN, CHECK > 0 | الدفعة المقدمة الأصلية |
| `remaining` | NUMERIC(10,2) | NN | المتبقي (يُخصَم عند استخدامه في فاتورة) |
| `notes` | TEXT | NULL | |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |

---

### جدول: `vault_transactions` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `vault_id` | UUID | NN, FK → vaults(id), IDX | الخزينة |
| `transaction_type` | VARCHAR(30) | NN, CHECK | النوع — انظر قائمة الأنواع أدناه |
| `amount` | NUMERIC(12,2) | NN | المبلغ (دائماً موجب) |
| `direction` | CHAR(2) | NN, CHECK | `in` (دخول) \| `out` (خروج) |
| `related_invoice_id` | UUID | FK → invoices(id), NULL | الفاتورة المرتبطة |
| `related_patient_id` | UUID | FK → patients(id), NULL | |
| `related_supplier_id` ★ | UUID | FK → suppliers(id), NULL | المورد (للمدفوعات) |
| `related_doctor_id` ★ | UUID | FK → doctors(id), NULL | الطبيب (للعمولة) |
| `related_employee_id` ★ | UUID | FK → staff(id), NULL | الموظف (للرواتب) |
| `related_claim_id` ★ | UUID | FK → claims(id), NULL | مطالبة التأمين |
| `cost_center_id` ★ | UUID | FK → cost_centers(id), NULL | مركز التكلفة (إلزامي لـ general) |
| `reference_number` | VARCHAR(50) | NULL | رقم مرجعي |
| `notes` | TEXT | NULL | |
| `is_reversed` ★ | BOOLEAN | NN, DEF false | true = هذه الحركة مُعكوسة |
| `is_reversal` ★ | BOOLEAN | NN, DEF false | true = هذه حركة عكسية |
| `day_closure_id` | UUID | NULL | معرف إقفال اليوم المرتبط |
| `created_at` | TIMESTAMPTZ | NN, IDX DESC | |
| `created_by` | UUID | FK → users(id) | |

**أنواع transaction_type:**

| النوع | الوصف | direction |
|-------|-------|-----------|
| `receipt_from_patient` | قبض من مريض | in |
| `payment_to_patient` | رد للمريض | out |
| `receipt_from_supplier` | قبض من مورد | in |
| `payment_to_supplier` | دفع لمورد | out |
| `payment_to_doctor` | دفع عمولة طبيب | out |
| `payment_to_technician` | دفع للتقنيين | out |
| `payment_to_employee` | دفع للموظف | out |
| `general_receipt` | إيراد عام | in |
| `general_payment` | مصروف عام | out |
| `inter_vault_transfer` | تحويل بين خزائن | in/out |
| `salary` | صرف راتب | out |
| `commission` | صرف عمولة | out |

---

### جدول: `reverse_transaction_links` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `original_transaction_id` | UUID | NN, FK → vault_transactions(id) | الحركة الأصلية |
| `reverse_transaction_id` | UUID | NN, FK → vault_transactions(id) | الحركة العكسية |
| `corrected_transaction_id` | UUID | FK → vault_transactions(id), NULL | الحركة الصحيحة البديلة (إن وُجدت) |
| `reason` | TEXT | NN, CHECK LENGTH >= 10 | سبب العكس (لا يقل عن 10 أحرف) |
| `performed_by` | UUID | NN, FK → users(id) | من أجرى العكس |
| `created_at` | TIMESTAMPTZ | NN | |

---

### جدول: `day_closures`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `closure_date` | DATE | NN, UQ | تاريخ الإقفال (يوم واحد مرة) |
| `vault_snapshots` | JSONB | NN | لقطة أرصدة كل خزينة: `{vault_id: {opening, closing, in, out}}` |
| `total_receipts` | NUMERIC(12,2) | NN | إجمالي المقبوضات |
| `total_payments` | NUMERIC(12,2) | NN | إجمالي المدفوعات |
| `net_flow` | NUMERIC(12,2) | NN | الصافي = receipts - payments |
| `closed_by` | UUID | NN, FK → users(id) | من أقفل |
| `closed_at` | TIMESTAMPTZ | NN | |

---

### جدول: `commission_records`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `doctor_id` | UUID | NN, FK → doctors(id) | الطبيب |
| `procedure_id` | UUID | NN, FK → procedures(id) | الإجراء |
| `payment_id` | UUID | NN, FK → payments(id) | الدفعة المُحفِّزة (Cash-Basis) |
| `commission_method` | VARCHAR(30) | NN | الطريقة المستخدمة في الحساب |
| `base_amount` | NUMERIC(10,2) | NN | المبلغ الخاضع للحساب |
| `commission_rate` | NUMERIC(10,4) | NN | النسبة (0.2000 = 20%) أو القيمة الثابتة |
| `commission_amount` | NUMERIC(10,2) | NN | العمولة المحتسبة |
| `is_paid` | BOOLEAN | NN, DEF false | هل صُرفت؟ |
| `paid_at` | TIMESTAMPTZ | NULL | وقت الصرف |
| `vault_transaction_id` | UUID | FK → vault_transactions(id), NULL | سند الصرف |
| `created_at` | TIMESTAMPTZ | NN | |

**صيغة الحساب:**
- `percentage_of_service`: `final_price × rate / 100`
- `fixed_amount`: قيمة ثابتة مباشرة
- `percentage_of_net_service`: `(final_price - lab_cost) × rate / 100`

---

### جدول: `payroll_records` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `staff_id` | UUID | NN, FK → staff(id) | الموظف |
| `period_month` | DATE | NN | أول يوم الشهر: `2026-06-01` |
| `amount` | NUMERIC(10,2) | NN, CHECK > 0 | إجمالي الراتب المصروف |
| `vault_transaction_id` | UUID | FK → vault_transactions(id), NULL | سند الخزينة |
| `notes` | TEXT | NULL | |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |
| — | — | UNIQUE(staff_id, period_month) | موظف × شهر مرة واحدة |

---

### جدول: `patient_credit_notes`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `patient_id` | UUID | NN, FK → patients(id) | المريض |
| `invoice_id` | UUID | FK → invoices(id), NULL | الفاتورة المرتبطة |
| `amount` | NUMERIC(10,2) | NN, CHECK > 0 | مبلغ الإشعار |
| `reason` | TEXT | NN | السبب |
| `vault_transaction_id` | UUID | FK → vault_transactions(id), NULL | سند الرد |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |

---

## MODULE 7: Insurance — التأمين

### جدول: `insurance_companies` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `name` | VARCHAR(150) | NN | اسم شركة التأمين |
| `contract_number` | VARCHAR(50) | NULL | رقم العقد |
| `default_coverage_pct` | NUMERIC(5,2) | NN, DEF 0, CHECK 0-100 | نسبة التغطية الافتراضية لكل الخدمات |
| `price_increase_pct` ★ | NUMERIC(5,2) | DEF 0, CHECK >= 0 | نسبة الزيادة على السعر الأساسي |
| `price_discount_pct` ★ | NUMERIC(5,2) | DEF 0, CHECK >= 0 | نسبة الخصم على السعر الأساسي |
| `contact_person` | VARCHAR(100) | NULL | مسؤول التواصل |
| `phone` | VARCHAR(20) | NULL | |
| `is_active` | BOOLEAN | NN, DEF true | |
| `created_at` | TIMESTAMPTZ | NN | |
| — | — | CHECK: لا يجتمع price_increase_pct > 0 مع price_discount_pct > 0 | |

**هرمية التسعير (الأولوية تنازلياً):**
1. `insurance_covered_services.custom_price` (إن وُجد)
2. `medical_services.price × (1 + price_increase_pct/100)` أو `× (1 - price_discount_pct/100)`
3. `medical_services.price` (السعر الأساسي)

---

### جدول: `insurance_covered_services` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `insurance_company_id` | UUID | NN, FK → insurance_companies(id) | الشركة |
| `service_id` | UUID | NN, FK → medical_services(id) | الخدمة |
| `coverage_pct` | NUMERIC(5,2) | NN, CHECK 0-100 | نسبة التغطية لهذه الخدمة تحديداً |
| `coverage_cap` | NUMERIC(10,2) | NULL | الحد الأقصى بالمبلغ (NULL = بلا حد) |
| `custom_price` ★ | NUMERIC(10,2) | NULL | سعر مخصص لهذه الشركة (أعلى الأولويات) |
| — | — | UNIQUE(insurance_company_id, service_id) | |

---

### جدول: `claims` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `claim_number` | VARCHAR(30) | NN, UQ | رقم المطالبة — CLM-YYYY-XXXXX |
| `patient_id` | UUID | NN, FK → patients(id) | المريض |
| `procedure_id` | UUID | NN, FK → procedures(id) | الإجراء المطالَب عنه |
| `insurance_company_id` | UUID | NN, FK → insurance_companies(id) | شركة التأمين |
| `claim_amount` | NUMERIC(10,2) | NN, CHECK >= 0 | مبلغ المطالبة |
| `base_price_used` ★ | NUMERIC(10,2) | NN | السعر المُطبَّق وقت إنشاء المطالبة (Snapshot ثابت) |
| `collected_amount` | NUMERIC(10,2) | NN, DEF 0 | المبلغ المُحصَّل فعلياً |
| `status` | VARCHAR(20) | NN, DEF 'open', CHECK | `open` \| `partially_paid` \| `paid` \| `rejected` \| `partially_rejected` |
| `rejection_reason` | TEXT | NULL | سبب الرفض |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |

---

## MODULE 8: Inventory — المخزون

### جدول: `stock_items`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف الصنف |
| `name` | VARCHAR(200) | NN | اسم الصنف |
| `code` | VARCHAR(30) | UQ, NULL | رمز الصنف الداخلي |
| `barcode` | VARCHAR(50) | NULL | الباركود |
| `category` | VARCHAR(100) | NULL | التصنيف (مثال: مواد طب أسنان، مستهلكات) |
| `unit` | VARCHAR(20) | NN, DEF 'piece' | وحدة القياس: `piece` \| `box` \| `ml` \| `gram` |
| `minimum_threshold` | NUMERIC(10,2) | NN, DEF 0 | الحد الأدنى — تنبيه عند الوصول |
| `expiry_alert_days` | SMALLINT | NN, DEF 60 | التنبيه قبل انتهاء الصلاحية بهذه الأيام |
| `is_active` | BOOLEAN | NN, DEF true | |
| `created_at` | TIMESTAMPTZ | NN | |
| `updated_at` | TIMESTAMPTZ | NULL | |
| `updated_by` | UUID | FK → users(id) | |

---

### جدول: `stock_batches`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف الدُفعة |
| `stock_item_id` | UUID | NN, FK → stock_items(id), IDX FEFO | الصنف |
| `batch_number` | VARCHAR(50) | NULL | رقم الدُفعة من المورد |
| `expiry_date` | DATE | NULL | تاريخ الانتهاء (NULL = لا يتقادم) |
| `quantity_in` | NUMERIC(10,2) | NN | الكمية الواردة أصلاً |
| `current_quantity` | NUMERIC(10,2) | NN | الكمية المتبقية حالياً |
| `unit_cost` | NUMERIC(10,2) | NULL | تكلفة الوحدة عند الاستلام |
| `received_at` | TIMESTAMPTZ | NN, DEF now() | وقت الاستلام |
| `purchase_invoice_item_id` | UUID | NULL | معرف بند فاتورة الشراء المُولِّدة |

**FEFO Index:** `(stock_item_id, expiry_date ASC NULLS LAST) WHERE current_quantity > 0`

---

### جدول: `stock_movements` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `stock_item_id` | UUID | NN, FK → stock_items(id), IDX | الصنف |
| `batch_id` | UUID | FK → stock_batches(id), NULL | الدُفعة المحددة |
| `movement_type` ★ | VARCHAR(30) | NN, DEF 'consumption', CHECK | `consumption` \| `waste` \| `purchase_in` \| `manual_issue` \| `stock_take_adjustment` |
| `quantity` | NUMERIC(10,2) | NN | الكمية |
| `direction` | CHAR(3) | NN, CHECK | `in` \| `out` |
| `waste_reason` ★ | VARCHAR(20) | NULL, CHECK | `expired` \| `damaged` \| `lost` \| `broken` (مطلوب عند type=waste) |
| `procedure_id` ★ | UUID | FK → procedures(id), NULL | الإجراء المُسبِّب (للـ consumption) |
| `doctor_id` ★ | UUID | FK → doctors(id), NULL | الطبيب (للتحليل) |
| `treatment_location_id` ★ | UUID | FK → treatment_locations(id), NULL | الموقع (للتحليل) |
| `notes` | TEXT | NULL | |
| `performed_by` | UUID | NN, FK → users(id) | من نفّذ الحركة |
| `performed_at` | TIMESTAMPTZ | NN, DEF now() | وقت التنفيذ |

---

## MODULE 9: Purchasing — المشتريات

### جدول: `suppliers`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف المورد |
| `name` | VARCHAR(150) | NN | اسم المورد |
| `contact_person` | VARCHAR(100) | NULL | مسؤول التواصل |
| `phone` | VARCHAR(20) | NULL | |
| `email` | VARCHAR(150) | NULL | |
| `address` | TEXT | NULL | |
| `tax_number` | VARCHAR(50) | NULL | الرقم الضريبي |
| `opening_balance` ★ | NUMERIC(12,2) | NN, DEF 0 | رصيد افتتاحي (ذمة مفتوحة) |
| `is_active` | BOOLEAN | NN, DEF true | |
| `created_at` | TIMESTAMPTZ | NN | |
| `deleted_at` | TIMESTAMPTZ | NULL | Soft Delete |

---

### جدول: `purchase_requests`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `title` | VARCHAR(200) | NULL | عنوان الطلب |
| `status` | VARCHAR(20) | NN, DEF 'draft', CHECK | `draft` \| `submitted` \| `approved` \| `rejected` \| `converted` |
| `notes` | TEXT | NULL | |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |

---

### جدول: `purchase_request_items`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `request_id` | UUID | NN, FK → purchase_requests(id) | الطلب |
| `stock_item_id` | UUID | NN, FK → stock_items(id) | الصنف |
| `requested_quantity` | NUMERIC(10,2) | NN | الكمية المطلوبة |
| `notes` | TEXT | NULL | |

---

### جدول: `purchase_orders`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `request_id` | UUID | FK → purchase_requests(id), NULL | الطلب المحوَّل |
| `supplier_id` | UUID | NN, FK → suppliers(id) | المورد |
| `order_number` | VARCHAR(30) | NN, UQ | رقم أمر الشراء |
| `status` | VARCHAR(20) | NN, DEF 'pending', CHECK | `pending` \| `approved` \| `received` \| `partial` \| `cancelled` |
| `expected_date` | DATE | NULL | تاريخ التسليم المتوقع |
| `notes` | TEXT | NULL | |
| `approved_by` | UUID | FK → users(id), NULL | المعتمِد |
| `approved_at` | TIMESTAMPTZ | NULL | وقت الاعتماد |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |

---

### جدول: `purchase_order_items`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `order_id` | UUID | NN, FK → purchase_orders(id) | أمر الشراء |
| `stock_item_id` | UUID | NN, FK → stock_items(id) | الصنف |
| `ordered_quantity` | NUMERIC(10,2) | NN | الكمية المطلوبة |
| `unit_price` | NUMERIC(10,2) | NULL | السعر التقديري |
| `received_quantity` | NUMERIC(10,2) | NN, DEF 0 | الكمية المستلَمة فعلياً |

---

### جدول: `purchase_invoices`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `order_id` | UUID | FK → purchase_orders(id), NULL | |
| `supplier_id` | UUID | NN, FK → suppliers(id) | المورد |
| `invoice_number` | VARCHAR(50) | NULL | رقم فاتورة المورد |
| `total_amount` | NUMERIC(12,2) | NN | الإجمالي |
| `paid_amount` | NUMERIC(12,2) | NN, DEF 0 | المدفوع |
| `status` | VARCHAR(20) | NN, DEF 'unpaid', CHECK | `unpaid` \| `partial` \| `paid` |
| `invoice_date` | DATE | NN | تاريخ الفاتورة |
| `due_date` | DATE | NULL | تاريخ الاستحقاق |
| `notes` | TEXT | NULL | |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |

---

### جدول: `purchase_invoice_items`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `purchase_invoice_id` | UUID | NN, FK → purchase_invoices(id) | فاتورة الشراء |
| `stock_item_id` | UUID | NN, FK → stock_items(id) | الصنف |
| `quantity` | NUMERIC(10,2) | NN | الكمية |
| `unit_price` | NUMERIC(10,2) | NN | السعر الفردي |
| `total` | NUMERIC(10,2) | NN | الإجمالي |
| `batch_id` | UUID | FK → stock_batches(id), NULL | الدُفعة الناتجة عن الاستلام |

---

## MODULE 10: Expenses + External

### جدول: `cost_centers` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `name` | VARCHAR(100) | NN, UQ | اسم مركز التكلفة |
| `description` | TEXT | NULL | وصف |
| `is_active` | BOOLEAN | NN, DEF true | |
| `created_at` | TIMESTAMPTZ | NN | |

---

### جدول: `expense_vouchers`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `voucher_number` | VARCHAR(30) | NN, UQ | رقم سند الصرف |
| `cost_center_id` | UUID | NN, FK → cost_centers(id) | مركز التكلفة (إلزامي) |
| `vault_id` | UUID | NN, FK → vaults(id) | الخزينة |
| `amount` | NUMERIC(10,2) | NN, CHECK > 0 | المبلغ |
| `description` | VARCHAR(255) | NN | البيان |
| `expense_date` | DATE | NN, DEF CURRENT_DATE | تاريخ الصرف |
| `receipt_url` | VARCHAR(500) | NULL | مرفق الإيصال |
| `vault_transaction_id` | UUID | FK → vault_transactions(id), NULL | السند المرتبط |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |

---

### جدول: `external_customers` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `name` | VARCHAR(150) | NN | اسم الجهة |
| `type` | VARCHAR(20) | NN, CHECK | `service_provider` (مزود خدمة: معمل/أشعة) \| `referral_source` (جهة إحالة) |
| `contact_person` | VARCHAR(100) | NULL | مسؤول التواصل |
| `phone` | VARCHAR(20) | NULL | |
| `opening_balance` | NUMERIC(12,2) | NN, DEF 0 | رصيد افتتاحي |
| `is_active` | BOOLEAN | NN, DEF true | |
| `created_at` | TIMESTAMPTZ | NN | |

---

### جدول: `external_customer_transactions` ★

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `customer_id` | UUID | NN, FK → external_customers(id) | الجهة |
| `transaction_type` | VARCHAR(20) | NN, CHECK | `service` (خدمة مقدَّمة) \| `payment` (مدفوع للجهة) |
| `amount` | NUMERIC(10,2) | NN | المبلغ |
| `description` | VARCHAR(255) | NULL | البيان |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |

---

## MODULE 11: Laboratory — المعمل ★ Core V1

### جدول: `lab_order_types`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `name` | VARCHAR(100) | NN, UQ | اسم النوع: تركيبات / تقويم / أطقم / فينير / إلخ |
| `description` | TEXT | NULL | وصف اختياري |
| `is_active` | BOOLEAN | NN, DEF true | يمكن إخفاؤه من القوائم |
| `created_at` | TIMESTAMPTZ | NN | |

---

### جدول: `lab_technicians`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `full_name` | VARCHAR(150) | NN | الاسم الكامل للفني |
| `lab_name` | VARCHAR(150) | NULL | اسم المعمل الخارجي (إن كان مستقلاً) |
| `phone` | VARCHAR(20) | NULL | |
| `email` | VARCHAR(150) | NULL | |
| `specialty` | VARCHAR(100) | NULL | تخصص الفني: تركيبات / تقويم / إلخ |
| `commission_method` | VARCHAR(30) | NN, CHECK | `percentage_of_service` \| `fixed_amount` |
| `default_commission_value` | NUMERIC(10,2) | NN, DEF 0 | نسبة % أو مبلغ ثابت |
| `opening_balance` | NUMERIC(12,2) | NN, DEF 0 | الرصيد الافتتاحي (دَيْن سابق) |
| `is_active` | BOOLEAN | NN, DEF true | |
| `created_at` | TIMESTAMPTZ | NN | |
| `deleted_at` | TIMESTAMPTZ | NULL | Soft Delete |

---

### جدول: `lab_orders`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `order_number` | VARCHAR(30) | NN, UQ | رقم الأمر: LAB-2026-00001 (يُولَّد تلقائياً) |
| `patient_id` | UUID | NN, FK → patients(id) | المريض صاحب الشغل |
| `doctor_id` | UUID | NN, FK → doctors(id) | الطبيب طالب الشغل |
| `procedure_id` | UUID | FK → procedures(id), NULL | الإجراء المرتبط (اختياري) |
| `lab_technician_id` | UUID | FK → lab_technicians(id), NULL | الفني المنفذ |
| `lab_order_type_id` | UUID | NN, FK → lab_order_types(id) | نوع الشغل |
| `description` | TEXT | NULL | وصف تفصيلي للشغل المطلوب |
| `tooth_numbers` | JSONB | NULL | مصفوفة أرقام الأسنان: [11, 12, 21] |
| `status` | VARCHAR(20) | NN, DEF pending | `pending` \| `in_progress` \| `completed` \| `delivered` \| `cancelled` |
| `order_date` | DATE | NN, DEF CURRENT_DATE | تاريخ إرسال الأمر |
| `expected_date` | DATE | NULL | تاريخ التسليم المتوقع |
| `delivery_date` | DATE | NULL | تاريخ الاستلام الفعلي |
| `cost` | NUMERIC(12,2) | NN, DEF 0 | التكلفة للمعمل (ما يُدفع للفني) |
| `price` | NUMERIC(12,2) | NN, DEF 0 | السعر للمريض (الدخل) |
| `notes` | TEXT | NULL | ملاحظات |
| `created_at` | TIMESTAMPTZ | NN | |
| `deleted_at` | TIMESTAMPTZ | NULL | Soft Delete |

**قواعد مهمة:**
- `cost` = ما يُدفع للمعمل (مصروف) — يُسجَّل في vault_transactions بنوع `payment_to_lab`
- `price` = ما يُحصَّل من المريض (دخل) — يُدرَج في الفاتورة أو يُسجَّل مباشرة
- `status = delivered` = الشغل مكتمل ومُسلَّم للطبيب

---

### جدول: `lab_expense_categories`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `name` | VARCHAR(100) | NN, UQ | اسم الفئة: مواد / صيانة / إيجار معمل / إلخ |
| `created_at` | TIMESTAMPTZ | NN | |

---

### جدول: `lab_expenses`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `category_id` | UUID | NN, FK → lab_expense_categories(id) | الفئة |
| `amount` | NUMERIC(12,2) | NN, CHECK > 0 | المبلغ |
| `description` | TEXT | NULL | البيان |
| `expense_date` | DATE | NN, DEF CURRENT_DATE | تاريخ المصروف |
| `vault_transaction_id` | UUID | FK → vault_transactions(id), NULL | الحركة المرتبطة |
| `created_at` | TIMESTAMPTZ | NN | |
| `deleted_at` | TIMESTAMPTZ | NULL | Immutable — Soft Delete فقط |

---

### جدول: `lab_commission_records`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `lab_technician_id` | UUID | NN, FK → lab_technicians(id) | الفني |
| `lab_order_id` | UUID | NN, FK → lab_orders(id), UQ | أمر المعمل — عمولة واحدة لكل أمر |
| `commission_method` | VARCHAR(30) | NN | الطريقة المُطبَّقة وقت الحساب (Snapshot) |
| `base_amount` | NUMERIC(12,2) | NN | المبلغ الخاضع للحساب (price أو cost) |
| `commission_amount` | NUMERIC(12,2) | NN, DEF 0 | قيمة العمولة المحسوبة |
| `is_paid` | BOOLEAN | NN, DEF false | هل صُرفت العمولة؟ |
| `paid_at` | TIMESTAMPTZ | NULL | وقت الصرف |
| `paid_by` | UUID | FK → users(id), NULL | من صرف العمولة |
| `vault_transaction_id` | UUID | FK → vault_transactions(id), NULL | حركة الصرف |
| `created_at` | TIMESTAMPTZ | NN | |

---

## MODULE 12: Radiology — الأشعة ★ Core V1

### جدول: `radiology_types`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `name` | VARCHAR(100) | NN, UQ | اسم النوع: OPG / CBCT / Periapical / Bitewing / Panoramic |
| `description` | TEXT | NULL | وصف اختياري |
| `price` | NUMERIC(12,2) | NN, DEF 0 | السعر الافتراضي (قابل للتعديل في الأمر) |
| `is_active` | BOOLEAN | NN, DEF true | |
| `created_at` | TIMESTAMPTZ | NN | |

---

### جدول: `radiology_technicians`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `full_name` | VARCHAR(150) | NN | الاسم الكامل |
| `phone` | VARCHAR(20) | NULL | |
| `email` | VARCHAR(150) | NULL | |
| `commission_method` | VARCHAR(30) | NN, CHECK | `percentage_of_service` \| `fixed_amount` |
| `default_commission_value` | NUMERIC(10,2) | NN, DEF 0 | نسبة % أو مبلغ ثابت |
| `is_active` | BOOLEAN | NN, DEF true | |
| `created_at` | TIMESTAMPTZ | NN | |
| `deleted_at` | TIMESTAMPTZ | NULL | Soft Delete |

---

### جدول: `radiology_orders`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `order_number` | VARCHAR(30) | NN, UQ | رقم الطلب: RAD-2026-00001 |
| `patient_type` | VARCHAR(20) | NN, CHECK | `internal` (مريض النظام) \| `external` (مريض خارجي) |
| `patient_id` | UUID | FK → patients(id), NULL | NN إن internal — NULL إن external |
| `external_patient_name` | VARCHAR(150) | NULL | NN إن external — NULL إن internal |
| `external_patient_phone` | VARCHAR(20) | NULL | رقم هاتف المريض الخارجي |
| `referring_doctor_id` | UUID | FK → doctors(id), NULL | الطبيب الطالب للأشعة |
| `radiology_technician_id` | UUID | FK → radiology_technicians(id), NULL | الفني المنفذ |
| `radiology_type_id` | UUID | NN, FK → radiology_types(id) | نوع الأشعة |
| `procedure_id` | UUID | FK → procedures(id), NULL | الإجراء المرتبط (اختياري) |
| `status` | VARCHAR(20) | NN, DEF pending | `pending` \| `in_progress` \| `completed` \| `cancelled` |
| `order_date` | DATE | NN, DEF CURRENT_DATE | تاريخ الطلب |
| `price` | NUMERIC(12,2) | NN, DEF 0 | سعر الأشعة |
| `paid_amount` | NUMERIC(12,2) | NN, DEF 0 | المبلغ المدفوع (خاصة للخارجيين) |
| `payment_method` | VARCHAR(20) | CHECK, NULL | طريقة الدفع (للخارجيين) |
| `vault_id` | UUID | FK → vaults(id), NULL | الخزينة (للخارجيين) |
| `report_notes` | TEXT | NULL | ملاحظات الفني على نتيجة الأشعة |
| `notes` | TEXT | NULL | ملاحظات عامة |
| `created_at` | TIMESTAMPTZ | NN | |
| `deleted_at` | TIMESTAMPTZ | NULL | Soft Delete |

**CONSTRAINT:** `ck_radiology_patient`:
- إن `patient_type = 'internal'`: patient_id NOT NULL، external_patient_name IS NULL
- إن `patient_type = 'external'`: patient_id IS NULL، external_patient_name NOT NULL

**دورة الدفع:**
- **مريض داخلي:** يُدرَج في فاتورته الاعتيادية أو يُدفع مباشرة → vault_transaction نوع `radiology_income`
- **مريض خارجي:** يدفع نقداً فوراً → vault_transaction نوع `radiology_income`

---

### جدول: `radiology_images`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `radiology_order_id` | UUID | NN, FK → radiology_orders(id) | الطلب |
| `file_url` | VARCHAR(500) | NN | رابط الصورة في MinIO (Object Storage) |
| `file_name` | VARCHAR(255) | NN | اسم الملف الأصلي |
| `file_type` | VARCHAR(10) | NULL | jpg / png / dcm (DICOM) |
| `file_size_bytes` | INTEGER | NULL | حجم الملف |
| `sort_order` | SMALLINT | NN, DEF 0 | ترتيب عرض الصور |
| `created_at` | TIMESTAMPTZ | NN | |
| `created_by` | UUID | FK → users(id) | |

---

### جدول: `radiology_expense_categories`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `name` | VARCHAR(100) | NN, UQ | اسم الفئة: مستهلكات / صيانة أجهزة / إلخ |
| `created_at` | TIMESTAMPTZ | NN | |

---

### جدول: `radiology_expenses`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `category_id` | UUID | NN, FK → radiology_expense_categories(id) | الفئة |
| `amount` | NUMERIC(12,2) | NN, CHECK > 0 | المبلغ |
| `description` | TEXT | NULL | البيان |
| `expense_date` | DATE | NN, DEF CURRENT_DATE | |
| `vault_transaction_id` | UUID | FK → vault_transactions(id), NULL | الحركة المرتبطة |
| `created_at` | TIMESTAMPTZ | NN | |
| `deleted_at` | TIMESTAMPTZ | NULL | Immutable — Soft Delete فقط |

---

### جدول: `radiology_commission_records`

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | PK | معرف |
| `radiology_technician_id` | UUID | NN, FK → radiology_technicians(id) | الفني |
| `radiology_order_id` | UUID | NN, FK → radiology_orders(id), UQ | الطلب — عمولة واحدة لكل طلب |
| `commission_method` | VARCHAR(30) | NN | الطريقة وقت الحساب (Snapshot) |
| `base_amount` | NUMERIC(12,2) | NN | المبلغ الخاضع للحساب (price) |
| `commission_amount` | NUMERIC(12,2) | NN, DEF 0 | قيمة العمولة |
| `is_paid` | BOOLEAN | NN, DEF false | هل صُرفت؟ |
| `paid_at` | TIMESTAMPTZ | NULL | وقت الصرف |
| `paid_by` | UUID | FK → users(id), NULL | |
| `vault_transaction_id` | UUID | FK → vault_transactions(id), NULL | |
| `created_at` | TIMESTAMPTZ | NN | |

---

## MODULE 13: Audit

### جدول: `audit_logs` (مُقسَّم شهرياً)

| العمود | النوع | القيود | الوصف |
|--------|-------|--------|-------|
| `id` | UUID | NN | معرف (ليس PK على مستوى الـ Partition) |
| `module` | VARCHAR(30) | NN | الوحدة: `Patients` \| `Clinical` \| `Invoicing` \| `Treasury` \| ... |
| `entity_type` | VARCHAR(50) | NN | نوع الكيان: `invoice` \| `procedure` \| ... |
| `entity_id` | UUID | NN | معرف الكيان |
| `action` | VARCHAR(20) | NN, CHECK | `CREATE` \| `UPDATE` \| `DELETE` \| `CANCEL` \| `PRINT` \| `REVERSE` \| `LOGIN` \| `LOGOUT` |
| `old_values` | JSONB | NULL | الحالة السابقة (للـ UPDATE/DELETE) |
| `new_values` | JSONB | NULL | الحالة الجديدة (للـ CREATE/UPDATE) |
| `user_id` | UUID | NN | معرف المستخدم |
| `username` | VARCHAR(50) | NN | اسم المستخدم (Snapshot) |
| `ip_address` | INET | NULL | عنوان IP |
| `created_at` | TIMESTAMPTZ | NN | وقت الحدث |

**التقسيم:**
- `PARTITION BY RANGE (created_at)`
- Partition شهرية: `audit_logs_YYYY_MM`
- Hangfire Job يُنشئ Partition الشهر القادم في اليوم الأول من كل شهر

---

## ملخص إحصائيات المخطط

| الفئة | العدد |
|-------|-------|
| الجداول الأساسية | **56** (42 + 14 جديدة للمعمل والأشعة) |
| الـ Views المحسوبة | **6** (+2: lab_technician_account_summary, radiology_daily_summary) |
| الـ Indexes الأساسية | **17** (+3 للمعمل والأشعة) |
| الـ Partitions (audit_logs) | شهرية — ديناميكية |
| أنواع vault_transactions | **15** (+3: lab_income, payment_to_lab, radiology_income) |
| الصلاحيات | **40** (+8 للمعمل والأشعة) |
| أنواع stock_movements | 5 |
| الحالات الممكنة لـ invoices | 5 |
| الحالات الممكنة لـ procedures | 4 |
| حالات lab_orders | 5 (pending/in_progress/completed/delivered/cancelled) |
| حالات radiology_orders | 4 (pending/in_progress/completed/cancelled) |

---

## ⚠️ نقاط تحتاج توضيح

1. **dental_chart_entries — إدارة التاريخ:** الجدول الحالي يسجل حالة السن الحالية فقط. هل نحتاج تاريخاً كاملاً لكل تغيير؟ يتطلب جدول منفصل `dental_chart_history` إن كان مطلوباً.

2. **audit_logs — Soft Delete لجداول المرجعية:** هل يُحفَظ سجل audit للـ Soft Delete في جداول مثل `doctors` و`staff`؟ (يُفترض نعم — يُضاف من Application Layer)

3. **inter_vault_transfer — هل يحتاج جدولاً مستقلاً؟** حالياً: قيدان في vault_transactions بـ direction مختلف. هل نحتاج ربطاً مباشراً بينهما مثل reverse_transaction_links؟

4. **stock_batches.current_quantity — آلية التحديث:** يُحدَّث من Application Layer عند كل حركة. هل نستخدم Trigger في PostgreSQL بدلاً من ذلك لضمان الاتساق؟

5. **installment_payments — حالة overdue:** هل يُحوَّل تلقائياً بـ Hangfire Job (scheduled daily) أم يكون CHECK فقط عند القراءة؟

---

*هذا المستند يُكمل [04_ERD_FINAL.md](04_ERD_FINAL.md) بالوصف التفصيلي لكل حقل.*
*عقود API التي تستهلك هذه الجداول → [06_API_CONTRACTS.md](06_API_CONTRACTS.md)*
