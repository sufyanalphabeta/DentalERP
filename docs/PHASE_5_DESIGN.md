# PHASE 5 DESIGN DOCUMENT

**Date:** 2026-06-17  
**Phase:** 5 — Laboratory, Radiology, Insurance Accounts  
**Status:** DESIGN — awaiting approval before implementation

---

## Pre-Phase 5 Verification Results

The following four items were verified against the current codebase before this design was produced.

### 1. Vault-to-Vault Transfer
**Finding:** `VaultTransaction.ValidTypes` includes `"inter_vault_transfer"` (defined in migration 017) but no command handler, endpoint, or workflow was implemented in Phase 4.  
**Resolution:** A dedicated `TransferBetweenVaults` command + endpoint is included in Phase 5 Financial Enhancements (migration 022).

### 2. Doctor Financial Statement
**Finding:** `GetDoctorAccountQueryHandler` returns commission records with date-range filtering (From/To). It does **not** include: Invoice Revenue per doctor, PaidAmount on those invoices, Remaining, or PDF export.  
**Resolution:** `GetDoctorFinancialStatement` query is added in Phase 5 — joins `invoices` + `commission_records` to produce a full statement. PDF export via a server-side endpoint is included.

### 3. Invoice Lifecycle → audit_logs + patient_timeline
**Finding:** `audit_logs` (migration 003) and `patient_timeline` (migration 012, category `'Financial'`) both exist with correct schema. However, `AddPaymentCommandHandler`, `CancelInvoiceCommandHandler`, and `CreateInvoiceCommandHandler` do **not** write to either table.  
**Resolution:** Phase 5 includes a `FinancialEventPublisher` service that writes to both tables from all Financial command handlers. This is a Financial module backfill, delivered in Phase 5 alongside the new modules.

### 4. Insurance Scope
**Confirmed:** Insurance is limited to:
- Insurance Companies (lookup)
- Insurance Claims (per invoice)
- Insurance Receivables (computed)
- Insurance Payments (received from company)

**Explicitly excluded:** Pre-Authorization, Coverage Engine, Policy Rules, TPA Workflow, Claim Adjudication.

---

## Scope Summary

| # | Module | Type | New Migrations |
|---|--------|------|----------------|
| 1 | Laboratory | Core V1 | 019 |
| 2 | Radiology | Core V1 | 020 |
| 3 | Insurance Accounts | Accounting | 021 |
| — | Financial Enhancements (backfill) | Phase 4 backfill | 022 |

**New screens:** S20–S29 (10 screens)  
**New migrations:** 4 files (019–022)  
**New backend module:** `DentalERP.Modules.Laboratory`, `DentalERP.Modules.Radiology`  
**Financial module additions:** Insurance sub-feature + event publisher + vault transfer + doctor statement

---

## Module 1: Laboratory

### Business Context
The dental clinic outsources lab work (crowns, bridges, dentures, orthodontic appliances) to external labs. A lab order tracks what was sent, to whom, at what cost, and when results were received. Lab cost feeds the commission engine's `percentage_of_net_service` deduction.

### Business Rules

1. A lab order is always linked to a patient. It may optionally link to a `procedure_id` (cross-module UUID, no FK).
2. Lab order statuses: `Draft → Sent → InProgress → ResultReceived → Completed → Cancelled`.
3. Only `Draft` orders can be edited or cancelled.
4. A lab order can have one or more **items** (individual tests/work units) with individual costs.
5. Total lab cost = sum of all item costs.
6. When a lab order is linked to a procedure, the system updates `procedures.lab_cost` with the total cost. This affects the commission engine for `percentage_of_net_service`.
7. Lab result is recorded as text notes + optional file attachment (stored path, same pattern as patient_media).
8. Status transitions to `ResultReceived` automatically when a result is recorded.
9. Lab order creation → `patient_timeline` (category: `Laboratory`, event_type: `lab_order_created`).
10. Result received → `patient_timeline` (event_type: `lab_result_received`).
11. All state changes → `audit_logs`.
12. Lab order number format: `LAB-YYYY-000001` (year-based sequence).

### Database DDL — Migration 019

