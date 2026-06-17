# 04 — ERD Final
# مخطط قاعدة البيانات النهائي — DentalERP

> **الإصدار:** V-Final | **التاريخ:** 2026-06-16 | **الحالة:** مرجع تنفيذي معتمد

---

## 1. مبادئ التصميم

| المبدأ | التطبيق |
|--------|---------|
| Single Tenant | لا `tenant_id` في أي جدول |
| UUID PKs | `gen_random_uuid()` لكل جدول |
| Soft Delete | `deleted_at TIMESTAMPTZ NULL` على كل جدول رئيسي |
| TIMESTAMPTZ | كل التوقيتات بـ Timezone (UTC في PostgreSQL) |
| NUMERIC(15,2) | كل المبالغ المالية — 2 منازل عشرية |
| FEFO | `ORDER BY expiry_date ASC NULLS LAST` للمخزون |
| Computed Views | الأرصدة محسوبة (لا حقول مخزّنة) لتجنب Stale Data |
| Immutable Finance | لا Physical Delete للسجلات المالية — Void/Reverse فقط |

---

## 2. مخطط العلاقات الكلي

```
┌──────────────────────────────────────────────────────────────────────┐
│                         CORE ENTITY GROUPS                           │
├──────────────────────────────────────────────────────────────────────┤
│                                                                      │
│  clinics(1) ──┬── doctors ──┬── patient_doctor_assignments           │
│               ├── staff      │        └── patients                   │
│               ├── vaults     └── procedures ── treatment_plans       │
│               ├── cost_centers ★                                     │
│               └── treatment_locations ★ (clinic→rooms→chairs)       │
│                                                                      │
│  patients ──┬── patient_medical_history                              │
│             ├── patient_documents (Media Library)                    │
│             ├── dental_chart_entries                                 │
│             ├── invoices ── invoice_items ── payments                │
│             ├── installments / advance_payments                      │
│             ├── patient_insurance_links ★ ── insurance_companies ★  │
│             └── claims ★                                            │
│                                                                      │
│  appointments ── queue_entries                                       │
│                                                                      │
│  suppliers ── purchase_orders ── purchase_invoices                  │
│           └── supplier_account_summary (View ★)                     │
│                                                                      │
│  external_customers ★ ── customer_account_ledger (View ★)           │
│                                                                      │
│  doctors ── doctor_account_summary (View ★)                         │
│          ── doctor_service_commissions ★                             │
│          ── commission_records                                       │
│                                                                      │
│  vault_transactions ── reverse_transaction_links ★                  │
│                                                                      │
│  stock_items ── stock_batches ── stock_movements                    │
│  medical_services ── service_default_materials ★                    │
│                                                                      │
│  users ── roles ── role_permissions ── permissions                  │
│  refresh_tokens | workflow_settings ★ | audit_logs                  │
│                                                                      │
│  lab_order_types ── lab_orders ── lab_technicians ★ Core V1         │
│  lab_commission_records | lab_expenses ── lab_expense_categories    │
│                                                                      │
│  radiology_types ── radiology_orders ── radiology_technicians ★ Core V1│
│  radiology_images (MinIO) | radiology_commission_records            │
│  radiology_expenses ── radiology_expense_categories                 │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

**الأسلوب:** ★ = جديد/معدَّل في V2 أو V-Final

---

## 3. DDL الكامل — جميع الجداول

### 3.0 Base Entity Pattern

```sql
-- كل جدول رئيسي يرث هذا النمط (يُطبَّق يدوياً لا بـ inheritance)
-- id, created_at, created_by, updated_at, updated_by, deleted_at, deleted_by
```

---

### 3.1 وحدة IAM — المستخدمون والصلاحيات

```sql
-- ═══════════════════════════════════════════════════════
-- USERS
-- ═══════════════════════════════════════════════════════
CREATE TABLE users (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username        VARCHAR(50) NOT NULL UNIQUE,
    email           VARCHAR(150) UNIQUE,
    password_hash   VARCHAR(255) NOT NULL,
    full_name       VARCHAR(150) NOT NULL,
    is_active       BOOLEAN NOT NULL DEFAULT true,
    doctor_id       UUID REFERENCES doctors(id),  -- NULL إذا ليس طبيباً
    staff_id        UUID REFERENCES staff(id),    -- NULL إذا ليس موظفاً
    failed_attempts SMALLINT NOT NULL DEFAULT 0,
    locked_until    TIMESTAMPTZ,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by      UUID REFERENCES users(id),
    updated_at      TIMESTAMPTZ,
    updated_by      UUID REFERENCES users(id),
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID REFERENCES users(id)
);

-- ═══════════════════════════════════════════════════════
-- ROLES
-- ═══════════════════════════════════════════════════════
CREATE TABLE roles (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(50) NOT NULL UNIQUE,
    description VARCHAR(255),
    is_system   BOOLEAN NOT NULL DEFAULT false,  -- الأدوار الافتراضية (لا تُحذف)
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ═══════════════════════════════════════════════════════
-- PERMISSIONS — كتالوج 32 صلاحية
-- ═══════════════════════════════════════════════════════
CREATE TABLE permissions (
    id     UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code   VARCHAR(50) NOT NULL UNIQUE,  -- مثال: Invoice.Cancel
    module VARCHAR(30) NOT NULL,         -- مثال: Invoicing
    label  VARCHAR(100)                  -- وصف بالعربية للعرض في S51
);

INSERT INTO permissions (code, module, label) VALUES
-- Patients (4)
('Patient.Create','Patients','تسجيل مريض جديد'),
('Patient.Edit','Patients','تعديل بيانات المريض'),
('Patient.Delete','Patients','حذف مريض'),
('Patient.View','Patients','عرض ملف المريض'),
-- Clinical (5)
('Procedure.Create','Clinical','تسجيل إجراء طبي'),
('Procedure.Edit','Clinical','تعديل إجراء طبي'),
('Procedure.Delete','Clinical','حذف إجراء طبي'),
('TreatmentPlan.Create','Clinical','إنشاء خطة علاج'),
('TreatmentPlan.Edit','Clinical','تعديل خطة علاج'),
-- Invoicing (5)
('Invoice.Create','Invoicing','إنشاء فاتورة'),
('Invoice.Edit','Invoicing','تعديل فاتورة'),
('Invoice.Delete','Invoicing','حذف فاتورة (مسودة فقط)'),
('Invoice.Cancel','Invoicing','إلغاء فاتورة'),
('Invoice.Print','Invoicing','طباعة فاتورة'),
-- Treasury (5)
('Treasury.Add','Treasury','إضافة حركة خزينة'),
('Treasury.Edit','Treasury','تعديل حركة خزينة'),
('Treasury.Delete','Treasury','حذف حركة خزينة'),
('Treasury.Reverse','Treasury','عكس حركة خزينة'),
('Treasury.Print','Treasury','طباعة سند خزينة'),
-- Inventory (4)
('Stock.Add','Inventory','إضافة أصناف للمخزون'),
('Stock.Edit','Inventory','تعديل بيانات صنف'),
('Stock.Delete','Inventory','صرف/حذف مخزون'),
('Stock.Count','Inventory','تنفيذ جرد دوري'),
-- Purchasing (4)
('Purchase.Create','Purchasing','إنشاء طلب/أمر شراء'),
('Purchase.Edit','Purchasing','تعديل طلب/أمر'),
('Purchase.Delete','Purchasing','حذف طلب/أمر'),
('Purchase.Approve','Purchasing','اعتماد أمر الشراء'),
-- Reports (2)
('Reports.View','Reports','عرض التقارير'),
('Reports.Export','Reports','تصدير PDF/Excel'),
-- Laboratory (4) ★ Core V1
('Lab.Create','Laboratory','إنشاء أمر معمل'),
('Lab.Edit','Laboratory','تعديل أمر معمل'),
('Lab.Delete','Laboratory','إلغاء أمر معمل'),
('Lab.Manage','Laboratory','إدارة فنيي المعمل وأنواع الأشغال'),
-- Radiology (4) ★ Core V1
('Radiology.Create','Radiology','إنشاء طلب أشعة'),
('Radiology.Edit','Radiology','تعديل طلب أشعة'),
('Radiology.Delete','Radiology','إلغاء طلب أشعة'),
('Radiology.Manage','Radiology','إدارة فنيي الأشعة وأنواع الأشعة'),
-- Administration (3)
('User.Manage','Administration','إدارة المستخدمين'),
('Role.Manage','Administration','إدارة الأدوار والصلاحيات'),
('System.Settings','Administration','إعدادات النظام');

-- ═══════════════════════════════════════════════════════
-- USER_ROLES + ROLE_PERMISSIONS
-- ═══════════════════════════════════════════════════════
CREATE TABLE user_roles (
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, role_id)
);

CREATE TABLE role_permissions (
    role_id       UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    permission_id UUID NOT NULL REFERENCES permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
);

