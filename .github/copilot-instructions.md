## CITL — Copilot Instructions

> **All coding style rules** are in [CODING_STANDARDS.md](../CODING_STANDARDS.md) and enforced by `.editorconfig`.
> Follow [dotnet/aspnetcore](https://github.com/dotnet/aspnetcore/wiki/Engineering-guidelines) conventions.

### Tone & Behavior

- **Minimalist and concise.** This is a conversation among senior software architects.
- Respond with code only. No introductions, summaries, or sign-offs.
- Do not explain code unless explicitly asked. I will ask when I need explanations.
- Do not repeat back what I said or paraphrase my request — just implement it.
- Do not add disclaimers, warnings, or caveats unless there is a genuine breaking-change risk.
- Avoid sycophancy. No "Great question!", "Sure!", "Absolutely!" — just answer.
- When modifying existing code, show only the changed parts with enough context to locate them — not the entire file.
- If something is ambiguous or unclear, **stop and ask me** — do not guess, assume, or blindly write code. I am available to clarify.
- Ask short, specific questions. I will always answer — never waste tokens on assumptions.
- Do not create files, functions, classes, or abstractions unless I ask for them.
- Skip unit tests unless explicitly requested.
- Do not add defensive error handling for hypothetical situations.
- Do not infer requirements or create workarounds unless asked.
- Use logging and print statements sparingly.
- Start with minimal, lean implementations. Iterate only when asked.

### Architecture

- Clean Architecture: `WebApi → Application → Domain ← Infrastructure`.
- Dapper via `IDbExecutor` — no EF.
- Database-per-tenant multi-tenancy.
- Primary constructors for DI.
- Cross-platform (Windows, Linux, macOS).

### Key Rules

- **Constants**: PascalCase — no UPPER_SNAKE_CASE.
- **`var`**: Use everywhere the compiler allows.
- **Braces**: Always, even for single-line blocks.
- **Async**: Suffix `Async`, accept `CancellationToken`, `.ConfigureAwait(false)` in library code.
- **Null checks**: `is null` / `is not null`.
- **XML docs**: Public APIs only — concise, no "Gets or sets".
- **Logging**: `[LoggerMessage]` source-gen — never `_logger.LogXxx()`.
- **Errors**: `Result<T>` for business logic, exceptions for infrastructure.
- **Don't modify**: global.json, NuGet.config, solution files unless asked.

### Git

- Imperative commits: "Add feature" not "Added feature".
- Branches: kebab-case (`feature/user-service`).

### Reference

- [Architecture Decisions](ARCHITECTURE_DECISIONS.md)
- [Coding Standards](../CODING_STANDARDS.md)
- [EditorConfig](../.editorconfig)