```sql
-- Migration 019: Laboratory Module

BEGIN;

-- External Labs (reference table)
CREATE TABLE IF NOT EXISTS external_labs (
    id           UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    name         VARCHAR(200) NOT NULL,
    contact_name VARCHAR(100),
    phone        VARCHAR(30),
    email        VARCHAR(150),
    address      TEXT,
    notes        TEXT,
    is_active    BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at   TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Lab Orders
CREATE TABLE IF NOT EXISTS lab_orders (
    id                UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    order_number      VARCHAR(30)  NOT NULL UNIQUE,
    patient_id        UUID         NOT NULL REFERENCES patients(id),
    doctor_id         UUID         NOT NULL REFERENCES users(id),
    lab_id            UUID         REFERENCES external_labs(id),
    procedure_id      UUID,          -- cross-module: no FK
    status            VARCHAR(20)  NOT NULL DEFAULT 'Draft'
        CHECK (status IN ('Draft','Sent','InProgress','ResultReceived','Completed','Cancelled')),
    description       TEXT,
    sent_at           TIMESTAMPTZ,
    expected_at       DATE,
    received_at       TIMESTAMPTZ,
    total_cost        NUMERIC(12,2) NOT NULL DEFAULT 0,
    currency          CHAR(3)      NOT NULL DEFAULT 'LYD',
    notes             TEXT,
    cancelled_reason  TEXT,
    created_by_id     UUID         REFERENCES users(id),
    created_at        TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at        TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_lab_orders_patient  ON lab_orders(patient_id);
CREATE INDEX IF NOT EXISTS ix_lab_orders_status   ON lab_orders(status) WHERE status NOT IN ('Completed','Cancelled');
CREATE INDEX IF NOT EXISTS ix_lab_orders_doctor   ON lab_orders(doctor_id);

-- Lab Order Items
CREATE TABLE IF NOT EXISTS lab_order_items (
    id            UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id      UUID          NOT NULL REFERENCES lab_orders(id) ON DELETE CASCADE,
    item_name     VARCHAR(200)  NOT NULL,
    description   TEXT,
    quantity      SMALLINT      NOT NULL DEFAULT 1 CHECK (quantity > 0),
    unit_cost     NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_cost    NUMERIC(12,2) NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS ix_lab_items_order ON lab_order_items(order_id);

-- Lab Results
CREATE TABLE IF NOT EXISTS lab_results (
    id           UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id     UUID         NOT NULL REFERENCES lab_orders(id),
    result_notes TEXT,
    file_path    VARCHAR(500),    -- nullable: text-only result allowed
    file_name    VARCHAR(200),
    file_size    BIGINT,
    received_by_id UUID        REFERENCES users(id),
    received_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_lab_results_order ON lab_results(order_id);

COMMIT;
```

### CQRS Handlers

| Handler | Type | Description |
|---------|------|-------------|
| `GetLabOrdersQuery` | Query | List with filters: patient, status, doctor, date range |
| `GetLabOrderByIdQuery` | Query | Full detail: items + results |
| `CreateLabOrderCommand` | Command | Creates order + items; generates order number |
| `UpdateLabOrderCommand` | Command | Edit only in `Draft` status |
| `SendLabOrderCommand` | Command | Status: Draft → Sent; records `sent_at` |
| `RecordLabResultCommand` | Command | Records result + optional file; status → `ResultReceived` |
| `CompleteLabOrderCommand` | Command | Status: ResultReceived → Completed |
| `CancelLabOrderCommand` | Command | Status: Draft only → Cancelled |
| `GetExternalLabsQuery` | Query | List active external labs |
| `CreateExternalLabCommand` | Command | Add new external lab |

### API Endpoints

```
GET    /api/lab/orders                         — list with filters
POST   /api/lab/orders                         — create order
GET    /api/lab/orders/{id}                    — order detail
PUT    /api/lab/orders/{id}                    — update (Draft only)
POST   /api/lab/orders/{id}/send               — mark as Sent
POST   /api/lab/orders/{id}/result             — record result
POST   /api/lab/orders/{id}/complete           — mark Completed
POST   /api/lab/orders/{id}/cancel             — cancel (Draft only)
GET    /api/lab/external-labs                  — list labs
POST   /api/lab/external-labs                  — create lab
```

All endpoints require `Authorization`.

### Financial Integration

