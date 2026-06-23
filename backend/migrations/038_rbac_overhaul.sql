-- ============================================================
-- Migration 038: Enterprise RBAC Overhaul
--   1. Add screen + sort_order columns to permissions
--   2. Create user_permissions table (per-user grant/deny)
--   3. Replace flat permissions with full module/screen/action catalog
--   4. Replace 4 basic roles with 13 professional role templates
-- ============================================================

BEGIN;

-- ── 1. Schema changes ─────────────────────────────────────────────

ALTER TABLE permissions
  ADD COLUMN IF NOT EXISTS screen VARCHAR(100),
  ADD COLUMN IF NOT EXISTS sort_order INT NOT NULL DEFAULT 0;

CREATE TABLE IF NOT EXISTS user_permissions (
  user_id      UUID NOT NULL REFERENCES users(id)       ON DELETE CASCADE,
  permission_id UUID NOT NULL REFERENCES permissions(id) ON DELETE CASCADE,
  grant_type   VARCHAR(10) NOT NULL DEFAULT 'Grant'  CHECK (grant_type IN ('Grant','Deny')),
  created_at   TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  PRIMARY KEY (user_id, permission_id)
);
CREATE INDEX IF NOT EXISTS ix_user_permissions_user ON user_permissions(user_id);

-- ── 2. Wipe old permissions & roles (keep admin user) ─────────────

DELETE FROM role_permissions;
DELETE FROM roles;
DELETE FROM permissions;

-- ── 3. Full Permission Catalog ────────────────────────────────────
-- Format: name = 'Module.Screen.Action'
-- Columns: id, name, display_name, module, screen, sort_order

INSERT INTO permissions (id, name, display_name, module, screen, sort_order) VALUES

-- ── Dashboard ──────────────────────────────────────────────────
(gen_random_uuid(),'Dashboard.Overview.View',     'عرض لوحة القيادة',               'Dashboard','Overview',    10),
(gen_random_uuid(),'Dashboard.Revenue.View',      'عرض إيرادات الشهر',              'Dashboard','Revenue',     11),
(gen_random_uuid(),'Dashboard.Operations.View',   'عرض إحصاءات العمليات',           'Dashboard','Operations',  12),
(gen_random_uuid(),'Dashboard.Financial.View',    'عرض الصحة المالية',              'Dashboard','Financial',   13),
(gen_random_uuid(),'Dashboard.Inventory.View',    'عرض مخزون ومشتريات',             'Dashboard','Inventory',   14),
(gen_random_uuid(),'Dashboard.Executive.View',    'لوحة التحكم التنفيذية',          'Dashboard','Executive',   15),

-- ── Patients ───────────────────────────────────────────────────
(gen_random_uuid(),'Patients.Patients.View',      'عرض المرضى',                     'Patients','Patients',     20),
(gen_random_uuid(),'Patients.Patients.Create',    'إضافة مريض',                     'Patients','Patients',     21),
(gen_random_uuid(),'Patients.Patients.Edit',      'تعديل مريض',                     'Patients','Patients',     22),
(gen_random_uuid(),'Patients.Patients.Delete',    'حذف مريض',                       'Patients','Patients',     23),
(gen_random_uuid(),'Patients.Patients.Print',     'طباعة ملف مريض',                 'Patients','Patients',     24),
(gen_random_uuid(),'Patients.Patients.ExportExcel','تصدير قائمة المرضى Excel',      'Patients','Patients',     25),
(gen_random_uuid(),'Patients.Patients.ExportPdf', 'تصدير قائمة المرضى PDF',         'Patients','Patients',     26),

-- ── Appointments ───────────────────────────────────────────────
(gen_random_uuid(),'Appointments.Appointments.View',       'عرض المواعيد',           'Appointments','Appointments',30),
(gen_random_uuid(),'Appointments.Appointments.Create',     'حجز موعد',               'Appointments','Appointments',31),
(gen_random_uuid(),'Appointments.Appointments.Edit',       'تعديل موعد',             'Appointments','Appointments',32),
(gen_random_uuid(),'Appointments.Appointments.Delete',     'حذف موعد',               'Appointments','Appointments',33),
(gen_random_uuid(),'Appointments.Appointments.Reschedule', 'إعادة جدولة موعد',       'Appointments','Appointments',34),
(gen_random_uuid(),'Appointments.Appointments.Cancel',     'إلغاء موعد',             'Appointments','Appointments',35),
(gen_random_uuid(),'Appointments.Appointments.Print',      'طباعة مواعيد',           'Appointments','Appointments',36),
(gen_random_uuid(),'Appointments.Queue.View',              'عرض طابور الانتظار',     'Appointments','Queue',       40),
(gen_random_uuid(),'Appointments.Queue.Create',            'إضافة للطابور',          'Appointments','Queue',       41),
(gen_random_uuid(),'Appointments.Queue.Edit',              'تعديل الطابور',          'Appointments','Queue',       42),
(gen_random_uuid(),'Appointments.Queue.Delete',            'إزالة من الطابور',       'Appointments','Queue',       43),

