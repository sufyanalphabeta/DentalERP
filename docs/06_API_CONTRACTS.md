# 06 — API Contracts
# عقود الـ API — DentalERP

> **الإصدار:** V-Final | **التاريخ:** 2026-06-16 | **المرجع:** [05_DATABASE_DICTIONARY.md](05_DATABASE_DICTIONARY.md)

---

## 1. اصطلاحات عامة

### Base URL
```
/api/v1/
```

### Authentication
```
Authorization: Bearer {access_token}
```
كل endpoint يتطلب Token ما لم يُذكر خلاف ذلك.

### أنواع الاستجابة القياسية

**نجاح (200/201):**
```json
{
  "success": true,
  "data": { ... },
  "meta": { "total": 0, "page": 1, "pageSize": 20 }
}
```

**خطأ (400/401/403/404/422/500):**
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "رسالة خطأ للمستخدم",
    "details": [
      { "field": "phone", "message": "الهاتف مطلوب" }
    ]
  }
}
```

### أكواد الخطأ الشائعة

| الكود | HTTP | المعنى |
|-------|------|--------|
| `UNAUTHORIZED` | 401 | Token غير موجود أو منتهي |
| `FORBIDDEN` | 403 | صلاحية غير كافية |
| `NOT_FOUND` | 404 | السجل غير موجود |
| `VALIDATION_ERROR` | 422 | خطأ في البيانات المُرسَلة |
| `CONFLICT` | 409 | تعارض (مكرر، حالة غير مسموح) |
| `IMMUTABLE_RECORD` | 409 | محاولة تعديل/حذف سجل غير قابل |
| `WORKFLOW_REQUIRED` | 403 | الإجراء يحتاج موافقة مدير |
| `INTERNAL_ERROR` | 500 | خطأ داخلي |

### Pagination
```
GET /api/v1/resource?page=1&pageSize=20&sortBy=created_at&sortDir=desc
```

---

## 2. AUTH — المصادقة

### POST `/api/auth/login`
**الوصف:** تسجيل الدخول — إرجاع access_token + refresh_token

**Request Body:**
```json
{
  "username": "admin",
  "password": "••••••••"
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "accessTokenExpiresAt": "2026-06-17T10:15:00Z",
    "refreshToken": "eyJ...",
    "refreshTokenExpiresAt": "2026-06-24T10:00:00Z",
    "user": {
      "id": "uuid",
      "username": "admin",
      "fullName": "محمد أحمد",
      "roles": ["Admin"],
      "permissions": ["Patient.Create", "Invoice.Cancel"]
    }
  }
}
```

**أخطاء:**
- `401 INVALID_CREDENTIALS` — بيانات خاطئة
- `401 ACCOUNT_LOCKED` — حساب مقفل + `lockedUntil`

---

### POST `/api/auth/refresh`
**الوصف:** تجديد access_token باستخدام refresh_token

**Request Body:**
```json
{ "refreshToken": "eyJ..." }
```

**Response 200:** نفس هيكل login (tokens جديدة)

---

### POST `/api/auth/logout`
**الوصف:** إلغاء refresh_token الحالي

**Request Body:**
```json
{ "refreshToken": "eyJ..." }
```

**Response 200:**
```json
{ "success": true }
```

---

## 3. PATIENTS — المرضى

### POST `/api/patients`
**الصلاحية:** `Patient.Create`  
**الوصف:** تسجيل مريض جديد

**Request Body:**
```json
{
  "fullName": "محمد علي محمد العربي",
  "phone": "0912345678",
  "phone2": null,
  "nationalId": null,
  "dateOfBirth": "1990-03-15",
  "gender": "male",
  "address": "طرابلس، حي الأندلس",
  "bloodType": "O+",
  "allergies": ["بنسلين"],
  "chronicDiseases": ["سكري"],
  "emergencyContact": {
    "name": "أحمد علي",
    "phone": "0913456789",
    "relation": "أخ"
  },
  "referralSource": "مريض آخر"
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "mrn": "DEN-2026-00001",
    "fullName": "محمد علي محمد العربي",
    "phone": "0912345678",
    "createdAt": "2026-06-17T08:30:00Z"
  }
}
```

**أخطاء:**
- `422 VALIDATION_ERROR` — حقول مطلوبة ناقصة
- ـ **Note:** قبل الإنشاء يُرسَل طلب `GET /api/patients/check-duplicate`

---

### GET `/api/patients/check-duplicate`
**الصلاحية:** `Patient.Create`  
**الوصف:** فحص التكرار قبل التسجيل (BR-PAT-01)

**Query Params:**
```
phone=0912345678&fullName=محمد علي
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "hasDuplicate": true,
    "matches": [
      {
        "id": "uuid",
        "mrn": "DEN-2025-00045",
        "fullName": "محمد علي أحمد",
        "phone": "0912345678",
        "dateOfBirth": "1990-03-15"
      }
    ]
  }
}
```

---

### GET `/api/patients`
**الصلاحية:** `Patient.View`  
**الوصف:** قائمة المرضى مع بحث وفلترة

**Query Params:**
```
q=محمد&page=1&pageSize=20&doctorId=uuid
```

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "mrn": "DEN-2026-00001",
      "fullName": "محمد علي",
      "phone": "0912345678",
      "age": 36,
      "lastVisit": "2026-06-10",
      "balance": 150.00
    }
  ],
  "meta": { "total": 150, "page": 1, "pageSize": 20 }
}
```

---

