# Database Deployment Order
**DentalERP — Phase 6**
**Last Updated:** 2026-06-18

---

## Overview

All migrations are plain SQL files in `backend/migrations/`. They must be applied in **strict numerical order** (001 → 027). Each file is idempotent (`IF NOT EXISTS`, `ON CONFLICT DO NOTHING`) — safe to re-run.

The Docker Compose PostgreSQL service auto-runs all files from `backend/migrations/` on first container initialization.

---

## Migration Table

| # | File | Module | Tables Created | Notes |
|---|---|---|---|---|
| 001 | `001_initial_schema.sql` | IAM | `permissions`, `roles`, `role_permissions`, `users`, `user_roles`, `refresh_tokens`, `system_settings` | Foundation schema |
| 002 | `002_permissions_seed.sql` | IAM | *(seed only)* | Seeds 40 permissions, 4 roles, admin user (password: `Admin@123`), system settings |
| 003 | `003_audit_logs.sql` | IAM | *(legacy audit table)* | Pre-Phase-6 audit logs — superseded by module-level `audit_logs` in 026 |
| 004 | `004_patients_appointments.sql` | Patients | `patients`, `appointment_types`, `appointments`, `reception_queue` | Core clinical entities |
| 005 | `005_patient_insurance.sql` | Patients | `insurance_companies`, `patient_insurance` | Patient insurance coverage |
| 006 | `006_teeth_seed.sql` | Clinical | *(seed only)* | Seeds 32 permanent + 20 primary teeth |
| 007 | `007_dental_chart.sql` | Clinical | `dental_chart_entries` | Per-tooth clinical records |
| 008 | `008_treatment_plans.sql` | Clinical | `treatment_plans`, `treatment_plan_items` | Treatment planning |
| 009 | `009_procedures.sql` | Clinical | `clinical_procedures` | Completed procedure records |
| 010 | `010_patient_media.sql` | Clinical | `patient_media` | Photos, X-rays, documents |
| 011 | `011_doctor_assignments.sql` | Clinical | `doctor_patient_assignments` | Doctor ownership |
| 012 | `012_patient_timeline.sql` | Clinical | `patient_timeline_events` | Timeline events |
| 013 | `013_service_catalog.sql` | Financial | `service_categories`, `services` | Billable services catalog |
| 014 | `014_vaults_doctor_profiles.sql` | Financial | `vaults`, `vault_transfers`, `doctor_profiles` | Vault (cash) management, doctor commissions |
| 015 | `015_procedures_alter.sql` | Clinical | *(ALTER only)* | Adds `service_id` to `clinical_procedures` |
| 016 | `016_invoices.sql` | Financial | `invoices`, `invoice_items` | Patient invoices |
| 017 | `017_payments_vault_transactions.sql` | Financial | `payments`, `vault_transactions` | Payments + vault ledger |
| 018 | `018_installments_commissions.sql` | Financial | `installment_plans`, `installment_payments`, `doctor_commissions` | Installment plans, commissions |
| 019 | `019_laboratory.sql` | Laboratory | `lab_tests`, `lab_orders`, `lab_order_items` | Lab orders |
| 020 | `020_radiology.sql` | Radiology | `radiology_types`, `radiology_orders` | Radiology orders |
| 021 | `021_insurance_accounts.sql` | Financial | `insurance_receivables`, `insurance_claims`, `insurance_claim_items` | Insurance billing |
| 022 | `022_vault_transfers.sql` | Financial | *(ALTER only)* | Vault transfer enhancements |
| 023 | `023_inventory.sql` | Inventory | `warehouses`, `item_categories`, `items`, `stock_movements`, `stock_adjustments` | Inventory management |
| 024 | `024_suppliers_purchasing.sql` | Purchasing | `suppliers`, `purchase_orders`, `purchase_order_items` | Supplier + PO management |
| 025 | `025_purchase_returns.sql` | Purchasing | `purchase_returns`, `purchase_return_items` | Purchase return management |
| **026** | **`026_expenses.sql`** | **Expenses** | **`audit_logs`, `cost_centers` (seeded), `expense_categories`, `expense_templates`, `expenses`** | **Phase 6 Group C** |
| **027** | **`027_assets.sql`** | **Assets** | **`asset_categories`, `assets`, `asset_documents`, `asset_maintenance`** | **Phase 6 Group C** |

