-- Migration 042: Fix missing columns in units_of_measure, expand vault_transactions constraint, add missing permission
BEGIN;

-- 1. Add missing timestamp columns to units_of_measure (same pattern as item_categories fix in 040)
ALTER TABLE units_of_measure
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ,
    ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMPTZ;

-- 2. Expand vault_transactions transaction_type check constraint to include purchasing types
ALTER TABLE vault_transactions
    DROP CONSTRAINT IF EXISTS vault_transactions_transaction_type_check;

ALTER TABLE vault_transactions
    ADD CONSTRAINT vault_transactions_transaction_type_check
    CHECK (transaction_type = ANY (ARRAY[
        'receipt_from_patient',
        'payment_to_doctor',
        'general_receipt',
        'general_payment',
        'inter_vault_transfer',
        'supplier_payment',
        'supplier_refund'
    ]::text[]));

-- 3. Add missing Assets.Categories.Delete permission
INSERT INTO permissions (id, name, display_name, module, screen, sort_order, created_at)
SELECT
    gen_random_uuid(),
    'Assets.Categories.Delete',
    'حذف فئات الأصول',
    'Assets',
    'Categories',
    350,
    NOW()
WHERE NOT EXISTS (
    SELECT 1 FROM permissions WHERE name = 'Assets.Categories.Delete'
);

COMMIT;
