# PHASE 3 DESIGN — DentalERP

> **التاريخ:** 2026-06-17 | **الحالة:** 📋 Design — Pending Approval
> **ملاحظة:** لا يُبدأ البرمجة قبل اعتماد هذا التصميم.

---

## 1. نطاق Phase 3

| الوحدة | الوصف |
|--------|-------|
| **Dental Chart** | مخطط الأسنان التفاعلي (32 سن + رسم الحالات) |
| **Treatment Plans** | خطط العلاج بالمراحل والتكاليف |
| **Procedures** | الإجراءات المنفَّذة على كل موعد |
| **Media Library** | الصور والوثائق المرفقة بالمريض |
| **Doctor Assignment** | تعيين الأطباء للمرضى والمواعيد |

---

## 2. تصميم قاعدة البيانات

### 2.1 جدول الأسنان المرجعية — `teeth`

```sql
CREATE TABLE teeth (
    id              SMALLINT PRIMARY KEY,  -- 11–18, 21–28, 31–38, 41–48 (FDI notation)
    fdi_number      SMALLINT NOT NULL UNIQUE,
    universal_number SMALLINT,             -- نظام ترقيم بديل
    name_ar         VARCHAR(50) NOT NULL,
    name_en         VARCHAR(50) NOT NULL,
    jaw             VARCHAR(10) NOT NULL CHECK (jaw IN ('Upper','Lower')),
    side            VARCHAR(10) NOT NULL CHECK (side IN ('Right','Left')),
    tooth_type      VARCHAR(20) NOT NULL   -- Incisor, Canine, Premolar, Molar
);
-- بيانات ثابتة: 32 سن بالترقيم الدولي FDI
```

### 2.2 مخطط الأسنان — `dental_chart_entries`

```sql
CREATE TABLE dental_chart_entries (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID NOT NULL REFERENCES patients(id),
    tooth_id        SMALLINT NOT NULL REFERENCES teeth(id),
    surface         VARCHAR(20),
        -- NULL = السن بالكامل
        -- M=Mesial, D=Distal, B=Buccal, L=Lingual, O=Occlusal
    condition       VARCHAR(50) NOT NULL
        CHECK (condition IN (
            'Healthy','Caries','Filled','Missing','Extracted',
            'Crown','Bridge','Implant','RootCanal','Fracture',
            'Impacted','Sensitive','Mobility','Other'
        )),
    severity        VARCHAR(10) CHECK (severity IN ('Mild','Moderate','Severe')),
    notes           TEXT,
    recorded_by_id  UUID NOT NULL REFERENCES users(id),
    recorded_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    appointment_id  UUID REFERENCES appointments(id),

    -- الحالة الحالية للسن (مُحدَّثة)
    is_current      BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE INDEX ix_chart_patient       ON dental_chart_entries(patient_id);
CREATE INDEX ix_chart_tooth         ON dental_chart_entries(patient_id, tooth_id);
CREATE INDEX ix_chart_appointment   ON dental_chart_entries(appointment_id);
```

### 2.3 خطط العلاج — `treatment_plans`

```sql
CREATE TABLE treatment_plans (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID NOT NULL REFERENCES patients(id),
    doctor_id       UUID NOT NULL REFERENCES users(id),
    title           VARCHAR(200) NOT NULL,
    description     TEXT,
    total_cost      DECIMAL(10,2) NOT NULL DEFAULT 0,
    paid_amount     DECIMAL(10,2) NOT NULL DEFAULT 0,
    status          VARCHAR(20) NOT NULL DEFAULT 'Draft'
        CHECK (status IN ('Draft','Active','Completed','Cancelled','OnHold')),
    start_date      DATE,
    end_date        DATE,
    notes           TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    deleted_at      TIMESTAMPTZ
);

CREATE INDEX ix_treatment_patient ON treatment_plans(patient_id);
CREATE INDEX ix_treatment_doctor  ON treatment_plans(doctor_id);
```

### 2.4 بنود خطة العلاج — `treatment_plan_items`

```sql
CREATE TABLE treatment_plan_items (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    treatment_plan_id   UUID NOT NULL REFERENCES treatment_plans(id) ON DELETE CASCADE,
    tooth_id            SMALLINT REFERENCES teeth(id),
    surface             VARCHAR(20),
    procedure_name      VARCHAR(200) NOT NULL,
    procedure_code      VARCHAR(50),       -- رمز الإجراء (CDT / محلي)
    quantity            INTEGER NOT NULL DEFAULT 1,
    unit_price          DECIMAL(10,2) NOT NULL DEFAULT 0,
    discount_percent    DECIMAL(5,2) NOT NULL DEFAULT 0,
    total_price         DECIMAL(10,2) GENERATED ALWAYS AS
                            (quantity * unit_price * (1 - discount_percent / 100)) STORED,
    status              VARCHAR(20) NOT NULL DEFAULT 'Pending'
        CHECK (status IN ('Pending','InProgress','Completed','Cancelled')),
    sequence_number     INTEGER NOT NULL DEFAULT 1,
    notes               TEXT,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_plan_items_plan ON treatment_plan_items(treatment_plan_id);
```

