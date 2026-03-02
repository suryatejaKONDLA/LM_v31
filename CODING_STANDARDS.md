# CITL Coding Standards

> **Single source of truth.** Follows [dotnet/aspnetcore](https://github.com/dotnet/aspnetcore/wiki/Engineering-guidelines) conventions.
> All projects in the solution honour these rules. The `.editorconfig` enforces them at build time.

---

## Formatting

- 4-space indentation (no tabs)
- Open braces (`{`) on a new line (Allman style)
- Always use braces, even for single-line blocks
- File-scoped namespaces
- One type per file; filename matches type name
- Sort `System` using directives first
- No multiple blank lines
- Modifier order: `public`, `private`, `protected`, `internal`, `static`, `extern`, `new`, `virtual`, `abstract`, `sealed`, `override`, `readonly`, `unsafe`, `volatile`, `async`

## Naming

| Element                   | Style               | Example                         |
|---------------------------|----------------------|---------------------------------|
| Classes, structs, enums   | PascalCase           | `AppMasterService`              |
| Methods, properties       | PascalCase           | `GetAsync`, `AppCode`           |
| Interfaces                | `I` + PascalCase     | `IAppMasterService`             |
| Constants                 | PascalCase           | `HeaderName`, `SectionName`     |
| `static readonly` fields  | PascalCase           | `None`, `NullValue`             |
| Private/internal fields   | `_camelCase`         | `_logger`, `_tenantMap`         |
| Local variables           | camelCase            | `spResult`, `validation`        |
| Parameters                | camelCase            | `request`, `cancellationToken`  |
| Async methods             | Suffix `Async`       | `GetAsync`, `AddOrUpdateAsync`  |

> **No UPPER_SNAKE_CASE.** All constants and static fields use PascalCase — same as aspnetcore.

## Language Usage

- **`var` everywhere** the compiler allows. Exceptions: `const` declarations and when the type isn't known.
- **C# type keywords** over .NET type names (`string` not `String`, `int` not `Int32`).
- **Pattern matching** (`is`, `switch` expressions) over `is` + cast.
- **Null checks**: `is null` / `is not null` — never `== null` / `!= null`.
- **`nameof()`** instead of string literals for member names.
- **No throw expressions** — use explicit `if` + `throw`.
- **Avoid `this.`** unless absolutely necessary.
- **Always specify visibility** — even the default (`private string _foo`, not `string _foo`).
- **Collection expressions** (`[]`) for empty collections.
- Use complete words in public APIs — no abbreviations (`AddReference` not `AddRef`).

## Async

- All I/O methods are `async` with `Async` suffix.
- Always accept `CancellationToken` and forward it.
- `.ConfigureAwait(false)` in **library code only** (Application, Infrastructure, SharedKernel).
- **WebApi layer** (controllers, middleware): **no** `ConfigureAwait(false)` — ASP.NET Core has no `SynchronizationContext`, so it's a no-op and just adds noise. CA2007 is suppressed in the WebApi project.

## Performance

- **`sealed`** on all classes that aren't designed for inheritance — enables JIT devirtualization (CA1852).
- **`static`** on methods that don't use instance state — avoids hidden `this` capture (CA1822).
- **Concrete types** over interfaces for local variables/returns when possible — enables devirtualization (CA1859).
- **`char` overloads** over `string` for single-character operations: `Contains('x')` not `Contains("x")` (CA1865-1867).
- **`Span<T>` / `AsSpan()`** over `Substring()` — avoids heap allocation (CA1846).
- **Cache `JsonSerializerOptions`** — never create per-call (CA1869).
- **Avoid LINQ on indexable collections** — use indexer/`Length` directly (CA1826, CA1829, CA1860).
- **`StringComparison` overloads** — use `Ordinal` / `OrdinalIgnoreCase` instead of culture-sensitive defaults (CA1862).
- **`FrozenDictionary`** / `FrozenSet` for lookup tables that don't change (`TenantRegistry`).

## XML Documentation

- **Public APIs only.** Non-public types and members don't require doc comments.
- Concise `<summary>` — one sentence, no "Gets or sets" boilerplate.
- Use `<param>`, `<returns>`, `<exception>` only when they add value beyond what is obvious.
- DTOs: doc on the class is enough; individual properties are self-documenting.
- `/// <inheritdoc />` for interface implementations.

## Error Handling

- `Result<T>` / `Result` for business logic outcomes — no exceptions for expected failures.
- Exceptions for infrastructure failures (DB down, config missing, bad tenant).
- `SpResult` → `SpResultExtensions.ToResult()` for stored procedure results.
- `FluentValidation` → `ValidationResultExtensions.ToResult()` for input validation.
- Guard clauses at method entry points: `Guard.NotNull()`, `Guard.Positive()`, etc.

## Logging

- Use `[LoggerMessage]` source-generator attributes — no `_logger.LogXxx()` calls in production code.
- `partial` class + `private static partial void LogXxx(...)` pattern.
- Template tokens are `PascalCasedCompoundWords`.
- Never use string interpolation in log templates.

## Cross-Platform

- `Path.Combine()` or `/` for paths — never hardcoded `\`.
- `IConfiguration` or `Environment.GetEnvironmentVariable()` — no OS-specific APIs.
- Assume case-sensitive file system (Linux).
- No Windows-only APIs (Registry, COM interop).

## Tests (xUnit)

- Method names: `MethodName_Condition_ExpectedResult` (underscores for readability).
- Arrange / Act / Assert structure with `//` comments.
- Act is exactly **one statement** — extract complex setup to Arrange.
- Mock only interfaces, only direct dependencies.
- Mirror source folder structure in test project.
- Test assembly naming: `CITL.<Layer>.Tests`.

---

Last Updated: February 2026
