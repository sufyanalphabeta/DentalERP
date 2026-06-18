# Docker Deployment Guide
**DentalERP — Phase 6**
**Last Updated:** 2026-06-18

---

## Overview

The `docker/docker-compose.yml` file defines the full DentalERP stack:

| Service | Image | Port | Purpose |
|---|---|---|---|
| `postgres` | postgres:16-alpine | 5432 | Primary database |
| `redis` | redis:7-alpine | 6379 | Session cache / future queue |
| `minio` | minio/minio:latest | 9000, 9001 | File storage (asset documents) |
| `backend` | custom (Dockerfile) | 8080 | ASP.NET Core API |
| `frontend` | custom (Dockerfile) | 3000 | Next.js frontend |
| `nginx` | nginx:alpine | 80, 443 | Reverse proxy |

---

## DEV: Infrastructure Only (Recommended)

For local development, run only infrastructure and start backend/frontend natively:

```powershell
# Start only infrastructure
docker compose -f docker/docker-compose.yml up -d postgres redis minio

# Check status
docker compose -f docker/docker-compose.yml ps
```

Then run backend with `dotnet run` and frontend with `npm run dev`.

---

## FULL STACK: All Services

### Prerequisites

1. Create `docker/.env` with secrets:

```env
DB_PASSWORD=postgres
REDIS_PASSWORD=redis
MINIO_USER=minioadmin
MINIO_PASSWORD=minioadmin123
JWT_SECRET_KEY=DentalERP-DEV-Secret-Key-2026-MustBe32CharsLong!!
```

2. Backend Dockerfile is at `backend/src/DentalERP.Host/Dockerfile` (targets .NET 9)

3. Frontend Dockerfile — create `frontend/Dockerfile`:

```dockerfile
FROM node:20-alpine AS base
WORKDIR /app

FROM base AS deps
COPY package*.json ./
RUN npm ci

FROM base AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
ENV NEXT_PUBLIC_API_URL=http://localhost/api
RUN npm run build

FROM base AS runner
WORKDIR /app
ENV NODE_ENV=production
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next/standalone ./
COPY --from=builder /app/.next/static ./.next/static
EXPOSE 3000
CMD ["node", "server.js"]
```

4. **Note:** The `nginx` service requires `docker/nginx/nginx.conf`. If it doesn't exist, skip nginx and use direct ports.

---

## Commands

```powershell
# Start all services
docker compose -f docker/docker-compose.yml up -d

# Start specific services
docker compose -f docker/docker-compose.yml up -d postgres redis minio

# View logs
docker compose -f docker/docker-compose.yml logs -f backend
docker compose -f docker/docker-compose.yml logs -f postgres

# Stop all (keep data)
docker compose -f docker/docker-compose.yml down

# Stop all and remove volumes (DESTRUCTIVE — deletes all data)
docker compose -f docker/docker-compose.yml down -v

# Rebuild images
docker compose -f docker/docker-compose.yml build
docker compose -f docker/docker-compose.yml up -d --force-recreate
```

---

## Database Initialization

PostgreSQL automatically runs all `.sql` files mounted at `/docker-entrypoint-initdb.d/` on **first container start**. The `docker-compose.yml` mounts:

```yaml
volumes:
  - ../backend/migrations:/docker-entrypoint-initdb.d:ro
```

This runs migrations 001→027 in alphabetical order automatically.

> **Only on first start.** To re-run migrations manually on an existing container, exec into the container.

---

## Apply Seed Data

After PostgreSQL starts:

```powershell
# Windows PowerShell
Get-Content backend/uat/seed_data.sql | docker exec -i dentalerp-postgres psql -U postgres -d dentalerp
```

---

## MinIO Setup (Asset Documents)

MinIO stores uploaded asset documents (invoices, warranties, manuals).

**Create required buckets after starting MinIO:**

```powershell
# Install mc (MinIO client) or use web console
# Web console: http://localhost:9001
# Login: minioadmin / minioadmin123

# Create bucket via CLI
docker exec -it dentalerp-minio mc alias set local http://localhost:9000 minioadmin minioadmin123
docker exec -it dentalerp-minio mc mb local/asset-documents
docker exec -it dentalerp-minio mc mb local/patient-media
```

---

## Service Health Checks

```powershell
# Check all services
docker compose -f docker/docker-compose.yml ps

# PostgreSQL
docker exec dentalerp-postgres pg_isready -U postgres -d dentalerp

# Redis
docker exec dentalerp-redis redis-cli ping

# MinIO
Invoke-WebRequest http://localhost:9000/minio/health/live

# Backend API
Invoke-WebRequest http://localhost:8080/health

# Frontend
Invoke-WebRequest http://localhost:3000
```

---

## Port Summary

| Service | DEV (native) | Docker |
|---|---|---|
| Backend API | `:5000` | `:8080` |
| Frontend | `:3000` | `:3000` |
| PostgreSQL | `:5432` | `:5432` |
| Redis | N/A | `:6379` |
| MinIO API | N/A | `:9000` |
| MinIO Console | N/A | `:9001` |
| Nginx | N/A | `:80` / `:443` |

---

## Volumes

| Volume | Purpose | Location in Container |
|---|---|---|
| `postgres_data` | PostgreSQL data | `/var/lib/postgresql/data` |
| `redis_data` | Redis persistence | `/data` |
| `minio_data` | File storage | `/data` |

Data persists across `docker compose down`. Only `docker compose down -v` removes it.
