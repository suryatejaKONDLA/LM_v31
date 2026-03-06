# CITL — Multi-Tenant SaaS Platform

> **Cradle Information Technologies Private Limited**
> © 2022–2026 · All rights reserved.

A production-grade, multi-tenant SaaS platform built on .NET 11 and React 19. Clean Architecture throughout, Dapper for data access, JWT authentication, Redis caching, and full observability via the Grafana LGTM stack.

---

## Tech Stack

### Backend

| Layer | Technology | Version |
|---|---|---|
| Runtime | .NET | 11.0 |
| Language | C# | 13 (preview) |
| Web Framework | ASP.NET Core | 11.0 |
| Data Access | Dapper | 2.1.66 |
| Database | SQL Server | 2019+ |
| Authentication | JWT Bearer | 10.0.3 |
| Caching | Redis (StackExchange) | 10.0.3 |
| Background Jobs | Quartz.NET | 3.15.1 |
| Email | MailKit | 4.15.0 |
| Image Processing | SkiaSharp | 3.119.0 |
| File Storage | Cloudflare R2 / Local (AWSSDK.S3) | 4.0.18.7 |
| Logging | Serilog | 10.0.0 |
| API Docs | Scalar + Swagger UI | 2.12.50 / 10.1.4 |
| OpenTelemetry | OTLP Exporter + Instrumentation | 1.15.0 |

### Frontend

| Technology | Version |
|---|---|
| React | 19.2.4 |
| TypeScript (`@typescript/native-preview` tsgo) | 7.0.0-dev.20260302.1 |
| Vite | 8.0.0-beta.16 (Rolldown/Oxc) |
| Ant Design | 6.3.1 |
| React Router | 7.13.1 |
| React Hook Form | 7.71.2 |
| Zustand | 5.0.11 |
| SignalR Client | 10.0.0 |
| Axios | 1.13.6 |
| Day.js | 1.11.19 |
| ESLint | 10.0.2 |
| Vitest | 4.0.18 |

### Infrastructure (Docker)

| Service | Image | Purpose |
|---|---|---|
| Grafana LGTM | `grafana/otel-lgtm:latest` | Loki + Tempo + Prometheus + OTel Collector |
| Redis | `redis:7-alpine` | Distributed cache + session |

---

## Solution Structure

```
CITL.sln
├── src/
│   ├── CITL.WebApi/            ← Controllers, middleware, DI root, Program.cs
│   ├── CITL.Application/       ← Business logic, services, use cases, DTOs
│   ├── CITL.Domain/            ← Entities, enums, domain rules (zero dependencies)
│   ├── CITL.Infrastructure/    ← Dapper repos, Redis, email, file storage, health checks
│   ├── CITL.SharedKernel/      ← Result<T>, Guards, exceptions, constants
│   └── CITL.Web/               ← React 19 SPA (Vite 8)
└── tests/
    ├── CITL.Application.Tests/
    ├── CITL.Infrastructure.Tests/
    └── CITL.WebApi.Tests/
```

### Architecture Diagrams

#### High-Level Architecture