-- ═══════════════════════════════════════════════════════
-- REFRESH_TOKENS
-- ═══════════════════════════════════════════════════════
CREATE TABLE refresh_tokens (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash  VARCHAR(255) NOT NULL UNIQUE,
    expires_at  TIMESTAMPTZ NOT NULL,
    is_revoked  BOOLEAN NOT NULL DEFAULT false,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    device_info VARCHAR(255)
);
```

---

### 3.2 وحدة Clinic — إعدادات العيادة

```sql
-- ═══════════════════════════════════════════════════════
-- CLINICS
-- ═══════════════════════════════════════════════════════
CREATE TABLE clinics (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name                VARCHAR(150) NOT NULL,
    logo_url            VARCHAR(500),
    address             TEXT,
    phone               VARCHAR(20),
    email               VARCHAR(150),
    tax_number          VARCHAR(50),
    currency_code       CHAR(3) NOT NULL DEFAULT 'LYD',
    working_hours       JSONB,  -- { monday: {open: "08:00", close: "17:00"}, ... }
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now()
);
-- Single Tenant: يحتوي دائماً على سجل واحد فقط

-- ═══════════════════════════════════════════════════════
-- SPECIALTIES
-- ═══════════════════════════════════════════════════════
CREATE TABLE specialties (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name       VARCHAR(100) NOT NULL UNIQUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ═══════════════════════════════════════════════════════
-- DOCTORS
-- ═══════════════════════════════════════════════════════
CREATE TABLE doctors (
    id                       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    full_name                VARCHAR(150) NOT NULL,
    specialty_id             UUID REFERENCES specialties(id),
    phone                    VARCHAR(20),
    email                    VARCHAR(150),
    license_number           VARCHAR(50),
    -- Commission Engine ★ V-Final
    commission_method        VARCHAR(30) NOT NULL DEFAULT 'percentage_of_service'
                             CHECK (commission_method IN
                                 ('percentage_of_service','fixed_amount','percentage_of_net_service')),
    default_commission_value NUMERIC(10,2) NOT NULL DEFAULT 0,
    is_active                BOOLEAN NOT NULL DEFAULT true,
    created_at               TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by               UUID REFERENCES users(id),
    updated_at               TIMESTAMPTZ,
    updated_by               UUID REFERENCES users(id),
    deleted_at               TIMESTAMPTZ,
    deleted_by               UUID REFERENCES users(id)
);

-- ★ تخصيص عمولة لكل طبيب × خدمة
CREATE TABLE doctor_service_commissions (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    doctor_id         UUID NOT NULL REFERENCES doctors(id),
    service_id        UUID NOT NULL REFERENCES medical_services(id),
    commission_method VARCHAR(30) NOT NULL
                      CHECK (commission_method IN
                          ('percentage_of_service','fixed_amount','percentage_of_net_service')),
    commission_value  NUMERIC(10,2) NOT NULL CHECK (commission_value >= 0),
    UNIQUE(doctor_id, service_id)
);

-- ═══════════════════════════════════════════════════════
-- STAFF
-- ═══════════════════════════════════════════════════════
CREATE TABLE staff (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    full_name       VARCHAR(150) NOT NULL,
    role_title      VARCHAR(100),
    phone           VARCHAR(20),
    base_salary     NUMERIC(10,2) NOT NULL DEFAULT 0,
    is_active       BOOLEAN NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at      TIMESTAMPTZ,
    deleted_by      UUID REFERENCES users(id)
);

-- ═══════════════════════════════════════════════════════
-- SERVICE_CATEGORIES + MEDICAL_SERVICES
-- ═══════════════════════════════════════════════════════
CREATE TABLE service_categories (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name       VARCHAR(100) NOT NULL UNIQUE,
    sort_order SMALLINT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE medical_services (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category_id             UUID REFERENCES service_categories(id),
    name                    VARCHAR(200) NOT NULL,
    code                    VARCHAR(30) UNIQUE,
    price                   NUMERIC(10,2) NOT NULL CHECK (price >= 0),
    has_inventory_tracking  BOOLEAN NOT NULL DEFAULT false,  -- ★ V2: اختياري per service
    requires_images         BOOLEAN NOT NULL DEFAULT false,
    is_active               BOOLEAN NOT NULL DEFAULT true,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at              TIMESTAMPTZ,
    updated_by              UUID REFERENCES users(id)
);

-- ★ المواد الافتراضية لكل خدمة (V-Final)
CREATE TABLE service_default_materials (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    service_id       UUID NOT NULL REFERENCES medical_services(id),
    stock_item_id    UUID NOT NULL REFERENCES stock_items(id),
    default_quantity NUMERIC(10,2) NOT NULL DEFAULT 1 CHECK (default_quantity > 0),
    UNIQUE(service_id, stock_item_id)
);

-- ═══════════════════════════════════════════════════════
-- TREATMENT_LOCATIONS ★ V2 (Self-Referencing)
-- ═══════════════════════════════════════════════════════
CREATE TABLE treatment_locations (
    id                 UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    clinic_id          UUID NOT NULL REFERENCES clinics(id),
    parent_id          UUID REFERENCES treatment_locations(id),  -- NULL = Clinic level
    level              VARCHAR(10) NOT NULL CHECK (level IN ('clinic','room','chair')),
    name               VARCHAR(100) NOT NULL,
    assigned_doctor_id UUID REFERENCES doctors(id),
    is_active          BOOLEAN NOT NULL DEFAULT true,
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now()
);
-- قيد: سجل واحد فقط بـ level='clinic'
CREATE UNIQUE INDEX uq_single_clinic_location
    ON treatment_locations (clinic_id) WHERE level = 'clinic';

-- ═══════════════════════════════════════════════════════
-- VAULTS (الخزائن)
-- ═══════════════════════════════════════════════════════
CREATE TABLE vaults (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100) NOT NULL UNIQUE,
    type            VARCHAR(20) NOT NULL CHECK (type IN ('cash','bank','card','pos')),
    bank_name       VARCHAR(100),
    account_number  VARCHAR(50),
    opening_balance NUMERIC(12,2) NOT NULL DEFAULT 0,
    is_active       BOOLEAN NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ═══════════════════════════════════════════════════════
-- WORKFLOW_SETTINGS ★ Development Plan Final
-- ═══════════════════════════════════════════════════════
CREATE TABLE workflow_settings (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    action_type      VARCHAR(30) NOT NULL UNIQUE
                     CHECK (action_type IN ('procedure_edit','procedure_delete','invoice_cancel')),
    requires_approval BOOLEAN NOT NULL DEFAULT false,
    updated_by       UUID REFERENCES users(id),
    updated_at       TIMESTAMPTZ NOT NULL DEFAULT now()
);
-- Seed Data
INSERT INTO workflow_settings (action_type, requires_approval) VALUES
('procedure_edit',   false),
('procedure_delete', false),
('invoice_cancel',   false);
```

---

### 3.3 وحدة Patient — المرضى

```sql
-- ═══════════════════════════════════════════════════════
-- PATIENTS
-- ═══════════════════════════════════════════════════════
CREATE TABLE patients (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    mrn               VARCHAR(20) NOT NULL UNIQUE,  -- DEN-YYYY-XXXXX
    full_name         VARCHAR(200) NOT NULL,
    phone             VARCHAR(20) NOT NULL,
    phone2            VARCHAR(20),
    national_id       VARCHAR(30),
    date_of_birth     DATE,
    gender            VARCHAR(10) CHECK (gender IN ('male','female')),
    address           TEXT,
    blood_type        VARCHAR(5) CHECK (blood_type IN ('A+','A-','B+','B-','O+','O-','AB+','AB-')),
    allergies         TEXT[],                  -- قائمة الحساسيات
    chronic_diseases  TEXT[],                  -- الأمراض المزمنة
    emergency_contact JSONB,                   -- { name, phone, relation }
    referral_source   VARCHAR(50),
    notes             TEXT,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by        UUID REFERENCES users(id),
    updated_at        TIMESTAMPTZ,
    updated_by        UUID REFERENCES users(id),
    deleted_at        TIMESTAMPTZ,
    deleted_by        UUID REFERENCES users(id)
);
-- MRN Generation Function
CREATE SEQUENCE IF NOT EXISTS mrn_seq_2026 START 1;
-- (تُنشأ sequence منفصلة لكل سنة عند الحاجة)

-- ═══════════════════════════════════════════════════════
-- PATIENT_DOCTOR_ASSIGNMENTS
-- ═══════════════════════════════════════════════════════
CREATE TABLE patient_doctor_assignments (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id UUID NOT NULL REFERENCES patients(id),
    doctor_id  UUID NOT NULL REFERENCES doctors(id),
    status     VARCHAR(20) NOT NULL DEFAULT 'active' CHECK (status IN ('active','closed')),
    can_edit   BOOLEAN NOT NULL DEFAULT true,
    notes      TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    closed_at  TIMESTAMPTZ,
    closed_by  UUID REFERENCES users(id),
    UNIQUE(patient_id, doctor_id)
);

-- ═══════════════════════════════════════════════════════
-- PATIENT_DOCUMENTS (Media Library)
-- ═══════════════════════════════════════════════════════
CREATE TABLE patient_documents (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id    UUID NOT NULL REFERENCES patients(id),
    procedure_id  UUID REFERENCES procedures(id),  -- NULL = لا يرتبط بإجراء
    type          VARCHAR(20) NOT NULL CHECK (type IN ('before','after','xray','opg','cbct','document','other')),
    file_url      VARCHAR(500) NOT NULL,
    file_name     VARCHAR(255),
    file_size     INTEGER,
    mime_type     VARCHAR(100),
    notes         TEXT,
    uploaded_by   UUID NOT NULL REFERENCES users(id),
    uploaded_at   TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ═══════════════════════════════════════════════════════
-- PATIENT_INSURANCE_LINKS ★
-- ═══════════════════════════════════════════════════════
CREATE TABLE patient_insurance_links (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id           UUID NOT NULL REFERENCES patients(id),
    insurance_company_id UUID NOT NULL REFERENCES insurance_companies(id),
    policy_number        VARCHAR(50),
    is_active            BOOLEAN NOT NULL DEFAULT true,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT now()
);
```

---

### 3.4 وحدة Scheduling — المواعيد

```sql
-- ═══════════════════════════════════════════════════════
-- APPOINTMENTS
-- ═══════════════════════════════════════════════════════
CREATE TABLE appointments (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id           UUID NOT NULL REFERENCES patients(id),
    doctor_id            UUID NOT NULL REFERENCES doctors(id),
    treatment_location_id UUID REFERENCES treatment_locations(id),
    scheduled_at         TIMESTAMPTZ NOT NULL,
    duration_minutes     SMALLINT NOT NULL DEFAULT 30,
    type                 VARCHAR(30) NOT NULL DEFAULT 'checkup'
                         CHECK (type IN ('checkup','followup','procedure','emergency')),
    status               VARCHAR(20) NOT NULL DEFAULT 'scheduled'
                         CHECK (status IN ('scheduled','confirmed','arrived','in_progress','completed','cancelled','no_show')),
    notes                TEXT,
    cancelled_reason     TEXT,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by           UUID REFERENCES users(id),
    updated_at           TIMESTAMPTZ,
    updated_by           UUID REFERENCES users(id)
);

-- ═══════════════════════════════════════════════════════
-- QUEUE_ENTRIES (الطابور الحي — Real-time)
-- ═══════════════════════════════════════════════════════
CREATE TABLE queue_entries (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id       UUID NOT NULL REFERENCES patients(id),
    doctor_id        UUID NOT NULL REFERENCES doctors(id),
    appointment_id   UUID REFERENCES appointments(id),
    queue_number     SMALLINT NOT NULL,
    status           VARCHAR(20) NOT NULL DEFAULT 'waiting'
                     CHECK (status IN ('waiting','called','with_doctor','done','left')),
    visit_type       VARCHAR(30),
    checked_in_at    TIMESTAMPTZ NOT NULL DEFAULT now(),
    called_at        TIMESTAMPTZ,
    done_at          TIMESTAMPTZ,
    queue_date       DATE NOT NULL DEFAULT CURRENT_DATE
);
CREATE INDEX idx_queue_date_doctor ON queue_entries (queue_date, doctor_id);
```

---

### 3.5 وحدة Clinical — السريرية

```sql
-- ═══════════════════════════════════════════════════════
-- DENTAL_CHART_ENTRIES (حالة الأسنان)
-- ═══════════════════════════════════════════════════════
CREATE TABLE dental_chart_entries (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id    UUID NOT NULL REFERENCES patients(id),
    tooth_number  SMALLINT NOT NULL CHECK (tooth_number BETWEEN 11 AND 85),  -- FDI
    surface       VARCHAR(5),  -- M, D, B, L, O
    condition     VARCHAR(30) NOT NULL,  -- healthy, decay, filling, crown, missing, extracted...
    mobility_grade SMALLINT CHECK (mobility_grade BETWEEN 0 AND 3),
    notes         TEXT,
    recorded_by   UUID NOT NULL REFERENCES users(id),
    recorded_at   TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ═══════════════════════════════════════════════════════
-- TREATMENT_PLANS
-- ═══════════════════════════════════════════════════════
CREATE TABLE treatment_plans (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID NOT NULL REFERENCES patients(id),
    doctor_id       UUID NOT NULL REFERENCES doctors(id),
    title           VARCHAR(200) NOT NULL,
    status          VARCHAR(20) NOT NULL DEFAULT 'active'
                    CHECK (status IN ('active','completed','cancelled')),
    notes           TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by      UUID REFERENCES users(id),
    updated_at      TIMESTAMPTZ,
    updated_by      UUID REFERENCES users(id)
);

-- ═══════════════════════════════════════════════════════
-- TREATMENT_PLAN_ITEMS
-- ═══════════════════════════════════════════════════════
CREATE TABLE treatment_plan_items (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    plan_id         UUID NOT NULL REFERENCES treatment_plans(id),
    service_id      UUID NOT NULL REFERENCES medical_services(id),
    tooth_number    SMALLINT,
    estimated_price NUMERIC(10,2) NOT NULL,
    status          VARCHAR(20) NOT NULL DEFAULT 'pending'
                    CHECK (status IN ('pending','in_progress','completed','skipped')),
    sort_order      SMALLINT NOT NULL DEFAULT 0,
    notes           TEXT
);

-- ═══════════════════════════════════════════════════════
-- PROCEDURES (الإجراءات الطبية)
-- ═══════════════════════════════════════════════════════
CREATE TABLE procedures (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id            UUID NOT NULL REFERENCES patients(id),
    doctor_id             UUID NOT NULL REFERENCES doctors(id),
    service_id            UUID NOT NULL REFERENCES medical_services(id),
    treatment_plan_id     UUID REFERENCES treatment_plans(id),
    treatment_plan_item_id UUID REFERENCES treatment_plan_items(id),
    treatment_location_id UUID NOT NULL REFERENCES treatment_locations(id),  -- ★ V2 إلزامي
    tooth_numbers         SMALLINT[],       -- قائمة الأسنان المتعلقة
    base_price            NUMERIC(10,2) NOT NULL,
    discount_type         VARCHAR(10) CHECK (discount_type IN ('percentage','fixed')),
    discount_value        NUMERIC(10,2) NOT NULL DEFAULT 0,
    final_price           NUMERIC(10,2) NOT NULL,
    lab_cost              NUMERIC(10,2) NOT NULL DEFAULT 0,  -- ★ V-Final للـ Net Commission
    clinical_notes        TEXT,
    status                VARCHAR(20) NOT NULL DEFAULT 'draft'
                          CHECK (status IN ('draft','confirmed','billed','cancelled')),
    confirmed_at          TIMESTAMPTZ,
    confirmed_by          UUID REFERENCES users(id),
    created_at            TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by            UUID REFERENCES users(id),
    updated_at            TIMESTAMPTZ,
    updated_by            UUID REFERENCES users(id)
);
CREATE INDEX idx_procedures_patient ON procedures (patient_id);
CREATE INDEX idx_procedures_doctor  ON procedures (doctor_id);

-- ═══════════════════════════════════════════════════════
-- APPROVAL_REQUESTS (اختياري — فقط عند workflow ON)
-- ═══════════════════════════════════════════════════════
CREATE TABLE approval_requests (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    request_type    VARCHAR(30) NOT NULL
                    CHECK (request_type IN ('procedure_edit','procedure_delete','invoice_cancel')),
    entity_id       UUID NOT NULL,   -- procedure_id أو invoice_id
    entity_type     VARCHAR(30) NOT NULL,
    reason          TEXT NOT NULL,
    change_data     JSONB,           -- التعديل المطلوب (للـ edit)
    status          VARCHAR(20) NOT NULL DEFAULT 'pending'
                    CHECK (status IN ('pending','approved','rejected')),
    requested_by    UUID NOT NULL REFERENCES users(id),
    reviewed_by     UUID REFERENCES users(id),
    review_notes    TEXT,
    requested_at    TIMESTAMPTZ NOT NULL DEFAULT now(),
    reviewed_at     TIMESTAMPTZ
);
CREATE INDEX idx_approval_pending ON approval_requests (status) WHERE status = 'pending';
```

---

### 3.6 وحدة Treasury — الخزينة

```sql
-- ═══════════════════════════════════════════════════════
-- INVOICES
-- ═══════════════════════════════════════════════════════
CREATE TABLE invoices (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_number  VARCHAR(30) NOT NULL UNIQUE,  -- INV-YYYY-XXXXXX
    patient_id      UUID NOT NULL REFERENCES patients(id),
    doctor_id       UUID NOT NULL REFERENCES doctors(id),
    status          VARCHAR(20) NOT NULL DEFAULT 'draft'
                    CHECK (status IN ('draft','confirmed','partially_paid','paid','cancelled')),
    subtotal        NUMERIC(12,2) NOT NULL DEFAULT 0,
    discount_total  NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_amount    NUMERIC(12,2) NOT NULL DEFAULT 0,
    paid_amount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    remaining       NUMERIC(12,2) GENERATED ALWAYS AS (total_amount - paid_amount) STORED,
    currency        CHAR(3) NOT NULL DEFAULT 'LYD',
    notes           TEXT,
    cancelled_reason TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by      UUID REFERENCES users(id),
    updated_at      TIMESTAMPTZ,
    updated_by      UUID REFERENCES users(id)
);
CREATE INDEX idx_invoices_patient ON invoices (patient_id);
CREATE INDEX idx_invoices_status  ON invoices (status) WHERE status NOT IN ('paid','cancelled');

-- ═══════════════════════════════════════════════════════
-- INVOICE_ITEMS
-- ═══════════════════════════════════════════════════════
CREATE TABLE invoice_items (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id   UUID NOT NULL REFERENCES invoices(id),
    procedure_id UUID NOT NULL REFERENCES procedures(id),
    service_name VARCHAR(200) NOT NULL,  -- Snapshot عند الإنشاء
    quantity     SMALLINT NOT NULL DEFAULT 1,
    unit_price   NUMERIC(10,2) NOT NULL,
    discount     NUMERIC(10,2) NOT NULL DEFAULT 0,
    total        NUMERIC(10,2) NOT NULL
);

-- ═══════════════════════════════════════════════════════
-- PAYMENTS
-- ═══════════════════════════════════════════════════════
CREATE TABLE payments (
    id                 UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id         UUID NOT NULL REFERENCES invoices(id),
    vault_id           UUID NOT NULL REFERENCES vaults(id),
    amount             NUMERIC(10,2) NOT NULL CHECK (amount > 0),
    payment_method     VARCHAR(20) NOT NULL CHECK (payment_method IN ('cash','bank_transfer','card','pos','cheque')),
    reference_number   VARCHAR(50),  -- للشيك/الحوالة
    notes              TEXT,
    created_at         TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by         UUID REFERENCES users(id)
);

-- ═══════════════════════════════════════════════════════
-- INSTALLMENT_PLANS + INSTALLMENT_PAYMENTS
-- ═══════════════════════════════════════════════════════
CREATE TABLE installment_plans (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id      UUID NOT NULL REFERENCES invoices(id),
    patient_id      UUID NOT NULL REFERENCES patients(id),
    total_amount    NUMERIC(10,2) NOT NULL,
    installments_count SMALLINT NOT NULL,
    notes           TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by      UUID REFERENCES users(id)
);

CREATE TABLE installment_payments (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    plan_id          UUID NOT NULL REFERENCES installment_plans(id),
    installment_num  SMALLINT NOT NULL,
    due_date         DATE NOT NULL,
    amount           NUMERIC(10,2) NOT NULL,
    status           VARCHAR(20) NOT NULL DEFAULT 'pending'
                     CHECK (status IN ('pending','paid','overdue')),
    paid_at          TIMESTAMPTZ,
    vault_id         UUID REFERENCES vaults(id),
    payment_method   VARCHAR(20)
);

-- ═══════════════════════════════════════════════════════
-- ADVANCE_PAYMENTS (الدفعات المقدمة)
-- ═══════════════════════════════════════════════════════
CREATE TABLE advance_payments (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID NOT NULL REFERENCES patients(id),
    vault_id        UUID NOT NULL REFERENCES vaults(id),
    amount          NUMERIC(10,2) NOT NULL CHECK (amount > 0),
    remaining       NUMERIC(10,2) NOT NULL,
    notes           TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by      UUID REFERENCES users(id)
);

-- ═══════════════════════════════════════════════════════
-- VAULT_TRANSACTIONS (كل حركات الخزينة)
-- ═══════════════════════════════════════════════════════
CREATE TABLE vault_transactions (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    vault_id                UUID NOT NULL REFERENCES vaults(id),
    transaction_type        VARCHAR(30) NOT NULL CHECK (transaction_type IN (
        'receipt_from_patient','payment_to_patient',
        'receipt_from_supplier','payment_to_supplier',
        'payment_to_doctor','payment_to_technician','payment_to_employee',
        'general_receipt','general_payment',
        'inter_vault_transfer','salary','commission',
        'lab_income','payment_to_lab',           -- ★ Lab Module
        'radiology_income'                        -- ★ Radiology Module
    )),
    amount                  NUMERIC(12,2) NOT NULL,
    direction               CHAR(2) NOT NULL CHECK (direction IN ('in','out')),
    -- روابط الكيانات المصدر (NULL لأنواع أخرى)
    related_invoice_id      UUID REFERENCES invoices(id),
    related_patient_id      UUID REFERENCES patients(id),
    related_supplier_id     UUID REFERENCES suppliers(id),
    related_doctor_id       UUID REFERENCES doctors(id),
    related_employee_id     UUID REFERENCES staff(id),
    related_claim_id        UUID REFERENCES claims(id),
    related_lab_order_id    UUID,  -- ★ Lab Module (FK بعد إنشاء الجدول)
    related_radiology_order_id UUID, -- ★ Radiology Module (FK بعد إنشاء الجدول)
    cost_center_id          UUID REFERENCES cost_centers(id),
    reference_number        VARCHAR(50),
    notes                   TEXT,
    is_reversed             BOOLEAN NOT NULL DEFAULT false,
    is_reversal             BOOLEAN NOT NULL DEFAULT false,
    day_closure_id          UUID,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by              UUID REFERENCES users(id)
);
CREATE INDEX idx_vault_tx_vault    ON vault_transactions (vault_id, created_at DESC);
CREATE INDEX idx_vault_tx_patient  ON vault_transactions (related_patient_id);

-- ★ REVERSE_TRANSACTION_LINKS (V2)
CREATE TABLE reverse_transaction_links (
    id                        UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    original_transaction_id   UUID NOT NULL REFERENCES vault_transactions(id),
    reverse_transaction_id    UUID NOT NULL REFERENCES vault_transactions(id),
    corrected_transaction_id  UUID REFERENCES vault_transactions(id),
    reason                    TEXT NOT NULL CHECK (LENGTH(reason) >= 10),
    performed_by              UUID NOT NULL REFERENCES users(id),
    created_at                TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ═══════════════════════════════════════════════════════
-- DAY_CLOSURES (إقفال اليوم)
-- ═══════════════════════════════════════════════════════
CREATE TABLE day_closures (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    closure_date    DATE NOT NULL UNIQUE,
    vault_snapshots JSONB NOT NULL,  -- { vault_id: { opening, closing } }
    total_receipts  NUMERIC(12,2) NOT NULL,
    total_payments  NUMERIC(12,2) NOT NULL,
    net_flow        NUMERIC(12,2) NOT NULL,
    closed_by       UUID NOT NULL REFERENCES users(id),
    closed_at       TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ═══════════════════════════════════════════════════════
-- COMMISSION_RECORDS
-- ═══════════════════════════════════════════════════════
CREATE TABLE commission_records (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    doctor_id         UUID NOT NULL REFERENCES doctors(id),
    procedure_id      UUID NOT NULL REFERENCES procedures(id),
    payment_id        UUID NOT NULL REFERENCES payments(id),  -- Cash-Basis
    commission_method VARCHAR(30) NOT NULL,
    base_amount       NUMERIC(10,2) NOT NULL,  -- المبلغ الخاضع للحساب
    commission_rate   NUMERIC(10,4) NOT NULL,  -- النسبة أو القيمة الثابتة
    commission_amount NUMERIC(10,2) NOT NULL,
    is_paid           BOOLEAN NOT NULL DEFAULT false,
    paid_at           TIMESTAMPTZ,
    vault_transaction_id UUID REFERENCES vault_transactions(id),
    created_at        TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ═══════════════════════════════════════════════════════
-- PAYROLL_RECORDS ★ V2
-- ═══════════════════════════════════════════════════════
CREATE TABLE payroll_records (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    staff_id             UUID NOT NULL REFERENCES staff(id),
    period_month         DATE NOT NULL,  -- أول يوم في الشهر
    amount               NUMERIC(10,2) NOT NULL CHECK (amount > 0),
    vault_transaction_id UUID REFERENCES vault_transactions(id),
    notes                TEXT,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by           UUID REFERENCES users(id),
    UNIQUE(staff_id, period_month)
);

-- ═══════════════════════════════════════════════════════
-- PATIENT_CREDIT_NOTES
-- ═══════════════════════════════════════════════════════
CREATE TABLE patient_credit_notes (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID NOT NULL REFERENCES patients(id),
    invoice_id      UUID REFERENCES invoices(id),
    amount          NUMERIC(10,2) NOT NULL CHECK (amount > 0),
    reason          TEXT NOT NULL,
    vault_transaction_id UUID REFERENCES vault_transactions(id),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by      UUID REFERENCES users(id)
);
```

---

### 3.7 وحدة Insurance — التأمين (Core Light)

```sql
-- ═══════════════════════════════════════════════════════
-- INSURANCE_COMPANIES ★ Core (V2)
-- ═══════════════════════════════════════════════════════
CREATE TABLE insurance_companies (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name                 VARCHAR(150) NOT NULL,
    contract_number      VARCHAR(50),
    default_coverage_pct NUMERIC(5,2) NOT NULL DEFAULT 0
                         CHECK (default_coverage_pct BETWEEN 0 AND 100),
    -- ★ V-Final: هرمية التسعير
    price_increase_pct   NUMERIC(5,2) DEFAULT 0 CHECK (price_increase_pct >= 0),
    price_discount_pct   NUMERIC(5,2) DEFAULT 0 CHECK (price_discount_pct >= 0),
    CONSTRAINT chk_no_both_increase_discount
        CHECK (NOT (price_increase_pct > 0 AND price_discount_pct > 0)),
    contact_person       VARCHAR(100),
    phone                VARCHAR(20),
    is_active            BOOLEAN NOT NULL DEFAULT true,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ═══════════════════════════════════════════════════════
-- INSURANCE_COVERED_SERVICES ★
-- ═══════════════════════════════════════════════════════
CREATE TABLE insurance_covered_services (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    insurance_company_id UUID NOT NULL REFERENCES insurance_companies(id),
    service_id           UUID NOT NULL REFERENCES medical_services(id),
    coverage_pct         NUMERIC(5,2) NOT NULL CHECK (coverage_pct BETWEEN 0 AND 100),
    coverage_cap         NUMERIC(10,2),  -- حد أعلى بالمبلغ (NULL = بلا حد)
    custom_price         NUMERIC(10,2),  -- ★ V-Final: سعر مخصّص (أعلى أولوية)
    UNIQUE(insurance_company_id, service_id)
);

-- ═══════════════════════════════════════════════════════
-- CLAIMS ★ (المطالبات)
-- ═══════════════════════════════════════════════════════
CREATE TABLE claims (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    claim_number         VARCHAR(30) NOT NULL UNIQUE,  -- CLM-YYYY-XXXXX
    patient_id           UUID NOT NULL REFERENCES patients(id),
    procedure_id         UUID NOT NULL REFERENCES procedures(id),
    insurance_company_id UUID NOT NULL REFERENCES insurance_companies(id),
    claim_amount         NUMERIC(10,2) NOT NULL CHECK (claim_amount >= 0),
    base_price_used      NUMERIC(10,2) NOT NULL,  -- ★ V-Final: Snapshot
    collected_amount     NUMERIC(10,2) NOT NULL DEFAULT 0,
    status               VARCHAR(20) NOT NULL DEFAULT 'open'
                         CHECK (status IN ('open','partially_paid','paid','rejected','partially_rejected')),
    rejection_reason     TEXT,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by           UUID REFERENCES users(id)
    -- لا حقول authorization_status أو approval_status (BR-INS scope)
);
```

---

### 3.8 وحدة Inventory — المخزون

```sql
-- ═══════════════════════════════════════════════════════
-- STOCK_ITEMS (الأصناف)
-- ═══════════════════════════════════════════════════════
CREATE TABLE stock_items (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name              VARCHAR(200) NOT NULL,
    code              VARCHAR(30) UNIQUE,
    barcode           VARCHAR(50),
    category          VARCHAR(100),
    unit              VARCHAR(20) NOT NULL DEFAULT 'piece',
    minimum_threshold NUMERIC(10,2) NOT NULL DEFAULT 0,
    expiry_alert_days SMALLINT NOT NULL DEFAULT 60,
    is_active         BOOLEAN NOT NULL DEFAULT true,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at        TIMESTAMPTZ,
    updated_by        UUID REFERENCES users(id)
);

-- ═══════════════════════════════════════════════════════
-- STOCK_BATCHES (الدُفعات)
-- ═══════════════════════════════════════════════════════
CREATE TABLE stock_batches (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    stock_item_id    UUID NOT NULL REFERENCES stock_items(id),
    batch_number     VARCHAR(50),
    expiry_date      DATE,  -- NULL = لا تاريخ انتهاء
    quantity_in      NUMERIC(10,2) NOT NULL,
    current_quantity NUMERIC(10,2) NOT NULL,
    unit_cost        NUMERIC(10,2),
    received_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    purchase_invoice_item_id UUID  -- ربط بفاتورة الشراء المصدر
);
CREATE INDEX idx_batches_fefo ON stock_batches (stock_item_id, expiry_date ASC NULLS LAST)
    WHERE current_quantity > 0;

-- ═══════════════════════════════════════════════════════
-- STOCK_MOVEMENTS (الحركات)
-- ═══════════════════════════════════════════════════════
CREATE TABLE stock_movements (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    stock_item_id         UUID NOT NULL REFERENCES stock_items(id),
    batch_id              UUID REFERENCES stock_batches(id),
    movement_type         VARCHAR(30) NOT NULL DEFAULT 'consumption'  -- ★ V-Final
                          CHECK (movement_type IN
                              ('consumption','waste','purchase_in','manual_issue','stock_take_adjustment')),
    quantity              NUMERIC(10,2) NOT NULL,
    direction             CHAR(3) NOT NULL CHECK (direction IN ('in','out')),
    -- ★ V-Final: التحقق
    waste_reason          VARCHAR(20) CHECK (waste_reason IN ('expired','damaged','lost','broken')),
    procedure_id          UUID REFERENCES procedures(id),      -- ★ لـ consumption فقط
    -- ★ V2: التحليل
    doctor_id             UUID REFERENCES doctors(id),
    treatment_location_id UUID REFERENCES treatment_locations(id),
    notes                 TEXT,
    performed_by          UUID NOT NULL REFERENCES users(id),
    performed_at          TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX idx_movements_item ON stock_movements (stock_item_id, performed_at DESC);
```

---

### 3.9 وحدة Purchasing — المشتريات

```sql
-- ═══════════════════════════════════════════════════════
-- SUPPLIERS (الموردون)
-- ═══════════════════════════════════════════════════════
CREATE TABLE suppliers (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(150) NOT NULL,
    contact_person  VARCHAR(100),
    phone           VARCHAR(20),
    email           VARCHAR(150),
    address         TEXT,
    tax_number      VARCHAR(50),
    opening_balance NUMERIC(12,2) NOT NULL DEFAULT 0,  -- ★ V2
    is_active       BOOLEAN NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    deleted_at      TIMESTAMPTZ
);

-- ═══════════════════════════════════════════════════════
-- PURCHASE_REQUESTS → PURCHASE_ORDERS → PURCHASE_INVOICES
-- ═══════════════════════════════════════════════════════
CREATE TABLE purchase_requests (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title       VARCHAR(200),
    status      VARCHAR(20) NOT NULL DEFAULT 'draft'
                CHECK (status IN ('draft','submitted','approved','rejected','converted')),
    notes       TEXT,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by  UUID REFERENCES users(id)
);

CREATE TABLE purchase_request_items (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    request_id        UUID NOT NULL REFERENCES purchase_requests(id),
    stock_item_id     UUID NOT NULL REFERENCES stock_items(id),
    requested_quantity NUMERIC(10,2) NOT NULL,
    notes             TEXT
);

CREATE TABLE purchase_orders (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    request_id        UUID REFERENCES purchase_requests(id),
    supplier_id       UUID NOT NULL REFERENCES suppliers(id),
    order_number      VARCHAR(30) NOT NULL UNIQUE,
    status            VARCHAR(20) NOT NULL DEFAULT 'pending'
                      CHECK (status IN ('pending','approved','received','partial','cancelled')),
    expected_date     DATE,
    notes             TEXT,
    approved_by       UUID REFERENCES users(id),
    approved_at       TIMESTAMPTZ,
    created_at        TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by        UUID REFERENCES users(id)
);

CREATE TABLE purchase_order_items (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id          UUID NOT NULL REFERENCES purchase_orders(id),
    stock_item_id     UUID NOT NULL REFERENCES stock_items(id),
    ordered_quantity  NUMERIC(10,2) NOT NULL,
    unit_price        NUMERIC(10,2),
    received_quantity NUMERIC(10,2) NOT NULL DEFAULT 0
);

CREATE TABLE purchase_invoices (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id        UUID REFERENCES purchase_orders(id),
    supplier_id     UUID NOT NULL REFERENCES suppliers(id),
    invoice_number  VARCHAR(50),
    total_amount    NUMERIC(12,2) NOT NULL,
    paid_amount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    status          VARCHAR(20) NOT NULL DEFAULT 'unpaid'
                    CHECK (status IN ('unpaid','partial','paid')),
    invoice_date    DATE NOT NULL,
    due_date        DATE,
    notes           TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by      UUID REFERENCES users(id)
);

CREATE TABLE purchase_invoice_items (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    purchase_invoice_id   UUID NOT NULL REFERENCES purchase_invoices(id),
    stock_item_id         UUID NOT NULL REFERENCES stock_items(id),
    quantity              NUMERIC(10,2) NOT NULL,
    unit_price            NUMERIC(10,2) NOT NULL,
    total                 NUMERIC(10,2) NOT NULL,
    batch_id              UUID REFERENCES stock_batches(id)  -- الدُفعة الناتجة عن الاستلام
);
```

---

### 3.10 وحدة Expenses — المصروفات

```sql
-- ═══════════════════════════════════════════════════════
-- COST_CENTERS ★ مرفوعة لكيان مستقل (V2)
-- ═══════════════════════════════════════════════════════
CREATE TABLE cost_centers (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    is_active   BOOLEAN NOT NULL DEFAULT true,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);
-- Seed Data
INSERT INTO cost_centers (name, description) VALUES
('العيادة', 'المصروفات التشغيلية العامة'),
('التدريب', 'مصروفات التدريب والتطوير');

-- ═══════════════════════════════════════════════════════
-- EXPENSE_VOUCHERS
-- ═══════════════════════════════════════════════════════
CREATE TABLE expense_vouchers (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    voucher_number  VARCHAR(30) NOT NULL UNIQUE,
    cost_center_id  UUID NOT NULL REFERENCES cost_centers(id),  -- إلزامي
    vault_id        UUID NOT NULL REFERENCES vaults(id),
    amount          NUMERIC(10,2) NOT NULL CHECK (amount > 0),
    description     VARCHAR(255) NOT NULL,
    expense_date    DATE NOT NULL DEFAULT CURRENT_DATE,
    receipt_url     VARCHAR(500),
    vault_transaction_id UUID REFERENCES vault_transactions(id),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by      UUID REFERENCES users(id)
);
```

---

### 3.11 وحدة Purchasing — الجهات الخارجية ★ V2

```sql
-- ═══════════════════════════════════════════════════════
-- EXTERNAL_CUSTOMERS ★ (أشعة/معامل خارجية/جهات إحالة)
-- ═══════════════════════════════════════════════════════
CREATE TABLE external_customers (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(150) NOT NULL,
    type            VARCHAR(20) NOT NULL
                    CHECK (type IN ('service_provider','referral_source')),
    contact_person  VARCHAR(100),
    phone           VARCHAR(20),
    opening_balance NUMERIC(12,2) NOT NULL DEFAULT 0,
    is_active       BOOLEAN NOT NULL DEFAULT true,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE external_customer_transactions (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_id      UUID NOT NULL REFERENCES external_customers(id),
    transaction_type VARCHAR(20) NOT NULL CHECK (transaction_type IN ('service','payment')),
    amount           NUMERIC(10,2) NOT NULL,
    description      VARCHAR(255),
    created_at       TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by       UUID REFERENCES users(id)
);
```

---

### 3.12 وحدة IAM — التدقيق والجلسات

```sql
-- ═══════════════════════════════════════════════════════
-- AUDIT_LOGS (مُقسَّم شهرياً)
-- ═══════════════════════════════════════════════════════
CREATE TABLE audit_logs (
    id          UUID NOT NULL DEFAULT gen_random_uuid(),
    module      VARCHAR(30) NOT NULL,
    entity_type VARCHAR(50) NOT NULL,
    entity_id   UUID NOT NULL,
    action      VARCHAR(20) NOT NULL CHECK (action IN ('CREATE','UPDATE','DELETE','CANCEL','PRINT','REVERSE','LOGIN','LOGOUT')),
    old_values  JSONB,
    new_values  JSONB,
    user_id     UUID NOT NULL,
    username    VARCHAR(50) NOT NULL,
    ip_address  INET,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
) PARTITION BY RANGE (created_at);

-- إنشاء partitions (تُنشأ تلقائياً شهرياً بـ Hangfire job)
CREATE TABLE audit_logs_2026_01 PARTITION OF audit_logs
    FOR VALUES FROM ('2026-01-01') TO ('2026-02-01');
CREATE TABLE audit_logs_2026_02 PARTITION OF audit_logs
    FOR VALUES FROM ('2026-02-01') TO ('2026-03-01');
-- ... يكمل لكل الأشهر
```

---

### 3.12 وحدة Laboratory — المعمل

```sql
-- ═══════════════════════════════════════════════════════
-- LAB_ORDER_TYPES (أنواع أشغال المعمل)
-- ═══════════════════════════════════════════════════════
CREATE TABLE lab_order_types (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL UNIQUE,  -- تركيبات، تقويم، أطقم، فينير، إلخ
    description TEXT,
    is_active   BOOLEAN NOT NULL DEFAULT true,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ═══════════════════════════════════════════════════════
-- LAB_TECHNICIANS (فنيو المعمل)
-- ═══════════════════════════════════════════════════════
CREATE TABLE lab_technicians (
    id                       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    full_name                VARCHAR(150) NOT NULL,
    lab_name                 VARCHAR(150),               -- اسم المعمل (إن خارجي)
    phone                    VARCHAR(20),
    email                    VARCHAR(150),
    specialty                VARCHAR(100),               -- تخصص: تركيبات / تقويم / إلخ
    commission_method        VARCHAR(30) NOT NULL DEFAULT 'percentage_of_service'
                             CHECK (commission_method IN ('percentage_of_service','fixed_amount')),
    default_commission_value NUMERIC(10,2) NOT NULL DEFAULT 0,
    opening_balance          NUMERIC(12,2) NOT NULL DEFAULT 0,  -- رصيد افتتاحي
    is_active                BOOLEAN NOT NULL DEFAULT true,
    created_at               TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by               UUID REFERENCES users(id),
    updated_at               TIMESTAMPTZ,
    updated_by               UUID REFERENCES users(id),
    deleted_at               TIMESTAMPTZ,
    deleted_by               UUID REFERENCES users(id)
);

-- ═══════════════════════════════════════════════════════
-- LAB_ORDERS (أوامر المعمل)
-- ═══════════════════════════════════════════════════════
CREATE TABLE lab_orders (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_number        VARCHAR(30) NOT NULL UNIQUE,   -- LAB-2026-00001
    patient_id          UUID NOT NULL REFERENCES patients(id),
    doctor_id           UUID NOT NULL REFERENCES doctors(id),
    procedure_id        UUID REFERENCES procedures(id),        -- اختياري
    lab_technician_id   UUID REFERENCES lab_technicians(id),
    lab_order_type_id   UUID NOT NULL REFERENCES lab_order_types(id),
    description         TEXT,
    tooth_numbers       JSONB,   -- [11, 12, 21] أرقام أسنان FDI
    status              VARCHAR(20) NOT NULL DEFAULT 'pending'
                        CHECK (status IN ('pending','in_progress','completed','delivered','cancelled')),
    order_date          DATE NOT NULL DEFAULT CURRENT_DATE,
    expected_date       DATE,
    delivery_date       DATE,
    cost                NUMERIC(12,2) NOT NULL DEFAULT 0,  -- تكلفة المعمل (ما يُدفع للفني)
    price               NUMERIC(12,2) NOT NULL DEFAULT 0,  -- السعر للمريض (دخل)
    notes               TEXT,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by          UUID REFERENCES users(id),
    updated_at          TIMESTAMPTZ,
    updated_by          UUID REFERENCES users(id),
    deleted_at          TIMESTAMPTZ,
    deleted_by          UUID REFERENCES users(id)
);
CREATE INDEX idx_lab_orders_patient   ON lab_orders (patient_id);
CREATE INDEX idx_lab_orders_status    ON lab_orders (status) WHERE status NOT IN ('delivered','cancelled');
CREATE INDEX idx_lab_orders_technician ON lab_orders (lab_technician_id);

-- ═══════════════════════════════════════════════════════
-- LAB_EXPENSE_CATEGORIES + LAB_EXPENSES
-- ═══════════════════════════════════════════════════════
CREATE TABLE lab_expense_categories (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name       VARCHAR(100) NOT NULL UNIQUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE lab_expenses (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category_id          UUID NOT NULL REFERENCES lab_expense_categories(id),
    amount               NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    description          TEXT,
    expense_date         DATE NOT NULL DEFAULT CURRENT_DATE,
    vault_transaction_id UUID REFERENCES vault_transactions(id),
    created_at           TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by           UUID REFERENCES users(id),
    deleted_at           TIMESTAMPTZ,
    deleted_by           UUID REFERENCES users(id)
);

-- ═══════════════════════════════════════════════════════
-- LAB_COMMISSION_RECORDS
-- ═══════════════════════════════════════════════════════
CREATE TABLE lab_commission_records (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    lab_technician_id   UUID NOT NULL REFERENCES lab_technicians(id),
    lab_order_id        UUID NOT NULL REFERENCES lab_orders(id),
    commission_method   VARCHAR(30) NOT NULL,
    base_amount         NUMERIC(12,2) NOT NULL,
    commission_amount   NUMERIC(12,2) NOT NULL DEFAULT 0,
    is_paid             BOOLEAN NOT NULL DEFAULT false,
    paid_at             TIMESTAMPTZ,
    paid_by             UUID REFERENCES users(id),
    vault_transaction_id UUID REFERENCES vault_transactions(id),
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE (lab_order_id)  -- عمولة واحدة لكل أمر
);

-- ═══════════════════════════════════════════════════════
-- FKs المؤجلة: vault_transactions → lab_orders
-- ═══════════════════════════════════════════════════════
ALTER TABLE vault_transactions
    ADD CONSTRAINT fk_vault_tx_lab_order
    FOREIGN KEY (related_lab_order_id) REFERENCES lab_orders(id);
```

---

### 3.13 وحدة Radiology — الأشعة

```sql
-- ═══════════════════════════════════════════════════════
-- RADIOLOGY_TYPES (أنواع الأشعة)
-- ═══════════════════════════════════════════════════════
CREATE TABLE radiology_types (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL UNIQUE,  -- OPG, CBCT, Periapical, Bitewing, Panoramic
    description TEXT,
    price       NUMERIC(12,2) NOT NULL DEFAULT 0,  -- السعر الافتراضي
    is_active   BOOLEAN NOT NULL DEFAULT true,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- ═══════════════════════════════════════════════════════
-- RADIOLOGY_TECHNICIANS (فنيو الأشعة)
-- ═══════════════════════════════════════════════════════
CREATE TABLE radiology_technicians (
    id                       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    full_name                VARCHAR(150) NOT NULL,
    phone                    VARCHAR(20),
    email                    VARCHAR(150),
    commission_method        VARCHAR(30) NOT NULL DEFAULT 'percentage_of_service'
                             CHECK (commission_method IN ('percentage_of_service','fixed_amount')),
    default_commission_value NUMERIC(10,2) NOT NULL DEFAULT 0,
    is_active                BOOLEAN NOT NULL DEFAULT true,
    created_at               TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by               UUID REFERENCES users(id),
    updated_at               TIMESTAMPTZ,
    updated_by               UUID REFERENCES users(id),
    deleted_at               TIMESTAMPTZ,
    deleted_by               UUID REFERENCES users(id)
);

-- ═══════════════════════════════════════════════════════
-- RADIOLOGY_ORDERS (طلبات الأشعة)
-- ═══════════════════════════════════════════════════════
CREATE TABLE radiology_orders (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    order_number            VARCHAR(30) NOT NULL UNIQUE,  -- RAD-2026-00001
    patient_type            VARCHAR(20) NOT NULL DEFAULT 'internal'
                            CHECK (patient_type IN ('internal','external')),
    patient_id              UUID REFERENCES patients(id),         -- NULL إن external
    external_patient_name   VARCHAR(150),                         -- NULL إن internal
    external_patient_phone  VARCHAR(20),
    referring_doctor_id     UUID REFERENCES doctors(id),          -- الطبيب الطالب (داخلي)
    radiology_technician_id UUID REFERENCES radiology_technicians(id),
    radiology_type_id       UUID NOT NULL REFERENCES radiology_types(id),
    procedure_id            UUID REFERENCES procedures(id),
    status                  VARCHAR(20) NOT NULL DEFAULT 'pending'
                            CHECK (status IN ('pending','in_progress','completed','cancelled')),
    order_date              DATE NOT NULL DEFAULT CURRENT_DATE,
    price                   NUMERIC(12,2) NOT NULL DEFAULT 0,
    paid_amount             NUMERIC(12,2) NOT NULL DEFAULT 0,
    payment_method          VARCHAR(20) CHECK (payment_method IN ('cash','bank_transfer','card','pos','cheque')),
    vault_id                UUID REFERENCES vaults(id),
    report_notes            TEXT,
    notes                   TEXT,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by              UUID REFERENCES users(id),
    updated_at              TIMESTAMPTZ,
    updated_by              UUID REFERENCES users(id),
    deleted_at              TIMESTAMPTZ,
    deleted_by              UUID REFERENCES users(id),
    CONSTRAINT ck_radiology_patient CHECK (
        (patient_type = 'internal' AND patient_id IS NOT NULL AND external_patient_name IS NULL)
        OR
        (patient_type = 'external' AND patient_id IS NULL AND external_patient_name IS NOT NULL)
    )
);
CREATE INDEX idx_radiology_orders_patient   ON radiology_orders (patient_id) WHERE patient_id IS NOT NULL;
CREATE INDEX idx_radiology_orders_status    ON radiology_orders (status) WHERE status NOT IN ('completed','cancelled');
CREATE INDEX idx_radiology_orders_date      ON radiology_orders (order_date DESC);

-- ═══════════════════════════════════════════════════════
-- RADIOLOGY_IMAGES (صور الأشعة — MinIO)
-- ═══════════════════════════════════════════════════════
CREATE TABLE radiology_images (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    radiology_order_id  UUID NOT NULL REFERENCES radiology_orders(id),
    file_url            VARCHAR(500) NOT NULL,   -- MinIO object URL
    file_name           VARCHAR(255) NOT NULL,
    file_type           VARCHAR(10),             -- jpg, png, dcm
    file_size_bytes     INTEGER,
    sort_order          SMALLINT NOT NULL DEFAULT 0,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by          UUID REFERENCES users(id)
);

-- ═══════════════════════════════════════════════════════
-- RADIOLOGY_EXPENSE_CATEGORIES + RADIOLOGY_EXPENSES
-- ═══════════════════════════════════════════════════════
CREATE TABLE radiology_expense_categories (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name       VARCHAR(100) NOT NULL UNIQUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE radiology_expenses (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category_id          UUID NOT NULL REFERENCES radiology_expense_categories(id),
    amount               NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    description          TEXT,
    expense_date         DATE NOT NULL DEFAULT CURRENT_DATE,
    vault_transaction_id UUID REFERENCES vault_transactions(id),
    created_at           TIMESTAMPTZ NOT NULL DEFAULT now(),
    created_by           UUID REFERENCES users(id),
    deleted_at           TIMESTAMPTZ,
    deleted_by           UUID REFERENCES users(id)
);

-- ═══════════════════════════════════════════════════════
-- RADIOLOGY_COMMISSION_RECORDS
-- ═══════════════════════════════════════════════════════
CREATE TABLE radiology_commission_records (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    radiology_technician_id UUID NOT NULL REFERENCES radiology_technicians(id),
    radiology_order_id      UUID NOT NULL REFERENCES radiology_orders(id),
    commission_method       VARCHAR(30) NOT NULL,
    base_amount             NUMERIC(12,2) NOT NULL,
    commission_amount       NUMERIC(12,2) NOT NULL DEFAULT 0,
    is_paid                 BOOLEAN NOT NULL DEFAULT false,
    paid_at                 TIMESTAMPTZ,
    paid_by                 UUID REFERENCES users(id),
    vault_transaction_id    UUID REFERENCES vault_transactions(id),
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE (radiology_order_id)  -- عمولة واحدة لكل طلب
);

-- ═══════════════════════════════════════════════════════
-- FKs المؤجلة: vault_transactions → radiology_orders
-- ═══════════════════════════════════════════════════════
ALTER TABLE vault_transactions
    ADD CONSTRAINT fk_vault_tx_radiology_order
    FOREIGN KEY (related_radiology_order_id) REFERENCES radiology_orders(id);
```

---

## 4. Computed Views (الأرصدة المحسوبة)

```sql
-- ═══════════════════════════════════════════════════════
-- SUPPLIER_ACCOUNT_SUMMARY ★
-- ═══════════════════════════════════════════════════════
CREATE VIEW supplier_account_summary AS
SELECT
    s.id AS supplier_id,
    s.name,
    s.opening_balance,
    COALESCE(SUM(pi.total_amount), 0) AS total_purchases,
    COALESCE(SUM(vt.amount) FILTER (
        WHERE vt.transaction_type = 'payment_to_supplier'
    ), 0) AS total_paid,
    s.opening_balance
        + COALESCE(SUM(pi.total_amount), 0)
        - COALESCE(SUM(vt.amount) FILTER (
              WHERE vt.transaction_type = 'payment_to_supplier'
          ), 0) AS current_balance
FROM suppliers s
LEFT JOIN purchase_invoices pi ON pi.supplier_id = s.id
LEFT JOIN vault_transactions vt ON vt.related_supplier_id = s.id
GROUP BY s.id, s.name, s.opening_balance;

-- ═══════════════════════════════════════════════════════
-- DOCTOR_ACCOUNT_SUMMARY ★
-- ═══════════════════════════════════════════════════════
CREATE VIEW doctor_account_summary AS
SELECT
    d.id AS doctor_id,
    d.full_name,
    COUNT(p.id) AS total_procedures,
    COALESCE(SUM(p.final_price), 0) AS total_revenue,
    COALESCE(SUM(cr.commission_amount), 0) AS total_commission_due,
    COALESCE(SUM(cr.commission_amount) FILTER (WHERE cr.is_paid = true), 0) AS total_paid,
    COALESCE(SUM(cr.commission_amount) FILTER (WHERE cr.is_paid = false), 0) AS remaining
FROM doctors d
LEFT JOIN procedures p ON p.doctor_id = d.id AND p.status NOT IN ('cancelled')
LEFT JOIN commission_records cr ON cr.doctor_id = d.id
GROUP BY d.id, d.full_name;

-- ═══════════════════════════════════════════════════════
-- PATIENT_FINANCIAL_SUMMARY
-- ═══════════════════════════════════════════════════════
CREATE VIEW patient_financial_summary AS
SELECT
    p.id AS patient_id,
    COALESCE(SUM(i.total_amount) FILTER (WHERE i.status != 'cancelled'), 0) AS total_invoiced,
    COALESCE(SUM(i.paid_amount) FILTER (WHERE i.status != 'cancelled'), 0) AS total_paid,
    COALESCE(SUM(i.total_amount - i.paid_amount) FILTER (
        WHERE i.status NOT IN ('paid','cancelled')
    ), 0) AS total_remaining,
    COALESCE(SUM(ap.remaining), 0) AS advance_balance
FROM patients p
LEFT JOIN invoices i ON i.patient_id = p.id
LEFT JOIN advance_payments ap ON ap.patient_id = p.id
GROUP BY p.id;

-- ═══════════════════════════════════════════════════════
-- STOCK_CURRENT_QUANTITY (مخزون حالي بـ FEFO)
-- ═══════════════════════════════════════════════════════
CREATE VIEW stock_current_quantity AS
SELECT
    si.id AS stock_item_id,
    si.name,
    si.unit,
    si.minimum_threshold,
    COALESCE(SUM(sb.current_quantity), 0) AS total_quantity,
    CASE
        WHEN COALESCE(SUM(sb.current_quantity), 0) <= si.minimum_threshold THEN 'low'
        WHEN MIN(sb.expiry_date) <= CURRENT_DATE + si.expiry_alert_days THEN 'expiring_soon'
        ELSE 'ok'
    END AS stock_status,
    MIN(sb.expiry_date) AS nearest_expiry
FROM stock_items si
LEFT JOIN stock_batches sb ON sb.stock_item_id = si.id AND sb.current_quantity > 0
WHERE si.is_active = true
GROUP BY si.id, si.name, si.unit, si.minimum_threshold, si.expiry_alert_days;

-- ═══════════════════════════════════════════════════════
-- LAB_TECHNICIAN_ACCOUNT_SUMMARY ★ Lab Module
-- ═══════════════════════════════════════════════════════
CREATE VIEW lab_technician_account_summary AS
SELECT
    lt.id AS lab_technician_id,
    lt.full_name,
    lt.lab_name,
    lt.opening_balance,
    COUNT(lo.id) AS total_orders,
    COALESCE(SUM(lo.cost), 0) AS total_cost,
    COALESCE(SUM(vt.amount) FILTER (
        WHERE vt.transaction_type = 'payment_to_lab'
    ), 0) AS total_paid,
    lt.opening_balance
        + COALESCE(SUM(lo.cost), 0)
        - COALESCE(SUM(vt.amount) FILTER (WHERE vt.transaction_type = 'payment_to_lab'), 0)
        AS current_balance
FROM lab_technicians lt
LEFT JOIN lab_orders lo ON lo.lab_technician_id = lt.id AND lo.deleted_at IS NULL
LEFT JOIN vault_transactions vt ON vt.related_lab_order_id = lo.id
GROUP BY lt.id, lt.full_name, lt.lab_name, lt.opening_balance;

-- ═══════════════════════════════════════════════════════
-- RADIOLOGY_SUMMARY (إحصائيات الأشعة اليومية)
-- ═══════════════════════════════════════════════════════
CREATE VIEW radiology_daily_summary AS
SELECT
    order_date,
    COUNT(*) FILTER (WHERE patient_type = 'internal') AS internal_orders,
    COUNT(*) FILTER (WHERE patient_type = 'external') AS external_orders,
    SUM(price) AS total_revenue,
    SUM(paid_amount) AS total_collected,
    SUM(price - paid_amount) AS total_remaining
FROM radiology_orders
WHERE deleted_at IS NULL AND status != 'cancelled'
GROUP BY order_date;
```

---

## 5. Indexes الرئيسية

```sql
-- Performance Indexes
CREATE INDEX idx_patients_phone       ON patients (phone);
CREATE INDEX idx_patients_name        ON patients (full_name varchar_pattern_ops);
CREATE INDEX idx_patients_mrn         ON patients (mrn);
CREATE INDEX idx_procedures_status    ON procedures (status) WHERE status NOT IN ('billed','cancelled');
CREATE INDEX idx_invoices_patient_status ON invoices (patient_id, status);
CREATE INDEX idx_vault_tx_date        ON vault_transactions (created_at DESC);
CREATE INDEX idx_claims_status        ON claims (status) WHERE status IN ('open','partially_paid');
CREATE INDEX idx_stock_low            ON stock_items (id) WHERE is_active = true;
CREATE INDEX idx_audit_entity           ON audit_logs (entity_type, entity_id);
CREATE INDEX idx_audit_user             ON audit_logs (user_id, created_at DESC);
-- Lab + Radiology
CREATE INDEX idx_lab_orders_date        ON lab_orders (order_date DESC);
CREATE INDEX idx_lab_orders_expected    ON lab_orders (expected_date) WHERE status NOT IN ('delivered','cancelled');
CREATE INDEX idx_radiology_tech_date    ON radiology_orders (radiology_technician_id, order_date DESC);
```

---

## 6. قرارات تصميم ERD (Architecture Notes)

| القرار | السبب |
|--------|-------|
| Computed Views للأرصدة | تجنب Stale Balance — مصدر حقيقة واحد في الحركات |
| Self-Referencing treatment_locations | أبسط من 3 جداول منفصلة — قابل للتوسع بلا تعقيد |
| reverse_transaction_links كجدول مستقل | يحتاج ربط 2-3 سجلات معاً بمرجع موحّد |
| claims منفصل عن invoices | المطالبة كيان مالي مستقل مرتبط بالإجراء مباشرة، لا بالفاتورة |
| approval_requests موجود (workflow ON فقط) | خطة التطوير النهائية: اختياري default=OFF |
| waste_reason في stock_movements | فصل التوالف عن الاستهلاك في نفس الجدول بـ filter |
| audit_logs partitioned شهرياً | أداء استعلامات + أرشفة سهلة بعد 5 سنوات |
| base_price_used snapshot في claims | تاريخ مطالبة لا يتأثر بتغيير إعدادات الشركة لاحقاً |

---

## ⚠️ نقاط تحتاج توضيح

1. **Dental Chart — تاريخ حالة السن:** الجدول الحالي يسجل آخر حالة فقط. هل نحتاج تاريخاً كاملاً لكل تغيير على كل سن؟ يحتاج جدول `dental_chart_history` منفصل إن كان مطلوباً.

2. **Appointments vs Queue:** هل يمكن أن يدخل مريض الطابور بدون موعد مسبق (walk-in)؟ التصميم الحالي يسمح بذلك (`appointment_id` nullable في queue_entries) لكن يحتاج تأكيداً.

3. **Patient Credit Notes:** هل يجب ربطها بالفاتورة الأصلية إلزامياً أم يمكن أن تكون مستقلة (رصيد آجل عام)؟

4. **Payroll Records:** هل نحتاج `payroll_items` (تفاصيل: أساسي + بدلات + خصومات) أم مبلغ واحد كافٍ في V2؟

5. **Stock current_quantity:** الجدول الحالي يعتمد على `stock_batches.current_quantity`. يجب التأكد من تحديثه تلقائياً عند كل حركة مخزون (Trigger أو يُعالَج في Application Layer؟).

---

*القاموس التفصيلي لكل حقل وقيوده → [05_DATABASE_DICTIONARY.md](05_DATABASE_DICTIONARY.md)*
*عقود API المبنية على هذا ERD → [06_API_CONTRACTS.md](06_API_CONTRACTS.md)*
