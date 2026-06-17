-- ============================================================
-- Migration 002: Seed — 40 Permissions + System Roles + Admin User
-- ============================================================

BEGIN;

-- ── 40 Permissions ───────────────────────────────────────────
INSERT INTO permissions (id, name, display_name, module) VALUES
-- IAM
(gen_random_uuid(), 'Users.View',           'عرض المستخدمين',           'IAM'),
(gen_random_uuid(), 'Users.Create',         'إنشاء مستخدم',             'IAM'),
(gen_random_uuid(), 'Users.Edit',           'تعديل مستخدم',             'IAM'),
(gen_random_uuid(), 'Users.Delete',         'حذف مستخدم',               'IAM'),
(gen_random_uuid(), 'Roles.View',           'عرض الأدوار',              'IAM'),
(gen_random_uuid(), 'Roles.Create',         'إنشاء دور',                'IAM'),
(gen_random_uuid(), 'Roles.Edit',           'تعديل دور',                'IAM'),
(gen_random_uuid(), 'Roles.Delete',         'حذف دور',                  'IAM'),
(gen_random_uuid(), 'Settings.View',        'عرض الإعدادات',            'IAM'),
(gen_random_uuid(), 'Settings.Edit',        'تعديل الإعدادات',          'IAM'),
-- Patients
(gen_random_uuid(), 'Patients.View',        'عرض المرضى',               'Patients'),
(gen_random_uuid(), 'Patients.Create',      'تسجيل مريض',               'Patients'),
(gen_random_uuid(), 'Patients.Edit',        'تعديل بيانات مريض',        'Patients'),
(gen_random_uuid(), 'Patients.Delete',      'حذف مريض',                 'Patients'),
-- Appointments
(gen_random_uuid(), 'Appointments.View',    'عرض المواعيد',             'Scheduling'),
(gen_random_uuid(), 'Appointments.Create',  'حجز موعد',                 'Scheduling'),
(gen_random_uuid(), 'Appointments.Edit',    'تعديل موعد',               'Scheduling'),
(gen_random_uuid(), 'Appointments.Delete',  'حذف موعد',                 'Scheduling'),
-- Clinical
(gen_random_uuid(), 'Clinical.View',        'عرض السجل السريري',        'Clinical'),
(gen_random_uuid(), 'Clinical.Create',      'إضافة إجراء سريري',        'Clinical'),
(gen_random_uuid(), 'Clinical.Edit',        'تعديل إجراء سريري',        'Clinical'),
(gen_random_uuid(), 'Clinical.Delete',      'حذف إجراء سريري',          'Clinical'),
-- Treasury
(gen_random_uuid(), 'Treasury.View',        'عرض الخزينة',              'Treasury'),
(gen_random_uuid(), 'Treasury.Create',      'إنشاء حركة مالية',         'Treasury'),
(gen_random_uuid(), 'Treasury.Edit',        'تعديل حركة مالية',         'Treasury'),
(gen_random_uuid(), 'Treasury.Delete',      'حذف حركة مالية',           'Treasury'),
-- Inventory
(gen_random_uuid(), 'Inventory.View',       'عرض المخزون',              'Inventory'),
(gen_random_uuid(), 'Inventory.Create',     'إضافة صنف',                'Inventory'),
(gen_random_uuid(), 'Inventory.Edit',       'تعديل صنف',                'Inventory'),
(gen_random_uuid(), 'Inventory.Delete',     'حذف صنف',                  'Inventory'),
-- Laboratory
(gen_random_uuid(), 'Lab.View',             'عرض المعمل',               'Laboratory'),
(gen_random_uuid(), 'Lab.Create',           'إنشاء أمر معمل',           'Laboratory'),
(gen_random_uuid(), 'Lab.Edit',             'تعديل أمر معمل',           'Laboratory'),
(gen_random_uuid(), 'Lab.Manage',           'إدارة المعمل',             'Laboratory'),
-- Radiology
(gen_random_uuid(), 'Radiology.View',       'عرض الأشعة',               'Radiology'),
(gen_random_uuid(), 'Radiology.Create',     'إنشاء طلب أشعة',           'Radiology'),
(gen_random_uuid(), 'Radiology.Edit',       'تعديل طلب أشعة',           'Radiology'),
(gen_random_uuid(), 'Radiology.Manage',     'إدارة الأشعة',             'Radiology'),
-- Reports
(gen_random_uuid(), 'Reports.View',         'عرض التقارير',             'Reports'),
(gen_random_uuid(), 'Reports.Export',       'تصدير التقارير',           'Reports')
ON CONFLICT (name) DO NOTHING;