### 2.5 الإجراءات المنفَّذة — `procedures`

```sql
CREATE TABLE procedures (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    appointment_id      UUID NOT NULL REFERENCES appointments(id),
    patient_id          UUID NOT NULL REFERENCES patients(id),
    doctor_id           UUID NOT NULL REFERENCES users(id),
    treatment_plan_item_id UUID REFERENCES treatment_plan_items(id),
    tooth_id            SMALLINT REFERENCES teeth(id),
    surface             VARCHAR(20),
    procedure_name      VARCHAR(200) NOT NULL,
    procedure_code      VARCHAR(50),
    notes               TEXT,
    performed_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    duration_minutes    INTEGER,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_procedures_appointment ON procedures(appointment_id);
CREATE INDEX ix_procedures_patient     ON procedures(patient_id);
CREATE INDEX ix_procedures_doctor      ON procedures(doctor_id);
```

### 2.6 مكتبة الوسائط — `patient_media`

```sql
CREATE TABLE patient_media (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID NOT NULL REFERENCES patients(id),
    appointment_id  UUID REFERENCES appointments(id),
    media_type      VARCHAR(20) NOT NULL
        CHECK (media_type IN ('XRay','Photo','Document','Video','Other')),
    file_name       VARCHAR(255) NOT NULL,
    file_path       TEXT NOT NULL,          -- MinIO object key
    file_size_bytes BIGINT,
    mime_type       VARCHAR(100),
    thumbnail_path  TEXT,                   -- MinIO thumbnail key (XRay/Photo)
    title           VARCHAR(200),
    description     TEXT,
    uploaded_by_id  UUID NOT NULL REFERENCES users(id),
    uploaded_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    deleted_at      TIMESTAMPTZ
);

CREATE INDEX ix_media_patient     ON patient_media(patient_id);
CREATE INDEX ix_media_appointment ON patient_media(appointment_id);
CREATE INDEX ix_media_type        ON patient_media(media_type);
```

### 2.7 تعيين الأطباء — `doctor_assignments`

```sql
CREATE TABLE doctor_assignments (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID NOT NULL REFERENCES patients(id),
    doctor_id       UUID NOT NULL REFERENCES users(id),
    assigned_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    is_primary      BOOLEAN NOT NULL DEFAULT FALSE,
    notes           TEXT,
    UNIQUE (patient_id, doctor_id)
);

CREATE INDEX ix_assignment_patient ON doctor_assignments(patient_id);
CREATE INDEX ix_assignment_doctor  ON doctor_assignments(doctor_id);
```

---

## 3. Business Rules

### 3.1 Dental Chart

| القاعدة | التفاصيل |
|---------|---------|
| السن الواحد قد يحمل حالات متعددة | مثل: Caries + Filling على نفس السن |
| `is_current = TRUE` يُمثّل الحالة الراهنة | عند التحديث: `is_current` السابق → FALSE |
| التاريخ محفوظ | لا حذف — إضافة حالة جديدة يُلغي القديمة بـ `is_current = FALSE` |
| Surface لها 5 قيم | M, D, B, L, O — أو NULL للسن كاملاً |
| السن المفقود لا يُحذف | يُسجَّل بحالة `Missing` أو `Extracted` |

### 3.2 Treatment Plans

| القاعدة | التفاصيل |
|---------|---------|
| خطة واحدة Active في نفس الوقت لكل مريض | يُمكن وجود Draft متعدد |
| `total_cost` محسوبة تلقائياً | مجموع `total_price` لكل البنود |
| لا يمكن حذف خطة Active | يجب إلغاؤها أو إتمامها أولاً |
| ربط الإجراء بالبند | عند تنفيذ الإجراء: البند ينتقل من Pending → Completed |
| تتبع الدفعات | `paid_amount` يُحدَّث من وحدة Treasury (Phase 5) |

### 3.3 Procedures

| القاعدة | التفاصيل |
|---------|---------|
| كل إجراء مرتبط بموعد | لا يمكن تسجيل إجراء بدون موعد |
| الإجراء يُنشئ chart entry تلقائياً | إذا كان للسن: يُحدَّث dental_chart_entry |
| الإجراء يُحدِّث بند خطة العلاج | إذا كان مرتبطاً بـ treatment_plan_item |

### 3.4 Media Library

| القاعدة | التفاصيل |
|---------|---------|
| الحجم الأقصى للملف | 50 MB |
| الصور: thumbnail تلقائي | JPEG بعرض 300px |
| الأشعة: DICOM support | مستقبلاً في Phase 7 |
| Soft Delete | `deleted_at` فقط — الملف يبقى في MinIO |
| التخزين | MinIO: bucket `patient-media/{patientId}/` |

