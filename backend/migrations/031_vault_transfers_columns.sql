-- Migration 031: Fix vault_transfers missing columns
-- EF config references transfer_date and transferred_by_id which were missing from migration 022

BEGIN;

ALTER TABLE vault_transfers
    ADD COLUMN IF NOT EXISTS transfer_date     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ADD COLUMN IF NOT EXISTS transferred_by_id UUID REFERENCES users(id);

-- Backfill transfer_date from created_at for existing rows
UPDATE vault_transfers SET transfer_date = created_at WHERE transfer_date = NOW();

-- Make doctor_id nullable (entities have Guid? DoctorId)
ALTER TABLE radiology_orders ALTER COLUMN doctor_id DROP NOT NULL;
ALTER TABLE lab_orders ALTER COLUMN doctor_id DROP NOT NULL;

COMMIT;
