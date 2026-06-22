-- Migration 028: Fix Missing Columns
-- Adds columns that EF Core entity configurations reference but were missing from initial migrations

BEGIN;

-- purchase_orders: BaseEntity has DeletedAt mapped to deleted_at
ALTER TABLE purchase_orders ADD COLUMN IF NOT EXISTS deleted_at TIMESTAMPTZ;

-- radiology_orders: Entity has OrderDate, Notes, CancellationReason not in original migration
ALTER TABLE radiology_orders ADD COLUMN IF NOT EXISTS order_date TIMESTAMPTZ NOT NULL DEFAULT NOW();
ALTER TABLE radiology_orders ADD COLUMN IF NOT EXISTS notes TEXT;
ALTER TABLE radiology_orders ADD COLUMN IF NOT EXISTS cancellation_reason TEXT;

-- Backfill order_date from created_at for existing rows
UPDATE radiology_orders SET order_date = created_at WHERE order_date = NOW();

COMMIT;