- `lab_order_items.total_cost` sums to `lab_orders.total_cost`.
- When `procedure_id` is set, `CompleteLabOrderCommand` updates `procedures.lab_cost = lab_orders.total_cost` via cross-module update (direct DB update, no FK).
- The Commission Engine's `percentage_of_net_service` reads `lab_cost` from the payment context — this is already wired in `AddPaymentCommandHandler`. No change needed there; updating `procedures.lab_cost` is sufficient.
- Lab orders themselves do **not** generate invoices — the procedure's `final_price` drives the invoice item. Lab cost is a cost deduction for commission only.

### Timeline Integration

| Event | Category | Trigger |
|-------|----------|---------|
| `lab_order_created` | `Laboratory` | `CreateLabOrderCommand` success |
| `lab_order_sent` | `Laboratory` | `SendLabOrderCommand` success |
| `lab_result_received` | `Laboratory` | `RecordLabResultCommand` success |
| `lab_order_completed` | `Laboratory` | `CompleteLabOrderCommand` success |

### Frontend Screens

**S20 — `/patients/[id]/lab-orders`** — Lab Orders per patient  
- Table: order number, lab name, description, status badge, cost, date  
- Button: New Lab Order → modal  
- Click row → S21

**S21 — `/lab/orders/[id]`** — Lab Order Detail  
- Header: order number, status, patient, doctor, external lab  
- Items table with costs  
- Result section: show recorded result + file download  
- Actions: Send, Record Result, Complete, Cancel

---

## Module 2: Radiology

### Business Context
Dental radiology covers intra-oral X-rays, OPG (panoramic), CBCT, bitewing, and periapical films. The clinic may have its own radiology unit or refer to external centers. Images are stored as file references. A radiologist (or treating doctor) writes a text report.

### Business Rules

1. A radiology order is always linked to a patient. Optional `procedure_id` link (cross-module UUID).
2. Radiology order statuses: `Ordered → InProgress → Imaged → Reported → Completed → Cancelled`.
3. Only `Ordered` orders can be cancelled or edited.
4. One order can have **multiple images** (multi-angle, multiple views).
5. One **report** per order (can be updated while status is `Reported`).
6. Status advances to `Imaged` when the first image is uploaded.
7. Status advances to `Reported` when a report is saved.
8. `Completed` is set manually by the treating doctor after reviewing the report.
9. Radiology order number format: `RAD-YYYY-000001`.
10. Order creation + completion → `patient_timeline` (category: `Radiology`).
11. All status transitions → `audit_logs`.
12. Radiology costs can optionally be tracked per order and linked to an invoice item (same cross-module UUID pattern).

### Database DDL — Migration 020

```sql
-- Migration 020: Radiology Module

BEGIN;

-- Radiology Types (lookup)
CREATE TABLE IF NOT EXISTS radiology_types (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    is_active   BOOLEAN     NOT NULL DEFAULT TRUE
);

INSERT INTO radiology_types (name, description) VALUES
    ('بانوراما (OPG)',       'أشعة بانورامية شاملة'),
    ('CBCT',                'أشعة مقطعية ثلاثية الأبعاد'),
    ('أشعة عضية',           'Periapical X-Ray'),
    ('أشعة جناحية',         'Bitewing X-Ray'),
    ('أشعة إطباقية',        'Occlusal X-Ray'),
    ('أشعة جانبية',         'Lateral Cephalometric')
ON CONFLICT DO NOTHING;

-- Radiology Orders
CREATE TABLE IF NOT EXISTS radiology_orders (
    id               UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    order_number     VARCHAR(30)  NOT NULL UNIQUE,
    patient_id       UUID         NOT NULL REFERENCES patients(id),
    doctor_id        UUID         NOT NULL REFERENCES users(id),
    radiology_type_id UUID        REFERENCES radiology_types(id),
    procedure_id     UUID,          -- cross-module: no FK
    status           VARCHAR(20)  NOT NULL DEFAULT 'Ordered'
        CHECK (status IN ('Ordered','InProgress','Imaged','Reported','Completed','Cancelled')),
    clinical_notes   TEXT,          -- reason / clinical indication
    cost             NUMERIC(12,2) NOT NULL DEFAULT 0,
    currency         CHAR(3)      NOT NULL DEFAULT 'LYD',
    cancelled_reason TEXT,
    created_by_id    UUID         REFERENCES users(id),
    created_at       TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_rad_orders_patient ON radiology_orders(patient_id);
CREATE INDEX IF NOT EXISTS ix_rad_orders_status  ON radiology_orders(status) WHERE status NOT IN ('Completed','Cancelled');
CREATE INDEX IF NOT EXISTS ix_rad_orders_doctor  ON radiology_orders(doctor_id);

-- Radiology Images
CREATE TABLE IF NOT EXISTS radiology_images (
    id           UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id     UUID         NOT NULL REFERENCES radiology_orders(id) ON DELETE CASCADE,
    file_path    VARCHAR(500) NOT NULL,
    file_name    VARCHAR(200) NOT NULL,
    file_size    BIGINT,
    mime_type    VARCHAR(50),
    view_label   VARCHAR(100),   -- e.g. "Upper Right Quadrant"
    taken_at     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    uploaded_by_id UUID       REFERENCES users(id)
);

CREATE INDEX IF NOT EXISTS ix_rad_images_order ON radiology_images(order_id);

-- Radiology Reports
CREATE TABLE IF NOT EXISTS radiology_reports (
    id              UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id        UUID         NOT NULL REFERENCES radiology_orders(id) UNIQUE,
    report_text     TEXT         NOT NULL,
    findings        TEXT,
    impression      TEXT,
    reported_by_id  UUID         REFERENCES users(id),
    reported_at     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_rad_reports_order ON radiology_reports(order_id);

COMMIT;
```

