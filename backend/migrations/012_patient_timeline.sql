-- Migration 012: Patient Timeline (append-only event log)

CREATE TABLE IF NOT EXISTS patient_timeline (
    id                  UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id          UUID        NOT NULL REFERENCES patients(id),
    event_type          VARCHAR(50) NOT NULL,
    title               VARCHAR(200) NOT NULL,
    description         TEXT,
    actor_id            UUID        REFERENCES users(id),
    actor_name          VARCHAR(100),  -- snapshot — survives user deletion
    linked_entity_type  VARCHAR(50),
    linked_entity_id    UUID,
    metadata            JSONB,
    event_at            TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    is_visible_to_doctor  BOOLEAN   NOT NULL DEFAULT TRUE,
    is_visible_to_patient BOOLEAN   NOT NULL DEFAULT FALSE,
    event_category      VARCHAR(20) NOT NULL DEFAULT 'Administrative'
        CHECK (event_category IN (
            'Clinical','Financial','Administrative',
            'Insurance','Radiology','Laboratory'
        ))
);

-- Append-only: no UPDATE or DELETE statements are permitted on this table

CREATE INDEX IF NOT EXISTS ix_timeline_patient
    ON patient_timeline(patient_id, event_at DESC);

CREATE INDEX IF NOT EXISTS ix_timeline_event_type
    ON patient_timeline(event_type);

CREATE INDEX IF NOT EXISTS ix_timeline_category
    ON patient_timeline(patient_id, event_category);

CREATE INDEX IF NOT EXISTS ix_timeline_entity
    ON patient_timeline(linked_entity_type, linked_entity_id)
    WHERE linked_entity_id IS NOT NULL;
