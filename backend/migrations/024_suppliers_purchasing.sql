-- Migration 024: Suppliers & Purchasing
-- suppliers (soft-delete, NO balance column), supplier_items, supplier_payments
-- purchase_orders, purchase_order_items, goods_receipts, goods_receipt_items

BEGIN;

-- -------------------------------------------------------
-- Suppliers (soft delete, computed balance)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS suppliers (
    id                  UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    supplier_code       VARCHAR(30)   NOT NULL,
    name                VARCHAR(200)  NOT NULL,
    name_ar             VARCHAR(200),
    category            VARCHAR(50)
        CHECK (category IN ('Medical','Equipment','General','Lab','Radiology','Pharma')),
    contact_person      VARCHAR(200),
    phone               VARCHAR(30),
    email               VARCHAR(200),
    address             TEXT,
    payment_terms_days  INT           NOT NULL DEFAULT 30,
    credit_limit        DECIMAL(12,2) NOT NULL DEFAULT 0,
    is_active           BOOLEAN       NOT NULL DEFAULT TRUE,
    notes               TEXT,
    created_at          TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    deleted_at          TIMESTAMPTZ
    -- NOTE: NO balance column. Balance is always computed:
    -- balance = Σ(goods_receipts.total_amount) - Σ(supplier_payments.amount) - Σ(purchase_returns.total_amount WHERE status='Confirmed')
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_suppliers_code ON suppliers(supplier_code) WHERE deleted_at IS NULL;

-- -------------------------------------------------------
-- Supplier Items (supplier's own SKU/code for items)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS supplier_items (
    id                  UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    supplier_id         UUID          NOT NULL REFERENCES suppliers(id),
    item_id             UUID          NOT NULL REFERENCES items(id),
    supplier_item_code  VARCHAR(100)  NOT NULL,
    supplier_item_name  VARCHAR(200),
    last_unit_cost      DECIMAL(10,2),
    is_preferred        BOOLEAN       NOT NULL DEFAULT FALSE,
    notes               TEXT,
    UNIQUE(supplier_id, item_id),
    UNIQUE(supplier_id, supplier_item_code)
);

CREATE INDEX IF NOT EXISTS ix_supplier_items_lookup ON supplier_items(supplier_id, supplier_item_code);

-- -------------------------------------------------------
-- Supplier Payments (writes vault_transactions via VaultTransactionWriter)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS supplier_payments (
    id               UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_number   VARCHAR(30)   NOT NULL UNIQUE,
    supplier_id      UUID          NOT NULL REFERENCES suppliers(id),
    vault_id         UUID          NOT NULL,    -- cross-module ref to Financial.Vaults
    amount           DECIMAL(12,2) NOT NULL,
    payment_date     DATE          NOT NULL,
    reference_number VARCHAR(100),
    notes            TEXT,
    paid_by_id       UUID,
    created_at       TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_supplier_payments_supplier ON supplier_payments(supplier_id, payment_date DESC);

-- -------------------------------------------------------
-- Purchase Orders
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS purchase_orders (
    id              UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    po_number       VARCHAR(30)   NOT NULL UNIQUE,
    supplier_id     UUID          NOT NULL REFERENCES suppliers(id),
    status          VARCHAR(30)   NOT NULL DEFAULT 'Draft'
        CHECK (status IN ('Draft','Approved','Sent','PartiallyReceived','FullyReceived','Closed','Cancelled')),
    order_date      DATE          NOT NULL,
    expected_date   DATE,
    subtotal        DECIMAL(12,2) NOT NULL DEFAULT 0,
    discount_amount DECIMAL(12,2) NOT NULL DEFAULT 0,
    total_amount    DECIMAL(12,2) NOT NULL DEFAULT 0,
    notes           TEXT,
    approved_by_id  UUID,
    approved_at     TIMESTAMPTZ,
    created_by_id   UUID,
    created_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_po_supplier_status ON purchase_orders(supplier_id, status);
CREATE INDEX IF NOT EXISTS ix_po_created_at      ON purchase_orders(created_at DESC);

-- -------------------------------------------------------
-- Purchase Order Items
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS purchase_order_items (
    id                  UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    po_id               UUID          NOT NULL REFERENCES purchase_orders(id) ON DELETE CASCADE,
    item_id             UUID          NOT NULL REFERENCES items(id),
    supplier_item_id    UUID          REFERENCES supplier_items(id),
    quantity_ordered    DECIMAL(10,3) NOT NULL,
    quantity_received   DECIMAL(10,3) NOT NULL DEFAULT 0,
    unit_cost           DECIMAL(10,2) NOT NULL,
    total_cost          DECIMAL(12,2) NOT NULL,
    notes               TEXT
);

-- -------------------------------------------------------
-- Goods Receipts
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS goods_receipts (
    id                   UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    gr_number            VARCHAR(30)   NOT NULL UNIQUE,
    po_id                UUID          REFERENCES purchase_orders(id),
    supplier_id          UUID          NOT NULL REFERENCES suppliers(id),
    warehouse_id         UUID          NOT NULL REFERENCES warehouses(id),
    receipt_date         DATE          NOT NULL,
    supplier_invoice_ref VARCHAR(100),
    total_amount         DECIMAL(12,2) NOT NULL DEFAULT 0,
    notes                TEXT,
    received_by_id       UUID,
    created_at           TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_gr_supplier    ON goods_receipts(supplier_id, receipt_date DESC);
CREATE INDEX IF NOT EXISTS ix_gr_po          ON goods_receipts(po_id) WHERE po_id IS NOT NULL;

-- -------------------------------------------------------
-- Goods Receipt Items
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS goods_receipt_items (
    id              UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    gr_id           UUID          NOT NULL REFERENCES goods_receipts(id) ON DELETE CASCADE,
    po_item_id      UUID          REFERENCES purchase_order_items(id),
    item_id         UUID          NOT NULL REFERENCES items(id),
    batch_number    VARCHAR(100),
    quantity        DECIMAL(10,3) NOT NULL,
    unit_cost       DECIMAL(10,2) NOT NULL,
    total_cost      DECIMAL(12,2) NOT NULL,
    expiry_date     DATE
);

COMMIT;
