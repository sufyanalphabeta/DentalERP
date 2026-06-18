-- Migration 016: Invoices + Invoice Items

BEGIN;

CREATE TABLE IF NOT EXISTS invoices (
    id               UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_number   VARCHAR(30)   NOT NULL UNIQUE,
    patient_id       UUID          NOT NULL REFERENCES patients(id),
    doctor_id        UUID          NOT NULL REFERENCES users(id),
    status           VARCHAR(20)   NOT NULL DEFAULT 'Draft'
        CHECK (status IN ('Draft','Confirmed','PartiallyPaid','Paid','Cancelled')),
    subtotal         NUMERIC(12,2) NOT NULL DEFAULT 0,
    discount_total   NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_amount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    paid_amount      NUMERIC(12,2) NOT NULL DEFAULT 0,
    currency         CHAR(3)       NOT NULL DEFAULT 'LYD',
    notes            TEXT,
    cancelled_reason TEXT,
    created_by_id    UUID          REFERENCES users(id),
    created_at       TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ,
    deleted_at       TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_invoices_patient
    ON invoices(patient_id);

CREATE INDEX IF NOT EXISTS ix_invoices_status
    ON invoices(status) WHERE status NOT IN ('Paid','Cancelled') AND deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_invoices_doctor
    ON invoices(doctor_id);

CREATE TABLE IF NOT EXISTS invoice_items (
    id           UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id   UUID          NOT NULL REFERENCES invoices(id) ON DELETE CASCADE,
    procedure_id UUID,                     -- cross-module: no FK constraint
    service_name VARCHAR(200)  NOT NULL,
    service_code VARCHAR(30),
    quantity     SMALLINT      NOT NULL DEFAULT 1 CHECK (quantity > 0),
    unit_price   NUMERIC(12,2) NOT NULL CHECK (unit_price >= 0),
    discount     NUMERIC(12,2) NOT NULL DEFAULT 0,
    total        NUMERIC(12,2) NOT NULL,
    created_at   TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_invoice_items_invoice
    ON invoice_items(invoice_id);

COMMIT;
