# CITL Style Guide — Gemini Code Assist

> This file is read by Gemini Code Assist when reviewing pull requests.
> It defines the coding standards, patterns, and conventions for the CITL project.

## Project Context

- **Stack**: .NET 11, C# 13, ASP.NET Core, Dapper, SQL Server, Redis, React 19, TypeScript, Vite 8
- **Architecture**: Clean Architecture — `WebApi → Application → Domain ← Infrastructure`
- **Multi-Tenant**: Database-per-tenant with scoped `TenantContext` via DI
- **Style Reference**: Follows [dotnet/aspnetcore](https://github.com/dotnet/aspnetcore/wiki/Engineering-guidelines) conventions

## C# Code Review Checklist

### Naming

- Constants and `static readonly` fields must use **PascalCase** — reject any UPPER_SNAKE_CASE.
- Private/internal fields must use `_camelCase` with underscore prefix.
- Async methods must have `Async` suffix.
- Interfaces must use `I` prefix: `IAppMasterService`.
- No abbreviations in public APIs: `AddReference` not `AddRef`.

### Language

- `var` must be used everywhere the compiler allows — reject explicit type declarations when type is apparent.
- Braces are required even for single-line `if`, `else`, `for`, `foreach`, `while`, `using` blocks.
- File-scoped namespaces only — reject block-scoped `namespace Foo { }`.
- Null checks must use `is null` / `is not null` — reject `== null` / `!= null`.
- Use `nameof()` instead of string literals for member names.
- Avoid `this.` qualifier unless absolutely necessary.
- Always specify visibility modifier — reject `string _foo` without `private`.
- Use C# type keywords: `string` not `String`, `int` not `Int32`.
- No throw expressions — use explicit `if` + `throw`.

### Async

- Every I/O method must be `async` with `Async` suffix.
- Every async method must accept `CancellationToken` and forward it.
- `.ConfigureAwait(false)` is required in Application, Infrastructure, SharedKernel projects.
- `.ConfigureAwait(false)` must NOT be used in WebApi project (controllers, middleware).

### Performance

- All classes must be `sealed` unless explicitly designed for inheritance.
- Methods that don't use instance state must be `static`.
- Use `char` overloads over `string` for single-character operations: `Contains('x')` not `Contains("x")`.
- Use `Span<T>` / `AsSpan()` over `Substring()`.
- `JsonSerializerOptions` must be cached — never created per-call.
- Use `StringComparison.Ordinal` / `OrdinalIgnoreCase` — no culture-sensitive defaults.
- Avoid LINQ on indexable collections — use indexer/`Length` directly.

### Error Handling

- Business logic must return `Result<T>` — reject throwing exceptions for expected failures.
- Infrastructure failures (DB down, config missing) may use exceptions.
- Guard clauses at method entry: `Guard.NotNull()`, `Guard.Positive()`.
- No defensive error handling for scenarios that cannot happen.

### Logging

- Must use `[LoggerMessage]` source-generator attributes.
- Reject any `_logger.LogInformation()`, `_logger.LogError()`, etc. direct calls.
- Log template tokens must be PascalCasedCompoundWords.
- No string interpolation in log templates.

### Architecture

- Inner layers (Domain, Application) must NEVER reference outer layers (WebApi, Infrastructure).
- Domain project must have zero framework dependencies.
- Interfaces are defined in Application, implementations in Infrastructure.
- Dapper only — reject any Entity Framework Core usage.
- Database-per-tenant — reject any `WHERE TenantId = @TenantId` patterns (wrong strategy).
- Primary constructors for dependency injection.

### XML Documentation

- Required only on public APIs.
- Must be concise — reject "Gets or sets" boilerplate.
- Use `/// <inheritdoc />` for interface implementations.

### Cross-Platform

- Use `Path.Combine()` — reject hardcoded `\` backslashes in paths.
- No Windows-only APIs (Registry, COM interop).
- Assume case-sensitive file system.

## Frontend Code Review Checklist

### React / TypeScript

- Must use `@vitejs/plugin-react-swc` — reject Babel-based plugin.
- Ant Design 6 for UI components — reject other UI libraries.
- Zustand for state management — reject Redux, MobX, Context API for global state.
- React Router 7 for routing.
- Axios for HTTP calls with interceptors.
- Clean Architecture layers: `Presentation → Application → Infrastructure → Domain`.

### File Organization

- One component per file.
- Stores in `Application/Stores/`.
- API services in `Infrastructure/Services/`.
- Pages in `Presentation/Pages/`.
- Reusable UI in `Presentation/Controls/`.

## Git

- Commits must use imperative mood: "Add feature" not "Added feature".
- Branch names in kebab-case: `feature/user-service`.

## Files That Must Not Be Modified

PR changes to these files should be flagged for review:

- `global.json`
- `NuGet.config`
- `.editorconfig` (formatting/analyzer rules)
- Solution files (`.sln`, `.slnx`)