---

## Dependency Graph

```
001 (IAM base)
 └── 002 (seed data — requires 001)
     └── 003 (audit logs)
         └── 004 (patients, appointments — requires users from 001)
             └── 005 (patient insurance — requires patients)
             └── 006 (teeth seed)
                 └── 007 (dental chart — requires patients, teeth)
                     └── 008 (treatment plans — requires patients)
                         └── 009 (procedures — requires treatment_plans)
                             └── 010 (media — requires patients)
                             └── 011 (doctor assignments — requires users, patients)
                             └── 012 (timeline — requires patients)
                                 └── 013 (services — financial)
                                     └── 014 (vaults, doctors — requires users)
                                         └── 015 (alter procedures — requires services)
                                             └── 016 (invoices — requires patients, services)
                                                 └── 017 (payments — requires invoices, vaults)
                                                     └── 018 (installments — requires invoices)
                                                         └── 019 (lab — requires patients, users)
                                                             └── 020 (radiology — requires patients)
                                                                 └── 021 (insurance — requires patients)
                                                                     └── 022 (vault alter — requires vaults)
                                                                         └── 023 (inventory)
                                                                             └── 024 (purchasing — requires inventory, suppliers)
                                                                                 └── 025 (purchase returns — requires purchasing)
                                                                                     └── 026 (expenses — Phase 6)
                                                                                         └── 027 (assets — requires expenses)
```

---

## Apply Migrations Manually

### Using Docker (recommended)

PostgreSQL Docker container auto-runs migrations on first start. For re-application:

```powershell
# Apply single migration
docker exec -i dentalerp-postgres psql -U postgres -d dentalerp `
  < backend/migrations/026_expenses.sql

# Apply all migrations in order
Get-ChildItem backend/migrations/*.sql | Sort-Object Name | ForEach-Object {
    Write-Host "Applying $($_.Name)..."
    docker exec -i dentalerp-postgres psql -U postgres -d dentalerp < $_.FullName
}
```

### Using psql locally

```powershell
Get-ChildItem backend/migrations/*.sql | Sort-Object Name | ForEach-Object {
    Write-Host "Applying $($_.Name)..."
    psql -h localhost -U postgres -d dentalerp -f $_.FullName
}
```

---

## Verification Queries

After applying all migrations, verify with:

```sql
-- Check all expected tables exist
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;

-- Expected: 50+ tables including:
-- users, roles, patients, appointments, vaults, expenses, assets, etc.

-- Check cost centers seeded
SELECT code FROM cost_centers ORDER BY code;
-- Expected: ADMINISTRATION, CLINIC, GENERAL, LABORATORY, RADIOLOGY, TRAINING

-- Check admin user
SELECT username, full_name, is_active FROM users WHERE username = 'admin';
-- Expected: admin | مدير النظام | true

-- Check roles
SELECT name FROM roles ORDER BY name;
-- Expected: Accountant, Administrator, Doctor, Receptionist
```

---

## Known Considerations

1. **Migration 003** (`audit_logs`) creates an older audit log table. **Migration 026** creates the Phase 6 `audit_logs` table used by Expenses and Assets modules. Both use `CREATE TABLE IF NOT EXISTS` — no conflict.
2. **Migration 015** only alters `clinical_procedures` — no new table.
3. **Migration 022** only alters vault-related tables — no new table.
4. **Migration 027** `purchase_date` column is **nullable** (fixed in Phase 6). Assets can be registered without a purchase date.