### GET `/api/patients/{id}`
**الصلاحية:** `Patient.View`  
**الوصف:** ملف المريض الكامل

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "mrn": "DEN-2026-00001",
    "fullName": "محمد علي",
    "phone": "0912345678",
    "dateOfBirth": "1990-03-15",
    "age": 36,
    "gender": "male",
    "bloodType": "O+",
    "allergies": ["بنسلين"],
    "chronicDiseases": ["سكري"],
    "emergencyContact": { "name": "أحمد علي", "phone": "0913456789", "relation": "أخ" },
    "insuranceLinks": [
      {
        "id": "uuid",
        "companyName": "شركة التأمين الليبية",
        "policyNumber": "POL-2026-001",
        "isActive": true
      }
    ],
    "assignedDoctors": [
      { "doctorId": "uuid", "doctorName": "د. أحمد سالم", "status": "active" }
    ],
    "createdAt": "2026-06-01T09:00:00Z"
  }
}
```

---

### PUT `/api/patients/{id}`
**الصلاحية:** `Patient.Edit`  
**Request Body:** نفس POST (حقول مُحدَّثة فقط)  
**Response 200:** البيانات المحدَّثة

---

### DELETE `/api/patients/{id}`
**الصلاحية:** `Patient.Delete`  
**الوصف:** Soft Delete (BR-PAT-06)

**Response 200:**
```json
{ "success": true }
```

**أخطاء:**
- `409 CONFLICT` — المريض له سجل مالي، لا يمكن حذفه

---

### GET `/api/patients/{id}/financial-account`
**الصلاحية:** `Patient.View`  
**الوصف:** كشف الحساب المالي للمريض

**Response 200:**
```json
{
  "success": true,
  "data": {
    "patientId": "uuid",
    "totalInvoiced": 5000.00,
    "totalPaid": 3500.00,
    "totalRemaining": 1500.00,
    "advanceBalance": 200.00,
    "invoices": [
      {
        "id": "uuid",
        "invoiceNumber": "INV-2026-000001",
        "totalAmount": 2000.00,
        "paidAmount": 2000.00,
        "status": "paid",
        "createdAt": "2026-06-01"
      }
    ],
    "installmentsDue": [
      {
        "planId": "uuid",
        "installmentNum": 2,
        "dueDate": "2026-07-01",
        "amount": 500.00,
        "status": "pending"
      }
    ]
  }
}
```

---

### GET `/api/patients/{id}/media`
**الصلاحية:** `Patient.View`  
**الوصف:** مكتبة الصور والوثائق

**Query Params:** `type=xray&procedureId=uuid`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "type": "xray",
      "fileUrl": "/uploads/patients/uuid/xray-001.jpg",
      "fileName": "xray-001.jpg",
      "notes": "صورة أشعة شعاعية للفك",
      "procedureId": "uuid",
      "uploadedAt": "2026-06-10T11:00:00Z"
    }
  ]
}
```

---

### POST `/api/patients/{id}/media`
**الصلاحية:** `Patient.Edit`  
**Content-Type:** `multipart/form-data`

**Form Fields:**
```
type: "xray"
procedureId: "uuid" (optional)
notes: "وصف الصورة"
file: [binary]
```

**Response 201:** بيانات الملف المرفوع

---

## 4. SCHEDULING — المواعيد

### POST `/api/appointments`
**الصلاحية:** `Patient.Create`  
**Request Body:**
```json
{
  "patientId": "uuid",
  "doctorId": "uuid",
  "scheduledAt": "2026-06-20T10:00:00Z",
  "durationMinutes": 45,
  "type": "procedure",
  "treatmentLocationId": "uuid",
  "notes": "متابعة حشوة ضرس"
}
```

**Response 201:** بيانات الموعد

---

### GET `/api/appointments`
**الصلاحية:** `Patient.View`  
**Query Params:** `doctorId=uuid&date=2026-06-20&status=scheduled`

**Response 200:** قائمة المواعيد مع بيانات المريض

---

### PUT `/api/appointments/{id}/status`
**الوصف:** تحديث حالة الموعد

**Request Body:**
```json
{ "status": "cancelled", "cancelledReason": "المريض اعتذر" }
```

---

### GET `/api/queue`
**الوصف:** الطابور الحي ليوم محدد (Real-time via SignalR)