```mermaid
graph TB
    subgraph CLIENTS["Clients"]
        WEB["React 19 SPA\nVite 8 + TypeScript"]
        MOB["Flutter Mobile"]
    end

    subgraph WEBAPI["CITL.WebApi"]
        CTRL["Controllers"]
        MW["Middleware Pipeline"]
        HUB["SignalR Hubs"]
        HC["Health Checks"]
    end

    subgraph APP["CITL.Application"]
        SVC["Services"]
        IFACE["Interfaces"]
        VAL["Validators"]
        DTO["DTOs"]
    end

    subgraph DOMAIN["CITL.Domain"]
        ENT["Entities"]
        ENUM["Enums"]
        RULES["Domain Rules"]
    end

    subgraph INFRA["CITL.Infrastructure"]
        REPO["Dapper Repositories"]
        CACHE["Cache Service"]
        AUTH["Token Service"]
        MAIL["Email Service"]
        FILES["File Storage"]
        SCHED["Quartz Scheduler"]
        OTEL["OpenTelemetry"]
    end

    subgraph SK["CITL.SharedKernel"]
        RES["Result T"]
        GRD["Guards"]
        EXC["Exceptions"]
    end

    subgraph DOCKER["Docker Services"]
        SQL["SQL Server\nper-tenant DBs"]
        REDIS["Redis 7"]
        GRAF["Grafana LGTM"]
        R2["Cloudflare R2"]
        SMTP["SMTP"]
    end

    WEB -->|"HTTPS + JWT"| CTRL
    MOB -->|"HTTPS + JWT"| CTRL
    WEB -->|"WebSocket"| HUB

    CTRL --> SVC
    HUB --> SVC
    MW --> SVC

    SVC --> ENT
    SVC --> IFACE
    SVC --> RES
    SVC --> GRD

    IFACE -.->|"implements"| REPO
    IFACE -.->|"implements"| CACHE
    IFACE -.->|"implements"| AUTH
    IFACE -.->|"implements"| MAIL
    IFACE -.->|"implements"| FILES

    REPO --> SQL
    CACHE --> REDIS
    OTEL --> GRAF
    FILES --> R2
    MAIL --> SMTP
    HC --> SQL
    HC --> REDIS

    classDef client fill:#1677ff,stroke:#0958d9,color:#fff
    classDef api fill:#13c2c2,stroke:#08979c,color:#fff
    classDef app fill:#52c41a,stroke:#389e0d,color:#fff
    classDef dom fill:#fa8c16,stroke:#d46b08,color:#fff
    classDef infra fill:#722ed1,stroke:#531dab,color:#fff
    classDef sk fill:#eb2f96,stroke:#c41d7f,color:#fff
    classDef ext fill:#595959,stroke:#434343,color:#fff

    class WEB,MOB client
    class CTRL,MW,HUB,HC api
    class SVC,IFACE,VAL,DTO app
    class ENT,ENUM,RULES dom
    class REPO,CACHE,AUTH,MAIL,FILES,SCHED,OTEL infra
    class RES,GRD,EXC sk
    class SQL,REDIS,GRAF,R2,SMTP ext
```

Inner layers never reference outer layers.

#### HTTP Request Pipeline

```mermaid
graph LR
    REQ["HTTP Request"] --> CID["1. CorrelationId\nMiddleware"]
    CID --> CORS["2. CORS"]
    CORS --> LOG["3. Request\nLogging"]
    LOG --> ERR["4. Global\nException"]
    ERR --> DOC["5. Swagger\nScalar"]
    DOC --> TEN["6. Tenant\nResolution"]
    TEN --> AUTH["7. Authentication\nAuthorization"]
    AUTH --> TG["8. Tenant\nGuard"]
    TG --> EP["Endpoints"]
    EP --> CTRL["Controllers"]
    EP --> HUB["SignalR Hubs"]
    EP --> HLTH["Health Checks"]

    classDef mw fill:#13c2c2,stroke:#08979c,color:#fff
    classDef ep fill:#52c41a,stroke:#389e0d,color:#fff

    class CID,CORS,LOG,ERR,DOC,TEN,AUTH,TG mw
    class CTRL,HUB,HLTH ep
```

#### Multi-Tenancy Flow

```mermaid
graph TD
    REQ["Client Request\nX-Tenant-Id: tn_abc123"] --> TRM["TenantResolutionMiddleware"]
    TRM -->|"Lookup tenant"| TR["TenantRegistry\nFrozenDictionary"]
    TR -->|"Returns DB name"| TRM
    TRM -->|"Sets scoped context"| TC["TenantContext\nTenantId + DatabaseName"]
    TC --> TGM["TenantGuardMiddleware\nJWT tenant_id == Header tenant"]
    TGM --> SVC["Application Services\nInject ITenantContext"]
    SVC --> SCF["SqlConnectionFactory\nReplace dbName placeholder"]
    SCF --> DB["Tenant Database\nCITL_Prod"]

    classDef req fill:#1677ff,stroke:#0958d9,color:#fff
    classDef mw fill:#13c2c2,stroke:#08979c,color:#fff
    classDef svc fill:#52c41a,stroke:#389e0d,color:#fff
    classDef db fill:#595959,stroke:#434343,color:#fff

    class REQ req
    class TRM,TR,TC,TGM mw
    class SVC,SCF svc
    class DB db
```

