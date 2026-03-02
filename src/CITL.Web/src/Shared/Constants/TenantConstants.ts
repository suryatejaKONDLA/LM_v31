/**
 * Cross-cutting constants for multi-tenant resolution.
 * Mirrors CITL.SharedKernel.Constants.TenantConstants.
 */
export const TenantConstants = {
    /** HTTP request header name carrying the tenant identifier. */
    HeaderName: "X-Tenant-Id",

    /** JWT claim type that carries the tenant identifier. */
    JwtClaimType: "tenant_id",
} as const;