**Query Params:** `doctorId=uuid&date=2026-06-17`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "queueNumber": 3,
      "patientId": "uuid",
      "patientName": "محمد علي",
      "status": "waiting",
      "waitMinutes": 25,
      "checkedInAt": "2026-06-17T09:30:00Z"
    }
  ]
}
```

---

### POST `/api/queue/check-in`
**الوصف:** تسجيل حضور مريض (Walk-in أو بموعد)

**Request Body:**
```json
{
  "patientId": "uuid",
  "doctorId": "uuid",
  "appointmentId": null,
  "visitType": "followup"
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "queueNumber": 4,
    "estimatedWaitMinutes": 30
  }
}
```

---

### PUT `/api/queue/{id}/status`
**الوصف:** تحديث حالة الطابور (called, with_doctor, done...)

**Request Body:**
```json
{ "status": "called" }
```

---

## 5. CLINICAL — السريرية

### GET `/api/dental-chart/{patientId}`
**الصلاحية:** `Patient.View`  
**الوصف:** الرسم الكامل لأسنان المريض

**Response 200:**
```json
{
  "success": true,
  "data": {
    "patientId": "uuid",
    "teeth": [
      {
        "toothNumber": 16,
        "surface": "O",
        "condition": "filling",
        "mobilityGrade": 0,
        "notes": "حشوة كومبوزيت 2025",
        "recordedAt": "2025-03-10"
      }
    ]
  }
}
```

---

### PUT `/api/dental-chart/{patientId}/teeth/{toothNumber}`
**الصلاحية:** `Procedure.Create` أو `Procedure.Edit`  
**الوصف:** تحديث حالة سن (BR-DOC-03)

**Request Body:**
```json
{
  "condition": "crown",
  "surface": null,
  "mobilityGrade": 1,
  "notes": "تاج خزفي"
}
```

---

### POST `/api/treatment-plans`
**الصلاحية:** `TreatmentPlan.Create`

**Request Body:**
```json
{
  "patientId": "uuid",
  "doctorId": "uuid",
  "title": "خطة التقويم الشامل",
  "notes": "6 أشهر متوقعة",
  "items": [
    {
      "serviceId": "uuid",
      "toothNumber": 11,
      "estimatedPrice": 500.00,
      "sortOrder": 1,
      "notes": null
    }
  ]
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "title": "خطة التقويم الشامل",
    "itemsCount": 1,
    "totalEstimated": 500.00
  }
}
```

---

### PUT `/api/treatment-plans/{id}`
**الصلاحية:** `TreatmentPlan.Edit`  
**Request Body:** نفس POST

---

### POST `/api/procedures`
**الصلاحية:** `Procedure.Create`

**Request Body:**
```json
{
  "patientId": "uuid",
  "doctorId": "uuid",
  "serviceId": "uuid",
  "treatmentLocationId": "uuid",
  "treatmentPlanId": null,
  "treatmentPlanItemId": null,
  "toothNumbers": [16],
  "basePrice": 300.00,
  "discountType": "percentage",
  "discountValue": 10,
  "finalPrice": 270.00,
  "labCost": 0,
  "clinicalNotes": "حشوة كومبوزيت"
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "status": "draft",
    "finalPrice": 270.00
  }
}
```

---

### PUT `/api/procedures/{id}`
**الصلاحية:** `Procedure.Edit`  
**الوصف:** تعديل إجراء — يتحقق من workflow_settings

**Flow:**
1. إذا `workflow_settings.procedure_edit.requires_approval = false` → تعديل مباشر
2. إذا `true` → ينشئ approval_request ويُعيد `{ requiresApproval: true, requestId: "uuid" }`

**Response 200 (مباشر):**
```json
{
  "success": true,
  "data": { "id": "uuid", "status": "draft", "finalPrice": 250.00 }
}
```

**Response 202 (يحتاج موافقة):**
```json
{
  "success": true,
  "data": {
    "requiresApproval": true,
    "requestId": "uuid",
    "message": "طلب التعديل قيد المراجعة"
  }
}
```

---

### DELETE `/api/procedures/{id}`
**الصلاحية:** `Procedure.Delete`  
**الوصف:** يتحقق من workflow_settings.procedure_delete

**Response:** مباشر 200 أو 202 (requiresApproval)  
**أخطاء:**
- `409 CONFLICT` — الإجراء بحالة `billed`، لا يمكن حذفه

---

### POST `/api/procedures/{id}/confirm`
**الصلاحية:** `Procedure.Create`  
**الوصف:** تأكيد الإجراء → يُغيّر status إلى `confirmed` + يُنشئ حركات مخزون تلقائياً

**Response 200:**
```json
{
  "success": true,
  "data": {
    "procedureId": "uuid",
    "status": "confirmed",
    "stockMovementsCreated": 2
  }
}
```

---

### GET `/api/procedures/{id}/default-materials`
**الوصف:** المواد الافتراضية للخدمة (لعرضها قبل التأكيد)

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "stockItemId": "uuid",
      "stockItemName": "مادة الحشو الكومبوزيت",
      "defaultQuantity": 2.0,
      "unit": "gram",
      "currentStock": 45.5
    }
  ]
}
```

---

## 6. INVOICING — الفواتير

### POST `/api/invoices`
**الصلاحية:** `Invoice.Create`

**Request Body:**
```json
{
  "patientId": "uuid",
  "doctorId": "uuid",
  "procedureIds": ["uuid1", "uuid2"],
  "notes": null
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "invoiceNumber": "INV-2026-000001",
    "subtotal": 570.00,
    "discountTotal": 30.00,
    "totalAmount": 540.00,
    "status": "confirmed"
  }
}
```

**أخطاء:**
- `409 CONFLICT` — الإجراء بحالة `draft` (يجب تأكيده أولاً)

---

### PUT `/api/invoices/{id}`
**الصلاحية:** `Invoice.Edit`  
**الوصف:** تعديل draft فقط — يُعيد 409 للـ confirmed

---

### DELETE `/api/invoices/{id}`
**الصلاحية:** `Invoice.Delete`  
**الوصف:** حذف draft فقط (BR-FIN-06)

---

### POST `/api/invoices/{id}/cancel`
**الصلاحية:** `Invoice.Cancel`  
**الوصف:** إلغاء فاتورة — يتحقق من workflow_settings.invoice_cancel

**Request Body:**
```json
{ "reason": "طلب إلغاء من المريض بعد الاتفاق" }
```

**Response 200 (مباشر):**
```json
{ "success": true, "data": { "id": "uuid", "status": "cancelled" } }
```

**Response 202 (يحتاج موافقة):**
```json
{
  "success": true,
  "data": { "requiresApproval": true, "requestId": "uuid" }
}
```

---

### GET `/api/invoices/{id}`
**الصلاحية:** `Invoice.Print`

