# Permissions Matrix — DentalERP

> **الإصدار:** V-Final | **التاريخ:** 2026-06-17 | **المجموع:** 40 صلاحية

---

## 1. جميع الصلاحيات — مرتّبة حسب Module

### IAM — إدارة المستخدمين (10 صلاحيات)

| # | Permission Name | DisplayName | Admin | Receptionist | Doctor | Accountant |
|---|-----------------|-------------|:-----:|:------------:|:------:|:----------:|
| 1 | `Users.View` | عرض المستخدمين | ✅ | ❌ | ❌ | ❌ |
| 2 | `Users.Create` | إنشاء مستخدم | ✅ | ❌ | ❌ | ❌ |
| 3 | `Users.Edit` | تعديل مستخدم | ✅ | ❌ | ❌ | ❌ |
| 4 | `Users.Delete` | حذف مستخدم | ✅ | ❌ | ❌ | ❌ |
| 5 | `Roles.View` | عرض الأدوار | ✅ | ❌ | ❌ | ❌ |
| 6 | `Roles.Create` | إنشاء دور | ✅ | ❌ | ❌ | ❌ |
| 7 | `Roles.Edit` | تعديل دور | ✅ | ❌ | ❌ | ❌ |
| 8 | `Roles.Delete` | حذف دور | ✅ | ❌ | ❌ | ❌ |
| 9 | `Settings.View` | عرض الإعدادات | ✅ | ❌ | ❌ | ❌ |
| 10 | `Settings.Edit` | تعديل الإعدادات | ✅ | ❌ | ❌ | ❌ |

### Patients — المرضى (4 صلاحيات)

| # | Permission Name | DisplayName | Admin | Receptionist | Doctor | Accountant |
|---|-----------------|-------------|:-----:|:------------:|:------:|:----------:|
| 11 | `Patients.View` | عرض المرضى | ✅ | ✅ | ✅ | ✅ |
| 12 | `Patients.Create` | تسجيل مريض | ✅ | ✅ | ✅ | ❌ |
| 13 | `Patients.Edit` | تعديل بيانات مريض | ✅ | ✅ | ✅ | ❌ |
| 14 | `Patients.Delete` | حذف مريض | ✅ | ❌ | ❌ | ❌ |

### Scheduling — المواعيد (4 صلاحيات)

| # | Permission Name | DisplayName | Admin | Receptionist | Doctor | Accountant |
|---|-----------------|-------------|:-----:|:------------:|:------:|:----------:|
| 15 | `Appointments.View` | عرض المواعيد | ✅ | ✅ | ✅ | ❌ |
| 16 | `Appointments.Create` | حجز موعد | ✅ | ✅ | ✅ | ❌ |
| 17 | `Appointments.Edit` | تعديل موعد | ✅ | ✅ | ❌ | ❌ |
| 18 | `Appointments.Delete` | حذف موعد | ✅ | ✅ | ❌ | ❌ |

### Clinical — السريري (4 صلاحيات)

| # | Permission Name | DisplayName | Admin | Receptionist | Doctor | Accountant |
|---|-----------------|-------------|:-----:|:------------:|:------:|:----------:|
| 19 | `Clinical.View` | عرض السجل السريري | ✅ | ❌ | ✅ | ❌ |
| 20 | `Clinical.Create` | إضافة إجراء سريري | ✅ | ❌ | ✅ | ❌ |
| 21 | `Clinical.Edit` | تعديل إجراء سريري | ✅ | ❌ | ✅ | ❌ |
| 22 | `Clinical.Delete` | حذف إجراء سريري | ✅ | ❌ | ❌ | ❌ |

### Treasury — الخزينة (4 صلاحيات)

| # | Permission Name | DisplayName | Admin | Receptionist | Doctor | Accountant |
|---|-----------------|-------------|:-----:|:------------:|:------:|:----------:|
| 23 | `Treasury.View` | عرض الخزينة | ✅ | ❌ | ❌ | ✅ |
| 24 | `Treasury.Create` | إنشاء حركة مالية | ✅ | ❌ | ❌ | ✅ |
| 25 | `Treasury.Edit` | تعديل حركة مالية | ✅ | ❌ | ❌ | ✅ |
| 26 | `Treasury.Delete` | حذف حركة مالية | ✅ | ❌ | ❌ | ❌ |

### Inventory — المخزون (4 صلاحيات)