### CQRS Handlers

| Handler | Type | Description |
|---------|------|-------------|
| `GetRadiologyOrdersQuery` | Query | List with filters: patient, type, status, date range |
| `GetRadiologyOrderByIdQuery` | Query | Full detail: images + report |
| `CreateRadiologyOrderCommand` | Command | Creates order; generates order number |
| `UpdateRadiologyOrderCommand` | Command | Edit clinical notes/type (Ordered only) |
| `UploadRadiologyImageCommand` | Command | Add image file reference; status → Imaged |
| `SaveRadiologyReportCommand` | Command | Create or update report; status → Reported |
| `CompleteRadiologyOrderCommand` | Command | Status: Reported → Completed |
| `CancelRadiologyOrderCommand` | Command | Status: Ordered only → Cancelled |
| `GetRadiologyTypesQuery` | Query | List active radiology types |

### API Endpoints

```
GET    /api/radiology/orders                   — list with filters
POST   /api/radiology/orders                   — create order
GET    /api/radiology/orders/{id}              — order detail
PUT    /api/radiology/orders/{id}              — update (Ordered only)
POST   /api/radiology/orders/{id}/images       — upload image
DELETE /api/radiology/orders/{id}/images/{imgId} — remove image
POST   /api/radiology/orders/{id}/report       — save/update report
POST   /api/radiology/orders/{id}/complete     — mark Completed
POST   /api/radiology/orders/{id}/cancel       — cancel (Ordered only)
GET    /api/radiology/types                    — list radiology types
```

All endpoints require `Authorization`.

### Financial Integration

- `radiology_orders.cost` is informational cost tracking.
- When `procedure_id` is set, `CompleteRadiologyOrderCommand` can update the linked procedure's price context if needed (business decision: left as optional in V1).
- Radiology orders do **not** directly generate invoice items in V1. The treating doctor adds radiology charges manually as invoice items. The `procedure_id` link provides traceability.

### Timeline Integration

| Event | Category | Trigger |
|-------|----------|---------|
| `radiology_order_created` | `Radiology` | `CreateRadiologyOrderCommand` success |
| `radiology_imaged` | `Radiology` | `UploadRadiologyImageCommand` success (first image only) |
| `radiology_report_saved` | `Radiology` | `SaveRadiologyReportCommand` success |
| `radiology_order_completed` | `Radiology` | `CompleteRadiologyOrderCommand` success |

### Frontend Screens

**S22 — `/patients/[id]/radiology`** — Radiology Orders per patient  
- Table: order number, type, clinical notes, status badge, cost, date  
- Button: New Radiology Order → modal  
- Click row → S23

