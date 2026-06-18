# PATIENT TIMELINE DESIGN — DentalERP

> **التاريخ:** 2026-06-17 | **الحالة:** 📋 Design Document

---

## 1. الهدف

Patient Timeline هي أقوى شاشة في النظام — تعرض كامل رحلة المريض من أول تسجيل حتى آخر دفعة في سياق زمني متسلسل. الطبيب والمُستقبِل يرون نفس القصة من زوايا مختلفة.

---

## 2. مكونات الـ Timeline

```
Timeline Event
├── timestamp          (متى حدث)
├── event_type         (نوع الحدث)
├── title              (عنوان مختصر)
├── description        (تفاصيل)
├── actor              (من قام به)
├── linked_entity      (ID + نوع الكيان المرتبط)
└── metadata           (JSONB — بيانات إضافية)
```

---

## 3. أنواع الأحداث (event_type)

| الرمز | الحدث | المصدر | Phase |
|-------|-------|--------|-------|
| `patient.registered` | تسجيل المريض | patients | Phase 2 |
| `patient.updated` | تحديث بيانات المريض | patients | Phase 2 |
| `appointment.scheduled` | حجز موعد | appointments | Phase 2 |
| `appointment.confirmed` | تأكيد الموعد | appointments | Phase 2 |
| `appointment.completed` | إتمام الموعد | appointments | Phase 2 |
| `appointment.cancelled` | إلغاء الموعد | appointments | Phase 2 |
| `appointment.noshow` | عدم الحضور | appointments | Phase 2 |
| `queue.checkin` | تسجيل الحضور | queue_entries | Phase 2 |
| `queue.called` | نداء المريض | queue_entries | Phase 2 |
| `queue.completed` | انتهاء الدور | queue_entries | Phase 2 |
| `chart.updated` | تحديث مخطط الأسنان | dental_chart_entries | Phase 3 |
| `procedure.performed` | تنفيذ إجراء | procedures | Phase 3 |
| `treatment_plan.created` | إنشاء خطة علاج | treatment_plans | Phase 3 |
| `treatment_plan.activated` | تفعيل خطة علاج | treatment_plans | Phase 3 |
| `treatment_plan.completed` | إتمام خطة علاج | treatment_plans | Phase 3 |
| `media.uploaded` | رفع صورة/وثيقة | patient_media | Phase 3 |
| `doctor.assigned` | تعيين طبيب | doctor_assignments | Phase 3 |
| `doctor.transferred` | تحويل لطبيب آخر | doctor_assignments | Phase 3 |
| `invoice.created` | إنشاء فاتورة | invoices | Phase 5 |
| `invoice.paid` | دفع فاتورة | payments | Phase 5 |
| `insurance.claimed` | تقديم مطالبة تأمين | insurance_claims | Phase 5 |
| `insurance.approved` | موافقة التأمين | insurance_claims | Phase 5 |
| `insurance.rejected` | رفض التأمين | insurance_claims | Phase 5 |

---

## 4. تصميم الجدول — `patient_timeline`

```sql
CREATE TABLE patient_timeline (
    id              UUID        PRIMARY KEY DEFAULT gen_random_uuid(),
    patient_id      UUID        NOT NULL REFERENCES patients(id),
    event_type      VARCHAR(50) NOT NULL,
    title           VARCHAR(200) NOT NULL,
    description     TEXT,
    actor_id        UUID        REFERENCES users(id),
    actor_name      VARCHAR(100),               -- مُحفوظ (حماية من حذف المستخدم)
    linked_entity_type VARCHAR(50),             -- 'Appointment' | 'Procedure' | ...
    linked_entity_id   UUID,
    metadata        JSONB,                      -- بيانات إضافية حسب نوع الحدث
    event_at        TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    is_visible_to_doctor   BOOLEAN NOT NULL DEFAULT TRUE,
    is_visible_to_patient  BOOLEAN NOT NULL DEFAULT FALSE,

    -- تصنيف الحدث للفلترة السريعة
    event_category  VARCHAR(20) NOT NULL DEFAULT 'Administrative'
        CHECK (event_category IN (
            'Clinical',        -- إجراءات طبية، مخطط الأسنان
            'Financial',       -- فواتير، دفعات
            'Administrative',  -- مواعيد، قائمة الانتظار، تسجيل
            'Insurance',       -- مطالبات التأمين
            'Radiology',       -- أشعة ووسائط طبية
            'Laboratory'       -- تحاليل مخبرية
        ))
);

CREATE INDEX ix_timeline_patient    ON patient_timeline(patient_id, event_at DESC);
CREATE INDEX ix_timeline_event_type ON patient_timeline(event_type);
CREATE INDEX ix_timeline_category   ON patient_timeline(patient_id, event_category);
CREATE INDEX ix_timeline_entity     ON patient_timeline(linked_entity_type, linked_entity_id)
    WHERE linked_entity_id IS NOT NULL;
```

