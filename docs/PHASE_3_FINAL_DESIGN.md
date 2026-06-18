# PHASE 3 FINAL DESIGN — DentalERP

> **التاريخ:** 2026-06-17 | **الحالة:** ✅ APPROVED — Implementation Started
> **يشمل:** كل تعديلات مراجعة PHASE_3_DESIGN_REVIEW + التعديلات الخمسة النهائية

---

## قرار تصميمي رئيسي: Dental Chart History

### الخياران

**الخيار أ — جدول واحد مع `is_current`**
```
dental_chart_entries
  id, patient_id, tooth_id, condition, is_current, recorded_at
```
الحالة الراهنة = `WHERE is_current = TRUE`
التاريخ الكامل = كل الصفوف

**الخيار ب — جدولان منفصلان**
```
dental_chart_entries   ← الحالة الراهنة فقط
dental_chart_history   ← نسخ قديمة منقولة من الجدول الأول
```

---

### ✅ القرار: الخيار أ — جدول واحد مع `is_current`

**لماذا؟**

| الحجة | التفسير |
|-------|---------|
| **لا ازدواجية** | في الخيار ب: البيانات تُنسَخ من جدول إلى آخر — إذا نسي المبرمج النقل يختفي التاريخ |
| **كل صف هو حالة تاريخية** | dental_chart_entries نفسها هي الـ audit trail |
| **استعلام بسيط** | `WHERE patient_id = X AND is_current = TRUE` للحالة الراهنة |
| **لا sync overhead** | لا transaction مزدوجة، لا trigger |
| **مُتّسق مع مبدأ append-only** | نفس نهج `audit_logs` التي أثبت نجاحها |
| **المستقبل** | Partitioning بالسنة على `recorded_at` إذا كبر الجدول |

**كيف يعمل؟**

```sql
-- عند تسجيل حالة جديدة للسن 16:
BEGIN;
UPDATE dental_chart_entries
   SET is_current = FALSE
 WHERE patient_id = :pid AND tooth_id = 16 AND is_current = TRUE;

INSERT INTO dental_chart_entries
   (patient_id, tooth_id, condition, ..., is_current)
VALUES (:pid, 16, 'Filled', ..., TRUE);
COMMIT;
```

---

## 1. جدول الأسنان — `teeth` (مُحدَّث: يشمل اللبنية)

### ترقيم FDI الكامل

```
الدائمة:                    اللبنية (الأطفال):
┌─────┬─────┐               ┌─────┬─────┐
│ 18-11 │ 21-28 │           │ 55-51 │ 61-65 │
├─────┼─────┤               ├─────┼─────┤
│ 48-41 │ 31-38 │           │ 85-81 │ 71-75 │
└─────┴─────┘               └─────┴─────┘
المجموع: 32 سن               المجموع: 20 سن
```

```sql
CREATE TABLE teeth (
    id               SMALLINT PRIMARY KEY,
    fdi_number       SMALLINT NOT NULL UNIQUE,
    universal_number SMALLINT,
    name_ar          VARCHAR(50) NOT NULL,
    name_en          VARCHAR(50) NOT NULL,
    jaw              VARCHAR(10) NOT NULL CHECK (jaw IN ('Upper','Lower')),
    side             VARCHAR(10) NOT NULL CHECK (side IN ('Right','Left')),
    tooth_type       VARCHAR(20) NOT NULL,
        -- Incisor, Canine, Premolar, Molar
    is_primary       BOOLEAN NOT NULL DEFAULT FALSE,
        -- FALSE = دائمة | TRUE = لبنية (أطفال)
    position         SMALLINT NOT NULL
        -- الترتيب من المنتصف: 1-8 للدائمة، 1-5 للبنية
);
```

### بيانات الأسنان اللبنية (Seed)

