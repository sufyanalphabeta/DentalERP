# PHASE 4 FINAL DESIGN — DentalERP
# الخدمات • الخزينة • الفواتير • المدفوعات • الأقساط • حسابات الأطباء

> **التاريخ:** 2026-06-17 | **الحالة:** ✅ APPROVED — Implementation Started
> **الاعتماد:** Phase 3 مكتملة (إجراءات + مخطط + خطط)

---

## 1. الوحدات والنطاق

| الوحدة | الجداول | الغرض |
|--------|---------|-------|
| Services Catalog | `service_categories`, `medical_services` | كتالوج الخدمات والأسعار |
| Treasury | `vaults`, `doctor_profiles`, `vault_transactions` | الخزائن + عمولات الأطباء + حركات |
| Invoices | `invoices`, `invoice_items` | إنشاء فواتير + بنود |
| Payments | `payments` | تسجيل دفعات على الفواتير |
| Installments | `installment_plans`, `installment_payments`, `advance_payments` | أقساط + دفعات مقدمة |
| Commissions | `commission_records` | عمولات الأطباء — Cash-Basis |

---

## 2. مسار العمل الكامل

```
إجراء طبي (Phase 3)
    → billing_status = SentToTreasury
    → إنشاء فاتورة (invoice) + بنود (invoice_items)
    → تسجيل دفعة (payment)
    → vault_transaction (receipt_from_patient)
    → Invoice.status → PartiallyPaid | Paid
    → عند Paid: Commission Engine يحسب عمولة الطبيب
    → timeline: invoice.created + payment.received
```

---

## 3. قرارات التصميم

| القرار | السبب |
|--------|-------|
| `invoice_items.procedure_id` بدون EF FK | فصل الوحدات (Modular Monolith) |
| Commission = Cash-Basis | يُحسَب عند استلام الدفعة، لا عند تسجيل الإجراء |
| رصيد الخزينة = Computed (لا حقل مخزَّن) | تجنب Stale Balance |
| DoctorProfile مستقل عن users | Commission settings منفصلة |
| `procedure_id` nullable في CommissionRecord | Phase 4: عمولة على الفاتورة الكاملة |
| `service_id` FK يُضاف على procedures في هذه Phase | تكامل Services Catalog مع Procedures |

---

## 4. DDL — Migrations الجديدة

### 013_procedures_alter.sql
```sql
-- إضافة حقول تسعير + lab_cost لدعم Commission Engine
ALTER TABLE procedures
    ADD COLUMN IF NOT EXISTS base_price     NUMERIC(12,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS discount_type  VARCHAR(10) CHECK (discount_type IN ('percentage','fixed')),
    ADD COLUMN IF NOT EXISTS discount_value NUMERIC(12,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS final_price    NUMERIC(12,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS lab_cost       NUMERIC(12,2) NOT NULL DEFAULT 0;
-- إضافة FK على service_id
ALTER TABLE procedures
    ADD CONSTRAINT fk_procedures_service
    FOREIGN KEY (service_id) REFERENCES medical_services(id)
    DEFERRABLE INITIALLY DEFERRED;
```
*(يُنفَّذ بعد إنشاء medical_services)*

### 014_service_catalog.sql
```sql
CREATE TABLE service_categories (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name       VARCHAR(100) NOT NULL UNIQUE,
    sort_order SMALLINT NOT NULL DEFAULT 0,
    is_active  BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE medical_services (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    category_id UUID REFERENCES service_categories(id),
    name        VARCHAR(200) NOT NULL,
    code        VARCHAR(30) UNIQUE,
    price       NUMERIC(12,2) NOT NULL DEFAULT 0 CHECK (price >= 0),
    is_active   BOOLEAN NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ,
    deleted_at  TIMESTAMPTZ
);
```

