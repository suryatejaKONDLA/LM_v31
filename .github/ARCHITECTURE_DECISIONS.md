# CITL Architecture Decisions Record (ADR)

> This document records all major architectural decisions, the reasoning behind them, pain points from the old application, and their solutions in the new build. It serves as a living reference for all developers and AI assistants.

---

## Project Overview

| Item | Detail |
|------|--------|
| **Project** | CITL — Multi-Tenant SaaS Platform |
| **Architecture** | Clean Architecture |
| **Language** | C# 13 / .NET 11+ |
| **Data Access** | Dapper (micro-ORM) |
| **Database** | SQL Server |
| **Authentication** | JWT (JSON Web Tokens) |
| **Caching** | Redis (L2 shared) + MemoryCache (L1 in-process) |
| **Migrations** | DbUp (SQL script-based, DB-first) |
| **Frontend** | React (SPA) |
| **Mobile** | Flutter |
| **Multi-Tenant Strategy** | Database-per-tenant (connection string switching) |

---

## Solution Structure

```
CITL.sln
├── src/
│   ├── CITL.WebApi/            ← Presentation layer
│   ├── CITL.Application/       ← Business logic & use cases
│   ├── CITL.Domain/            ← Entities, enums, domain rules
│   ├── CITL.Infrastructure/    ← Data access, external services
│   └── CITL.SharedKernel/      ← Cross-cutting shared code
└── tests/
    ├── CITL.Application.Tests/
    ├── CITL.Infrastructure.Tests/
    └── CITL.WebApi.Tests/
```

### Project Responsibilities

| Project | Responsibility | References |
|---------|---------------|------------|
| **CITL.WebApi** | Controllers, middleware, filters, DI composition root, `Program.cs` | Application, Infrastructure |
| **CITL.Application** | Service interfaces & implementations, DTOs, validators, use case logic | Domain, SharedKernel |
| **CITL.Domain** | Entities, value objects, enums, domain events, domain rules (zero dependencies on frameworks) | SharedKernel |
| **CITL.Infrastructure** | Dapper repositories, Redis/MemoryCache, email, file storage, DbUp migrations | Application, Domain, SharedKernel |
| **CITL.SharedKernel** | Base classes, custom exceptions, extension methods, constants, common interfaces | *(no project references)* |

### Dependency Direction

```
WebApi → Application → Domain → SharedKernel
  ↓                                   ↑
Infrastructure ─────────────────────────┘
```

**Rule**: Inner layers NEVER reference outer layers. Domain knows nothing about HTTP, databases, or caching.

---

## ADR-001: Clean Architecture

**Decision**: Use Clean Architecture (Uncle Bob / Jason Taylor pattern)

**Why chosen over N-Layer (old app)**:
- Old app had direct coupling: Controller → Service → Repository (all knew about Dapper, SQL, response models)
- Changing data access (e.g., Dapper → EF Core for specific queries) required touching every layer
- Business logic was entangled with infrastructure concerns
- Unit testing required database setup because services directly used Dapper

**Benefits**:
- Domain/Application layers are 100% testable without database
- Can swap Infrastructure implementations without changing business logic
- Interfaces defined in Application, implementations in Infrastructure
- Clear separation: "what the app does" (Application) vs "how it does it" (Infrastructure)

