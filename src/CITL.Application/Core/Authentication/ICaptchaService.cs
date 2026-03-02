using CITL.SharedKernel.Results;

namespace CITL.Application.Core.Authentication;

/// <summary>
/// Generates and validates image CAPTCHAs with dark/light theme support.
/// Single call returns both theme images and stores the answer in cache.
/// </summary>
public interface ICaptchaService
{
    /// <summary>
    /// Generates a CAPTCHA if the user has exceeded the failed attempt threshold.
    /// Returns both dark and light theme images in a single call.
    /// When CAPTCHA is not required, returns empty images with <c>CaptchaRequired = false</c>.
    /// </summary>
    /// <param name="request">The request containing the username.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result containing the CAPTCHA response.</returns>
    Task<Result<CaptchaResponse>> GenerateAsync(CaptchaRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether a CAPTCHA is required for the given user based on failed attempt count.
    /// </summary>
    /// <param name="loginUser">The username to check.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the user has exceeded the failed attempt threshold.</returns>
    Task<bool> IsCaptchaRequiredAsync(string loginUser, CancellationToken cancellationToken);

    /// <summary>
    /// Validates a CAPTCHA answer against the stored value.
    /// Each CAPTCHA is single-use — removed after validation regardless of outcome.
    /// </summary>
    /// <param name="captchaId">The CAPTCHA identifier from the generate response.</param>
    /// <param name="captchaValue">The user's answer.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<Result> ValidateAsync(string captchaId, string captchaValue, CancellationToken cancellationToken);
}
