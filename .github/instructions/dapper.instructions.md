---
name: 'Dapper Data Access'
description: 'Dapper repository patterns and SQL conventions — coding style is in CODING_STANDARDS.md'
applyTo: '**/Infrastructure/**/*.cs'
---

# Dapper Data Access Conventions

> **Coding style** lives in `CODING_STANDARDS.md`. This file covers Dapper-specific patterns.

## Repository Pattern
- Interfaces in `CITL.Application`, `internal sealed` implementations in `CITL.Infrastructure`.
- Primary constructors — repos inject only `IDbExecutor`.
- Application layer never references Dapper or `System.Data` directly
- Repos are `internal sealed` — exposed only via interface

### Repository Template
```csharp
internal sealed class XxxRepository(IDbExecutor db) : IXxxRepository
{
    private const string GetSql = """
        SELECT e.Entity_ID, e.Entity_Name, e.Created_Date
        FROM schema.Entity e
        WHERE e.Is_Active = 1
        """;

    public async Task<SpResult> SaveAsync(SaveRequest request, CancellationToken ct)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["Entity_Name"] = request.Name,
            ["Session_ID"] = request.SessionId,
            ["BRANCH_Code"] = request.BranchCode
        };

        return await db.ExecuteSpAsync("schema.Entity_Insert", parameters, ct)
            .ConfigureAwait(false);
    }

    public async Task<EntityResponse?> GetAsync(CancellationToken ct)
    {
        return await db.QuerySingleOrDefaultAsync<EntityResponse>(GetSql, cancellationToken: ct)
            .ConfigureAwait(false);
    }
}
```

### Method Style Rules
- Always use **method bodies** with `async`/`await` — never expression-bodied (`=>`) for repository methods
- Extract SQL strings to `private const string` fields at the top of the class
- Extract SP parameters to a local `var parameters` variable — never pass inline `new Dictionary<>` to the executor
- Always call `.ConfigureAwait(false)` on awaited tasks (library code)

## SQL Queries
- Always use **parameterized queries** — never string concatenation or interpolation in SQL
- Use raw string literals (`"""..."""`) for multi-line queries — never `@"..."` or string concatenation
- Group related columns on a single line; don't put every column on its own line
- Keep SQL keywords uppercase: `SELECT`, `FROM`, `INNER JOIN`, `LEFT JOIN`, `WHERE`, `ORDER BY`
- Use meaningful parameter names: `@UserId`, `@TenantName`, not `@p1`

### SQL Formatting Style
```sql
SELECT
    am.APP_Code, am.APP_Header1, am.APP_Header2,
    am.APP_Logo1, am.APP_Logo2, am.APP_Logo3,
    am.APP_Created_ID, cu.Login_Name AS AppCreatedName, am.APP_Created_Date
FROM citl.App_Master am
    INNER JOIN citl.Login_Name cu ON cu.Login_ID = am.APP_Created_ID
    LEFT  JOIN citl.Login_Name mu ON mu.Login_ID = am.APP_Modified_ID
```

## Column Mapping — No `AS` Aliases
- **Global setting**: `DefaultTypeMap.MatchNamesWithUnderscores = true` is configured in `Infrastructure/DependencyInjection.cs`
- This makes Dapper auto-map `APP_Code` → `AppCode`, `APP_Header1` → `AppHeader1`, etc.
- **Do NOT use `AS` aliases** for columns whose names already match the C# property (after underscore removal)
- **Only alias** columns where the SQL column name truly differs from the C# property — e.g., join columns: `cu.Login_Name AS AppCreatedName`
- **Never** add `[JsonPropertyName("APP_Code")]` attributes to DTOs — rely on ASP.NET Core's default camelCase policy

## Connection Management
- **Never** create `SqlConnection` or call Dapper directly in repositories
- Repos use `IDbExecutor` which handles connection lifecycle internally
- `IDbConnectionFactory` is used internally by `DbExecutor` — repos should not inject it
- Connection open → execute → dispose is handled automatically

## Async Only
- Use `QueryAsync<T>`, `ExecuteAsync`, `QueryFirstOrDefaultAsync<T>`
- Never use synchronous Dapper methods (`Query`, `Execute`)
- Always pass `CancellationToken` where supported

## Mapping
- Dapper auto-maps underscored SQL columns to PascalCase properties (global `MatchNamesWithUnderscores`)
- Use `splitOn` for multi-table joins when mapping to multiple objects
- For complex mappings, use custom `ITypeHandler` implementations
- **Never** use `[Column]` attributes — they defeat auto-mapping simplicity

## Stored Procedures
- Use `IDbExecutor.ExecuteSpAsync(spName, params, ct)` — never build `DynamicParameters` manually
- Pass input parameters as `Dictionary<string, object?>` — keys are SP parameter names **without** the `@` prefix
- Standard output parameters (`@ResultVal`, `@ResultType`, `@ResultMessage`) are added automatically by `DbExecutor`
- Returns `SpResult` — the Application service calls `spResult.ToResult("ErrorCode")` to convert
- Never manually check `SpResult.IsSuccess` in the repository; return it raw

### SP Call Example
```csharp
public async Task<SpResult> SaveAsync(SaveRequest request, CancellationToken ct)
{
    var parameters = new Dictionary<string, object?>
    {
        ["Entity_Name"] = request.Name,
        ["Session_ID"] = request.SessionId,
        ["BRANCH_Code"] = request.BranchCode
    };

    return await db.ExecuteSpAsync("citlsp.Entity_Insert", parameters, ct)
        .ConfigureAwait(false);
}
```
