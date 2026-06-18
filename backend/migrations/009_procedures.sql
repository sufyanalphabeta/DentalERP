-- Migration 009: Procedures

CREATE TABLE IF NOT EXISTS procedures (
    id                      UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    appointment_id          UUID         NOT NULL REFERENCES appointments(id),
    patient_id              UUID         NOT NULL REFERENCES patients(id),
    doctor_id               UUID         NOT NULL REFERENCES users(id),
    treatment_plan_item_id  UUID         REFERENCES treatment_plan_items(id),
    tooth_id                SMALLINT     REFERENCES teeth(id),
    surface                 VARCHAR(20),
    procedure_name          VARCHAR(200) NOT NULL,
    procedure_code          VARCHAR(50),
    service_id              UUID,
        -- NULL now — FK added in Phase 5 when services catalog is built
    notes                   TEXT,
    performed_at            TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    duration_minutes        INTEGER,
    billing_status          VARCHAR(20)  NOT NULL DEFAULT 'Pending'
        CHECK (billing_status IN ('Pending','SentToTreasury','Paid','Cancelled')),
    created_at              TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS ix_procedures_appointment
    ON procedures(appointment_id);

CREATE INDEX IF NOT EXISTS ix_procedures_patient
    ON procedures(patient_id);

CREATE INDEX IF NOT EXISTS ix_procedures_doctor
    ON procedures(doctor_id);

CREATE INDEX IF NOT EXISTS ix_procedures_service
    ON procedures(service_id)
    WHERE service_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS ix_procedures_billing
    ON procedures(billing_status)
    WHERE billing_status != 'Paid';