-- ── Clinical ───────────────────────────────────────────────────
(gen_random_uuid(),'Clinical.Workspace.View',        'مساحة الطبيب',               'Clinical','Workspace',      50),
(gen_random_uuid(),'Clinical.TreatmentPlans.View',   'عرض خطط العلاج',             'Clinical','TreatmentPlans', 51),
(gen_random_uuid(),'Clinical.TreatmentPlans.Create', 'إنشاء خطة علاج',             'Clinical','TreatmentPlans', 52),
(gen_random_uuid(),'Clinical.TreatmentPlans.Edit',   'تعديل خطة علاج',             'Clinical','TreatmentPlans', 53),
(gen_random_uuid(),'Clinical.TreatmentPlans.Delete', 'حذف خطة علاج',               'Clinical','TreatmentPlans', 54),
(gen_random_uuid(),'Clinical.TreatmentPlans.Approve','اعتماد خطة علاج',            'Clinical','TreatmentPlans', 55),
(gen_random_uuid(),'Clinical.TreatmentPlans.Print',  'طباعة خطة علاج',             'Clinical','TreatmentPlans', 56),
(gen_random_uuid(),'Clinical.DentalChart.View',      'عرض مخطط الأسنان',           'Clinical','DentalChart',    57),
(gen_random_uuid(),'Clinical.DentalChart.Edit',      'تحديث مخطط الأسنان',         'Clinical','DentalChart',    58),
(gen_random_uuid(),'Clinical.Procedures.View',       'عرض الإجراءات السريرية',      'Clinical','Procedures',     59),
(gen_random_uuid(),'Clinical.Procedures.Create',     'إضافة إجراء سريري',          'Clinical','Procedures',     60),
(gen_random_uuid(),'Clinical.Procedures.Edit',       'تعديل إجراء سريري',          'Clinical','Procedures',     61),
(gen_random_uuid(),'Clinical.Procedures.Delete',     'حذف إجراء سريري',            'Clinical','Procedures',     62),
(gen_random_uuid(),'Clinical.Files.View',            'عرض ملفات المريض',           'Clinical','Files',          63),
(gen_random_uuid(),'Clinical.Files.Upload',          'رفع ملفات للمريض',           'Clinical','Files',          64),
(gen_random_uuid(),'Clinical.Files.Delete',          'حذف ملفات المريض',           'Clinical','Files',          65),

-- ── Laboratory ─────────────────────────────────────────────────
(gen_random_uuid(),'Lab.Orders.View',           'عرض طلبات المختبر',              'Lab','Orders',      70),
(gen_random_uuid(),'Lab.Orders.Create',         'إنشاء طلب مختبر',               'Lab','Orders',      71),
(gen_random_uuid(),'Lab.Orders.Edit',           'تعديل طلب مختبر',               'Lab','Orders',      72),
(gen_random_uuid(),'Lab.Orders.Delete',         'حذف طلب مختبر',                 'Lab','Orders',      73),
(gen_random_uuid(),'Lab.Orders.Print',          'طباعة طلب مختبر',               'Lab','Orders',      74),
(gen_random_uuid(),'Lab.Results.View',          'عرض نتائج المختبر',             'Lab','Results',     75),
(gen_random_uuid(),'Lab.Results.Edit',          'إدخال نتائج المختبر',           'Lab','Results',     76),
(gen_random_uuid(),'Lab.ExternalLabs.View',     'عرض المختبرات الخارجية',        'Lab','ExternalLabs',77),
(gen_random_uuid(),'Lab.ExternalLabs.Create',   'إضافة مختبر خارجي',             'Lab','ExternalLabs',78),
(gen_random_uuid(),'Lab.ExternalLabs.Edit',     'تعديل مختبر خارجي',             'Lab','ExternalLabs',79),

-- ── Radiology ──────────────────────────────────────────────────
(gen_random_uuid(),'Radiology.Orders.View',     'عرض طلبات الأشعة',              'Radiology','Orders', 80),
(gen_random_uuid(),'Radiology.Orders.Create',   'إنشاء طلب أشعة',               'Radiology','Orders', 81),
(gen_random_uuid(),'Radiology.Orders.Edit',     'تعديل طلب أشعة',               'Radiology','Orders', 82),
(gen_random_uuid(),'Radiology.Orders.Delete',   'حذف طلب أشعة',                 'Radiology','Orders', 83),
(gen_random_uuid(),'Radiology.Orders.Print',    'طباعة طلب أشعة',               'Radiology','Orders', 84),
(gen_random_uuid(),'Radiology.Images.View',     'عرض صور الأشعة',               'Radiology','Images', 85),
(gen_random_uuid(),'Radiology.Images.Upload',   'رفع صور أشعة',                 'Radiology','Images', 86),

