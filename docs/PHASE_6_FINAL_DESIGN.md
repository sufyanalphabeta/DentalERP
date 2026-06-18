# PHASE 6 FINAL DESIGN DOCUMENT
# DentalERP — Inventory, Supply Chain, Expenses, Assets & Analytics

**Version:** 2.0 — FINAL  
**Date:** 2026-06-17  
**Status:** FROZEN — APPROVED FOR IMPLEMENTATION  
**Prerequisite:** Phase 5 Approved ✓ (2026-06-17)  
**Preceding Draft:** PHASE_6_DESIGN.md v1.0

---

## Revision Log (v1.0 → v2.0)

| # | Change | Affected Tables/Sections |
|---|--------|--------------------------|
| R1 | Item Barcode support | `items` + new `item_barcodes` table |
| R2 | Supplier Item Code | New `supplier_items` junction table |
| R3 | Issue Destinations (Clinic/Lab/Radiology/Doctor/Other) | `stock_movements` |
| R4 | Supplier Balance: computed, not stored | `suppliers` — remove `balance` column |
| R5 | Expense linkage to modules | `expenses` — add `related_module`, `related_entity_id` |
| R6 | Asset Tag support | `assets` — add `asset_tag` |
| R7 | Expanded Executive Dashboard KPIs | Section 8 |
| R8 | Analytics Query Layer / AnalyticsDbContext | New Section 9 |
| R9 | Future Accounting Module Reserved | New Section 12 |

---

## Table of Contents

