-- Add opening balance to suppliers for migrating existing supplier debts
ALTER TABLE suppliers
    ADD COLUMN IF NOT EXISTS opening_balance DECIMAL(12,2) NOT NULL DEFAULT 0;