**Response 200:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "invoiceNumber": "INV-2026-000001",
    "patient": { "id": "uuid", "name": "محمد علي", "mrn": "DEN-2026-00001" },
    "doctor": { "id": "uuid", "name": "د. أحمد سالم" },
    "items": [
      {
        "serviceName": "حشوة كومبوزيت",
        "toothNumbers": [16],
        "quantity": 1,
        "unitPrice": 300.00,
        "discount": 30.00,
        "total": 270.00
      }
    ],
    "subtotal": 300.00,
    "discountTotal": 30.00,
    "totalAmount": 270.00,
    "paidAmount": 270.00,
    "remaining": 0.00,
    "status": "paid",
    "payments": [
      {
        "amount": 270.00,
        "method": "cash",
        "createdAt": "2026-06-17T11:00:00Z"
      }
    ]
  }
}
```

---

### GET `/api/invoices/{id}/print`
**الصلاحية:** `Invoice.Print`  
**الوصف:** إنشاء PDF للطباعة (FastReport.NET)

**Response 200:**
```
Content-Type: application/pdf
Content-Disposition: attachment; filename="INV-2026-000001.pdf"
[binary PDF]
```

---

### POST `/api/invoices/{id}/payments`
**الصلاحية:** `Treasury.Add`  
**الوصف:** تسجيل دفعة على فاتورة

**Request Body:**
```json
{
  "vaultId": "uuid",
  "amount": 150.00,
  "paymentMethod": "cash",
  "referenceNumber": null,
  "notes": null
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "paymentId": "uuid",
    "invoiceStatus": "partially_paid",
    "remainingAmount": 120.00
  }
}
```

---

## 7. TREASURY — الخزينة

### POST `/api/treasury/transactions`
**الصلاحية:** `Treasury.Add`  
**الوصف:** إنشاء حركة خزينة عامة

**Request Body:**
```json
{
  "vaultId": "uuid",
  "transactionType": "general_payment",
  "amount": 500.00,
  "direction": "out",
  "costCenterId": "uuid",
  "notes": "صيانة أجهزة",
  "referenceNumber": null
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "transactionType": "general_payment",
    "amount": 500.00,
    "vaultBalance": 4500.00
  }
}
```

---

### PUT `/api/treasury/transactions/{id}`
**الصلاحية:** `Treasury.Edit`  
**الوصف:** تعديل حركة (غير مُعكوسة)

---

### DELETE `/api/treasury/transactions/{id}`
**الصلاحية:** `Treasury.Delete`  
**الوصف:** حذف — مقيّد بـ BR-FIN-06

---

### POST `/api/treasury/transactions/{id}/reverse`
**الصلاحية:** `Treasury.Reverse`  
**الوصف:** عكس حركة خزينة (BR-FIN-07)

**Request Body:**
```json
{
  "reason": "خطأ في تسجيل المبلغ — المبلغ الصحيح 450 لا 500",
  "correctedTransaction": {
    "vaultId": "uuid",
    "transactionType": "general_payment",
    "amount": 450.00,
    "direction": "out",
    "costCenterId": "uuid",
    "notes": "صيانة أجهزة (المبلغ الصحيح)"
  }
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "reversalId": "uuid",
    "originalTransactionId": "uuid",
    "correctedTransactionId": "uuid",
    "reverseLinkId": "uuid"
  }
}
```

**أخطاء:**
- `409 ALREADY_REVERSED` — الحركة مُعكوسة مسبقاً
- `403 REVERSAL_WINDOW_EXPIRED` — أكثر من 30 يوم (يحتاج صلاحية Admin)

---

### GET `/api/treasury/vaults/balances`
**الصلاحية:** `Treasury.Add`  
**الوصف:** أرصدة كل الخزائن الحالية

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "vaultId": "uuid",
      "name": "الخزينة الرئيسية",
      "type": "cash",
      "currentBalance": 15000.00
    }
  ]
}
```

---

### POST `/api/treasury/day-closure`
**الصلاحية:** `System.Settings`  
**الوصف:** إقفال اليوم

**Request Body:**
```json
{ "closureDate": "2026-06-17" }
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "closureDate": "2026-06-17",
    "totalReceipts": 8500.00,
    "totalPayments": 2300.00,
    "netFlow": 6200.00,
    "vaultSnapshots": {
      "vault-uuid-1": { "opening": 8800.00, "closing": 15000.00 }
    }
  }
}
```

---

### GET `/api/treasury/doctors/{doctorId}/account`
**الصلاحية:** `Reports.View` أو طبيب يرى حسابه فقط  
**الوصف:** كشف حساب الطبيب

**Response 200:**
```json
{
  "success": true,
  "data": {
    "doctorId": "uuid",
    "doctorName": "د. أحمد سالم",
    "totalProcedures": 145,
    "totalRevenue": 28500.00,
    "totalCommissionDue": 5700.00,
    "totalPaid": 4000.00,
    "remaining": 1700.00,
    "commissionRecords": [
      {
        "procedureId": "uuid",
        "serviceName": "حشوة كومبوزيت",
        "commissionAmount": 54.00,
        "isPaid": false
      }
    ]
  }
}
```

---

### GET `/api/treasury/suppliers/{supplierId}/account`
**الصلاحية:** `Purchase.View`  
**الوصف:** كشف حساب المورد

**Response 200:**
```json
{
  "success": true,
  "data": {
    "supplierId": "uuid",
    "supplierName": "مورد المواد الطبية",
    "openingBalance": 0,
    "totalPurchases": 12000.00,
    "totalPaid": 9000.00,
    "currentBalance": 3000.00
  }
}
```

---

