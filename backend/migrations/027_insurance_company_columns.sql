-- Migration 027: Fix insurance tables to match Financial module entity schema

-- Add columns to insurance_companies
ALTER TABLE insurance_companies
    ADD COLUMN IF NOT EXISTS name_ar VARCHAR(200),
    ADD COLUMN IF NOT EXISTS default_coverage_percent NUMERIC(5,2) NOT NULL DEFAULT 80.00;

-- Add columns to insurance_claims
ALTER TABLE insurance_claims
    ADD COLUMN IF NOT EXISTS coverage_percent NUMERIC(5,2) NOT NULL DEFAULT 80.00,
    ADD COLUMN IF NOT EXISTS submitted_at TIMESTAMPTZ;
