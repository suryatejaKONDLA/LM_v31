---
name: 'Test Conventions'
description: 'Unit and integration test patterns — coding style is in CODING_STANDARDS.md'
applyTo: '**/*Test*/**/*.cs'
---

# Testing Conventions

> Test style rules (naming, AAA pattern) are also in `CODING_STANDARDS.md`.

## Naming
- `MethodName_Condition_ExpectedResult` — e.g. `GetAsync_WithValidId_ReturnsUser`.

## Structure
- Arrange / Act / Assert with `//` comments.
- Act is exactly **one statement** — extract setup to Arrange.

## Mocking
- Mock only interfaces, only direct dependencies.
- Descriptive setup — make clear what the mock returns.

## What to Test
- Public service methods, edge cases, error paths, tenant isolation.

## What NOT to Test
- Private methods, framework internals, simple DTOs.
