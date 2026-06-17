-- ============================================================
-- Migration 001: Phase 1 — IAM Schema
-- Tables: permissions, roles, role_permissions, users, user_roles, refresh_tokens, system_settings
-- ============================================================

BEGIN;

-- ── permissions ──────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS permissions (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL,
    display_name VARCHAR(200) NOT NULL,
    module      VARCHAR(50)  NOT NULL,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ,
    deleted_at  TIMESTAMPTZ,
    CONSTRAINT ux_permissions_name UNIQUE (name)
);

-- ── roles ────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS roles (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL,
    description VARCHAR(500),
    is_system   BOOLEAN     NOT NULL DEFAULT FALSE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ,
    deleted_at  TIMESTAMPTZ,
    CONSTRAINT ux_roles_name UNIQUE (name)
);

-- ── role_permissions ─────────────────────────────────────────
CREATE TABLE IF NOT EXISTS role_permissions (
    role_id       UUID NOT NULL REFERENCES roles(id)       ON DELETE CASCADE,
    permission_id UUID NOT NULL REFERENCES permissions(id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
);

-- ── users ────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS users (
    id            UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    username      VARCHAR(100) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    full_name     VARCHAR(200) NOT NULL,
    email         VARCHAR(200),
    phone         VARCHAR(30),
    is_active     BOOLEAN     NOT NULL DEFAULT TRUE,
    last_login_at TIMESTAMPTZ,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at    TIMESTAMPTZ,
    deleted_at    TIMESTAMPTZ,
    CONSTRAINT ux_users_username UNIQUE (username)
);

-- ── user_roles ───────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS user_roles (
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, role_id)
);

-- ── refresh_tokens ───────────────────────────────────────────
CREATE TABLE IF NOT EXISTS refresh_tokens (
    id         UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id    UUID        NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token      VARCHAR(512) NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    revoked_at TIMESTAMPTZ,
    CONSTRAINT ux_refresh_tokens_token UNIQUE (token)
);

-- ── system_settings ──────────────────────────────────────────
CREATE TABLE IF NOT EXISTS system_settings (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    key         VARCHAR(100) NOT NULL,
    value       VARCHAR(2000) NOT NULL,
    description VARCHAR(500),
    "group"     VARCHAR(50)  NOT NULL DEFAULT 'General',
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at  TIMESTAMPTZ,
    deleted_at  TIMESTAMPTZ,
    CONSTRAINT ux_system_settings_key UNIQUE (key)
);

-- ── Performance Indexes ──────────────────────────────────────
CREATE INDEX IF NOT EXISTS ix_users_is_active     ON users(is_active)     WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_refresh_tokens_user ON refresh_tokens(user_id);
CREATE INDEX IF NOT EXISTS ix_refresh_tokens_exp  ON refresh_tokens(expires_at) WHERE revoked_at IS NULL;

COMMIT;
