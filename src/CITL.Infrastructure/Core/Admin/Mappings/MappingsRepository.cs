using System.Data;
using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.Mappings.Mapping;
using Dapper;

namespace CITL.Infrastructure.Core.Admin.Mappings;

internal sealed class MappingsRepository(IDbExecutor db) : IMappingsRepository
{
    private const string LoginDropDownSql = """
        SELECT Login_ID AS Col1, Login_Name AS Col2
        FROM citl.Login_Master
        ORDER BY Login_Name;
        """;

    public async Task<IReadOnlyList<MappingsResponse>> GetByQueryStringAsync(
        string queryString,
        string anchorId,
        int swapFlag,
        CancellationToken cancellationToken)
    {
        var sql = queryString switch
        {
            "010703" => swapFlag == 0
                ? """
                  SELECT Login_ID AS Left_Column, ROLE_ID AS Right_Column
                  FROM citl.Login_ROLE_Mapping
                  WHERE Login_ID = @AnchorId
                  ORDER BY ROLE_ID;
                  """
                : """
                  SELECT ROLE_ID AS Left_Column, Login_ID AS Right_Column
                  FROM citl.Login_ROLE_Mapping
                  WHERE ROLE_ID = @AnchorId
                  ORDER BY Login_ID;
                  """,
            _ => throw new ArgumentException($"Unsupported QueryString: {queryString}", nameof(queryString))
        };

        return await db.QueryAsync<MappingsResponse>(sql, new { AnchorId = anchorId }, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DropDownResponse<int>>> GetLoginDropDownAsync(CancellationToken cancellationToken)
    {
        return await db.QueryAsync<DropDownResponse<int>>(LoginDropDownSql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<SpResult> InsertAsync(MappingsRequest request, int sessionId, CancellationToken cancellationToken)
    {
        return db.ExecuteSpAsync("citlsp.Mapping_Insert", new Dictionary<string, object?>
        {
            { "QueryString", request.QueryString },
            { "Swap_Flag", request.SwapFlag },
            { "Mapping_TBType1", CreateTableValuedParameter(request).AsTableValuedParameter("citltypes.Mapping_TBType1") },
            { "Session_ID", sessionId }
        }, cancellationToken: cancellationToken);
    }

    private static DataTable CreateTableValuedParameter(MappingsRequest request)
    {
        var dt = new DataTable();
        dt.Columns.Add("Left_Column", typeof(string));
        dt.Columns.Add("Right_Column", typeof(string));

        if (request.MappingIds.Count > 0)
        {
            foreach (var id in request.MappingIds)
            {
                dt.Rows.Add(request.AnchorId, id);
            }
        }
        else
        {
            dt.Rows.Add(request.AnchorId, DBNull.Value);
        }

        return dt;
    }
}
