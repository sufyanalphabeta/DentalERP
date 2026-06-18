# Test Scenarios Document
**DentalERP — Phase 6 UAT**
**Date:** 2026-06-18
**Modules:** Inventory, Purchasing, Purchase Returns, Expenses, Assets

---

## How to Use This Document

For each scenario:
1. Obtain a JWT token: `POST /api/auth/login` with `{ "username": "admin", "password": "Admin@123" }`
2. Include `Authorization: Bearer <token>` on all subsequent requests
3. Mark each step **PASS** or **FAIL** with notes

---

## Module 1: Expense Categories

### TC-EXP-CAT-001 — Create Expense Category
**Endpoint:** `POST /api/expenses/categories`

**Request:**
```json
{
  "name": "Communications",
  "nameAr": "الاتصالات",
  "description": "Phone and internet bills"
}
```
**Expected:** `201 Created` with new category id and `isActive: true`

---

### TC-EXP-CAT-002 — Get All Expense Categories
**Endpoint:** `GET /api/expenses/categories`

**Expected:** `200 OK` — list includes the 8 seeded categories plus any newly created ones. No soft-deleted categories returned.

---

### TC-EXP-CAT-003 — Delete Expense Category (Soft Delete)
**Setup:** Create a throwaway category first.

**Endpoint:** `DELETE /api/expenses/categories/{id}`

**Expected:** `204 No Content`. Category no longer appears in `GET /api/expenses/categories`.

---

## Module 2: Expenses

### TC-EXP-001 — Create Expense (No Vault)
**Endpoint:** `POST /api/expenses`

**Request:**
```json
{
  "categoryId": "c1000000-0000-0000-0000-000000000001",
  "costCenter": "CLINIC",
  "expenseDate": "2026-06-18",
  "amount": 750.00,
  "description": "Internet service monthly fee"
}
```
**Expected:** `201 Created` with `expenseNumber` matching pattern `EXP-2026-NNNNNN`.

---

### TC-EXP-002 — Create Expense with Vault Deduction
**Setup:** Vault `b0000000-0000-0000-0000-000000000001` has balance 50000.00.

**Endpoint:** `POST /api/expenses`

**Request:**
```json
{
  "categoryId": "c1000000-0000-0000-0000-000000000002",
  "costCenter": "ADMINISTRATION",
  "expenseDate": "2026-06-18",
  "amount": 1500.00,
  "description": "Office supplies purchase",
  "vaultId": "b0000000-0000-0000-0000-000000000001"
}
```
**Expected:** `201 Created`. Vault balance decreased by 1500.00. A `vault_transactions` entry created with `direction=out`.

---

### TC-EXP-003 — Create Expense: Invalid Cost Center
**Endpoint:** `POST /api/expenses`

**Request:**
```json
{
  "categoryId": "c1000000-0000-0000-0000-000000000001",
  "costCenter": "INVALID_CENTER",
  "expenseDate": "2026-06-18",
  "amount": 100.00,
  "description": "Test"
}
```
**Expected:** `400 Bad Request` with validation error on `costCenter`.

---

### TC-EXP-004 — Create Expense: Negative Amount
**Endpoint:** `POST /api/expenses`

**Request:**
```json
{
  "categoryId": "c1000000-0000-0000-0000-000000000001",
  "costCenter": "GENERAL",
  "expenseDate": "2026-06-18",
  "amount": -50.00,
  "description": "Test negative"
}
```
**Expected:** `400 Bad Request` with validation error on `amount`.

---

### TC-EXP-005 — Get Expense List
**Endpoint:** `GET /api/expenses?pageNumber=1&pageSize=10`

**Expected:** `200 OK` with paginated list. Total includes 4 seeded expenses + any created.

---

### TC-EXP-006 — Get Expense by ID
**Setup:** Use an expense ID from seed data (e.g., `i1000000-0000-0000-0000-000000000001`).

**Endpoint:** `GET /api/expenses/{id}`

**Expected:** `200 OK` with full expense detail including `expenseNumber: EXP-2026-000001`.

---

### TC-EXP-007 — Get Expense PDF Voucher
**Endpoint:** `GET /api/expenses/{id}/pdf`

**Expected:** `200 OK` with `Content-Type: application/pdf`. Response body is a valid PDF byte stream.

