-- Migration 008: Treatment Plans + Items

CREATE TABLE IF NOT EXISTS treatment_plans (
    id              UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID         NOT NULL REFERENCES patients(id),
    doctor_id       UUID         NOT NULL REFERENCES users(id),
    title           VARCHAR(200) NOT NULL,
    description     TEXT,
    estimated_cost  DECIMAL(10,2) NOT NULL DEFAULT 0,
    total_cost      DECIMAL(10,2) NOT NULL DEFAULT 0,
    actual_cost     DECIMAL(10,2) NOT NULL DEFAULT 0,
    paid_amount     DECIMAL(10,2) NOT NULL DEFAULT 0,
    priority        VARCHAR(10)  NOT NULL DEFAULT 'Normal'
        CHECK (priority IN ('Low','Normal','High','Urgent')),
    status          VARCHAR(20)  NOT NULL DEFAULT 'Draft'
        CHECK (status IN ('Draft','Active','Completed','Cancelled','OnHold')),
    start_date      DATE,
    end_date        DATE,
    notes           TEXT,
    created_at      TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    deleted_at      TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_treatment_patient
    ON treatment_plans(patient_id) WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_treatment_doctor
    ON treatment_plans(doctor_id) WHERE deleted_at IS NULL;

CREATE TABLE IF NOT EXISTS treatment_plan_items (
    id                  UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    treatment_plan_id   UUID         NOT NULL REFERENCES treatment_plans(id) ON DELETE CASCADE,
    tooth_id            SMALLINT     REFERENCES teeth(id),
    surface             VARCHAR(20),
    procedure_name      VARCHAR(200) NOT NULL,
    procedure_code      VARCHAR(50),
    quantity            INTEGER      NOT NULL DEFAULT 1,
    unit_price          DECIMAL(10,2) NOT NULL DEFAULT 0,
    discount_percent    DECIMAL(5,2)  NOT NULL DEFAULT 0,
    status              VARCHAR(20)  NOT NULL DEFAULT 'Pending'
        CHECK (status IN ('Pending','InProgress','Completed','Cancelled')),
    sequence_number     INTEGER      NOT NULL DEFAULT 1,
    notes               TEXT,
    created_at          TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_plan_items_plan
    ON treatment_plan_items(treatment_plan_id);
