-- Migration 029: Add missing timestamp columns to queue_entries
-- The QueueEntry EF Core configuration maps CreatedAt/UpdatedAt but they were missing from migration 004

BEGIN;

ALTER TABLE queue_entries ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT NOW();
ALTER TABLE queue_entries ADD COLUMN IF NOT EXISTS updated_at TIMESTAMPTZ;

COMMIT;