---

### TC-EXP-008 — Delete Expense (Soft Delete)
**Setup:** Create a test expense first (TC-EXP-001).

**Endpoint:** `DELETE /api/expenses/{id}`

**Expected:** `204 No Content`. Expense no longer appears in `GET /api/expenses` list.

---

## Module 3: Expense Templates

### TC-EXP-TPL-001 — Create Expense Template
**Endpoint:** `POST /api/expenses/templates`

**Request:**
```json
{
  "name": "Monthly Rent Template",
  "categoryId": "c1000000-0000-0000-0000-000000000004",
  "costCenter": "ADMINISTRATION",
  "defaultAmount": 8500.00,
  "notes": "Due on 1st of every month"
}
```
**Expected:** `201 Created`.

---

### TC-EXP-TPL-002 — Get All Templates
**Endpoint:** `GET /api/expenses/templates`

**Expected:** `200 OK` — includes the template created above.

---

## Module 4: Asset Categories

### TC-AST-CAT-001 — Create Asset Category
**Endpoint:** `POST /api/assets/categories`

**Request:**
```json
{
  "name": "Medical Instruments",
  "nameAr": "الأدوات الطبية",
  "description": "Precision medical instruments"
}
```
**Expected:** `201 Created` with `isActive: true`.

---

### TC-AST-CAT-002 — Get Asset Categories
**Endpoint:** `GET /api/assets/categories`

**Expected:** `200 OK` — list includes all 6 seeded categories.

---

## Module 5: Assets

### TC-AST-001 — Create Asset
**Endpoint:** `POST /api/assets`

**Request:**
```json
{
  "name": "Dental Chair #3",
  "categoryId": "d1000000-0000-0000-0000-000000000001",
  "costCenter": "CLINIC",
  "purchaseDate": "2026-06-18",
  "purchaseCost": 30000.00,
  "location": "Clinic Room 3"
}
```
**Expected:** `201 Created` with `assetTag` matching pattern `AST-NNNNNN`, `status: Active`.

---

### TC-AST-002 — Create Asset Without Purchase Date
**Endpoint:** `POST /api/assets`

**Request:**
```json
{
  "name": "Spare X-Ray Sensor",
  "categoryId": "d1000000-0000-0000-0000-000000000004",
  "costCenter": "RADIOLOGY",
  "purchaseCost": 0
}
```
**Expected:** `201 Created` with `purchaseDate: null`.

---

### TC-AST-003 — Get Assets List (Paginated)
**Endpoint:** `GET /api/assets?pageNumber=1&pageSize=10`

**Expected:** `200 OK` — includes 5 seeded assets + any created. `status: UnderMaintenance` asset visible.

---

### TC-AST-004 — Get Asset by ID
**Endpoint:** `GET /api/assets/{id}`

**Expected:** `200 OK` with full detail including category name and cost center.

---

### TC-AST-005 — Get Asset by Tag
**Endpoint:** `GET /api/assets/by-tag/AST-000001`

**Expected:** `200 OK` — returns Dental Unit Chair #1.

---

### TC-AST-006 — Get Asset by Tag: Not Found
**Endpoint:** `GET /api/assets/by-tag/AST-999999`

**Expected:** `404 Not Found`.

---

### TC-AST-007 — Duplicate Asset Tag Prevention
**Setup:** Note the asset tag from TC-AST-001 (e.g., `AST-000006`).

**Endpoint:** Attempt two simultaneous creates and confirm no duplicate tags are issued.

**Expected:** Each asset gets a unique `AST-NNNNNN` tag.

---

### TC-AST-008 — Update Asset
**Endpoint:** `PUT /api/assets/{id}`

**Request:**
```json
{
  "name": "Dental Chair #3 (Updated)",
  "location": "Clinic Room 4",
  "notes": "Moved from room 3"
}
```
**Expected:** `200 OK` with updated `name` and `location`. `updatedAt` timestamp updated.

---

### TC-AST-009 — Dispose Asset
**Setup:** Use TC-AST-001 asset (or seed asset in Active status).

**Endpoint:** `POST /api/assets/{id}/dispose`

