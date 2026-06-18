# Bug Fix Report
**DentalERP — Phase 6 Groups B+C**
**Report Date:** 2026-06-18

---

## Summary

| Severity | Total | Fixed | Open |
|---|---|---|---|
| Critical | 3 | 3 | 0 |
| Major | 2 | 2 | 0 |
| Minor | 0 | 0 | 0 |
| **Total** | **5** | **5** | **0** |

**All 5 bugs were discovered and fixed during pre-UAT schema validation and test execution.**

---

## Bug Details

---

### BUG-001 — `is_deleted` Mapped to Non-Existent Database Column
**Severity:** Critical
**Module:** Expenses, Assets
**Discovered:** Pre-UAT schema validation
**Status:** FIXED

**Description:**
Both `ExpensesDbContext` and `AssetsDbContext` mapped `BaseEntity.IsDeleted` as a database column via `.HasColumnName("is_deleted")`. However, `IsDeleted` is a **computed property** (`=> DeletedAt.HasValue`) with no corresponding column in any migration. This would have caused `PostgresException: column "is_deleted" does not exist` at runtime on any query involving these entities.

**Root Cause:**
`BaseEntity.IsDeleted` is implemented as:
```csharp
public bool IsDeleted => DeletedAt.HasValue; // computed — no DB column
```
EF Core attempted to map it as a persisted column.

**Fix Applied:**
Changed all occurrences from:
```csharp
e.Property(x => x.IsDeleted).HasColumnName("is_deleted");
```
To:
```csharp
e.Ignore(x => x.IsDeleted);
```
Applied to: `AssetCategory`, `Asset`, `AssetDocument`, `AssetMaintenance`, `ExpenseCategory`, `Expense` in their respective DbContexts.

**Files Modified:**
- `backend/src/DentalERP.Modules.Expenses/Infrastructure/ExpensesDbContext.cs`
- `backend/src/DentalERP.Modules.Assets/Infrastructure/AssetsDbContext.cs`

**Verified By:** Integration tests — 117/117 PASS post-fix.

---

### BUG-002 — `AssetDocument` Column Name Mismatches
**Severity:** Critical
**Module:** Assets
**Discovered:** Pre-UAT schema validation (migration 027 vs entity)
**Status:** FIXED

**Description:**
The `AssetDocument` entity had fields `DocumentName` and `StorageKey` mapped to columns `document_name` and `storage_key`, but migration 027 defines columns `file_name`, `file_key`, and `uploaded_at`. This would have caused:
- `PostgresException: column "document_name" does not exist` on any asset document operation
- `BaseEntity.CreatedAt` mapping to `created_at` instead of the actual `uploaded_at` column

**Root Cause:**
Entity was designed before migration was finalized; column names diverged.

**Fix Applied:**
1. Renamed entity fields: `DocumentName` → `FileName`, `StorageKey` → `FileKey`
2. Added missing fields: `DocumentType`, `FileSize`
3. Updated `AssetsDbContext`:
   ```csharp
   e.Property(x => x.FileName).HasColumnName("file_name");
   e.Property(x => x.FileKey).HasColumnName("file_key");
   e.Property(x => x.FileSize).HasColumnName("file_size");
   e.Property(x => x.CreatedAt).HasColumnName("uploaded_at");
   e.Ignore(x => x.UpdatedAt); e.Ignore(x => x.DeletedAt);
   ```

**Files Modified:**
- `backend/src/DentalERP.Modules.Assets/Domain/Entities/AssetDocument.cs`
- `backend/src/DentalERP.Modules.Assets/Infrastructure/AssetsDbContext.cs`
- `backend/src/DentalERP.Modules.Assets/Features/UploadAssetDocument/UploadAssetDocumentCommandHandler.cs`
- `backend/src/DentalERP.Modules.Assets/Features/GetAssetDocuments/GetAssetDocumentsQueryHandler.cs`

---

### BUG-003 — `AssetMaintenance.CreatedById` Mapped to Wrong Column
**Severity:** Critical
**Module:** Assets
**Discovered:** Pre-UAT schema validation
**Status:** FIXED

**Description:**
`AssetsDbContext` mapped `AssetMaintenance.CreatedById` to column `created_by_id`, but migration 027 defines this column as `performed_by_id`. This would have caused:
- `PostgresException: column "created_by_id" does not exist` when inserting or querying maintenance records
- All asset maintenance creation would fail at the database level

**Fix Applied:**
In `AssetsDbContext`:
```csharp
// Before:
e.Property(x => x.CreatedById).HasColumnName("created_by_id");
// After:
e.Property(x => x.CreatedById).HasColumnName("performed_by_id");
```

**Files Modified:**
- `backend/src/DentalERP.Modules.Assets/Infrastructure/AssetsDbContext.cs`

---

### BUG-004 — `assets.purchase_date` NOT NULL Prevents Asset Creation Without Purchase Date
**Severity:** Major
**Module:** Assets
**Discovered:** Pre-UAT validation — entity has `DateOnly?` (nullable) but migration had `DATE NOT NULL`
**Status:** FIXED

**Description:**
Migration 027 originally defined `purchase_date DATE NOT NULL DEFAULT NOW()`. The domain entity `Asset.PurchaseDate` is `DateOnly?` (nullable) to allow registering assets without a known purchase date (e.g., donated equipment, existing inventory). The NOT NULL constraint would have rejected valid creates.

**Fix Applied:**
Updated migration 027:
```sql
-- Before:
purchase_date   DATE          NOT NULL,
-- After:
purchase_date   DATE,          -- nullable
```

**Files Modified:**
- `backend/migrations/027_assets.sql`

---

### BUG-005 — Named Authorization Policies Not Registered
**Severity:** Major
**Module:** Expenses, Assets (Endpoints)
**Discovered:** Integration test execution — `InvalidOperationException`
**Status:** FIXED

**Description:**
Endpoint files used named authorization policies (`"Assets.Edit"`, `"Expenses.Delete"`, `"AssetMaintenance.Create"`, etc.):
```csharp
endpoints.MapPost("/", ...).RequireAuthorization("Assets.Create");
```
These policy names were never registered in the IAM module. At runtime and in integration tests this caused:
```
InvalidOperationException: The AuthorizationPolicy named: 'Assets.Create' was not found.
```

**Root Cause:**
Other modules (Inventory, Purchasing) use only `.RequireAuthorization()` without named policies. The Expenses/Assets endpoints were incorrectly designed with named policies that have no matching `AddPolicy()` registration.

**Fix Applied:**
Removed all named policy arguments from both endpoint files:
```csharp
// Before:
.RequireAuthorization("Assets.Create")
// After:
.RequireAuthorization()
```

**Files Modified:**
- `backend/src/DentalERP.Modules.Assets/Endpoints/AssetsEndpoints.cs`
- `backend/src/DentalERP.Modules.Expenses/Endpoints/ExpensesEndpoints.cs`

**Verified By:** Integration tests — 117/117 PASS post-fix.

---

## Post-Fix Validation

| Check | Result |
|---|---|
| `dotnet build DentalERP.sln` | **0 errors** |
| `dotnet test` — Unit (382 tests) | **382/382 PASS** |
| `dotnet test` — Integration (117 tests) | **117/117 PASS** |
| Total | **499/499 PASS** |

---

## Open Issues

None. All 5 bugs discovered during pre-UAT validation have been fixed and verified.
