using System.Text.Json.Serialization;

namespace CITL.Application.Core.Admin.BranchMaster;

/// <summary>
/// Request DTO for creating/updating a branch.
/// </summary>
public sealed class BranchMasterRequest
{
    [JsonPropertyName("BRANCH_Code")]
    public int BranchCode { get; init; }

    [JsonPropertyName("BRANCH_Name")]
    public string BranchName { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_State")]
    public int BranchState { get; init; }

    [JsonPropertyName("BRANCH_Name2")]
    public string BranchName2 { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_Address1")]
    public string? BranchAddress1 { get; init; }

    [JsonPropertyName("BRANCH_Address2")]
    public string? BranchAddress2 { get; init; }

    [JsonPropertyName("BRANCH_Address3")]
    public string? BranchAddress3 { get; init; }

    [JsonPropertyName("BRANCH_City")]
    public string BranchCity { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_PIN")]
    public string BranchPin { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_Contact_Person")]
    public string BranchContactPerson { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_Phone_No1")]
    public string BranchPhoneNo1 { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_Phone_No2")]
    public string? BranchPhoneNo2 { get; init; }

    [JsonPropertyName("BRANCH_Email_ID")]
    public string BranchEmailId { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_GSTIN")]
    public string? BranchGstin { get; init; }

    [JsonPropertyName("BRANCH_PAN_No")]
    public string? BranchPanNo { get; init; }

    [JsonPropertyName("BRANCH_AutoApproval_Enabled")]
    public bool BranchAutoApprovalEnabled { get; init; }

    [JsonPropertyName("BRANCH_Discounts_Enabled")]
    public bool BranchDiscountsEnabled { get; init; }

    [JsonPropertyName("BRANCH_CreditLimits_Enabled")]
    public bool BranchCreditLimitsEnabled { get; init; }

    [JsonPropertyName("BRANCH_Currency_Code")]
    public string BranchCurrencyCode { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_TimeZone_Code")]
    public int BranchTimeZoneCode { get; init; }

    [JsonPropertyName("BRANCH_Order")]
    public int BranchOrder { get; init; }

    [JsonPropertyName("BRANCH_Active_Flag")]
    public bool BranchActiveFlag { get; init; } = true;

    /// <summary>Set by controller from IFormFile.</summary>
    [JsonPropertyName("BRANCH_Logo")]
    public byte[]? BranchLogo { get; set; }
}

/// <summary>
/// Response DTO for branch GET endpoints.
/// </summary>
public sealed class BranchResponse
{
    [JsonPropertyName("BRANCH_Code")]
    public int BranchCode { get; init; }

    [JsonPropertyName("BRANCH_Name")]
    public string BranchName { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_State")]
    public int BranchState { get; init; }

    [JsonPropertyName("BRANCH_Name2")]
    public string BranchName2 { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_Address1")]
    public string? BranchAddress1 { get; init; }

    [JsonPropertyName("BRANCH_Address2")]
    public string? BranchAddress2 { get; init; }

    [JsonPropertyName("BRANCH_Address3")]
    public string? BranchAddress3 { get; init; }

    [JsonPropertyName("BRANCH_City")]
    public string BranchCity { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_PIN")]
    public string BranchPin { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_Contact_Person")]
    public string BranchContactPerson { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_Phone_No1")]
    public string BranchPhoneNo1 { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_Phone_No2")]
    public string? BranchPhoneNo2 { get; init; }

    [JsonPropertyName("BRANCH_Email_ID")]
    public string BranchEmailId { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_GSTIN")]
    public string? BranchGstin { get; init; }

    [JsonPropertyName("BRANCH_PAN_No")]
    public string? BranchPanNo { get; init; }

    [JsonPropertyName("BRANCH_AutoApproval_Enabled")]
    public bool BranchAutoApprovalEnabled { get; init; }

    [JsonPropertyName("BRANCH_Discounts_Enabled")]
    public bool BranchDiscountsEnabled { get; init; }

    [JsonPropertyName("BRANCH_CreditLimits_Enabled")]
    public bool BranchCreditLimitsEnabled { get; init; }

    [JsonPropertyName("BRANCH_Currency_Code")]
    public string BranchCurrencyCode { get; init; } = string.Empty;

    [JsonPropertyName("BRANCH_TimeZone_Code")]
    public int BranchTimeZoneCode { get; init; }

    [JsonPropertyName("BRANCH_Order")]
    public int BranchOrder { get; init; }

    [JsonPropertyName("BRANCH_Active_Flag")]
    public bool BranchActiveFlag { get; init; }

    [JsonPropertyName("BRANCH_Created_ID")]
    public int BranchCreatedId { get; init; }

    [JsonPropertyName("BRANCH_Created_Date")]
    public DateTime BranchCreatedDate { get; init; }

    [JsonPropertyName("BRANCH_Modified_ID")]
    public int? BranchModifiedId { get; init; }

    [JsonPropertyName("BRANCH_Modified_Date")]
    public DateTime? BranchModifiedDate { get; init; }

    [JsonPropertyName("BRANCH_Approved_ID")]
    public int? BranchApprovedId { get; init; }

    [JsonPropertyName("BRANCH_Approved_Date")]
    public DateTime? BranchApprovedDate { get; init; }

    [JsonPropertyName("BRANCH_Created_Name")]
    public string? BranchCreatedName { get; init; }

    [JsonPropertyName("BRANCH_Modified_Name")]
    public string? BranchModifiedName { get; init; }

    [JsonPropertyName("BRANCH_Approved_Name")]
    public string? BranchApprovedName { get; init; }

    [JsonPropertyName("BRANCH_Logo")]
    public byte[]? BranchLogo { get; init; }
}
