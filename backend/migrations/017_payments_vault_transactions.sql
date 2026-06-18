-- Migration 017: Payments + Vault Transactions

BEGIN;

CREATE TABLE IF NOT EXISTS payments (
    id               UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id       UUID          NOT NULL REFERENCES invoices(id),
    vault_id         UUID          NOT NULL REFERENCES vaults(id),
    amount           NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    payment_method   VARCHAR(20)   NOT NULL
        CHECK (payment_method IN ('cash','bank_transfer','card','pos','cheque')),
    reference_number VARCHAR(50),
    notes            TEXT,
    created_by_id    UUID          REFERENCES users(id),
    created_at       TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_payments_invoice
    ON payments(invoice_id);

CREATE TABLE IF NOT EXISTS vault_transactions (
    id                 UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    vault_id           UUID          NOT NULL REFERENCES vaults(id),
    transaction_type   VARCHAR(30)   NOT NULL CHECK (transaction_type IN (
        'receipt_from_patient', 'payment_to_doctor',
        'general_receipt', 'general_payment', 'inter_vault_transfer'
    )),
    amount             NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    direction          VARCHAR(3)    NOT NULL CHECK (direction IN ('in','out')),
    related_invoice_id UUID          REFERENCES invoices(id),
    related_patient_id UUID          REFERENCES patients(id),
    related_doctor_id  UUID          REFERENCES users(id),
    reference_number   VARCHAR(50),
    notes              TEXT,
    is_reversed        BOOLEAN       NOT NULL DEFAULT FALSE,
    is_reversal        BOOLEAN       NOT NULL DEFAULT FALSE,
    created_by_id      UUID          REFERENCES users(id),
    created_at         TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_vault_tx_vault
    ON vault_transactions(vault_id, created_at DESC);

CREATE INDEX IF NOT EXISTS ix_vault_tx_patient
    ON vault_transactions(related_patient_id) WHERE related_patient_id IS NOT NULL;

COMMIT;
