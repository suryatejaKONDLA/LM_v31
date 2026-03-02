using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Authentication;

/// <summary>
/// Application service interface for authentication operations.
/// Orchestrates login, token refresh, and logout flows.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with credentials and returns tokens.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <param name="tenantId">The tenant identifier from the request header.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the login response on success, or an error.</returns>
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes an access token using a valid refresh token.
    /// Implements refresh token rotation — old token is invalidated, new one is issued.
    /// </summary>
    /// <param name="request">The refresh token request.</param>
    /// <param name="tenantId">The tenant identifier from the request header.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the new login response on success, or an error.</returns>
    Task<Result<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request, string tenantId, CancellationToken cancellationToken);

    /// <summary>
    /// Logs out a user by blacklisting the current access token and revoking the refresh token.
    /// </summary>
    /// <param name="accessToken">The current JWT access token.</param>
    /// <param name="loginUser">The username to revoke refresh tokens for.</param>
    /// <param name="tenantId">The tenant identifier from the request header.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> LogoutAsync(string accessToken, string loginUser, string tenantId, CancellationToken cancellationToken);
}