| FDI | الاسم (عر) | الاسم (en) | الفك | الجهة |
|-----|------------|------------|------|-------|
| 51 | القاطع المركزي العلوي الأيمن | Upper Right Central Incisor | Upper | Right |
| 52 | القاطع الجانبي العلوي الأيمن | Upper Right Lateral Incisor | Upper | Right |
| 53 | الناب العلوي الأيمن | Upper Right Canine | Upper | Right |
| 54 | الضاحك الأول العلوي الأيمن | Upper Right First Molar | Upper | Right |
| 55 | الضاحك الثاني العلوي الأيمن | Upper Right Second Molar | Upper | Right |
| 61-65 | (مرايا الجانب الأيسر العلوي) | Upper Left | Upper | Left |
| 71-75 | (الفك السفلي الأيسر) | Lower Left | Lower | Left |
| 81-85 | (الفك السفلي الأيمن) | Lower Right | Lower | Right |

**المجموع الكلي: 52 سن (32 دائمة + 20 لبنية)**

---

## 2. مخطط الأسنان — `dental_chart_entries` (مع قرار `is_current`)

```sql
CREATE TABLE dental_chart_entries (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID        NOT NULL REFERENCES patients(id),
    tooth_id        SMALLINT    NOT NULL REFERENCES teeth(id),
    surface         VARCHAR(20),
        -- NULL=كامل, M=Mesial, D=Distal, B=Buccal, L=Lingual, O=Occlusal
    condition       VARCHAR(50) NOT NULL
        CHECK (condition IN (
            'Healthy','Caries','Filled','Missing','Extracted',
            'Crown','Bridge','Implant','RootCanal','Fracture',
            'Impacted','Sensitive','Mobility','Other'
        )),
    severity        VARCHAR(10) CHECK (severity IN ('Mild','Moderate','Severe')),
    notes           TEXT,
    recorded_by_id  UUID        NOT NULL REFERENCES users(id),
    recorded_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    appointment_id  UUID        REFERENCES appointments(id),

    -- يُمثّل الحالة الحالية للسن
    -- عند تحديث الحالة: is_current للصف السابق → FALSE
    is_current      BOOLEAN     NOT NULL DEFAULT TRUE
);

CREATE INDEX ix_chart_patient_current ON dental_chart_entries(patient_id)
    WHERE is_current = TRUE;
CREATE INDEX ix_chart_patient_history  ON dental_chart_entries(patient_id, recorded_at DESC);
CREATE INDEX ix_chart_tooth            ON dental_chart_entries(patient_id, tooth_id, is_current);
CREATE INDEX ix_chart_appointment      ON dental_chart_entries(appointment_id);
```

### استعلامات جاهزة

```sql
-- الحالة الراهنة لكل أسنان مريض
SELECT * FROM dental_chart_entries
WHERE patient_id = :id AND is_current = TRUE
ORDER BY tooth_id;

-- تاريخ سن معين
SELECT * FROM dental_chart_entries
WHERE patient_id = :id AND tooth_id = 16
ORDER BY recorded_at DESC;
```

---

## 3. Doctor Assignments — دورة حياة كاملة

### المشكلة التي يحلّها هذا التصميم

```
د. أحمد يبدأ علاج المريض (Active)
  ↓
يُحيل المريض إلى د. خالد (Transferred)
  ↓
د. أحمد: can_view = TRUE | can_edit = FALSE
د. خالد: can_view = TRUE | can_edit = TRUE  (Active)
```

```sql
CREATE TABLE doctor_assignments (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID        NOT NULL REFERENCES patients(id),
    doctor_id       UUID        NOT NULL REFERENCES users(id),

    -- دورة حياة التعيين
    status          VARCHAR(20) NOT NULL DEFAULT 'Active'
        CHECK (status IN ('Active','Completed','Transferred','Closed')),
    assigned_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ended_at        TIMESTAMPTZ,        -- عند انتقال لحالة غير Active

    -- صلاحيات صريحة بعد انتهاء الدور
    can_view        BOOLEAN     NOT NULL DEFAULT TRUE,
    can_edit        BOOLEAN     NOT NULL DEFAULT TRUE,

    -- من حوَّل (إذا كانت Transferred)
    transferred_to_id UUID      REFERENCES users(id),
    transferred_at  TIMESTAMPTZ,
    transfer_reason TEXT,

    is_primary      BOOLEAN     NOT NULL DEFAULT FALSE,
    notes           TEXT,
    assigned_by_id  UUID        REFERENCES users(id),

    -- لا UNIQUE على الإطلاق — نفس الطبيب يمكنه معالجة نفس المريض مرات متعددة
    -- التحكم عبر Application Logic: لا يُسمح بـ Active جديد إذا يوجد Active للطبيب+المريض
);

CREATE INDEX ix_assignment_patient        ON doctor_assignments(patient_id);
CREATE INDEX ix_assignment_doctor_active  ON doctor_assignments(doctor_id)
    WHERE status = 'Active';
```