#### Authentication Flow

```mermaid
graph TD
    LOGIN["POST /Auth/Login\nusername + password"] --> TS["TokenService"]
    TS -->|"Generate"| AT["Access Token\nJWT 30 min\nClaims: loginId, user, name, tenant_id"]
    TS -->|"Generate"| RT["Refresh Token\n64-byte Base64\nSHA256 hashed"]
    RT -->|"Store hash"| REDIS["Redis\nTTL = refresh expiry"]
    RT -->|"Store hash"| DB["SQL Server\ncitl.Refresh_Token"]

    REFRESH["POST /Auth/Refresh\nrefreshToken"] --> VAL["Validate Token"]
    VAL -->|"Check hash"| REDIS
    VAL -->|"Fallback"| DB
    VAL -->|"Valid"| ROT["Rotate Tokens\nNew access + refresh\nOld token expires"]

    LOGOUT["POST /Auth/Logout\nBearer token"] --> BL["Blacklist Token"]
    BL -->|"Store hash + TTL"| REDIS
    BL -->|"Audit trail"| DB

    classDef endpoint fill:#1677ff,stroke:#0958d9,color:#fff
    classDef service fill:#52c41a,stroke:#389e0d,color:#fff
    classDef token fill:#fa8c16,stroke:#d46b08,color:#fff
    classDef store fill:#595959,stroke:#434343,color:#fff

    class LOGIN,REFRESH,LOGOUT endpoint
    class TS,VAL,BL,ROT service
    class AT,RT token
    class REDIS,DB store
```

#### Two-Tier Caching

```mermaid
graph TD
    REQ["Cache Request\nGetOrSetAsync key"] --> L1{"L1 MemoryCache\nin-process"}
    L1 -->|"HIT"| RET1["Return Value"]
    L1 -->|"MISS"| L2{"L2 Redis\ndistributed"}
    L2 -->|"HIT"| PROMOTE["Promote to L1\nDeserialize JSON"]
    PROMOTE --> RET2["Return Value"]
    L2 -->|"MISS"| LOCK["Acquire Semaphore\nprevent stampede"]
    LOCK --> FACTORY["Execute Factory\ncompute value"]
    FACTORY --> STORE["Store in L1 + L2\nwith TTL"]
    STORE --> RET3["Return Value"]

    classDef req fill:#1677ff,stroke:#0958d9,color:#fff
    classDef cache fill:#13c2c2,stroke:#08979c,color:#fff
    classDef hit fill:#52c41a,stroke:#389e0d,color:#fff
    classDef miss fill:#fa8c16,stroke:#d46b08,color:#fff

    class REQ req
    class L1,L2 cache
    class RET1,RET2,PROMOTE hit
    class LOCK,FACTORY,STORE,RET3 miss
```

#### Frontend Architecture

```mermaid
graph TD
    subgraph PRESENTATION["Presentation Layer"]
        PAGES["Pages\nLogin, Home, Admin"]
        LAYOUTS["Layouts\nMainLayout, LoginLayout"]
        CONTROLS["Controls\nReusable Components"]
        ROUTING["Routing\nProtectedRoute, PublicOnlyRoute"]
    end

    subgraph APPLICATION["Application Layer"]
        STORES["Zustand Stores\nAuth, Theme, Menu\nConnectivity, Location\nShortcuts, Company"]
        HOOKS["Custom Hooks\nuseLocationPermission"]
    end

    subgraph INFRASTRUCTURE["Infrastructure Layer"]
        SERVICES["API Services\nAxios HTTP Client"]
        SIGNALR["SignalR Client\nPingHub, NotificationHub"]
        PERSIST["Persistence\nlocalStorage"]
        AUTHMOD["Authentication\nJWT Token Management"]
    end

    subgraph DOMAIN["Domain Layer"]
        ENTITIES["Entities\nUser, Company, Role"]
    end

    PAGES --> STORES
    PAGES --> HOOKS
    PAGES --> CONTROLS
    LAYOUTS --> ROUTING
    STORES --> SERVICES
    STORES --> PERSIST
    SERVICES --> AUTHMOD
    SERVICES -->|"HTTPS"| API["ASP.NET Core\nBackend API"]
    SIGNALR -->|"WebSocket"| API

    classDef pres fill:#13c2c2,stroke:#08979c,color:#fff
    classDef app fill:#52c41a,stroke:#389e0d,color:#fff
    classDef infra fill:#722ed1,stroke:#531dab,color:#fff
    classDef dom fill:#fa8c16,stroke:#d46b08,color:#fff
    classDef ext fill:#595959,stroke:#434343,color:#fff

    class PAGES,LAYOUTS,CONTROLS,ROUTING pres
    class STORES,HOOKS app
    class SERVICES,SIGNALR,PERSIST,AUTHMOD infra
    class ENTITIES dom
    class API ext
```

