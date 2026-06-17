# 08 — Deployment Architecture
# بنية النشر والتنصيب — DentalERP

> **الإصدار:** V-Final | **التاريخ:** 2026-06-16

---

## 1. نموذج النشر

**DentalERP = Single Tenant On-Premise**

```
كل عيادة = خادم مستقل داخل الشبكة المحلية (LAN)
لا Cloud، لا SaaS، لا Multi-tenant
كل تنصيب = DB مستقلة + Application مستقل + بيانات مستقلة
```

**لماذا On-Premise؟**
1. البيانات الطبية حساسة — تبقى داخل الشبكة المحلية للعيادة
2. العمل الكامل بلا Internet — يشمل المرضى، العيادة، المعمل، الأشعة، الخزينة، التقارير
3. سهولة النسخ الاحتياطي بواسطة طاقم العيادة
4. لا اعتماد على Cloud لأي وظيفة تشغيلية

---

## 2. بنية Offline الرسمية — Local Server First

**المبدأ:** السيرفر المحلي داخل المركز الطبي هو مصدر الحقيقة الوحيد.  
لا Client-Side Database، لا IndexedDB، لا Browser Sync، لا PWA Offline Data.

| الحالة | ما يعمل |
|--------|---------|
| **LAN طبيعي (اتصال الإنترنت موجود أو لا)** | كل النظام يعمل بالكامل Real-time |
| **انقطاع الإنترنت (LAN يعمل)** | كل النظام يعمل بالكامل — لا Internet dependency |
| **وصول خارجي (خارج المركز)** | Cloudflare Tunnel أو Reverse Proxy إلى السيرفر المحلي |

**ما يعمل عند انقطاع الإنترنت:**
- ✅ استقبال المرضى والمواعيد
- ✅ الإجراءات السريرية والخريطة
- ✅ الفواتير والتحصيل والخزينة
- ✅ أوامر المعمل وطلبات الأشعة
- ✅ المخزون والمشتريات
- ✅ التقارير والإحصاءات

---

## 3. متطلبات الخادم (Server Requirements)

### الخادم الرئيسي (الحد الأدنى للإنتاج)

| المكوّن | المتطلب |
|---------|---------|
| CPU | 4 Cores (Intel Core i5/i7 أو Xeon) |
| RAM | 16 GB (8 GB حد أدنى للاختبار) |
| Storage | 500 GB SSD (يزيد مع أرشيف الصور) |
| OS | Ubuntu Server 22.04 LTS أو Windows Server 2022 |
| Network | شبكة LAN Gigabit |

### الخادم للعيادة الكبيرة (أكثر من 5 أطباء / 200+ مريض/يوم)

| المكوّن | المتطلب |
|---------|---------|
| CPU | 8 Cores |
| RAM | 32 GB |
| Storage | 1 TB SSD NVMe |
| Network | Gigabit Ethernet |

---

## 4. مكوّنات النشر (Docker Compose)

```
┌─────────────────────────────────────────────────────────┐
│                    Docker Host (الخادم)                  │
│                                                         │
│  ┌─────────────┐   ┌─────────────┐   ┌───────────────┐ │
│  │    Nginx    │   │  Next.js 15 │   │  ASP.NET Core │ │
│  │  (Reverse   │──▶│  Frontend   │   │  Backend API  │ │
│  │   Proxy)    │   │  :3000      │   │  :5000        │ │
│  │  :80/:443   │   └─────────────┘   └───────────────┘ │
│  └─────────────┘          │                  │         │
│                           └──────────────────┘         │
│                                    │                   │
│  ┌──────────────┐  ┌──────────────┐ ┌──────────────┐   │
│  │ PostgreSQL16 │  │   Redis 7    │ │   Hangfire   │   │
│  │   :5432      │  │   :6379      │ │  Dashboard   │   │
│  └──────────────┘  └──────────────┘ └──────────────┘   │
│                                                         │
│  ┌──────────────┐   ┌──────────────────────────────┐   │
│  │    MinIO     │   │     Local File Storage       │   │
│  │  :9000/9001  │   │   /app/uploads/ (patients)   │   │
│  │ (Radiology)  │   │                              │   │
│  └──────────────┘   └──────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
```

---

## 5. Docker Compose Configuration