-- ── Financial ──────────────────────────────────────────────────
(gen_random_uuid(),'Financial.CashierDesk.View',  'مساحة الصراف',               'Financial','CashierDesk',  90),
(gen_random_uuid(),'Financial.Invoices.View',     'عرض الفواتير',               'Financial','Invoices',     91),
(gen_random_uuid(),'Financial.Invoices.Create',   'إنشاء فاتورة',               'Financial','Invoices',     92),
(gen_random_uuid(),'Financial.Invoices.Edit',     'تعديل فاتورة',               'Financial','Invoices',     93),
(gen_random_uuid(),'Financial.Invoices.Delete',   'حذف فاتورة',                 'Financial','Invoices',     94),
(gen_random_uuid(),'Financial.Invoices.Print',    'طباعة فاتورة',               'Financial','Invoices',     95),
(gen_random_uuid(),'Financial.Invoices.ExportPdf','تصدير فاتورة PDF',           'Financial','Invoices',     96),
(gen_random_uuid(),'Financial.Invoices.Confirm',  'تأكيد فاتورة',               'Financial','Invoices',     97),
(gen_random_uuid(),'Financial.Invoices.Cancel',   'إلغاء فاتورة',               'Financial','Invoices',     98),
(gen_random_uuid(),'Financial.Invoices.Refund',   'إصدار مرتجع',                'Financial','Invoices',     99),
(gen_random_uuid(),'Financial.Installments.View', 'عرض الأقساط',                'Financial','Installments', 100),
(gen_random_uuid(),'Financial.Installments.Create','إنشاء خطة أقساط',           'Financial','Installments', 101),
(gen_random_uuid(),'Financial.Installments.Edit', 'تعديل خطة أقساط',            'Financial','Installments', 102),
(gen_random_uuid(),'Financial.Installments.Delete','حذف خطة أقساط',             'Financial','Installments', 103),
(gen_random_uuid(),'Financial.Payments.View',     'عرض المدفوعات',              'Financial','Payments',     104),
(gen_random_uuid(),'Financial.Payments.Create',   'تسجيل دفعة',                 'Financial','Payments',     105),
(gen_random_uuid(),'Financial.Payments.Delete',   'حذف دفعة',                   'Financial','Payments',     106),
(gen_random_uuid(),'Financial.Treasury.View',     'عرض الخزائن',                'Financial','Treasury',     107),
(gen_random_uuid(),'Financial.Treasury.Create',   'إنشاء خزينة',                'Financial','Treasury',     108),
(gen_random_uuid(),'Financial.Treasury.Edit',     'تعديل الخزينة',              'Financial','Treasury',     109),
(gen_random_uuid(),'Financial.Treasury.Transfer', 'تحويل بين خزائن',            'Financial','Treasury',     110),
(gen_random_uuid(),'Financial.Expenses.View',     'عرض المصروفات',              'Financial','Expenses',     111),
(gen_random_uuid(),'Financial.Expenses.Create',   'إضافة مصروف',                'Financial','Expenses',     112),
(gen_random_uuid(),'Financial.Expenses.Edit',     'تعديل مصروف',                'Financial','Expenses',     113),
(gen_random_uuid(),'Financial.Expenses.Delete',   'حذف مصروف',                  'Financial','Expenses',     114),
(gen_random_uuid(),'Financial.Expenses.ExportPdf','تصدير مصروفات PDF',          'Financial','Expenses',     115),
(gen_random_uuid(),'Financial.Expenses.ExportExcel','تصدير مصروفات Excel',      'Financial','Expenses',     116),
(gen_random_uuid(),'Financial.Doctors.View',      'حسابات الأطباء - عرض',       'Financial','Doctors',      117),
(gen_random_uuid(),'Financial.Doctors.Edit',      'حسابات الأطباء - تعديل',     'Financial','Doctors',      118),

-- ── Insurance ──────────────────────────────────────────────────
(gen_random_uuid(),'Insurance.Companies.View',   'عرض شركات التأمين',           'Insurance','Companies',  120),
(gen_random_uuid(),'Insurance.Companies.Create', 'إضافة شركة تأمين',            'Insurance','Companies',  121),
(gen_random_uuid(),'Insurance.Companies.Edit',   'تعديل شركة تأمين',            'Insurance','Companies',  122),
(gen_random_uuid(),'Insurance.Companies.Delete', 'حذف شركة تأمين',              'Insurance','Companies',  123),
(gen_random_uuid(),'Insurance.Claims.View',      'عرض المطالبات',               'Insurance','Claims',     124),
(gen_random_uuid(),'Insurance.Claims.Create',    'إنشاء مطالبة',                'Insurance','Claims',     125),
(gen_random_uuid(),'Insurance.Claims.Edit',      'تعديل مطالبة',                'Insurance','Claims',     126),
(gen_random_uuid(),'Insurance.Claims.Delete',    'حذف مطالبة',                  'Insurance','Claims',     127),
(gen_random_uuid(),'Insurance.Claims.Print',     'طباعة مطالبة',                'Insurance','Claims',     128),
(gen_random_uuid(),'Insurance.Claims.ExportPdf', 'تصدير مطالبات PDF',           'Insurance','Claims',     129),
(gen_random_uuid(),'Insurance.Claims.Approve',   'اعتماد مطالبة',               'Insurance','Claims',     130),
(gen_random_uuid(),'Insurance.Claims.Cancel',    'إلغاء مطالبة',                'Insurance','Claims',     131),
(gen_random_uuid(),'Insurance.Receivables.View', 'عرض مستحقات التأمين',         'Insurance','Receivables',132),

