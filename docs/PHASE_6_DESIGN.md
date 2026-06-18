# PHASE 6 DESIGN DOCUMENT
# DentalERP — Inventory, Expenses, Assets & Analytics

**Version:** 1.0  
**Date:** 2026-06-17  
**Status:** PENDING APPROVAL  
**Prerequisite:** Phase 5 Approved ✓

---

## Table of Contents

1. [Phase Overview](#1-phase-overview)
2. [Module Catalog](#2-module-catalog)
3. [Database Design — Full ERD](#3-database-design--full-erd)
4. [Business Rules](#4-business-rules)
5. [Screen Inventory S30–S47](#5-screen-inventory-s30s47)
6. [API Contracts](#6-api-contracts)
7. [Integration Map](#7-integration-map)
8. [Reporting Requirements](#8-reporting-requirements)
9. [Migration Plan (023–028)](#9-migration-plan-023028)
10. [Implementation Timeline](#10-implementation-timeline)

---

## 1. Phase Overview

Phase 6 closes the operational loop of DentalERP V1. It adds supply chain (Inventory, Suppliers, Purchasing, Returns), financial operations (Expenses), physical assets management, and the analytics layer that makes all prior data actionable.

### V1 Completion Status After Phase 6

| Domain | Status |
|--------|--------|
| Patient Management | ✓ Phase 2 |
| Clinical | ✓ Phase 3 |
| Financial — Invoicing & Treasury | ✓ Phase 4 |
| Laboratory & Radiology | ✓ Phase 5 |
| Insurance Accounts | ✓ Phase 5 |
| **Inventory & Supply Chain** | **Phase 6** |
| **Expenses** | **Phase 6** |
| **Assets & Documents** | **Phase 6** |
| **Dashboard & Analytics** | **Phase 6** |

---

## 2. Module Catalog

### 2.1 Inventory Management
Track all consumable items used in the clinic — dental supplies, radiology films, lab materials, sterilization products. Supports per-batch expiry tracking, reorder alerts, and multi-warehouse/location.

### 2.2 Suppliers
Master data for all vendors. Supplier account balance (payables) computed from GoodsReceipts minus supplier payments. Extensible: same table used later for recurring supplier relationships (insurance companies already exist in Financial module separately).

### 2.3 Purchasing
Full PO workflow: Draft → Approved → Sent → PartiallyReceived → FullyReceived → Closed. Goods Receipts create stock batches and supplier payable entries.

### 2.4 Purchase Returns
Return items to supplier with reason and debit note. Reduces supplier payable balance. Triggers negative stock movement.

### 2.5 Stock Movements
Unified ledger of all inventory changes. Movement types: PurchaseReceipt, ManualIssue, LabConsumption, RadiologyConsumption, Adjustment, WriteOff, SupplierReturn, Transfer.

### 2.6 Expiry Tracking
Batch-level expiry dates. System alerts at 90 / 60 / 30 days before expiry. FIFO issuing by default.

### 2.7 Reorder Levels
Per-item minimum stock threshold. Alert generated when current stock ≤ reorder_level. Dashboard widget shows items needing reorder.

### 2.8 Expenses Management
Record clinic operating expenses (rent, utilities, staff salaries, maintenance). Always linked to a Treasury vault (deduction posted as VaultTransaction). Supports recurring expense templates.

### 2.9 Assets & Documents
Physical asset register (dental chairs, X-ray machines, sterilizers, computers). Depreciation tracking (straight-line or declining balance). Document storage for warranties, maintenance records, calibration certificates using IFileStorageService.

### 2.10 Executive Dashboard
Aggregated KPI view: revenue, collections, lab & radiology throughput, insurance receivables, expense burn rate, stock valuation, low-stock alerts, expiry alerts.

### 2.11 Operational Analytics
Drill-down analytics: patient flow, appointment utilization, treatment completion rates, revenue by service/doctor/period, lab & radiology order velocity, top procedures, doctor performance comparison.

---

## 3. Database Design — Full ERD

### 3.1 Inventory Tables

#### `item_categories`
```
id              UUID PK
name            VARCHAR(150) NOT NULL
name_ar         VARCHAR(150)
parent_id       UUID FK → item_categories(id) NULL  -- supports hierarchy
is_active       BOOLEAN DEFAULT TRUE
created_at      TIMESTAMPTZ
```

#### `units_of_measure`
```
id              UUID PK
name            VARCHAR(50) NOT NULL          -- "Piece", "Box", "Milliliter"
name_ar         VARCHAR(50)                   -- "قطعة", "علبة", "مل"
abbreviation    VARCHAR(10)                   -- "pcs", "box", "mL"
```

#### `items`
```
id              UUID PK
item_code       VARCHAR(30) UNIQUE NOT NULL   -- AUTO: ITM-000001
name            VARCHAR(200) NOT NULL
name_ar         VARCHAR(200)
category_id     UUID FK → item_categories(id)
unit_of_measure_id UUID FK → units_of_measure(id)
unit_cost       DECIMAL(10,2)                 -- last known cost
reorder_level   DECIMAL(10,3) DEFAULT 0       -- alert threshold
reorder_quantity DECIMAL(10,3) DEFAULT 0      -- suggested order qty
is_expiry_tracked BOOLEAN DEFAULT FALSE
is_active       BOOLEAN DEFAULT TRUE
notes           TEXT
created_at      TIMESTAMPTZ
```

#### `warehouses`
```
id              UUID PK
name            VARCHAR(150) NOT NULL
name_ar         VARCHAR(150)
location        VARCHAR(300)
is_default      BOOLEAN DEFAULT FALSE
is_active       BOOLEAN DEFAULT TRUE
```
> Note: Single-clinic default. Multi-location ready.

#### `stock_batches`
```
id              UUID PK
item_id         UUID FK → items(id)
warehouse_id    UUID FK → warehouses(id)
batch_number    VARCHAR(100)
quantity        DECIMAL(10,3) NOT NULL DEFAULT 0
unit_cost       DECIMAL(10,2) NOT NULL
expiry_date     DATE NULL
received_date   DATE NOT NULL
is_depleted     BOOLEAN DEFAULT FALSE
```
> Index on (item_id, warehouse_id, expiry_date) for FIFO queries.

#### `stock_movements`
```
id              UUID PK
movement_number VARCHAR(30) UNIQUE
item_id         UUID FK → items(id)
warehouse_id    UUID FK → warehouses(id)
batch_id        UUID FK → stock_batches(id) NULL
movement_type   VARCHAR(40) NOT NULL
  -- Values: PurchaseReceipt | ManualIssue | LabConsumption | RadiologyConsumption
  --         Adjustment | WriteOff | SupplierReturn | Transfer
direction       VARCHAR(3) NOT NULL          -- 'in' | 'out'
quantity        DECIMAL(10,3) NOT NULL
unit_cost       DECIMAL(10,2)
total_cost      DECIMAL(12,2)
reference_id    UUID NULL                    -- PO, LabOrder, RadiologyOrder, etc.
reference_type  VARCHAR(50) NULL             -- 'PurchaseReceipt' | 'LabOrder' | ...
notes           TEXT
created_by_id   UUID NULL
created_at      TIMESTAMPTZ
```

---

### 3.2 Supplier Tables

#### `suppliers`
```
id              UUID PK
supplier_code   VARCHAR(30) UNIQUE            -- AUTO: SUP-000001
name            VARCHAR(200) NOT NULL
name_ar         VARCHAR(200)
category        VARCHAR(50)                   -- Medical | Equipment | General | Lab | Radiology
contact_person  VARCHAR(200)
phone           VARCHAR(30)
email           VARCHAR(200)
address         TEXT
payment_terms_days INT DEFAULT 30
credit_limit    DECIMAL(12,2) DEFAULT 0
balance         DECIMAL(12,2) DEFAULT 0       -- payable balance (computed or maintained)
is_active       BOOLEAN DEFAULT TRUE
notes           TEXT
created_at      TIMESTAMPTZ
```

#### `supplier_payments`
```
id              UUID PK
payment_number  VARCHAR(30) UNIQUE            -- AUTO: SPAY-YYYY-000001
supplier_id     UUID FK → suppliers(id)
vault_id        UUID                          -- cross-module UUID ref (no EF FK)
amount          DECIMAL(12,2) NOT NULL
payment_date    TIMESTAMPTZ
reference_number VARCHAR(100)
notes           TEXT
paid_by_id      UUID NULL
created_at      TIMESTAMPTZ
```
> When posted: creates VaultTransaction (direction: out, type: payment_to_supplier) in Financial module via cross-module event.

---

### 3.3 Purchasing Tables

#### `purchase_orders`
```
id              UUID PK
po_number       VARCHAR(30) UNIQUE            -- AUTO: PO-YYYY-000001
supplier_id     UUID FK → suppliers(id)
status          VARCHAR(30) NOT NULL
  -- Draft | Approved | Sent | PartiallyReceived | FullyReceived | Closed | Cancelled
order_date      DATE NOT NULL
expected_date   DATE NULL
subtotal        DECIMAL(12,2) DEFAULT 0
discount_amount DECIMAL(12,2) DEFAULT 0
total_amount    DECIMAL(12,2) DEFAULT 0
notes           TEXT
approved_by_id  UUID NULL
approved_at     TIMESTAMPTZ NULL
created_by_id   UUID NULL
created_at      TIMESTAMPTZ
updated_at      TIMESTAMPTZ
```

#### `purchase_order_items`
```
id              UUID PK
po_id           UUID FK → purchase_orders(id)
item_id         UUID FK → items(id)
quantity_ordered DECIMAL(10,3) NOT NULL
quantity_received DECIMAL(10,3) DEFAULT 0
unit_cost       DECIMAL(10,2) NOT NULL
total_cost      DECIMAL(12,2) NOT NULL
notes           TEXT
```

#### `goods_receipts`
```
id              UUID PK
gr_number       VARCHAR(30) UNIQUE            -- AUTO: GR-YYYY-000001
po_id           UUID FK → purchase_orders(id) NULL   -- may receive without PO
supplier_id     UUID FK → suppliers(id)
warehouse_id    UUID FK → warehouses(id)
receipt_date    DATE NOT NULL
invoice_reference VARCHAR(100)               -- supplier invoice number
notes           TEXT
received_by_id  UUID NULL
created_at      TIMESTAMPTZ
```

#### `goods_receipt_items`
```
id              UUID PK
gr_id           UUID FK → goods_receipts(id)
po_item_id      UUID FK → purchase_order_items(id) NULL
item_id         UUID FK → items(id)
batch_number    VARCHAR(100)
quantity        DECIMAL(10,3) NOT NULL
unit_cost       DECIMAL(10,2) NOT NULL
expiry_date     DATE NULL
```
> On save: creates stock_batch + stock_movement(PurchaseReceipt, in) + updates items.unit_cost + updates supplier.balance + updates po_items.quantity_received.

---

### 3.4 Purchase Returns Table

#### `purchase_returns`
```
id              UUID PK
return_number   VARCHAR(30) UNIQUE            -- AUTO: PR-YYYY-000001
supplier_id     UUID FK → suppliers(id)
po_id           UUID FK → purchase_orders(id) NULL
return_date     DATE NOT NULL
reason          TEXT NOT NULL
status          VARCHAR(20) NOT NULL          -- Draft | Confirmed | Credited
total_amount    DECIMAL(12,2) DEFAULT 0
notes           TEXT
created_by_id   UUID NULL
created_at      TIMESTAMPTZ
```

#### `purchase_return_items`
```
id              UUID PK
return_id       UUID FK → purchase_returns(id)
item_id         UUID FK → items(id)
batch_id        UUID FK → stock_batches(id) NULL
quantity        DECIMAL(10,3) NOT NULL
unit_cost       DECIMAL(10,2) NOT NULL
total_cost      DECIMAL(12,2) NOT NULL
```
> On confirm: creates stock_movement(SupplierReturn, out) + reduces supplier.balance.

---

### 3.5 Expense Tables

#### `expense_categories`
```
id              UUID PK
name            VARCHAR(150) NOT NULL
name_ar         VARCHAR(150)
is_active       BOOLEAN DEFAULT TRUE
```

#### `expense_templates`
```
id              UUID PK
name            VARCHAR(200) NOT NULL
category_id     UUID FK → expense_categories(id)
amount          DECIMAL(10,2)
vault_id        UUID                          -- default vault (cross-module ref)
vendor_name     VARCHAR(200)
recurrence      VARCHAR(20)                   -- Monthly | Weekly | Quarterly | Annually
notes           TEXT
is_active       BOOLEAN DEFAULT TRUE
```

#### `expenses`
```
id              UUID PK
expense_number  VARCHAR(30) UNIQUE            -- AUTO: EXP-YYYY-000001
category_id     UUID FK → expense_categories(id)
template_id     UUID FK → expense_templates(id) NULL
vault_id        UUID NOT NULL                 -- cross-module ref to Financial.Vaults
amount          DECIMAL(10,2) NOT NULL
expense_date    DATE NOT NULL
description     TEXT NOT NULL
vendor_name     VARCHAR(200)
reference_number VARCHAR(100)
approved_by_id  UUID NULL
created_by_id   UUID NULL
created_at      TIMESTAMPTZ
```
> On save: creates VaultTransaction (direction: out, type: general_payment) in Financial module via shared DB or domain event.

---

### 3.6 Asset Tables

#### `asset_categories`
```
id              UUID PK
name            VARCHAR(150) NOT NULL
name_ar         VARCHAR(150)
```

#### `assets`
```
id              UUID PK
asset_code      VARCHAR(30) UNIQUE            -- AUTO: AST-000001
name            VARCHAR(200) NOT NULL
name_ar         VARCHAR(200)
category_id     UUID FK → asset_categories(id)
serial_number   VARCHAR(100)
purchase_date   DATE NOT NULL
purchase_cost   DECIMAL(12,2) NOT NULL
current_value   DECIMAL(12,2) NOT NULL
depreciation_method VARCHAR(20)              -- StraightLine | DecliningBalance | None
depreciation_rate DECIMAL(5,2) DEFAULT 0     -- annual %
location        VARCHAR(200)
supplier_id     UUID FK → suppliers(id) NULL
warranty_expiry DATE NULL
status          VARCHAR(30) NOT NULL          -- Active | UnderMaintenance | Disposed
notes           TEXT
created_at      TIMESTAMPTZ
updated_at      TIMESTAMPTZ
```

#### `asset_documents`
```
id              UUID PK
asset_id        UUID FK → assets(id)
document_type   VARCHAR(50)                   -- Invoice | Warranty | Maintenance | Calibration | Manual | Other
storage_bucket  VARCHAR(200) NOT NULL         -- IFileStorageService pattern
storage_key     VARCHAR(500) NOT NULL
file_name       VARCHAR(300) NOT NULL
file_size       BIGINT
notes           TEXT
uploaded_by_id  UUID
uploaded_at     TIMESTAMPTZ
```

#### `asset_maintenance_logs`
```
id              UUID PK
asset_id        UUID FK → assets(id)
maintenance_date DATE NOT NULL
description     TEXT NOT NULL
cost            DECIMAL(10,2) DEFAULT 0
performed_by    VARCHAR(200)
next_maintenance_date DATE NULL
created_at      TIMESTAMPTZ
```

---

### 3.7 ERD Relationships Summary

```
item_categories ─< items >─ units_of_measure
items ─< stock_batches >─ warehouses
items ─< stock_movements >─ warehouses
items ─< purchase_order_items >─ purchase_orders >─ suppliers
purchase_orders ─< goods_receipts
goods_receipts ─< goods_receipt_items >─ items
suppliers ─< purchase_returns
purchase_returns ─< purchase_return_items >─ items
suppliers ─< supplier_payments
expense_categories ─< expenses
expense_categories ─< expense_templates ─< expenses
asset_categories ─< assets >─ suppliers
assets ─< asset_documents
assets ─< asset_maintenance_logs

Cross-module references (UUID only, no FK):
expenses.vault_id → Financial.Vaults
supplier_payments.vault_id → Financial.Vaults
stock_movements.reference_id → [LabOrder | RadiologyOrder | PurchaseOrder]
```

---

## 4. Business Rules

### 4.1 Inventory Rules

**BR-INV-01: Stock Cannot Go Negative**
- On any `out` movement, validate: current_stock >= quantity_requested.
- Exception: `Adjustment` movements may override with explicit `allow_negative` flag.

**BR-INV-02: FIFO Batch Selection**
- When issuing stock, system selects batches ordered by `expiry_date ASC NULLS LAST`, then by `received_date ASC`.
- Batch with earliest expiry is consumed first.

**BR-INV-03: Reorder Alert Trigger**
- After every `out` movement, check: if `current_stock(item, warehouse) <= item.reorder_level`, generate a reorder alert record.
- Alert is dismissed when new stock arrives or user acknowledges.

**BR-INV-04: Expiry Alerts**
- Daily job (or on-demand): flag all batches where `expiry_date <= TODAY + 90 days` and `quantity > 0`.
- Three levels: `Critical` (≤ 30 days), `Warning` (31–60 days), `Notice` (61–90 days).

**BR-INV-05: Item Code Auto-Generation**
- Format: `ITM-{6-digit-sequence}` (e.g., `ITM-000001`).

**BR-INV-06: Movement Number Auto-Generation**
- Format: `MOV-YYYY-{6-digit}`.

### 4.2 Purchasing Rules

**BR-PO-01: PO Approval Required Before Sending**
- PO can only transition `Draft → Approved` by an authorized user.
- PO can only transition `Approved → Sent` after approval.

**BR-PO-02: PO Status Machine**
```
Draft → Approved → Sent → PartiallyReceived → FullyReceived → Closed
      ↓             ↓
  Cancelled      Cancelled (if nothing received)
```

**BR-PO-03: Goods Receipt Updates PO Progress**
- On GR save: update `po_items.quantity_received += gr_item.quantity`.
- If all items fully received: PO → `FullyReceived`.
- If partial: PO → `PartiallyReceived`.

**BR-PO-04: Supplier Balance on GR**
- On GR confirmation: `supplier.balance += goods_receipt.total_amount`.
- This represents an accounts-payable entry.

**BR-PO-05: PO Number Format** — `PO-YYYY-000001`  
**BR-PO-06: GR Number Format** — `GR-YYYY-000001`

### 4.3 Purchase Return Rules

**BR-PR-01: Return Only Received Items**
- Can only return items that appear in a confirmed GoodsReceipt.
- Quantity returned cannot exceed quantity received in the original batch.

**BR-PR-02: Return Confirmation Reduces Supplier Balance**
- On confirm: `supplier.balance -= purchase_return.total_amount`.
- Creates stock_movement (SupplierReturn, out).

**BR-PR-03: Return Number Format** — `PR-YYYY-000001`

### 4.4 Expense Rules

**BR-EXP-01: Expense Always Debits a Vault**
- Every expense must reference a `vault_id`.
- On save: creates `VaultTransaction` (direction: `out`, type: `general_payment`) in Financial module.
- VaultTransaction is atomic with the expense record (same DB transaction).

**BR-EXP-02: Expense Categories Required**
- An uncategorized expense is not allowed. `category_id` is mandatory.

**BR-EXP-03: Recurring Expense Templates**
- Templates do not auto-post. They serve as defaults when creating an expense.
- User manually creates the expense from a template (or a future scheduled job).

**BR-EXP-04: Expense Number Format** — `EXP-YYYY-000001`

### 4.5 Asset Rules

**BR-AST-01: Asset Code Format** — `AST-000001` (no year prefix — assets are permanent records)

**BR-AST-02: Depreciation Computation**
- Straight-Line: `annual_depreciation = purchase_cost × (depreciation_rate / 100)`
- Declining Balance: `annual_depreciation = current_value × (depreciation_rate / 100)`
- Computed on demand, not stored daily. `current_value` updated when user runs depreciation period.

**BR-AST-03: Disposal Locks Asset**
- Once status = `Disposed`, no further changes allowed except document uploads.

**BR-AST-04: Document Storage via IFileStorageService**
- All `asset_documents` use `storage_bucket` + `storage_key` — same pattern as Lab/Radiology.

### 4.6 Supplier Account Rules

**BR-SUP-01: Supplier Balance**
- `supplier.balance` = Σ(GoodsReceipt totals) − Σ(SupplierPayments) − Σ(PurchaseReturn totals).
- Balance > 0 means clinic owes supplier.
- Balance < 0 means supplier owes clinic (overpayment/credit).

**BR-SUP-02: Payment to Supplier Requires Vault**
- Supplier payment always references a vault. Creates VaultTransaction (out).

**BR-SUP-03: Supplier Code Format** — `SUP-000001`

### 4.7 Analytics Rules

**BR-ANA-01: Dashboard Data is Read-Only**
- Dashboard and analytics screens never write data — query-only.

**BR-ANA-02: Date Range Defaults**
- Default range: current month (1st to today).
- All analytics support: Today / This Week / This Month / This Quarter / Custom.

**BR-ANA-03: Doctor Performance is Commission-Adjusted**
- Revenue attributed to doctor = Σ(InvoiceItems where procedure.doctor_id = doctor) minus doctor commission paid.

---

## 5. Screen Inventory S30–S47

### Inventory Screens

| ID | Path | Description |
|----|------|-------------|
| S30 | `/inventory/items` | Items catalog — list with search, filter by category, low-stock badge |
| S31 | `/inventory/items/[id]` | Item detail — current stock per warehouse, batches, movement history |
| S32 | `/inventory/stock` | Stock overview — all items with current qty, reorder alerts, expiry alerts |
| S33 | `/inventory/movements` | Stock movement ledger — filter by item/type/date |

### Purchasing Screens

| ID | Path | Description |
|----|------|-------------|
| S34 | `/purchasing/orders` | Purchase orders list with status filters + Create PO |
| S35 | `/purchasing/orders/[id]` | PO detail — items, approve/send/receive actions, GR list |
| S36 | `/purchasing/receipts/[id]` | Goods receipt detail — batch entry per item with expiry dates |
| S37 | `/purchasing/returns` | Purchase returns list + Create return |

### Suppliers Screen

| ID | Path | Description |
|----|------|-------------|
| S38 | `/suppliers` | Supplier list — name, balance, status |
| S39 | `/suppliers/[id]` | Supplier detail — account balance, PO history, payment history, pay button |

### Expenses Screens

| ID | Path | Description |
|----|------|-------------|
| S40 | `/finance/expenses` | Expenses list with category filter + Create expense |
| S41 | `/settings/expense-categories` | Expense categories CRUD + templates |

### Assets Screens

| ID | Path | Description |
|----|------|-------------|
| S42 | `/assets` | Asset register list — filter by category/status |
| S43 | `/assets/[id]` | Asset detail — value, depreciation, maintenance log, documents |

### Dashboard & Analytics Screens

| ID | Path | Description |
|----|------|-------------|
| S44 | `/` (Dashboard) | Executive dashboard — KPI cards, revenue chart, alerts widget |
| S45 | `/analytics/revenue` | Revenue analytics — by service, doctor, period |
| S46 | `/analytics/operations` | Operational analytics — patient flow, appointments, treatment completion |
| S47 | `/analytics/inventory` | Inventory analytics — stock valuation, consumption, expiry report |

---

## 6. API Contracts

### 6.1 Inventory API `/api/inventory`

```
GET    /items                          ?category, search, lowStock, page, pageSize
POST   /items                          CreateItem
GET    /items/{id}                     GetItemDetail (stock per warehouse + recent movements)
PUT    /items/{id}                     UpdateItem
GET    /items/{id}/movements           GetItemMovements ?from, to
POST   /items/{id}/adjust              CreateAdjustmentMovement { warehouseId, quantity, reason }
GET    /stock/summary                  GetStockSummary (all items current qty)
GET    /stock/alerts                   GetStockAlerts (low-stock + expiry alerts)
GET    /stock/batches                  GetBatches ?itemId, expiryBefore, page
GET    /warehouses                     GetWarehouses
POST   /warehouses                     CreateWarehouse
GET    /item-categories                GetItemCategories
POST   /item-categories                CreateItemCategory
GET    /units-of-measure               GetUnitsOfMeasure
```

### 6.2 Purchasing API `/api/purchasing`

```
GET    /orders                         ?supplierId, status, from, to, page
POST   /orders                         CreatePurchaseOrder
GET    /orders/{id}                    GetPurchaseOrderDetail
POST   /orders/{id}/approve            ApprovePO
POST   /orders/{id}/send               MarkPOSent
POST   /orders/{id}/cancel             CancelPO
POST   /orders/{id}/receive            CreateGoodsReceipt { warehouseId, items[{poItemId, batchNumber, quantity, unitCost, expiryDate}] }
GET    /receipts/{id}                  GetGoodsReceiptDetail
GET    /returns                        ?supplierId, status, page
POST   /returns                        CreatePurchaseReturn
POST   /returns/{id}/confirm           ConfirmPurchaseReturn
```

### 6.3 Suppliers API `/api/suppliers`

```
GET    /suppliers                      ?search, activeOnly, page
POST   /suppliers                      CreateSupplier
GET    /suppliers/{id}                 GetSupplierDetail (balance, recent POs, payments)
PUT    /suppliers/{id}                 UpdateSupplier
POST   /suppliers/{id}/payment         RecordSupplierPayment { vaultId, amount, referenceNumber, notes }
GET    /suppliers/{id}/statement       GetSupplierStatement ?from, to (POs + Payments)
```

### 6.4 Expenses API `/api/expenses`

```
GET    /expenses                       ?categoryId, from, to, page
POST   /expenses                       CreateExpense
GET    /expense-categories             GetCategories
POST   /expense-categories             CreateCategory
GET    /expense-templates              GetTemplates
POST   /expense-templates              CreateTemplate
```

### 6.5 Assets API `/api/assets`

```
GET    /assets                         ?categoryId, status, page
POST   /assets                         CreateAsset
GET    /assets/{id}                    GetAssetDetail (maintenance log + documents)
PUT    /assets/{id}                    UpdateAsset
POST   /assets/{id}/depreciate         RunDepreciation { periodDate }
POST   /assets/{id}/dispose            DisposeAsset { disposalDate, reason }
POST   /assets/{id}/documents          UploadAssetDocument { documentType, storageBucket, storageKey, fileName, fileSize }
POST   /assets/{id}/maintenance        AddMaintenanceLog { date, description, cost, performedBy, nextDate }
```

### 6.6 Analytics API `/api/analytics`

```
GET    /dashboard                      GetExecutiveDashboard ?date (single-day or current)
GET    /revenue                        GetRevenueReport ?from, to, groupBy (doctor|service|day|month)
GET    /revenue/by-doctor              GetDoctorRevenueBreakdown ?from, to
GET    /revenue/by-service             GetServiceRevenueBreakdown ?from, to
GET    /operations/appointments        GetAppointmentAnalytics ?from, to
GET    /operations/patient-flow        GetPatientFlowReport ?from, to
GET    /operations/treatment-completion GetTreatmentCompletionRate ?from, to
GET    /lab/utilization                GetLabUtilizationReport ?from, to
GET    /radiology/utilization          GetRadiologyUtilizationReport ?from, to
GET    /inventory/valuation            GetStockValuationReport
GET    /inventory/consumption          GetConsumptionReport ?from, to, itemId
GET    /inventory/expiry               GetExpiryReport ?within_days
```

---

## 7. Integration Map

### 7.1 Inventory ↔ Laboratory

When a LabOrder is `Completed`, the system should deduct consumables linked to that procedure type:
- Future: `lab_procedure_consumables` mapping table (Phase 6 deferred — requires clinical template setup).
- Phase 6 immediate: `ManualIssue` stock movement linkable to a LabOrder by `reference_id` / `reference_type = 'LabOrder'`.
- A user-triggered "Issue Stock for Lab Order" action is sufficient for V1.

### 7.2 Inventory ↔ Radiology

Same pattern as Laboratory. `reference_type = 'RadiologyOrder'`. Manual issue per order for V1.

### 7.3 Expenses ↔ Treasury

**Integration path:** Expenses module writes to `DentalERP.Modules.Financial.Infrastructure.FinancialDbContext` — both share the same PostgreSQL database.

On expense creation:
```
INSERT INTO vault_transactions (id, vault_id, transaction_type, amount, direction, notes, ...)
VALUES (newGuid, expense.vault_id, 'general_payment', expense.amount, 'out', expense.description, ...)
```
This is executed as part of the same `SaveChangesAsync` transaction.

> Design note: The Inventory/Expenses module references FinancialDbContext via a shared DI context or a lightweight IVaultTransactionWriter interface injected from Financial module. This avoids circular project references: Financial ← (ref) ← Inventory (same direction as existing modules).

### 7.4 Supplier Payments ↔ Treasury

Same as expenses: supplier payment → VaultTransaction (out) using `IVaultTransactionWriter`.

### 7.5 Purchasing ↔ Inventory

GoodsReceipt confirmation (atomic):
1. `stock_batches` INSERT (new batch with expiry + quantity)
2. `stock_movements` INSERT (PurchaseReceipt, in)
3. `items.unit_cost` UPDATE (weighted average or latest cost)
4. `supplier.balance` UPDATE (+= total)
5. `po_items.quantity_received` UPDATE
6. PO status re-evaluation

### 7.6 Dashboard ↔ All Modules

Executive dashboard aggregates across databases (all modules share the same PostgreSQL instance):

| KPI | Source Table |
|-----|-------------|
| Total Revenue (month) | `invoices` WHERE status='Confirmed/Paid' |
| Collections (month) | `payments` |
| Outstanding Invoices | `invoices` WHERE status='Draft/Confirmed' |
| Insurance Receivables | `insurance_claims` WHERE status IN ('Submitted','PartiallyPaid') |
| Lab Orders (active) | `lab_orders` WHERE status NOT IN ('Completed','Cancelled') |
| Radiology Orders (active) | `radiology_orders` WHERE status NOT IN ('Completed','Cancelled') |
| Expenses (month) | `expenses` |
| Low Stock Items | `items` WHERE current_stock <= reorder_level |
| Expiring Soon | `stock_batches` WHERE expiry_date <= TODAY+30 |
| Asset Maintenance Due | `assets` WHERE next_maintenance implied from logs |

---

## 8. Reporting Requirements

### 8.1 Inventory Reports

| Report | Description | Filters |
|--------|-------------|---------|
| Stock Valuation Report | Current qty × unit_cost per item | Date, Category, Warehouse |
| Stock Movement Report | All in/out by item | Item, Date Range, Type |
| Expiry Report | Batches expiring within N days | Days threshold |
| Reorder Report | Items at or below reorder level | Category |
| Consumption Report | Items issued over period | Item, Date Range |

### 8.2 Purchasing Reports

| Report | Description |
|--------|-------------|
| PO Status Report | Open POs by supplier and status |
| Goods Receipt Report | All receipts by date/supplier |
| Supplier Payables Aging | Outstanding supplier balances with aging buckets (30/60/90 days) |
| Purchase Returns Report | Returns by supplier and reason |

### 8.3 Expense Reports

| Report | Description |
|--------|-------------|
| Expense by Category | Grouped totals for period |
| Expense by Month | Trend chart data |
| Expense vs Budget | (Budget module: future) |
| Recurring Expense Schedule | Upcoming recurring expenses |

### 8.4 Asset Reports

| Report | Description |
|--------|-------------|
| Asset Register | Full list with current values |
| Depreciation Schedule | Annual depreciation per asset |
| Maintenance Log Report | All maintenance by asset/date |
| Assets by Status | Active / Under Maintenance / Disposed |

### 8.5 Executive Dashboard KPIs

```
Revenue KPIs:
  - Monthly Revenue (invoiced vs collected)
  - Revenue by Service Category (pie)
  - Revenue by Doctor (bar)
  - MoM Revenue Trend (line chart, 6 months)

Operational KPIs:
  - Appointments this month (scheduled vs attended)
  - Patient visit frequency
  - Treatment plan completion rate
  - Lab orders by status (donut)
  - Radiology orders by status (donut)

Financial Health:
  - Cash position by vault
  - Insurance receivables outstanding
  - Expenses this month vs last month
  - Top expense categories

Inventory Alerts:
  - Low stock items count (link to report)
  - Items expiring within 30 days (link to report)
  - Pending POs count
```

### 8.6 Operational Analytics Drill-Downs

```
Patient Flow:
  - New patients per month
  - Return patients rate
  - Inactive patients (no visit > 6 months)

Appointment Analytics:
  - Utilization rate (booked/total slots)
  - No-show rate
  - Average appointment duration by type

Doctor Performance:
  - Revenue attributed per doctor (net of commissions)
  - Procedures completed count
  - Lab orders generated
  - Treatment plans closed

Lab & Radiology Velocity:
  - Average turnaround time (Lab: Draft → Completed)
  - Average turnaround time (Radiology: Ordered → Completed)
  - Orders by type breakdown
```

---

## 9. Migration Plan (023–028)

| Migration | File | Content |
|-----------|------|---------|
| 023 | `023_inventory.sql` | item_categories, units_of_measure, items, warehouses, stock_batches, stock_movements |
| 024 | `024_suppliers_purchasing.sql` | suppliers, supplier_payments, purchase_orders, purchase_order_items, goods_receipts, goods_receipt_items |
| 025 | `025_purchase_returns.sql` | purchase_returns, purchase_return_items |
| 026 | `026_expenses.sql` | expense_categories, expense_templates, expenses |
| 027 | `027_assets.sql` | asset_categories, assets, asset_documents, asset_maintenance_logs |
| 028 | `028_seeds_inventory.sql` | Seed: default warehouse, common item categories, default expense categories, asset categories |

---

## 10. Implementation Timeline

### Module Groupings

**Group A — Supply Chain** (Migrations 023–025)
- `DentalERP.Modules.Inventory` — Items, Warehouses, Stock Batches, Movements
- `DentalERP.Modules.Purchasing` — POs, GRs, Suppliers, Purchase Returns

**Group B — Financial Operations** (Migration 026)
- `DentalERP.Modules.Expenses` — integrated into Financial module or standalone

**Group C — Assets** (Migration 027)
- `DentalERP.Modules.Assets`

**Group D — Analytics** (no new migrations)
- `DentalERP.Modules.Analytics` — query-only, cross-module reads

### Frontend Screens Distribution

| Group | Screens | Count |
|-------|---------|-------|
| A — Inventory | S30–S37 | 8 |
| B — Suppliers | S38–S39 | 2 |
| C — Expenses | S40–S41 | 2 |
| D — Assets | S42–S43 | 2 |
| E — Dashboard/Analytics | S44–S47 | 4 |

**Total Phase 6:** 18 screens, 5 new modules, 6 migrations, target 100+ new unit tests.

---

## Architectural Notes

1. **Single Database Strategy** — All Phase 6 modules share the same PostgreSQL instance. Cross-module reads for analytics use raw SQL or EF queries targeting other schemas — no API calls between modules.

2. **IVaultTransactionWriter** — A minimal interface defined in SharedKernel, implemented by FinancialModule, injected into Inventory/Expenses to write vault transactions without circular project dependencies.

3. **No Background Jobs in V1** — Expiry alerts and reorder alerts are computed on-demand (query at dashboard load or explicit report request). No scheduled job engine required.

4. **Stock Current Quantity** — Always computed as `Σ(stock_batches.quantity WHERE is_depleted=false)` per item+warehouse. No denormalized "current_stock" column — avoids sync issues.

5. **Asset Depreciation** — Applied manually per period. System computes the depreciation amount; user confirms before `current_value` is updated.

6. **Analytics Module** — Read-only CQRS queries. No writes. No own DbContext. Receives `IDbConnection` (Dapper) or reads directly via EF contexts from other modules (registered in same DI container).

---

*PHASE_6_DESIGN.md — Awaiting approval before implementation begins.*