**S23 — `/radiology/orders/[id]`** — Radiology Order Detail  
- Header: order number, type, patient, doctor, status  
- Images grid: thumbnails with view labels, upload button  
- Report section: editable text report with findings + impression  
- Actions: Complete, Cancel

---

## Module 3: Insurance Accounts

### Business Context
The clinic deals with insurance companies that partially cover patient invoices. Insurance is purely an **accounting module** in V1: record which company owes what, track claims submitted, record payments received. No coverage rules, no eligibility checks, no pre-authorization.

### Business Rules

1. `insurance_companies` is a lookup table managed by admin.
2. A patient may be linked to one insurance company (`patients.insurance_company_id` — already added in migration 005).
3. An **insurance claim** is created against an invoice to declare the amount the insurance company owes.
4. One invoice can have **at most one** insurance claim.
5. Claim statuses: `Draft → Submitted → PartiallyPaid → Paid → Rejected`.
6. The claim amount + patient-paid amount must **not exceed** invoice total. Validation at claim creation.
7. An **insurance payment** records money actually received from the insurance company.
8. Receiving an insurance payment calls `invoice.ApplyPayment(amount)` (same mechanism as patient payment) + creates a `VaultTransaction` (type: `general_receipt`, direction: `in`).
9. Insurance receivable = `claim_amount − insurance_paid_amount` (computed, not stored).
10. Commission engine: commission is calculated on the full invoice amount when it becomes `Paid` regardless of whether payment came from patient or insurance. Business rule: commission is on the service, not the payer.
11. Claim creation → `patient_timeline` (category: `Insurance`, event_type: `insurance_claim_submitted`).
12. Insurance payment → `patient_timeline` (event_type: `insurance_payment_received`).

### Database DDL — Migration 021

```sql
-- Migration 021: Insurance Accounts

BEGIN;

-- Insurance Companies
CREATE TABLE IF NOT EXISTS insurance_companies (
    id           UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    name         VARCHAR(200) NOT NULL,
    short_code   VARCHAR(20)  UNIQUE,
    contact_name VARCHAR(100),
    phone        VARCHAR(30),
    email        VARCHAR(150),
    address      TEXT,
    notes        TEXT,
    is_active    BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at   TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Add FK from patients to insurance_companies (column already exists from migration 005)
ALTER TABLE patients
    ADD CONSTRAINT fk_patients_insurance_company
    FOREIGN KEY (insurance_company_id)
    REFERENCES insurance_companies(id)
    DEFERRABLE INITIALLY DEFERRED;

-- Insurance Claims
CREATE TABLE IF NOT EXISTS insurance_claims (
    id                  UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    claim_number        VARCHAR(30)   NOT NULL UNIQUE,
    invoice_id          UUID          NOT NULL REFERENCES invoices(id) UNIQUE,  -- 1 claim per invoice
    patient_id          UUID          NOT NULL REFERENCES patients(id),
    company_id          UUID          NOT NULL REFERENCES insurance_companies(id),
    claim_amount        NUMERIC(12,2) NOT NULL CHECK (claim_amount > 0),
    paid_amount         NUMERIC(12,2) NOT NULL DEFAULT 0,
    status              VARCHAR(20)   NOT NULL DEFAULT 'Draft'
        CHECK (status IN ('Draft','Submitted','PartiallyPaid','Paid','Rejected')),
    submission_date     DATE,
    rejection_reason    TEXT,
    notes               TEXT,
    created_by_id       UUID          REFERENCES users(id),
    created_at          TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_claims_company  ON insurance_claims(company_id, status);
CREATE INDEX IF NOT EXISTS ix_claims_patient  ON insurance_claims(patient_id);
CREATE INDEX IF NOT EXISTS ix_claims_invoice  ON insurance_claims(invoice_id);

-- Insurance Payments
CREATE TABLE IF NOT EXISTS insurance_payments (
    id             UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    claim_id       UUID          NOT NULL REFERENCES insurance_claims(id),
    company_id     UUID          NOT NULL REFERENCES insurance_companies(id),
    vault_id       UUID          NOT NULL REFERENCES vaults(id),
    amount         NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    payment_method VARCHAR(20)   NOT NULL
        CHECK (payment_method IN ('cash','bank_transfer','card','pos','cheque')),
    reference_number VARCHAR(50),
    notes          TEXT,
    received_by_id UUID          REFERENCES users(id),
    received_at    TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_ins_payments_claim   ON insurance_payments(claim_id);
CREATE INDEX IF NOT EXISTS ix_ins_payments_company ON insurance_payments(company_id);

-- Claim number sequence helper
CREATE SEQUENCE IF NOT EXISTS ins_claim_seq START 1;

COMMIT;
```

