using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Account.Theme;

/// <summary>
/// Application service interface for user theme operations.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets the current user's theme configuration.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the theme response on success.</returns>
    Task<Result<ThemeResponse>> GetAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the current user's theme configuration.
    /// </summary>
    /// <param name="request">The theme save request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> SaveAsync(SaveThemeRequest request, CancellationToken cancellationToken);
}
