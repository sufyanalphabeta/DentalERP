-- DentalERP DEV Seed Data
-- Phase 6 — All Modules
-- Apply AFTER all 27 migrations
-- Migration 002 already seeds: admin user, 4 roles, 40 permissions
-- Password for ALL seed users: Admin@123

BEGIN;

-- -------------------------------------------------------
-- Additional DEV Users (Receptionist, Doctor, Accountant)
-- Password: Admin@123
-- Hash: $2a$11$K7Q3FnLpqyJm8/.1KGVBQeWzOE6y1DZH.o/PbGZ0VPtFkQ9UoMT0a
-- Role IDs from migration 002:
--   Administrator = 00000000-0000-0000-0000-000000000001
--   Receptionist  = 00000000-0000-0000-0000-000000000002
--   Doctor        = 00000000-0000-0000-0000-000000000003
--   Accountant    = 00000000-0000-0000-0000-000000000004
-- -------------------------------------------------------
INSERT INTO users (id, username, password_hash, full_name, email, is_active)
VALUES
    ('00000000-0000-0000-0000-000000000002', 'reception',
     '$2a$11$K7Q3FnLpqyJm8/.1KGVBQeWzOE6y1DZH.o/PbGZ0VPtFkQ9UoMT0a',
     'موظف الاستقبال', 'reception@dentalerp.local', TRUE),
    ('00000000-0000-0000-0000-000000000003', 'doctor',
     '$2a$11$K7Q3FnLpqyJm8/.1KGVBQeWzOE6y1DZH.o/PbGZ0VPtFkQ9UoMT0a',
     'الدكتور أحمد', 'doctor@dentalerp.local', TRUE),
    ('00000000-0000-0000-0000-000000000004', 'accountant',
     '$2a$11$K7Q3FnLpqyJm8/.1KGVBQeWzOE6y1DZH.o/PbGZ0VPtFkQ9UoMT0a',
     'المحاسب محمد', 'accountant@dentalerp.local', TRUE)
ON CONFLICT (username) DO NOTHING;

INSERT INTO user_roles (user_id, role_id) VALUES
    ('00000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000002'),
    ('00000000-0000-0000-0000-000000000003', '00000000-0000-0000-0000-000000000003'),
    ('00000000-0000-0000-0000-000000000004', '00000000-0000-0000-0000-000000000004')
ON CONFLICT DO NOTHING;

-- -------------------------------------------------------
-- Vault
-- Schema: id, name, type (cash|bank|card|pos), opening_balance, is_active, created_at
-- -------------------------------------------------------
INSERT INTO vaults (id, name, type, opening_balance, is_active, created_at)
VALUES (
    'b0000000-0000-0000-0000-000000000001',
    'الخزينة الرئيسية',
    'cash',
    50000.00,
    true,
    NOW()
) ON CONFLICT (id) DO NOTHING;

-- -------------------------------------------------------
-- Expense Categories
-- -------------------------------------------------------
INSERT INTO expense_categories (id, name, name_ar, description, is_active, created_at)
VALUES
    ('c1000000-0000-0000-0000-000000000001', 'Utilities',       'المرافق',       'Electricity, water, internet', true, NOW()),
    ('c1000000-0000-0000-0000-000000000002', 'Supplies',        'المستلزمات',    'Office and clinical supplies',  true, NOW()),
    ('c1000000-0000-0000-0000-000000000003', 'Maintenance',     'الصيانة',       'Equipment maintenance',         true, NOW()),
    ('c1000000-0000-0000-0000-000000000004', 'Rent',            'الإيجار',       'Clinic and office rent',        true, NOW()),
    ('c1000000-0000-0000-0000-000000000005', 'Salaries',        'الرواتب',       'Staff salaries and bonuses',    true, NOW()),
    ('c1000000-0000-0000-0000-000000000006', 'Marketing',       'التسويق',       'Advertising and promotions',    true, NOW()),
    ('c1000000-0000-0000-0000-000000000007', 'Training',        'التدريب',       'Staff training and courses',    true, NOW()),
    ('c1000000-0000-0000-0000-000000000008', 'Laboratory Fees', 'رسوم المختبر',  'External lab fees',             true, NOW())