| # | Permission Name | DisplayName | Admin | Receptionist | Doctor | Accountant |
|---|-----------------|-------------|:-----:|:------------:|:------:|:----------:|
| 27 | `Inventory.View` | عرض المخزون | ✅ | ❌ | ❌ | ✅ |
| 28 | `Inventory.Create` | إضافة صنف | ✅ | ❌ | ❌ | ❌ |
| 29 | `Inventory.Edit` | تعديل صنف | ✅ | ❌ | ❌ | ❌ |
| 30 | `Inventory.Delete` | حذف صنف | ✅ | ❌ | ❌ | ❌ |

### Laboratory — المعمل (4 صلاحيات)

| # | Permission Name | DisplayName | Admin | Receptionist | Doctor | Accountant |
|---|-----------------|-------------|:-----:|:------------:|:------:|:----------:|
| 31 | `Lab.View` | عرض المعمل | ✅ | ❌ | ✅ | ❌ |
| 32 | `Lab.Create` | إنشاء أمر معمل | ✅ | ❌ | ✅ | ❌ |
| 33 | `Lab.Edit` | تعديل أمر معمل | ✅ | ❌ | ❌ | ❌ |
| 34 | `Lab.Manage` | إدارة المعمل | ✅ | ❌ | ❌ | ❌ |

### Radiology — الأشعة (4 صلاحيات)

| # | Permission Name | DisplayName | Admin | Receptionist | Doctor | Accountant |
|---|-----------------|-------------|:-----:|:------------:|:------:|:----------:|
| 35 | `Radiology.View` | عرض الأشعة | ✅ | ❌ | ✅ | ❌ |
| 36 | `Radiology.Create` | إنشاء طلب أشعة | ✅ | ❌ | ✅ | ❌ |
| 37 | `Radiology.Edit` | تعديل طلب أشعة | ✅ | ❌ | ❌ | ❌ |
| 38 | `Radiology.Manage` | إدارة الأشعة | ✅ | ❌ | ❌ | ❌ |

### Reports — التقارير (2 صلاحيات)

| # | Permission Name | DisplayName | Admin | Receptionist | Doctor | Accountant |
|---|-----------------|-------------|:-----:|:------------:|:------:|:----------:|
| 39 | `Reports.View` | عرض التقارير | ✅ | ❌ | ❌ | ✅ |
| 40 | `Reports.Export` | تصدير التقارير | ✅ | ❌ | ❌ | ✅ |

---

## 2. ملخص الأدوار

| الدور | عدد الصلاحيات | is_system |
|-------|--------------|-----------|
| **Administrator** | 40 (جميع) | TRUE |
| **Receptionist** | 7 | FALSE |
| **Doctor** | 12 | FALSE |
| **Accountant** | 8 | FALSE |

### Receptionist (7 صلاحيات)
`Patients.View` · `Patients.Create` · `Patients.Edit` · `Appointments.View` · `Appointments.Create` · `Appointments.Edit` · `Appointments.Delete`

### Doctor (12 صلاحيات)
`Patients.View` · `Patients.Create` · `Patients.Edit` · `Appointments.View` · `Appointments.Create` · `Clinical.View` · `Clinical.Create` · `Clinical.Edit` · `Lab.View` · `Lab.Create` · `Radiology.View` · `Radiology.Create`

### Accountant (8 صلاحيات)
`Patients.View` · `Treasury.View` · `Treasury.Create` · `Treasury.Edit` · `Inventory.View` · `Reports.View` · `Reports.Export`

---

## 3. خريطة الـ Modules

```
Modules (9):
├── IAM          → 10 permissions (Users × 4, Roles × 4, Settings × 2)
├── Patients     →  4 permissions (CRUD)
├── Scheduling   →  4 permissions (CRUD)
├── Clinical     →  4 permissions (CRUD)
├── Treasury     →  4 permissions (CRUD)
├── Inventory    →  4 permissions (CRUD)
├── Laboratory   →  4 permissions (View, Create, Edit, Manage)
├── Radiology    →  4 permissions (View, Create, Edit, Manage)
└── Reports      →  2 permissions (View, Export)
                 ──────
Total            → 40 permissions
```

---

## 4. ملاحظات التطوير

- صلاحيات Lab.Delete وRadiology.Delete **غير موجودة** عمداً — الحذف محظور ولا يسمح به في Module 10/11
- `Lab.Manage` و`Radiology.Manage` تُتيح عمليات الإدارة (التقسيط، إدارة الفنيين، الصرف المالي)
- جميع الصلاحيات يتم تحميلها في JWT token عند Login كـ `"permission"` claims
- الـ `AuthorizationBehavior` يتحقق منها قبل وصول Command للـ Handler