-- ── Inventory ──────────────────────────────────────────────────
(gen_random_uuid(),'Inventory.Items.View',         'عرض الأصناف',               'Inventory','Items',      140),
(gen_random_uuid(),'Inventory.Items.Create',       'إضافة صنف',                 'Inventory','Items',      141),
(gen_random_uuid(),'Inventory.Items.Edit',         'تعديل صنف',                 'Inventory','Items',      142),
(gen_random_uuid(),'Inventory.Items.Delete',       'حذف صنف',                   'Inventory','Items',      143),
(gen_random_uuid(),'Inventory.Items.ExportExcel',  'تصدير الأصناف Excel',        'Inventory','Items',      144),
(gen_random_uuid(),'Inventory.Movements.View',     'عرض حركات المخزون',         'Inventory','Movements',  145),
(gen_random_uuid(),'Inventory.Movements.Create',   'تسجيل حركة مخزون',          'Inventory','Movements',  146),
(gen_random_uuid(),'Inventory.Stocktake.View',     'عرض الجرد',                 'Inventory','Stocktake',  147),
(gen_random_uuid(),'Inventory.Stocktake.Create',   'إنشاء جرد',                 'Inventory','Stocktake',  148),
(gen_random_uuid(),'Inventory.Alerts.View',        'عرض تنبيهات المخزون',       'Inventory','Alerts',     149),

-- ── Purchasing ─────────────────────────────────────────────────
(gen_random_uuid(),'Purchasing.Suppliers.View',    'عرض الموردين',               'Purchasing','Suppliers',  150),
(gen_random_uuid(),'Purchasing.Suppliers.Create',  'إضافة مورد',                 'Purchasing','Suppliers',  151),
(gen_random_uuid(),'Purchasing.Suppliers.Edit',    'تعديل مورد',                 'Purchasing','Suppliers',  152),
(gen_random_uuid(),'Purchasing.Orders.View',       'عرض أوامر الشراء',           'Purchasing','Orders',     153),
(gen_random_uuid(),'Purchasing.Orders.Create',     'إنشاء أمر شراء',             'Purchasing','Orders',     154),
(gen_random_uuid(),'Purchasing.Orders.Edit',       'تعديل أمر شراء',             'Purchasing','Orders',     155),
(gen_random_uuid(),'Purchasing.Orders.Delete',     'حذف أمر شراء',               'Purchasing','Orders',     156),
(gen_random_uuid(),'Purchasing.Orders.Approve',    'اعتماد أمر شراء',            'Purchasing','Orders',     157),
(gen_random_uuid(),'Purchasing.Orders.Print',      'طباعة أمر شراء',             'Purchasing','Orders',     158),
(gen_random_uuid(),'Purchasing.Invoices.View',     'عرض فواتير الشراء',          'Purchasing','Invoices',   159),
(gen_random_uuid(),'Purchasing.Invoices.Create',   'إنشاء فاتورة شراء',          'Purchasing','Invoices',   160),
(gen_random_uuid(),'Purchasing.Invoices.Edit',     'تعديل فاتورة شراء',          'Purchasing','Invoices',   161),
(gen_random_uuid(),'Purchasing.Invoices.Delete',   'حذف فاتورة شراء',            'Purchasing','Invoices',   162),
(gen_random_uuid(),'Purchasing.Invoices.Print',    'طباعة فاتورة شراء',          'Purchasing','Invoices',   163),
(gen_random_uuid(),'Purchasing.Invoices.ExportPdf','تصدير فاتورة شراء PDF',      'Purchasing','Invoices',   164),
(gen_random_uuid(),'Purchasing.Returns.View',      'عرض مرتجعات الشراء',         'Purchasing','Returns',    165),
(gen_random_uuid(),'Purchasing.Returns.Create',    'إنشاء مرتجع شراء',           'Purchasing','Returns',    166),

-- ── Assets ─────────────────────────────────────────────────────
(gen_random_uuid(),'Assets.Assets.View',         'عرض الأصول الثابتة',          'Assets','Assets',       170),
(gen_random_uuid(),'Assets.Assets.Create',       'إضافة أصل ثابت',              'Assets','Assets',       171),
(gen_random_uuid(),'Assets.Assets.Edit',         'تعديل أصل ثابت',              'Assets','Assets',       172),
(gen_random_uuid(),'Assets.Assets.Delete',       'حذف أصل ثابت',                'Assets','Assets',       173),
(gen_random_uuid(),'Assets.Assets.Print',        'طباعة سجل الأصول',            'Assets','Assets',       174),
(gen_random_uuid(),'Assets.Maintenance.View',    'عرض سجل الصيانة',             'Assets','Maintenance',  175),
(gen_random_uuid(),'Assets.Maintenance.Create',  'تسجيل صيانة',                 'Assets','Maintenance',  176),
(gen_random_uuid(),'Assets.Categories.View',     'عرض فئات الأصول',             'Assets','Categories',   177),
(gen_random_uuid(),'Assets.Categories.Create',   'إضافة فئة أصول',              'Assets','Categories',   178),
(gen_random_uuid(),'Assets.Categories.Edit',     'تعديل فئة أصول',              'Assets','Categories',   179),

