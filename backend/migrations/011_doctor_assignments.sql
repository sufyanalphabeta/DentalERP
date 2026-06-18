-- Migration 011: Doctor Assignments (lifecycle with status machine)
-- No UNIQUE constraint — same doctor can be re-assigned after Completed/Transferred

CREATE TABLE IF NOT EXISTS doctor_assignments (
    id                UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id        UUID        NOT NULL REFERENCES patients(id),
    doctor_id         UUID        NOT NULL REFERENCES users(id),
    status            VARCHAR(20) NOT NULL DEFAULT 'Active'
        CHECK (status IN ('Active','Completed','Transferred','Closed')),
    assigned_at       TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ended_at          TIMESTAMPTZ,
    can_view          BOOLEAN     NOT NULL DEFAULT TRUE,
    can_edit          BOOLEAN     NOT NULL DEFAULT TRUE,
    transferred_to_id UUID        REFERENCES users(id),
    transferred_at    TIMESTAMPTZ,
    transfer_reason   TEXT,
    is_primary        BOOLEAN     NOT NULL DEFAULT FALSE,
    notes             TEXT,
    assigned_by_id    UUID        REFERENCES users(id)
);

CREATE INDEX IF NOT EXISTS ix_assignment_patient
    ON doctor_assignments(patient_id);

CREATE INDEX IF NOT EXISTS ix_assignment_doctor
    ON doctor_assignments(doctor_id);

CREATE INDEX IF NOT EXISTS ix_assignment_active
    ON doctor_assignments(patient_id, doctor_id)
    WHERE status = 'Active';
