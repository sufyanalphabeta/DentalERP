# Backend Run Guide
**DentalERP — Phase 6**
**Last Updated:** 2026-06-18

---

## Quick Start

```powershell
# 1. From repo root — ensure PostgreSQL is running
docker compose -f docker/docker-compose.yml up -d postgres

# 2. Run the backend
cd backend/src/DentalERP.Host
dotnet run --environment Development
```

API is ready when you see:
```
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

---

## URLs

| URL | Description |
|---|---|
| `http://localhost:5000` | API root |
| `http://localhost:5000/swagger` | Swagger UI (DEV only) |
| `http://localhost:5000/health` | Health check (unauthenticated) |
| `http://localhost:5000/api/auth/login` | JWT login endpoint |

---

## Prerequisites

| Item | Version |
|---|---|
| .NET SDK | 9.0.x (`dotnet --version`) |
| PostgreSQL | 16.x (via Docker or local) |

---

## Build Commands

```powershell
cd backend

# Restore packages
dotnet restore DentalERP.sln

# Build (Debug)
dotnet build DentalERP.sln
# Expected: Build succeeded. 0 Error(s), ~5 NU1603 warnings (benign)

# Build (Release)
dotnet build DentalERP.sln -c Release

# Run tests
dotnet test DentalERP.sln
# Expected: 499/499 PASS (382 unit + 117 integration)

# Run the host
dotnet run --project src/DentalERP.Host --environment Development
```

---

## Solution Structure

```
DentalERP.sln
├── src/
│   ├── DentalERP.Host/              ← ASP.NET Core entry point (port 5000)
│   ├── DentalERP.SharedKernel/      ← BaseEntity, Result<T>, interfaces
│   ├── DentalERP.Modules.IAM/       ← Auth, Users, Roles, Permissions
│   ├── DentalERP.Modules.Patients/  ← Patients, Appointments, Queue
│   ├── DentalERP.Modules.Clinical/  ← Dental chart, Treatment plans, Procedures
│   ├── DentalERP.Modules.Financial/ ← Invoices, Payments, Vault, Commissions
│   ├── DentalERP.Modules.Laboratory/← Lab orders
│   ├── DentalERP.Modules.Radiology/ ← Radiology orders
│   ├── DentalERP.Modules.Inventory/ ← Items, Warehouses, Stock
│   ├── DentalERP.Modules.Purchasing/← Suppliers, POs, Purchase Returns
│   ├── DentalERP.Modules.Expenses/  ← Expense categories, Templates, Expenses
│   └── DentalERP.Modules.Assets/    ← Asset categories, Assets, Maintenance
└── tests/
    ├── DentalERP.UnitTests/         ← 382 unit tests
    └── DentalERP.IntegrationTests/  ← 117 integration tests (Testcontainers)
```

---

## API Modules in Swagger

After starting the API, open `http://localhost:5000/swagger`. You will see these route groups:

| Group | Base Path | Endpoints |
|---|---|---|
| Auth | `/api/auth` | login, refresh-token, logout, change-password |
| Users | `/api/users` | CRUD |
| Roles | `/api/roles` | CRUD + permissions |
| Patients | `/api/patients` | CRUD + search |
| Appointments | `/api/appointments` | CRUD + status |
| Clinical | `/api/clinical` | Chart, treatment plans, procedures |
| Financial | `/api/invoices`, `/api/payments`, `/api/vaults` | Billing, vault management |
| Laboratory | `/api/lab` | Lab orders |
| Radiology | `/api/radiology` | Radiology orders |
| Inventory | `/api/inventory` | Items, stock |
| Purchasing | `/api/purchasing` | POs, purchase returns |
| Expenses | `/api/expenses` | Categories, templates, expenses, PDF |
| Assets | `/api/assets` | Categories, assets, documents, maintenance |
| System | `/health` | Health check |

---

## Test Login via Swagger

1. Open `http://localhost:5000/swagger`
2. Find `POST /api/auth/login`
3. Click **Try it out**
4. Send: `{"username": "admin", "password": "Admin@123"}`
5. Copy the `accessToken` from the response
6. Click the **Authorize** button (top-right)
7. Enter: `Bearer <your-token>`
8. All subsequent requests will be authenticated

---

## Configuration

### `appsettings.json` (committed, DEV defaults)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dentalerp;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "SecretKey": "CHANGE_ME_TO_A_STRONG_SECRET_KEY_AT_LEAST_32_CHARS",
    "Issuer": "DentalERP",
    "Audience": "DentalERP-Clients"
  }
}
```

### `appsettings.Development.json` (optional local override)

Create this file to override without modifying the committed `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=dentalerp;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "SecretKey": "DentalERP-DEV-Secret-Key-2026-MustBe32CharsLong!!"
  }
}
```

---

## Known Warnings (Non-Blocking)

```
warning NU1603: DentalERP.Modules.IAM depends on System.IdentityModel.Tokens.Jwt (>= 8.3.3)
               but 8.3.3 was not found. 8.4.0 was resolved instead.
warning NU1603: DentalERP.IntegrationTests depends on Testcontainers.PostgreSql (>= 3.11.0)
               but 3.11.0 was not found. 4.0.0 was resolved instead.
```

These are pre-existing benign version resolution warnings. They do not affect functionality or tests.

---

## Logs

Log files are written to `backend/src/DentalERP.Host/logs/app-YYYY-MM-DD.log` (Serilog rolling file).

View live in terminal during `dotnet run`.
