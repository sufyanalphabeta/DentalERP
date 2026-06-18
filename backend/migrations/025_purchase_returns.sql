-- Migration 025: Purchase Returns

BEGIN;

CREATE TABLE IF NOT EXISTS purchase_returns (
    id              UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    return_number   VARCHAR(30)   NOT NULL UNIQUE,
    supplier_id     UUID          NOT NULL REFERENCES suppliers(id),
    po_id           UUID          REFERENCES purchase_orders(id),
    return_date     DATE          NOT NULL,
    reason          TEXT          NOT NULL,
    status          VARCHAR(20)   NOT NULL DEFAULT 'Draft'
        CHECK (status IN ('Draft','Confirmed','Completed')),
    total_amount    DECIMAL(12,2) NOT NULL DEFAULT 0,
    notes           TEXT,
    created_by_id   UUID,
    created_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    deleted_at      TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_purchase_returns_supplier ON purchase_returns(supplier_id, return_date DESC);
CREATE INDEX IF NOT EXISTS ix_purchase_returns_status   ON purchase_returns(status);

CREATE TABLE IF NOT EXISTS purchase_return_items (
    id          UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    return_id   UUID          NOT NULL REFERENCES purchase_returns(id) ON DELETE CASCADE,
    item_id     UUID          NOT NULL REFERENCES items(id),
    batch_id    UUID          REFERENCES stock_batches(id),
    quantity    DECIMAL(10,3) NOT NULL,
    unit_cost   DECIMAL(10,2) NOT NULL,
    total_cost  DECIMAL(12,2) NOT NULL
);

COMMIT;
