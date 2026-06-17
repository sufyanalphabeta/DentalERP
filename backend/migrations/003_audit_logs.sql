-- ============================================================
-- Migration 003: Audit Logs Table
-- ============================================================

BEGIN;

CREATE TABLE IF NOT EXISTS audit_logs (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID,
    username    VARCHAR(100) NOT NULL DEFAULT 'system',
    entity_name VARCHAR(100) NOT NULL,
    entity_id   VARCHAR(100) NOT NULL,
    action      VARCHAR(50)  NOT NULL
        CHECK (action IN ('Created','Updated','Deleted','Login','Logout','PasswordChanged','Other')),
    old_values  JSONB,
    new_values  JSONB,
    ip_address  VARCHAR(45),
    user_agent  VARCHAR(500),
    timestamp   TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_audit_logs_timestamp  ON audit_logs(timestamp DESC);
CREATE INDEX IF NOT EXISTS ix_audit_logs_entity     ON audit_logs(entity_name, entity_id);
CREATE INDEX IF NOT EXISTS ix_audit_logs_user       ON audit_logs(user_id) WHERE user_id IS NOT NULL;

COMMENT ON TABLE audit_logs IS 'Immutable audit trail — no UPDATE or DELETE allowed on this table';

COMMIT;
