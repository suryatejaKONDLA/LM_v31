---
name: 'C# Patterns'
description: 'C# patterns and conventions for all .cs files — coding style is in CODING_STANDARDS.md'
applyTo: '**/*.cs'
---

# C# Patterns

> **Coding style** (naming, formatting, braces, `var`, etc.) lives in `CODING_STANDARDS.md` and `.editorconfig`.
> This file covers **patterns** specific to the CITL codebase.

## Dependency Injection

- **Primary constructors** for all DI injection — no manual field + constructor boilerplate.
- Exceptions: complex constructor logic (`TenantRegistry`), abstract classes, exception types, validators.
- Constructor injection only — never service locator.
- Interfaces in `CITL.Application`, implementations in `CITL.Infrastructure`.

## Error Handling Patterns

- `Result<T>` / `Result` for business logic — no exceptions for expected failures.
- `SpResult` → `spResult.ToResult("ErrorCode")` for stored procedure outputs.
- `FluentValidation` → `validation.ToResult()` for input validation.
- Guard clauses at entry points: `Guard.NotNull()`, `Guard.Positive()`, etc.

## FluentValidation

- All request/input validation via FluentValidation — never manual if/else.
- Validators in `Application/Core/{Feature}/` beside the request DTO.
- Name: `{RequestName}Validator` inheriting `AbstractValidator<T>`.
- Auto-registered via `AddValidatorsFromAssemblyContaining<ApplicationAssemblyMarker>()`.
- Convert: `var validation = await validator.ValidateAsync(request, ct);` then `validation.ToResult()`.

## Logging

- `[LoggerMessage]` source-generator attributes — never `_logger.LogXxx()`.
- `partial` class + `private static partial void LogXxx(...)`.
- Template tokens: `PascalCasedCompoundWords` — no string interpolation.
