-- Migration 022: Vault-to-Vault Transfers
-- Each transfer creates 2 VaultTransactions (inter_vault_transfer in/out)

BEGIN;

CREATE TABLE IF NOT EXISTS vault_transfers (
    id              UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    transfer_number VARCHAR(30)   NOT NULL UNIQUE,
    from_vault_id   UUID          NOT NULL REFERENCES vaults(id),
    to_vault_id     UUID          NOT NULL REFERENCES vaults(id),
    amount          NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    notes           TEXT,
    created_by_id   UUID          REFERENCES users(id),
    created_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    CONSTRAINT chk_different_vaults CHECK (from_vault_id <> to_vault_id)
);

CREATE INDEX IF NOT EXISTS ix_vault_transfers_from ON vault_transfers(from_vault_id);
CREATE INDEX IF NOT EXISTS ix_vault_transfers_to   ON vault_transfers(to_vault_id);
CREATE INDEX IF NOT EXISTS ix_vault_transfers_date ON vault_transfers(created_at DESC);

COMMIT;
