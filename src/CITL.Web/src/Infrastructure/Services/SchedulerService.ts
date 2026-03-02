import { type ApiResponseWithData } from "@/Shared/Index";
import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";

// ── Response types matching SchedulerDtos.cs ─────────────────────────────────

export interface JobStatusResponse
{
    SCH_JobId: number;
    SCH_JobName: string;
    TenantId: string;
    CronExpression: string;
    State: string;
    NextFireTimeUtc: string | null;
    PreviousFireTimeUtc: string | null;
    LastRunStatus: string | null;
    LastErrorMessage: string | null;
}

export interface TenantSchedulerStatusResponse
{
    TenantId: string;
    TotalJobs: number;
    ActiveJobs: number;
    PausedJobs: number;
    ErrorJobs: number;
    Jobs: JobStatusResponse[];
}

// ── Service ──────────────────────────────────────────────────────────────────

const base = ApiRoutes.Scheduler;

export const SchedulerService =
{
    getStatus(signal?: AbortSignal): Promise<ApiResponseWithData<TenantSchedulerStatusResponse>>
    {
        return apiClient.get<TenantSchedulerStatusResponse>(`${base}/Status`, undefined, signal);
    },

    pauseJob(jobId: number): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(`${base}/Job/${String(jobId)}/Pause`, null);
    },

    resumeJob(jobId: number): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(`${base}/Job/${String(jobId)}/Resume`, null);
    },

    triggerJob(jobId: number): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(`${base}/Job/${String(jobId)}/Trigger`, null);
    },

    stopJob(jobId: number): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(`${base}/Job/${String(jobId)}/Stop`, null);
    },

    pauseAll(): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(`${base}/PauseAll`, null);
    },

    resumeAll(): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(`${base}/ResumeAll`, null);
    },

    reload(): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(`${base}/Reload`, null);
    },
} as const;