### قواعد العمل

| الحالة | can_edit | can_view | المعنى |
|--------|----------|----------|--------|
| `Active` | ✅ TRUE | ✅ TRUE | الطبيب يعالج المريض حالياً |
| `Transferred` | ❌ FALSE | ✅ TRUE | حوَّل المريض — يرى فقط |
| `Completed` | ❌ FALSE | ✅ TRUE | انتهى العلاج |
| `Closed` | ❌ FALSE | ❌ FALSE | أُغلق الملف بالكامل |

### دورة الحياة (State Machine)

```
Active → Transferred  (Doctor.Transfer command)
Active → Completed    (TreatmentPlan completed)
Active → Closed       (Admin only)
Transferred → Closed  (Admin only)
Completed → Closed    (Admin only)
```

### عند التحويل

```sql
-- خطوتان في transaction واحدة:
BEGIN;
-- 1. أغلق تعيين الطبيب الأول
UPDATE doctor_assignments
   SET status = 'Transferred',
       can_edit = FALSE,
       ended_at = NOW(),
       transferred_to_id = :new_doctor_id,
       transferred_at = NOW(),
       transfer_reason = :reason
 WHERE patient_id = :pid AND doctor_id = :old_doctor_id;

-- 2. أنشئ تعيين للطبيب الجديد
INSERT INTO doctor_assignments
    (patient_id, doctor_id, status, can_view, can_edit, is_primary, assigned_by_id)
VALUES (:pid, :new_doctor_id, 'Active', TRUE, TRUE, TRUE, :admin_id);
COMMIT;
```

---

## 4. مكتبة الوسائط — أنواع مُحدَّثة

### أنواع الوسائط الجديدة

```sql
CREATE TABLE patient_media (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID        NOT NULL REFERENCES patients(id),
    appointment_id  UUID        REFERENCES appointments(id),

    media_type      VARCHAR(20) NOT NULL
        CHECK (media_type IN (
            'Before',    -- صورة قبل العلاج (مراقبة الجودة)
            'After',     -- صورة بعد العلاج (مراقبة الجودة)
            'OPG',       -- أشعة بانورامية
            'CBCT',      -- أشعة مقطعية ثلاثية الأبعاد
            'XRay',      -- أشعة عادية
            'Document'   -- وثيقة (موافقة، تقرير، ...)
        )),

    file_name       VARCHAR(255) NOT NULL,
    file_path       TEXT        NOT NULL,   -- MinIO object key
    file_size_bytes BIGINT,
    mime_type       VARCHAR(100),
    thumbnail_path  TEXT,                   -- MinIO thumbnail (Before/After/XRay/OPG)
    title           VARCHAR(200),
    description     TEXT,
    tooth_id        SMALLINT    REFERENCES teeth(id),  -- اختياري: لأي سن؟

    -- هل الوسيط مطلوب؟ (مثل: OPG قبل تركيب الجسر)
    is_required     BOOLEAN     NOT NULL DEFAULT FALSE,

    -- موافقة الطبيب على الوسيط (للأشعة والصور الطبية)
    is_approved     BOOLEAN     NOT NULL DEFAULT FALSE,
    approved_by_id  UUID        REFERENCES users(id),
    approved_at     TIMESTAMPTZ,

    uploaded_by_id  UUID        NOT NULL REFERENCES users(id),
    uploaded_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at      TIMESTAMPTZ
);

CREATE INDEX ix_media_patient   ON patient_media(patient_id);
CREATE INDEX ix_media_type      ON patient_media(patient_id, media_type);
CREATE INDEX ix_media_appt      ON patient_media(appointment_id);
```

### مراقبة الجودة (Before/After)

