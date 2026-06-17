-- ============================================================
-- Migration 005: Add insurance_company_id to patients
-- ============================================================

BEGIN;

ALTER TABLE patients
    ADD COLUMN IF NOT EXISTS insurance_company_id UUID;

COMMENT ON COLUMN patients.insurance_company_id IS 'FK to insurance_companies table (Phase 5 — Treasury)';

COMMIT;
