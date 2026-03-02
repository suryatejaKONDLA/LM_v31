using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Authentication;

namespace CITL.Infrastructure.Core.Authentication;

/// <summary>
/// Dapper implementation of <see cref="IIdentityVerificationRepository"/>.
/// Handles <c>citl.Login_Token_Store</c> CRUD and user identity verification queries.
/// </summary>
internal sealed class IdentityVerificationRepository(IDbExecutor db) : IIdentityVerificationRepository
{
    private const string VerifyIdentitySql = """
        SELECT
            m.Login_ID    AS LoginId,
            m.Login_Name  AS LoginName,
            m2.Login_Email_ID AS LoginEmailId,
            m2.Login_Email_Verified_Flag AS LoginEmailVerifiedFlag
        FROM citl.Login_Master m
            INNER JOIN citl.Login_Master2 m2 ON m.Login_ID = m2.Login_ID
        WHERE m.Login_User = @LoginUser
          AND m2.Login_Email_ID = @LoginEmailId
          AND m2.Login_Mobile_No = @LoginMobileNo
        """;

    private const string UpsertTokenSql = """
        MERGE citl.Login_Token_Store AS target
        USING (VALUES (@LoginId, @TokenType, @TokenValue, @TokenExpiry))
              AS source (Login_ID, Token_Type, Token_Value, Token_Expiry)
        ON target.Login_ID = source.Login_ID AND target.Token_Type = source.Token_Type
        WHEN MATCHED THEN
            UPDATE SET Token_Value = source.Token_Value, Token_Expiry = source.Token_Expiry
        WHEN NOT MATCHED THEN
            INSERT (Login_ID, Token_Type, Token_Value, Token_Expiry)
            VALUES (source.Login_ID, source.Token_Type, source.Token_Value, source.Token_Expiry);
        """;

    private const string ValidateTokenSql = """
        SELECT Login_ID
        FROM citl.Login_Token_Store
        WHERE Token_Type = @TokenType
          AND Token_Value = @TokenValue
          AND Token_Expiry > GETUTCDATE()
        """;

    private const string DeleteTokenSql = """
        DELETE FROM citl.Login_Token_Store
        WHERE Login_ID = @LoginId AND Token_Type = @TokenType
        """;

    private const string ResetPasswordSql = """
        UPDATE citl.Login_Master2
        SET Login_Password = HASHBYTES('SHA2_512', @NewPassword),
            Login_Last_Password_Change_Date = GETUTCDATE(),
            Login_Modified_Date = GETUTCDATE()
        WHERE Login_ID = @LoginId
        """;

    private const string SetEmailVerifiedSql = """
        UPDATE citl.Login_Master2
        SET Login_Email_Verified_Flag = 1,
            Login_Modified_Date = GETUTCDATE()
        WHERE Login_ID = @LoginId
        """;

    private const string GetLoginNameSql = "SELECT Login_Name FROM citl.Login_Master WHERE Login_ID = @LoginId";

    private const string CleanupExpiredSql = "DELETE FROM citl.Login_Token_Store WHERE Token_Expiry <= GETUTCDATE()";

    /// <inheritdoc />
    public async Task<UserIdentityInfo?> VerifyUserIdentityAsync(
        string loginUser,
        string loginEmailId,
        string loginMobileNo,
        CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<UserIdentityInfo>(
            VerifyIdentitySql,
            new { LoginUser = loginUser, LoginEmailId = loginEmailId, LoginMobileNo = loginMobileNo },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpsertTokenAsync(
        int loginId, byte tokenType, string tokenValue, DateTime expiry, CancellationToken cancellationToken)
    {
        await db.ExecuteAsync(
            UpsertTokenSql,
            new { LoginId = loginId, TokenType = tokenType, TokenValue = tokenValue, TokenExpiry = expiry },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int?> ValidateTokenAsync(byte tokenType, string tokenValue, CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<int?>(
            ValidateTokenSql,
            new { TokenType = tokenType, TokenValue = tokenValue },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task DeleteTokenAsync(int loginId, byte tokenType, CancellationToken cancellationToken)
    {
        await db.ExecuteAsync(
            DeleteTokenSql,
            new { LoginId = loginId, TokenType = tokenType },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ResetPasswordAsync(int loginId, string newPassword, CancellationToken cancellationToken)
    {
        await db.ExecuteAsync(
            ResetPasswordSql,
            new { LoginId = loginId, NewPassword = newPassword },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SetEmailVerifiedAsync(int loginId, CancellationToken cancellationToken)
    {
        await db.ExecuteAsync(
            SetEmailVerifiedSql,
            new { LoginId = loginId },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<string?> GetLoginNameByIdAsync(int loginId, CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<string>(
            GetLoginNameSql,
            new { LoginId = loginId },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
    {
        await db.ExecuteAsync(CleanupExpiredSql, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}