-- ── Reports ────────────────────────────────────────────────────
(gen_random_uuid(),'Reports.Financial.View',        'التقارير المالية - عرض',    'Reports','Financial',   180),
(gen_random_uuid(),'Reports.Financial.Print',       'التقارير المالية - طباعة',  'Reports','Financial',   181),
(gen_random_uuid(),'Reports.Financial.ExportPdf',   'التقارير المالية - PDF',    'Reports','Financial',   182),
(gen_random_uuid(),'Reports.Financial.ExportExcel', 'التقارير المالية - Excel',  'Reports','Financial',   183),
(gen_random_uuid(),'Reports.Operational.View',      'التقارير التشغيلية - عرض',  'Reports','Operational', 184),
(gen_random_uuid(),'Reports.Operational.Print',     'التقارير التشغيلية - طباعة','Reports','Operational', 185),
(gen_random_uuid(),'Reports.Operational.ExportPdf', 'التقارير التشغيلية - PDF',  'Reports','Operational', 186),
(gen_random_uuid(),'Reports.Operational.ExportExcel','التقارير التشغيلية - Excel','Reports','Operational', 187),
(gen_random_uuid(),'Reports.Purchasing.View',       'تقارير المشتريات - عرض',    'Reports','Purchasing',  188),
(gen_random_uuid(),'Reports.Purchasing.Print',      'تقارير المشتريات - طباعة',  'Reports','Purchasing',  189),
(gen_random_uuid(),'Reports.Purchasing.ExportPdf',  'تقارير المشتريات - PDF',    'Reports','Purchasing',  190),
(gen_random_uuid(),'Reports.Inventory.View',        'تقارير المخزون - عرض',      'Reports','Inventory',   191),
(gen_random_uuid(),'Reports.Inventory.Print',       'تقارير المخزون - طباعة',    'Reports','Inventory',   192),
(gen_random_uuid(),'Reports.ARaging.View',          'تقرير تقادم الذمم - عرض',   'Reports','ARaging',     193),
(gen_random_uuid(),'Reports.ARaging.ExportPdf',     'تقرير تقادم الذمم - PDF',   'Reports','ARaging',     194),
(gen_random_uuid(),'Reports.Collections.View',      'تقرير التحصيلات - عرض',     'Reports','Collections', 195),
(gen_random_uuid(),'Reports.Collections.ExportPdf', 'تقرير التحصيلات - PDF',     'Reports','Collections', 196),

-- ── IAM (Settings / Admin) ─────────────────────────────────────
(gen_random_uuid(),'IAM.Users.View',       'عرض المستخدمين',                    'IAM','Users',     200),
(gen_random_uuid(),'IAM.Users.Create',     'إنشاء مستخدم',                      'IAM','Users',     201),
(gen_random_uuid(),'IAM.Users.Edit',       'تعديل مستخدم',                      'IAM','Users',     202),
(gen_random_uuid(),'IAM.Users.Delete',     'حذف مستخدم',                        'IAM','Users',     203),
(gen_random_uuid(),'IAM.Roles.View',       'عرض الأدوار والصلاحيات',            'IAM','Roles',     204),
(gen_random_uuid(),'IAM.Roles.Create',     'إنشاء دور',                         'IAM','Roles',     205),
(gen_random_uuid(),'IAM.Roles.Edit',       'تعديل دور',                         'IAM','Roles',     206),
(gen_random_uuid(),'IAM.Roles.Delete',     'حذف دور',                           'IAM','Roles',     207),
(gen_random_uuid(),'IAM.Settings.View',    'عرض إعدادات النظام',                'IAM','Settings',  208),
(gen_random_uuid(),'IAM.Settings.Edit',    'تعديل إعدادات النظام',              'IAM','Settings',  209),
(gen_random_uuid(),'IAM.Services.View',    'عرض الخدمات',                       'IAM','Services',  210),
(gen_random_uuid(),'IAM.Services.Create',  'إضافة خدمة',                        'IAM','Services',  211),
(gen_random_uuid(),'IAM.Services.Edit',    'تعديل خدمة',                        'IAM','Services',  212),
(gen_random_uuid(),'IAM.Services.Delete',  'حذف خدمة',                          'IAM','Services',  213),
(gen_random_uuid(),'IAM.Vaults.View',      'عرض إعداد الخزائن',                 'IAM','Vaults',    214),
(gen_random_uuid(),'IAM.Vaults.Create',    'إضافة خزينة',                       'IAM','Vaults',    215),
(gen_random_uuid(),'IAM.Vaults.Edit',      'تعديل خزينة',                       'IAM','Vaults',    216),
(gen_random_uuid(),'IAM.Doctors.View',     'عرض إعداد الأطباء',                 'IAM','Doctors',   217),
(gen_random_uuid(),'IAM.Doctors.Create',   'إضافة طبيب',                        'IAM','Doctors',   218),
(gen_random_uuid(),'IAM.Doctors.Edit',     'تعديل طبيب',                        'IAM','Doctors',   219),
(gen_random_uuid(),'IAM.Insurance.View',   'عرض إعداد التأمين',                 'IAM','Insurance', 220),
(gen_random_uuid(),'IAM.Insurance.Create', 'إضافة شركة تأمين (إعدادات)',        'IAM','Insurance', 221)
ON CONFLICT (name) DO NOTHING;

-- ── 4. Role Templates ─────────────────────────────────────────────

