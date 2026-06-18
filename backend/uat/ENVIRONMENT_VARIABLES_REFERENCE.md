# Environment Variables Reference
**DentalERP — Phase 6**
**Last Updated:** 2026-06-18

---

## Backend (`appsettings.json` / Environment Variables)

### Connection Strings

| Key | Default (DEV) | Docker Compose | Description |
|---|---|---|---|
| `ConnectionStrings:DefaultConnection` | `Host=localhost;Port=5432;Database=dentalerp;Username=postgres;Password=postgres` | `Host=postgres;Port=5432;...` | PostgreSQL connection |

### JWT Authentication

| Key | Default (DEV) | Required | Description |
|---|---|---|---|
| `Jwt:SecretKey` | `CHANGE_ME_TO_A_STRONG_SECRET_KEY_AT_LEAST_32_CHARS` | **Yes** | Minimum 32 characters. Change before production |
| `Jwt:Issuer` | `DentalERP` | Yes | JWT issuer claim |
| `Jwt:Audience` | `DentalERP-Clients` | Yes | JWT audience claim |
| `Jwt:AccessTokenExpiryMinutes` | `60` | No | Access token lifetime (minutes) |
| `Jwt:RefreshTokenExpiryDays` | `30` | No | Refresh token lifetime (days) |

> **Critical:** `Jwt:Audience` must be **exactly** `DentalERP-Clients` (with the trailing `s`). The frontend authStore expects this audience.

### CORS

| Key | Default | Description |
|---|---|---|
| `AllowedOrigins` | `["http://localhost:3000","http://localhost"]` | Allowed frontend origins |

### MinIO / File Storage

| Key | Default | Description |
|---|---|---|
| `Minio:Endpoint` | `localhost:9000` | MinIO server address |
| `Minio:AccessKey` | `minioadmin` | MinIO access key |
| `Minio:SecretKey` | `minioadmin123` | MinIO secret key |
| `Minio:UseSSL` | `false` | Enable HTTPS for MinIO |

> In DEV without MinIO configured, `IFileStorageService` uses a no-op stub. Asset documents upload silently without storing files.

### Logging (Serilog)

| Key | Default | Description |
|---|---|---|
| `Serilog:MinimumLevel:Default` | `Information` | Root log level |
| `Serilog:MinimumLevel:Override:Microsoft` | `Warning` | ASP.NET framework logs |
| `Serilog:MinimumLevel:Override:System` | `Warning` | System logs |

---

## Frontend (`.env.local`)

| Variable | DEV Value | Docker Value | Description |
|---|---|---|---|
| `NEXT_PUBLIC_API_URL` | `http://localhost:5000` | `http://localhost:8080` | Backend API base URL |

> **Important:** All frontend env vars prefixed with `NEXT_PUBLIC_` are embedded at build time and visible in the browser. Never put secrets here.

---

## Docker Compose (`docker/.env`)

Create `docker/.env` (not committed) for Docker Compose overrides:

```env
# PostgreSQL
DB_PASSWORD=postgres

# Redis
REDIS_PASSWORD=redis

# MinIO
MINIO_USER=minioadmin
MINIO_PASSWORD=minioadmin123

# JWT — CHANGE THIS before production
JWT_SECRET_KEY=DentalERP-DEV-Secret-Key-2026-MustBe32Chars!!
```

---

## Production Overrides

For production, override all defaults via environment variables (never commit passwords):

```bash
# Backend
export ConnectionStrings__DefaultConnection="Host=prod-db;Port=5432;Database=dentalerp;Username=erp_user;Password=<strong-password>"
export Jwt__SecretKey="<64-char-random-secret>"
export AllowedOrigins__0="https://your-domain.com"

# Frontend
NEXT_PUBLIC_API_URL=https://api.your-domain.com
```

---

## ASP.NET Core Environment

| Value | Swagger | Logging | Migrations |
|---|---|---|---|
| `Development` | Enabled | Verbose | Raw SQL (manual) |
| `Production` | Disabled | Info+ | Raw SQL (manual) |

Set via `ASPNETCORE_ENVIRONMENT` environment variable or `--environment` flag.
