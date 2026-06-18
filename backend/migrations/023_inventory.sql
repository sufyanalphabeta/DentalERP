-- Migration 023: Inventory Module
-- item_categories, units_of_measure, items (barcode, allow_negative_stock, soft-delete)
-- item_barcodes, warehouses, stock_batches, stock_movements (destination_type, destination_id)
-- Inventory snapshot support via stock_batches ledger

BEGIN;

-- -------------------------------------------------------
-- Item Categories (hierarchical, soft delete)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS item_categories (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(150) NOT NULL,
    name_ar     VARCHAR(150),
    parent_id   UUID         REFERENCES item_categories(id),
    is_active   BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    deleted_at  TIMESTAMPTZ
);

-- -------------------------------------------------------
-- Units of Measure
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS units_of_measure (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(50) NOT NULL UNIQUE,
    name_ar         VARCHAR(50),
    abbreviation    VARCHAR(10)
);

-- Seed: common units
INSERT INTO units_of_measure (id, name, name_ar, abbreviation) VALUES
  (gen_random_uuid(), 'Piece',      'قطعة',  'pcs'),
  (gen_random_uuid(), 'Box',        'علبة',  'box'),
  (gen_random_uuid(), 'Bottle',     'زجاجة', 'btl'),
  (gen_random_uuid(), 'Milliliter', 'مل',    'mL'),
  (gen_random_uuid(), 'Gram',       'غرام',  'g'),
  (gen_random_uuid(), 'Liter',      'لتر',   'L'),
  (gen_random_uuid(), 'Meter',      'متر',   'm'),
  (gen_random_uuid(), 'Strip',      'شريط',  'strip')
ON CONFLICT (name) DO NOTHING;

-- Seed: item categories
INSERT INTO item_categories (id, name, name_ar) VALUES
  (gen_random_uuid(), 'Dental Materials',    'مواد طب الأسنان'),
  (gen_random_uuid(), 'Anesthetics',         'مخدرات'),
  (gen_random_uuid(), 'Radiology Supplies',  'مستلزمات الأشعة'),
  (gen_random_uuid(), 'Lab Supplies',        'مستلزمات المختبر'),
  (gen_random_uuid(), 'Sterilization',       'مستلزمات التعقيم'),
  (gen_random_uuid(), 'PPE',                 'معدات الوقاية الشخصية'),
  (gen_random_uuid(), 'Office Supplies',     'مستلزمات مكتبية'),
  (gen_random_uuid(), 'Medications',         'أدوية');

-- -------------------------------------------------------
-- Items (soft delete + barcode + allow_negative_stock)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS items (
    id                  UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    item_code           VARCHAR(30)   NOT NULL,
    barcode             VARCHAR(100)  UNIQUE,
    name                VARCHAR(200)  NOT NULL,
    name_ar             VARCHAR(200),
    category_id         UUID          REFERENCES item_categories(id),
    unit_of_measure_id  UUID          REFERENCES units_of_measure(id),
    unit_cost           DECIMAL(10,2) NOT NULL DEFAULT 0,
    reorder_level       DECIMAL(10,3) NOT NULL DEFAULT 0,
    reorder_quantity    DECIMAL(10,3) NOT NULL DEFAULT 0,
    is_expiry_tracked   BOOLEAN       NOT NULL DEFAULT FALSE,
    allow_negative_stock BOOLEAN      NOT NULL DEFAULT FALSE,
    storage_conditions  VARCHAR(200),
    is_active           BOOLEAN       NOT NULL DEFAULT TRUE,
    notes               TEXT,
    created_at          TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    deleted_at          TIMESTAMPTZ
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_items_item_code ON items(item_code) WHERE deleted_at IS NULL;

-- -------------------------------------------------------
-- Item Barcodes (multiple barcodes per item)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS item_barcodes (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    item_id     UUID         NOT NULL REFERENCES items(id) ON DELETE CASCADE,
    barcode     VARCHAR(100) NOT NULL,
    label       VARCHAR(100),
    is_primary  BOOLEAN      NOT NULL DEFAULT FALSE,
    UNIQUE(barcode)
);

-- -------------------------------------------------------
-- Warehouses (soft delete)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS warehouses (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(150) NOT NULL,
    name_ar     VARCHAR(150),
    location    VARCHAR(300),
    is_default  BOOLEAN      NOT NULL DEFAULT FALSE,
    is_active   BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    deleted_at  TIMESTAMPTZ
);

-- Seed: default warehouse
INSERT INTO warehouses (id, name, name_ar, is_default)
VALUES (gen_random_uuid(), 'Main Warehouse', 'المخزن الرئيسي', TRUE)
ON CONFLICT DO NOTHING;

-- -------------------------------------------------------
-- Stock Batches (FIFO ledger)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS stock_batches (
    id              UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    item_id         UUID          NOT NULL REFERENCES items(id),
    warehouse_id    UUID          NOT NULL REFERENCES warehouses(id),
    batch_number    VARCHAR(100),
    quantity        DECIMAL(10,3) NOT NULL DEFAULT 0,
    unit_cost       DECIMAL(10,2) NOT NULL,
    expiry_date     DATE,
    received_date   DATE          NOT NULL,
    is_depleted     BOOLEAN       NOT NULL DEFAULT FALSE,
    created_at      TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_stock_batches_fifo
    ON stock_batches(item_id, warehouse_id, expiry_date ASC NULLS LAST, received_date ASC)
    WHERE is_depleted = FALSE;

-- -------------------------------------------------------
-- Stock Movements (with destination tracking)
-- -------------------------------------------------------
CREATE TABLE IF NOT EXISTS stock_movements (
    id                  UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    movement_number     VARCHAR(30)   NOT NULL UNIQUE,
    item_id             UUID          NOT NULL REFERENCES items(id),
    warehouse_id        UUID          NOT NULL REFERENCES warehouses(id),
    batch_id            UUID          REFERENCES stock_batches(id),
    movement_type       VARCHAR(40)   NOT NULL
        CHECK (movement_type IN ('PurchaseReceipt','ManualIssue','LabConsumption',
                                  'RadiologyConsumption','Adjustment','WriteOff',
                                  'SupplierReturn','Transfer')),
    direction           VARCHAR(3)    NOT NULL CHECK (direction IN ('in','out')),
    quantity            DECIMAL(10,3) NOT NULL,
    unit_cost           DECIMAL(10,2),
    total_cost          DECIMAL(12,2),
    destination_type    VARCHAR(30)
        CHECK (destination_type IN ('Clinic','Lab','Radiology','Doctor','Other')),
    destination_id      UUID,
    reference_id        UUID,
    reference_type      VARCHAR(50),
    is_negative_stock   BOOLEAN       NOT NULL DEFAULT FALSE,
    notes               TEXT,
    created_by_id       UUID,
    created_at          TIMESTAMPTZ   NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_stock_movements_item_date    ON stock_movements(item_id, created_at DESC);
CREATE INDEX IF NOT EXISTS ix_stock_movements_type_date    ON stock_movements(movement_type, created_at DESC);
CREATE INDEX IF NOT EXISTS ix_stock_movements_destination  ON stock_movements(destination_type, destination_id)
    WHERE destination_type IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_stock_movements_reference    ON stock_movements(reference_type, reference_id)
    WHERE reference_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS ix_stock_movements_negative     ON stock_movements(item_id)
    WHERE is_negative_stock = TRUE;

COMMIT;