-- Role UUIDs (deterministic for reference)
INSERT INTO roles (id, name, description, is_system) VALUES
('00000000-0000-0000-0001-000000000001', 'System Administrator',  'مدير النظام — كامل الصلاحيات',                TRUE),
('00000000-0000-0000-0001-000000000002', 'Clinic Manager',        'مدير العيادة',                                 TRUE),
('00000000-0000-0000-0001-000000000003', 'Receptionist',          'موظف الاستقبال',                               TRUE),
('00000000-0000-0000-0001-000000000004', 'Doctor',                'طبيب',                                         TRUE),
('00000000-0000-0000-0001-000000000005', 'Nurse',                 'ممرض/ة',                                       TRUE),
('00000000-0000-0000-0001-000000000006', 'Cashier',               'أمين الصندوق',                                 TRUE),
('00000000-0000-0000-0001-000000000007', 'Insurance Officer',     'مسؤول التأمين',                                TRUE),
('00000000-0000-0000-0001-000000000008', 'Inventory Officer',     'مسؤول المخزون',                                TRUE),
('00000000-0000-0000-0001-000000000009', 'Purchasing Officer',    'مسؤول المشتريات',                              TRUE),
('00000000-0000-0000-0001-000000000010', 'Laboratory Technician', 'فني مختبر',                                    TRUE),
('00000000-0000-0000-0001-000000000011', 'Radiology Technician',  'فني أشعة',                                     TRUE),
('00000000-0000-0000-0001-000000000012', 'Auditor',               'مدقق — عرض وتصدير فقط',                       TRUE),
('00000000-0000-0000-0001-000000000013', 'Read Only',             'مستخدم للقراءة فقط',                           TRUE)
ON CONFLICT (name) DO NOTHING;

-- ── System Administrator: ALL permissions ─────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000001', id FROM permissions
ON CONFLICT DO NOTHING;

-- ── Clinic Manager ────────────────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000002', id FROM permissions
WHERE name IN (
  -- Dashboard: all
  'Dashboard.Overview.View','Dashboard.Revenue.View','Dashboard.Operations.View',
  'Dashboard.Financial.View','Dashboard.Inventory.View','Dashboard.Executive.View',
  -- Patients: full
  'Patients.Patients.View','Patients.Patients.Create','Patients.Patients.Edit',
  'Patients.Patients.Delete','Patients.Patients.Print','Patients.Patients.ExportExcel','Patients.Patients.ExportPdf',
  -- Appointments: full
  'Appointments.Appointments.View','Appointments.Appointments.Create','Appointments.Appointments.Edit',
  'Appointments.Appointments.Delete','Appointments.Appointments.Reschedule','Appointments.Appointments.Cancel','Appointments.Appointments.Print',
  'Appointments.Queue.View','Appointments.Queue.Create','Appointments.Queue.Edit','Appointments.Queue.Delete',
  -- Clinical: full
  'Clinical.Workspace.View','Clinical.TreatmentPlans.View','Clinical.TreatmentPlans.Create',
  'Clinical.TreatmentPlans.Edit','Clinical.TreatmentPlans.Delete','Clinical.TreatmentPlans.Approve','Clinical.TreatmentPlans.Print',
  'Clinical.DentalChart.View','Clinical.DentalChart.Edit','Clinical.Procedures.View','Clinical.Procedures.Create',
  'Clinical.Procedures.Edit','Clinical.Procedures.Delete','Clinical.Files.View','Clinical.Files.Upload','Clinical.Files.Delete',
  -- Lab + Radiology: full
  'Lab.Orders.View','Lab.Orders.Create','Lab.Orders.Edit','Lab.Orders.Delete','Lab.Orders.Print',
  'Lab.Results.View','Lab.Results.Edit','Lab.ExternalLabs.View','Lab.ExternalLabs.Create','Lab.ExternalLabs.Edit',
  'Radiology.Orders.View','Radiology.Orders.Create','Radiology.Orders.Edit','Radiology.Orders.Delete','Radiology.Orders.Print',
  'Radiology.Images.View','Radiology.Images.Upload',
  -- Financial: full
  'Financial.CashierDesk.View','Financial.Invoices.View','Financial.Invoices.Create','Financial.Invoices.Edit',
  'Financial.Invoices.Delete','Financial.Invoices.Print','Financial.Invoices.ExportPdf',
  'Financial.Invoices.Confirm','Financial.Invoices.Cancel','Financial.Invoices.Refund',
  'Financial.Installments.View','Financial.Installments.Create','Financial.Installments.Edit','Financial.Installments.Delete',
  'Financial.Payments.View','Financial.Payments.Create','Financial.Payments.Delete',
  'Financial.Treasury.View','Financial.Treasury.Create','Financial.Treasury.Edit','Financial.Treasury.Transfer',
  'Financial.Expenses.View','Financial.Expenses.Create','Financial.Expenses.Edit','Financial.Expenses.Delete',
  'Financial.Expenses.ExportPdf','Financial.Expenses.ExportExcel',
  'Financial.Doctors.View','Financial.Doctors.Edit',
  -- Insurance: full
  'Insurance.Companies.View','Insurance.Companies.Create','Insurance.Companies.Edit','Insurance.Companies.Delete',
  'Insurance.Claims.View','Insurance.Claims.Create','Insurance.Claims.Edit','Insurance.Claims.Delete',
  'Insurance.Claims.Print','Insurance.Claims.ExportPdf','Insurance.Claims.Approve','Insurance.Claims.Cancel',
  'Insurance.Receivables.View',
  -- Inventory: full
  'Inventory.Items.View','Inventory.Items.Create','Inventory.Items.Edit','Inventory.Items.Delete','Inventory.Items.ExportExcel',
  'Inventory.Movements.View','Inventory.Movements.Create','Inventory.Stocktake.View','Inventory.Stocktake.Create','Inventory.Alerts.View',
  -- Purchasing: full
  'Purchasing.Suppliers.View','Purchasing.Suppliers.Create','Purchasing.Suppliers.Edit',
  'Purchasing.Orders.View','Purchasing.Orders.Create','Purchasing.Orders.Edit','Purchasing.Orders.Delete',
  'Purchasing.Orders.Approve','Purchasing.Orders.Print',
  'Purchasing.Invoices.View','Purchasing.Invoices.Create','Purchasing.Invoices.Edit','Purchasing.Invoices.Delete',
  'Purchasing.Invoices.Print','Purchasing.Invoices.ExportPdf',
  'Purchasing.Returns.View','Purchasing.Returns.Create',
  -- Assets: full
  'Assets.Assets.View','Assets.Assets.Create','Assets.Assets.Edit','Assets.Assets.Delete','Assets.Assets.Print',
  'Assets.Maintenance.View','Assets.Maintenance.Create','Assets.Categories.View','Assets.Categories.Create','Assets.Categories.Edit',
  -- Reports: full
  'Reports.Financial.View','Reports.Financial.Print','Reports.Financial.ExportPdf','Reports.Financial.ExportExcel',
  'Reports.Operational.View','Reports.Operational.Print','Reports.Operational.ExportPdf','Reports.Operational.ExportExcel',
  'Reports.Purchasing.View','Reports.Purchasing.Print','Reports.Purchasing.ExportPdf',
  'Reports.Inventory.View','Reports.Inventory.Print',
  'Reports.ARaging.View','Reports.ARaging.ExportPdf','Reports.Collections.View','Reports.Collections.ExportPdf',
  -- IAM: view users/roles + settings
  'IAM.Users.View','IAM.Users.Create','IAM.Users.Edit',
  'IAM.Roles.View','IAM.Settings.View','IAM.Settings.Edit',
  'IAM.Services.View','IAM.Services.Create','IAM.Services.Edit',
  'IAM.Vaults.View','IAM.Vaults.Create','IAM.Vaults.Edit',
  'IAM.Doctors.View','IAM.Doctors.Create','IAM.Doctors.Edit',
  'IAM.Insurance.View','IAM.Insurance.Create'
) ON CONFLICT DO NOTHING;

