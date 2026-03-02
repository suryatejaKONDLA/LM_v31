# CITL — Agent Instructions

> Multi-tenant SaaS platform · Clean Architecture · C# 13 · .NET 11+ · Dapper · SQL Server

**All coding style** is defined once in [CODING_STANDARDS.md](CODING_STANDARDS.md) and enforced by [.editorconfig](.editorconfig).
Follow [dotnet/aspnetcore](https://github.com/dotnet/aspnetcore/wiki/Engineering-guidelines) conventions.

## Architecture

- **Clean Architecture**: `WebApi → Application → Domain ← Infrastructure`
- **SharedKernel**: Cross-cutting concerns (results, exceptions, guards, constants) — no project references.
- Inner layers never reference outer layers. Domain knows nothing about HTTP, DB, or caching.

## Key Decisions

| Area | Choice |
|------|--------|
| Data Access | Dapper via `IDbExecutor` (micro-ORM, full SQL control) |
| Multi-Tenant | Database-per-tenant, `{dbName}` connection string switching |
| Tenant Context | Scoped `TenantContext` via DI — **no AsyncLocal** |
| Authentication | JWT Bearer tokens (React + Flutter clients) |
| Caching | Redis (L2 shared) + MemoryCache (L1 in-process) |
| Migrations | DbUp (SQL scripts, DB-first) |
| Cross-Platform | Windows, Linux, macOS |

## Key Rules

- **Constants**: PascalCase — no UPPER_SNAKE_CASE.
- **`var`**: Use everywhere the compiler allows.
- **Braces**: Always, even for single-line blocks.
- **Async**: Suffix `Async`, accept `CancellationToken`, `.ConfigureAwait(false)` in library code.
- **Null checks**: `is null` / `is not null`.
- **XML docs**: Public APIs only — concise, no "Gets or sets".
- **Logging**: `[LoggerMessage]` source-gen — never `_logger.LogXxx()`.
- **Errors**: `Result<T>` for business logic, exceptions for infrastructure.
- **Don't modify**: global.json, NuGet.config, solution files unless asked.

## Reference

- [Coding Standards](CODING_STANDARDS.md) — single source of truth for style
- [Architecture Decisions](.github/ARCHITECTURE_DECISIONS.md)
- [EditorConfig](.editorconfig) — formatting and analyzer rules