**References**:
- [Microsoft Clean Architecture Guide](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)
- [Jason Taylor Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture)
- [Ardalis Clean Architecture](https://github.com/ardalis/cleanarchitecture)

---

## ADR-002: Dapper for Data Access

**Decision**: Use Dapper as the primary data access library

**Why Dapper over EF Core**:
- Micro-level performance control — zero ORM overhead
- Full SQL control — write exactly the query you want
- Lightweight — no change tracking, no proxy objects, no lazy loading surprises
- Team already proficient with SQL and Dapper from old app
- Better for stored procedures and complex reporting queries

**Trade-offs accepted**:
- No automatic migrations (using DbUp instead)
- No global query filters (tenant filtering must be manual in every query)
- No change tracking (must handle insert/update/delete explicitly)
- More boilerplate than EF Core for basic CRUD

**Mitigation for tenant filtering**:
- Create a base `DatabaseService` or `TenantAwareRepository` that automatically injects tenant connection
- All repository methods receive connection from a centralized `IDbConnectionFactory`
- Code review checklist item: "Does every query include tenant filtering?"

---

## ADR-003: Database-per-Tenant Multi-Tenancy

**Decision**: Each tenant gets a separate database. Connection string uses `{dbName}` placeholder replaced at runtime.

**How it works**:
```
appsettings.json:
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database={dbName};..."
  }

Runtime:
  connectionString.Replace("{dbName}", tenantContext.TenantName)
```

**Why database-per-tenant**:
- Strongest data isolation — tenants physically cannot access each other's data
- Easy per-tenant backup/restore
- Can scale individual tenant databases independently
- Compliance-friendly (data residency per tenant)
- Simpler queries — no `WHERE TenantId = @TenantId` on every query

**Old app pain point — tenant context in background tasks**:
The old app used `AsyncLocal<string>` in `TenantProvider` with manual `SetTenantOverride()` / `ClearTenantOverride()`. This caused:
1. Forgotten overrides → tenant data leakage
2. HttpContext lost in `Task.Run` → `TenantNotFoundException`
3. Every background task needed boilerplate: capture tenant → set override → try/catch → clear
4. No DI scope in `Task.Run` → scoped services (DbConnection) could be disposed
5. Fragile `AsyncLocal` — values don't flow correctly across all async contexts

**New approach — Scoped TenantContext + Background Worker with new DI scope**:

For HTTP requests:
- Middleware reads tenant from request header
- Sets `TenantContext.TenantName` (registered as Scoped in DI)
- All downstream services get it via constructor injection — no static/AsyncLocal

For background tasks:
- Service queues a work item: `{ TenantName, TaskType, Payload }`
- Dedicated `BackgroundService` picks up the item
- Creates a NEW `IServiceScope` via `IServiceScopeFactory`
- Sets `TenantContext.TenantName` in the new scope
- Executes the task with full DI support (fresh connections, fresh scoped services)
- No `AsyncLocal`, no `SetTenantOverride`, no `ClearTenantOverride`

---

## ADR-004: JWT Authentication

**Decision**: Use JWT (JSON Web Tokens) for authentication

**Why JWT**:
- **React SPA**: JWT in `Authorization: Bearer` header — no CORS/cookie issues
- **Flutter Mobile**: Just attach token to every request — cookies don't work well on mobile
- **Stateless**: Any server instance can validate the token — no session store needed
- **Horizontal scaling**: No sticky sessions, no shared session store
- **One token format**: Works identically for web, mobile, and future API consumers

**Token strategy** (to be detailed during implementation):
- Access token: Short-lived (15-30 min)
- Refresh token: Long-lived, stored securely, rotated on use
- Tenant claim: Include `tenant_name` claim in JWT payload
- Role claims: Include user roles for authorization

---

## ADR-005: Two-Tier Caching (Redis + MemoryCache)

**Decision**: L1 MemoryCache (in-process) + L2 Redis (shared/distributed)

**How it works**:
```
Request → Check MemoryCache (L1)
              ↓ miss
          Check Redis (L2)
              ↓ miss
          Query Database → Store in Redis (L2) → Store in MemoryCache (L1)
```

| Layer | Use For | Speed | Shared Across Servers |
|-------|---------|-------|-----------------------|
| **MemoryCache (L1)** | Tenant config, app settings, static lookups, menus | ~0ms | No (per-instance) |
| **Redis (L2)** | User sessions, permissions, frequently queried lists, shared state | ~1-2ms | Yes |

**Cache key pattern** (tenant-aware):
```
{tenant}:{entity}:{id}
Example: "CompanyA:user:42", "CompanyB:products:list"
```

**Invalidation strategy** (to be finalized):
- Cache-aside pattern with explicit invalidation on write
- Redis pub/sub for cross-server L1 invalidation (when data changes, notify all instances to clear L1)

---

## ADR-006: DbUp for Database Migrations

**Decision**: Use DbUp with raw SQL scripts for database schema management

**Why DbUp**:
- **DB-first approach**: Write SQL scripts directly — no C# migration classes
- **Simple**: Just SQL files in a folder, numbered sequentially
- **Multi-tenant**: Loop through all tenant databases and apply pending scripts
- **Version tracked**: `SchemaVersions` table records which scripts have been applied
- **No ORM dependency**: Works perfectly with Dapper (no EF Core required)

**Script organization**:
```
Migrations/
├── 0001_CreateUsersTable.sql
├── 0002_CreateProductsTable.sql
├── 0003_AddIndexOnUserEmail.sql
└── ...
```

**Execution flow**:
1. On app startup (or via CLI command)
2. Get list of all tenant databases
3. For each tenant DB: connect → run pending scripts → log results
4. `SchemaVersions` table per tenant DB tracks applied scripts

---

## Pain Points from Old App & Solutions

| # | Pain Point (Old App) | Root Cause | Solution (New App) |
|---|---------------------|------------|-------------------|
| 1 | Background tasks lost tenant context | `AsyncLocal` + `HttpContext` dependency | Scoped `TenantContext` via DI + `IServiceScopeFactory` for background tasks |
| 2 | `SetTenantOverride` / `ClearTenantOverride` boilerplate everywhere | Manual tenant propagation | Tenant flows automatically via DI scope — no manual set/clear |
| 3 | Tight coupling between layers | N-Layer with direct dependencies | Clean Architecture with dependency inversion |
| 4 | Hard to unit test services | Services directly used Dapper/SQL | Interfaces in Application, implementations in Infrastructure |
| 5 | Changing data access required touching all layers | No abstraction between business logic and DB | Repository interfaces in Application, Dapper in Infrastructure |
| 6 | All services inherited from `ServiceBase` with many responsibilities | God-object base class | Composition over inheritance — inject only what you need |
| 7 | `AutoRegisterService` attribute magic | Implicit DI registration | Explicit DI registration in composition root or use `Scrutor` for convention-based scanning (decided later) |
| 8 | No migration tracking | Manual SQL scripts | DbUp with `SchemaVersions` table per tenant |
| 9 | Mixed concerns in Common project | Shared project had everything | `SharedKernel` for cross-cutting only; domain logic in `Domain` |
| 10 | Module folders (Core/Dt7/Ogo) duplicated across all layers | Module-per-folder in every project | Feature folders within Application layer; Infrastructure is flat |

---

## Decisions Still Pending

| Area | Options Being Considered | Decision By |
|------|-------------------------|-------------|
| **API Response Wrapper** | Custom `Result<T>` / `ApiResponse<T>` / raw responses | When building first endpoint |
| **Validation Library** | FluentValidation / DataAnnotations / manual | When building first endpoint |
| **Logging Framework** | Serilog / NLog / built-in `ILogger` only | During infrastructure setup |
| **DI Registration** | Manual / Scrutor convention / custom attribute | During project scaffolding |
| **Exception Handling** | Global middleware approach / per-controller | When building first endpoint |
| **API Versioning** | URL-based (`/v1/`) / header-based / query string | When building first endpoint |
| **Health Checks** | Built-in ASP.NET Core / custom | During deployment setup |
| **Rate Limiting** | Built-in ASP.NET Core / custom middleware | When needed |
| **Background Job Queue** | Channels / Hangfire / custom `BackgroundService` | When first background task is needed |
| **Testing Framework** | xUnit / NUnit / MSTest | When setting up test projects |

---

## Technology Stack Summary

### Backend
| Component | Technology | Version |
|-----------|-----------|---------|
| Runtime | .NET | 11+ |
| Language | C# | 13 |
| Web Framework | ASP.NET Core | 11+ |
| Data Access | Dapper | Latest |
| Database | SQL Server | 2019+ |
| Caching (Distributed) | Redis | Latest |
| Caching (In-Memory) | Microsoft.Extensions.Caching.Memory | Built-in |
| Authentication | JWT (Microsoft.AspNetCore.Authentication.JwtBearer) | Latest |
| Migrations | DbUp | Latest |

### Frontend & Mobile
| Component | Technology |
|-----------|-----------|
| Web SPA | React |
| Mobile | Flutter |

### DevOps & Tools
| Component | Technology |
|-----------|-----------|
| Source Control | Git / GitHub |
| Code Style | EditorConfig + Roslyn Analyzers |
| CI/CD | To be decided |

---

## References

- [Microsoft Clean Architecture](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures#clean-architecture)
- [Azure Multi-Tenant Architecture Guide](https://learn.microsoft.com/en-us/azure/architecture/guide/multitenant/overview)
- [Jason Taylor Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture)
- [Dapper GitHub](https://github.com/DapperLib/Dapper)
- [DbUp GitHub](https://github.com/DbUp/DbUp)
- [JWT Best Practices (RFC 8725)](https://datatracker.ietf.org/doc/html/rfc8725)

---

**Created**: February 28, 2026
**Last Updated**: February 28, 2026
**Status**: Active — Living Document