```yaml
# docker-compose.yml
version: '3.9'

services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./nginx/ssl:/etc/nginx/ssl:ro
      - uploads:/app/uploads:ro
    depends_on:
      - frontend
      - backend
    restart: unless-stopped

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    environment:
      - NEXT_PUBLIC_API_URL=http://backend:5000
      - NEXT_PUBLIC_SIGNALR_URL=http://backend:5000/hubs
    depends_on:
      - backend
    restart: unless-stopped

  backend:
    build:
      context: ./backend
      dockerfile: Dockerfile
    environment:
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=dental_erp;Username=dental_user;Password=${DB_PASSWORD}
      - ConnectionStrings__Redis=redis:6379
      - Jwt__PrivateKeyPath=/run/secrets/jwt_private_key
      - Jwt__PublicKeyPath=/run/secrets/jwt_public_key
      - Storage__UploadsPath=/app/uploads
      - MinIO__Endpoint=minio:9000
      - MinIO__AccessKey=${MINIO_USER}
      - MinIO__SecretKey=${MINIO_PASSWORD}
      - MinIO__BucketRadiology=radiology
      - MinIO__UseSSL=false
    volumes:
      - uploads:/app/uploads
    secrets:
      - jwt_private_key
      - jwt_public_key
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_started
    restart: unless-stopped

  postgres:
    image: postgres:16-alpine
    environment:
      - POSTGRES_DB=dental_erp
      - POSTGRES_USER=dental_user
      - POSTGRES_PASSWORD=${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U dental_user -d dental_erp"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

  redis:
    image: redis:7-alpine
    command: redis-server --requirepass ${REDIS_PASSWORD} --save 60 1000
    volumes:
      - redis_data:/data
    restart: unless-stopped

  minio:
    image: minio/minio:latest
    ports:
      - "9000:9000"   # API
      - "9001:9001"   # Console (admin UI)
    environment:
      - MINIO_ROOT_USER=${MINIO_USER}
      - MINIO_ROOT_PASSWORD=${MINIO_PASSWORD}
    volumes:
      - minio_data:/data
    command: server /data --console-address ":9001"
    healthcheck:
      test: ["CMD", "mc", "ready", "local"]
      interval: 30s
      timeout: 20s
      retries: 3
    restart: unless-stopped

volumes:
  postgres_data:
  redis_data:
  uploads:
  minio_data:

secrets:
  jwt_private_key:
    file: ./secrets/jwt_private_key.pem
  jwt_public_key:
    file: ./secrets/jwt_public_key.pem
```

---

## 6. Nginx Configuration

```nginx
# nginx/nginx.conf
server {
    listen 80;
    server_name _;

    # Redirect to HTTPS (if SSL configured)
    # return 301 https://$host$request_uri;

    # API → Backend
    location /api/ {
        proxy_pass http://backend:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";  # للـ SignalR WebSocket
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_read_timeout 300s;
    }

    # SignalR Hubs
    location /hubs/ {
        proxy_pass http://backend:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # Hangfire Dashboard (Admin only — internal access)
    location /hangfire {
        proxy_pass http://backend:5000;
        proxy_set_header Host $host;
        allow 192.168.0.0/16;  # شبكة LAN فقط
        deny all;
    }

    # Static Files (Patient Uploads)
    location /uploads/ {
        alias /app/uploads/;
        expires 7d;
        add_header Cache-Control "public, immutable";
        autoindex off;
    }

    # MinIO Radiology Images (Proxy)
    location /radiology/ {
        proxy_pass http://minio:9000/radiology/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        # Security: مقيَّد بالمصادقة عبر Backend API (لا وصول مباشر)
    }

    # MinIO Admin Console (LAN only)
    location /minio-console/ {
        proxy_pass http://minio:9001/;
        allow 192.168.0.0/16;
        deny all;
    }

    # Frontend (Next.js)
    location / {
        proxy_pass http://frontend:3000;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
    }

    # Health Check
    location /health {
        proxy_pass http://backend:5000/health;
    }
}
```

---

## 7. إجراء التنصيب (Installation Guide)

### الخطوات على خادم Linux جديد

```bash
# 1. تثبيت Docker + Docker Compose
curl -fsSL https://get.docker.com | sh
sudo apt-get install -y docker-compose-plugin

# 2. استنساخ المشروع
git clone https://github.com/clinic/dental-erp.git /opt/dental-erp
cd /opt/dental-erp

# 3. إنشاء ملف البيئة
cp .env.example .env
# تعديل .env: DB_PASSWORD, REDIS_PASSWORD, MINIO_USER, MINIO_PASSWORD

# 4. إنشاء JWT Keys
mkdir -p secrets
openssl genrsa -out secrets/jwt_private_key.pem 2048
openssl rsa -in secrets/jwt_private_key.pem -pubout -out secrets/jwt_public_key.pem

# 5. تشغيل المشروع
docker compose up -d

# 6. تشغيل Migrations
docker compose exec backend dotnet ef database update

# 7. إنشاء مستخدم Admin أول
docker compose exec backend dotnet run --seed-admin
```

### التحقق من التشغيل

```bash
docker compose ps           # كل الـ containers تعمل
curl http://localhost/health # يُعيد {"status":"healthy"}
```

---

## 8. النسخ الاحتياطي (Backup Strategy)

### النسخ اليومي التلقائي (Cron Job)

