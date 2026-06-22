CREATE TABLE purchase_invoices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_number VARCHAR(30) NOT NULL,
    invoice_date DATE NOT NULL DEFAULT CURRENT_DATE,
    supplier_id UUID NOT NULL REFERENCES suppliers(id),
    warehouse_id UUID REFERENCES warehouses(id),
    status VARCHAR(20) NOT NULL DEFAULT 'Draft',
    subtotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    discount DECIMAL(12,2) NOT NULL DEFAULT 0,
    net_total DECIMAL(12,2) NOT NULL DEFAULT 0,
    notes TEXT,
    created_by_id UUID,
    posted_at TIMESTAMPTZ,
    cancelled_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ,
    CONSTRAINT uq_pi_number UNIQUE (invoice_number) WHERE deleted_at IS NULL
);

CREATE TABLE purchase_invoice_items (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    invoice_id UUID NOT NULL REFERENCES purchase_invoices(id) ON DELETE CASCADE,
    item_id UUID NOT NULL REFERENCES items(id),
    item_code VARCHAR(50),
    item_name VARCHAR(200) NOT NULL,
    barcode VARCHAR(100),
    unit_name VARCHAR(50),
    quantity DECIMAL(12,3) NOT NULL,
    purchase_price DECIMAL(12,2) NOT NULL DEFAULT 0,
    sale_price DECIMAL(12,2),
    line_total DECIMAL(12,2) NOT NULL DEFAULT 0,
    expiry_date DATE,
    batch_number VARCHAR(50),
    sort_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_pi_supplier ON purchase_invoices(supplier_id);
CREATE INDEX idx_pi_status ON purchase_invoices(status);
CREATE INDEX idx_pi_date ON purchase_invoices(invoice_date);
CREATE INDEX idx_pii_invoice ON purchase_invoice_items(invoice_id);