```
مريض → Before (قبل العلاج) → إجراء → After (بعد العلاج)
                                        ↓
                              تقرير الجودة = مقارنة Before/After
                              لنفس patient_id + appointment_id
```

---

## 5. الإجراءات — إضافة `service_id`

```sql
CREATE TABLE procedures (
    id                      UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    appointment_id          UUID        NOT NULL REFERENCES appointments(id),
    patient_id              UUID        NOT NULL REFERENCES patients(id),
    doctor_id               UUID        NOT NULL REFERENCES users(id),
    treatment_plan_item_id  UUID        REFERENCES treatment_plan_items(id),
    tooth_id                SMALLINT    REFERENCES teeth(id),
    surface                 VARCHAR(20),
    procedure_name          VARCHAR(200) NOT NULL,
    procedure_code          VARCHAR(50),

    -- جاهز لـ Services Catalog (Phase 5)
    -- NULL حالياً، يُملأ عند بناء الكتالوج
    service_id              UUID        NULL,
    -- لا FK الآن — يُضاف عند إنشاء جدول services

    notes                   TEXT,
    performed_at            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    duration_minutes        INTEGER,

    -- حالة الفوترة (تُحدَّث من Treasury — Phase 5)
    billing_status          VARCHAR(20) NOT NULL DEFAULT 'Pending'
        CHECK (billing_status IN ('Pending','SentToTreasury','Paid','Cancelled')),

    created_at              TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_procedures_appointment ON procedures(appointment_id);
CREATE INDEX ix_procedures_patient     ON procedures(patient_id);
CREATE INDEX ix_procedures_service     ON procedures(service_id)
    WHERE service_id IS NOT NULL;
CREATE INDEX ix_procedures_billing     ON procedures(billing_status)
    WHERE billing_status != 'Paid';
```

### لماذا بدون FK الآن؟

```
services (Phase 5)
    ↑
procedures.service_id ── يشير إليه

عند إنشاء جدول services:
ALTER TABLE procedures
    ADD CONSTRAINT fk_procedures_service
    FOREIGN KEY (service_id) REFERENCES services(id);
```

لا نُعيد تصميم الجدول — فقط نُضيف الـ FK لاحقاً.

---

## 6. خطط العلاج — التكاليف المُحدَّثة

```sql
CREATE TABLE treatment_plans (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID        NOT NULL REFERENCES patients(id),
    doctor_id       UUID        NOT NULL REFERENCES users(id),
    title           VARCHAR(200) NOT NULL,
    description     TEXT,

    -- التكاليف الثلاثة
    estimated_cost  DECIMAL(10,2) NOT NULL DEFAULT 0,
        -- التقدير الأولي عند وضع الخطة
    total_cost      DECIMAL(10,2) NOT NULL DEFAULT 0,
        -- مجموع بنود الخطة (يُحسَب تلقائياً)
    actual_cost     DECIMAL(10,2) NOT NULL DEFAULT 0,
        -- ما تم تنفيذه فعلياً (يُحدَّث عند إضافة إجراء)

    paid_amount     DECIMAL(10,2) NOT NULL DEFAULT 0,
        -- ما دُفع فعلياً (من Treasury — Phase 5)

    priority        VARCHAR(10) NOT NULL DEFAULT 'Normal'
        CHECK (priority IN ('Low','Normal','High','Urgent')),
        -- Low=انتظار | Normal=عادي | High=مهم | Urgent=طارئ

    status          VARCHAR(20) NOT NULL DEFAULT 'Draft'
        CHECK (status IN ('Draft','Active','Completed','Cancelled','OnHold')),
    start_date      DATE,
    end_date        DATE,
    notes           TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    deleted_at      TIMESTAMPTZ
);
```

### الفرق بين الأرقام الأربعة

| الحقل | المعنى | مثال |
|-------|--------|------|
| `estimated_cost` | تقدير الطبيب الأولي | 5000 ر.س |
| `total_cost` | مجموع بنود الخطة المُدرَجة | 4800 ر.س |
| `actual_cost` | ما نُفِّذ فعلاً من إجراءات | 3200 ر.س |
| `paid_amount` | ما دُفع من الفاتورة | 2000 ر.س |