### POST `/api/treasury/commissions/{doctorId}/pay`
**الصلاحية:** `Treasury.Add`  
**الوصف:** صرف عمولة للطبيب

**Request Body:**
```json
{
  "vaultId": "uuid",
  "amount": 1700.00,
  "commissionIds": ["uuid1", "uuid2"],
  "notes": null
}
```

---

### POST `/api/treasury/advance-payments`
**الصلاحية:** `Treasury.Add`  
**الوصف:** تسجيل دفعة مقدمة لمريض

**Request Body:**
```json
{
  "patientId": "uuid",
  "vaultId": "uuid",
  "amount": 500.00,
  "notes": "دفعة مقدمة لخطة التقويم"
}
```

---

### POST `/api/treasury/payroll`
**الصلاحية:** `Treasury.Add`  
**الوصف:** صرف رواتب شهرية

**Request Body:**
```json
{
  "periodMonth": "2026-06-01",
  "vaultId": "uuid",
  "payments": [
    { "staffId": "uuid", "amount": 2000.00 }
  ]
}
```

---

## 8. INSURANCE — التأمين

### POST `/api/insurance/companies`
**الصلاحية:** `System.Settings`

**Request Body:**
```json
{
  "name": "شركة الوفاء للتأمين",
  "contractNumber": "CON-2026-001",
  "defaultCoveragePct": 70,
  "priceIncreasePct": 0,
  "priceDiscountPct": 10,
  "contactPerson": "سعيد محمد",
  "phone": "021-1234567"
}
```

---

### POST `/api/insurance/companies/{id}/covered-services`
**الصلاحية:** `System.Settings`  
**الوصف:** تعريف خدمات مغطاة لشركة تأمين

**Request Body:**
```json
{
  "serviceId": "uuid",
  "coveragePct": 80,
  "coverageCap": 300.00,
  "customPrice": null
}
```

---

### POST `/api/insurance/claims`
**الصلاحية:** `Invoice.Create`  
**الوصف:** إنشاء مطالبة تأمين يدوية

**Request Body:**
```json
{
  "patientId": "uuid",
  "procedureId": "uuid",
  "insuranceCompanyId": "uuid"
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "claimNumber": "CLM-2026-00001",
    "claimAmount": 240.00,
    "basePriceUsed": 300.00,
    "status": "open"
  }
}
```

---

### POST `/api/insurance/claims/{id}/collect`
**الصلاحية:** `Treasury.Add`  
**الوصف:** تسجيل تحصيل من شركة التأمين

**Request Body:**
```json
{
  "vaultId": "uuid",
  "amount": 200.00,
  "notes": "تحصيل جزئي من شركة الوفاء"
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "claimId": "uuid",
    "collectedAmount": 200.00,
    "status": "partially_paid",
    "remaining": 40.00
  }
}
```

---

### POST `/api/insurance/claims/{id}/reject`
**الصلاحية:** `Invoice.Edit`  
**الوصف:** تسجيل رفض مطالبة

**Request Body:**
```json
{
  "rejectionReason": "الخدمة غير مشمولة في العقد",
  "isPartial": false
}
```

---

## 9. INVENTORY — المخزون

### POST `/api/inventory/items`
**الصلاحية:** `Stock.Add`

**Request Body:**
```json
{
  "name": "مادة الحشو الكومبوزيت",
  "code": "STK-001",
  "category": "مواد طب أسنان",
  "unit": "gram",
  "minimumThreshold": 50,
  "expiryAlertDays": 60
}
```

---

### POST `/api/inventory/items/{id}/receive`
**الصلاحية:** `Stock.Add`  
**الوصف:** استلام دُفعة جديدة

**Request Body:**
```json
{
  "batchNumber": "BATCH-2026-001",
  "expiryDate": "2028-06-01",
  "quantity": 200,
  "unitCost": 5.50
}
```

---

### POST `/api/inventory/issue`
**الصلاحية:** `Stock.Delete`  
**الوصف:** صرف يدوي

**Request Body:**
```json
{
  "stockItemId": "uuid",
  "quantity": 5,
  "movementType": "waste",
  "wasteReason": "expired",
  "notes": "مادة منتهية الصلاحية"
}
```

---

### POST `/api/inventory/stock-take`
**الصلاحية:** `Stock.Count`  
**الوصف:** إجراء جرد دوري

**Request Body:**
```json
{
  "items": [
    {
      "stockItemId": "uuid",
      "batchId": "uuid",
      "actualQuantity": 85.5
    }
  ],
  "notes": "جرد شهر يونيو"
}
```

**Response 200:**
```json
{
  "success": true,
  "data": {
    "variances": [
      {
        "stockItemId": "uuid",
        "itemName": "مادة الحشو",
        "expectedQty": 90.0,
        "actualQty": 85.5,
        "variance": -4.5,
        "variancePct": -5.0,
        "requiresApproval": true
      }
    ]
  }
}
```

---

### GET `/api/inventory/alerts`
**الصلاحية:** `Stock.View`  
**الوصف:** تنبيهات المخزون (تحت الحد + قارب الانتهاء)

**Response 200:**
```json
{
  "success": true,
  "data": {
    "lowStock": [
      { "itemId": "uuid", "name": "قفازات لاتكس", "currentQty": 10, "threshold": 50 }
    ],
    "expiringSoon": [
      { "itemId": "uuid", "name": "صبغة تشخيصية", "expiryDate": "2026-07-15", "daysLeft": 28 }
    ],
    "expired": [
      { "itemId": "uuid", "name": "مادة التعقيم", "expiryDate": "2026-06-01" }
    ]
  }
}
```

