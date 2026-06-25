-- Migration 041: Add missing columns to assets module
BEGIN;

-- depreciation rate on asset categories
ALTER TABLE asset_categories
    ADD COLUMN IF NOT EXISTS depreciation_rate DECIMAL(5,2);

-- next maintenance date on asset maintenance records
ALTER TABLE asset_maintenance
    ADD COLUMN IF NOT EXISTS next_maintenance_date DATE;

-- serial_number already exists in assets table per migration 027; ensure it is there
-- (idempotent — no-op if already present)
ALTER TABLE assets
    ADD COLUMN IF NOT EXISTS serial_number VARCHAR(100);

COMMIT;
