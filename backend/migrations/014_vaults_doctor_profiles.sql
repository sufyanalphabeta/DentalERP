-- Migration 014: Vaults + Doctor Profiles (commission settings)

BEGIN;

CREATE TABLE IF NOT EXISTS vaults (
    id              UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100)  NOT NULL UNIQUE,
    type            VARCHAR(20)   NOT NULL CHECK (type IN ('cash','bank','card','pos')),
    opening_balance NUMERIC(12,2) NOT NULL DEFAULT 0,
    is_active       BOOLEAN       NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

-- Seed: خزينة رئيسية نقدية
INSERT INTO vaults (name, type, opening_balance) VALUES
    ('الخزينة الرئيسية', 'cash', 0)
ON CONFLICT (name) DO NOTHING;

-- Doctor profiles — commission settings (1:1 with users who are doctors)
CREATE TABLE IF NOT EXISTS doctor_profiles (
    user_id                  UUID          PRIMARY KEY REFERENCES users(id),
    commission_method        VARCHAR(30)   NOT NULL DEFAULT 'percentage_of_service'
        CHECK (commission_method IN ('percentage_of_service','fixed_amount','percentage_of_net_service')),
    default_commission_value NUMERIC(10,2) NOT NULL DEFAULT 0 CHECK (default_commission_value >= 0),
    updated_at               TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

COMMIT;
