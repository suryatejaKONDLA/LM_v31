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
    /// Builds a welcome email with login credentials for a newly created account.
    /// </summary>
    public static string BuildWelcomeEmail(
        string appName,
        bool hasInlineLogo,
        string displayName,
        string loginUser,
        string password,
        string supportEmail,
        string supportPhone)
    {
        var logoHtml = hasInlineLogo
            ? $"""<img src="cid:app-logo" alt="{appName}" style="max-height:48px;max-width:200px;" />"""
            : $"""<span style="font-size:24px;font-weight:700;color:#4F46E5;">{appName}</span>""";

        var supportSection = string.IsNullOrWhiteSpace(supportEmail) && string.IsNullOrWhiteSpace(supportPhone)
            ? string.Empty
            : $"""
               <p style="color:#555555;font-size:14px;line-height:1.6;margin-top:24px;">
                   Need help? Contact support{(!string.IsNullOrWhiteSpace(supportEmail) ? $" at <a href=\"mailto:{supportEmail}\" style=\"color:#4F46E5;\">{supportEmail}</a>" : string.Empty)}{(!string.IsNullOrWhiteSpace(supportPhone) ? $" or call {supportPhone}" : string.Empty)}.
               </p>
               """;

        var year = DateTime.UtcNow.Year.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return WelcomeEmailTemplate
            .Replace("{{AppName}}", appName)
            .Replace("{{AppLogo}}", logoHtml)
            .Replace("{{DisplayName}}", displayName)
            .Replace("{{LoginUser}}", loginUser)
            .Replace("{{Password}}", password)
            .Replace("{{SupportSection}}", supportSection)
            .Replace("{{Year}}", year);
    }

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

    private const string WelcomeEmailTemplate = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="UTF-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>Welcome — {{AppName}}</title>
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
                                    <h2 style="color:#111827;margin:0 0 8px;font-size:22px;">Welcome, {{DisplayName}}!</h2>
                                    <p style="color:#555555;font-size:15px;line-height:1.6;margin:0 0 24px;">
                                        Your account has been created on <strong>{{AppName}}</strong>.
                                        Use the credentials below to log in for the first time.
                                    </p>

                                    <!-- Credential card -->
                                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0"
                                           style="background-color:#f9fafb;border:1px solid #e5e7eb;border-radius:8px;margin-bottom:24px;">
                                        <tr>
                                            <td style="padding:20px 24px;">
                                                <table role="presentation" width="100%" cellspacing="0" cellpadding="0">
                                                    <tr>
                                                        <td style="padding:8px 0;border-bottom:1px solid #e5e7eb;">
                                                            <span style="color:#6b7280;font-size:12px;font-weight:600;text-transform:uppercase;letter-spacing:0.05em;">Username</span><br />
                                                            <span style="color:#111827;font-size:16px;font-weight:700;font-family:monospace;">{{LoginUser}}</span>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td style="padding:8px 0;">
                                                            <span style="color:#6b7280;font-size:12px;font-weight:600;text-transform:uppercase;letter-spacing:0.05em;">Password</span><br />
                                                            <span style="color:#111827;font-size:16px;font-weight:700;font-family:monospace;">{{Password}}</span>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </table>

                                    <!-- Security notice -->
                                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0"
                                           style="background-color:#fffbeb;border:1px solid #fcd34d;border-radius:6px;margin-bottom:24px;">
                                        <tr>
                                            <td style="padding:12px 16px;">
                                                <p style="color:#92400e;font-size:13px;margin:0;line-height:1.5;">
                                                    <strong>Security reminder:</strong> Please change your password immediately after your first login.
                                                    Do not share your credentials with anyone.
                                                </p>
                                            </td>
                                        </tr>
                                    </table>

                                    {{SupportSection}}
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
