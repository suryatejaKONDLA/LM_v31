using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Account.Theme;

namespace CITL.Infrastructure.Core.Account;

/// <inheritdoc />
internal sealed class ThemeRepository(IDbExecutor db) : IThemeRepository
{
    private const string GetSql = """
        SELECT Login_ID, Theme_Json
        FROM citl.Login_Theme
        WHERE Login_ID = @LoginId
        """;

    /// <inheritdoc />
    public async Task<ThemeResponse?> GetAsync(
        int loginId,
        CancellationToken cancellationToken)
    {
        return await db.QuerySingleOrDefaultAsync<ThemeResponse>(
            GetSql,
            new { LoginId = loginId },
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<SpResult> SaveAsync(
        int loginId,
        string themeJson,
        CancellationToken cancellationToken)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["Login_ID"] = loginId,
            ["Theme_Json"] = themeJson,
        };

        return await db.ExecuteSpAsync("citlsp.Login_Theme_Set", parameters, cancellationToken)
            .ConfigureAwait(false);
    }
}
