# Local Run Checklist
**DentalERP — Phase 6**
**Last Updated:** 2026-06-18

Use this checklist before every DEV session or after pulling new code.

---

## Infrastructure

- [ ] Docker Desktop is running
- [ ] `docker compose -f docker/docker-compose.yml up -d postgres redis minio`
- [ ] All 3 containers show `healthy` (`docker compose -f docker/docker-compose.yml ps`)
- [ ] PostgreSQL port 5432 accessible (`pg_isready -h localhost -U postgres`)

---

## Database

- [ ] All 27 migrations applied (auto-applied by Docker on first run)
- [ ] Seed data applied (`backend/uat/seed_data.sql`)
- [ ] Admin user exists: `SELECT username FROM users WHERE username = 'admin'`
- [ ] 4 roles exist: `SELECT name FROM roles`
- [ ] Expense categories seeded (8 rows in `expense_categories`)
- [ ] Asset categories seeded (6 rows in `asset_categories`)

---

## Backend

- [ ] `cd backend && dotnet build DentalERP.sln` → 0 errors
- [ ] `cd backend/src/DentalERP.Host && dotnet run --environment Development` starts without errors
- [ ] `GET http://localhost:5000/health` returns `Healthy|<timestamp>`
- [ ] `http://localhost:5000/swagger` loads Swagger UI
- [ ] Swagger shows all modules: IAM, Patients, Clinical, Financial, Laboratory, Radiology, Inventory, Purchasing, Expenses, Assets
- [ ] `POST http://localhost:5000/api/auth/login` with `{"username":"admin","password":"Admin@123"}` returns 200 + accessToken

---

## Frontend

- [ ] `frontend/.env.local` exists with `NEXT_PUBLIC_API_URL=http://localhost:5000`
- [ ] `cd frontend && npm install` completes without errors
- [ ] `cd frontend && npm run build` completes with 0 errors (TypeScript check)
- [ ] `cd frontend && npm run dev` starts on port 3000
- [ ] `http://localhost:3000` redirects to `/login`
- [ ] Login with `admin` / `Admin@123` succeeds → redirected to dashboard
- [ ] Sidebar shows all navigation items
- [ ] Clicking "المرضى" (Patients) loads without 401/500 errors

---

## Tests

- [ ] `cd backend && dotnet test DentalERP.sln` → 499/499 PASS
  - [ ] Unit tests: 382/382
  - [ ] Integration tests: 117/117

---

## Post-Session

- [ ] Stop backend (Ctrl+C)
- [ ] Stop frontend (Ctrl+C)
- [ ] Optionally: `docker compose -f docker/docker-compose.yml down` (data persists in volumes)
