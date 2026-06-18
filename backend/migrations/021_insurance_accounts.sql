-- Migration 021: Insurance Accounts
-- Scope: Companies, Claims, Payments — NO pre-auth, coverage, or TPA workflow

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

-- Add FK from patients to insurance_companies
-- (column insurance_company_id already exists from migration 005)
ALTER TABLE patients
    ADD CONSTRAINT fk_patients_insurance_company
    FOREIGN KEY (insurance_company_id)
    REFERENCES insurance_companies(id)
    DEFERRABLE INITIALLY DEFERRED;

-- Insurance Claims (1 per invoice)
CREATE TABLE IF NOT EXISTS insurance_claims (
    id               UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    claim_number     VARCHAR(30)   NOT NULL UNIQUE,
    invoice_id       UUID          NOT NULL REFERENCES invoices(id) UNIQUE,
    patient_id       UUID          NOT NULL REFERENCES patients(id),
    company_id       UUID          NOT NULL REFERENCES insurance_companies(id),
    claim_amount     NUMERIC(12,2) NOT NULL CHECK (claim_amount > 0),
    paid_amount      NUMERIC(12,2) NOT NULL DEFAULT 0,
    status           VARCHAR(20)   NOT NULL DEFAULT 'Draft'
        CHECK (status IN ('Draft','Submitted','PartiallyPaid','Paid','Rejected')),
    submission_date  DATE,
    rejection_reason TEXT,
    notes            TEXT,
    created_by_id    UUID          REFERENCES users(id),
    created_at       TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_claims_company  ON insurance_claims(company_id, status);
CREATE INDEX IF NOT EXISTS ix_claims_patient  ON insurance_claims(patient_id);

-- Insurance Payments
CREATE TABLE IF NOT EXISTS insurance_payments (
    id               UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    claim_id         UUID          NOT NULL REFERENCES insurance_claims(id),
    company_id       UUID          NOT NULL REFERENCES insurance_companies(id),
    vault_id         UUID          NOT NULL REFERENCES vaults(id),
    amount           NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    payment_method   VARCHAR(20)   NOT NULL
        CHECK (payment_method IN ('cash','bank_transfer','card','pos','cheque')),
    reference_number VARCHAR(50),
    notes            TEXT,
    received_by_id   UUID          REFERENCES users(id),
    received_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_ins_payments_claim   ON insurance_payments(claim_id);
CREATE INDEX IF NOT EXISTS ix_ins_payments_company ON insurance_payments(company_id);

COMMIT;