---

## Prerequisites

| Tool | Minimum Version |
|---|---|
| .NET SDK | 11.0 preview |
| Node.js | 22 LTS |
| Docker Desktop | Latest |
| SQL Server | 2019+ |

---

## Getting Started

### 1. Clone

```bash
git clone https://github.com/suryatejaKONDLA/LM_v31.git
cd LM_v31/CITL
```

### 2. Start Infrastructure (Docker)

```powershell
# Windows
.\scripts\start-infra.ps1

# Linux / macOS
./scripts/start-infra.sh
```

This starts:
- **Grafana** at `http://localhost:3000` (admin / password set via `GF_ADMIN_PASSWORD` in `.env`)
- **Redis** at `localhost:6379`
- **OTLP gRPC** at `localhost:4317`
- **OTLP HTTP** at `localhost:4318`

### 3. Configure Secrets

Sensitive values are **never stored in `appsettings*.json`**. They live in [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) (Development) or environment variables (Production).

#### Initialize User Secrets (one-time)

```bash
cd src/CITL.WebApi
dotnet user-secrets init
```

> The `UserSecretsId` is already set in `CITL.WebApi.csproj` — this command is a no-op if already initialised.

#### Set All Required Secrets

```bash
cd src/CITL.WebApi

# SQL Server connection string (replace values for your environment)
dotnet user-secrets set "MultiTenancy:ConnectionStringTemplate" "Server=YOUR_SERVER;Database={dbName};User Id=sa;Password=YOUR_PASSWORD;Encrypt=true;TrustServerCertificate=true;MultipleActiveResultSets=true;Connection Timeout=30;Application Name=CITL"

# JWT signing key — generate a random 64-byte Base64 string:
#   [Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
dotnet user-secrets set "Jwt:SecretKey" "YOUR_BASE64_JWT_SECRET"

# Cloudflare R2 Storage
dotnet user-secrets set "FileStorage:R2Endpoint" "https://YOUR_ACCOUNT_ID.r2.cloudflarestorage.com"
dotnet user-secrets set "FileStorage:R2AccessKey" "YOUR_R2_ACCESS_KEY"
dotnet user-secrets set "FileStorage:R2SecretKey" "YOUR_R2_SECRET_KEY"
dotnet user-secrets set "FileStorage:R2BucketName" "YOUR_BUCKET"
dotnet user-secrets set "FileStorage:R2PublicDomain" "https://YOUR_CUSTOM_DOMAIN"
```

#### Generate a JWT Secret Key

```powershell
# PowerShell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
```

```bash
# Bash
openssl rand -base64 64
```

#### List / Clear Secrets

```bash
# View all set secrets
dotnet user-secrets list

# Remove a single secret
dotnet user-secrets remove "Jwt:SecretKey"

# Remove all secrets
dotnet user-secrets clear
```

#### Non-Secret Configuration

Non-sensitive Development overrides (log level, Redis URL, OTLP endpoint, CORS origins, tenant mappings) remain in `appsettings.Development.json` and are safe to commit.

### 4. Run the Backend

```powershell
# Windows — starts infra + WebApi together
.\scripts\start.ps1

# Or manually
cd src/CITL.WebApi
dotnet run
```

API endpoints once running:
- **Swagger UI** → `https://localhost:7001/swagger`
- **Scalar** → `https://localhost:7001/scalar`
- **Health** → `https://localhost:7001/health`

### 5. Run the Frontend

```bash
cd src/CITL.Web
npm install
npm run dev
```

Frontend dev server: `http://localhost:5173`