**Request:**
```json
{
  "disposalNotes": "End of life — beyond repair"
}
```
**Expected:** `200 OK` with `status: Disposed`. Asset soft-deleted (no longer in list).

---

### TC-AST-010 — Cannot Dispose Already-Disposed Asset
**Setup:** Use the disposed asset from TC-AST-009.

**Endpoint:** `POST /api/assets/{id}/dispose`

**Expected:** `400 Bad Request` — "Asset is already disposed".

---

## Module 6: Asset Documents

### TC-AST-DOC-001 — Upload Asset Document
**Endpoint:** `POST /api/assets/{id}/documents`

**Request:** Multipart form-data with:
- `file` — any PDF or image
- `documentType` — `"Invoice"`
- `notes` — `"Purchase invoice"`

**Expected:** `201 Created` with `fileName`, `fileKey`, `documentType`.

> Note: In DEV without S3/MinIO, the file will not persist to storage but the metadata record will be created.

---

### TC-AST-DOC-002 — List Asset Documents
**Endpoint:** `GET /api/assets/{id}/documents`

**Expected:** `200 OK` — returns list of documents including the one uploaded above. Each document has a `url` field (may be null in DEV without file storage).

---

## Module 7: Asset Maintenance (Cross-Module)

### TC-AST-MNT-001 — Create Maintenance Record
**Setup:** Use an Active asset (e.g., AST-000001).

**Endpoint:** `POST /api/assets/{id}/maintenance`

**Request:**
```json
{
  "maintenanceDate": "2026-06-18",
  "description": "Annual compressor service",
  "cost": 1200.00,
  "vendor": "DentalTech Services",
  "categoryId": "c1000000-0000-0000-0000-000000000003",
  "costCenter": "CLINIC"
}
```
**Expected:**
- `201 Created` with maintenance record
- Asset status changes to `UnderMaintenance`
- An expense is **automatically created** in the Expenses module with `relatedModule: Asset`
- Maintenance record has `expenseId` populated

---

### TC-AST-MNT-002 — Verify Auto-Created Expense
**Setup:** After TC-AST-MNT-001.

**Endpoint:** `GET /api/expenses?relatedModule=Asset`

**Expected:** Expense exists with amount matching maintenance cost (1200.00).

---

### TC-AST-MNT-003 — Restore Asset After Maintenance
**Setup:** After TC-AST-MNT-001 (asset is `UnderMaintenance`).

**Endpoint:** `POST /api/assets/{id}/restore`

**Expected:** `200 OK` with `status: Active`.

---

### TC-AST-MNT-004 — Cannot Maintain Disposed Asset
**Setup:** Use disposed asset from TC-AST-009.

**Endpoint:** `POST /api/assets/{id}/maintenance`

**Expected:** `400 Bad Request` — "Cannot create maintenance for a disposed asset".

---

### TC-AST-MNT-005 — Get Maintenance History
**Endpoint:** `GET /api/assets/{id}/maintenance`

**Expected:** `200 OK` — list of maintenance records in descending date order.

---

## Module 8: End-to-End Workflow

### TC-E2E-001 — Full Clinic Operations Cycle

**Goal:** Validate that all modules work together in a realistic clinic workflow.

**Steps:**

1. **Procurement:** `POST /api/purchase-orders` — create PO for Composite Resin A2 (10 units × 45.00 = 450.00)
2. **Receive:** Approve and receive the PO → stock increases
3. **Purchase Return:** Create purchase return for 2 defective units → stock decreases, credit note
4. **Operating Expense:** `POST /api/expenses` — create monthly rent expense (8500.00, cost center: ADMINISTRATION)
5. **Asset Registration:** `POST /api/assets` — register new autoclave (8500.00, cost center: CLINIC)
6. **Asset Maintenance:** `POST /api/assets/{id}/maintenance` — service the autoclave (500.00, vendor: XYZ Service)
7. **Verify:** Asset is `UnderMaintenance`, maintenance expense auto-created
8. **Restore:** `POST /api/assets/{id}/restore` — asset back to `Active`

**Expected:** All 8 steps complete with no errors. Data consistent across modules.

---

### TC-E2E-002 — Vault-Integrated Expense Workflow

**Goal:** Validate vault balance is atomically decremented on expense creation.

**Steps:**