### CQRS Handlers

| Handler | Type | Description |
|---------|------|-------------|
| `GetInsuranceCompaniesQuery` | Query | List active companies |
| `CreateInsuranceCompanyCommand` | Command | Add company |
| `UpdateInsuranceCompanyCommand` | Command | Edit company details |
| `GetInsuranceClaimsQuery` | Query | List with filters: company, status, date range |
| `GetInsuranceClaimByIdQuery` | Query | Claim detail + payments |
| `CreateInsuranceClaimCommand` | Command | Create claim for invoice; validates total |
| `SubmitInsuranceClaimCommand` | Command | Status: Draft → Submitted; sets submission_date |
| `RecordInsurancePaymentCommand` | Command | Receive payment; calls `invoice.ApplyPayment`; updates `claim.paid_amount`; creates VaultTransaction |
| `RejectInsuranceClaimCommand` | Command | Status → Rejected; records reason |
| `GetCompanyReceivablesQuery` | Query | Receivables per company: total claimed, total paid, balance |

### API Endpoints

```
GET    /api/insurance/companies                — list companies
POST   /api/insurance/companies                — create company
PUT    /api/insurance/companies/{id}           — update company
GET    /api/insurance/claims                   — list claims (filters)
POST   /api/insurance/claims                   — create claim
GET    /api/insurance/claims/{id}              — claim detail
POST   /api/insurance/claims/{id}/submit       — submit claim
POST   /api/insurance/claims/{id}/payments     — record insurance payment
POST   /api/insurance/claims/{id}/reject       — reject claim
GET    /api/insurance/receivables              — receivables summary per company
```

All endpoints require `Authorization`.

### Financial Integration

- `RecordInsurancePaymentCommand` calls `invoice.ApplyPayment(amount)` — the same domain method used by patient payments.
- Creates `VaultTransaction` (type: `general_receipt`, direction: `in`, `related_invoice_id` set).
- Commission engine fires when `invoice.Status == "Paid"` — works identically regardless of whether the final payment came from patient or insurance.
- Insurance receivable is a reporting concept: `claim_amount − SUM(insurance_payments.amount)`.
- Insurance paid amount does NOT appear separately in the invoice — it flows through the same `PaidAmount` / `Remaining` mechanism.

### Timeline Integration

| Event | Category | Trigger |
|-------|----------|---------|
| `insurance_claim_created` | `Insurance` | `CreateInsuranceClaimCommand` |
| `insurance_claim_submitted` | `Insurance` | `SubmitInsuranceClaimCommand` |
| `insurance_payment_received` | `Insurance` | `RecordInsurancePaymentCommand` |
| `insurance_claim_rejected` | `Insurance` | `RejectInsuranceClaimCommand` |

### Frontend Screens

**S26 — `/settings/insurance`** — Insurance Companies CRUD  
- Table: name, short code, phone, status badge  
- Modal: add/edit company

**S27 — `/finance/insurance/claims`** — Claims List  
- Filters: company, status, date range  
- Table: claim number, patient, company, claimed amount, paid amount, receivable, status  
- Click row → S28

**S28 — `/finance/insurance/claims/[id]`** — Claim Detail  
- Invoice reference, claim amount, receivable  
- Payments history table  
- Actions: Submit, Record Payment, Reject

**S29 — `/finance/insurance/receivables`** — Receivables Dashboard  
- Per-company summary: total claimed, total paid, outstanding balance  
- Exportable table

---

## Phase 5 Financial Enhancements (Backfill + New)

### Migration 022 — Vault-to-Vault Transfer