---

### POST `/api/inventory/default-materials`
**الصلاحية:** `Stock.Edit`  
**الوصف:** تعريف مواد افتراضية لخدمة

**Request Body:**
```json
{
  "serviceId": "uuid",
  "materials": [
    { "stockItemId": "uuid", "defaultQuantity": 2.0 }
  ]
}
```

---

## 10. PURCHASING — المشتريات

### POST `/api/purchasing/requests`
**الصلاحية:** `Purchase.Create`

**Request Body:**
```json
{
  "title": "طلب شراء مواد شهر يونيو",
  "items": [
    { "stockItemId": "uuid", "requestedQuantity": 100 }
  ],
  "notes": null
}
```

---

### POST `/api/purchasing/orders`
**الصلاحية:** `Purchase.Create`

**Request Body:**
```json
{
  "supplierId": "uuid",
  "requestId": null,
  "expectedDate": "2026-06-25",
  "items": [
    { "stockItemId": "uuid", "orderedQuantity": 100, "unitPrice": 5.50 }
  ]
}
```

---

### POST `/api/purchasing/orders/{id}/approve`
**الصلاحية:** `Purchase.Approve`

**Response 200:**
```json
{
  "success": true,
  "data": { "orderId": "uuid", "status": "approved" }
}
```

---

### POST `/api/purchasing/invoices`
**الصلاحية:** `Purchase.Create`  
**الوصف:** تسجيل فاتورة شراء + استلام بضاعة

**Request Body:**
```json
{
  "orderId": "uuid",
  "supplierId": "uuid",
  "invoiceNumber": "SUP-INV-001",
  "invoiceDate": "2026-06-18",
  "dueDate": "2026-07-18",
  "items": [
    {
      "stockItemId": "uuid",
      "quantity": 100,
      "unitPrice": 5.50,
      "batchNumber": "BATCH-001",
      "expiryDate": "2028-01-01"
    }
  ]
}
```

---

## 11. REPORTS — التقارير

### GET `/api/reports/daily-summary`
**الصلاحية:** `Reports.View`  
**Query Params:** `date=2026-06-17`

**Response 200:**
```json
{
  "success": true,
  "data": {
    "date": "2026-06-17",
    "totalPatients": 25,
    "totalProcedures": 32,
    "totalInvoiced": 8500.00,
    "totalCollected": 7200.00,
    "vaultSummary": [
      { "vaultName": "الخزينة الرئيسية", "receipts": 7200.00, "payments": 1500.00, "net": 5700.00 }
    ]
  }
}
```

---

### GET `/api/reports/financial`
**الصلاحية:** `Reports.View`  
**Query Params:** `startDate=2026-06-01&endDate=2026-06-30&doctorId=uuid`

**Response 200:** تقرير مالي شامل (إيرادات، مصروفات، صافي)

---

### GET `/api/reports/inventory`
**الصلاحية:** `Reports.View`  
**Query Params:** `type=consumption&startDate=2026-06-01&endDate=2026-06-30`

---

### GET `/api/reports/insurance`
**الصلاحية:** `Reports.View`  
**Query Params:** `insuranceCompanyId=uuid&status=open`

**Response 200:**
```json
{
  "success": true,
  "data": {
    "totalClaims": 45,
    "totalClaimAmount": 12000.00,
    "collected": 8500.00,
    "pending": 3500.00,
    "claims": [ ... ]
  }
}
```

---

### GET `/api/reports/{category}/export`
**الصلاحية:** `Reports.Export`  
**الوصف:** تصدير التقرير

**Query Params:** `format=pdf` أو `format=excel`

**Response 200:**
```
Content-Type: application/pdf OR application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
Content-Disposition: attachment; filename="report-2026-06.pdf"
[binary file]
```

---

## 12. ADMINISTRATION — الإدارة

### POST `/api/admin/users`
**الصلاحية:** `User.Manage`

**Request Body:**
```json
{
  "username": "dr_ahmed",
  "fullName": "د. أحمد سالم",
  "email": "ahmed@clinic.ly",
  "password": "••••••••",
  "roleIds": ["uuid"],
  "doctorId": "uuid"
}
```

---

### PUT `/api/admin/users/{id}/roles`
**الصلاحية:** `User.Manage`

**Request Body:**
```json
{ "roleIds": ["uuid1", "uuid2"] }
```

---

### POST `/api/admin/roles`
**الصلاحية:** `Role.Manage`

**Request Body:**
```json
{
  "name": "موظف مشتريات",
  "description": "يدير طلبات الشراء فقط",
  "permissionIds": ["uuid-purchase-create", "uuid-purchase-edit"]
}
```

---

### GET `/api/admin/workflow-settings`
**الصلاحية:** `System.Settings`

**Response 200:**
```json
{
  "success": true,
  "data": [
    { "actionType": "procedure_edit", "requiresApproval": false },
    { "actionType": "procedure_delete", "requiresApproval": true },
    { "actionType": "invoice_cancel", "requiresApproval": false }
  ]
}
```

---

### PUT `/api/admin/workflow-settings/{actionType}`
**الصلاحية:** `System.Settings`

**Request Body:**
```json
{ "requiresApproval": true }
```

---

