---
name: 'Architecture Map'
description: 'Clean Architecture layer map, responsibilities, and data flow'
applyTo: '**/*.cs'
---

# CITL Architecture Map

> Clean Architecture with 5 source projects and 2 test projects.
> Every layer is a separate assembly — dependency direction enforced by project references.

## Visual Layer Map

```
┌─────────────────────────────────────────────────────┐
│                    CITL.WebApi                       │
│  Controllers → Middleware → Program.cs (DI root)    │
│  Extensions (ResultExtensions) → Attributes          │
├─────────────────────────────────────────────────────┤
│              CITL.Infrastructure                     │
│  Persistence (DbExecutor, SqlConnectionFactory)     │
│  MultiTenancy (TenantContext, TenantRegistry)       │
│  Core/{Feature}/ (Repository implementations)       │
├─────────────────────────────────────────────────────┤
│              CITL.Application                        │
│  Common/Interfaces (IDbExecutor, ITenantContext)    │
│  Common/Models (SpResult, SpResultExtensions)       │
│  Common/Validation (ValidationResultExtensions)     │
│  Core/{Feature}/ (Service, DTOs, Validator, Repo IF)│
├─────────────────────────────────────────────────────┤
│                CITL.Domain                           │
│  Core/{Feature}/ (Entities, Value Objects, Enums)   │
├─────────────────────────────────────────────────────┤
│              CITL.SharedKernel                       │
│  Results (Error, Result, Result<T>)                 │
│  Exceptions (AppException hierarchy)                │
│  Guards (Guard.NotNull, NotNullOrEmpty, etc.)       │
│  Constants (TenantConstants)                        │
└─────────────────────────────────────────────────────┘
```

## Dependency Direction (enforced by .csproj references)

```
WebApi → Application, Infrastructure
Infrastructure → Application, Domain, SharedKernel
Application → Domain, SharedKernel
Domain → SharedKernel
SharedKernel → (nothing)
```

**Rule**: Inner layers NEVER reference outer layers. Domain knows nothing about HTTP, Dapper, or Redis.

## Layer Responsibilities

### SharedKernel (innermost — zero references)

| Contains | Purpose |
|----------|---------|
| `Results/` | `Error`, `Result`, `Result<T>` — value-based error handling, no exceptions for business logic |
| `Exceptions/` | `AppException` hierarchy — infrastructure/unrecoverable errors only |
| `Guards/` | `Guard` class — entry-point validation with JIT-optimized throw helpers |
| `Constants/` | Cross-cutting constants (`TenantConstants`) used by multiple layers |
| Marker | `SharedKernelAssemblyMarker` |

### Domain (entities, value objects)

| Contains | Purpose |
|----------|---------|
| `Core/{Feature}/` | Domain entities mapping to database tables |
| Marker | `DomainAssemblyMarker` |

- Entities are `sealed class` with `init`-only properties — no primary constructors needed
- No domain logic yet (SPs handle business rules) — will grow as domain services are introduced
- Follows `Core/{Module}/{Feature}` folder convention matching the database schema grouping

### Application (orchestration, contracts)

| Contains | Purpose |
|----------|---------|
| `Common/Interfaces/` | `IDbExecutor`, `IDbConnectionFactory`, `ITenantContext`, `ITenantRegistry` |
| `Common/Models/` | `SpResult`, `SpResultExtensions` — shared SP output mapping |
| `Common/Validation/` | `ValidationResultExtensions` — FluentValidation → Result bridge |
| `Core/{Feature}/` | Service interface + implementation, request/response DTOs, FluentValidation validator, repository interface |
| `DependencyInjection.cs` | `AddApplication()` — registers services + FluentValidation auto-scan |
| Marker | `ApplicationAssemblyMarker` (non-static, needed for `AddValidatorsFromAssemblyContaining<T>()`) |

**Feature folder structure** (each feature is self-contained):
```
Core/Admin/AppMaster/
  ├── IAppMasterRepository.cs    ← interface (implemented in Infrastructure)
  ├── IAppMasterService.cs       ← interface
  ├── AppMasterService.cs        ← implementation (primary constructor)
  ├── AddOrUpdateAppMasterRequest.cs  ← input DTO
  ├── AddOrUpdateAppMasterRequestValidator.cs  ← FluentValidation
  └── AppMasterResponse.cs       ← output DTO
```

### Infrastructure (implementations)

