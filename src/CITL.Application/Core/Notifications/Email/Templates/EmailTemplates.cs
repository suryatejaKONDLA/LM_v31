namespace CITL.Application.Core.Notifications.Email.Templates;

/// <summary>
/// Provides HTML email templates with placeholders for dynamic content.
/// Placeholders: <c>{{AppName}}</c>, <c>{{AppLogo}}</c>, <c>{{UserName}}</c>,
/// <c>{{ActionUrl}}</c>, <c>{{ExpiryHours}}</c>, <c>{{Year}}</c>.
/// </summary>
public static class EmailTemplates
{
    /// <summary>
    /// Returns the password reset email HTML.
    /// Placeholders: <c>{{AppName}}</c>, <c>{{AppLogo}}</c>, <c>{{UserName}}</c>,
    /// <c>{{ActionUrl}}</c>, <c>{{ExpiryHours}}</c>, <c>{{Year}}</c>.
    /// </summary>
    public static string PasswordReset => LayoutTemplate
        .Replace("{{Title}}", "Reset Your Password")
        .Replace("{{Body}}", """
            <h2 style="color:#333333;margin:0 0 16px;">Reset Your Password</h2>
            <p style="color:#555555;font-size:15px;line-height:1.6;">
                Hi <strong>{{UserName}}</strong>,
            </p>
            <p style="color:#555555;font-size:15px;line-height:1.6;">
                We received a request to reset your password. Click the button below to set a new password.
                This link will expire in <strong>{{ExpiryHours}} hour(s)</strong>.
            </p>
            <div style="text-align:center;margin:32px 0;">
                <a href="{{ActionUrl}}" style="display:inline-block;padding:14px 32px;background-color:#4F46E5;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;font-size:15px;">
                    Reset Password
                </a>
            </div>
            <p style="color:#888888;font-size:13px;line-height:1.6;">
                If you didn't request a password reset, you can safely ignore this email. Your password will remain unchanged.
            </p>
            """);

    /// <summary>
    /// Returns the email verification HTML.
    /// Placeholders: <c>{{AppName}}</c>, <c>{{AppLogo}}</c>, <c>{{UserName}}</c>,
    /// <c>{{ActionUrl}}</c>, <c>{{ExpiryHours}}</c>, <c>{{Year}}</c>.
    /// </summary>
    public static string EmailVerification => LayoutTemplate
        .Replace("{{Title}}", "Verify Your Email")
        .Replace("{{Body}}", """
            <h2 style="color:#333333;margin:0 0 16px;">Verify Your Email Address</h2>
            <p style="color:#555555;font-size:15px;line-height:1.6;">
                Hi <strong>{{UserName}}</strong>,
            </p>
            <p style="color:#555555;font-size:15px;line-height:1.6;">
                Thank you for registering. Please verify your email address by clicking the button below.
                This link will expire in <strong>{{ExpiryHours}} hour(s)</strong>.
            </p>
            <div style="text-align:center;margin:32px 0;">
                <a href="{{ActionUrl}}" style="display:inline-block;padding:14px 32px;background-color:#059669;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;font-size:15px;">
                    Verify Email
                </a>
            </div>
            <p style="color:#888888;font-size:13px;line-height:1.6;">
                If you didn't create an account, you can safely ignore this email.
            </p>
            """);

    /// <summary>
    /// Renders a template by replacing all placeholders with the provided values.
    /// </summary>
    public static string Render(
        string template,
        string appName,
        bool hasInlineLogo,
        string userName,
        string actionUrl,
        int expiryHours)
    {
        var logoHtml = hasInlineLogo
            ? $"""<img src="cid:app-logo" alt="{appName}" style="max-height:48px;max-width:200px;" />"""
            : $"""<span style="font-size:24px;font-weight:700;color:#4F46E5;">{appName}</span>""";

        return template
            .Replace("{{AppName}}", appName)
            .Replace("{{AppLogo}}", logoHtml)
            .Replace("{{UserName}}", userName)
            .Replace("{{ActionUrl}}", actionUrl)
            .Replace("{{ExpiryHours}}", expiryHours.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("{{Year}}", DateTime.UtcNow.Year.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    private const string LayoutTemplate = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>{{Title}} — {{AppName}}</title>
        </head>
        <body style="margin:0;padding:0;background-color:#f4f4f5;font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif;">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:#f4f4f5;">
                <tr>
                    <td align="center" style="padding:40px 16px;">
                        <table role="presentation" width="560" cellspacing="0" cellpadding="0" style="background-color:#ffffff;border-radius:8px;box-shadow:0 1px 3px rgba(0,0,0,0.1);">
                            <!-- Header -->
                            <tr>
                                <td style="padding:24px 32px;text-align:center;border-bottom:1px solid #e5e7eb;">
                                    {{AppLogo}}
                                </td>
                            </tr>
                            <!-- Body -->
                            <tr>
                                <td style="padding:32px;">
                                    {{Body}}
                                </td>
                            </tr>
                            <!-- Footer -->
                            <tr>
                                <td style="padding:16px 32px;text-align:center;border-top:1px solid #e5e7eb;">
                                    <p style="color:#aaaaaa;font-size:12px;margin:0;">
                                        &copy; {{Year}} {{AppName}}. All rights reserved.
                                    </p>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>
        """;
}
