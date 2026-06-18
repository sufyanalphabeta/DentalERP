-- Migration 019: Laboratory Module
-- External labs, lab clients, lab orders, items, results

BEGIN;

-- External Labs (third-party lab companies)
CREATE TABLE IF NOT EXISTS external_labs (
    id           UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    name         VARCHAR(200) NOT NULL,
    contact_name VARCHAR(100),
    phone        VARCHAR(30),
    email        VARCHAR(150),
    address      TEXT,
    notes        TEXT,
    is_active    BOOLEAN      NOT NULL DEFAULT TRUE,
    created_at   TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

-- Lab Clients (external-facing: doctors/clinics who send work to this lab)
CREATE TABLE IF NOT EXISTS lab_clients (
    id          UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    name        VARCHAR(200) NOT NULL,
    client_type VARCHAR(20) NOT NULL DEFAULT 'ExternalClient'
        CHECK (client_type IN ('Doctor','Clinic','ExternalClient')),
    phone       VARCHAR(30),
    email       VARCHAR(150),
    address     TEXT,
    notes       TEXT,
    is_active   BOOLEAN     NOT NULL DEFAULT TRUE,
    created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Lab Orders
CREATE TABLE IF NOT EXISTS lab_orders (
    id               UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    order_number     VARCHAR(30)   NOT NULL UNIQUE,
    patient_id       UUID          NOT NULL REFERENCES patients(id),
    doctor_id        UUID          NOT NULL REFERENCES users(id),
    lab_id           UUID          REFERENCES external_labs(id),
    client_id        UUID          REFERENCES lab_clients(id),  -- set when work is for external client
    procedure_id     UUID,           -- cross-module: no FK
    is_external      BOOLEAN       NOT NULL DEFAULT FALSE, -- TRUE when client_id is set
    status           VARCHAR(20)   NOT NULL DEFAULT 'Draft'
        CHECK (status IN ('Draft','Sent','InProgress','ResultReceived','Completed','Cancelled')),
    description      TEXT,
    sent_at          TIMESTAMPTZ,
    expected_at      DATE,
    received_at      TIMESTAMPTZ,
    total_cost       NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_revenue    NUMERIC(12,2) NOT NULL DEFAULT 0, -- billed to client (if external)
    currency         CHAR(3)       NOT NULL DEFAULT 'LYD',
    notes            TEXT,
    cancelled_reason TEXT,
    created_by_id    UUID          REFERENCES users(id),
    created_at       TIMESTAMPTZ   NOT NULL DEFAULT NOW(),
    updated_at       TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_lab_orders_patient  ON lab_orders(patient_id);
CREATE INDEX IF NOT EXISTS ix_lab_orders_status   ON lab_orders(status) WHERE status NOT IN ('Completed','Cancelled');
CREATE INDEX IF NOT EXISTS ix_lab_orders_doctor   ON lab_orders(doctor_id);
CREATE INDEX IF NOT EXISTS ix_lab_orders_client   ON lab_orders(client_id) WHERE client_id IS NOT NULL;

-- Lab Order Items
CREATE TABLE IF NOT EXISTS lab_order_items (
    id          UUID          PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id    UUID          NOT NULL REFERENCES lab_orders(id) ON DELETE CASCADE,
    item_name   VARCHAR(200)  NOT NULL,
    description TEXT,
    quantity    SMALLINT      NOT NULL DEFAULT 1 CHECK (quantity > 0),
    unit_cost   NUMERIC(12,2) NOT NULL DEFAULT 0,
    total_cost  NUMERIC(12,2) NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS ix_lab_items_order ON lab_order_items(order_id);

-- Lab Results
CREATE TABLE IF NOT EXISTS lab_results (
    id             UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    order_id       UUID         NOT NULL REFERENCES lab_orders(id),
    result_notes   TEXT,
    storage_bucket VARCHAR(100),  -- IFileStorageService bucket
    storage_key    VARCHAR(500),  -- IFileStorageService key
    file_name      VARCHAR(200),
    file_size      BIGINT,
    received_by_id UUID         REFERENCES users(id),
    received_at    TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_lab_results_order ON lab_results(order_id);

COMMIT;
