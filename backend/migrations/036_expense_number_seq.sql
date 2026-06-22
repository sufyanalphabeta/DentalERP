-- Migration 036: Expense number sequence
-- Replaces the unsafe COUNT+1 pattern in CreateExpenseCommandHandler
-- with an atomic PostgreSQL sequence. nextval() is guaranteed unique
-- even under concurrent transactions.

BEGIN;

CREATE SEQUENCE IF NOT EXISTS expense_number_seq START 1;

-- Seed the sequence past any existing expense numbers so no collision
-- can occur on first use after deployment.
-- regexp_match extracts the trailing digits from 'EXP-YYYY-NNNNNN'.
DO $$
DECLARE
    max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(expense_number, '\d+$'))[1]::bigint), 0)
    INTO max_num
    FROM expenses;

    IF max_num > 0 THEN
        PERFORM setval('expense_number_seq', max_num);
    END IF;
END $$;

COMMIT;
