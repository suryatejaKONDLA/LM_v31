using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Common.Validation;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Account.Theme;

/// <summary>
/// Application service for user theme operations.
/// </summary>
public sealed partial class ThemeService(
    IThemeRepository themeRepository,
    ICurrentUser currentUser,
    IValidator<SaveThemeRequest> validator,
    ILogger<ThemeService> logger) : IThemeService
{
    /// <inheritdoc />
    public async Task<Result<ThemeResponse>> GetAsync(CancellationToken cancellationToken)
    {
        var theme = await themeRepository.GetAsync(
            currentUser.LoginId,
            cancellationToken).ConfigureAwait(false);

        if (theme is null)
        {
            // Return empty theme — user hasn't customized yet
            return Result.Success(new ThemeResponse
            {
                LoginId = currentUser.LoginId,
                ThemeJson = """{"tokens":{}}""",
            });
        }

        LogThemeRetrieved(logger, currentUser.LoginId);
        return Result.Success(theme);
    }

    /// <inheritdoc />
    public async Task<Result> SaveAsync(
        SaveThemeRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator
            .ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult();
        }

        var spResult = await themeRepository.SaveAsync(
            currentUser.LoginId,
            request.ThemeJson,
            cancellationToken).ConfigureAwait(false);

        if (!spResult.IsSuccess)
        {
            LogThemeSaveFailed(logger, currentUser.LoginId, spResult.ResultMessage);
        }
        else
        {
            LogThemeSaved(logger, currentUser.LoginId);
        }

        return spResult.ToResult("Account.ThemeSaveFailed");
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Theme retrieved for LoginId {LoginId}")]
    private static partial void LogThemeRetrieved(ILogger logger, int loginId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Theme saved for LoginId {LoginId}")]
    private static partial void LogThemeSaved(ILogger logger, int loginId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Theme save failed for LoginId {LoginId}: {Reason}")]
    private static partial void LogThemeSaveFailed(ILogger logger, int loginId, string reason);
}
