-- Migration 027: Asset Categories, Assets, Asset Documents, Asset Maintenance

BEGIN;

-- -------------------------------------------------------
-- Asset Categories (soft-delete)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS asset_categories (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL,
    name_ar     VARCHAR(100),
    description TEXT,
    is_active   BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ,
    deleted_at  TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_asset_categories_name ON asset_categories(name) WHERE deleted_at IS NULL;

-- -------------------------------------------------------
-- Assets (soft-delete)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS assets (
    id              UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    asset_tag       VARCHAR(50)   NOT NULL UNIQUE,
    name            VARCHAR(200)  NOT NULL,
    name_ar         VARCHAR(200),
    category_id     UUID          REFERENCES asset_categories(id),
    cost_center     VARCHAR(50)   NOT NULL DEFAULT 'GENERAL',
    purchase_date   DATE,
    purchase_cost   DECIMAL(12,2) NOT NULL DEFAULT 0,
    supplier_id     UUID          REFERENCES suppliers(id),
    location        VARCHAR(200),
    serial_number   VARCHAR(100),
    model           VARCHAR(100),
    status          VARCHAR(30)   NOT NULL DEFAULT 'Active'
        CHECK (status IN ('Active','UnderMaintenance','Disposed')),
    disposed_at     DATE,
    disposal_notes  TEXT,
    notes           TEXT,
    created_by_id   UUID,
    created_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    deleted_at      TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_assets_tag      ON assets(asset_tag)   WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_assets_category ON assets(category_id) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_assets_status   ON assets(status)      WHERE deleted_at IS NULL;

-- -------------------------------------------------------
-- Asset Documents
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS asset_documents (
    id              UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    asset_id        UUID         NOT NULL REFERENCES assets(id) ON DELETE CASCADE,
    document_type   VARCHAR(50)  NOT NULL DEFAULT 'Other',
    file_name       VARCHAR(200) NOT NULL,
    file_key        VARCHAR(500) NOT NULL,
    file_size       BIGINT,
    content_type    VARCHAR(100),
    notes           TEXT,
    uploaded_by_id  UUID,
    uploaded_at     TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_asset_documents_asset ON asset_documents(asset_id);

-- -------------------------------------------------------
-- Asset Maintenance
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS asset_maintenance (
    id                  UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    asset_id            UUID          NOT NULL REFERENCES assets(id),
    maintenance_date    DATE          NOT NULL,
    description         TEXT          NOT NULL,
    cost                DECIMAL(12,2) NOT NULL DEFAULT 0,
    vendor              VARCHAR(200),
    expense_id          UUID          REFERENCES expenses(id),
    performed_by_id     UUID,
    created_at          TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_asset_maintenance_asset ON asset_maintenance(asset_id, maintenance_date DESC);

COMMIT;