---

## Build

### Backend

```bash
dotnet build
dotnet publish -c Release
```

### Frontend

```bash
cd src/CITL.Web
npm run build
```

Output lands in `src/CITL.Web/dist/` — includes `web.config` for IIS deployment with Brotli/Gzip precompressed serving and React Router fallback.

---

## Scripts

| Script | Purpose |
|---|---|
| `scripts/set-secrets.ps1` / `.sh` | **Interactive secrets setup** — set all user-secrets in one go |
| `scripts/pull-schema.ps1` / `.sh` | Extract SQL schema per tenant via `sqlpackage` |
| `scripts/start.ps1` / `.sh` | Start infra (Docker) + WebApi |
| `scripts/start-infra.ps1` / `.sh` | Start only Docker services |
| `scripts/build.ps1` / `.sh` | Full solution build |
| `scripts/publish.ps1` / `.sh` | Publish for deployment |

### Frontend npm scripts

| Command | Purpose |
|---|---|
| `npm run dev` | Start Vite dev server |
| `npm run build` | Production build + copy `web.config` |
| `npm run lint` | ESLint (zero warnings) |
| `npm run check` | Lint + TypeScript type-check |
| `npm run test` | Vitest unit tests |
| `npm run test:ui` | Vitest UI |
| `npm run test:coverage` | Coverage report |
| `npm run packages:outdated` | Show outdated packages |
| `npm run clean` | Clear dist + Vite cache |
| `npm run clean:all` | Full reset (node_modules + dist) |

---

## Architecture

### Multi-Tenancy (Database-per-Tenant)

Each tenant gets an isolated SQL Server database. The connection string uses a `{dbName}` placeholder replaced at runtime:

```
Server=...;Database={dbName};...
```

Tenant resolution flows via a **scoped `TenantContext`** injected through DI — no `AsyncLocal`, no static state.

### Authentication

JWT Bearer tokens with short-lived access tokens (30 min) and long-lived refresh tokens (30 days). Token payload carries tenant and role claims. Works identically for the React SPA and Flutter mobile client.

### Caching

Two-tier:
- **L1** — `IMemoryCache` (in-process, ~0 ms)
- **L2** — Redis (shared across servers, ~1–2 ms)

Cache keys follow the pattern `{tenant}:{entity}:{id}`.

### Health Checks

10 health checks registered and exposed at `/health`:

| Check | What it monitors |
|---|---|
| SQL Server | Tenant database connectivity |
| Redis | Cache connectivity |
| R2 Storage | Cloudflare R2 bucket reachability |
| Disk Space | Local folder quota usage |
| Process Memory | Working set vs threshold |
| Quartz Scheduler | Background job scheduler state |
| Grafana | Grafana dashboard reachability |
| OTLP Collector | OpenTelemetry collector reachability |
| Mail | SMTP connectivity |
| SignalR | Hub connectivity |

### Observability

Full OpenTelemetry pipeline → Grafana LGTM stack:
- **Traces** → Tempo
- **Metrics** → Prometheus
- **Logs** → Loki (via Serilog OTLP sink)

Grafana dashboards are pre-provisioned in `grafana/dashboards/`.

---

## Deployment

The frontend `dist/` folder is a static SPA deployable to IIS. The included `web.config` handles:
- **Brotli** precompressed asset serving
- **Gzip** fallback
- **React Router** SPA fallback (`index.html`)
- **Security headers** (CSP, X-Frame-Options, HSTS-ready, etc.)
- **Immutable cache** headers for versioned assets

---

## Key Conventions

- **Constants** — PascalCase, never UPPER_SNAKE_CASE
- **`var`** everywhere the compiler allows
- **Braces** always (Allman style)
- **Async** — `Async` suffix, always accept `CancellationToken`, `.ConfigureAwait(false)` in library code
- **Null checks** — `is null` / `is not null`
- **Logging** — `[LoggerMessage]` source-generator, never `_logger.LogXxx()`
- **Errors** — `Result<T>` for business logic, exceptions for infrastructure

Full details in [CODING_STANDARDS.md](CODING_STANDARDS.md).

---

## Project Info

| | |
|---|---|
| **Company** | Cradle Information Technologies Private Limited |
