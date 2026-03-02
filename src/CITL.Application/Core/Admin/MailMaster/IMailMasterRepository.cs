using CITL.Application.Common.Models;

namespace CITL.Application.Core.Admin.MailMaster;

/// <summary>
/// Repository interface for Mail Master database operations.
/// Defined in Application layer; implemented in Infrastructure with Dapper.
/// </summary>
public interface IMailMasterRepository
{
    /// <summary>
    /// Gets all mail configurations, optionally filtered to approved-only.
    /// </summary>
    Task<IReadOnlyList<MailMasterResponse>> GetAllAsync(bool isApproved, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a simplified dropdown list of mail configurations (SNo + FromAddress).
    /// </summary>
    Task<IReadOnlyList<DropDownResponse<int>>> GetDropDownAsync(bool isApproved, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a single mail configuration by serial number.
    /// </summary>
    Task<MailMasterResponse?> GetByIdAsync(int mailSNo, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates a mail configuration by calling <c>citlsp.Mail_Master_Insert</c>.
    /// When <c>request.MailSNo</c> is 0, the SP auto-generates the ID.
    /// </summary>
    Task<SpResult> AddOrUpdateAsync(MailMasterRequest request, int sessionId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a mail configuration by serial number.
    /// Also removes related records from <c>citl_sys.Activity_Mail_Log</c>.
    /// </summary>
    Task<SpResult> DeleteAsync(int mailSNo, CancellationToken cancellationToken);

    /// <summary>
    /// Gets SMTP configuration (including password) for sending emails.
    /// When <paramref name="mailSNo"/> is null, returns the default active configuration.
    /// </summary>
    Task<SmtpConfig?> GetSmtpConfigAsync(int? mailSNo, CancellationToken cancellationToken);
}
