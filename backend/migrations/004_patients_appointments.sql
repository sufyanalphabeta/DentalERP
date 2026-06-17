-- ============================================================
-- Migration 004: Patients + Appointments + Reception Queue
-- ============================================================

BEGIN;

-- ── Patients ────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS patients (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    file_number     VARCHAR(20)     NOT NULL UNIQUE,
    full_name       VARCHAR(200)    NOT NULL,
    date_of_birth   DATE,
    gender          VARCHAR(10)     CHECK (gender IN ('Male','Female')),
    phone           VARCHAR(20)     NOT NULL,
    phone2          VARCHAR(20),
    email           VARCHAR(200),
    address         TEXT,
    national_id     VARCHAR(50),
    blood_type      VARCHAR(5)      CHECK (blood_type IN ('A+','A-','B+','B-','O+','O-','AB+','AB-')),
    allergies       TEXT,
    chronic_diseases TEXT,
    notes           TEXT,
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ,
    deleted_at      TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_patients_file_number   ON patients(file_number);
CREATE INDEX IF NOT EXISTS ix_patients_phone         ON patients(phone);
CREATE INDEX IF NOT EXISTS ix_patients_name          ON patients(full_name);
CREATE INDEX IF NOT EXISTS ix_patients_national_id   ON patients(national_id) WHERE national_id IS NOT NULL;

-- ── Appointment Types ────────────────────────────────────────
CREATE TABLE IF NOT EXISTS appointment_types (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100)    NOT NULL,
    name_ar         VARCHAR(100)    NOT NULL,
    default_duration_minutes INTEGER NOT NULL DEFAULT 30,
    color           VARCHAR(7)      NOT NULL DEFAULT '#3B82F6',
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW()
);

INSERT INTO appointment_types (name, name_ar, default_duration_minutes, color) VALUES
    ('Consultation',        'استشارة',          30, '#3B82F6'),
    ('Cleaning',            'تنظيف',            45, '#10B981'),
    ('Filling',             'حشو',              60, '#F59E0B'),
    ('Extraction',          'خلع',              30, '#EF4444'),
    ('Root Canal',          'علاج عصب',         90, '#8B5CF6'),
    ('Crown',               'تاج',              60, '#EC4899'),
    ('Orthodontics',        'تقويم',            45, '#06B6D4'),
    ('Implant',             'زراعة',            120,'#84CC16'),
    ('X-Ray',               'أشعة',             15, '#6B7280'),
    ('Follow-up',           'متابعة',           20, '#14B8A6')
ON CONFLICT DO NOTHING;

-- ── Appointments ────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS appointments (
    id                  UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id          UUID        NOT NULL REFERENCES patients(id),
    doctor_id           UUID        NOT NULL REFERENCES users(id),
    appointment_type_id UUID        REFERENCES appointment_types(id),
    scheduled_at        TIMESTAMPTZ NOT NULL,
    duration_minutes    INTEGER     NOT NULL DEFAULT 30,
    status              VARCHAR(20) NOT NULL DEFAULT 'Scheduled'
        CHECK (status IN ('Scheduled','Confirmed','InProgress','Completed','Cancelled','NoShow')),
    chief_complaint     TEXT,
    notes               TEXT,
    cancellation_reason TEXT,
    created_by_id       UUID        REFERENCES users(id),
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ,
    deleted_at          TIMESTAMPTZ
);

CREATE INDEX IF NOT EXISTS ix_appointments_patient      ON appointments(patient_id);
CREATE INDEX IF NOT EXISTS ix_appointments_doctor       ON appointments(doctor_id);
CREATE INDEX IF NOT EXISTS ix_appointments_scheduled_at ON appointments(scheduled_at);
CREATE INDEX IF NOT EXISTS ix_appointments_status       ON appointments(status);

-- ── Queue ────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS queue_entries (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    appointment_id  UUID        REFERENCES appointments(id),
    patient_id      UUID        NOT NULL REFERENCES patients(id),
    doctor_id       UUID        REFERENCES users(id),
    queue_date      DATE        NOT NULL DEFAULT CURRENT_DATE,
    token_number    INTEGER     NOT NULL,
    status          VARCHAR(20) NOT NULL DEFAULT 'Waiting'
        CHECK (status IN ('Waiting','Called','InProgress','Completed','Skipped')),
    check_in_at     TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    called_at       TIMESTAMPTZ,
    started_at      TIMESTAMPTZ,
    completed_at    TIMESTAMPTZ,
    notes           TEXT,
    UNIQUE (queue_date, token_number)
);

CREATE INDEX IF NOT EXISTS ix_queue_date        ON queue_entries(queue_date);
CREATE INDEX IF NOT EXISTS ix_queue_patient     ON queue_entries(patient_id);
CREATE INDEX IF NOT EXISTS ix_queue_doctor      ON queue_entries(doctor_id);
CREATE INDEX IF NOT EXISTS ix_queue_status      ON queue_entries(status);

COMMIT;
