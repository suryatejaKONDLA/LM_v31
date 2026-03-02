using CITL.Application.Common.Models;

namespace CITL.Application.Core.Account;

/// <summary>
/// Repository interface for account-related database operations.
/// Defined in Application layer; implemented in Infrastructure with Dapper.
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Gets the full profile for a user by login ID.
    /// Joins Login_Master, Login_Master2, Login_Master_Pic, and Login_Startup_Page.
    /// </summary>
    /// <param name="loginId">The login ID of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The profile response, or <c>null</c> if not found.</returns>
    Task<ProfileResponse?> GetProfileAsync(int loginId, CancellationToken cancellationToken);

    /// <summary>
    /// Changes a user's password by calling <c>citlsp.Password_Reset</c>.
    /// The SP validates the old password and hashes the new one with SHA2_512.
    /// </summary>
    /// <param name="loginId">The login ID of the user.</param>
    /// <param name="newPassword">The new password (plain text — hashed by the SP).</param>
    /// <param name="oldPassword">The current password for verification.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The SP result with success/failure outcome.</returns>
    Task<SpResult> ChangePasswordAsync(int loginId, string newPassword, string oldPassword, CancellationToken cancellationToken);

    /// <summary>
    /// Updates a user's profile by calling <c>citlsp.Login_Profile_Update</c>.
    /// The SP updates Login_Master, Login_Master2, Login_Master_Pic, and Login_Startup_Page.
    /// </summary>
    /// <param name="request">The profile update request.</param>
    /// <param name="loginId">The login ID of the user (also used as session ID for audit).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The SP result with success/failure outcome.</returns>
    Task<SpResult> UpdateProfileAsync(UpdateProfileRequest request, int loginId, CancellationToken cancellationToken);
}