```
remaining = total_cost - paid_amount       → 2800 ر.س (متبقٍ للدفع)
variance  = estimated_cost - actual_cost   → 1800 ر.س (فارق التقدير)
```

---

## 7. Patient Timeline — ربط مع Phase 3

تفاصيل كاملة في [PATIENT_TIMELINE_DESIGN.md](PATIENT_TIMELINE_DESIGN.md).

أحداث Phase 3 المُضافة:

| الحدث | المُطلِق |
|-------|---------|
| `chart.updated` | عند تحديث dental_chart_entries |
| `procedure.performed` | عند إضافة procedure |
| `treatment_plan.created` | عند إنشاء treatment_plan |
| `treatment_plan.activated` | عند تفعيل الخطة |
| `treatment_plan.completed` | عند إتمام الخطة |
| `media.uploaded` | عند رفع patient_media |
| `doctor.assigned` | عند إنشاء doctor_assignment |
| `doctor.transferred` | عند التحويل |

---

## 8. قائمة Migrations الكاملة

| # | الملف | المحتوى |
|---|-------|---------|
| 006 | `006_teeth_seed.sql` | 52 سن (32 دائمة + 20 لبنية) |
| 007 | `007_dental_chart.sql` | `dental_chart_entries` |
| 008 | `008_treatment_plans.sql` | `treatment_plans` + `treatment_plan_items` |
| 009 | `009_procedures.sql` | `procedures` (مع `service_id`) |
| 010 | `010_patient_media.sql` | `patient_media` (الأنواع الجديدة) |
| 011 | `011_doctor_assignments.sql` | `doctor_assignments` (مع status + can_edit) |
| 012 | `012_patient_timeline.sql` | `patient_timeline` |

---

## 9. API Endpoints الكاملة (22)

### Dental Chart (4)
| Method | Path | الصلاحية |
|--------|------|---------|
| GET | `/api/patients/{id}/chart` | `Patients.View` |
| GET | `/api/patients/{id}/chart/history` | `Patients.View` |
| GET | `/api/patients/{id}/chart/{toothId}/history` | `Patients.View` |
| POST | `/api/patients/{id}/chart` | `Patients.Edit` |

### Treatment Plans (6)
| Method | Path | الصلاحية |
|--------|------|---------|
| GET | `/api/patients/{id}/treatment-plans` | `Patients.View` |
| POST | `/api/patients/{id}/treatment-plans` | `Patients.Edit` |
| GET | `/api/treatment-plans/{id}` | `Patients.View` |
| PUT | `/api/treatment-plans/{id}` | `Patients.Edit` |
| DELETE | `/api/treatment-plans/{id}` | `Patients.Delete` |
| PATCH | `/api/treatment-plans/{id}/status` | `Patients.Edit` |

### Procedures (4)
| Method | Path | الصلاحية |
|--------|------|---------|
| GET | `/api/appointments/{id}/procedures` | `Patients.View` |
| POST | `/api/appointments/{id}/procedures` | `Patients.Edit` |
| PUT | `/api/procedures/{id}` | `Patients.Edit` |
| DELETE | `/api/procedures/{id}` | `Patients.Delete` |

### Media Library (4)
| Method | Path | الصلاحية |
|--------|------|---------|
| GET | `/api/patients/{id}/media` | `Patients.View` |
| POST | `/api/patients/{id}/media` | `Patients.Edit` |
| GET | `/api/patients/{id}/media/{mediaId}/url` | `Patients.View` |
| DELETE | `/api/patients/{id}/media/{mediaId}` | `Patients.Delete` |

### Doctor Assignments (4)
| Method | Path | الصلاحية |
|--------|------|---------|
| GET | `/api/patients/{id}/doctors` | `Patients.View` |
| POST | `/api/patients/{id}/doctors` | `Users.Edit` |
| PATCH | `/api/patients/{id}/doctors/{assignmentId}/transfer` | `Users.Edit` |
| PATCH | `/api/patients/{id}/doctors/{assignmentId}/status` | `Users.Edit` |

### Patient Timeline (1)
| Method | Path | الصلاحية |
|--------|------|---------|
| GET | `/api/patients/{id}/timeline` | `Patients.View` |

