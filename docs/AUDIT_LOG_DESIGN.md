# Audit Log Design — DentalERP

> **التاريخ:** 2026-06-17 | **الحالة:** مُنفَّذ ✅

---

## 1. الجدول — audit_logs

```sql
CREATE TABLE audit_logs (
    id          UUID         PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id     UUID,                          -- NULL للعمليات التلقائية (system)
    username    VARCHAR(100) NOT NULL,
    entity_name VARCHAR(100) NOT NULL,         -- "User" | "Role" | "Patient" | ...
    entity_id   VARCHAR(100) NOT NULL,         -- UUID أو composite key
    action      VARCHAR(50)  NOT NULL
        CHECK (action IN ('Created','Updated','Deleted','Login','Logout','PasswordChanged','Other')),
    old_values  JSONB,                         -- قيم ما قبل التعديل
    new_values  JSONB,                         -- قيم ما بعد التعديل
    ip_address  VARCHAR(45),                   -- IPv4 أو IPv6
    user_agent  VARCHAR(500),
    timestamp   TIMESTAMPTZ  NOT NULL DEFAULT NOW()
);
```

**الجدول immutable — لا UPDATE ولا DELETE مسموح.**

---

## 2. الحقول المطلوبة

| الحقل | النوع | المطلوب | الوصف |
|-------|-------|---------|-------|
| `id` | UUID | ✅ | مفتاح رئيسي |
| `user_id` | UUID | ❌ (nullable) | معرّف المستخدم المنفِّذ |
| `username` | VARCHAR(100) | ✅ | اسم المستخدم أو "system" |
| `entity_name` | VARCHAR(100) | ✅ | اسم الكيان (User, Role, Patient...) |
| `entity_id` | VARCHAR(100) | ✅ | معرّف السجل المتأثر |
| `action` | VARCHAR(50) | ✅ | نوع العملية |
| `old_values` | JSONB | ❌ | القيم قبل التعديل (للـ Updated فقط) |
| `new_values` | JSONB | ❌ | القيم بعد التعديل (للـ Created/Updated) |
| `ip_address` | VARCHAR(45) | ❌ | عنوان IP المصدر |
| `user_agent` | VARCHAR(500) | ❌ | متصفح/تطبيق المصدر |
| `timestamp` | TIMESTAMPTZ | ✅ | وقت العملية (UTC) |

---

## 3. أنواع العمليات (Actions)

| Action | متى يُسجَّل |
|--------|------------|
| `Created` | عند إنشاء أي كيان جديد |
| `Updated` | عند تعديل أي كيان |
| `Deleted` | عند حذف (soft أو hard) أي كيان |
| `Login` | عند تسجيل دخول ناجح |
| `Logout` | عند تسجيل خروج |
| `PasswordChanged` | عند تغيير كلمة المرور |
| `Other` | عمليات أخرى (مثل تصدير تقرير) |

---

## 4. تطبيق الكود

### AuditService
الموقع: [backend/src/DentalERP.Modules.IAM/Infrastructure/Services/AuditService.cs](../backend/src/DentalERP.Modules.IAM/Infrastructure/Services/AuditService.cs)

المهام:
- `GetAuditLogs(entries)` — يحلّل EF Core ChangeTracker ويستخرج تغييرات الكيانات
- `CreateActionLog(action, entity, id)` — يُنشئ سجل يدوي (Login، Logout، إلخ)

### حماية كلمة المرور
```csharp
var current = entry.Properties
    .Where(p => p.Metadata.Name != "PasswordHash")  // ← لا تُسجَّل أبداً
    .ToDictionary(...);
```

### تسجيل Login
```csharp
db.AuditLogs.Add(auditService.CreateActionLog("Login", "User", user.Id.ToString()));
await db.SaveChangesAsync(ct);
```

---

## 5. الـ Indexes

| Index | الهدف |
|-------|-------|
| `ix_audit_logs_timestamp` | البحث بالتاريخ (DESC للأحدث أولاً) |
| `ix_audit_logs_entity` | عرض تاريخ كيان معيّن |
| `ix_audit_logs_user` | عرض عمليات مستخدم معيّن |

---

## 6. الاستخدام في APIs المستقبلية

```
GET /api/audit-logs?from=&to=&userId=&entityName=&action=
→ يحتاج Permission: Settings.View
→ Phase 8 (Reporting)
```

---

## 7. قواعد البيانات المتوقعة

- نموّ متوقّع: ~500–2000 سجل/يوم في عيادة متوسطة
- سياسة الاحتفاظ: 2 سنة (يمكن تعديلها من system_settings)
- قسيمة (Partition) بالشهر: مُوصى به بعد السنة الأولى

---

**الحالة: ✅ مُنفَّذ في Phase 1 — جاهز للاستخدام من Phase 2**
