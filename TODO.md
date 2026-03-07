# CITL — Feature Implementation TODO

> Prioritized list of features to implement before production.
> Work through each tier sequentially, completing all items before moving to the next.

---

## Tier 1 — Security Essentials (Must-have before production)

- [ ] **1.1 CORS Policy**
  - React SPA won't work without it — browsers block cross-origin requests
  - Add `AddCors()` in DI with a named policy for allowed origins (from `appsettings.json`)
  - Add `UseCors()` in pipeline **before** `UseAuthentication()`
  - Support configurable origins, methods, headers, credentials
  - Allow all origins in Development, restrict in Production
  - Files: `Program.cs`, `appsettings.json`, new `CorsSettings` class

- [ ] **1.2 Rate Limiting**
  - Auth endpoints (login, forgot-password, CAPTCHA) are brute-forceable without throttling
  - Use built-in `System.Threading.RateLimiting` (.NET 7+)
  - Add `AddRateLimiter()` with policies: `fixed` (global), `sliding` (auth endpoints), `token-bucket` (API)
  - Add `UseRateLimiter()` in pipeline **before** `UseAuthentication()`
  - Apply `[EnableRateLimiting("auth")]` on `AuthenticationController`
  - Return `429 Too Many Requests` with `Retry-After` header using `ApiResponse` envelope
  - Configurable via `RateLimitSettings` from `appsettings.json`
  - Files: `Program.cs`, `appsettings.json`, new `RateLimitSettings`, `AuthenticationController`

- [ ] **1.3 Security Headers Middleware**
  - Add custom middleware to set standard security headers on every response:
    - `X-Content-Type-Options: nosniff` — prevents MIME-type sniffing
    - `X-Frame-Options: DENY` — prevents clickjacking
    - `X-XSS-Protection: 0` — modern recommendation (CSP handles this)
    - `Referrer-Policy: strict-origin-when-cross-origin` — limits referrer leakage
    - `Content-Security-Policy: default-src 'self'` — XSS protection (configure for SPA)
    - `Permissions-Policy: camera=(), microphone=(), geolocation=()` — restricts browser APIs
    - `Strict-Transport-Security: max-age=31536000; includeSubDomains` — HSTS (production only)
  - Remove `Server` header and `X-Powered-By` header
  - Place in pipeline after `UseForwardedHeaders()`, before CORS
  - Files: new `SecurityHeadersMiddleware.cs`, `Program.cs`
  - Tests: new `SecurityHeadersMiddlewareTests.cs`

- [ ] **1.4 Forwarded Headers**
  - Required when behind reverse proxy (Nginx, Docker, load balancer)
  - Without this, `HttpContext.Connection.RemoteIpAddress` and `Request.Scheme` are wrong
  - Add `UseForwardedHeaders()` at the very start of the pipeline (before everything)
  - Configure `ForwardedHeadersOptions` with `XForwardedFor | XForwardedProto`
  - Files: `Program.cs`

- [ ] **1.5 Fix Tenant Middleware Response Consistency**
  - `TenantResolutionMiddleware` and `TenantGuardMiddleware` return anonymous `{ Code, Message }`
  - Should use the standard `ApiResponse` envelope matching `GlobalExceptionMiddleware` pattern
  - Update both to write `ApiResponse.Error(message)` with proper `ApiResponseCode`
  - Files: `TenantResolutionMiddleware.cs`, `TenantGuardMiddleware.cs`
  - Tests: update existing middleware tests if response shape changes

---

## Tier 2 — Production Hardening (Should-have)

- [ ] **2.1 Request Timeouts**
  - Long-running requests can tie up server threads indefinitely
  - Add `AddRequestTimeouts()` with default timeout (30s) and named policies
  - Add `UseRequestTimeouts()` in pipeline before `MapControllers()`
  - Apply shorter timeouts on specific endpoints (e.g., login: 10s, file upload: 120s)
  - Timeout triggers `OperationCanceledException` → caught by `GlobalExceptionMiddleware` → 499
  - Files: `Program.cs`, controller attributes

- [ ] **2.2 Response Compression**
  - JSON API payloads compress well with gzip/brotli (50–80% reduction)
  - Add `AddResponseCompression()` with Brotli (preferred) + Gzip providers
  - Add `UseResponseCompression()` in pipeline after exception handler
  - Exclude already-compressed content types (images, binary files)
  - Configure minimum response size (e.g., 1 KB) to avoid overhead on tiny responses
  - Files: `Program.cs`

- [ ] **2.3 HTTPS Redirection / HSTS**
  - `UseHsts()` — tells browsers to always use HTTPS (production only, not Development)
  - `UseHttpsRedirection()` — redirects HTTP requests to HTTPS
  - Place after `UseForwardedHeaders()`, before security headers
  - Files: `Program.cs`

- [ ] **2.4 Options Validation at Startup**
  - Settings classes (`JwtSettings`, `TenantSettings`, `FileStorageSettings`, `R2Settings`, `RedisSettings`) are not validated
  - App starts with bad config and fails at runtime instead of startup
  - Add `[Required]` / `[Range]` data annotations on settings properties
  - Register with `.ValidateDataAnnotations().ValidateOnStart()` — app fails fast on misconfiguration
  - Add custom `IValidateOptions<T>` for complex validation (e.g., JWT secret minimum length)
  - Files: all settings classes, DI registration in `Program.cs` / extension methods

