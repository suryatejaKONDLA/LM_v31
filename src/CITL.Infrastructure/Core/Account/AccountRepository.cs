using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Account;

namespace CITL.Infrastructure.Core.Account;

/// <inheritdoc />
internal sealed class AccountRepository(IDbExecutor db) : IAccountRepository
{
    private const string GetProfileSql = """
        SELECT
            lm.Login_ID,
            lm.Login_User,
            lm.Login_Name,
            lm.Login_Branch_Code,
            lm2.Login_Designation,
            lm2.Login_Mobile_No,
            lm2.Login_Email_ID,
            lm2.Login_DOB,
            lm2.Login_Gender,
            lm2.Login_Email_Verified_Flag,
            lsp.MENU_ID,
            lmp.Login_Pic
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
            LoginId = row.Login_ID,
            LoginUser = row.Login_User,
            LoginName = row.Login_Name,
            LoginBranchCode = row.Login_Branch_Code,
            LoginDesignation = row.Login_Designation,
            LoginMobileNo = row.Login_Mobile_No,
            LoginEmailId = row.Login_Email_ID,
            LoginDob = row.Login_DOB,
            LoginGender = row.Login_Gender,
            LoginEmailVerified = row.Login_Email_Verified_Flag,
            MenuId = row.MENU_ID,
            LoginPic = row.Login_Pic is { Length: > 0 }
                ? Convert.ToBase64String(row.Login_Pic)
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
        public int Login_ID { get; init; }
        public string Login_User { get; init; } = string.Empty;
        public string Login_Name { get; init; } = string.Empty;
        public int Login_Branch_Code { get; init; }
        public string Login_Designation { get; init; } = string.Empty;
        public string Login_Mobile_No { get; init; } = string.Empty;
        public string Login_Email_ID { get; init; } = string.Empty;
        public DateTime? Login_DOB { get; init; }
        public string Login_Gender { get; init; } = string.Empty;
        public bool Login_Email_Verified_Flag { get; init; }
        public string? MENU_ID { get; init; }
        public byte[]? Login_Pic { get; init; }
    }
}
