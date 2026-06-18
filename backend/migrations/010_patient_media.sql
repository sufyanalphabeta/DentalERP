-- Migration 010: Patient Media (MinIO object storage)

CREATE TABLE IF NOT EXISTS patient_media (
    id              UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID         NOT NULL REFERENCES patients(id),
    appointment_id  UUID         REFERENCES appointments(id),
    media_type      VARCHAR(20)  NOT NULL
        CHECK (media_type IN ('Before','After','OPG','CBCT','XRay','Document')),
    file_name       VARCHAR(255) NOT NULL,
    file_path       TEXT         NOT NULL,   -- MinIO object key: patient-media/{patientId}/...
    file_size_bytes BIGINT,
    mime_type       VARCHAR(100),
    thumbnail_path  TEXT,                    -- MinIO thumbnail for Before/After/OPG/XRay
    title           VARCHAR(200),
    description     TEXT,
    tooth_id        SMALLINT     REFERENCES teeth(id),
    is_required     BOOLEAN      NOT NULL DEFAULT FALSE,
    is_approved     BOOLEAN      NOT NULL DEFAULT FALSE,
    approved_by_id  UUID         REFERENCES users(id),
    approved_at     TIMESTAMPTZ,
    uploaded_by_id  UUID         NOT NULL REFERENCES users(id),
    uploaded_at     TIMESTAMPTZ  NOT NULL DEFAULT NOW(),
    deleted_at      TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_media_patient
    ON patient_media(patient_id) WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_media_type
    ON patient_media(patient_id, media_type) WHERE deleted_at IS NULL;

CREATE INDEX IF NOT EXISTS ix_media_appt
    ON patient_media(appointment_id) WHERE appointment_id IS NOT NULL;