### GET `/api/admin/approval-requests`
**الصلاحية:** `System.Settings` (مدير النظام)  
**Query Params:** `status=pending&type=invoice_cancel`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "requestType": "invoice_cancel",
      "entityId": "uuid",
      "reason": "طلب إلغاء بعد الاتفاق",
      "requestedBy": "محمد علي",
      "requestedAt": "2026-06-17T10:00:00Z",
      "status": "pending"
    }
  ]
}
```

---

### PUT `/api/admin/approval-requests/{id}/review`
**الصلاحية:** `System.Settings`  
**الوصف:** البت في طلب الموافقة (BR-APR-02)

**Request Body:**
```json
{
  "decision": "approved",
  "reviewNotes": null
}
```

**أخطاء:**
- `403 SELF_APPROVAL` — مقدم الطلب هو نفسه المراجع

---

### GET `/api/admin/audit-logs`
**الصلاحية:** `System.Settings`  
**Query Params:** `module=Invoicing&entityId=uuid&startDate=2026-06-01&endDate=2026-06-30`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "action": "CANCEL",
      "entityType": "invoice",
      "entityId": "uuid",
      "username": "admin",
      "ipAddress": "192.168.1.10",
      "oldValues": { "status": "confirmed" },
      "newValues": { "status": "cancelled" },
      "createdAt": "2026-06-17T09:45:00Z"
    }
  ]
}
```

---

### GET `/api/admin/clinic-settings`
**الصلاحية:** `System.Settings`  
**الوصف:** إعدادات العيادة

---

### PUT `/api/admin/clinic-settings`
**الصلاحية:** `System.Settings`

**Request Body:**
```json
{
  "name": "عيادة الأمل لطب الأسنان",
  "phone": "021-1234567",
  "address": "طرابلس، شارع الجمهورية",
  "taxNumber": "TAX-001",
  "workingHours": {
    "saturday": { "isOpen": true, "open": "08:00", "close": "17:00" },
    "sunday": { "isOpen": false }
  }
}
```

---

## 13. LABORATORY — المعمل ★ Core V1

### GET `/api/lab/orders`
**الصلاحية:** `Lab.Create`  
**Query Params:** `status=pending&patientId=uuid&from=2026-06-01&to=2026-06-30`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "orderNumber": "LAB-2026-00001",
      "patientName": "أحمد محمد",
      "doctorName": "د. سالم",
      "labOrderType": "تركيبات ثابتة",
      "labTechnicianName": "معمل الأندلس",
      "status": "pending",
      "orderDate": "2026-06-15",
      "expectedDate": "2026-06-22",
      "cost": 350.00,
      "price": 500.00
    }
  ],
  "meta": { "total": 15, "page": 1, "pageSize": 20 }
}
```

---

### POST `/api/lab/orders`
**الصلاحية:** `Lab.Create`

**Request Body:**
```json
{
  "patientId": "uuid",
  "doctorId": "uuid",
  "procedureId": "uuid",
  "labTechnicianId": "uuid",
  "labOrderTypeId": "uuid",
  "description": "كراون بورسلين على السن 26",
  "toothNumbers": [26],
  "expectedDate": "2026-06-22",
  "cost": 350.00,
  "price": 500.00,
  "notes": null
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "orderNumber": "LAB-2026-00001",
    "status": "pending"
  }
}
```

---

### PUT `/api/lab/orders/{id}/status`
**الصلاحية:** `Lab.Edit`  
**الوصف:** تحديث حالة أمر المعمل

**Request Body:**
```json
{
  "status": "delivered",
  "deliveryDate": "2026-06-21",
  "notes": "مطابق للمواصفات"
}
```

---

### GET `/api/lab/technicians`
**الصلاحية:** `Lab.Manage`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "fullName": "أحمد فرج",
      "labName": "معمل الأندلس",
      "specialty": "تركيبات ثابتة",
      "currentBalance": 1200.00
    }
  ]
}
```

---

### GET `/api/lab/technicians/{id}/account`
**الصلاحية:** `Lab.Manage`  
**الوصف:** كشف حساب فني المعمل (مبني على lab_technician_account_summary View)

**Response 200:**
```json
{
  "success": true,
  "data": {
    "labTechnicianId": "uuid",
    "fullName": "أحمد فرج",
    "labName": "معمل الأندلس",
    "openingBalance": 0,
    "totalCost": 4500.00,
    "totalPaid": 3300.00,
    "currentBalance": 1200.00,
    "orders": [...]
  }
}
```

---

### POST `/api/lab/technicians/{id}/pay`
**الصلاحية:** `Lab.Manage`  
**الوصف:** تسديد مدفوعات لفني المعمل

**Request Body:**
```json
{
  "vaultId": "uuid",
  "amount": 1200.00,
  "labOrderIds": ["uuid1", "uuid2"],
  "notes": "دفعة شهر يونيو"
}
```

---

### GET `/api/lab/commissions/{technicianId}`
**الصلاحية:** `Lab.Manage`  
**الوصف:** عمولات فني معمل مستحقة

---

### POST `/api/lab/commissions/{technicianId}/pay`
**الصلاحية:** `Lab.Manage`

**Request Body:**
```json
{
  "vaultId": "uuid",
  "commissionIds": ["uuid1", "uuid2"]
}
```

---

## 14. RADIOLOGY — الأشعة ★ Core V1

### GET `/api/radiology/orders`
**الصلاحية:** `Radiology.Create`  
**Query Params:** `status=pending&patientType=internal&from=2026-06-01`

