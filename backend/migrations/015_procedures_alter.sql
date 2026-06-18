-- Migration 015: Alter procedures — add pricing + lab_cost fields
-- Required for Commission Engine (percentage_of_net_service)

BEGIN;

ALTER TABLE procedures
    ADD COLUMN IF NOT EXISTS base_price     NUMERIC(12,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS discount_type  VARCHAR(10)   CHECK (discount_type IN ('percentage','fixed')),
    ADD COLUMN IF NOT EXISTS discount_value NUMERIC(12,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS final_price    NUMERIC(12,2) NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS lab_cost       NUMERIC(12,2) NOT NULL DEFAULT 0;

-- Add FK for service_id (was NULL in Phase 3 — now medical_services exists)
-- Using DEFERRABLE to allow existing NULL values
ALTER TABLE procedures
    ADD CONSTRAINT fk_procedures_service
    FOREIGN KEY (service_id)
    REFERENCES medical_services(id)
    DEFERRABLE INITIALLY DEFERRED;

COMMIT;
