# Local Development Setup
**DentalERP — Phase 6**
**Last Updated:** 2026-06-18

---

## Overview

DentalERP runs as two processes locally:
- **Backend**: ASP.NET Core 9 API → `http://localhost:5000`
- **Frontend**: Next.js 16 → `http://localhost:3000`

Database and services run via Docker Compose.

---

## Prerequisites

| Tool | Required Version | Check Command | Install |
|---|---|---|---|
| .NET SDK | 9.0.x | `dotnet --version` | [dot.net](https://dot.net) |
| Node.js | 20+ (tested: 24.16.0) | `node --version` | [nodejs.org](https://nodejs.org) |
| npm | 10+ (tested: 11.13.0) | `npm --version` | bundled with Node |
| Docker Desktop | 24+ | `docker --version` | [docker.com](https://docker.com) |
| Git | any | `git --version` | [git-scm.com](https://git-scm.com) |

---

## Step 1 — Start Infrastructure (Docker)

From the repo root:

```powershell
docker compose -f docker/docker-compose.yml up -d postgres redis minio
```

Wait for healthy status:

```powershell
docker compose -f docker/docker-compose.yml ps
```

All three services should show `healthy`.

> **Note:** PostgreSQL automatically runs all `backend/migrations/*.sql` files on first startup (sorted alphabetically 001→027). This takes ~10 seconds on first run.

---

## Step 2 — Apply Seed Data

```powershell
docker exec -i dentalerp-postgres psql -U postgres -d dentalerp `
  -f /dev/stdin < backend/uat/seed_data.sql
```

Or with psql installed locally:

```powershell
psql -h localhost -U postgres -d dentalerp -f backend/uat/seed_data.sql
```

Password: `postgres` (default)

---

## Step 3 — Run the Backend API

```powershell
cd backend/src/DentalERP.Host
dotnet run --environment Development
```

API available at:
- API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`
- Health: `http://localhost:5000/health`

---

## Step 4 — Run the Frontend

```powershell
cd frontend
npm install        # first time only
npm run dev
```

Frontend available at: `http://localhost:3000`

---

## Step 5 — Login

Open `http://localhost:3000` in your browser. You will be redirected to `/login`.

| Role | Username | Password |
|---|---|---|
| Administrator | `admin` | `Admin@123` |
| Receptionist | `reception` | `Admin@123` |
| Doctor | `doctor` | `Admin@123` |
| Accountant | `accountant` | `Admin@123` |

---

## Environment Variables

### Backend (`backend/src/DentalERP.Host/appsettings.json`)

Already configured with defaults for local PostgreSQL (`localhost:5432`, user: `postgres`).

No additional configuration needed for local dev.

### Frontend (`frontend/.env.local`)

Already created. Contents:

```
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

## Running Tests

```powershell
cd backend
dotnet test DentalERP.sln
# Expected: 499/499 PASS (382 unit + 117 integration)
```

Integration tests use **Testcontainers** — they spin up a real PostgreSQL container automatically. Docker Desktop must be running.

---

## Stopping Everything

```powershell
# Stop Docker services
docker compose -f docker/docker-compose.yml down

# Stop backend: Ctrl+C in the dotnet run terminal
# Stop frontend: Ctrl+C in the npm run dev terminal
```

---

## Troubleshooting

| Problem | Solution |
|---|---|
| Backend fails: "Jwt:SecretKey is required" | Check `appsettings.json` — SecretKey is pre-filled |
| PostgreSQL connection refused | Ensure Docker Desktop is running and `dentalerp-postgres` is healthy |
| Frontend shows login page but login fails | Confirm backend is running on port 5000; check `.env.local` |
| `dotnet run` shows port 5000 in use | Kill the process: `netstat -ano \| findstr :5000` then `taskkill /PID <pid> /F` |
| npm: module not found | Run `npm install` in the `frontend/` directory |