**Response 200:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "orderNumber": "RAD-2026-00001",
      "patientType": "internal",
      "patientName": "أحمد محمد",
      "radiologyType": "OPG",
      "technicianName": "أنس المبروك",
      "status": "completed",
      "orderDate": "2026-06-17",
      "price": 80.00,
      "paidAmount": 80.00
    }
  ],
  "meta": { "total": 32, "page": 1, "pageSize": 20 }
}
```

---

### POST `/api/radiology/orders`
**الصلاحية:** `Radiology.Create`

**Request Body (مريض داخلي):**
```json
{
  "patientType": "internal",
  "patientId": "uuid",
  "referringDoctorId": "uuid",
  "radiologyTechnicianId": "uuid",
  "radiologyTypeId": "uuid",
  "procedureId": null,
  "price": 80.00,
  "notes": null
}
```

**Request Body (مريض خارجي):**
```json
{
  "patientType": "external",
  "externalPatientName": "سالم عمر",
  "externalPatientPhone": "0912345678",
  "radiologyTechnicianId": "uuid",
  "radiologyTypeId": "uuid",
  "price": 80.00,
  "paidAmount": 80.00,
  "paymentMethod": "cash",
  "vaultId": "uuid"
}
```

**Response 201:**
```json
{
  "success": true,
  "data": {
    "id": "uuid",
    "orderNumber": "RAD-2026-00001",
    "status": "pending"
  }
}
```

---

### PUT `/api/radiology/orders/{id}/status`
**الصلاحية:** `Radiology.Edit`

**Request Body:**
```json
{
  "status": "completed",
  "reportNotes": "لا توجد إشكاليات ظاهرة في الصورة"
}
```

---

### POST `/api/radiology/orders/{id}/images`
**الصلاحية:** `Radiology.Edit`  
**الوصف:** رفع صور الأشعة (يُخزَّن في MinIO)

**Request:** `multipart/form-data` — حقل `files` (صور متعددة)

**Response 201:**
```json
{
  "success": true,
  "data": [
    {
      "id": "uuid",
      "fileUrl": "/radiology/2026/06/RAD-2026-00001/img_001.jpg",
      "fileName": "opg_scan.jpg",
      "fileType": "jpg"
    }
  ]
}
```

---

### GET `/api/radiology/orders/{id}/images`
**الصلاحية:** `Radiology.Create`  
**الوصف:** عرض صور طلب الأشعة (من MinIO)

---

### CRUD `/api/radiology/types`
**الصلاحية:** `Radiology.Manage`  
**الوصف:** أنواع الأشعة (OPG, CBCT, X-Ray...)

---

### CRUD `/api/radiology/technicians`
**الصلاحية:** `Radiology.Manage`

---

### GET `/api/radiology/technicians/{id}/account`
**الصلاحية:** `Radiology.Manage`  
**الوصف:** كشف حساب فني الأشعة

---

### POST `/api/radiology/commissions/{technicianId}/pay`
**الصلاحية:** `Radiology.Manage`

---

## 15. REAL-TIME (SignalR Hubs)

### Hub: `/hubs/notifications`

| Event | الاتجاه | البيانات |
|-------|---------|---------|
| `QueueUpdated` | Server → Client | `{ queueDate, doctorId, entries: [...] }` |
| `ApprovalRequest` | Server → Admin | `{ requestId, requestType, requestedBy }` |
| `ApprovalDecision` | Server → Requester | `{ requestId, decision, reviewNotes }` |
| `LowStockAlert` | Server → Staff | `{ itemId, itemName, currentQty, threshold }` |
| `ClaimStatusChanged` | Server → Admin | `{ claimId, claimNumber, newStatus }` |

**اتصال JavaScript:**
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/notifications', { accessTokenFactory: () => token })
  .withAutomaticReconnect()
  .build();

connection.on('QueueUpdated', (data) => { /* تحديث الطابور */ });
await connection.start();
```

---

## 16. ملخص الـ Endpoints

| الوحدة | عدد الـ Endpoints |
|--------|-----------------|
| Auth | 3 |
| Patients | 8 |
| Scheduling | 5 |
| Clinical | 8 |
| Invoicing | 7 |
| Treasury | 8 |
| Insurance | 5 |
| Inventory | 6 |
| Purchasing | 5 |
| Reports | 4 |
| Administration | 9 |
| Laboratory ★ | 8 |
| Radiology ★ | 9 |
| **الإجمالي** | **85** |

---

## 17. ⚠️ نقاط تحتاج توضيح

1. **Pagination على Dental Chart:** Dental Chart يُعيد كل الأسنان (32 سناً بالحد الأقصى) — هل نحتاج pagination أم مصفوفة كاملة مرة واحدة؟ (الأفضل: مرة واحدة لأن العدد محدود)

2. **Media Upload — Local vs Cloud:** `POST /api/patients/{id}/media` — هل الملفات تُرفع على خادم محلي (LAN) أم تحتاج خياراً للـ Cloud Storage مستقبلاً؟ يؤثر على بنية `file_url`.

3. **تقرير الطبيب من Admin:** `GET /api/treasury/doctors/{doctorId}/account` — هل الطبيب نفسه يصل بنفس الـ endpoint؟ أم يحتاج endpoint منفصل `/api/me/account`؟

4. **SignalR Authentication:** هل نستخدم نفس JWT token للـ SignalR؟ أم نُنشئ ticket منفصل للاتصال بالـ Hub؟

5. **Soft Delete — هل يُعيد 404 أم 410 Gone؟** البيانات المحذوفة (Soft) — هل يراها Admin؟ أم تختفي تماماً؟

---

*هذه العقود تُبنى على [05_DATABASE_DICTIONARY.md](05_DATABASE_DICTIONARY.md)*  
*خطة التطوير المرحلية التي تُنفذ هذه الـ APIs → [07_DEVELOPMENT_PLAN.md](07_DEVELOPMENT_PLAN.md)*
