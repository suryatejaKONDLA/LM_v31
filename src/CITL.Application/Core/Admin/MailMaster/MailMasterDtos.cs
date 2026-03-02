using System.Text.Json.Serialization;

namespace CITL.Application.Core.Admin.MailMaster;

/// <summary>
/// Request DTO for creating or updating a mail configuration.
/// When <see cref="MailSNo"/> is 0, the SP auto-generates the ID (insert mode).
/// </summary>
public sealed class MailMasterRequest
{
    [JsonPropertyName("Mail_SNo")]
    public int MailSNo { get; init; }

    [JsonPropertyName("Mail_Branch_Code")]
    public int MailBranchCode { get; init; }

    [JsonPropertyName("Mail_From_Address")]
    public required string MailFromAddress { get; init; }

    [JsonPropertyName("Mail_From_Password")]
    public required string MailFromPassword { get; init; }

    [JsonPropertyName("Mail_Display_Name")]
    public required string MailDisplayName { get; init; }

    [JsonPropertyName("Mail_Host")]
    public required string MailHost { get; init; }

    [JsonPropertyName("Mail_Port")]
    public int MailPort { get; init; }

    [JsonPropertyName("Mail_SSL_Enabled")]
    public bool MailSslEnabled { get; init; }

    [JsonPropertyName("Mail_Max_Recipients")]
    public int MailMaxRecipients { get; init; }

    [JsonPropertyName("Mail_Retry_Attempts")]
    public int MailRetryAttempts { get; init; }

    [JsonPropertyName("Mail_Retry_Interval_Minutes")]
    public int MailRetryIntervalMinutes { get; init; }

    [JsonPropertyName("Mail_Is_Active")]
    public bool MailIsActive { get; init; }

    [JsonPropertyName("Mail_Is_Default")]
    public bool MailIsDefault { get; init; }
}

/// <summary>
/// Response DTO for a single mail configuration with audit trail.
/// Password is never returned to the client.
/// </summary>
public sealed class MailMasterResponse
{
    [JsonPropertyName("Mail_SNo")]
    public int MailSNo { get; init; }

    [JsonPropertyName("Mail_Branch_Code")]
    public int MailBranchCode { get; init; }

    [JsonPropertyName("Mail_From_Address")]
    public string MailFromAddress { get; init; } = string.Empty;

    [JsonPropertyName("Mail_Display_Name")]
    public string MailDisplayName { get; init; } = string.Empty;

    [JsonPropertyName("Mail_Host")]
    public string MailHost { get; init; } = string.Empty;

    [JsonPropertyName("Mail_Port")]
    public int MailPort { get; init; }

    [JsonPropertyName("Mail_SSL_Enabled")]
    public bool MailSslEnabled { get; init; }

    [JsonPropertyName("Mail_Max_Recipients")]
    public int MailMaxRecipients { get; init; }

    [JsonPropertyName("Mail_Retry_Attempts")]
    public int MailRetryAttempts { get; init; }

    [JsonPropertyName("Mail_Retry_Interval_Minutes")]
    public int MailRetryIntervalMinutes { get; init; }

    [JsonPropertyName("Mail_Is_Active")]
    public bool MailIsActive { get; init; }

    [JsonPropertyName("Mail_Is_Default")]
    public bool MailIsDefault { get; init; }

    [JsonPropertyName("Mail_Created_ID")]
    public int MailCreatedId { get; init; }

    [JsonPropertyName("Mail_Created_Name")]
    public string? MailCreatedName { get; init; }

    [JsonPropertyName("Mail_Created_Date")]
    public DateTime? MailCreatedDate { get; init; }

    [JsonPropertyName("Mail_Modified_ID")]
    public int? MailModifiedId { get; init; }

    [JsonPropertyName("Mail_Modified_Name")]
    public string? MailModifiedName { get; init; }

    [JsonPropertyName("Mail_Modified_Date")]
    public DateTime? MailModifiedDate { get; init; }

    [JsonPropertyName("Mail_Approved_ID")]
    public int? MailApprovedId { get; init; }

    [JsonPropertyName("Mail_Approved_Name")]
    public string? MailApprovedName { get; init; }

    [JsonPropertyName("Mail_Approved_Date")]
    public DateTime? MailApprovedDate { get; init; }
}

/// <summary>
/// Internal SMTP configuration used by email services. Includes password.
/// Never exposed via API responses.
/// </summary>
public sealed class SmtpConfig
{
    public int MailSNo { get; init; }
    public string MailFromAddress { get; init; } = string.Empty;
    public string MailFromPassword { get; init; } = string.Empty;
    public string MailDisplayName { get; init; } = string.Empty;
    public string MailHost { get; init; } = string.Empty;
    public int MailPort { get; init; }
    public bool MailSslEnabled { get; init; }
}