### 3.5 Doctor Assignment

| القاعدة | التفاصيل |
|---------|---------|
| طبيب واحد primary لكل مريض | `UNIQUE (patient_id, doctor_id)` |
| الطبيب يرى فقط مرضاه | `Doctor` role: فلترة بـ `doctor_assignments` |
| `Administrator` يرى الكل | بدون فلترة |
| التعيين يُنشأ تلقائياً | عند إنشاء أول موعد للمريض مع الطبيب |

---

## 4. API Endpoints المخططة

### Dental Chart (4)
| Method | Path | الصلاحية |
|--------|------|---------|
| GET | `/api/patients/{id}/chart` | `Patients.View` |
| POST | `/api/patients/{id}/chart` | `Patients.Edit` |
| GET | `/api/patients/{id}/chart/history` | `Patients.View` |
| PUT | `/api/patients/{id}/chart/{toothId}` | `Patients.Edit` |

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
| GET | `/api/patients/{id}/media/{mediaId}` | `Patients.View` |
| DELETE | `/api/patients/{id}/media/{mediaId}` | `Patients.Delete` |

### Doctor Assignments (3)
| Method | Path | الصلاحية |
|--------|------|---------|
| GET | `/api/patients/{id}/doctors` | `Patients.View` |
| POST | `/api/patients/{id}/doctors` | `Users.Edit` |
| DELETE | `/api/patients/{id}/doctors/{doctorId}` | `Users.Edit` |

**المجموع: 21 Endpoint جديد**

---

## 5. Frontend Screens

| الشاشة | المسار | الأولوية |
|--------|--------|---------|
| S07 — تفاصيل المريض | `/patients/{id}` | عالية |
| S08 — مخطط الأسنان | `/patients/{id}/chart` | عالية |
| S09 — خطط العلاج | `/patients/{id}/treatment-plans` | عالية |
| S10 — إجراءات الموعد | `/appointments/{id}/procedures` | متوسطة |
| S11 — مكتبة الوسائط | `/patients/{id}/media` | متوسطة |

---

## 6. بنية الوحدة الجديدة

```
backend/src/DentalERP.Modules.Clinical/
├── Domain/Entities/
│   ├── Tooth.cs              (seed data entity)
│   ├── DentalChartEntry.cs
│   ├── TreatmentPlan.cs
│   ├── TreatmentPlanItem.cs
│   ├── Procedure.cs
│   ├── PatientMedia.cs
│   └── DoctorAssignment.cs
├── Infrastructure/
│   ├── ClinicalDbContext.cs
│   └── Configurations/
├── Features/
│   ├── Chart/         (4 features)
│   ├── TreatmentPlans/ (6 features)
│   ├── Procedures/    (4 features)
│   ├── Media/         (4 features)
│   └── Assignments/   (3 features)
├── Endpoints/
└── ClinicalModule.cs
```

---

## 7. Migration

```
backend/migrations/
├── 006_teeth_seed.sql          ← 32 سن بالترقيم FDI
├── 007_dental_chart.sql        ← dental_chart_entries
├── 008_treatment_plans.sql     ← treatment_plans + items
├── 009_procedures.sql          ← procedures
├── 010_patient_media.sql       ← patient_media
└── 011_doctor_assignments.sql  ← doctor_assignments
```

---

## 8. التبعيات

| التبعية | الحالة |
|---------|--------|
| Phase 1 (IAM — Users/Doctors) | ✅ مكتملة |
| Phase 2 (Patients + Appointments) | ✅ مكتملة |
| MinIO (لـ Media Library) | يحتاج تشغيل Docker Compose |
| Phase 5 (Treasury) | مرتبط بـ `treatment_plans.paid_amount` — لاحقاً |

---

## 9. الاختبارات المخططة

### Unit Tests (المتوقع: ~25)
- DentalChartEntry state transitions
- TreatmentPlan total_cost calculation
- TreatmentPlanItem price computation
- Procedure → ChartEntry automation
- DoctorAssignment uniqueness

### Integration Tests (المتوقع: ~12)
- Chart endpoints (401 + 404 + happy path)
- TreatmentPlan CRUD
- Procedure creation
- Media upload/download
- Doctor assignment

---

## 10. الجاهزية للتنفيذ

| البند | الحالة |
|-------|--------|
| قاعدة البيانات — مصممة | ✅ |
| Business Rules — محددة | ✅ |
| API Endpoints — محددة | ✅ |
| Frontend Screens — محددة | ✅ |
| Module Structure — محددة | ✅ |
| Migrations — 6 ملفات مخططة | ✅ |

---

**📋 هذا الوثيق جاهز للمراجعة والاعتماد. لا يُبدأ البرمجة قبل الموافقة.**
