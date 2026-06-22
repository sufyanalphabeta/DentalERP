-- Migration 030: Add missing module permissions (Purchasing, Assets, Expenses, Lab alias)
BEGIN;

INSERT INTO permissions (id, name, display_name, module) VALUES
(gen_random_uuid(), 'Purchasing.View',   'عرض المشتريات',       'Purchasing'),
(gen_random_uuid(), 'Purchasing.Create', 'إنشاء أمر شراء',      'Purchasing'),
(gen_random_uuid(), 'Purchasing.Edit',   'تعديل أمر شراء',      'Purchasing'),
(gen_random_uuid(), 'Purchasing.Delete', 'حذف أمر شراء',        'Purchasing'),
(gen_random_uuid(), 'Assets.View',       'عرض الأصول الثابتة',  'Assets'),
(gen_random_uuid(), 'Assets.Create',     'إضافة أصل',           'Assets'),
(gen_random_uuid(), 'Assets.Edit',       'تعديل أصل',           'Assets'),
(gen_random_uuid(), 'Assets.Delete',     'حذف أصل',             'Assets'),
(gen_random_uuid(), 'Expenses.View',     'عرض المصروفات',        'Expenses'),
(gen_random_uuid(), 'Expenses.Create',   'إضافة مصروف',          'Expenses'),
(gen_random_uuid(), 'Expenses.Edit',     'تعديل مصروف',          'Expenses'),
(gen_random_uuid(), 'Expenses.Delete',   'حذف مصروف',            'Expenses'),
(gen_random_uuid(), 'Laboratory.View',   'عرض المختبر',          'Laboratory'),
(gen_random_uuid(), 'Laboratory.Create', 'إنشاء أمر مختبر',     'Laboratory'),
(gen_random_uuid(), 'Laboratory.Edit',   'تعديل أمر مختبر',     'Laboratory'),
(gen_random_uuid(), 'Laboratory.Manage', 'إدارة المختبر',        'Laboratory')
ON CONFLICT (name) DO NOTHING;

-- Grant all new permissions to Administrator role
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0000-000000000001', id
FROM permissions
WHERE name IN (
    'Purchasing.View','Purchasing.Create','Purchasing.Edit','Purchasing.Delete',
    'Assets.View','Assets.Create','Assets.Edit','Assets.Delete',
    'Expenses.View','Expenses.Create','Expenses.Edit','Expenses.Delete',
    'Laboratory.View','Laboratory.Create','Laboratory.Edit','Laboratory.Manage'
)
ON CONFLICT DO NOTHING;

-- Grant Expenses and Purchasing to Accountant role
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0000-000000000004', id
FROM permissions
WHERE name IN ('Purchasing.View','Expenses.View','Expenses.Create','Assets.View')
ON CONFLICT DO NOTHING;

COMMIT;
