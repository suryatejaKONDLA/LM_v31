---
name: 'API Controllers'
description: 'ASP.NET Core controller conventions and REST API patterns'
applyTo: '**/{Controllers,WebApi}/**/*.cs'
---

# API Controller Conventions

> **Coding style** lives in `CODING_STANDARDS.md`. This file covers controller-specific patterns.

## Controller Design
- Thin controllers — delegate to Application services.
- `[ApiController]`, `[Route("api/[controller]")]`, `[Produces("application/json")]`.
- Primary constructors for DI.
- `CancellationToken` in all async actions.
- 1–2 lines per action: call service, return `.ToActionResult()`.

## Authentication
- JWT Bearer: `Authorization: Bearer {token}`.
- `[Authorize]` by default; `[AllowAnonymous]` only for public endpoints.

## HTTP Methods & Status Codes
- `GET` → 200 / 404.
- `POST` → 201 (Created) or 200.
- `PUT` → 200 / 204.
- `DELETE` → 204 / 200.
- Validation → 400. Unauthorized → 401. Forbidden → 403. Server error → 500.

## Result → IActionResult
- `result.ToActionResult()` — auto-maps error codes to HTTP status.
- `result.ToActionResult("message")` — 200 with message on success.
- `result.ToCreatedResult(location)` — 201 Created.
- Error mapping: `NotFound` → 404, `Conflict` → 409, `Validation` → 400, default → 400.
