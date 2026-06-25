-- Migration 040: Add missing updated_at columns to inventory tables
-- item_categories and warehouses were missing this column,
-- causing EF Core SELECT/UPDATE to fail (column does not exist).

BEGIN;

ALTER TABLE item_categories ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ;
ALTER TABLE warehouses      ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ;

COMMIT;