ON CONFLICT (id) DO NOTHING;

-- -------------------------------------------------------
-- Asset Categories
-- -------------------------------------------------------
INSERT INTO asset_categories (id, name, name_ar, description, is_active, created_at)
VALUES
    ('d1000000-0000-0000-0000-000000000001', 'Dental Equipment', 'معدات الأسنان',  'Dental chairs, units', true, NOW()),
    ('d1000000-0000-0000-0000-000000000002', 'IT Equipment',     'معدات تقنية',    'Computers, servers',   true, NOW()),
    ('d1000000-0000-0000-0000-000000000003', 'Furniture',        'الأثاث',         'Desks, chairs',        true, NOW()),
    ('d1000000-0000-0000-0000-000000000004', 'X-Ray Equipment',  'معدات الأشعة',   'Digital X-ray, CBCT',  true, NOW()),
    ('d1000000-0000-0000-0000-000000000005', 'Sterilization',    'التعقيم',        'Autoclaves',           true, NOW()),
    ('d1000000-0000-0000-0000-000000000006', 'Vehicles',         'المركبات',       'Clinic vehicles',      true, NOW())
ON CONFLICT (id) DO NOTHING;

-- -------------------------------------------------------
-- Suppliers
-- Schema: id, supplier_code, name, name_ar, category, contact_person, phone, email, is_active, created_at
-- category CHECK: Medical | Equipment | General | Lab | Radiology | Pharma
-- -------------------------------------------------------
INSERT INTO suppliers (id, supplier_code, name, name_ar, category, contact_person, phone, email, is_active, created_at)
VALUES
    ('e1000000-0000-0000-0000-000000000001', 'SUP-001', 'DentalSupply Co.',    'شركة دينتال سبلاي', 'Equipment', 'Ahmed Al-Rashidi', '+966501234567', 'ahmed@dentalsupply.com', true, NOW()),
    ('e1000000-0000-0000-0000-000000000002', 'SUP-002', 'MedEquip Arabia',     'ميد إكويب العربية', 'Medical',   'Sara Al-Khalid',   '+966502345678', 'sara@medequip.sa',       true, NOW()),
    ('e1000000-0000-0000-0000-000000000003', 'SUP-003', 'PharmaDent Ltd.',     'فارما دينت',         'Pharma',    'Khalid Hassan',    '+966503456789', 'info@pharmadent.com',    true, NOW()),
    ('e1000000-0000-0000-0000-000000000004', 'SUP-004', 'Gulf Dental Imports', 'الخليج للاستيراد',   'General',   'Fatima Al-Zahra',  '+966504567890', 'orders@gulfdental.com',  true, NOW())
ON CONFLICT (id) DO NOTHING;

-- -------------------------------------------------------
-- Item Categories
-- Schema: id, name, name_ar, is_active, created_at
-- -------------------------------------------------------
INSERT INTO item_categories (id, name, name_ar, is_active, created_at)
VALUES
    ('f1000000-0000-0000-0000-000000000001', 'Dental Materials',   'مواد طب الأسنان',   true, NOW()),
    ('f1000000-0000-0000-0000-000000000002', 'Disposables',        'المواد المستهلكة',   true, NOW()),
    ('f1000000-0000-0000-0000-000000000003', 'Anesthetics',        'المخدرات',           true, NOW()),
    ('f1000000-0000-0000-0000-000000000004', 'Lab Supplies',       'مستلزمات المختبر',   true, NOW()),
    ('f1000000-0000-0000-0000-000000000005', 'Radiology Supplies', 'مستلزمات الأشعة',    true, NOW())
ON CONFLICT (id) DO NOTHING;