---

## 10. Frontend Screens (5 + 1)

| الشاشة | المسار | الأولوية |
|--------|--------|---------|
| S07 — تفاصيل المريض | `/patients/{id}` | 🔴 عالية |
| S08 — مخطط الأسنان | `/patients/{id}/chart` | 🔴 عالية |
| S09 — خطط العلاج | `/patients/{id}/treatment-plans` | 🔴 عالية |
| S10 — إجراءات الموعد | `/appointments/{id}/procedures` | 🟡 متوسطة |
| S11 — مكتبة الوسائط | `/patients/{id}/media` | 🟡 متوسطة |
| S12 — Timeline المريض | `/patients/{id}/timeline` | 🟢 مهمة |

---

## 11. Module Structure

```
backend/src/DentalERP.Modules.Clinical/
├── Domain/
│   └── Entities/
│       ├── Tooth.cs
│       ├── DentalChartEntry.cs
│       ├── TreatmentPlan.cs
│       ├── TreatmentPlanItem.cs
│       ├── Procedure.cs
│       ├── PatientMedia.cs
│       ├── DoctorAssignment.cs
│       └── PatientTimelineEvent.cs
├── Infrastructure/
│   ├── ClinicalDbContext.cs
│   └── Configurations/
│       ├── ToothConfiguration.cs
│       ├── DentalChartEntryConfiguration.cs
│       ├── TreatmentPlanConfiguration.cs
│       ├── TreatmentPlanItemConfiguration.cs
│       ├── ProcedureConfiguration.cs
│       ├── PatientMediaConfiguration.cs
│       ├── DoctorAssignmentConfiguration.cs
│       └── PatientTimelineEventConfiguration.cs
├── Features/
│   ├── Chart/           (4 features)
│   ├── TreatmentPlans/  (6 features)
│   ├── Procedures/      (4 features)
│   ├── Media/           (4 features)
│   ├── Assignments/     (4 features)
│   └── Timeline/        (1 feature)
├── Services/
│   └── TimelineService.cs
├── Endpoints/
│   ├── ChartEndpoints.cs
│   ├── TreatmentPlanEndpoints.cs
│   ├── ProcedureEndpoints.cs
│   ├── MediaEndpoints.cs
│   ├── AssignmentEndpoints.cs
│   └── TimelineEndpoints.cs
└── ClinicalModule.cs
```

---

## 12. Unit Tests المخططة (~28 اختبار)

| الوحدة | الاختبارات |
|--------|-----------|
| DentalChartEntry | is_current transitions، condition validation |
| TreatmentPlan | status machine، cost calculations |
| TreatmentPlanItem | price computation، discount |
| DoctorAssignment | status machine، can_edit rules |
| PatientMedia | type validation |

---

## 13. ملخص التغييرات عن PHASE_3_DESIGN.md

| # | التغيير | الوصف |
|---|---------|-------|
| 1 | ✅ الأسنان اللبنية | FDI 51-85، حقل `is_primary = TRUE` |
| 2 | ✅ Doctor Assignment Lifecycle | `status` + `can_view` + `can_edit` + Transfer، **بدون UNIQUE** |
| 3 | ✅ Media Types + Approval | Before, After, OPG, CBCT, XRay, Document + `is_required/is_approved/approved_by_id/approved_at` |
| 4 | ✅ `service_id` + `billing_status` على Procedures | UUID nullable + Pending/SentToTreasury/Paid/Cancelled |
| 5 | ✅ Treatment Plan Costs + Priority | `estimated_cost/total_cost/actual_cost/paid_amount` + `priority` (Low/Normal/High/Urgent) |
| 6 | ✅ Patient Timeline + event_category | وثيقة مستقلة + Clinical/Financial/Administrative/Insurance/Radiology/Laboratory |
| 7 | ✅ Dental Chart History | **قرار نهائي**: جدول واحد + `is_current` |

---

**📋 هذا هو التصميم النهائي لـ Phase 3 — جاهز للمراجعة والاعتماد.**
**✋ لا يُبدأ البرمجة قبل الموافقة الصريحة.**