-- ── System Roles ─────────────────────────────────────────────
INSERT INTO roles (id, name, description, is_system) VALUES
('00000000-0000-0000-0000-000000000001', 'Administrator', 'مدير النظام — كامل الصلاحيات', TRUE),
('00000000-0000-0000-0000-000000000002', 'Receptionist',  'موظف الاستقبال',                FALSE),
('00000000-0000-0000-0000-000000000003', 'Doctor',        'طبيب',                          FALSE),
('00000000-0000-0000-0000-000000000004', 'Accountant',    'محاسب',                         FALSE)
ON CONFLICT (name) DO NOTHING;

-- ── Admin Role: All Permissions ───────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0000-000000000001', id FROM permissions
ON CONFLICT DO NOTHING;

-- ── Receptionist Permissions ──────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0000-000000000002', id
FROM permissions
WHERE name IN ('Patients.View','Patients.Create','Patients.Edit',
               'Appointments.View','Appointments.Create','Appointments.Edit','Appointments.Delete')
ON CONFLICT DO NOTHING;

-- ── Doctor Permissions ────────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0000-000000000003', id
FROM permissions
WHERE name IN ('Patients.View','Patients.Create','Patients.Edit',
               'Appointments.View','Appointments.Create',
               'Clinical.View','Clinical.Create','Clinical.Edit',
               'Lab.View','Lab.Create','Radiology.View','Radiology.Create')
ON CONFLICT DO NOTHING;

-- ── Accountant Permissions ────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0000-000000000004', id
FROM permissions
WHERE name IN ('Patients.View','Treasury.View','Treasury.Create','Treasury.Edit',
               'Inventory.View','Reports.View','Reports.Export')
ON CONFLICT DO NOTHING;

-- ── Default Admin User (password: Admin@123) ──────────────────
INSERT INTO users (id, username, password_hash, full_name, email, is_active)
VALUES (
    '00000000-0000-0000-0000-000000000001',
    'admin',
    '$2a$11$K7Q3FnLpqyJm8/.1KGVBQeWzOE6y1DZH.o/PbGZ0VPtFkQ9UoMT0a',
    'مدير النظام',
    'admin@dentalerp.local',
    TRUE
) ON CONFLICT (username) DO NOTHING;

-- Admin User → Administrator Role
INSERT INTO user_roles (user_id, role_id)
VALUES ('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000001')
ON CONFLICT DO NOTHING;

-- ── Default System Settings ───────────────────────────────────
INSERT INTO system_settings (key, value, description, "group") VALUES
('clinic.name',             'عيادة الأسنان', 'اسم العيادة',                      'Clinic'),
('clinic.phone',            '',               'رقم هاتف العيادة',                  'Clinic'),
('clinic.address',          '',               'عنوان العيادة',                     'Clinic'),
('clinic.currency',         'SAR',            'رمز العملة',                        'Clinic'),
('workflow.require_approval','false',          'تفعيل نظام الموافقة على الإجراءات', 'Workflow'),
('invoice.tax_rate',        '0',              'نسبة الضريبة %',                    'Finance'),
('invoice.allow_discount',  'true',           'السماح بالخصم على الفواتير',        'Finance'),
('appointment.slot_minutes','30',             'مدة الموعد الافتراضية (دقيقة)',     'Scheduling')
ON CONFLICT (key) DO NOTHING;

COMMIT;