### 015_vaults_doctor_profiles.sql
```sql
CREATE TABLE vaults (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100) NOT NULL UNIQUE,
    type            VARCHAR(20) NOT NULL CHECK (type IN ('cash','bank','card','pos')),
    opening_balance NUMERIC(12,2) NOT NULL DEFAULT 0,
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
INSERT INTO vaults (name, type, opening_balance) VALUES ('الخزينة الرئيسية', 'cash', 0);

CREATE TABLE doctor_profiles (
    user_id                  UUID PRIMARY KEY REFERENCES users(id),
    commission_method        VARCHAR(30) NOT NULL DEFAULT 'percentage_of_service'
        CHECK (commission_method IN ('percentage_of_service','fixed_amount','percentage_of_net_service')),
    default_commission_value NUMERIC(10,2) NOT NULL DEFAULT 0 CHECK (default_commission_value >= 0),
    updated_at               TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

### 016_invoices.sql
```sql
CREATE TABLE invoices (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_number   VARCHAR(30) NOT NULL UNIQUE,
    patient_id       UUID NOT NULL REFERENCES patients(id),
    doctor_id        UUID NOT NULL REFERENCES users(id),
    status           VARCHAR(20) NOT NULL DEFAULT 'Draft'
        CHECK (status IN ('Draft','Confirmed','PartiallyPaid','Paid','Cancelled')),
    subtotal         NUMERIC(12,2) NOT NULL DEFAULT 0,
    discount_total   NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_amount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    paid_amount      NUMERIC(12,2) NOT NULL DEFAULT 0,
    currency         CHAR(3) NOT NULL DEFAULT 'LYD',
    notes            TEXT,
    cancelled_reason TEXT,
    created_by_id    UUID REFERENCES users(id),
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ,
    deleted_at       TIMESTAMPTZ
);
CREATE INDEX ix_invoices_patient ON invoices(patient_id);
CREATE INDEX ix_invoices_status  ON invoices(status) WHERE status NOT IN ('Paid','Cancelled');

