using CITL.Application.Common.Models;

namespace CITL.Application.Core.Authentication;

/// <summary>
/// Repository interface for authentication database operations.
/// Defined in Application layer; implemented in Infrastructure with Dapper.
/// </summary>
public interface IAuthenticationRepository
{
    /// <summary>
    /// Validates credentials by calling <c>citlsp.Login_Check</c>.
    /// The SP uses output parameters (<c>@ResultVal</c>, <c>@ResultType</c>, <c>@ResultMessage</c>).
    /// Passes geo/device info for audit logging.
    /// </summary>
    /// <param name="request">The login request containing credentials and geo/device info.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The SP result with authentication outcome.</returns>
    Task<SpResult> LoginCheckAsync(
        LoginRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets a user profile by username (lightweight lookup without password validation).
    /// Used after successful login check, refresh token, and other post-auth flows.
    /// </summary>
    /// <param name="loginUser">The username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The user profile, or <c>null</c> if not found.</returns>
    Task<UserProfile?> GetUserProfileAsync(string loginUser, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all roles assigned to a user from <c>citl.Login_ROLE_Mapping</c>.
    /// </summary>
    /// <param name="loginId">The user's login ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of role names.</returns>
    Task<IReadOnlyList<string>> GetUserRolesAsync(int loginId, CancellationToken cancellationToken);

    /// <summary>
    /// Stores a refresh token hash in the database via <c>citlsp.Refresh_Token</c>.
    /// </summary>
    /// <param name="loginUser">The username.</param>
    /// <param name="refreshTokenHash">The SHA-256 hash of the refresh token.</param>
    /// <param name="expiryDate">The refresh token expiry date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<SpResult> StoreRefreshTokenAsync(
        string loginUser,
        byte[] refreshTokenHash,
        DateTime expiryDate,
        CancellationToken cancellationToken);

    /// <summary>
    /// Blacklists a JWT token hash in the database via <c>citlsp.BlackList_Token</c>.
    /// </summary>
    /// <param name="tokenHash">The SHA-256 hash of the JWT token.</param>
    /// <param name="createdDate">The token's original creation date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<SpResult> BlacklistTokenAsync(byte[] tokenHash, DateTime createdDate, CancellationToken cancellationToken);

    /// <summary>
    /// Validates a refresh token hash against the database store.
    /// </summary>
    /// <param name="loginUser">The username.</param>
    /// <param name="refreshTokenHash">The SHA-256 hash to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the hash matches and the token has not expired.</returns>
    Task<bool> ValidateRefreshTokenAsync(string loginUser, byte[] refreshTokenHash, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all branches mapped to a user from <c>citl.Login_Branch_Mapping</c>.
    /// </summary>
    /// <param name="loginId">The user's login ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of branches with code and name.</returns>
    Task<IReadOnlyList<BranchInfo>> GetUserBranchesAsync(int loginId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the number of recent failed login attempts for a user from <c>citl_sys.Login_Failed_Attempts</c>.
    /// Only counts attempts within the decay window (default 30 minutes).
    /// </summary>
    /// <param name="loginUser">The username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The failed attempt count (0 if none or expired).</returns>
    Task<int> GetFailedAttemptCountAsync(string loginUser, CancellationToken cancellationToken);
}
