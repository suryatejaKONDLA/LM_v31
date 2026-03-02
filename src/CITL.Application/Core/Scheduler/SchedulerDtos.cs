using System.Text.Json.Serialization;

namespace CITL.Application.Core.Scheduler;

/// <summary>
/// Represents a row from the <c>citl_sys.Scheduler_Configuration</c> table.
/// </summary>
public sealed class SchedulerConfigResponse
{
    [JsonPropertyName("SCH_JobId")]
    public int SchJobId { get; init; }

    [JsonPropertyName("SCH_Branch_Code")]
    public int SchBranchCode { get; init; }

    [JsonPropertyName("SCH_JobName")]
    public string SchJobName { get; init; } = string.Empty;

    [JsonPropertyName("SCH_Description")]
    public string? SchDescription { get; init; }

    [JsonPropertyName("SCH_ReportName")]
    public string? SchReportName { get; init; }

    [JsonPropertyName("SCH_ReportPath")]
    public string? SchReportPath { get; init; }

    [JsonPropertyName("SCH_Query")]
    public string? SchQuery { get; init; }

    [JsonPropertyName("SCH_Parameters")]
    public string? SchParameters { get; init; }

    [JsonPropertyName("SCH_MailSubject")]
    public string SchMailSubject { get; init; } = string.Empty;

    [JsonPropertyName("SCH_MailBody")]
    public string SchMailBody { get; init; } = string.Empty;

    [JsonPropertyName("SCH_MailTo")]
    public string SchMailTo { get; init; } = string.Empty;

    [JsonPropertyName("SCH_MailCc")]
    public string? SchMailCc { get; init; }

    [JsonPropertyName("SCH_MailBcc")]
    public string? SchMailBcc { get; init; }

    [JsonPropertyName("SCH_CronExpression")]
    public string SchCronExpression { get; init; } = string.Empty;

    [JsonPropertyName("SCH_Timeout_Seconds")]
    public int SchTimeoutSeconds { get; init; }

    [JsonPropertyName("SCH_Retry_Attempts")]
    public int SchRetryAttempts { get; init; }

    [JsonPropertyName("SCH_Retry_Interval_Seconds")]
    public int SchRetryIntervalSeconds { get; init; }

    [JsonPropertyName("SCH_Is_Active")]
    public bool SchIsActive { get; init; }

    [JsonPropertyName("SCH_Last_Run_Date")]
    public DateTime? SchLastRunDate { get; init; }

    [JsonPropertyName("SCH_Next_Run_Date")]
    public DateTime? SchNextRunDate { get; init; }

    [JsonPropertyName("SCH_Last_Run_Status")]
    public string? SchLastRunStatus { get; init; }

    [JsonPropertyName("SCH_Last_Error_Message")]
    public string? SchLastErrorMessage { get; init; }

    [JsonPropertyName("SCH_Created_ID")]
    public int SchCreatedId { get; init; }

    [JsonPropertyName("SCH_Created_Date")]
    public DateTime SchCreatedDate { get; init; }

    [JsonPropertyName("SCH_Modified_ID")]
    public int? SchModifiedId { get; init; }

    [JsonPropertyName("SCH_Modified_Date")]
    public DateTime? SchModifiedDate { get; init; }

    [JsonPropertyName("SCH_Approved_ID")]
    public int? SchApprovedId { get; init; }

    [JsonPropertyName("SCH_Approved_Date")]
    public DateTime? SchApprovedDate { get; init; }
}

/// <summary>
/// Runtime status of a single scheduled job in the Quartz scheduler.
/// </summary>
public sealed class JobStatusResponse
{
    [JsonPropertyName("SCH_JobId")]
    public int SchJobId { get; init; }

    [JsonPropertyName("SCH_JobName")]
    public string SchJobName { get; init; } = string.Empty;

    [JsonPropertyName("TenantId")]
    public string TenantId { get; init; } = string.Empty;

    [JsonPropertyName("CronExpression")]
    public string CronExpression { get; init; } = string.Empty;

    [JsonPropertyName("State")]
    public string State { get; init; } = string.Empty;

    [JsonPropertyName("NextFireTimeUtc")]
    public DateTimeOffset? NextFireTimeUtc { get; init; }

    [JsonPropertyName("PreviousFireTimeUtc")]
    public DateTimeOffset? PreviousFireTimeUtc { get; init; }

    [JsonPropertyName("LastRunStatus")]
    public string? LastRunStatus { get; init; }

    [JsonPropertyName("LastErrorMessage")]
    public string? LastErrorMessage { get; init; }
}

/// <summary>
/// Aggregated scheduler status for a single tenant.
/// </summary>
public sealed class TenantSchedulerStatusResponse
{
    [JsonPropertyName("TenantId")]
    public string TenantId { get; init; } = string.Empty;

    [JsonPropertyName("TotalJobs")]
    public int TotalJobs { get; init; }

    [JsonPropertyName("ActiveJobs")]
    public int ActiveJobs { get; init; }

    [JsonPropertyName("PausedJobs")]
    public int PausedJobs { get; init; }

    [JsonPropertyName("ErrorJobs")]
    public int ErrorJobs { get; init; }

    [JsonPropertyName("Jobs")]
    public IReadOnlyList<JobStatusResponse> Jobs { get; init; } = [];
}
