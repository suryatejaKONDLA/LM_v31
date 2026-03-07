using System.Text.Json.Serialization;

namespace CITL.Application.Core.Admin.Mappings.RoleMenuMapping;

public sealed record RoleMenuMappingRequest(int RoleId, IList<string> MenuIds);

public sealed class RoleMenuMappingResponse
{
    [JsonPropertyName("ROLE_ID")]
    public int RoleId { get; init; }

    [JsonPropertyName("MENU_ID")]
    public string MenuId { get; init; } = string.Empty;

    [JsonPropertyName("ROLE_MENU_Created_ID")]
    public int RoleMenuCreatedId { get; init; }

    [JsonPropertyName("ROLE_MENU_Created_Date")]
    public DateTime RoleMenuCreatedDate { get; init; }

    [JsonPropertyName("ROLE_MENU_Modified_ID")]
    public int? RoleMenuModifiedId { get; init; }

    [JsonPropertyName("ROLE_MENU_Modified_Date")]
    public DateTime? RoleMenuModifiedDate { get; init; }

    [JsonPropertyName("ROLE_MENU_Approved_ID")]
    public int? RoleMenuApprovedId { get; init; }

    [JsonPropertyName("ROLE_MENU_Approved_Date")]
    public DateTime? RoleMenuApprovedDate { get; init; }

    [JsonPropertyName("CreatedByName")]
    public string? CreatedByName { get; init; }

    [JsonPropertyName("ModifiedByName")]
    public string? ModifiedByName { get; init; }

    [JsonPropertyName("ApprovedByName")]
    public string? ApprovedByName { get; init; }
}