-- ── Receptionist ──────────────────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000003', id FROM permissions
WHERE name IN (
  'Dashboard.Overview.View','Dashboard.Operations.View',
  'Patients.Patients.View','Patients.Patients.Create','Patients.Patients.Edit','Patients.Patients.Print',
  'Appointments.Appointments.View','Appointments.Appointments.Create','Appointments.Appointments.Edit',
  'Appointments.Appointments.Delete','Appointments.Appointments.Reschedule','Appointments.Appointments.Cancel',
  'Appointments.Appointments.Print',
  'Appointments.Queue.View','Appointments.Queue.Create','Appointments.Queue.Edit','Appointments.Queue.Delete',
  'Financial.Invoices.View','Financial.CashierDesk.View'
) ON CONFLICT DO NOTHING;

-- ── Doctor ────────────────────────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000004', id FROM permissions
WHERE name IN (
  'Dashboard.Overview.View','Dashboard.Operations.View',
  'Patients.Patients.View','Patients.Patients.Edit','Patients.Patients.Print',
  'Appointments.Appointments.View','Appointments.Appointments.Edit',
  'Appointments.Queue.View',
  'Clinical.Workspace.View',
  'Clinical.TreatmentPlans.View','Clinical.TreatmentPlans.Create','Clinical.TreatmentPlans.Edit',
  'Clinical.TreatmentPlans.Delete','Clinical.TreatmentPlans.Approve','Clinical.TreatmentPlans.Print',
  'Clinical.DentalChart.View','Clinical.DentalChart.Edit',
  'Clinical.Procedures.View','Clinical.Procedures.Create','Clinical.Procedures.Edit','Clinical.Procedures.Delete',
  'Clinical.Files.View','Clinical.Files.Upload',
  'Lab.Orders.View','Lab.Orders.Create','Lab.Orders.Edit','Lab.Orders.Print',
  'Lab.Results.View',
  'Radiology.Orders.View','Radiology.Orders.Create','Radiology.Orders.Edit','Radiology.Orders.Print',
  'Radiology.Images.View',
  'Financial.Invoices.View'
) ON CONFLICT DO NOTHING;

-- ── Nurse ─────────────────────────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000005', id FROM permissions
WHERE name IN (
  'Dashboard.Overview.View','Dashboard.Operations.View',
  'Patients.Patients.View',
  'Appointments.Appointments.View','Appointments.Queue.View',
  'Clinical.Workspace.View','Clinical.TreatmentPlans.View','Clinical.DentalChart.View',
  'Clinical.Procedures.View','Clinical.Files.View','Clinical.Files.Upload',
  'Lab.Orders.View','Lab.Results.View',
  'Radiology.Orders.View','Radiology.Images.View'
) ON CONFLICT DO NOTHING;