| Contains | Purpose |
|----------|---------|
| `Persistence/DbExecutor.cs` | **Central DB helper** — handles connection lifecycle, SP output params, Dapper queries |
| `Persistence/SqlConnectionFactory.cs` | Creates tenant-scoped `SqlConnection` — used internally by `DbExecutor` |
| `MultiTenancy/` | `TenantContext` (scoped), `TenantRegistry` (singleton FrozenDictionary), `TenantSettings` |
| `Core/{Feature}/` | Repository implementations using `IDbExecutor` |
| `DependencyInjection.cs` | `AddInfrastructure(config)` — registers everything |

**Repository pattern**: Repos inject `IDbExecutor` and contain ONLY SQL + parameter dictionaries:
```csharp
internal sealed class XxxRepository(IDbExecutor db) : IXxxRepository
{
    // SP call → db.ExecuteSpAsync("schema.SP_Name", params, ct)
    // Query   → db.QuerySingleOrDefaultAsync<T>(sql, params, ct)
    // List    → db.QueryAsync<T>(sql, params, ct)
}
```

### WebApi (HTTP layer — composition root)

| Contains | Purpose |
|----------|---------|
| `Controllers/Core/{Feature}/` | Thin controllers: call service, return `result.ToActionResult()` |
| `Middleware/` | `GlobalExceptionMiddleware`, `TenantResolutionMiddleware`, `TenantGuardMiddleware` |
| `Extensions/ResultExtensions.cs` | `Result` → `IActionResult` mapping (auto error-code-to-HTTP-status) |
| `Attributes/` | `BypassTenantAttribute` + `TenantEndpointExtensions` |
| `Program.cs` | DI composition root + middleware pipeline |

## Request Flow (typical POST)

```
HTTP Request (with X-Tenant-Id header)
  │
  ▼
GlobalExceptionMiddleware     ← catches all exceptions → JSON error
  │
  ▼
TenantResolutionMiddleware    ← reads header → resolves DB name → sets TenantContext
  │
  ▼
TenantGuardMiddleware         ← cross-validates JWT tenant_id claim (if authenticated)
  │
  ▼
AppMasterController           ← [FromBody] → calls service → result.ToActionResult()
  │
  ▼
AppMasterService              ← validator.ValidateAsync() → repo.AddOrUpdateAsync()
  │                              → spResult.ToResult("ErrorCode")
  ▼
AppMasterRepository           ← db.ExecuteSpAsync("citlsp.App_Insert", params)
  │
  ▼
DbExecutor                    ← DynamicParameters + output params → Dapper → SpResult
  │
  ▼
SqlConnectionFactory          ← TenantContext.DatabaseName → SqlConnection
  │
  ▼
SQL Server (tenant database)
```

## Key Design Patterns

| Pattern | Where | Purpose |
|---------|-------|---------|
| **Result<T>** | Application services return values | No exceptions for business logic failures |
| **SpResult** | Repository → Service | Standard SP output contract (`@ResultVal`, `@ResultType`, `@ResultMessage`) |
| **IDbExecutor** | Repos inject this | Eliminates Dapper/ADO boilerplate (connections, params, output extraction) |
| **FluentValidation** | `AbstractValidator<T>` beside request DTO | Declarative validation, auto-registered |
| **ResultExtensions** | Controller actions | `result.ToActionResult()` — 1 line per action |
| **Primary constructors** | Services, repos, middleware, controllers | Eliminates field + constructor boilerplate |
| **FrozenDictionary** | `TenantRegistry` | O(1) tenant lookup, hot-reload via `IOptionsMonitor` |

## Where to Put New Code

| I need to... | Put it in... | Example |
|--------------|-------------|---------|
| Add a new entity | `Domain/Core/{Module}/{Entity}.cs` | `Domain/Core/Admin/Branch.cs` |
| Add a new feature (CRUD) | `Application/Core/{Module}/{Feature}/` | 6 files: repo IF, service IF+impl, DTOs, validator |
| Implement a repository | `Infrastructure/Core/{Module}/{Feature}Repository.cs` | Inject `IDbExecutor` |
| Add a controller | `WebApi/Controllers/Core/{Module}/{Feature}Controller.cs` | Primary constructor, `ToActionResult()` |
| Add cross-cutting logic | `SharedKernel/` | New exception type, guard method, constant |
| Add shared Application helper | `Application/Common/{subfolder}/` | Extension methods, shared models |
| Add Infrastructure helper | `Infrastructure/Persistence/` or `Infrastructure/{Concern}/` | Caching, email, etc. |