1. Query vault balance: `GET /api/vaults/b0000000-0000-0000-0000-000000000001`
2. Note initial balance (should be ≥ 50000)
3. Create expense with vault: `POST /api/expenses` (amount: 2000.00, vaultId: main vault)
4. Query vault balance again
5. Verify balance decreased by exactly 2000.00
6. Verify `vault_transactions` has a new `out` entry for 2000.00

**Expected:** Balance = Initial − 2000.00. No partial writes.

---

## Authorization Tests

### TC-AUTH-001 — Unauthenticated Request Rejected
**Endpoint:** `GET /api/expenses` (no Authorization header)

**Expected:** `401 Unauthorized`

---

### TC-AUTH-002 — Invalid Token Rejected
**Endpoint:** `GET /api/assets` with `Authorization: Bearer invalidtoken`

**Expected:** `401 Unauthorized`

---

## Error Handling Tests

### TC-ERR-001 — Expense with Non-Existent Category
**Endpoint:** `POST /api/expenses`
```json
{ "categoryId": "00000000-0000-0000-0000-000000000000", "costCenter": "GENERAL", "expenseDate": "2026-06-18", "amount": 100.00, "description": "Test" }
```
**Expected:** `404 Not Found` — "Expense category not found"

---

### TC-ERR-002 — Asset with Non-Existent Category
**Endpoint:** `POST /api/assets`
```json
{ "name": "Test", "categoryId": "00000000-0000-0000-0000-000000000000", "costCenter": "GENERAL" }
```
**Expected:** `404 Not Found` — "Asset category not found"

---

### TC-ERR-003 — Get Non-Existent Asset
**Endpoint:** `GET /api/assets/00000000-0000-0000-0000-000000000000`

**Expected:** `404 Not Found`

---

## Test Results Summary Template

| TC ID | Description | Result | Notes |
|---|---|---|---|
| TC-EXP-CAT-001 | Create Expense Category | | |
| TC-EXP-CAT-002 | Get All Categories | | |
| TC-EXP-CAT-003 | Soft Delete Category | | |
| TC-EXP-001 | Create Expense (No Vault) | | |
| TC-EXP-002 | Create Expense with Vault | | |
| TC-EXP-003 | Invalid Cost Center | | |
| TC-EXP-004 | Negative Amount | | |
| TC-EXP-005 | Get Expense List | | |
| TC-EXP-006 | Get Expense by ID | | |
| TC-EXP-007 | PDF Voucher | | |
| TC-EXP-008 | Delete Expense | | |
| TC-EXP-TPL-001 | Create Template | | |
| TC-EXP-TPL-002 | Get Templates | | |
| TC-AST-CAT-001 | Create Asset Category | | |
| TC-AST-CAT-002 | Get Asset Categories | | |
| TC-AST-001 | Create Asset | | |
| TC-AST-002 | Create Asset (No Purchase Date) | | |
| TC-AST-003 | Get Assets List | | |
| TC-AST-004 | Get Asset by ID | | |
| TC-AST-005 | Get Asset by Tag | | |
| TC-AST-006 | Tag Not Found | | |
| TC-AST-007 | Duplicate Tag Prevention | | |
| TC-AST-008 | Update Asset | | |
| TC-AST-009 | Dispose Asset | | |
| TC-AST-010 | Double Dispose Guard | | |
| TC-AST-DOC-001 | Upload Document | | |
| TC-AST-DOC-002 | List Documents | | |
| TC-AST-MNT-001 | Create Maintenance + Auto-Expense | | |
| TC-AST-MNT-002 | Verify Auto-Created Expense | | |
| TC-AST-MNT-003 | Restore After Maintenance | | |
| TC-AST-MNT-004 | No Maintenance on Disposed | | |
| TC-AST-MNT-005 | Maintenance History | | |
| TC-E2E-001 | Full Clinic Cycle | | |
| TC-E2E-002 | Vault-Integrated Expense | | |
| TC-AUTH-001 | Unauthenticated Rejected | | |
| TC-AUTH-002 | Invalid Token Rejected | | |
| TC-ERR-001 | Non-Existent Category | | |
| TC-ERR-002 | Asset Non-Existent Category | | |
| TC-ERR-003 | Non-Existent Asset | | |

**Total Scenarios:** 39
