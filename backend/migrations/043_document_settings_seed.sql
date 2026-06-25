-- Migration 043: Enterprise Document System — company branding settings
BEGIN;

-- Expand value column to hold longer text (terms & conditions, etc.)
ALTER TABLE system_settings ALTER COLUMN value TYPE VARCHAR(5000);

-- Insert all company branding settings used by the PDF document engine
INSERT INTO system_settings (key, value, description, "group") VALUES
('company.nameAr',                 'عيادة الأسنان',
    'اسم الشركة بالعربية',                                  'company'),
('company.nameEn',                 'Dental Clinic',
    'Company Name in English',                               'company'),
('company.logoUrl',                '',
    'رابط شعار الشركة (URL)',                               'company'),
('company.businessType',           'عيادة أسنان',
    'نوع النشاط التجاري',                                   'company'),
('company.address',                '',
    'العنوان التفصيلي',                                     'company'),
('company.city',                   'طرابلس',
    'المدينة',                                              'company'),
('company.country',                'ليبيا',
    'الدولة',                                               'company'),
('company.phone',                  '',
    'رقم الهاتف',                                           'company'),
('company.mobile',                 '',
    'رقم الجوال',                                           'company'),
('company.email',                  '',
    'البريد الإلكتروني',                                    'company'),
('company.website',                '',
    'الموقع الإلكتروني',                                    'company'),
('company.taxNumber',              '',
    'الرقم الضريبي',                                        'company'),
('company.commercialRegistration', '',
    'رقم السجل التجاري',                                    'company'),
('company.licenseNumber',          '',
    'رقم الترخيص',                                          'company'),
('company.currency',               'LYD',
    'كود العملة الدولي',                                    'company'),
('company.currencySymbol',         'د.ل',
    'رمز العملة',                                           'company'),
('company.footerNotes',            '',
    'ملاحظات تظهر في تذييل المستندات',                     'company'),
('company.termsAndConditions',     'تم استلام البضاعة بحالة جيدة وسليمة.' || chr(10) ||
    'يجب الرجوع إلى الإدارة خلال 3 أيام عمل في حالة وجود أي ملاحظات أو مغايرات.' || chr(10) ||
    'لا تُقبل المرتجعات إلا وفق سياسة الشركة المعتمدة.' || chr(10) ||
    'جميع المبالغ بالدينار الليبي ما لم يُذكر خلاف ذلك.',
    'الشروط والأحكام الافتراضية للمستندات',                'company'),
('company.bankName',               '',
    'اسم البنك',                                            'company'),
('company.bankAccount',            '',
    'رقم الحساب البنكي',                                   'company'),
('company.iban',                   '',
    'رقم الآيبان IBAN',                                    'company')
ON CONFLICT (key) DO NOTHING;

-- Sync nameAr from existing clinic.name if clinic.name was customized
UPDATE system_settings
SET value = (SELECT value FROM system_settings s2 WHERE s2.key = 'clinic.name')
WHERE key = 'company.nameAr'
  AND value = 'عيادة الأسنان'
  AND EXISTS (
    SELECT 1 FROM system_settings s3
    WHERE s3.key = 'clinic.name'
      AND s3.value <> ''
      AND s3.value <> 'عيادة الأسنان'
  );

-- Fix old SAR currency to LYD
UPDATE system_settings SET value = 'LYD' WHERE key = 'clinic.currency' AND value = 'SAR';

COMMIT;
