using System.Text.Json.Serialization;

namespace CITL.Application.Core.Authentication;

/// <summary>
/// Request DTO for generating a CAPTCHA.
/// The service checks failed attempt count and only generates an image if required.
/// </summary>
public sealed class CaptchaRequest
{
    /// <summary>Gets the username to check failed attempts for.</summary>
    [JsonPropertyName("Login_User")]
    public required string LoginUser { get; init; }
}

/// <summary>
/// Response DTO containing CAPTCHA images and metadata.
/// When <see cref="CaptchaRequired"/> is <c>false</c>, the image fields are empty.
/// </summary>
public sealed class CaptchaResponse
{
    /// <summary>Gets the unique CAPTCHA identifier for validation.</summary>
    [JsonPropertyName("Captcha_Id")]
    public required string CaptchaId { get; init; }

    /// <summary>Gets the base64-encoded PNG image for light theme.</summary>
    [JsonPropertyName("Captcha_Image_Light")]
    public required string CaptchaImageLight { get; init; }

    /// <summary>Gets the base64-encoded PNG image for dark theme.</summary>
    [JsonPropertyName("Captcha_Image_Dark")]
    public required string CaptchaImageDark { get; init; }

    /// <summary>Gets a value indicating whether the user must solve a CAPTCHA.</summary>
    [JsonPropertyName("Captcha_Required")]
    public required bool CaptchaRequired { get; init; }

    /// <summary>Gets the current number of failed login attempts.</summary>
    [JsonPropertyName("Failed_Attempts")]
    public required int FailedAttempts { get; init; }
}
