using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Authentication;
using CITL.SharedKernel.Helpers;
using CITL.SharedKernel.Results;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace CITL.Infrastructure.Core.Authentication;

/// <summary>
/// Generates image CAPTCHAs using SkiaSharp with dark/light theme support.
/// Stores CAPTCHA answers in Redis via <see cref="ICacheService"/> (tenant-scoped, short TTL).
/// </summary>
internal sealed partial class CaptchaService(
    IAuthenticationRepository authRepository,
    ICacheService cacheService,
    ITenantContext tenantContext,
    ILogger<CaptchaService> logger) : ICaptchaService
{
    private static readonly string[] Fonts = ["Arial", "Verdana", "Times New Roman", "Courier New", "Georgia"];
    private const int CaptchaLength = 6;
    private const int CaptchaWidth = 240;
    private const int CaptchaHeight = 60;
    private const int CaptchaExpiryMinutes = 5;
    private const int FailedAttemptThreshold = 2;
    private const string CachePrefix = "captcha";

    // Excludes confusing characters: 0/O, 1/I/l
    private const string AllowedChars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz";

    /// <inheritdoc />
    public async Task<Result<CaptchaResponse>> GenerateAsync(
        CaptchaRequest request,
        CancellationToken cancellationToken)
    {
        var failedAttempts = await authRepository.GetFailedAttemptCountAsync(
            request.LoginUser, cancellationToken).ConfigureAwait(false);

        // CAPTCHA not required — return empty response
        if (failedAttempts < FailedAttemptThreshold)
        {
            LogCaptchaNotRequired(logger, request.LoginUser, failedAttempts);

            return Result.Success(new CaptchaResponse
            {
                CaptchaId = string.Empty,
                CaptchaImageLight = string.Empty,
                CaptchaImageDark = string.Empty,
                CaptchaRequired = false,
                FailedAttempts = failedAttempts
            });
        }

        // Generate CAPTCHA value and images
        var captchaValue = CryptoHelper.GenerateRandomCode(CaptchaLength, AllowedChars);
        var captchaId = CryptoHelper.GenerateBase64UrlToken(24);
        var imageLight = GenerateCaptchaImage(captchaValue, isLightTheme: true);
        var imageDark = GenerateCaptchaImage(captchaValue, isLightTheme: false);

        // Store in Redis with tenant-scoped key
        var cacheKey = $"{CachePrefix}:{tenantContext.TenantId}:{captchaId}";
        await cacheService.SetAsync(
            cacheKey,
            new CaptchaStoredValue { Value = captchaValue },
            new() { AbsoluteExpiration = TimeSpan.FromMinutes(CaptchaExpiryMinutes) },
            cancellationToken).ConfigureAwait(false);

        LogCaptchaGenerated(logger, request.LoginUser, failedAttempts);

        return Result.Success(new CaptchaResponse
        {
            CaptchaId = captchaId,
            CaptchaImageLight = imageLight,
            CaptchaImageDark = imageDark,
            CaptchaRequired = true,
            FailedAttempts = failedAttempts
        });
    }

    /// <inheritdoc />
    public async Task<bool> IsCaptchaRequiredAsync(
        string loginUser,
        CancellationToken cancellationToken)
    {
        var failedAttempts = await authRepository.GetFailedAttemptCountAsync(
            loginUser, cancellationToken).ConfigureAwait(false);

        return failedAttempts >= FailedAttemptThreshold;
    }

    /// <inheritdoc />
    public async Task<Result> ValidateAsync(
        string captchaId,
        string captchaValue,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(captchaId) || string.IsNullOrWhiteSpace(captchaValue))
        {
            return Result.Failure(new("Captcha.Missing", "CAPTCHA ID and value are required."));
        }

        var cacheKey = $"{CachePrefix}:{tenantContext.TenantId}:{captchaId}";
        var stored = await cacheService.GetAsync<CaptchaStoredValue>(cacheKey, cancellationToken)
            .ConfigureAwait(false);

        // Always remove after retrieval — single-use
        await cacheService.RemoveAsync(cacheKey, cancellationToken).ConfigureAwait(false);

        if (stored is null)
        {
            return Result.Failure(new("Captcha.Expired", "Invalid or expired CAPTCHA. Please refresh."));
        }

        if (!string.Equals(stored.Value, captchaValue, StringComparison.Ordinal))
        {
            LogCaptchaFailed(logger, captchaId);
            return Result.Failure(new("Captcha.Invalid", "Invalid CAPTCHA. Please try again."));
        }

        LogCaptchaValidated(logger, captchaId);
        return Result.Success();
    }

    #region Image Generation

    private static string GenerateCaptchaImage(string captchaText, bool isLightTheme)
    {
        using var surface = SKSurface.Create(new SKImageInfo(CaptchaWidth, CaptchaHeight));
        var canvas = surface.Canvas;

        // Theme-aware colors
        var bgColor1 = isLightTheme ? SKColor.Parse("#f0f2f5") : SKColor.Parse("#1f1f1f");
        var bgColor2 = isLightTheme ? SKColor.Parse("#e6e9ef") : SKColor.Parse("#2d2d2d");
        var noiseColor = isLightTheme ? SKColor.Parse("#d0d0d0") : SKColor.Parse("#404040");
        var textColor = isLightTheme ? SKColors.Black : SKColors.White;

        // Draw gradient background
        using var bgPaint = new SKPaint();
        bgPaint.Shader = SKShader.CreateLinearGradient(
            new(0, 0),
            new(CaptchaWidth, CaptchaHeight),
            [bgColor1, bgColor2],
            SKShaderTileMode.Clamp);

        canvas.DrawRect(0, 0, CaptchaWidth, CaptchaHeight, bgPaint);

        // Draw noise lines and dots
        DrawNoise(canvas, noiseColor);

        // Draw characters with random fonts, sizes, rotations, and offsets
        DrawCharacters(canvas, captchaText, textColor);

        // Encode to base64 PNG
        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 90);
        return $"data:image/png;base64,{Convert.ToBase64String(data.ToArray())}";
    }

    private static void DrawNoise(SKCanvas canvas, SKColor noiseColor)
    {
        using var noisePaint = new SKPaint();
        noisePaint.Color = noiseColor;
        noisePaint.StrokeWidth = 1;
        noisePaint.IsAntialias = true;

        // Random noise lines
        for (var i = 0; i < 8; i++)
        {
            canvas.DrawLine(
                RandomShared.Next(CaptchaWidth),
                RandomShared.Next(CaptchaHeight),
                RandomShared.Next(CaptchaWidth),
                RandomShared.Next(CaptchaHeight),
                noisePaint);
        }

        // Random dots
        for (var i = 0; i < 50; i++)
        {
            canvas.DrawCircle(
                RandomShared.Next(CaptchaWidth),
                RandomShared.Next(CaptchaHeight),
                1,
                noisePaint);
        }
    }

    private static void DrawCharacters(SKCanvas canvas, string captchaText, SKColor textColor)
    {
        float xOffset = 15;
        const float baseY = 40f;

        foreach (var character in captchaText)
        {
            var alpha = (byte)RandomShared.Next(180, 230);
            var charColor = textColor.WithAlpha(alpha);

            var typeface = SKTypeface.FromFamilyName(
                Fonts[RandomShared.Next(Fonts.Length)],
                SKFontStyleWeight.Bold,
                SKFontStyleWidth.Normal,
                SKFontStyleSlant.Upright);

            using var font = new SKFont(typeface, RandomShared.Next(32, 40));
            using var textPaint = new SKPaint();
            textPaint.Color = charColor;
            textPaint.IsAntialias = true;

            var yOffset = baseY + RandomShared.Next(-8, 8);
            float rotation = RandomShared.Next(-15, 15);

            canvas.Save();
            canvas.Translate(xOffset, yOffset);
            canvas.RotateDegrees(rotation);
            canvas.DrawText(character.ToString(), 0, 0, SKTextAlign.Left, font, textPaint);
            canvas.Restore();

            xOffset += font.MeasureText(character.ToString()) + RandomShared.Next(5, 15);
        }
    }

    #endregion

    #region Helpers

    /// <summary>Thread-safe random for noise generation (non-cryptographic).</summary>
    private static Random RandomShared => Random.Shared;

    #endregion

    #region Logging

    [LoggerMessage(Level = LogLevel.Debug, Message = "CAPTCHA not required for {LoginUser} ({FailedAttempts} failed attempts)")]
    private static partial void LogCaptchaNotRequired(ILogger logger, string loginUser, int failedAttempts);

    [LoggerMessage(Level = LogLevel.Information, Message = "CAPTCHA generated for {LoginUser} ({FailedAttempts} failed attempts)")]
    private static partial void LogCaptchaGenerated(ILogger logger, string loginUser, int failedAttempts);

    [LoggerMessage(Level = LogLevel.Warning, Message = "CAPTCHA validation failed for ID {CaptchaId}")]
    private static partial void LogCaptchaFailed(ILogger logger, string captchaId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "CAPTCHA validated successfully for ID {CaptchaId}")]
    private static partial void LogCaptchaValidated(ILogger logger, string captchaId);

    #endregion
}

/// <summary>
/// Internal DTO stored in Redis for CAPTCHA answer lookup.
/// </summary>
internal sealed class CaptchaStoredValue
{
    public required string Value { get; init; }
}