```sql
-- Migration 022: Vault Transfer workflow

BEGIN;

CREATE TABLE IF NOT EXISTS vault_transfers (
    id                 UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    transfer_number    VARCHAR(30)   NOT NULL UNIQUE,
    from_vault_id      UUID          NOT NULL REFERENCES vaults(id),
    to_vault_id        UUID          NOT NULL REFERENCES vaults(id),
    amount             NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    notes              TEXT,
    created_by_id      UUID          REFERENCES users(id),
    created_at         TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    CHECK (from_vault_id <> to_vault_id)
);

CREATE INDEX IF NOT EXISTS ix_vault_transfers_from ON vault_transfers(from_vault_id);
CREATE INDEX IF NOT EXISTS ix_vault_transfers_to   ON vault_transfers(to_vault_id);

COMMIT;
```

**Workflow:** `CreateVaultTransferCommand` creates the `vault_transfers` record + two `VaultTransaction` rows: one `inter_vault_transfer / out` on the source vault, one `inter_vault_transfer / in` on the target vault. Both are committed in a single transaction.

**Endpoint:** `POST /api/treasury/vaults/transfer`  
**Screen S25 — `/finance/treasury`:** Add transfer modal to existing treasury dashboard.

### Invoice Lifecycle → audit_logs + patient_timeline

A `FinancialEventPublisher` service is added to `DentalERP.Modules.Financial`:

```csharp
public interface IFinancialEventPublisher {
    Task PublishInvoiceCreatedAsync(Invoice invoice, Guid actorId, CancellationToken ct);
    Task PublishPaymentReceivedAsync(Invoice invoice, decimal amount, Guid actorId, CancellationToken ct);
    Task PublishInvoiceCancelledAsync(Invoice invoice, Guid actorId, CancellationToken ct);
    Task PublishInvoiceConfirmedAsync(Invoice invoice, Guid actorId, CancellationToken ct);
}
```

Injected into:
- `CreateInvoiceCommandHandler` → publishes `invoice_created` (audit + timeline)
- `AddPaymentCommandHandler` → publishes `payment_received` (audit + timeline)
- `CancelInvoiceCommandHandler` → publishes `invoice_cancelled` (audit + timeline)
- `ConfirmInvoiceCommandHandler` → publishes `invoice_confirmed` (audit + timeline)

Timeline event types written:

| event_type | category | linked_entity_type |
|------------|----------|--------------------|
| `invoice_created` | `Financial` | `Invoice` |
| `invoice_confirmed` | `Financial` | `Invoice` |
| `payment_received` | `Financial` | `Invoice` |
| `invoice_cancelled` | `Financial` | `Invoice` |

### Doctor Financial Statement

`GetDoctorFinancialStatementQuery` — new query on `FinancialDbContext`:

**Parameters:** `doctorId`, `from?`, `to?`

**Response:**
```json
{
  "doctorId": "...",
  "doctorName": "...",
  "period": { "from": "...", "to": "..." },
  "revenue": {
    "totalInvoiced": 15000.00,
    "totalPaid": 12000.00,
    "totalRemaining": 3000.00,
    "invoiceCount": 42
  },
  "commissions": {
    "totalEarned": 1200.00,
    "totalPaid": 900.00,
    "totalUnpaid": 300.00,
    "recordCount": 38
  },
  "invoices": [ ... ],
  "commissionRecords": [ ... ]
}
```

**Endpoint:** `GET /api/treasury/doctors/{id}/statement?from=&to=`  
**PDF Export:** `GET /api/treasury/doctors/{id}/statement/pdf?from=&to=`  
Server-side PDF rendered with a simple HTML-to-PDF approach (QuestPDF or equivalent).

**Screen S24 — `/finance/doctors/[id]/account`** (replaces/extends S19): Tab layout — Commissions tab (Phase 4) + Financial Statement tab (Phase 5) with date-range picker and PDF download button.

---

## Screen Inventory — Phase 5

| Screen | Route | Module |
|--------|-------|--------|
| S20 | `/patients/[id]/lab-orders` | Laboratory |
| S21 | `/lab/orders/[id]` | Laboratory |
| S22 | `/patients/[id]/radiology` | Radiology |
| S23 | `/radiology/orders/[id]` | Radiology |
| S24 | `/finance/doctors/[id]/account` (extended) | Financial Enhancement |
| S25 | `/finance/treasury` (vault transfer modal added) | Financial Enhancement |
| S26 | `/settings/insurance` | Insurance |
| S27 | `/finance/insurance/claims` | Insurance |
| S28 | `/finance/insurance/claims/[id]` | Insurance |
| S29 | `/finance/insurance/receivables` | Insurance |