1. [Phase Overview & Goals](#1-phase-overview--goals)
2. [Module Catalog](#2-module-catalog)
3. [Complete Database Design — ERD](#3-complete-database-design--erd)
4. [Business Rules](#4-business-rules)
5. [Screen Inventory S30–S47](#5-screen-inventory-s30s47)
6. [API Contracts](#6-api-contracts)
7. [Integration Map](#7-integration-map)
8. [Executive Dashboard KPIs — Expanded](#8-executive-dashboard-kpis--expanded)
9. [Analytics Query Layer — AnalyticsDbContext](#9-analytics-query-layer--analyticsdbcontext)
10. [Reporting Requirements](#10-reporting-requirements)
11. [Migration Plan (023–028)](#11-migration-plan-023028)
12. [Future Accounting Module — Reserved](#12-future-accounting-module--reserved)
13. [Implementation Plan](#13-implementation-plan)
14. [Test Strategy](#14-test-strategy)

---

## 1. Phase Overview & Goals

Phase 6 is the final major phase of DentalERP V1. It completes the operational loop by adding:
- **Supply chain** — track every item from supplier invoice to patient procedure.
- **Financial operations** — expense management fully integrated with Treasury.
- **Physical assets** — register, depreciate, and maintain clinic equipment.
- **Intelligence layer** — executive dashboard and operational analytics turning five phases of data into decisions.

### V1 Completion After Phase 6

| Domain | Phase |
|--------|-------|
| Patient Management | 2 |
| Clinical (Treatment Plans, Appointments) | 3 |
| Financial (Invoices, Treasury, Commissions) | 4 |
| Laboratory, Radiology, Insurance | 5 |
| **Inventory, Supply Chain, Expenses, Assets, Analytics** | **6 ← Current** |

---

## 2. Module Catalog

### 2.1 `DentalERP.Modules.Inventory`
Items catalog with barcode support. Stock batches with expiry dates. Stock movements ledger with destination tracking (Clinic / Lab / Radiology / Doctor / Other). Reorder alerts. Expiry alerts.

### 2.2 `DentalERP.Modules.Purchasing`
Purchase Orders, Goods Receipts, and Purchase Returns. Supplier item code lookup on receive. GR atomically creates stock batch + stock movement + updates PO progress + posts supplier payable.

### 2.3 `DentalERP.Modules.Suppliers`
Supplier master data. Supplier item catalog (supplier_items). Supplier balance **computed** (not stored) from GRs minus payments minus returns. Supplier statement report.

### 2.4 `DentalERP.Modules.Expenses`
Expense categories and templates. Expense entries always linked to a Vault (auto-posts VaultTransaction). Expenses optionally linked to another module entity (related_module + related_entity_id).

### 2.5 `DentalERP.Modules.Assets`
Asset register with asset_tag (physical barcode/QR tag). Depreciation (straight-line / declining balance). Maintenance log. Document uploads via IFileStorageService.

### 2.6 `DentalERP.Modules.Analytics`
Read-only analytics module. Uses a dedicated `AnalyticsDbContext` (no-tracking, no migrations). Provides all dashboard KPIs and operational reports by querying across all module tables.

---

## 3. Complete Database Design — ERD

### 3.1 Inventory — Items & Stock

---

#### `item_categories`
```
id              UUID        PK
name            VARCHAR(150) NOT NULL
name_ar         VARCHAR(150)
parent_id       UUID        FK → item_categories(id) NULL    ← hierarchy
is_active       BOOLEAN     DEFAULT TRUE
created_at      TIMESTAMPTZ DEFAULT NOW()
```

---

#### `units_of_measure`
```
id              UUID        PK
name            VARCHAR(50)  NOT NULL     -- "Piece", "Box", "Milliliter", "Gram"
name_ar         VARCHAR(50)               -- "قطعة", "علبة", "مل", "غرام"
abbreviation    VARCHAR(10)               -- "pcs", "box", "mL", "g"
```

---

#### `items`
```
id                  UUID        PK
item_code           VARCHAR(30)  UNIQUE NOT NULL         -- AUTO: ITM-000001
barcode             VARCHAR(100) UNIQUE NULL             ← R1: primary/default barcode
name                VARCHAR(200) NOT NULL
name_ar             VARCHAR(200)
category_id         UUID        FK → item_categories(id)
unit_of_measure_id  UUID        FK → units_of_measure(id)
unit_cost           DECIMAL(10,2) DEFAULT 0              -- last known / weighted-avg cost
reorder_level       DECIMAL(10,3) DEFAULT 0              -- alert threshold
reorder_quantity    DECIMAL(10,3) DEFAULT 0              -- suggested order qty
is_expiry_tracked   BOOLEAN     DEFAULT FALSE
storage_conditions  VARCHAR(200) NULL                   -- "Refrigerate 2–8°C"
is_active           BOOLEAN     DEFAULT TRUE
notes               TEXT
created_at          TIMESTAMPTZ DEFAULT NOW()
updated_at          TIMESTAMPTZ
```

---

#### `item_barcodes`     ← R1: multiple packaging barcodes per item
```
id          UUID        PK
item_id     UUID        FK → items(id)
barcode     VARCHAR(100) NOT NULL
label       VARCHAR(100) NULL       -- "Retail Pack", "Bulk Box", "Strip of 10"
is_primary  BOOLEAN     DEFAULT FALSE
UNIQUE(barcode)
```
> Use case: same item sold in box-of-10 and single unit, each with a different barcode.

---

#### `warehouses`
```
id          UUID        PK
name        VARCHAR(150) NOT NULL
name_ar     VARCHAR(150)
location    VARCHAR(300)
is_default  BOOLEAN     DEFAULT FALSE
is_active   BOOLEAN     DEFAULT TRUE
```

---

#### `stock_batches`
```
id              UUID        PK
item_id         UUID        FK → items(id)
warehouse_id    UUID        FK → warehouses(id)
batch_number    VARCHAR(100) NULL
quantity        DECIMAL(10,3) NOT NULL  DEFAULT 0
unit_cost       DECIMAL(10,2) NOT NULL
expiry_date     DATE        NULL
received_date   DATE        NOT NULL
is_depleted     BOOLEAN     DEFAULT FALSE
INDEX (item_id, warehouse_id, expiry_date, is_depleted)    ← FIFO queries
```

---

#### `stock_movements`
```
id                  UUID        PK
movement_number     VARCHAR(30)  UNIQUE            -- AUTO: MOV-YYYY-000001
item_id             UUID        FK → items(id)
warehouse_id        UUID        FK → warehouses(id)
batch_id            UUID        FK → stock_batches(id) NULL
movement_type       VARCHAR(40)  NOT NULL
  -- PurchaseReceipt | ManualIssue | LabConsumption | RadiologyConsumption
  -- Adjustment | WriteOff | SupplierReturn | Transfer
direction           VARCHAR(3)   NOT NULL                  -- 'in' | 'out'
quantity            DECIMAL(10,3) NOT NULL
unit_cost           DECIMAL(10,2) NULL
total_cost          DECIMAL(12,2) NULL
destination_type    VARCHAR(30)  NULL              ← R3: Clinic|Lab|Radiology|Doctor|Other
destination_id      UUID        NULL               ← R3: linked entity (lab_order_id, etc.)
reference_id        UUID        NULL               -- PO, GR, ReturnId
reference_type      VARCHAR(50)  NULL              -- 'GoodsReceipt' | 'PurchaseReturn'
notes               TEXT
created_by_id       UUID        NULL
created_at          TIMESTAMPTZ DEFAULT NOW()
INDEX (item_id, created_at)
INDEX (movement_type, created_at)
INDEX (destination_type, destination_id)   ← lookup all movements for a lab order
```

---

### 3.2 Suppliers

#### `suppliers`
```
id                  UUID        PK
supplier_code       VARCHAR(30)  UNIQUE NOT NULL         -- AUTO: SUP-000001
name                VARCHAR(200) NOT NULL
name_ar             VARCHAR(200)
category            VARCHAR(50)  NULL    -- Medical|Equipment|General|Lab|Radiology|Pharma
contact_person      VARCHAR(200)
phone               VARCHAR(30)
email               VARCHAR(200)
address             TEXT
payment_terms_days  INT         DEFAULT 30
credit_limit        DECIMAL(12,2) DEFAULT 0
is_active           BOOLEAN     DEFAULT TRUE
notes               TEXT
created_at          TIMESTAMPTZ DEFAULT NOW()
updated_at          TIMESTAMPTZ
```
> ⚠️ R4: `balance` column REMOVED. Balance is always computed:
> `balance = Σ(gr.total) − Σ(supplier_payments.amount) − Σ(confirmed_returns.total)`

---

#### `supplier_items`     ← R2: supplier's own catalog/codes
```
id                  UUID        PK
supplier_id         UUID        FK → suppliers(id)
item_id             UUID        FK → items(id)
supplier_item_code  VARCHAR(100) NOT NULL     -- supplier's own SKU/reference
supplier_item_name  VARCHAR(200) NULL         -- supplier's name for this item
last_unit_cost      DECIMAL(10,2) NULL        -- last price from this supplier
is_preferred        BOOLEAN     DEFAULT FALSE -- preferred supplier for this item
notes               TEXT
UNIQUE(supplier_id, item_id)
UNIQUE(supplier_id, supplier_item_code)
```
> Use case: on GR, operator scans supplier barcode → lookup via supplier_item_code → auto-populate item.

---

#### `supplier_payments`
```
id                  UUID        PK
payment_number      VARCHAR(30)  UNIQUE        -- AUTO: SPAY-YYYY-000001
supplier_id         UUID        FK → suppliers(id)
vault_id            UUID        NOT NULL        -- cross-module ref (no EF FK)
amount              DECIMAL(12,2) NOT NULL
payment_date        DATE        NOT NULL
reference_number    VARCHAR(100) NULL
notes               TEXT
paid_by_id          UUID        NULL
created_at          TIMESTAMPTZ DEFAULT NOW()
```

---

### 3.3 Purchasing

#### `purchase_orders`
```
id                  UUID        PK
po_number           VARCHAR(30)  UNIQUE        -- AUTO: PO-YYYY-000001
supplier_id         UUID        FK → suppliers(id)
status              VARCHAR(30)  NOT NULL
  -- Draft|Approved|Sent|PartiallyReceived|FullyReceived|Closed|Cancelled
order_date          DATE        NOT NULL
expected_date       DATE        NULL
subtotal            DECIMAL(12,2) DEFAULT 0
discount_amount     DECIMAL(12,2) DEFAULT 0
total_amount        DECIMAL(12,2) DEFAULT 0
notes               TEXT
approved_by_id      UUID        NULL
approved_at         TIMESTAMPTZ NULL
created_by_id       UUID        NULL
created_at          TIMESTAMPTZ DEFAULT NOW()
updated_at          TIMESTAMPTZ
```

---

#### `purchase_order_items`
```
id                  UUID        PK
po_id               UUID        FK → purchase_orders(id)
item_id             UUID        FK → items(id)
supplier_item_id    UUID        FK → supplier_items(id) NULL   ← R2: links to supplier catalog
quantity_ordered    DECIMAL(10,3) NOT NULL
quantity_received   DECIMAL(10,3) DEFAULT 0
unit_cost           DECIMAL(10,2) NOT NULL
total_cost          DECIMAL(12,2) NOT NULL
notes               TEXT
```

---

#### `goods_receipts`
```
id                  UUID        PK
gr_number           VARCHAR(30)  UNIQUE        -- AUTO: GR-YYYY-000001
po_id               UUID        FK → purchase_orders(id) NULL
supplier_id         UUID        FK → suppliers(id)
warehouse_id        UUID        FK → warehouses(id)
receipt_date        DATE        NOT NULL
supplier_invoice_ref VARCHAR(100) NULL         -- supplier's invoice number
total_amount        DECIMAL(12,2) DEFAULT 0    -- computed from items on save
notes               TEXT
received_by_id      UUID        NULL
created_at          TIMESTAMPTZ DEFAULT NOW()
```

---

#### `goods_receipt_items`
```
id                  UUID        PK
gr_id               UUID        FK → goods_receipts(id)
po_item_id          UUID        FK → purchase_order_items(id) NULL
item_id             UUID        FK → items(id)
batch_number        VARCHAR(100) NULL
quantity            DECIMAL(10,3) NOT NULL
unit_cost           DECIMAL(10,2) NOT NULL
total_cost          DECIMAL(12,2) NOT NULL
expiry_date         DATE        NULL
```

---

### 3.4 Purchase Returns

#### `purchase_returns`
```
id                  UUID        PK
return_number       VARCHAR(30)  UNIQUE        -- AUTO: PRN-YYYY-000001
supplier_id         UUID        FK → suppliers(id)
po_id               UUID        FK → purchase_orders(id) NULL
return_date         DATE        NOT NULL
reason              TEXT        NOT NULL
status              VARCHAR(20)  NOT NULL      -- Draft|Confirmed|Credited
total_amount        DECIMAL(12,2) DEFAULT 0
notes               TEXT
created_by_id       UUID        NULL
created_at          TIMESTAMPTZ DEFAULT NOW()
```

---

#### `purchase_return_items`
```
id                  UUID        PK
return_id           UUID        FK → purchase_returns(id)
item_id             UUID        FK → items(id)
batch_id            UUID        FK → stock_batches(id) NULL
quantity            DECIMAL(10,3) NOT NULL
unit_cost           DECIMAL(10,2) NOT NULL
total_cost          DECIMAL(12,2) NOT NULL
```

---

### 3.5 Expenses

#### `expense_categories`
```
id              UUID        PK
name            VARCHAR(150) NOT NULL
name_ar         VARCHAR(150)
is_active       BOOLEAN     DEFAULT TRUE
```

#### `expense_templates`
```
id              UUID        PK
name            VARCHAR(200) NOT NULL
category_id     UUID        FK → expense_categories(id)
default_amount  DECIMAL(10,2) NULL
default_vault_id UUID       NULL               -- cross-module ref
vendor_name     VARCHAR(200) NULL
recurrence      VARCHAR(20)  NULL              -- Monthly|Weekly|Quarterly|Annually
notes           TEXT
is_active       BOOLEAN     DEFAULT TRUE
```

#### `expenses`
```
id                  UUID        PK
expense_number      VARCHAR(30)  UNIQUE        -- AUTO: EXP-YYYY-000001
category_id         UUID        FK → expense_categories(id)
template_id         UUID        FK → expense_templates(id) NULL
vault_id            UUID        NOT NULL        -- cross-module ref to Financial.Vaults
amount              DECIMAL(10,2) NOT NULL
expense_date        DATE        NOT NULL
description         TEXT        NOT NULL
vendor_name         VARCHAR(200) NULL
reference_number    VARCHAR(100) NULL
related_module      VARCHAR(50)  NULL          ← R5: Laboratory|Radiology|Clinical|Inventory|Assets|Financial
related_entity_id   UUID        NULL           ← R5: e.g., asset_id for maintenance, lab_order_id for reagent
approved_by_id      UUID        NULL
created_by_id       UUID        NULL
created_at          TIMESTAMPTZ DEFAULT NOW()
```
> Example uses of related_module linkage:
> - Expense for lab reagents → related_module='Laboratory', related_entity_id=lab_order_id
> - Maintenance cost for chair → related_module='Assets', related_entity_id=asset_id
> - Radiology film expense → related_module='Radiology', related_entity_id=radiology_order_id

---

### 3.6 Assets

#### `asset_categories`
```
id          UUID        PK
name        VARCHAR(150) NOT NULL
name_ar     VARCHAR(150)
```

#### `assets`
```
id                  UUID        PK
asset_code          VARCHAR(30)  UNIQUE NOT NULL    -- AUTO: AST-000001
asset_tag           VARCHAR(100) UNIQUE NULL        ← R6: physical tag (QR/barcode on device)
name                VARCHAR(200) NOT NULL
name_ar             VARCHAR(200)
category_id         UUID        FK → asset_categories(id)
serial_number       VARCHAR(100) NULL
purchase_date       DATE        NOT NULL
purchase_cost       DECIMAL(12,2) NOT NULL
current_value       DECIMAL(12,2) NOT NULL
depreciation_method VARCHAR(20)  NULL               -- StraightLine|DecliningBalance|None
depreciation_rate   DECIMAL(5,2) DEFAULT 0          -- annual %
location            VARCHAR(200) NULL
supplier_id         UUID        FK → suppliers(id) NULL
warranty_expiry     DATE        NULL
status              VARCHAR(30)  NOT NULL            -- Active|UnderMaintenance|Disposed
disposal_date       DATE        NULL
disposal_reason     TEXT        NULL
notes               TEXT
created_at          TIMESTAMPTZ DEFAULT NOW()
updated_at          TIMESTAMPTZ
```

#### `asset_documents`
```
id              UUID        PK
asset_id        UUID        FK → assets(id)
document_type   VARCHAR(50)  NOT NULL    -- Invoice|Warranty|Maintenance|Calibration|Manual|Other
storage_bucket  VARCHAR(200) NOT NULL
storage_key     VARCHAR(500) NOT NULL
file_name       VARCHAR(300) NOT NULL
file_size       BIGINT
notes           TEXT
uploaded_by_id  UUID
uploaded_at     TIMESTAMPTZ DEFAULT NOW()
```

#### `asset_maintenance_logs`
```
id                      UUID        PK
asset_id                UUID        FK → assets(id)
maintenance_date        DATE        NOT NULL
description             TEXT        NOT NULL
cost                    DECIMAL(10,2) DEFAULT 0
expense_id              UUID        NULL             -- links to expenses.id if cost posted
performed_by            VARCHAR(200) NULL
next_maintenance_date   DATE        NULL
created_at              TIMESTAMPTZ DEFAULT NOW()
```
> `expense_id` links back to expenses.id — when maintenance generates an expense entry, both records cross-reference.

---

### 3.7 Full ERD Relationship Summary

```
INVENTORY
─────────
item_categories ─(hierarchy)─ item_categories
item_categories ──< items >── units_of_measure
items ──< item_barcodes
items ──< stock_batches >── warehouses
items ──< stock_movements >── warehouses
stock_movements.batch_id ── stock_batches

SUPPLIERS
─────────
suppliers ──< supplier_items >── items
suppliers ──< supplier_payments
suppliers ──< purchase_orders
suppliers ──< purchase_returns
suppliers ──< assets (nullable purchase source)

PURCHASING
──────────
purchase_orders ──< purchase_order_items >── items
purchase_order_items >── supplier_items (nullable)
purchase_orders ──< goods_receipts
goods_receipts ──< goods_receipt_items >── items
goods_receipt_items >── purchase_order_items (nullable)
purchase_returns ──< purchase_return_items >── items
purchase_return_items >── stock_batches (nullable)

EXPENSES
────────
expense_categories ──< expense_templates
expense_categories ──< expenses
expense_templates ──< expenses (nullable)

ASSETS
──────
asset_categories ──< assets >── suppliers (nullable)
assets ──< asset_documents
assets ──< asset_maintenance_logs
asset_maintenance_logs.expense_id ── expenses (nullable)

CROSS-MODULE (UUID refs, no EF FK)
───────────────────────────────────
expenses.vault_id               → Financial.Vaults
supplier_payments.vault_id      → Financial.Vaults
expenses.related_entity_id      → [any module entity]
stock_movements.destination_id  → [LabOrder | RadiologyOrder | Doctor | ...]
stock_movements.reference_id    → [GoodsReceipt | PurchaseReturn]
```

---

## 4. Business Rules

### 4.1 Inventory — Items & Barcodes

**BR-INV-01: Item Code Format** — `ITM-{6-digit}` (e.g., `ITM-000001`)

**BR-INV-02: Barcode Uniqueness**
- `items.barcode` is UNIQUE across all items.
- `item_barcodes.barcode` is UNIQUE across the entire table.
- The same barcode value cannot appear in both `items.barcode` and `item_barcodes.barcode`.
- Barcode lookup searches both `items.barcode` and `item_barcodes` → returns the item.

**BR-INV-03: Primary Barcode Flag**
- `item_barcodes.is_primary = true` flags the preferred barcode. Only one primary per item.
- If `items.barcode` is set, it is implicitly the primary. Additional barcodes go in `item_barcodes`.

**BR-INV-04: Stock Cannot Go Negative**
- On any `out` movement, validate: `Σ(batch.quantity WHERE item+warehouse) >= quantity_requested`.
- Exception: `Adjustment` type with explicit `allow_negative = true` in command.

**BR-INV-05: FIFO Batch Selection**
- When issuing stock without specifying a batch: select batches ordered by `expiry_date ASC NULLS LAST`, then `received_date ASC`.
- Operator may override FIFO by specifying `batch_id` explicitly.

**BR-INV-06: Reorder Alert**
- After every `out` movement: compute `current_stock = Σ(batch.quantity WHERE item+warehouse AND NOT is_depleted)`.
- If `current_stock <= item.reorder_level`, insert/update a reorder alert record.

**BR-INV-07: Expiry Alert Levels**
- `Critical` — expiry ≤ 30 days from today.
- `Warning` — expiry 31–60 days from today.
- `Notice` — expiry 61–90 days from today.
- Computed on-demand (no background job required in V1).

**BR-INV-08: Movement Number Format** — `MOV-YYYY-{6-digit}`

### 4.2 Inventory — Stock Movements & Destinations (R3)

**BR-MOV-01: Issue Destination Required for Manual Issues**
- When `movement_type = 'ManualIssue'`, `destination_type` is required.
- Valid values: `Clinic` | `Lab` | `Radiology` | `Doctor` | `Other`.

**BR-MOV-02: Destination ID Rules**
- `destination_type = 'Lab'` → `destination_id` should be a LabOrder UUID (optional but recommended).
- `destination_type = 'Radiology'` → `destination_id` should be a RadiologyOrder UUID.
- `destination_type = 'Doctor'` → `destination_id` should be a DoctorProfile UUID.
- `destination_type = 'Clinic'` or `'Other'` → `destination_id` may be null.

**BR-MOV-03: Auto-Typed Movements**
- `movement_type = 'LabConsumption'` implicitly sets `destination_type = 'Lab'`.
- `movement_type = 'RadiologyConsumption'` implicitly sets `destination_type = 'Radiology'`.
- `movement_type = 'PurchaseReceipt'` does not set destination (incoming).
- `movement_type = 'SupplierReturn'` does not set destination (outgoing to supplier).

### 4.3 Suppliers (R4: Computed Balance)

**BR-SUP-01: Supplier Balance is Always Computed**
```
supplier_balance =
    Σ goods_receipt.total_amount (WHERE supplier_id = X)
  − Σ supplier_payment.amount   (WHERE supplier_id = X)
  − Σ purchase_return.total_amount (WHERE supplier_id = X AND status = 'Confirmed')
```
- Balance > 0 → clinic owes supplier (payable).
- Balance < 0 → supplier owes clinic (overpayment / credit memo).
- No `balance` column on `suppliers` table.
- Query exposed via `GetSupplierBalanceQuery`.

**BR-SUP-02: Supplier Item Code Lookup (R2)**
- On GoodsReceipt creation, operator may enter `supplier_item_code` instead of internal item_id.
- System resolves: `supplier_items WHERE supplier_id = X AND supplier_item_code = Y` → returns `item_id`.
- If no mapping exists, operator must select item manually.

**BR-SUP-03: Preferred Supplier**
- `supplier_items.is_preferred = true` marks the default supplier for that item.
- Only one preferred supplier per item (enforced at application level; not DB constraint).

**BR-SUP-04: Supplier Code Format** — `SUP-000001`

**BR-SUP-05: Supplier Payment Format** — `SPAY-YYYY-000001`

### 4.4 Purchasing

**BR-PO-01: PO Status Machine**
```
Draft ──→ Approved ──→ Sent ──→ PartiallyReceived ──→ FullyReceived ──→ Closed
  │             │
  └──→ Cancelled  └──→ Cancelled (only if nothing received)
```

**BR-PO-02: Approval Gate**
- PO cannot be marked `Sent` unless status = `Approved`.
- Approved_by_id must be populated. Same user who created PO cannot self-approve (enforced at application layer, not DB).

**BR-PO-03: GR Atomic Cascade** (single DB transaction)
1. `goods_receipt` record inserted.
2. `goods_receipt_items` inserted.
3. Per item: create/update `stock_batch`.
4. Per item: create `stock_movement` (PurchaseReceipt, in, no destination).
5. Per item: update `po_items.quantity_received += quantity`.
6. Per item: update `items.unit_cost` = (weighted average or latest — configurable per item).
7. Recompute `goods_receipt.total_amount`.
8. Evaluate PO status: if all po_items fully received → `FullyReceived`; else → `PartiallyReceived`.

**BR-PO-04: PO Number Format** — `PO-YYYY-000001`  
**BR-PO-05: GR Number Format** — `GR-YYYY-000001`

### 4.5 Purchase Returns

**BR-PR-01: Can Only Return Received Items**
- Return quantity for a batch cannot exceed the original received quantity minus any prior returns on that batch.

**BR-PR-02: Confirm Cascade** (single DB transaction)
1. Validate quantities.
2. Create `stock_movement` (SupplierReturn, out).
3. Reduce `stock_batch.quantity`.

**BR-PR-03: Return Number Format** — `PRN-YYYY-000001`

### 4.6 Expenses (R5: Module Linkage)

**BR-EXP-01: Vault Deduction is Atomic**
- `expenses` record + `vault_transactions` record saved in same DB transaction.
- VaultTransaction: `direction=out`, `type=general_payment`, `related_invoice_id=null`, `notes=expense.description`.

**BR-EXP-02: Module Linkage is Optional**
- `related_module` and `related_entity_id` are nullable.
- When provided, `related_module` must be one of: `Financial` | `Laboratory` | `Radiology` | `Clinical` | `Inventory` | `Assets`.
- No FK constraint — cross-module reference by UUID only.

**BR-EXP-03: Maintenance Expense Auto-Link**
- When creating an expense from `asset_maintenance_logs`, system automatically sets:
  - `related_module = 'Assets'`
  - `related_entity_id = asset_id`
  - `expense_id` written back to `asset_maintenance_logs.expense_id`.

**BR-EXP-04: Expense Number Format** — `EXP-YYYY-000001`

### 4.7 Assets (R6: Asset Tag)

**BR-AST-01: Asset Code Format** — `AST-{6-digit}` (no year — assets are permanent records)

**BR-AST-02: Asset Tag** (R6)
- `asset_tag` is a free-text unique identifier placed on the physical asset (barcode label, QR sticker, engraved number).
- Format: user-defined or suggested as `TAG-{6-digit}`.
- Unique constraint enforced at DB level.
- Used for physical inventory scanning. System lookup: enter tag → show asset.

**BR-AST-03: Depreciation Computation**
- Straight-line: `period_depreciation = purchase_cost × (rate/100) × (days_in_period/365)`.
- Declining balance: `period_depreciation = current_value × (rate/100) × (days_in_period/365)`.
- User triggers depreciation per period. System computes and proposes `new_current_value`. User confirms → `assets.current_value` updated.

**BR-AST-04: Disposal Lock**
- Once `status = Disposed`, no edits except uploading documents.
- `disposal_date` and `disposal_reason` are set on disposal.

**BR-AST-05: Document Storage via IFileStorageService**
- All `asset_documents` use `storage_bucket` + `storage_key`.

---

## 5. Screen Inventory S30–S47

### 5.1 Inventory Screens

| ID | Path | Description | Key Actions |
|----|------|-------------|-------------|
| S30 | `/inventory/items` | Items catalog — search by name, code, barcode; filter by category; low-stock badge | Add item, Edit item |
| S31 | `/inventory/items/[id]` | Item detail — current stock per warehouse, active batches with expiry, movement history, supplier item codes | Issue stock, Adjust, View history |
| S32 | `/inventory/stock` | Stock overview — all items with qty, reorder alerts (red), expiry alerts (amber/red) | Quick issue, Link to PO |
| S33 | `/inventory/movements` | Movement ledger — all movements; filter by item, type, destination, date range | Export |

### 5.2 Purchasing Screens

| ID | Path | Description | Key Actions |
|----|------|-------------|-------------|
| S34 | `/purchasing/orders` | PO list with status chips; filter by supplier/status | Create PO |
| S35 | `/purchasing/orders/[id]` | PO detail — items table with ordered/received qty; timeline of approvals | Approve, Send, Receive, Cancel |
| S36 | `/purchasing/receive/[poId]` | Goods receipt form — per-item entry: batch number, quantity, unit cost, expiry date; supplier_item_code lookup | Confirm receipt |
| S37 | `/purchasing/returns` | Purchase returns list | Create return, Confirm return |

### 5.3 Supplier Screens

| ID | Path | Description | Key Actions |
|----|------|-------------|-------------|
| S38 | `/suppliers` | Supplier list — name, category, computed balance, status | Add supplier |
| S39 | `/suppliers/[id]` | Supplier detail — computed balance, PO history, payment history, item catalog | Pay supplier, Add item code |

### 5.4 Expense Screens

| ID | Path | Description | Key Actions |
|----|------|-------------|-------------|
| S40 | `/finance/expenses` | Expenses list — filter by category, date, module; totals banner | Add expense |
| S41 | `/settings/expense-categories` | Categories CRUD + expense templates CRUD | Add category, Add template |

### 5.5 Asset Screens

| ID | Path | Description | Key Actions |
|----|------|-------------|-------------|
| S42 | `/assets` | Asset register — filter by category/status; asset_tag display | Add asset |
| S43 | `/assets/[id]` | Asset detail — value, depreciation schedule, maintenance timeline, documents | Run depreciation, Add maintenance, Upload doc, Dispose |

### 5.6 Dashboard & Analytics Screens

| ID | Path | Description | Key Components |
|----|------|-------------|----------------|
| S44 | `/` (Dashboard) | Executive dashboard | KPI cards, Revenue chart (6-month trend), Alerts widget (low stock + expiry), Insurance receivables, Pending POs |
| S45 | `/analytics/revenue` | Revenue analytics | By doctor, by service, by month — bar/line charts; collection rate |
| S46 | `/analytics/operations` | Operational analytics | Patient flow, appointment utilization, treatment completion, Lab & Radiology velocity |
| S47 | `/analytics/inventory` | Inventory analytics | Stock valuation, consumption trend, expiry report, reorder report |

---

## 6. API Contracts

### 6.1 Inventory API `/api/inventory`

```
Items:
GET    /items                        ?category, search, barcode, lowStock, page
POST   /items                        CreateItem
GET    /items/{id}                   GetItemDetail
PUT    /items/{id}                   UpdateItem
GET    /items/by-barcode/{barcode}   LookupItemByBarcode    ← R1
POST   /items/{id}/barcodes          AddItemBarcode          ← R1
DELETE /items/{id}/barcodes/{bid}    RemoveItemBarcode       ← R1
POST   /items/{id}/adjust            CreateAdjustmentMovement { warehouseId, quantity, reason, allowNegative }

Stock:
GET    /stock/summary                GetStockSummary (all items, all warehouses)
GET    /stock/alerts                 GetStockAlerts (reorder + expiry)
GET    /stock/batches                GetBatches ?itemId, warehouseId, expiryBefore
POST   /stock/issue                  CreateManualIssue { itemId, warehouseId, batchId?, quantity, destinationType, destinationId?, notes }   ← R3

Movements:
GET    /movements                    GetMovements ?itemId, type, destinationType, destinationId, from, to, page

Warehouses:
GET    /warehouses                   GetWarehouses
POST   /warehouses                   CreateWarehouse

Categories & UOM:
GET    /item-categories              GetItemCategories
POST   /item-categories              CreateItemCategory
GET    /units-of-measure             GetUnitsOfMeasure
POST   /units-of-measure             CreateUnitOfMeasure
```

### 6.2 Purchasing API `/api/purchasing`

```
Purchase Orders:
GET    /orders                       ?supplierId, status, from, to, page
POST   /orders                       CreatePurchaseOrder
GET    /orders/{id}                  GetPurchaseOrderDetail
POST   /orders/{id}/approve          ApprovePO
POST   /orders/{id}/send             MarkPOSent
POST   /orders/{id}/cancel           CancelPO

Goods Receipts:
POST   /orders/{id}/receive          CreateGoodsReceipt { warehouseId, supplierInvoiceRef, items[{poItemId, batchNumber, quantity, unitCost, expiryDate}] }
GET    /receipts/{id}                GetGoodsReceiptDetail

Purchase Returns:
GET    /returns                      ?supplierId, status, page
POST   /returns                      CreatePurchaseReturn { supplierId, poId?, reason, items[{itemId, batchId, quantity, unitCost}] }
POST   /returns/{id}/confirm         ConfirmPurchaseReturn
```

### 6.3 Suppliers API `/api/suppliers`

```
GET    /suppliers                    ?search, activeOnly, category, page
POST   /suppliers                    CreateSupplier
GET    /suppliers/{id}               GetSupplierDetail    (includes computed balance)   ← R4
PUT    /suppliers/{id}               UpdateSupplier
GET    /suppliers/{id}/balance       GetSupplierBalance   (real-time computed)          ← R4
POST   /suppliers/{id}/payment       RecordSupplierPayment { vaultId, amount, referenceNumber, paymentDate, notes }
GET    /suppliers/{id}/statement     GetSupplierStatement ?from, to

Supplier Items:
GET    /suppliers/{id}/items         GetSupplierItemCatalog                             ← R2
POST   /suppliers/{id}/items         AddSupplierItemCode { itemId, supplierItemCode, supplierItemName, lastUnitCost }   ← R2
GET    /suppliers/{id}/items/lookup/{code}  LookupBySupplierCode                       ← R2
```

### 6.4 Expenses API `/api/expenses`

```
GET    /expenses                     ?categoryId, vaultId, relatedModule, from, to, page   ← R5
POST   /expenses                     CreateExpense (with optional related_module + related_entity_id)
GET    /expense-categories           GetCategories
POST   /expense-categories           CreateCategory
GET    /expense-templates            GetTemplates
POST   /expense-templates            CreateTemplate
```

### 6.5 Assets API `/api/assets`

```
GET    /assets                       ?categoryId, status, page
POST   /assets                       CreateAsset
GET    /assets/{id}                  GetAssetDetail
PUT    /assets/{id}                  UpdateAsset
GET    /assets/by-tag/{tag}          LookupAssetByTag                                   ← R6
POST   /assets/{id}/depreciate       RunDepreciation { periodDate, confirm }
POST   /assets/{id}/dispose          DisposeAsset { disposalDate, reason }
POST   /assets/{id}/documents        UploadAssetDocument
POST   /assets/{id}/maintenance      AddMaintenanceLog { date, description, cost, vaultId?, performedBy, nextDate }
```

### 6.6 Analytics API `/api/analytics`

```
Dashboard:
GET    /dashboard                    GetExecutiveDashboard ?date

Revenue:
GET    /revenue                      GetRevenueReport ?from, to, groupBy(doctor|service|day|month)
GET    /revenue/by-doctor            GetDoctorRevenueBreakdown ?from, to
GET    /revenue/by-service           GetServiceRevenueBreakdown ?from, to
GET    /revenue/collection-rate      GetCollectionRate ?from, to

Operations:
GET    /operations/appointments      GetAppointmentAnalytics ?from, to
GET    /operations/patient-flow      GetPatientFlowReport ?from, to
GET    /operations/treatment-completion  GetTreatmentCompletionRate ?from, to

Lab & Radiology:
GET    /lab/utilization              GetLabUtilizationReport ?from, to
GET    /radiology/utilization        GetRadiologyUtilizationReport ?from, to
GET    /lab/revenue                  GetLabRevenueReport ?from, to
GET    /radiology/revenue            GetRadiologyRevenueReport ?from, to

Inventory:
GET    /inventory/valuation          GetStockValuationReport
GET    /inventory/consumption        GetConsumptionReport ?from, to, itemId
GET    /inventory/expiry             GetExpiryReport ?within_days
GET    /inventory/reorder            GetReorderReport

Expenses:
GET    /expenses/by-category         GetExpensesByCategory ?from, to
GET    /expenses/trend               GetExpenseTrend ?months(default 6)

Suppliers:
GET    /suppliers/payables-aging     GetSupplierPayablesAging
```

---

## 7. Integration Map

### 7.1 Inventory ↔ Treasury (Vault)

**Path:** `CreateManualIssueCommand` → if item has a cost, optionally creates a `VaultTransaction` via `IVaultTransactionWriter`.
- V1 decision: stock issue does NOT automatically debit vault (items are already costed at purchase time via GR).
- Vault integration only for: Expenses, Supplier Payments.

### 7.2 Expenses ↔ Treasury

**Path:** `CreateExpenseCommandHandler` writes to both `expenses` and `vault_transactions` tables in the same `SaveChangesAsync` call.

```csharp
// IVaultTransactionWriter interface in SharedKernel
// Implemented by FinancialModule, registered in DI
// Injected into ExpensesModule handlers
public interface IVaultTransactionWriter
{
    void QueueTransaction(DbContext context, Guid vaultId, string type,
        decimal amount, string direction, string? notes, Guid? createdByUserId);
}
```

**Project dependency direction:** `DentalERP.Modules.Expenses` → `DentalERP.SharedKernel` (no direct reference to Financial). IVaultTransactionWriter is resolved at runtime via DI.

### 7.3 Supplier Payments ↔ Treasury

Same as Expenses — `RecordSupplierPaymentCommandHandler` calls `IVaultTransactionWriter`.

### 7.4 Purchasing ↔ Inventory

GoodsReceipt confirmation cascade (Section 4.4, BR-PO-03) — atomic.

### 7.5 Assets ↔ Expenses ↔ Suppliers

Maintenance log → creates expense with `related_module='Assets'`, `related_entity_id=asset_id`.
Asset purchase can reference `supplier_id` from Suppliers module.

### 7.6 Inventory ↔ Laboratory

Phase 6 V1: Manual linkage only.
- User issues stock via `POST /inventory/stock/issue` with `destinationType='Lab'`, `destinationId=labOrderId`.
- This creates `stock_movement` with full traceability to the lab order.
- Future Phase: `lab_procedure_consumables` template for auto-deduction.

### 7.7 Inventory ↔ Radiology

Same as Laboratory — `destinationType='Radiology'`, `destinationId=radiologyOrderId`.

### 7.8 Analytics ↔ All Modules

Analytics reads directly from all module tables via `AnalyticsDbContext` (Section 9). No API calls between modules. Single PostgreSQL database.

---

## 8. Executive Dashboard KPIs — Expanded (R7)

### 8.1 Revenue Panel
```
┌─────────────────────────────────────────────────────────────┐
│ REVENUE — This Month                                        │
│                                                             │
│ Invoiced:         [Σ invoices WHERE status≠Draft, this mo] │
│ Collected:        [Σ payments, this month]                  │
│ Collection Rate:  [Collected / Invoiced × 100]%             │
│ Outstanding:      [Σ invoices WHERE status='Confirmed']     │
│                                                             │
│ Lab Revenue:      [Σ radiology_orders.price WHERE Completed]│
│ Radiology Revenue:[Σ radiology_orders.price WHERE Completed]│
│                                                             │
│ Chart: 6-month revenue trend (invoiced vs collected)       │
└─────────────────────────────────────────────────────────────┘
```

### 8.2 Insurance Panel
```
Insurance Receivables:
  Submitted claims:      [Σ claimedAmount WHERE status='Submitted']
  Partially paid:        [Σ (claimedAmount - paidAmount) WHERE status='PartiallyPaid']
  Total outstanding:     [sum of above]
  Claims this month:     [count created this month]
```

### 8.3 Doctor Performance Panel
```
Per Doctor (top 5 by revenue):
  Revenue Attributed:    [Σ invoice_items WHERE procedure.doctor_id]
  Commissions Earned:    [Σ commission_records WHERE doctor_id, this month]
  Net Revenue:           [attributed - commissions]
  Procedures Count:      [count completed procedures]
  Lab Orders:            [count lab_orders created]
```

### 8.4 Operations Panel
```
Appointments this month:
  Scheduled:     [count appointments]
  Attended:      [count WHERE status='Attended']
  No-Show:       [count WHERE status='NoShow']
  Utilization:   [Attended / Scheduled × 100]%

Patient Flow:
  New Patients:         [count patients WHERE created_at this month]
  Return Patients:      [count patients with >1 appointment this month]
  Active Treatment Plans:[count plans WHERE status='Active']
```

### 8.5 Inventory Alerts Panel
```
⚠️  Low Stock Items:        [count items WHERE current_stock <= reorder_level]
🔴  Expiring ≤ 30 days:     [count batches WHERE expiry ≤ today+30]
🟡  Expiring 31–60 days:    [count batches WHERE expiry ≤ today+60]
📦  Pending POs:            [count POs WHERE status IN (Draft,Approved,Sent)]
💰  Supplier Payables:      [Σ computed supplier balances WHERE balance > 0]
```

### 8.6 Financial Health Panel
```
Cash Position by Vault:
  [vault_name]: [computed balance]  (from Financial module)

Expenses this month:    [Σ expenses.amount, this month]
vs Last month:          [Σ expenses.amount, last month] [Δ%]

Top 3 Expense Categories:
  1. [category]: [amount]
  2. [category]: [amount]
  3. [category]: [amount]
```

### 8.7 Asset Alerts Panel
```
Assets Under Maintenance:  [count WHERE status='UnderMaintenance']
Warranties Expiring (90d): [count WHERE warranty_expiry ≤ today+90]
Next Scheduled Maintenance:[list top 3 assets with next_maintenance_date]
```

---

## 9. Analytics Query Layer — AnalyticsDbContext (R8)

### 9.1 Design Rationale

Analytics queries span all module tables. Using individual module DbContexts would:
- Require each analytics handler to depend on 5+ DbContexts.
- Create complex query composition across multiple contexts.
- Make join queries across modules impossible at EF level.

**Solution:** A dedicated `AnalyticsDbContext` in `DentalERP.Modules.Analytics` that maps all relevant tables from all modules as **read-only, no-tracking** entities.

### 9.2 AnalyticsDbContext Definition

```csharp
// DentalERP.Modules.Analytics/Infrastructure/AnalyticsDbContext.cs

public sealed class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
    : DbContext(options)
{
    // Financial
    public DbSet<InvoiceReadModel> Invoices => Set<InvoiceReadModel>();
    public DbSet<PaymentReadModel> Payments => Set<PaymentReadModel>();
    public DbSet<VaultReadModel> Vaults => Set<VaultReadModel>();
    public DbSet<CommissionReadModel> Commissions => Set<CommissionReadModel>();
    public DbSet<ExpenseReadModel> Expenses => Set<ExpenseReadModel>();

    // Clinical
    public DbSet<AppointmentReadModel> Appointments => Set<AppointmentReadModel>();
    public DbSet<TreatmentPlanReadModel> TreatmentPlans => Set<TreatmentPlanReadModel>();
    public DbSet<ProcedureReadModel> Procedures => Set<ProcedureReadModel>();

    // Patients
    public DbSet<PatientReadModel> Patients => Set<PatientReadModel>();

    // Laboratory
    public DbSet<LabOrderReadModel> LabOrders => Set<LabOrderReadModel>();

    // Radiology
    public DbSet<RadiologyOrderReadModel> RadiologyOrders => Set<RadiologyOrderReadModel>();

    // Insurance
    public DbSet<InsuranceClaimReadModel> InsuranceClaims => Set<InsuranceClaimReadModel>();

    // Inventory
    public DbSet<ItemReadModel> Items => Set<ItemReadModel>();
    public DbSet<StockBatchReadModel> StockBatches => Set<StockBatchReadModel>();
    public DbSet<StockMovementReadModel> StockMovements => Set<StockMovementReadModel>();

    // Suppliers / Purchasing
    public DbSet<SupplierReadModel> Suppliers => Set<SupplierReadModel>();
    public DbSet<GoodsReceiptReadModel> GoodsReceipts => Set<GoodsReceiptReadModel>();
    public DbSet<SupplierPaymentReadModel> SupplierPayments => Set<SupplierPaymentReadModel>();
    public DbSet<PurchaseReturnReadModel> PurchaseReturns => Set<PurchaseReturnReadModel>();

    // Assets
    public DbSet<AssetReadModel> Assets => Set<AssetReadModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // All entity configurations — table names, column names
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Force NoTracking globally
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
}
```

### 9.3 Read Model Convention

Each ReadModel is a flat projection of a table — only the columns needed for analytics:

```csharp
// Example: InvoiceReadModel
public sealed class InvoiceReadModel
{
    public Guid Id { get; init; }
    public Guid PatientId { get; init; }
    public decimal Total { get; init; }
    public string Status { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
    // ... only what analytics needs
}
```

### 9.4 AnalyticsDbContext Registration

```csharp
// AnalyticsModule.cs
services.AddDbContext<AnalyticsDbContext>(opts =>
    opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
```

**No migrations from AnalyticsDbContext.** All tables are owned by their respective module DbContexts. AnalyticsDbContext is purely a read lens.

### 9.5 Query Strategy

For simple KPIs: use EF LINQ on AnalyticsDbContext with `AsNoTracking()` (redundant but explicit).

For complex cross-table aggregations: use raw SQL via `context.Database.SqlQueryRaw<T>()`:

```csharp
// Example: Doctor Revenue Report
var sql = """
    SELECT
        dp.id            AS doctor_id,
        dp.name          AS doctor_name,
        SUM(p.amount)    AS total_collected,
        COUNT(DISTINCT i.id) AS invoice_count
    FROM doctor_profiles dp
    LEFT JOIN invoices i ON i.doctor_id = dp.id
        AND i.created_at BETWEEN {0} AND {1}
    LEFT JOIN payments p ON p.invoice_id = i.id
    GROUP BY dp.id, dp.name
    ORDER BY total_collected DESC
    """;
```

### 9.6 Project Dependencies

```
DentalERP.Modules.Analytics
    → DentalERP.SharedKernel  (only)
    (NO references to other module projects — avoids circular deps)
    (Table mapping done via hard-coded table names in EF configs)
```

### 9.7 AnalyticsModule Registration

```csharp
// Host/Program.cs additions:
builder.Services.AddAnalyticsModule(builder.Configuration);
app.MapAnalyticsModule();
```

---

## 10. Reporting Requirements

### 10.1 Inventory Reports

| Report | Query Source | Filters | Output |
|--------|-------------|---------|--------|
| Stock Valuation | stock_batches × items.unit_cost | Warehouse, Category, Date | Item, Qty, Unit Cost, Total Value |
| Stock Movement Ledger | stock_movements | Item, Type, Destination, Date Range | Movement-by-movement log |
| Expiry Report | stock_batches WHERE expiry_date | Within N days | Batch, Item, Qty, Expiry, Days Left |
| Reorder Report | items WHERE current_stock ≤ reorder_level | Category | Item, Current Qty, Reorder Qty, Preferred Supplier |
| Consumption Report | stock_movements WHERE direction='out' | Item, Date Range, Destination Type | Consumed qty by period |
| Barcode Lookup | items + item_barcodes | Barcode value | Item card |

### 10.2 Purchasing Reports

| Report | Description |
|--------|-------------|
| PO Status Report | Open POs with aging (days since order_date) |
| Goods Receipt Report | All GRs by date/supplier, total amounts |
| Supplier Payables Aging | Balance per supplier, buckets: Current / 30 / 60 / 90+ days |
| Purchase Returns Report | Returns by supplier, reason, status |

### 10.3 Supplier Reports

| Report | Description |
|--------|-------------|
| Supplier Statement | GRs + Payments + Returns for period, running balance |
| Supplier Item Catalog | Items per supplier with last unit cost |

### 10.4 Expense Reports

| Report | Description |
|--------|-------------|
| Expenses by Category | Grouped totals, % of total |
| Expenses by Module | Grouped by related_module (Lab/Radiology/etc.) |
| Monthly Expense Trend | Last 12 months bar chart data |
| Recurring Expense Schedule | Templates with last/next occurrence |

### 10.5 Asset Reports

| Report | Description |
|--------|-------------|
| Asset Register | Full list: code, tag, name, category, purchase cost, current value, status |
| Depreciation Schedule | Annual depreciation per asset, book value over time |
| Maintenance Log Report | All maintenance by asset, cost, performer |
| Assets by Status | Counts: Active / Maintenance / Disposed |
| Warranty Expiry Report | Assets with warranty_expiry within 90 days |

### 10.6 Analytics Reports

| Report | Description |
|--------|-------------|
| Monthly Revenue Summary | Invoiced, collected, collection rate, by doctor, by service |
| Lab Revenue Report | Revenue from completed lab orders (as revenue center) |
| Radiology Revenue Report | Revenue from completed radiology orders (as revenue center) |
| Doctor Performance Report | Revenue attributed, procedures, commissions, net |
| Patient Activity Report | New, returning, inactive (no visit >6 months) |
| Appointment Utilization | Booked vs attended vs no-show by month |
| Treatment Plan Completion | Active plans, average completion %, closed this period |

---

## 11. Migration Plan (023–028)

### Migration 023 — `023_inventory.sql`
```sql
-- item_categories, units_of_measure, items (with barcode column)
-- item_barcodes
-- warehouses (with default warehouse seed)
-- stock_batches, stock_movements (with destination_type, destination_id)
-- Indexes: (item_id, warehouse_id, expiry_date), (item_id, created_at),
--          (movement_type, created_at), (destination_type, destination_id)
-- Seed: 1 default warehouse, 6 item categories, 5 units of measure
```

### Migration 024 — `024_suppliers_purchasing.sql`
```sql
-- suppliers (NO balance column — R4)
-- supplier_items (supplier_item_code junction)
-- supplier_payments
-- purchase_orders, purchase_order_items (supplier_item_id FK)
-- goods_receipts, goods_receipt_items
```

### Migration 025 — `025_purchase_returns.sql`
```sql
-- purchase_returns, purchase_return_items
```

### Migration 026 — `026_expenses.sql`
```sql
-- expense_categories (with seed: Rent, Utilities, Salaries, Medical Supplies, Maintenance, Other)
-- expense_templates
-- expenses (with related_module, related_entity_id columns — R5)
```

### Migration 027 — `027_assets.sql`
```sql
-- asset_categories (with seed: Dental Equipment, Imaging Equipment, Furniture, IT Equipment, Vehicles, Other)
-- assets (with asset_tag column — R6)
-- asset_documents
-- asset_maintenance_logs (with expense_id FK)
```

### Migration 028 — `028_seeds_phase6.sql`
```sql
-- Seed: units_of_measure (Piece, Box, Bottle, Milliliter, Gram, Liter, Meter, Strip)
-- Seed: item_categories (Dental Materials, Anesthetics, Radiology Supplies, Lab Supplies,
--                        Sterilization, PPE, Office Supplies, Medications)
-- Seed: expense_categories (as above)
-- Seed: asset_categories (as above)
-- Seed: 1 default warehouse named "المخزن الرئيسي"
```

---

## 12. Future Accounting Module — Reserved (R9)

### 12.1 Scope Reserved (Not in V1)

The following capabilities are **explicitly out of scope** for V1 and reserved for a future Accounting Module:

| Feature | Notes |
|---------|-------|
| General Ledger (Chart of Accounts) | Journal entries, debit/credit, account codes |
| Trial Balance | Aggregation of all GL entries |
| Profit & Loss Statement | Revenue − Expenses structured report |
| Balance Sheet | Assets, Liabilities, Equity |
| Accounts Payable (AP) Module | Full AP workflow beyond supplier balance tracking |
| Accounts Receivable (AR) Module | Full AR workflow beyond invoice tracking |
| VAT / Tax Engine | Tax codes, VAT returns |
| Multi-Currency | Foreign exchange, rate management |
| Budget Management | Budget vs actuals |
| Bank Reconciliation | Match bank statement to vault transactions |

### 12.2 Compatibility Guardrails

The Phase 6 design is intentionally compatible with a future Accounting Module:

1. **`vault_transactions`** — already structured as a simple ledger. A future GL module can transform these into journal entries without schema changes.

2. **`supplier_payments`** — represents AP payments. Future AP module adds approval workflow on top.

3. **`expenses`** — maps 1:1 to a GL expense entry. `related_module` supports future cost-center allocation.

4. **`IVaultTransactionWriter`** — the interface is a precursor to `IJournalEntryWriter`. The signature will be extended, not replaced.

5. **`AnalyticsDbContext`** — designed to expand. Future Accounting tables will be added as ReadModels without architectural change.

### 12.3 Naming Conventions Preserved

- All financial amounts use `DECIMAL(12,2)` — sufficient for SAR/LYD precision.
- `direction` field (`in`/`out`) mirrors debit/credit in accounting terms.
- `reference_id` + `reference_type` pattern mirrors GL posting references.

---

## 13. Implementation Plan

### 13.1 Module Build Order

Phase 6 is implemented in 4 sequential groups. Each group must build and test before the next begins.

```
Group A — Foundation (Migrations 023–024)
  1. DentalERP.Modules.Inventory
     - Entities: Item, ItemBarcode, Warehouse, StockBatch, StockMovement
     - EF Configurations
     - InventoryDbContext
     - Services: IItemCodeGenerator, IMovementNumberGenerator
     - CQRS: 14 handlers
     - InventoryEndpoints, InventoryModule

  2. DentalERP.Modules.Purchasing (depends on Inventory)
     - Entities: Supplier, SupplierItem, SupplierPayment,
                 PurchaseOrder, POItem, GoodsReceipt, GRItem
     - EF Configurations
     - PurchasingDbContext
     - Services: IPONumberGenerator, IGRNumberGenerator, ISupplierCodeGenerator
     - CQRS: 16 handlers
     - IVaultTransactionWriter (SharedKernel interface + Financial implementation)
     - PurchasingEndpoints, SupplierEndpoints, PurchasingModule

Group B — Supply Chain Completion (Migration 025)
  3. Purchase Returns (within PurchasingDbContext)
     - Entities: PurchaseReturn, PurchaseReturnItem
     - EF Configurations
     - CQRS: 4 handlers (Create, Confirm, List, Detail)
     - Added to PurchasingEndpoints

Group C — Financial Operations (Migration 026–027)
  4. DentalERP.Modules.Expenses
     - Entities: ExpenseCategory, ExpenseTemplate, Expense
     - EF Configurations + ExpensesDbContext
     - Services: IExpenseNumberGenerator
     - CQRS: 6 handlers
     - Expense posts VaultTransaction via IVaultTransactionWriter
     - ExpensesEndpoints, ExpensesModule

  5. DentalERP.Modules.Assets
     - Entities: AssetCategory, Asset, AssetDocument, AssetMaintenanceLog
     - EF Configurations + AssetsDbContext
     - Services: IAssetCodeGenerator
     - CQRS: 10 handlers
     - AssetsEndpoints, AssetsModule

Group D — Intelligence Layer (no migrations)
  6. DentalERP.Modules.Analytics
     - AnalyticsDbContext (read-only, all ReadModels)
     - All EF ReadModel configurations (table mappings only)
     - CQRS: 18 query handlers (Dashboard + 5 analytics domains)
     - AnalyticsEndpoints, AnalyticsModule
```

### 13.2 Host Integration

After each group, update `DentalERP.Host`:
- Add project references to new module csproj files.
- Register module in `Program.cs`: `AddXModule()` + `MapXModule()`.
- Update `DentalERP.sln`.

### 13.3 Deliverables

| Deliverable | Count |
|------------|-------|
| Database Migrations | 6 (023–028) |
| New Module Projects | 5 |
| Domain Entities | ~28 |
| CQRS Handlers | ~68 |
| API Endpoints | ~55 |
| Frontend Screens | 18 (S30–S47) |
| Unit Tests | ≥ 120 new |
| Integration Auth Tests | ≥ 50 new |
| Updated Sidebar | 1 |
| PHASE_6_COMPLETION_REPORT.md | 1 |

---

## 14. Test Strategy

### 14.1 Unit Test Coverage

**Group A — Inventory Domain Tests** (target: 40+ tests)

```
ItemTests:
  - Create item with valid data
  - Create item without barcode (barcode optional)
  - Item code auto-format validation

StockBatchTests:
  - Cannot create batch with quantity ≤ 0
  - Expiry date optional for non-tracked items

StockMovementTests:
  - ManualIssue requires destination_type
  - LabConsumption auto-sets destination_type='Lab'
  - FIFO batch selection — earliest expiry selected first
  - Insufficient stock returns failure Result

SupplierTests:
  - SupplierBalance computed correctly: GRs - Payments - Returns
  - SupplierBalance = 0 on no activity
  - Returns not counted until status='Confirmed'
  - Overpayment produces negative balance

SupplierItemTests:
  - Duplicate supplier_item_code for same supplier fails
  - Same item_code for different suppliers succeeds

PurchaseOrderTests:
  - Status machine: valid transitions
  - Status machine: invalid transitions return failure
  - Approve from Approved fails
  - Cancel after receiving fails

GoodsReceiptTests:
  - Receipt updates po_item.quantity_received
  - Full receipt triggers PO → FullyReceived
  - Partial receipt triggers PO → PartiallyReceived
  - Overage receipt fails (quantity > ordered)
```

**Group B — Expenses Domain Tests** (target: 20+ tests)

```
ExpenseTests:
  - Create expense links to vault
  - Expense with related_module='Assets' stores entity reference
  - Expense with related_module='Laboratory' stores entity reference
  - Invalid related_module value fails validation
  - ExpenseNumber format: EXP-YYYY-NNNNNN

VaultTransactionWriterTests:
  - Expense creation queues VaultTransaction (out)
  - Supplier payment creation queues VaultTransaction (out)
```

**Group C — Asset Domain Tests** (target: 25+ tests)

```
AssetTests:
  - Create asset: status defaults to Active
  - AssetTag uniqueness enforced
  - Depreciation StraightLine: correct computation
  - Depreciation DecliningBalance: correct computation
  - Dispose: sets status=Disposed, disposal_date populated
  - Dispose: subsequent edit attempt fails
  - Lookup by asset_tag returns correct asset

MaintenanceLogTests:
  - Create maintenance log with cost
  - Log with cost creates expense linkage
  - next_maintenance_date is optional
```

**Group D — Analytics Query Tests** (target: 35+ tests)

```
DashboardQueryTests:
  - GetExecutiveDashboard: all KPI sections present
  - Collection rate = 0 when no payments
  - Collection rate = 100% when fully paid
  - Low stock count matches items below reorder_level
  - Expiry count matches batches within 30 days

RevenueReportTests:
  - GroupBy='doctor' returns per-doctor breakdown
  - GroupBy='service' returns per-service breakdown
  - Date range filter applied correctly

SupplierPayablesAgingTests:
  - Aging buckets: Current / 30d / 60d / 90d+
  - Supplier with no GRs not included (balance=0)
  - Supplier with confirmed return shows reduced payable
```

### 14.2 Integration Tests — Auth Guards

One auth-guard test per endpoint (all return 401 without token):

| Module | Endpoint Count | Test Count |
|--------|---------------|-----------|
| Inventory | ~15 | 15 |
| Purchasing | ~12 | 12 |
| Suppliers | ~8 | 8 |
| Expenses | ~6 | 6 |
| Assets | ~8 | 8 |
| Analytics | ~14 | 14 |
| **Total** | **~63** | **~63** |

### 14.3 Build Validation Gate

Before PHASE_6_COMPLETION_REPORT.md is generated:
1. `dotnet build` — 0 errors, 0 warnings treated as errors.
2. `dotnet test` — 100% pass rate, 0 failures.
3. Total test count ≥ 374 (254 existing + 120 new).

---

*PHASE_6_FINAL_DESIGN.md — Version 2.0 — FROZEN — Ready for implementation.*  
*Do not modify this document after implementation begins. Raise change requests as separate addenda.*