CREATE TABLE invoice_items (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id   UUID NOT NULL REFERENCES invoices(id) ON DELETE CASCADE,
    procedure_id UUID,        -- UUID без FK (cross-module)
    service_name VARCHAR(200) NOT NULL,
    service_code VARCHAR(30),
    quantity     SMALLINT NOT NULL DEFAULT 1 CHECK (quantity > 0),
    unit_price   NUMERIC(12,2) NOT NULL CHECK (unit_price >= 0),
    discount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    total        NUMERIC(12,2) NOT NULL,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX ix_invoice_items_invoice ON invoice_items(invoice_id);
```

### 017_payments_vault_transactions.sql
```sql
CREATE TABLE payments (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id       UUID NOT NULL REFERENCES invoices(id),
    vault_id         UUID NOT NULL REFERENCES vaults(id),
    amount           NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    payment_method   VARCHAR(20) NOT NULL
        CHECK (payment_method IN ('cash','bank_transfer','card','pos','cheque')),
    reference_number VARCHAR(50),
    notes            TEXT,
    created_by_id    UUID REFERENCES users(id),
    created_at       TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX ix_payments_invoice ON payments(invoice_id);

CREATE TABLE vault_transactions (
    id                   UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    vault_id             UUID NOT NULL REFERENCES vaults(id),
    transaction_type     VARCHAR(30) NOT NULL CHECK (transaction_type IN (
        'receipt_from_patient','payment_to_doctor',
        'general_receipt','general_payment','inter_vault_transfer'
    )),
    amount               NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    direction            VARCHAR(3) NOT NULL CHECK (direction IN ('in','out')),
    related_invoice_id   UUID REFERENCES invoices(id),
    related_patient_id   UUID REFERENCES patients(id),
    related_doctor_id    UUID REFERENCES users(id),
    reference_number     VARCHAR(50),
    notes                TEXT,
    is_reversed          BOOLEAN NOT NULL DEFAULT FALSE,
    is_reversal          BOOLEAN NOT NULL DEFAULT FALSE,
    created_by_id        UUID REFERENCES users(id),
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX ix_vault_tx_vault   ON vault_transactions(vault_id, created_at DESC);
CREATE INDEX ix_vault_tx_patient ON vault_transactions(related_patient_id);
```

### 018_installments_commissions.sql
```sql
CREATE TABLE installment_plans (
    id                 UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id         UUID NOT NULL REFERENCES invoices(id),
    patient_id         UUID NOT NULL REFERENCES patients(id),
    total_amount       NUMERIC(12,2) NOT NULL CHECK (total_amount > 0),
    installments_count SMALLINT NOT NULL CHECK (installments_count > 0),
    notes              TEXT,
    created_by_id      UUID REFERENCES users(id),
    created_at         TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE installment_payments (
    id               UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    plan_id          UUID NOT NULL REFERENCES installment_plans(id),
    installment_num  SMALLINT NOT NULL CHECK (installment_num > 0),
    due_date         DATE NOT NULL,
    amount           NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    status           VARCHAR(20) NOT NULL DEFAULT 'Pending'
        CHECK (status IN ('Pending','Paid','Overdue')),
    paid_at          TIMESTAMPTZ,
    vault_id         UUID REFERENCES vaults(id),
    payment_method   VARCHAR(20) CHECK (payment_method IN ('cash','bank_transfer','card','pos','cheque'))
);
CREATE INDEX ix_installments_plan ON installment_payments(plan_id);

CREATE TABLE advance_payments (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id    UUID NOT NULL REFERENCES patients(id),
    vault_id      UUID NOT NULL REFERENCES vaults(id),
    amount        NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    remaining     NUMERIC(12,2) NOT NULL,
    notes         TEXT,
    created_by_id UUID REFERENCES users(id),
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX ix_advance_patient ON advance_payments(patient_id);

CREATE TABLE commission_records (
    id                UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    doctor_id         UUID NOT NULL REFERENCES users(id),
    invoice_id        UUID NOT NULL REFERENCES invoices(id),
    payment_id        UUID NOT NULL REFERENCES payments(id),
    procedure_id      UUID,        -- nullable: Phase 4 = invoice-level
    commission_method VARCHAR(30) NOT NULL,
    base_amount       NUMERIC(12,2) NOT NULL,
    commission_rate   NUMERIC(10,4) NOT NULL DEFAULT 0,
    commission_amount NUMERIC(12,2) NOT NULL DEFAULT 0,
    is_paid           BOOLEAN NOT NULL DEFAULT FALSE,
    paid_at           TIMESTAMPTZ,
    vault_transaction_id UUID REFERENCES vault_transactions(id),
    created_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX ix_commission_doctor ON commission_records(doctor_id, is_paid);
```

---

## 5. Domain Entities (12)

| Entity | Aggregate | الخصائص الرئيسية |
|--------|-----------|-----------------|
| ServiceCategory | ServiceCategory | name, sort_order, is_active |
| MedicalService | MedicalService | category_id, name, code, price, soft-delete |
| Vault | Vault | name, type (cash/bank/card/pos), opening_balance |
| DoctorProfile | DoctorProfile | user_id, commission_method, default_value |
| Invoice | Invoice (Root) | invoice_number, status machine, Items collection |
| InvoiceItem | (Invoice) | procedure_id (UUID no FK), service_name snapshot |
| Payment | Payment | invoice_id, vault_id, amount, method |
| VaultTransaction | VaultTransaction | direction (in/out), type, related_* |
| InstallmentPlan | InstallmentPlan | invoice_id, installments_count |
| InstallmentPayment | (Plan) | due_date, status (Pending/Paid/Overdue) |
| AdvancePayment | AdvancePayment | patient_id, amount, remaining |
| CommissionRecord | CommissionRecord | doctor_id, Cash-Basis, is_paid |

---

## 6. Features (CQRS Handlers)

| Handler | نوع | وصف |
|---------|-----|-----|
| GetServicesQuery | Query | قائمة خدمات مع تصفية |
| CreateServiceCommand | Command | إنشاء خدمة + تصنيف |
| UpdateServiceCommand | Command | تعديل سعر + بيانات |
| GetVaultBalancesQuery | Query | أرصدة كل الخزائن المحسوبة |
| CreateInvoiceCommand | Command | فاتورة + بنود + timeline |
| GetInvoiceQuery | Query | فاتورة كاملة + دفعات + متبقي |
| GetInvoicesQuery | Query | قائمة مصفاة مع pagination |
| CancelInvoiceCommand | Command | إلغاء مع سبب |
| AddPaymentCommand | Command | دفعة → vault_tx → commission |
| CreateInstallmentPlanCommand | Command | خطة أقساط |
| PayInstallmentCommand | Command | دفع قسط |
| CreateAdvancePaymentCommand | Command | دفعة مقدمة |
| GetDoctorAccountQuery | Query | كشف حساب طبيب |
| PayDoctorCommissionCommand | Command | دفع عمولة مستحقة |

---

## 7. Commission Engine

```
DoctorProfile.commission_method:
  percentage_of_service  → commission = paidAmount × rate / 100
  fixed_amount           → commission = fixed_value
  percentage_of_net_service → net = paidAmount - lab_cost; commission = net × rate / 100

Trigger: AddPayment → Invoice.status = Paid
         → ICommissionEngine.CalculateAsync(doctorId, invoiceId, paymentId, totalPaid, labCost=0)
         → CommissionRecord { is_paid: false }
```

---

## 8. Frontend Screens

| الشاشة | المسار | الوصف |
|--------|--------|-------|
| **S13** — كتالوج الخدمات | `/settings/services` | قائمة + CRUD + تصنيفات |
| **S14** — الخزائن | `/settings/vaults` | قائمة خزائن + أرصدة |
| **S15** — الفواتير | `/finance/invoices` | قائمة + فلاتر + حالة |
| **S16** — تفاصيل الفاتورة | `/finance/invoices/[id]` | فاتورة + بنود + دفعات + Modal دفع |
| **S17** — الخزينة | `/finance/treasury` | أرصدة الخزائن + حركات |
| **S18** — الأقساط | `/finance/installments` | خطط أقساط + دفع |
| **S19** — حساب الطبيب | `/finance/doctors/[id]/account` | كشف حساب + عمولات |

---

## 9. API Endpoints الجديدة

| Method | Path | الصلاحية |
|--------|------|---------|
| GET | `/api/services` | `Invoice.View` |
| POST | `/api/services` | `System.Settings` |
| PUT | `/api/services/{id}` | `System.Settings` |
| GET | `/api/vaults/balances` | `Treasury.Add` |
| POST | `/api/invoices` | `Invoice.Create` |
| GET | `/api/invoices` | `Invoice.Create` |
| GET | `/api/invoices/{id}` | `Invoice.Create` |
| POST | `/api/invoices/{id}/cancel` | `Invoice.Cancel` |
| POST | `/api/invoices/{id}/payments` | `Treasury.Add` |
| POST | `/api/installments/plans` | `Invoice.Create` |
| POST | `/api/installments/{planId}/pay/{num}` | `Treasury.Add` |
| POST | `/api/advance-payments` | `Treasury.Add` |
| GET | `/api/treasury/doctors/{id}/account` | `Treasury.Add` |
| POST | `/api/treasury/commissions/{id}/pay` | `Treasury.Add` |

---

## 10. التحقق من المتطلبات

| المطلب | الحالة |
|--------|--------|
| TreatmentPlanItem.tooth_id ✅ | موجود (Phase 3) |
| TreatmentPlanItem.surface ✅ | موجود (Phase 3) |
| material_cost | مؤجل Phase 5 (مخزون) |
| lab_cost في procedures | ✅ يُضاف في Migration 013 |
| estimated_cost في TreatmentPlanItem | ✅ موجود كـ unit_price × quantity |

---

**🔴 لا يُبدأ Phase 5 قبل الموافقة الصريحة على Phase 4.**