```bash
# /etc/cron.d/dental-erp-backup
0 2 * * * root /opt/dental-erp/scripts/backup.sh >> /var/log/dental-backup.log 2>&1
```

```bash
# scripts/backup.sh
#!/bin/bash
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/opt/dental-erp/backups"
KEEP_DAYS=30

mkdir -p "$BACKUP_DIR"

# 1. Database Backup
docker compose exec -T postgres pg_dump -U dental_user dental_erp | \
    gzip > "$BACKUP_DIR/db_$DATE.sql.gz"

# 2. Uploads Backup (Patient Files)
tar -czf "$BACKUP_DIR/uploads_$DATE.tar.gz" /opt/dental-erp/uploads/

# 3. MinIO Backup (Radiology Images)
docker compose exec -T minio mc mirror /data "$BACKUP_DIR/minio_$DATE/"
tar -czf "$BACKUP_DIR/minio_$DATE.tar.gz" "$BACKUP_DIR/minio_$DATE/" \
    && rm -rf "$BACKUP_DIR/minio_$DATE/"

# 4. حذف النسخ القديمة (أكثر من 30 يوم)
find "$BACKUP_DIR" -name "*.gz" -mtime +$KEEP_DAYS -delete

echo "Backup completed: $DATE"
```

### الاستعادة (Restore)

```bash
# استعادة DB
gunzip -c backups/db_20260617_020000.sql.gz | \
    docker compose exec -T postgres psql -U dental_user dental_erp

# استعادة الملفات
tar -xzf backups/uploads_20260617_020000.tar.gz -C /
```

---

## 9. التحديث (Update Procedure)

```bash
cd /opt/dental-erp

# 1. سحب الكود الجديد
git pull origin main

# 2. بناء الـ Images الجديدة
docker compose build

# 3. تطبيق Migrations (قبل إعادة تشغيل الـ Application)
docker compose run --rm backend dotnet ef database update

# 4. إعادة تشغيل الـ Containers بدون Downtime (Rolling Update)
docker compose up -d --no-deps backend frontend
```

**ملاحظة:** يُنشأ Changelog في `CHANGELOG.md` لكل إصدار يوثّق الـ Migrations المطلوبة.

---

## 10. المراقبة (Monitoring)

### Health Checks

| Endpoint | الوصف |
|---------|-------|
| `GET /health` | حالة كل المكوّنات (DB + Redis + Storage) |
| `/hangfire` | لوحة Hangfire Jobs (من LAN فقط) |

### Health Check Response

```json
{
  "status": "healthy",
  "components": {
    "database": { "status": "healthy", "responseTime": "5ms" },
    "redis": { "status": "healthy", "responseTime": "1ms" },
    "storage": { "status": "healthy", "freeSpace": "245GB" }
  },
  "version": "1.2.0",
  "uptime": "5d 14h 32m"
}
```

### Logs

```bash
# Application Logs (Serilog → File + PostgreSQL)
docker compose logs backend --follow --tail=100

# PostgreSQL Logs
docker compose logs postgres --follow

# Nginx Access Logs
docker compose logs nginx --follow
```

---

## 11. الأمان (Security Hardening)

```bash
# Firewall: يسمح فقط بـ LAN
ufw default deny incoming
ufw allow from 192.168.0.0/16 to any port 80
ufw allow from 192.168.0.0/16 to any port 443
ufw allow 22/tcp  # SSH من عنوان إدارة محدد فقط
ufw enable

# PostgreSQL: يقبل اتصالات Docker Network فقط
# (محدود في pg_hba.conf تلقائياً عبر Docker Network)

# Redis: محمي بكلمة مرور + يقبل Docker Network فقط

# Uploads: الملفات لا تُشغَّل (Nginx: default_type application/octet-stream)
```

---

## ⚠️ نقاط تحتاج توضيح

1. **SSL Certificate:** هل تستخدم العيادة HTTPS داخل الشبكة المحلية؟ يتطلب إما شهادة Self-Signed (تحتاج استثناء في المتصفح) أو شهادة داخلية من CA خاص.

2. **Backup خارج الموقع:** هل يُرسَل النسخ الاحتياطي لـ USB/External Drive تلقائياً؟ أم يكفي البقاء على الخادم؟

3. **Windows Server vs Linux:** هل الخادم Linux (Ubuntu) أم Windows Server؟ يؤثر على أوامر النشر والـ Cron Jobs.

4. **Multiple Branches:** إذا كانت العيادة لها فروع — هل كل فرع خادم مستقل؟ أم يمكن ربطهم على خادم مركزي؟ (خارج نطاق V1)

5. **MinIO Bucket Structure:** هل نستخدم bucket واحد `radiology` أم نُقسّم: `radiology-images`, `patient-documents` إلخ؟ (يؤثر على Backup Strategy)
