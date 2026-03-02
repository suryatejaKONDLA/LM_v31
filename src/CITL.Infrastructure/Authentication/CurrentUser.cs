using System.Security.Claims;
using CITL.Application.Common.Interfaces;
using CITL.SharedKernel.Constants;
using Microsoft.AspNetCore.Http;

namespace CITL.Infrastructure.Authentication;

/// <summary>
/// Reads the current authenticated user's identity claims from <see cref="HttpContext.User"/>.
/// Registered as Scoped — one instance per HTTP request.
/// </summary>
/// <param name="httpContextAccessor">The HTTP context accessor.</param>
internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    /// <inheritdoc />
    public int LoginId => int.TryParse(
        User?.FindFirstValue(AuthConstants.LoginIdClaimType),
        System.Globalization.CultureInfo.InvariantCulture,
        out var id)
        ? id
        : 0;

    /// <inheritdoc />
    public string LoginUser => User?.FindFirstValue(AuthConstants.LoginUserClaimType) ?? string.Empty;

    /// <inheritdoc />
    public string LoginName => User?.FindFirstValue(AuthConstants.LoginNameClaimType) ?? string.Empty;

    /// <inheritdoc />
    public string TenantId => User?.FindFirstValue(TenantConstants.JwtClaimType) ?? string.Empty;

    /// <inheritdoc />
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
