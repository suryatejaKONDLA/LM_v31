# Contributing to CITL

## Overview

CITL is a multi-tenant application with extensible architecture.

## Code of Conduct

Be respectful, inclusive, and professional in all interactions.

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/suryatejaKONDLA/CITL_v31.git
   cd CITL
   ```

2. **Understand the architecture**
   - Clean Architecture: `WebApi → Application → Domain ← Infrastructure`
   - Multi-tenant design with database-per-tenant isolation


3. **Set up your development environment**
   - .NET 11+ SDK
   - Visual Studio 2022+ (with ASP.NET workload) or VS Code with C# extension
   - SQL Server (local or remote)
   - Configure secrets via `dotnet user-secrets` — see the [Secrets Setup](README.md#3-configure-secrets) section in `README.md`

4. **Never commit secrets** — `appsettings*.json` must not contain passwords, API keys, or connection strings with credentials. All sensitive values go into User Secrets (Development) or environment variables (Production).

4. **Familiarize yourself with the codebase**
   - Read [CODING_STANDARDS.md](./CODING_STANDARDS.md) — single source of truth for all style rules
   - Check [.editorconfig](./.editorconfig) — enforces rules at build time
   - Review [Architecture Decisions](./.github/ARCHITECTURE_DECISIONS.md)

## Before You Start

1. **Check existing issues**: Search for similar feature requests or bug reports
2. **Create an issue**: If one doesn't exist, create a detailed issue describing:
   - Problem statement
   - Expected behavior
   - Steps to reproduce (for bugs)
   - Affected module(s)

3. **Discuss design**: For significant changes, create a design proposal issue and get feedback before coding

## Development Workflow

### 1. Create a Branch
```bash
git checkout -b feature/user-authentication
```

Use kebab-case naming: `feature/`, `fix/`, `docs/`, `refactor/`

### 2. Make Changes

- Follow naming conventions in `.editorconfig`
- Use file-scoped namespaces
- Add XML documentation for public APIs
- Keep commits small and logical
- Write descriptive commit messages

```bash
git commit -m "Add JWT authentication for API endpoints"
```

### 3. Add Tests

- Write unit tests using **xUnit**
- Test all public methods
- Include both success and failure scenarios
- Use **Moq** for mocking dependencies

Example test structure:
```csharp
[Fact]
public async Task GetUser_WithValidId_ReturnsUser()
{
    // Arrange
    var userId = 123;

    // Act
    var result = await _userService.GetUserAsync(userId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(userId, result.Id);
}
```

### 4. Validate Code Quality

- Run EditorConfig checks: `dotnet format --verify-no-changes`
- Run code analyzer: `dotnet build /p:EnforceCodeStyleInBuild=true`
- Ensure all tests pass: `dotnet test`

### 5. Submit a Pull Request

**Before submitting:**
- [ ] Title clearly describes the change
- [ ] Description references the issue number(s)
- [ ] Tests are included and passing
- [ ] Code follows `.editorconfig` rules
- [ ] No breaking changes (unless approved)
- [ ] Commit history is clean

**PR Template:**
```
## Description
Fixes #123 - Brief description of what this PR does

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Area(s) Affected
- [ ] API
- [ ] Services
- [ ] Repository
- [ ] Models
- [ ] Common

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing completed

## Checklist
- [ ] Code follows style guidelines
- [ ] XML comments added for public APIs
- [ ] No console.WriteLine() or Debug.WriteLine() calls
- [ ] Proper error handling and logging
- [ ] Tenant isolation verified (if applicable)
```

## Multi-Tenant Best Practices

### Always Consider Tenant Isolation

- Always validate tenant context before performing critical operations
- Never allow cross-tenant data access — filter all queries by tenant
- Tenant context propagation and implementation patterns will be defined during architecture phase

### Logging Context

- Always include tenant and user context in log entries
- Logging framework and conventions will be finalized during development

## Performance Guidelines

- Use async/await to prevent thread blocking
- Implement caching for frequently accessed data
- Use pagination for large result sets
- Avoid N+1 query problems
- Profile code before optimizing (premature optimization is expensive)

## Reporting Issues

When reporting a bug, include:
1. **Environment**: OS, .NET version, Visual Studio version
2. **Reproduction steps**: Exact steps to reproduce the issue
3. **Expected behavior**: What should happen
4. **Actual behavior**: What actually happens
5. **Screenshots**: If applicable
6. **Error logs**: Stack traces, log output
7. **Affected area**: API, Services, Repository, Models, or Common

Example:
```
**Environment**: Windows 11 / Linux / macOS, .NET 11.0, VS 2022

**Area**: Services

**Reproduction**:
1. Step-by-step reproduction steps

**Expected**: Expected behavior

**Actual**: Actual behavior

**Logs**:
[Error] ...
```

## Release Process

1. Features are merged to `develop`
2. Release candidate created from `develop`
3. Testing and fixes applied
4. Version bump and release to `main`
5. Tags created for version tracking

## Additional Resources

- [Copilot Instructions](./.github/copilot-instructions.md)
- [Coding Standards](./CODING_STANDARDS.md)
- [EditorConfig Rules](./.editorconfig)
- [CITL Architecture Documentation](#) (To be added)

## Questions?

- Open an issue with `question` label
- Check existing discussions
- Engage with the community

---

Thank you for contributing to CITL! 🎉
