# DEV Environment Setup Guide
**DentalERP — Phase 6 DEV Deployment**
**Date:** 2026-06-18

---

## Prerequisites

| Component | Version | Notes |
|---|---|---|
| .NET SDK | 9.0.314 | `dotnet --version` |
| PostgreSQL | 16.x | Local or Docker |
| Docker (optional) | 24+ | For containerized DB |
| Node.js | 20+ | Frontend (if deploying UI) |

---

## Step 1 — Clone & Build

```bash
git clone <repo-url>
cd DentalERP/backend
dotnet build DentalERP.sln
# Expected: 0 Errors, ~10 Warnings (NU1603 — benign)
```

---

## Step 2 — PostgreSQL Setup

### Option A: Docker (Recommended for DEV)

```bash
docker run -d \
  --name dental-erp-db \
  -e POSTGRES_USER=dentalerp \
  -e POSTGRES_PASSWORD=dentalerp_dev_2026 \
  -e POSTGRES_DB=dentalerp \
  -p 5432:5432 \
  postgres:16-alpine
```

### Option B: Local PostgreSQL

```sql
CREATE USER dentalerp WITH PASSWORD 'dentalerp_dev_2026';
CREATE DATABASE dentalerp OWNER dentalerp;
GRANT ALL PRIVILEGES ON DATABASE dentalerp TO dentalerp;
```

---

## Step 3 — appsettings.Development.json

Create `backend/src/DentalERP.Host/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dentalerp;Username=dentalerp;Password=dentalerp_dev_2026"
  },
  "Jwt": {
    "SecretKey": "DentalERP-DEV-Secret-Key-2026-MustBe32Chars!!",
    "Issuer": "DentalERP",
    "Audience": "DentalERP-Client",
    "ExpiryMinutes": 480
  },
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:5173"
  ],
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
```

---

## Step 4 — Run Migrations

Execute all SQL migrations in order against the DEV database:

```bash
# Using psql
for f in backend/migrations/0*.sql; do
  echo "Applying $f ..."
  psql -h localhost -U dentalerp -d dentalerp -f "$f"
done
```

### Migration Order
| File | Description |
|---|---|
| 001 | Initial schema (users, roles, permissions) |
| 002 | Permissions seed |
| 003 | Audit logs (legacy) |
| 004–012 | Patients, appointments, clinical |
| 013–018 | Financial, invoices, payments, vault |
| 019–020 | Laboratory, radiology |
| 021–022 | Insurance, vault transfers |
| 023 | Inventory (items, warehouses, stock) |
| 024 | Suppliers + purchasing |
| 025 | Purchase returns |
| **026** | **Expenses, cost centers, audit_logs** |
| **027** | **Assets** |

---

## Step 5 — Apply Seed Data

```bash
psql -h localhost -U dentalerp -d dentalerp -f backend/uat/seed_data.sql
```

---

## Step 6 — Run the API

```bash
cd backend/src/DentalERP.Host
dotnet run --environment Development
# API available at: http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
# Health: http://localhost:5000/health
```

---

## Step 7 — Verify Build & Tests

```bash
cd backend
dotnet build DentalERP.sln          # 0 errors
dotnet test DentalERP.sln           # 499/499 PASS
```

---

## Environment Checklist

- [ ] PostgreSQL 16 running and accessible
- [ ] All 27 migrations applied successfully
- [ ] Seed data applied (seed_data.sql)
- [ ] API starts without errors
- [ ] GET http://localhost:5000/health returns `Healthy`
- [ ] Swagger UI loads at http://localhost:5000/swagger
- [ ] Can obtain JWT token via POST /api/auth/login with admin credentials
- [ ] Authenticated requests return 200 (not 401)

---

## Known Issues / Notes

1. **NU1603 Warnings** — Pre-existing, non-blocking. JWT package minor version mismatch.
2. **File Storage (Assets)** — `IFileStorageService` defaults to a no-op stub in DEV unless S3/MinIO is configured. Asset document upload will not persist files but the endpoint will return 200.
3. **Vault Integration (Expenses)** — Expenses with `VaultId` will write to `vault_transactions`. Ensure a vault record exists before testing.
4. **Migration 027 `purchase_date`** — Now nullable (updated from NOT NULL to allow assets without purchase dates).
