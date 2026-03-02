namespace CITL.Application.Common.Interfaces;

/// <summary>
/// Provides access to the current authenticated user's identity claims.
/// Registered as Scoped — reads claims from <c>HttpContext.User</c>.
/// </summary>
public interface ICurrentUser
{
    /// <summary>Gets the Login ID (primary key).</summary>
    int LoginId { get; }

    /// <summary>Gets the login username.</summary>
    string LoginUser { get; }

    /// <summary>Gets the display name.</summary>
    string LoginName { get; }

    /// <summary>Gets the tenant identifier from the JWT claim.</summary>
    string TenantId { get; }

    /// <summary>Gets a value indicating whether the user is authenticated.</summary>
    bool IsAuthenticated { get; }
}
