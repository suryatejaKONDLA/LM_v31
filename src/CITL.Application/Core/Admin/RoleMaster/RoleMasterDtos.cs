using System.Text.Json.Serialization;

namespace CITL.Application.Core.Admin.RoleMaster;

/// <summary>
/// Request DTO for creating or updating a role.
/// When <see cref="RoleId"/> is 0, the SP auto-generates the ID (insert mode).
/// </summary>
public sealed class RoleMasterRequest
{
    [JsonPropertyName("ROLE_ID")]
    public int RoleId { get; init; }

    [JsonPropertyName("ROLE_Name")]
    public required string RoleName { get; init; }

    [JsonPropertyName("BRANCH_Code")]
    public int BranchCode { get; init; }
}

/// <summary>
/// Response DTO for a single role with audit trail.
/// </summary>
/// <remarks>
/// Dapper maps SQL underscore columns via <c>DefaultTypeMap.MatchNamesWithUnderscores</c>.
/// <c>[JsonPropertyName]</c> ensures JSON output matches DB column names.
/// </remarks>
public sealed class RoleResponse
{
    [JsonPropertyName("ROLE_ID")]
    public int RoleId { get; init; }

    [JsonPropertyName("ROLE_Name")]
    public string RoleName { get; init; } = string.Empty;

    [JsonPropertyName("ROLE_Branch_Code")]
    public int RoleBranchCode { get; init; }

    [JsonPropertyName("ROLE_Created_ID")]
    public int RoleCreatedId { get; init; }

    [JsonPropertyName("ROLE_Created_Name")]
    public string? RoleCreatedName { get; init; }

    [JsonPropertyName("ROLE_Created_Date")]
    public DateTime RoleCreatedDate { get; init; }

    [JsonPropertyName("ROLE_Modified_ID")]
    public int? RoleModifiedId { get; init; }

    [JsonPropertyName("ROLE_Modified_Name")]
    public string? RoleModifiedName { get; init; }

    [JsonPropertyName("ROLE_Modified_Date")]
    public DateTime? RoleModifiedDate { get; init; }

    [JsonPropertyName("ROLE_Approved_ID")]
    public int? RoleApprovedId { get; init; }

    [JsonPropertyName("ROLE_Approved_Name")]
    public string? RoleApprovedName { get; init; }

    [JsonPropertyName("ROLE_Approved_Date")]
    public DateTime? RoleApprovedDate { get; init; }
}