-- -------------------------------------------------------
-- Items
-- Schema: id, item_code, name, name_ar, category_id, unit_cost, reorder_level, reorder_quantity,
--         allow_negative_stock, is_active, created_at
-- -------------------------------------------------------
INSERT INTO items (id, item_code, name, name_ar, category_id, unit_cost, reorder_level, reorder_quantity, allow_negative_stock, is_active, created_at)
VALUES
    ('a1000000-0000-0000-0000-000000000001', 'ITM-001', 'Composite Resin A2',   'راتنج مركب A2', 'f1000000-0000-0000-0000-000000000001', 45.00, 10, 50,  false, true, NOW()),
    ('a1000000-0000-0000-0000-000000000002', 'ITM-002', 'Examination Gloves L', 'قفازات فحص L',  'f1000000-0000-0000-0000-000000000002', 12.50, 20, 100, false, true, NOW()),
    ('a1000000-0000-0000-0000-000000000003', 'ITM-003', 'Lidocaine 2% 1.8ml',   'ليدوكايين 2%',  'f1000000-0000-0000-0000-000000000003', 8.75,  30, 200, false, true, NOW()),
    ('a1000000-0000-0000-0000-000000000004', 'ITM-004', 'Alginate Impression',  'مادة الطباعة',  'f1000000-0000-0000-0000-000000000004', 22.00, 5,  20,  false, true, NOW()),
    ('a1000000-0000-0000-0000-000000000005', 'ITM-005', 'X-Ray Film Size 2',    'فيلم أشعة 2',   'f1000000-0000-0000-0000-000000000005', 3.50,  50, 200, false, true, NOW())
ON CONFLICT (id) DO NOTHING;

-- -------------------------------------------------------
-- Assets
-- -------------------------------------------------------
INSERT INTO assets (id, asset_tag, name, category_id, cost_center, purchase_date, purchase_cost, location, status, created_at)
VALUES
    ('a2000000-0000-0000-0000-000000000001', 'AST-000001', 'Dental Unit Chair #1',    'd1000000-0000-0000-0000-000000000001', 'CLINIC',         '2024-01-15', 28000.00, 'Clinic Room 1',      'Active',           NOW()),
    ('a2000000-0000-0000-0000-000000000002', 'AST-000002', 'Digital X-Ray Sensor #1', 'd1000000-0000-0000-0000-000000000004', 'RADIOLOGY',      '2024-03-10', 15000.00, 'Radiology Room',     'Active',           NOW()),
    ('a2000000-0000-0000-0000-000000000003', 'AST-000003', 'Autoclave 22L',           'd1000000-0000-0000-0000-000000000005', 'CLINIC',         '2023-11-20', 8500.00,  'Sterilization Room', 'Active',           NOW()),
    ('a2000000-0000-0000-0000-000000000004', 'AST-000004', 'Reception Computer',      'd1000000-0000-0000-0000-000000000002', 'ADMINISTRATION', '2024-06-01', 3200.00,  'Reception',          'Active',           NOW()),
    ('a2000000-0000-0000-0000-000000000005', 'AST-000005', 'Dental Unit Chair #2',    'd1000000-0000-0000-0000-000000000001', 'CLINIC',         '2024-01-15', 28000.00, 'Clinic Room 2',      'UnderMaintenance', NOW())
ON CONFLICT (id) DO NOTHING;

-- -------------------------------------------------------
-- Expenses
-- -------------------------------------------------------
INSERT INTO expenses (id, expense_number, category_id, cost_center, expense_date, amount, description, created_at)
VALUES
    ('a3000000-0000-0000-0000-000000000001', 'EXP-2026-000001', 'c1000000-0000-0000-0000-000000000001', 'CLINIC',         '2026-06-01', 1200.00, 'Monthly electricity bill',   NOW()),
    ('a3000000-0000-0000-0000-000000000002', 'EXP-2026-000002', 'c1000000-0000-0000-0000-000000000004', 'ADMINISTRATION', '2026-06-01', 8500.00, 'Monthly clinic rent',        NOW()),
    ('a3000000-0000-0000-0000-000000000003', 'EXP-2026-000003', 'c1000000-0000-0000-0000-000000000002', 'CLINIC',         '2026-06-05', 340.00,  'Disposables restock',        NOW()),
    ('a3000000-0000-0000-0000-000000000004', 'EXP-2026-000004', 'c1000000-0000-0000-0000-000000000003', 'CLINIC',         '2026-06-10', 500.00,  'Dental unit #2 maintenance', NOW())
ON CONFLICT (id) DO NOTHING;

COMMIT;
