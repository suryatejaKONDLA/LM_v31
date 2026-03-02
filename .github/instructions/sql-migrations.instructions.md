---
name: 'SQL Migration Scripts'
description: 'DbUp SQL migration script conventions'
applyTo: '**/Migrations/**/*.sql,**/Scripts/**/*.sql'
---

# SQL Migration Script Conventions

## File Naming
- Use sequential numbering: `0001_CreateUsersTable.sql`, `0002_AddIndexOnEmail.sql`
- Use descriptive names that explain what the script does
- Never rename or modify an already-applied script — create a new one

## Script Rules
- Every script must be **idempotent** where possible (use `IF NOT EXISTS` checks)
- Include a header comment with: purpose, date, author
- Test against a fresh database AND an existing database before committing
- Scripts run against ALL tenant databases — ensure they are safe for any tenant state

## Transactions
- Wrap DDL changes in transactions where supported
- Keep scripts focused — one logical change per script
- Do not mix DDL (schema) and DML (data) in the same script unless necessary

## Cross-Platform
- Use ANSI SQL where possible
- Target SQL Server syntax specifically only when needed
- Never use hardcoded file paths in SQL scripts
