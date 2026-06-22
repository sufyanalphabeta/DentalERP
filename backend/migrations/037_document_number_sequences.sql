-- Migration 037: Atomic document number sequences — all document types
-- Replaces every COUNT+1 / MAX+1 pattern system-wide with PostgreSQL sequences.
-- nextval() is atomic and unique under concurrent load. Gaps are acceptable;
-- duplicates are impossible. Sequences are NOT transactional — a rolled-back
-- transaction does not reclaim the consumed value.
--
-- Seeding strategy: extract trailing digits from existing documents via regexp,
-- then call setval() to advance past the highest existing number.
-- For tables with no existing rows the DO blocks are no-ops.
-- Development database only — safe to re-run (CREATE SEQUENCE IF NOT EXISTS).

BEGIN;

-- ─── Expenses ────────────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS expense_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(expense_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM expenses;
    IF max_num > 0 THEN PERFORM setval('expense_number_seq', max_num); END IF;
END $$;

-- ─── Patient Invoice ─────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS invoice_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(invoice_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM invoices;
    IF max_num > 0 THEN PERFORM setval('invoice_number_seq', max_num); END IF;
END $$;

-- ─── Purchase Invoice ────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS purchase_invoice_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(invoice_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM purchase_invoices;
    IF max_num > 0 THEN PERFORM setval('purchase_invoice_number_seq', max_num); END IF;
END $$;

-- ─── Purchase Order ──────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS po_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(po_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM purchase_orders;
    IF max_num > 0 THEN PERFORM setval('po_number_seq', max_num); END IF;
END $$;

-- ─── Goods Receipt ───────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS gr_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(gr_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM goods_receipts;
    IF max_num > 0 THEN PERFORM setval('gr_number_seq', max_num); END IF;
END $$;

-- ─── Purchase Return ─────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS purchase_return_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(return_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM purchase_returns;
    IF max_num > 0 THEN PERFORM setval('purchase_return_number_seq', max_num); END IF;
END $$;

-- ─── Supplier Payment ────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS supplier_payment_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(payment_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM supplier_payments;
    IF max_num > 0 THEN PERFORM setval('supplier_payment_number_seq', max_num); END IF;
END $$;

-- ─── Supplier Code ───────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS supplier_code_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(supplier_code, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM suppliers;
    IF max_num > 0 THEN PERFORM setval('supplier_code_seq', max_num); END IF;
END $$;

-- ─── Stock Movement ──────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS stock_movement_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(movement_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM stock_movements;
    IF max_num > 0 THEN PERFORM setval('stock_movement_number_seq', max_num); END IF;
END $$;

-- ─── Inventory Item Code ─────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS item_code_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(item_code, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM items;
    IF max_num > 0 THEN PERFORM setval('item_code_seq', max_num); END IF;
END $$;

-- ─── Insurance Claim ─────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS insurance_claim_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(claim_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM insurance_claims;
    IF max_num > 0 THEN PERFORM setval('insurance_claim_number_seq', max_num); END IF;
END $$;

-- ─── Vault Transfer ──────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS vault_transfer_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(transfer_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM vault_transfers;
    IF max_num > 0 THEN PERFORM setval('vault_transfer_number_seq', max_num); END IF;
END $$;

-- ─── Lab Order ───────────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS lab_order_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(order_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM lab_orders;
    IF max_num > 0 THEN PERFORM setval('lab_order_number_seq', max_num); END IF;
END $$;

-- ─── Radiology Order ─────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS radiology_order_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(order_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM radiology_orders;
    IF max_num > 0 THEN PERFORM setval('radiology_order_number_seq', max_num); END IF;
END $$;

-- ─── Asset Tag ───────────────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS asset_tag_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(asset_tag, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM assets;
    IF max_num > 0 THEN PERFORM setval('asset_tag_seq', max_num); END IF;
END $$;

-- ─── Patient File Number ─────────────────────────────────────────────────────
CREATE SEQUENCE IF NOT EXISTS patient_file_number_seq START 1;
DO $$
DECLARE max_num BIGINT;
BEGIN
    SELECT COALESCE(MAX((regexp_match(file_number, '\d+$'))[1]::bigint), 0)
    INTO max_num FROM patients;
    IF max_num > 0 THEN PERFORM setval('patient_file_number_seq', max_num); END IF;
END $$;

COMMIT;