**الجدول append-only — لا UPDATE ولا DELETE.**

---

## 5. كيف يُملَأ الجدول

### 5.1 تلقائياً (Domain Events)

كل مكوّن يُطلق Domain Event → `TimelineService` يستمع ويُسجّل:

```
AppointmentCompletedEvent
    → patient_timeline INSERT
      { event_type: 'appointment.completed',
        title: 'اكتمل الموعد',
        linked_entity_type: 'Appointment',
        linked_entity_id: appointmentId,
        metadata: { duration_minutes: 45, doctor_name: '...' } }
```

### 5.2 يدوياً (للأحداث المعقدة)

```
ProcedurePerformedCommand
    → ProcedureCreated (في DB)
    → TimelineService.Record('procedure.performed', patient_id, ...)
```

### 5.3 خدمة الـ Timeline

```csharp
// TimelineService.cs
public interface ITimelineService
{
    Task RecordAsync(
        Guid patientId,
        string eventType,
        string title,
        string? description = null,
        Guid? actorId = null,
        string? linkedEntityType = null,
        Guid? linkedEntityId = null,
        object? metadata = null,
        CancellationToken ct = default);
}
```

---

## 6. API

```
GET /api/patients/{id}/timeline
    ?from=2026-01-01
    &to=2026-12-31
    &eventTypes=appointment.completed,procedure.performed
    &page=1
    &pageSize=50
```

**Response:**
```json
{
  "patientId": "...",
  "totalCount": 87,
  "events": [
    {
      "id": "...",
      "eventType": "appointment.completed",
      "title": "اكتمل الموعد",
      "description": "حشو ضرس عقل — 45 دقيقة",
      "actorName": "د. محمد علي",
      "linkedEntityType": "Appointment",
      "linkedEntityId": "...",
      "metadata": { "durationMinutes": 45 },
      "eventAt": "2026-06-15T10:30:00Z"
    }
  ]
}
```

---

## 7. شاشة الـ Timeline (Frontend)

### التصميم المقترح

```
┌────────────────────────────────────────────────────────┐
│  محمد علي — P2026-00001                  [تصفية ▼]   │
├────────────────────────────────────────────────────────┤
│                                                        │
│  ●  يونيو 2026                                        │
│  │                                                    │
│  ├─ 17/06  ✅ اكتمل الموعد                           │
│  │         حشو ضرس — د. خالد — 45 دق               │
│  │                                                    │
│  ├─ 15/06  📋 تفعيل خطة علاج                        │
│  │         خطة تقويم شاملة — 3 مراحل — 4500 ر.س     │
│  │                                                    │
│  ├─ 10/06  📸 صورة مرفقة                            │
│  │         X-Ray قبل العلاج                          │
│  │                                                    │
│  ●  مايو 2026                                        │
│  │                                                    │
│  ├─ 28/05  🗓️ حجز موعد                              │
│  │         استشارة — د. خالد                         │
│  │                                                    │
│  └─ 20/05  👤 تسجيل المريض                          │
│            المُستقبِل: سارة                          │
│                                                        │
└────────────────────────────────────────────────────────┘
```

### الفلاتر
- نوع الحدث (متعدد الاختيار)
- المدة الزمنية
- الطبيب
- البحث النصي

### الأيقونات حسب نوع الحدث

| النوع | الأيقونة | اللون |
|-------|---------|-------|
| تسجيل | 👤 | رمادي |
| موعد | 🗓️ | أزرق |
| إجراء | 🦷 | أخضر |
| خطة علاج | 📋 | بنفسجي |
| وسائط | 📸 | برتقالي |
| فاتورة | 💰 | ذهبي |
| تحويل طبيب | 🔄 | أصفر |
| تأمين | 🛡️ | أزرق فاتح |

---

## 8. قواعد العمل (Timeline)

| القاعدة | التفاصيل |
|---------|---------|
| لا حذف | الجدول append-only |
| الخصوصية | `is_visible_to_patient = FALSE` افتراضياً |
| الأداء | Index على `(patient_id, event_at DESC)` |
| الـ metadata | JSONB — كل event_type له schema خاص |
| التوقيت | `event_at` UTC دائماً |
| اسم المنفِّذ | محفوظ في `actor_name` حتى بعد حذف المستخدم |

---

## 9. خارطة المراحل

| Phase | الأحداث المضافة |
|-------|---------------|
| Phase 2 (الآن) | Registration, Appointment, Queue |
| Phase 3 | Chart, Procedure, TreatmentPlan, Media, DoctorAssignment |
| Phase 5 | Invoice, Payment, InsuranceClaim |

---

**📋 هذا الوثيق جاهز للمراجعة — لا يُبدأ التنفيذ قبل الموافقة.**
