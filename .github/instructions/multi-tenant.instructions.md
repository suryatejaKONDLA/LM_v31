---
name: 'Multi-Tenant Rules'
description: 'Multi-tenant data isolation and tenant context rules'
applyTo: '**/{Infrastructure,Application}/**/*.cs'
---

# Multi-Tenant Rules

## Tenant Context
- Scoped `TenantContext` via DI — **never** `AsyncLocal`, `ThreadLocal`, or static fields.
- Middleware resolves tenant from `X-Tenant-Id` header → sets `TenantContext`.

## Connections
- All DB connections via `IDbConnectionFactory` (reads tenant from `TenantContext`).
- Connection strings use `{dbName}` placeholder replaced at runtime.
- Never hardcode a database name.

## Background Tasks
- New `IServiceScope` via `IServiceScopeFactory` — set tenant explicitly.
- Never `Task.Run` with captured `HttpContext`.

## Cache Keys
- Format: `{tenant}:{entity}:{id}` — always include tenant prefix.

## Data Queries
- Database-per-tenant — no `WHERE TenantId` needed.
- Never share connections across tenant boundaries.
