using CITL.Application.Common.Interfaces;
using CITL.Application.Core.Scheduler;

namespace CITL.Infrastructure.Core.Scheduler;

/// <inheritdoc />
internal sealed class SchedulerRepository(IDbExecutor db) : ISchedulerRepository
{
    private const string GetActiveJobsSql = """
        SELECT
            SCH_JobId, SCH_Branch_Code, SCH_JobName, SCH_Description,
            SCH_ReportName, SCH_ReportPath, SCH_Query, SCH_Parameters,
            SCH_MailSubject, SCH_MailBody, SCH_MailTo, SCH_MailCc, SCH_MailBcc,
            SCH_CronExpression, SCH_Timeout_Seconds, SCH_Retry_Attempts, SCH_Retry_Interval_Seconds,
            SCH_Is_Active, SCH_Last_Run_Date, SCH_Next_Run_Date,
            SCH_Last_Run_Status, SCH_Last_Error_Message,
            SCH_Created_ID, SCH_Created_Date, SCH_Modified_ID, SCH_Modified_Date,
            SCH_Approved_ID, SCH_Approved_Date
        FROM citl_sys.Scheduler_Configuration
        WHERE SCH_Is_Active = 1
        """;

    private const string UpdateLastRunSql = """
        UPDATE citl_sys.Scheduler_Configuration
        SET SCH_Last_Run_Date = @LastRunDate,
            SCH_Next_Run_Date = @NextRunDate,
            SCH_Last_Run_Status = @Status,
            SCH_Last_Error_Message = @ErrorMessage
        WHERE SCH_JobId = @JobId
        """;

    /// <inheritdoc />
    public async Task<IReadOnlyList<SchedulerConfigResponse>> GetActiveJobsAsync(
        CancellationToken cancellationToken)
    {
        return await db.QueryAsync<SchedulerConfigResponse>(GetActiveJobsSql, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task UpdateLastRunAsync(
        int jobId,
        DateTime lastRunDate,
        DateTime? nextRunDate,
        string status,
        string? errorMessage,
        CancellationToken cancellationToken)
    {
        await db.ExecuteAsync(
            UpdateLastRunSql,
            new { JobId = jobId, LastRunDate = lastRunDate, NextRunDate = nextRunDate, Status = status, ErrorMessage = errorMessage },
            cancellationToken).ConfigureAwait(false);
    }
}
