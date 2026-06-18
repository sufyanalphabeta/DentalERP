# GROUP C COMPLETION REPORT
**Phase 6 — Group C: Expenses + Assets + Cost Centers**
**Date:** 2026-06-18
**Status:** COMPLETE

---

## Summary

Group C implements the full Expenses module, Assets module, and lightweight Cost Centers. Both modules feature soft-delete, vault integration, audit logging, and QuestPDF PDF exports.

---

## Cost Centers (Lightweight)

Seeded in migration `026_expenses.sql`. Values:

| Code | Description |
|---|---|
| GENERAL | General/unallocated |
| CLINIC | Dental clinic operations |
| LABORATORY | Lab operations |
| RADIOLOGY | Radiology operations |
| TRAINING | Training & education |
| ADMINISTRATION | Administrative |

Used as a string column on both `expenses` and as a query/filter dimension.

---

## Expenses Module (`DentalERP.Modules.Expenses`)

### Entities
| Entity | Soft-Delete | Key Fields |
|---|---|---|
| `ExpenseCategory` | ✅ | Name, NameAr, Description, IsActive |
| `ExpenseTemplate` | No | Name, CategoryId, CostCenter, DefaultAmount |
| `Expense` | ✅ | ExpenseNumber (EXP-YYYY-NNNNNN), Amount, ExpenseDate, CostCenter, RelatedModule, RelatedEntityId, VaultId, AttachmentKey |

### Business Rules
- **Vault Integration**: Every expense with a VaultId creates a `vault_transactions` record (`general_payment`, direction=`out`) atomically in the same `SaveChangesAsync`
- **Expense Number Format**: `EXP-{Year}-{NNNNNN}`
- **Soft Delete**: `HasQueryFilter(x => x.DeletedAt == null)` on `ExpenseCategory` and `Expense`
- **Audit Logs**: Created/Updated/Deleted actions logged to `audit_logs`

### API Endpoints
| Method | Route | Description |
|---|---|---|
| GET | `/api/expenses/categories` | List expense categories |
| POST | `/api/expenses/categories` | Create expense category |
| GET | `/api/expenses/` | List expenses (paginated, filterable) |
| GET | `/api/expenses/{id}` | Get expense detail |
| POST | `/api/expenses/` | Create expense (+ atomic vault deduction) |
| PUT | `/api/expenses/{id}` | Update expense |
| DELETE | `/api/expenses/{id}` | Soft-delete expense |
| GET | `/api/expenses/report/pdf` | Expense Report PDF (QuestPDF) |

### Filters (GET /api/expenses/)
- `costCenter` — filter by cost center code
- `categoryId` — filter by category
- `dateFrom` / `dateTo` — date range
- `relatedModule` — filter by originating module
- `page` / `pageSize` — pagination

---

## Assets Module (`DentalERP.Modules.Assets`)

### Entities
| Entity | Soft-Delete | Key Fields |
|---|---|---|
| `AssetCategory` | ✅ | Name, NameAr, Description, IsActive |
| `Asset` | ✅ | AssetTag (AST-NNNNNN, UNIQUE), Name, Status (Active/UnderMaintenance/Disposed), PurchaseCost, Location |
| `AssetDocument` | No | StorageKey, ContentType, UploadedById |
| `AssetMaintenance` | No | MaintenanceDate, Cost, Description, Vendor, ExpenseId (FK → expenses) |

### Asset Status Transitions
```
Active ←→ UnderMaintenance
Active → Disposed (soft-delete, irreversible)
UnderMaintenance → Disposed (soft-delete, irreversible)
```

### Business Rules
- **Asset Tag Format**: `AST-{NNNNNN}` — unique index enforced
- **Asset Documents**: Use `IFileStorageService` for S3-compatible storage (`asset-documents` bucket)
- **Asset Maintenance → Auto-Creates Expense**: `CreateAssetMaintenance` dispatches `CreateExpenseCommand` via MediatR with `RelatedModule="Asset"`, then links `ExpenseId` back to the maintenance record. Asset status automatically set to `UnderMaintenance`.
- **Soft Delete + Dispose**: `Asset.Dispose()` sets status=`Disposed` and calls `SoftDelete()`
- **Audit Logs**: Created/Updated/Disposed/Maintenance actions logged to `audit_logs`

### API Endpoints
| Method | Route | Description |
|---|---|---|
| GET | `/api/assets/categories` | List asset categories |
| POST | `/api/assets/categories` | Create asset category |
| GET | `/api/assets/` | List assets (paginated, filterable) |
| GET | `/api/assets/{id}` | Get asset detail |
| GET | `/api/assets/by-tag/{tag}` | Get asset by tag (e.g. AST-000001) |
| POST | `/api/assets/` | Create asset |
| PUT | `/api/assets/{id}` | Update asset |
| POST | `/api/assets/{id}/dispose` | Dispose asset |
| GET | `/api/assets/{id}/documents` | List documents (with presigned URLs) |
| POST | `/api/assets/{id}/documents` | Upload document (multipart) |
| GET | `/api/assets/{id}/maintenances` | List maintenance records |
| POST | `/api/assets/{id}/maintenances` | Create maintenance (auto-creates expense) |
| GET | `/api/assets/register/pdf` | Asset Register PDF (QuestPDF, A4 Landscape) |

---

## Migrations

| Migration | Description |
|---|---|
| `026_expenses.sql` | audit_logs, cost_centers (seeded), expense_categories, expense_templates, expenses |
| `027_assets.sql` | asset_categories, assets (asset_tag UNIQUE), asset_documents, asset_maintenance (expense_id FK) |

---

## PDF Exports (QuestPDF 2024.12.0, Community License)
| Document | Endpoint | Format |
|---|---|---|
| Expense Report | GET /api/expenses/report/pdf | A4 Portrait, filterable by date/cost center/category |
| Asset Register | GET /api/assets/register/pdf | A4 Landscape, filterable by category/status |
| Purchase Return Voucher | GET /api/purchasing/purchase-returns/{id}/voucher | A4 Portrait (Group B) |

---

## Host Registration

```csharp
// Program.cs additions
builder.Services.AddExpensesModule(builder.Configuration);
builder.Services.AddAssetsModule(builder.Configuration);
// ...
app.MapExpensesModule();
app.MapAssetsModule();
```

---

## Test Results

| Suite | Tests |
|---|---|
| ExpenseCategoryTests | 10 unit tests |
| ExpenseTests | 16 unit tests (incl. Theory × 6 cost centers) |
| AssetCategoryTests | 7 unit tests |
| AssetTests | 14 unit tests |
| AssetMaintenanceTests | 8 unit tests |
| ExpensesEndpointTests | 9 integration tests (auth guards) |
| AssetsEndpointTests | 13 integration tests (auth guards) |

---

## Build & Test Summary

| Metric | Value |
|---|---|
| Build Errors | **0** |
| Build Warnings | 10 (pre-existing NU1603 only) |
| Unit Tests | **382 PASS** |
| Integration Tests | **117 PASS** |
| **TOTAL TESTS** | **499 PASS** |

---

## Permissions Defined (to be wired to IAM in future sprint)
- `Expenses.View`, `Expenses.Create`, `Expenses.Edit`, `Expenses.Delete`
- `Assets.View`, `Assets.Create`, `Assets.Edit`, `Assets.Delete`
- `AssetMaintenance.Manage`

---

## STOP — Awaiting Approval

Per the approved scope, work stops here. Do NOT start Group D (Analytics).

**Next step:** DEV Deployment / End-to-End Testing / UAT approval before Analytics.
