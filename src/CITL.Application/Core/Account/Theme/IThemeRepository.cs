using CITL.Application.Common.Models;

namespace CITL.Application.Core.Account.Theme;

/// <summary>
/// Repository interface for user theme operations.
/// Defined in Application layer; implemented in Infrastructure with Dapper.
/// </summary>
public interface IThemeRepository
{
    /// <summary>
    /// Gets the theme configuration for a user by login ID.
    /// </summary>
    /// <param name="loginId">The login ID of the user.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The theme response, or <c>null</c> if not found.</returns>
    Task<ThemeResponse?> GetAsync(int loginId, CancellationToken cancellationToken);

    /// <summary>
    /// Saves a user's theme configuration by calling <c>citlsp.Login_Theme_Set</c>.
    /// Uses MERGE internally (insert or update).
    /// </summary>
    /// <param name="loginId">The login ID of the user.</param>
    /// <param name="themeJson">The theme JSON string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The SP result with success/failure outcome.</returns>
    Task<SpResult> SaveAsync(int loginId, string themeJson, CancellationToken cancellationToken);
}
