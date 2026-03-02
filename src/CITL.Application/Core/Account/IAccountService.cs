using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Account;

/// <summary>
/// Application service interface for account management operations.
/// Orchestrates profile retrieval, profile update, and password change flows.
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// Gets the current user's profile.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the profile response on success, or an error.</returns>
    Task<Result<ProfileResponse>> GetProfileAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Changes the current user's password.
    /// Validates the old password server-side via the stored procedure.
    /// </summary>
    /// <param name="request">The change password request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    /// <param name="request">The profile update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken);
}