- [ ] **2.5 Polly Resilience on HTTP Calls**
  - External HTTP calls (Grafana health check, OTLP, R2 S3) have no retry/circuit-breaker
  - Add `Microsoft.Extensions.Http.Resilience` (Polly v8 integration)
  - Configure `AddStandardResilienceHandler()` on named `HttpClient` registrations
  - Policies: retry with exponential backoff (3 attempts), circuit breaker (5 failures → 30s break)
  - Files: `Program.cs` or HttpClient DI registration, NuGet package add

---

## Tier 3 — Future-Proofing (Nice-to-have)

- [ ] **3.1 API Versioning**
  - URL-based versioning (`/api/v1/...`) is simplest for React + Flutter clients
  - Add `Asp.Versioning.Http` + `Asp.Versioning.Mvc.ApiExplorer` NuGet packages
  - Configure `AddApiVersioning()` with `DefaultApiVersion = 1.0`, `AssumeDefaultVersionWhenUnspecified = true`
  - Group controllers by version, update Swagger/OpenAPI doc generation
  - Files: `Program.cs`, all controllers get `[ApiVersion("1.0")]`, route prefix update

- [ ] **3.2 DbUp Migration Runner**
  - ADR calls this out; Infrastructure DI has a TODO comment
  - Create `Migrations/` folder in Infrastructure with SQL scripts
  - Add `DbUp` NuGet package
  - Implement `IMigrationRunner` that loops through all tenant DBs and applies pending scripts
  - Run on startup (or via CLI command) with `SchemaVersions` tracking table per tenant DB
  - Files: new `Migrations/` folder, `IMigrationRunner`, `DbUpMigrationRunner`, DI registration

- [ ] **3.3 Redis Pub/Sub Cache Invalidation**
  - ADR mentions this — L1 (MemoryCache) invalidation across server instances
  - When data changes on one instance, publish invalidation message via Redis Pub/Sub
  - All instances subscribe and clear their L1 cache for the affected key/pattern
  - Prevents stale L1 data in multi-instance deployments
  - Files: extend `RedisCacheService`, new `CacheInvalidationSubscriber` background service

---

## Inconsistencies to Fix

- [ ] **Tenant middleware responses** — use `ApiResponse` envelope (covered in 1.5)
- [ ] **Secrets in appsettings** — move to User Secrets / environment variables (operational task)
- [ ] **CA2007 suppression in WebApi** — add to WebApi `.editorconfig` section or project file
- [ ] **`ConfigureAwait(false)` in `HealthCheckResponseWriter`** — remove per CODING_STANDARDS.md (WebApi layer must not use it)

---

## Login Master: Email Verification Flow

> Context: Admin creates user accounts. Welcome email carries the initial password.
> A wrong email means credentials land in a stranger's inbox — actual security risk.
> Goal: confirm the email is reachable **before** the password email fires.

- [ ] **DB Migration**
  - Add columns to `LOGIN_Master`:
    - `Login_Email_Verified BIT NOT NULL DEFAULT 0`
    - `Login_Email_VerifyToken NVARCHAR(100) NULL`
    - `Login_Email_VerifyToken_Expiry DATETIME2 NULL`

- [ ] **On INSERT** (in `LoginMasterRepository` / stored proc)
  - Generate a token (`NEWID()`)
  - Set expiry to `+48h`
  - Send a **verification-only** email first: *"Confirm your email to receive your login credentials"*
  - Do **not** send the welcome email (with password) until verified

- [ ] **On Email Verified**
  - New SP: `usp_LoginMaster_VerifyEmail(@Token)`
    - Validates token is not expired
    - Sets `Login_Email_Verified = 1`, nulls out token + expiry
    - Returns SP result
  - Fire the welcome email with password **after** verification succeeds

- [ ] **New public endpoint**: `POST /LoginMaster/verify-email`
  - No `[Authorize]` — must be reachable without a JWT
  - Accepts `{ token: string }`
  - Calls `usp_LoginMaster_VerifyEmail`
  - Returns a simple success/failure `ApiResponse`

- [ ] **Admin override**: Manual verify on the form
  - For legacy/imported users where email is confirmed by other means
  - New SP: `usp_LoginMaster_ManualVerify(@Login_ID)`
  - New endpoint: `POST /LoginMaster/{id}/verify-manual` (authorized, admin-only)

- [ ] **Resend verification**
  - New SP: regenerate token + expiry, return new token
  - New endpoint: `POST /LoginMaster/{id}/resend-verification` (authorized)
  - Re-sends verification email with fresh token/link

- [ ] **LoginMaster form badge**
  - Show `✓ Verified` (green) or `⚠ Unverified` (orange) next to Email Address field
  - Show "Resend" button when unverified (calls resend endpoint)
  - Show "Mark Verified" button for admin override

- [ ] **Frontend verify-email page**
  - Route: `/verify-email?token=xxx` (public, no auth required)
  - Calls `POST /LoginMaster/verify-email` on mount
  - Shows success card or error card (token expired / invalid)

- [ ] **No login block** — verification is about ensuring credentials reach the right person, not gating access

---

Last Updated: March 8, 2026
