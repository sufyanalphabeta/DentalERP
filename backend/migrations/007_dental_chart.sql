-- Migration 007: Dental Chart Entries (single table + is_current strategy)

CREATE TABLE IF NOT EXISTS dental_chart_entries (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID        NOT NULL REFERENCES patients(id),
    tooth_id        SMALLINT    NOT NULL REFERENCES teeth(id),
    surface         VARCHAR(20),
        -- NULL = كامل السن | M=Mesial D=Distal B=Buccal L=Lingual O=Occlusal
    condition       VARCHAR(50) NOT NULL
        CHECK (condition IN (
            'Healthy','Caries','Filled','Missing','Extracted',
            'Crown','Bridge','Implant','RootCanal','Fracture',
            'Impacted','Sensitive','Mobility','Other'
        )),
    severity        VARCHAR(10) CHECK (severity IN ('Mild','Moderate','Severe')),
    notes           TEXT,
    recorded_by_id  UUID        NOT NULL REFERENCES users(id),
    recorded_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    appointment_id  UUID        REFERENCES appointments(id),
    is_current      BOOLEAN     NOT NULL DEFAULT TRUE
);

CREATE INDEX IF NOT EXISTS ix_chart_tooth
    ON dental_chart_entries(patient_id, tooth_id, is_current);

CREATE INDEX IF NOT EXISTS ix_chart_patient_history
    ON dental_chart_entries(patient_id, recorded_at DESC);

CREATE INDEX IF NOT EXISTS ix_chart_appointment
    ON dental_chart_entries(appointment_id);