**Total new/modified screens: 10**

---

## API Contracts Summary

### New Modules

| Prefix | Module |
|--------|--------|
| `/api/lab/*` | Laboratory |
| `/api/radiology/*` | Radiology |
| `/api/insurance/*` | Insurance Accounts |

### Financial Enhancements

| Endpoint | Purpose |
|----------|---------|
| `POST /api/treasury/vaults/transfer` | Vault-to-vault transfer |
| `GET /api/treasury/doctors/{id}/statement` | Full financial statement |
| `GET /api/treasury/doctors/{id}/statement/pdf` | PDF export |

---

## Reporting Requirements

### Laboratory Reports

| Report | Description |
|--------|-------------|
| Lab Orders by Status | Count + cost grouped by status and external lab |
| Pending Lab Orders | All orders not yet `Completed` or `Cancelled` |
| Lab Cost by Doctor | Total lab costs per doctor (for commission context) |
| Overdue Lab Orders | Orders where `expected_at < TODAY` and status not Received/Completed |

### Radiology Reports

| Report | Description |
|--------|-------------|
| Radiology Orders by Type | Count + cost grouped by radiology type |
| Pending Reports | Orders in `Imaged` status without a report |
| Radiology by Patient | All radiology orders for a patient (timeline view) |

### Insurance Reports

| Report | Description |
|--------|-------------|
| Receivables by Company | Per-company: claimed, paid, outstanding balance |
| Claims Aging | Claims by submission date brackets (0–30, 31–60, 60+ days) |
| Insurance Cash Collection | Payments received per company per period |
| Rejection Rate | Count + amount of rejected claims per company |

### Doctor Financial Statement

| Field | Source |
|-------|--------|
| Revenue (Invoiced) | `invoices.total_amount WHERE doctor_id = ?` |
| Revenue (Collected) | `invoices.paid_amount WHERE doctor_id = ?` |
| Revenue (Remaining) | `invoices.total_amount - invoices.paid_amount WHERE doctor_id = ?` |
| Commissions Earned | `commission_records.commission_amount WHERE doctor_id = ?` |
| Commissions Paid | `commission_records WHERE is_paid = true AND doctor_id = ?` |
| Commissions Unpaid | Earned − Paid |

All reports support `date_from` / `date_to` filtering.

---

## Migration Plan

| Migration | File | Contents |
|-----------|------|----------|
| 019 | `019_laboratory.sql` | `external_labs`, `lab_orders`, `lab_order_items`, `lab_results` |
| 020 | `020_radiology.sql` | `radiology_types` (+ seed), `radiology_orders`, `radiology_images`, `radiology_reports` |
| 021 | `021_insurance_accounts.sql` | `insurance_companies`, FK on patients, `insurance_claims`, `insurance_payments` |
| 022 | `022_vault_transfers.sql` | `vault_transfers` |

---

## New Backend Projects

| Project | Module |
|---------|--------|
| `DentalERP.Modules.Laboratory` | Lab orders, results, external labs |
| `DentalERP.Modules.Radiology` | Radiology orders, images, reports |
| Financial module additions | Insurance + vault transfer + event publisher + doctor statement |

---

## Implementation Order (Recommended)

Phase 5 should be implemented in this sequence to manage dependencies:

1. **Migration 022** — Vault Transfers (no module dependency)
2. **Financial backfill** — `FinancialEventPublisher` (audit + timeline writes)
3. **Migration 019 + Laboratory module** — standalone, no cross-module deps beyond patients/users
4. **Migration 020 + Radiology module** — standalone, same pattern as lab
5. **Migration 021 + Insurance module** — depends on `invoices` and `vaults` (Financial module)
6. **Doctor Financial Statement** — query enhancement on existing tables
7. **PDF Export** — depends on statement query being stable
8. **Frontend screens S20–S29**
9. **Unit tests** (target: 80+ new tests)
10. **Integration tests** (auth guards: 401 tests per endpoint group)
11. **Build validation**
12. **PHASE_5_COMPLETION_REPORT.md**

---

## Constraint

> **Do not start implementation before approval of this document.**
