using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Authentication;

namespace CITL.Infrastructure.Core.Authentication;

/// <summary>
/// Dapper implementation of <see cref="IAuthenticationRepository"/>.
/// Calls CITL stored procedures for login, refresh tokens, and blacklisting.
/// </summary>
/// <param name="db">The tenant-scoped database executor.</param>
internal sealed class AuthenticationRepository(IDbExecutor db) : IAuthenticationRepository
{
    private const string GetRolesSql = """
        SELECT r.ROLE_Name AS RoleName
        FROM citl.Login_ROLE_Mapping lrm
            INNER JOIN citl.ROLE_Master r ON r.ROLE_ID = lrm.ROLE_ID
        WHERE lrm.Login_ID = @LoginId
        """;

    private const string GetBranchesSql = """
        SELECT BRANCH_Code, BRANCH_Name
        FROM citl.Login_BRANCH_Mapping_View1
        WHERE Login_ID = @LoginId
        ORDER BY BRANCH_Code
        """;

    private const string GetUserProfileSql = """
        SELECT
            lm.Login_ID AS LoginId,
            lm.Login_User AS LoginUser,
            lm.Login_Name AS LoginName
        FROM citl.Login_Master lm
        WHERE lm.Login_User = @LoginUser
        """;

    private const string ValidateRefreshTokenSql = """
        SELECT CASE
            WHEN EXISTS (
                SELECT 1 FROM citl_sys.Refresh_Token_Store
                WHERE Login_User = @LoginUser
                  AND Refresh_Token_Hash = @RefreshTokenHash
                  AND Refresh_Token_Expiry_Date > GETUTCDATE()
            ) THEN 1
            ELSE 0
        END
        """;

    private const string GetFailedAttemptCountSql = """
        SELECT ISNULL(Login_Attempt_Count, 0)
        FROM citl_sys.Login_Failed_Attempts
        WHERE Login_User = @LoginUser
          AND DATEDIFF(MINUTE, Login_LastAttempt_DateTime, GETUTCDATE()) < 30
        """;

    /// <inheritdoc />
    public async Task<SpResult> LoginCheckAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        // citlsp.Login_Check uses output params (@ResultVal, @ResultType, @ResultMessage)
        var parameters = new Dictionary<string, object?>
        {
            ["Login_User"] = request.LoginUser,
            ["Login_Password"] = request.LoginPassword,
            ["Login_Latitude"] = request.LoginLatitude,
            ["Login_Longitude"] = request.LoginLongitude,
            ["Login_Accuracy"] = request.LoginAccuracy,
            ["Login_IP"] = request.LoginIp,
            ["Login_Device"] = request.LoginDevice
        };

        return await db.ExecuteSpAsync("citlsp.Login_Check", parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<UserProfile?> GetUserProfileAsync(string loginUser, CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<UserProfile>(
            GetUserProfileSql,
            new { LoginUser = loginUser },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetUserRolesAsync(int loginId, CancellationToken cancellationToken)
    {
        var roles = await db.QueryAsync<UserRole>(
            GetRolesSql,
            new { LoginId = loginId },
            cancellationToken).ConfigureAwait(false);

        return [.. roles.Select(r => r.RoleName)];
    }

    /// <inheritdoc />
    public async Task<SpResult> StoreRefreshTokenAsync(
        string loginUser,
        byte[] refreshTokenHash,
        DateTime expiryDate,
        CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["Login_User"] = loginUser,
            ["Refresh_Token_Hash"] = refreshTokenHash,
            ["Refresh_Token_Expiry_Date"] = expiryDate
        };

        return await db.ExecuteSpAsync("citlsp.Refresh_Token", parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpResult> BlacklistTokenAsync(
        byte[] tokenHash,
        DateTime createdDate,
        CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["Login_Token"] = tokenHash,
            ["Login_Token_Created_Date"] = createdDate
        };

        return await db.ExecuteSpAsync("citlsp.BlackList_Token", parameters, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ValidateRefreshTokenAsync(
        string loginUser,
        byte[] refreshTokenHash,
        CancellationToken cancellationToken)
    {
        var result = await db.QuerySingleOrDefaultAsync<int>(
            ValidateRefreshTokenSql,
            new { LoginUser = loginUser, RefreshTokenHash = refreshTokenHash },
            cancellationToken).ConfigureAwait(false);

        return result is 1;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<BranchInfo>> GetUserBranchesAsync(int loginId, CancellationToken cancellationToken)
    {
        return await db.QueryAsync<BranchInfo>(
            GetBranchesSql,
            new { LoginId = loginId },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int> GetFailedAttemptCountAsync(string loginUser, CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<int>(
            GetFailedAttemptCountSql,
            new { LoginUser = loginUser },
            cancellationToken).ConfigureAwait(false);
    }
}
