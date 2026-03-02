using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Account;

namespace CITL.Infrastructure.Core.Account;

/// <inheritdoc />
internal sealed class AccountRepository(IDbExecutor db) : IAccountRepository
{
    private const string GetProfileSql = """
        SELECT
            lm.Login_ID       AS LoginId,
            lm.Login_User     AS LoginUser,
            lm.Login_Name     AS LoginName,
            lm.Login_Branch_Code AS LoginBranchCode,
            lm2.Login_Designation   AS LoginDesignation,
            lm2.Login_Mobile_No     AS LoginMobileNo,
            lm2.Login_Email_ID      AS LoginEmailId,
            lm2.Login_DOB           AS LoginDob,
            lm2.Login_Gender        AS LoginGender,
            lm2.Login_Email_Verified_Flag AS LoginEmailVerified,
            lsp.MENU_ID             AS MenuId,
            lmp.Login_Pic           AS LoginPicBytes
        FROM citl.Login_Master lm
            LEFT JOIN citl.Login_Master2 lm2 ON lm2.Login_ID = lm.Login_ID
            LEFT JOIN citl.Login_Master_Pic lmp ON lmp.Login_ID = lm.Login_ID
            LEFT JOIN citl.Login_Startup_Page lsp ON lsp.Login_ID = lm.Login_ID
        WHERE lm.Login_ID = @LoginId
        """;

    private const string GetBranchCodeSql = """
        SELECT Login_Branch_Code
        FROM citl.Login_Master
        WHERE Login_ID = @LoginId
        """;

    /// <inheritdoc />
    public async Task<ProfileResponse?> GetProfileAsync(
        int loginId,
        CancellationToken cancellationToken)
    {
        var row = await db.QuerySingleOrDefaultAsync<ProfileQueryRow>(
            GetProfileSql,
            new { LoginId = loginId },
            cancellationToken).ConfigureAwait(false);

        if (row is null)
        {
            return null;
        }

        return new()
        {
            LoginId = row.LoginId,
            LoginUser = row.LoginUser,
            LoginName = row.LoginName,
            LoginBranchCode = row.LoginBranchCode,
            LoginDesignation = row.LoginDesignation,
            LoginMobileNo = row.LoginMobileNo,
            LoginEmailId = row.LoginEmailId,
            LoginDob = row.LoginDob,
            LoginGender = row.LoginGender,
            LoginEmailVerified = row.LoginEmailVerified,
            MenuId = row.MenuId,
            LoginPic = row.LoginPicBytes is { Length: > 0 }
                ? Convert.ToBase64String(row.LoginPicBytes)
                : null,
        };
    }

    /// <inheritdoc />
    public async Task<SpResult> ChangePasswordAsync(
        int loginId,
        string newPassword,
        string oldPassword,
        CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["Login_ID"] = loginId,
            ["Login_Password"] = newPassword,
            ["Login_Password_Old"] = oldPassword,
        };

        return await db.ExecuteSpAsync("citlsp.Password_Reset", parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpResult> UpdateProfileAsync(
        UpdateProfileRequest request,
        int loginId,
        CancellationToken cancellationToken)
    {
        // Read the user's branch code for SysDate calculation
        var branchCode = await db.QuerySingleOrDefaultAsync<int?>(
            GetBranchCodeSql,
            new { LoginId = loginId },
            cancellationToken).ConfigureAwait(false) ?? 0;

        byte[]? picBytes = null;
        if (!string.IsNullOrEmpty(request.LoginPic))
        {
            picBytes = Convert.FromBase64String(request.LoginPic);
        }

        var parameters = new Dictionary<string, object?>
        {
            ["Login_ID"] = loginId,
            ["Login_Name"] = request.LoginName,
            ["Login_Mobile_No"] = request.LoginMobileNo,
            ["Login_Email_ID"] = request.LoginEmailId,
            ["Login_DOB"] = request.LoginDob,
            ["Login_Pic"] = picBytes,
            ["Menu_ID"] = request.MenuId,
            ["Session_ID"] = loginId,
            ["BRANCH_Code"] = branchCode,
        };

        return await db.ExecuteSpAsync("citlsp.Login_Profile_Update", parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Internal row type for the profile query — maps the VARBINARY pic to <c>byte[]</c>
    /// before converting to base64 in <see cref="GetProfileAsync"/>.
    /// </summary>
    private sealed class ProfileQueryRow
    {
        public int LoginId { get; init; }
        public string LoginUser { get; init; } = string.Empty;
        public string LoginName { get; init; } = string.Empty;
        public int LoginBranchCode { get; init; }
        public string LoginDesignation { get; init; } = string.Empty;
        public string LoginMobileNo { get; init; } = string.Empty;
        public string LoginEmailId { get; init; } = string.Empty;
        public DateTime? LoginDob { get; init; }
        public string LoginGender { get; init; } = string.Empty;
        public bool LoginEmailVerified { get; init; }
        public string? MenuId { get; init; }
        public byte[]? LoginPicBytes { get; init; }
    }
}