-- ── Cashier ───────────────────────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000006', id FROM permissions
WHERE name IN (
  'Dashboard.Overview.View','Dashboard.Financial.View',
  'Patients.Patients.View',
  'Financial.CashierDesk.View',
  'Financial.Invoices.View','Financial.Invoices.Create','Financial.Invoices.Edit',
  'Financial.Invoices.Print','Financial.Invoices.ExportPdf','Financial.Invoices.Confirm','Financial.Invoices.Cancel',
  'Financial.Installments.View','Financial.Installments.Create','Financial.Installments.Edit',
  'Financial.Payments.View','Financial.Payments.Create','Financial.Payments.Delete',
  'Financial.Treasury.View','Financial.Treasury.Transfer',
  'Financial.Expenses.View','Financial.Expenses.Create',
  'Financial.Doctors.View',
  'Insurance.Claims.View','Insurance.Receivables.View',
  'Reports.Financial.View','Reports.Financial.Print','Reports.Financial.ExportPdf',
  'Reports.Collections.View','Reports.Collections.ExportPdf',
  'Reports.ARaging.View','Reports.ARaging.ExportPdf'
) ON CONFLICT DO NOTHING;

-- ── Insurance Officer ─────────────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000007', id FROM permissions
WHERE name IN (
  'Dashboard.Overview.View',
  'Patients.Patients.View',
  'Financial.Invoices.View','Financial.Invoices.Print',
  'Insurance.Companies.View','Insurance.Companies.Create','Insurance.Companies.Edit',
  'Insurance.Claims.View','Insurance.Claims.Create','Insurance.Claims.Edit',
  'Insurance.Claims.Delete','Insurance.Claims.Print','Insurance.Claims.ExportPdf',
  'Insurance.Claims.Approve','Insurance.Claims.Cancel',
  'Insurance.Receivables.View',
  'Reports.Financial.View','Reports.Financial.ExportPdf'
) ON CONFLICT DO NOTHING;

-- ── Inventory Officer ─────────────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000008', id FROM permissions
WHERE name IN (
  'Dashboard.Overview.View','Dashboard.Inventory.View',
  'Inventory.Items.View','Inventory.Items.Create','Inventory.Items.Edit','Inventory.Items.Delete','Inventory.Items.ExportExcel',
  'Inventory.Movements.View','Inventory.Movements.Create',
  'Inventory.Stocktake.View','Inventory.Stocktake.Create','Inventory.Alerts.View',
  'Purchasing.Suppliers.View','Purchasing.Invoices.View',
  'Reports.Inventory.View','Reports.Inventory.Print'
) ON CONFLICT DO NOTHING;

-- ── Purchasing Officer ────────────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000009', id FROM permissions
WHERE name IN (
  'Dashboard.Overview.View','Dashboard.Inventory.View',
  'Inventory.Items.View','Inventory.Alerts.View',
  'Purchasing.Suppliers.View','Purchasing.Suppliers.Create','Purchasing.Suppliers.Edit',
  'Purchasing.Orders.View','Purchasing.Orders.Create','Purchasing.Orders.Edit',
  'Purchasing.Orders.Delete','Purchasing.Orders.Approve','Purchasing.Orders.Print',
  'Purchasing.Invoices.View','Purchasing.Invoices.Create','Purchasing.Invoices.Edit',
  'Purchasing.Invoices.Delete','Purchasing.Invoices.Print','Purchasing.Invoices.ExportPdf',
  'Purchasing.Returns.View','Purchasing.Returns.Create',
  'Reports.Purchasing.View','Reports.Purchasing.Print','Reports.Purchasing.ExportPdf'
) ON CONFLICT DO NOTHING;

-- ── Laboratory Technician ─────────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000010', id FROM permissions
WHERE name IN (
  'Dashboard.Overview.View',
  'Patients.Patients.View',
  'Lab.Orders.View','Lab.Orders.Edit','Lab.Orders.Print',
  'Lab.Results.View','Lab.Results.Edit',
  'Lab.ExternalLabs.View','Lab.ExternalLabs.Create','Lab.ExternalLabs.Edit'
) ON CONFLICT DO NOTHING;

-- ── Radiology Technician ──────────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000011', id FROM permissions
WHERE name IN (
  'Dashboard.Overview.View',
  'Patients.Patients.View',
  'Radiology.Orders.View','Radiology.Orders.Edit','Radiology.Orders.Print',
  'Radiology.Images.View','Radiology.Images.Upload'
) ON CONFLICT DO NOTHING;

-- ── Auditor: all View + Print + Export ────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000012', id FROM permissions
WHERE name LIKE '%.View' OR name LIKE '%.Print'
   OR name LIKE '%.ExportPdf' OR name LIKE '%.ExportExcel'
ON CONFLICT DO NOTHING;

-- ── Read Only: core views only ────────────────────────────────────
INSERT INTO role_permissions (role_id, permission_id)
SELECT '00000000-0000-0000-0001-000000000013', id FROM permissions
WHERE name IN (
  'Dashboard.Overview.View',
  'Patients.Patients.View',
  'Appointments.Appointments.View','Appointments.Queue.View',
  'Clinical.TreatmentPlans.View','Clinical.DentalChart.View','Clinical.Procedures.View',
  'Lab.Orders.View','Radiology.Orders.View',
  'Financial.Invoices.View','Financial.Treasury.View',
  'Reports.Financial.View'
) ON CONFLICT DO NOTHING;

-- ── 5. Migrate admin user to new System Administrator role ────────
-- Remove old role assignments for admin user
DELETE FROM user_roles WHERE user_id = '00000000-0000-0000-0000-000000000001';

-- Assign to new System Administrator role
INSERT INTO user_roles (user_id, role_id)
VALUES ('00000000-0000-0000-0000-000000000001', '00000000-0000-0000-0001-000000000001')
ON CONFLICT DO NOTHING;

COMMIT;
