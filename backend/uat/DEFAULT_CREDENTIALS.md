# Default Credentials
**DentalERP — Phase 6 DEV Environment**
**Last Updated:** 2026-06-18

> **Security Notice:** These are DEV-only default credentials. Change ALL passwords before deploying to production.

---

## Application Users

These users are seeded by migrations (002) and `seed_data.sql`:

| Role | Username | Password | Full Name | Permissions |
|---|---|---|---|---|
| **Administrator** | `admin` | `Admin@123` | مدير النظام | ALL (40 permissions) |
| **Receptionist** | `reception` | `Admin@123` | موظف الاستقبال | Patients.View/Create/Edit, Appointments.* |
| **Doctor** | `doctor` | `Admin@123` | الدكتور أحمد | Patients.*, Appointments.View/Create, Clinical.*, Lab.View/Create, Radiology.View/Create |
| **Accountant** | `accountant` | `Admin@123` | المحاسب محمد | Patients.View, Treasury.*, Inventory.View, Reports.* |

---

## Role Permission Matrix

| Permission | Administrator | Receptionist | Doctor | Accountant |
|---|---|---|---|---|
| Users.* | ✅ | ❌ | ❌ | ❌ |
| Roles.* | ✅ | ❌ | ❌ | ❌ |
| Settings.* | ✅ | ❌ | ❌ | ❌ |
| Patients.View | ✅ | ✅ | ✅ | ✅ |
| Patients.Create | ✅ | ✅ | ✅ | ❌ |
| Patients.Edit | ✅ | ✅ | ✅ | ❌ |
| Patients.Delete | ✅ | ❌ | ❌ | ❌ |
| Appointments.* | ✅ | ✅ | View/Create | ❌ |
| Clinical.* | ✅ | ❌ | View/Create/Edit | ❌ |
| Treasury.* | ✅ | ❌ | ❌ | View/Create/Edit |
| Inventory.* | ✅ | ❌ | ❌ | View |
| Lab.* | ✅ | ❌ | View/Create | ❌ |
| Radiology.* | ✅ | ❌ | View/Create | ❌ |
| Reports.* | ✅ | ❌ | ❌ | View/Export |

---

## Infrastructure Credentials

### PostgreSQL (Docker)

| Parameter | Value |
|---|---|
| Host | `localhost` |
| Port | `5432` |
| Database | `dentalerp` |
| Username | `postgres` |
| Password | `postgres` |

Connection string: `Host=localhost;Port=5432;Database=dentalerp;Username=postgres;Password=postgres`

### Redis (Docker)

| Parameter | Value |
|---|---|
| Host | `localhost` |
| Port | `6379` |
| Password | `redis` |

### MinIO (Docker)

| Parameter | Value |
|---|---|
| API Endpoint | `http://localhost:9000` |
| Web Console | `http://localhost:9001` |
| Access Key | `minioadmin` |
| Secret Key | `minioadmin123` |

---

## JWT

| Parameter | DEV Value |
|---|---|
| Secret Key | `CHANGE_ME_TO_A_STRONG_SECRET_KEY_AT_LEAST_32_CHARS` |
| Issuer | `DentalERP` |
| Audience | `DentalERP-Clients` |
| Access Token Expiry | 60 minutes |
| Refresh Token Expiry | 30 days |

---

## Password Hash Details

All DEV seed users use the same bcrypt hash (cost factor 11):

```
Hash: $2a$11$K7Q3FnLpqyJm8/.1KGVBQeWzOE6y1DZH.o/PbGZ0VPtFkQ9UoMT0a
Plaintext: Admin@123
Algorithm: BCrypt, cost: 11
```

This hash is seeded directly in migration 002 (admin) and `seed_data.sql` (other users).

---

## Login Test

```http
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

Expected response:
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "...",
  "userId": "00000000-0000-0000-0000-000000000001",
  "username": "admin",
  "fullName": "مدير النظام",
  "permissions": ["Users.View", "Users.Create", ...]
}
```
