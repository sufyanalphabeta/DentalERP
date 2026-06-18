-- Migration 026: Audit Logs, Cost Centers, Expense Categories, Expenses

BEGIN;

-- -------------------------------------------------------
-- Audit Logs (global — written by all modules)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS audit_logs (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    entity_type     VARCHAR(100) NOT NULL,
    entity_id       UUID         NOT NULL,
    action          VARCHAR(100) NOT NULL,
    performed_by_id UUID,
    details         TEXT,
    created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_audit_logs_entity   ON audit_logs(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS ix_audit_logs_created  ON audit_logs(created_at DESC);
CREATE INDEX IF NOT EXISTS ix_audit_logs_actor    ON audit_logs(performed_by_id);

-- -------------------------------------------------------
-- Cost Centers (lightweight — used by Expenses + Assets)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS cost_centers (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    code        VARCHAR(50)  NOT NULL UNIQUE,
    name        VARCHAR(100) NOT NULL,
    is_active   BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

INSERT INTO cost_centers (code, name) VALUES
    ('GENERAL',        'General'),
    ('CLINIC',         'Clinic'),
    ('LABORATORY',     'Laboratory'),
    ('RADIOLOGY',      'Radiology'),
    ('TRAINING',       'Training'),
    ('ADMINISTRATION', 'Administration')
ON CONFLICT (code) DO NOTHING;

-- -------------------------------------------------------
-- Expense Categories (soft-delete)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS expense_categories (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL,
    name_ar     VARCHAR(100),
    description TEXT,
    is_active   BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ,
    deleted_at  TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_expense_categories_name ON expense_categories(name) WHERE deleted_at IS NULL;

-- -------------------------------------------------------
-- Expense Templates
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS expense_templates (
    id                  UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    name                VARCHAR(200) NOT NULL,
    category_id         UUID         REFERENCES expense_categories(id),
    cost_center         VARCHAR(50)  NOT NULL DEFAULT 'GENERAL',
    default_amount      DECIMAL(12,2),
    notes               TEXT,
    created_at          TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- -------------------------------------------------------
-- Expenses
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS expenses (
    id                  UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    expense_number      VARCHAR(30)  NOT NULL UNIQUE,
    category_id         UUID         REFERENCES expense_categories(id),
    cost_center         VARCHAR(50)  NOT NULL DEFAULT 'GENERAL',
    expense_date        DATE         NOT NULL,
    amount              DECIMAL(12,2) NOT NULL,
    description         TEXT          NOT NULL,
    related_module      VARCHAR(50),
    related_entity_id   UUID,
    vault_id            UUID,
    notes               TEXT,
    attachment_key      VARCHAR(500),
    attachment_name     VARCHAR(200),
    created_by_id       UUID,
    created_at          TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    deleted_at          TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_expenses_date         ON expenses(expense_date DESC) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_expenses_category     ON expenses(category_id)      WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_expenses_cost_center  ON expenses(cost_center)      WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_expenses_module       ON expenses(related_module, related_entity_id) WHERE related_module IS NOT NULL;

COMMIT;
