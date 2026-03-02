---
name: 'JSON Configuration'
description: 'JSON formatting and configuration file conventions'
applyTo: '**/*.json'
---

# JSON Configuration Conventions

## Formatting
- Use 2-space indentation
- Use double quotes for all keys and string values
- No trailing commas

## appsettings.json
- Never commit secrets (passwords, API keys, connection strings with credentials)
- Use `{dbName}` placeholder in connection strings for multi-tenant resolution
- Use hierarchical configuration: `"Section:SubSection:Key"`
- Prefer `IOptions<T>` pattern for strongly-typed configuration

## Sensitive Data
- Use User Secrets for local development: `dotnet user-secrets`
- Use environment variables or Azure Key Vault for production
- Add sensitive config files to `.gitignore`
