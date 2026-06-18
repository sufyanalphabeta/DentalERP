-- Migration 018: Installment Plans + Advance Payments + Commission Records

BEGIN;

-- ── Installment Plans ────────────────────────────────────────
CREATE TABLE IF NOT EXISTS installment_plans (
    id                 UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id         UUID          NOT NULL REFERENCES invoices(id),
    patient_id         UUID          NOT NULL REFERENCES patients(id),
    total_amount       NUMERIC(12,2) NOT NULL CHECK (total_amount > 0),
    installments_count SMALLINT      NOT NULL CHECK (installments_count > 0),
    notes              TEXT,
    created_by_id      UUID          REFERENCES users(id),
    created_at         TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS installment_payments (
    id              UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    plan_id         UUID          NOT NULL REFERENCES installment_plans(id),
    installment_num SMALLINT      NOT NULL CHECK (installment_num > 0),
    due_date        DATE          NOT NULL,
    amount          NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    status          VARCHAR(20)   NOT NULL DEFAULT 'Pending'
        CHECK (status IN ('Pending','Paid','Overdue')),
    paid_at         TIMESTAMPTZ,
    vault_id        UUID          REFERENCES vaults(id),
    payment_method  VARCHAR(20)   CHECK (payment_method IN ('cash','bank_transfer','card','pos','cheque'))
);

CREATE INDEX IF NOT EXISTS ix_installments_plan
    ON installment_payments(plan_id);

CREATE INDEX IF NOT EXISTS ix_installments_due
    ON installment_payments(due_date) WHERE status = 'Pending';

-- ── Advance Payments ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS advance_payments (
    id            UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id    UUID          NOT NULL REFERENCES patients(id),
    vault_id      UUID          NOT NULL REFERENCES vaults(id),
    amount        NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    remaining     NUMERIC(12,2) NOT NULL,
    notes         TEXT,
    created_by_id UUID          REFERENCES users(id),
    created_at    TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_advance_patient
    ON advance_payments(patient_id);

-- ── Commission Records ───────────────────────────────────────
CREATE TABLE IF NOT EXISTS commission_records (
    id                   UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    doctor_id            UUID          NOT NULL REFERENCES users(id),
    invoice_id           UUID          NOT NULL REFERENCES invoices(id),
    payment_id           UUID          NOT NULL REFERENCES payments(id),
    procedure_id         UUID,                  -- nullable: Phase 4 = invoice-level
    commission_method    VARCHAR(30)   NOT NULL,
    base_amount          NUMERIC(12,2) NOT NULL,
    commission_rate      NUMERIC(10,4) NOT NULL DEFAULT 0,
    commission_amount    NUMERIC(12,2) NOT NULL DEFAULT 0,
    is_paid              BOOLEAN       NOT NULL DEFAULT FALSE,
    paid_at              TIMESTAMPTZ,
    vault_transaction_id UUID          REFERENCES vault_transactions(id),
    created_at           TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_commission_doctor
    ON commission_records(doctor_id, is_paid);

COMMIT;
