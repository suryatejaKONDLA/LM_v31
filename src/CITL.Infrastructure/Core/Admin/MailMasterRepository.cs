using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.MailMaster;

namespace CITL.Infrastructure.Core.Admin;

/// <inheritdoc />
internal sealed class MailMasterRepository(IDbExecutor db) : IMailMasterRepository
{
    private const string GetAllBaseSql = """
        SELECT
            mm.Mail_SNo, mm.Mail_Branch_Code,
            mm.Mail_From_Address, mm.Mail_Display_Name,
            mm.Mail_Host, mm.Mail_Port, mm.Mail_SSL_Enabled,
            mm.Mail_Max_Recipients, mm.Mail_Retry_Attempts, mm.Mail_Retry_Interval_Minutes,
            mm.Mail_Is_Active, mm.Mail_Is_Default,
            mm.Mail_Created_ID,  cu.Login_Name AS MailCreatedName,  mm.Mail_Created_Date,
            mm.Mail_Modified_ID, mu.Login_Name AS MailModifiedName, mm.Mail_Modified_Date,
            mm.Mail_Approved_ID, au.Login_Name AS MailApprovedName, mm.Mail_Approved_Date
        FROM citl_sys.Mail_Master mm
            INNER JOIN citl.Login_Name cu ON cu.Login_ID = mm.Mail_Created_ID
            LEFT  JOIN citl.Login_Name mu ON mu.Login_ID = mm.Mail_Modified_ID
            LEFT  JOIN citl.Login_Name au ON au.Login_ID = mm.Mail_Approved_ID
        """;

    private const string GetByIdSql = """
        SELECT
            mm.Mail_SNo, mm.Mail_Branch_Code,
            mm.Mail_From_Address, mm.Mail_Display_Name,
            mm.Mail_Host, mm.Mail_Port, mm.Mail_SSL_Enabled,
            mm.Mail_Max_Recipients, mm.Mail_Retry_Attempts, mm.Mail_Retry_Interval_Minutes,
            mm.Mail_Is_Active, mm.Mail_Is_Default,
            mm.Mail_Created_ID,  cu.Login_Name AS MailCreatedName,  mm.Mail_Created_Date,
            mm.Mail_Modified_ID, mu.Login_Name AS MailModifiedName, mm.Mail_Modified_Date,
            mm.Mail_Approved_ID, au.Login_Name AS MailApprovedName, mm.Mail_Approved_Date
        FROM citl_sys.Mail_Master mm
            INNER JOIN citl.Login_Name cu ON cu.Login_ID = mm.Mail_Created_ID
            LEFT  JOIN citl.Login_Name mu ON mu.Login_ID = mm.Mail_Modified_ID
            LEFT  JOIN citl.Login_Name au ON au.Login_ID = mm.Mail_Approved_ID
        WHERE mm.Mail_SNo = @MailSNo
        """;

    private const string DropDownBaseSql = """
        SELECT Mail_SNo AS Col1, Mail_From_Address AS Col2
        FROM citl_sys.Mail_Master
        """;

    private const string SmtpConfigByIdSql = """
        SELECT
            Mail_SNo, Mail_From_Address, Mail_From_Password,
            Mail_Display_Name, Mail_Host, Mail_Port, Mail_SSL_Enabled
        FROM citl_sys.Mail_Master
        WHERE Mail_SNo = @MailSNo
        """;

    private const string SmtpConfigDefaultSql = """
        SELECT TOP 1
            Mail_SNo, Mail_From_Address, Mail_From_Password,
            Mail_Display_Name, Mail_Host, Mail_Port, Mail_SSL_Enabled
        FROM citl_sys.Mail_Master
        WHERE Mail_Is_Default = 1 AND Mail_Is_Active = 1
        """;

    private const string DeleteSql = """
        DELETE FROM citl_sys.Activity_Mail_Log WHERE Mail_SNo = @MailSNo;

        DELETE FROM citl_sys.Mail_Master WHERE Mail_SNo = @MailSNo;

        IF @@ROWCOUNT = 0
            SELECT -1 AS ResultVal, 'error' AS ResultType, 'Mail configuration not found.' AS ResultMessage;
        ELSE
            SELECT 1 AS ResultVal, 'success' AS ResultType, 'Mail configuration deleted successfully.' AS ResultMessage;
        """;

    /// <inheritdoc />
    public async Task<IReadOnlyList<MailMasterResponse>> GetAllAsync(bool isApproved, CancellationToken cancellationToken)
    {
        var sql = isApproved
            ? $"{GetAllBaseSql} WHERE mm.Mail_Approved_ID IS NOT NULL ORDER BY mm.Mail_From_Address"
            : $"{GetAllBaseSql} ORDER BY mm.Mail_From_Address";

        return await db.QueryAsync<MailMasterResponse>(sql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DropDownResponse<int>>> GetDropDownAsync(bool isApproved, CancellationToken cancellationToken)
    {
        var sql = isApproved
            ? $"{DropDownBaseSql} WHERE Mail_Approved_ID IS NOT NULL ORDER BY Mail_From_Address"
            : $"{DropDownBaseSql} ORDER BY Mail_From_Address";

        return await db.QueryAsync<DropDownResponse<int>>(sql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<MailMasterResponse?> GetByIdAsync(int mailSNo, CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<MailMasterResponse>(
            GetByIdSql,
            new { MailSNo = mailSNo },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpResult> AddOrUpdateAsync(
        MailMasterRequest request,
        int sessionId,
        CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["Mail_SNo"] = request.MailSNo,
            ["Mail_Branch_Code"] = request.MailBranchCode,
            ["Mail_From_Address"] = request.MailFromAddress,
            ["Mail_From_Password"] = request.MailFromPassword,
            ["Mail_Display_Name"] = request.MailDisplayName,
            ["Mail_Host"] = request.MailHost,
            ["Mail_Port"] = request.MailPort,
            ["Mail_SSL_Enabled"] = request.MailSslEnabled,
            ["Mail_Max_Recipients"] = request.MailMaxRecipients,
            ["Mail_Retry_Attempts"] = request.MailRetryAttempts,
            ["Mail_Retry_Interval_Minutes"] = request.MailRetryIntervalMinutes,
            ["Mail_Is_Active"] = request.MailIsActive,
            ["Mail_Is_Default"] = request.MailIsDefault,
            ["Session_ID"] = sessionId
        };

        return await db.ExecuteSpAsync("citlsp.Mail_Master_Insert", parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpResult> DeleteAsync(int mailSNo, CancellationToken cancellationToken)
    {
        var result = await db.QuerySingleOrDefaultAsync<SpResult>(
            DeleteSql,
            new { MailSNo = mailSNo },
            cancellationToken).ConfigureAwait(false);

        return result ?? new SpResult { ResultVal = -1, ResultType = "error", ResultMessage = "Mail configuration not found." };
    }

    /// <inheritdoc />
    public async Task<SmtpConfig?> GetSmtpConfigAsync(int? mailSNo, CancellationToken cancellationToken)
    {
        if (mailSNo.HasValue)
        {
            return await db.QuerySingleOrDefaultAsync<SmtpConfig>(
                SmtpConfigByIdSql,
                new { MailSNo = mailSNo.Value },
                cancellationToken).ConfigureAwait(false);
        }

        return await db.QuerySingleOrDefaultAsync<SmtpConfig>(
            SmtpConfigDefaultSql,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
