-- Migration 032: Add must_change_password to users
BEGIN;

ALTER TABLE users
    ADD COLUMN IF NOT EXISTS must_change_password BOOLEAN NOT NULL DEFAULT FALSE;

-- Existing users keep FALSE; new users will have TRUE set by application
COMMIT;
