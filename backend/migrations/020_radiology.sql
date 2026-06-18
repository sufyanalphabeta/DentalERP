-- Migration 020: Radiology Module
-- Revenue center: supports technician, invoice link, external patients

BEGIN;

-- Radiology Types (lookup)
CREATE TABLE IF NOT EXISTS radiology_types (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(100) NOT NULL UNIQUE,
    description TEXT,
    base_price  NUMERIC(12,2) NOT NULL DEFAULT 0,
    is_active   BOOLEAN      NOT NULL DEFAULT TRUE
);

INSERT INTO radiology_types (name, description, base_price) VALUES
    ('بانوراما (OPG)',   'أشعة بانورامية شاملة',              80.00),
    ('CBCT',            'أشعة مقطعية ثلاثية الأبعاد',        250.00),
    ('أشعة عضية',       'Periapical X-Ray',                   30.00),
    ('أشعة جناحية',     'Bitewing X-Ray',                     30.00),
    ('أشعة إطباقية',    'Occlusal X-Ray',                     40.00),
    ('أشعة جانبية',     'Lateral Cephalometric',              60.00)
ON CONFLICT DO NOTHING;

-- Radiology Orders
CREATE TABLE IF NOT EXISTS radiology_orders (
    id                   UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    order_number         VARCHAR(30)   NOT NULL UNIQUE,
    patient_id           UUID          NOT NULL REFERENCES patients(id),
    doctor_id            UUID          NOT NULL REFERENCES users(id),
    technician_id        UUID          REFERENCES users(id),  -- radiology technician
    radiology_type_id    UUID          REFERENCES radiology_types(id),
    procedure_id         UUID,           -- cross-module: no FK
    invoice_id           UUID,           -- cross-module: no FK (Financial module)
    is_external_patient  BOOLEAN       NOT NULL DEFAULT FALSE,
    external_patient_name VARCHAR(200),  -- name when is_external_patient = true
    external_patient_phone VARCHAR(30),
    status               VARCHAR(20)   NOT NULL DEFAULT 'Ordered'
        CHECK (status IN ('Ordered','InProgress','Imaged','Reported','Completed','Cancelled')),
    clinical_notes       TEXT,
    cost                 NUMERIC(12,2) NOT NULL DEFAULT 0,   -- internal cost
    price                NUMERIC(12,2) NOT NULL DEFAULT 0,   -- billed to patient
    currency             CHAR(3)       NOT NULL DEFAULT 'LYD',
    doctor_commission_amount  NUMERIC(12,2) NOT NULL DEFAULT 0,
    tech_commission_amount    NUMERIC(12,2) NOT NULL DEFAULT 0,
    cancelled_reason     TEXT,
    created_by_id        UUID          REFERENCES users(id),
    created_at           TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at           TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_rad_orders_patient    ON radiology_orders(patient_id);
CREATE INDEX IF NOT EXISTS ix_rad_orders_status     ON radiology_orders(status) WHERE status NOT IN ('Completed','Cancelled');
CREATE INDEX IF NOT EXISTS ix_rad_orders_doctor     ON radiology_orders(doctor_id);
CREATE INDEX IF NOT EXISTS ix_rad_orders_technician ON radiology_orders(technician_id) WHERE technician_id IS NOT NULL;

-- Radiology Images
CREATE TABLE IF NOT EXISTS radiology_images (
    id             UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id       UUID         NOT NULL REFERENCES radiology_orders(id) ON DELETE CASCADE,
    storage_bucket VARCHAR(100) NOT NULL,  -- IFileStorageService bucket
    storage_key    VARCHAR(500) NOT NULL,  -- IFileStorageService key
    file_name      VARCHAR(200) NOT NULL,
    file_size      BIGINT,
    mime_type      VARCHAR(50),
    view_label     VARCHAR(100),
    taken_at       TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    uploaded_by_id UUID         REFERENCES users(id)
);

CREATE INDEX IF NOT EXISTS ix_rad_images_order ON radiology_images(order_id);

-- Radiology Reports
CREATE TABLE IF NOT EXISTS radiology_reports (
    id             UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id       UUID        NOT NULL REFERENCES radiology_orders(id) UNIQUE,
    report_text    TEXT        NOT NULL,
    findings       TEXT,
    impression     TEXT,
    reported_by_id UUID        REFERENCES users(id),
    reported_at    TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at     TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_rad_reports_order ON radiology_reports(order_id);

COMMIT;
