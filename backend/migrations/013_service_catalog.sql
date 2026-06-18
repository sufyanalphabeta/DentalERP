-- Migration 013: Services Catalog (service_categories + medical_services)

BEGIN;

CREATE TABLE IF NOT EXISTS service_categories (
    id         UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    name       VARCHAR(100) NOT NULL UNIQUE,
    sort_order SMALLINT     NOT NULL DEFAULT 0,
    is_active  BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS medical_services (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    category_id UUID         REFERENCES service_categories(id),
    name        VARCHAR(200) NOT NULL,
    code        VARCHAR(30)  UNIQUE,
    price       NUMERIC(12,2) NOT NULL DEFAULT 0 CHECK (price >= 0),
    is_active   BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ,
    deleted_at  TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_medical_services_category
    ON medical_services(category_id) WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_medical_services_active
    ON medical_services(is_active) WHERE deleted_at IS NULL;

-- Seed: تصنيفات افتراضية
INSERT INTO service_categories (name, sort_order) VALUES
    ('تنظيف وتبييض', 1),
    ('حشوات', 2),
    ('علاج عصب', 3),
    ('تركيبات', 4),
    ('تقويم', 5),
    ('خلع', 6),
    ('جراحة الفم', 7),
    ('أشعة', 8),
    ('أخرى', 99)
ON CONFLICT (name) DO NOTHING;

COMMIT;
