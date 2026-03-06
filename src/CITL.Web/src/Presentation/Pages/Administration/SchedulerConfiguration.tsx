import { useState, useEffect, useCallback } from "react";
import { Card, Row, Col, Table, Tag, Button, Space, Typography, Tooltip, Popconfirm, theme, App } from "antd";
import {
    ReloadOutlined,
    PauseCircleOutlined,
    PlayCircleOutlined,
    ThunderboltOutlined,
    StopOutlined,
    ClockCircleOutlined,
    WarningOutlined,
} from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import { SchedulerService } from "@/Infrastructure/Index";
import type { TenantSchedulerStatusResponse, JobStatusResponse } from "@/Infrastructure/Services/SchedulerService";
import { ApiResponseCode } from "@/Shared/Index";
import { isCancelledRequest } from "@/Shared/Helpers/Index";
import { GlobalSpinner } from "@/Shared/UI/Index";

const { Title, Text } = Typography;

// ── State tag colors ─────────────────────────────────────────────────────────

const stateColors: Record<string, string> = {
    Normal: "success",
    Paused: "warning",
    Complete: "default",
    Error: "error",
    Blocked: "volcano",
    None: "default",
};

const runStatusColors: Record<string, string> = {
    Success: "success",
    Failed: "error",
    Running: "processing",
};

// ── Helpers ──────────────────────────────────────────────────────────────────

const Months = [ "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" ] as const;

function formatDateTime(value: string | null): string
{
    if (!value)
    {
        return "—";
    }

    const d = new Date(value);
    const day = String(d.getDate()).padStart(2, "0");
    const mon = Months[d.getMonth()] ?? "";
    const year = String(d.getFullYear());
    const time = [ d.getHours(), d.getMinutes(), d.getSeconds() ]
        .map((n) => String(n).padStart(2, "0"))
        .join(":");

    return `${day}-${mon}-${year} ${time}`;
}

// ── Page ─────────────────────────────────────────────────────────────────────

export default function SchedulerConfiguration(): React.JSX.Element
{
    const { token } = theme.useToken();
    const { message } = App.useApp();

    const [ status, setStatus ] = useState<TenantSchedulerStatusResponse | null>(null);
    const [ actionLoading, setActionLoading ] = useState<Record<string, boolean>>({});

    // ── Fetch ────────────────────────────────────────────────────

    const fetchStatus = useCallback(async (signal?: AbortSignal) =>
    {
        GlobalSpinner.show("Loading scheduler status...");

        try
        {
            const response = await SchedulerService.getStatus(signal);

            if (response.Code === ApiResponseCode.Success)
            {
                setStatus(response.Data);
            }
        }
        catch (err: unknown)
        {
            if (isCancelledRequest(err))
            {
                return;
            }

            void message.error("Failed to fetch scheduler status");
        }
        finally
        {
            if (signal?.aborted !== true)
            {
                GlobalSpinner.hide();
            }
        }
    }, [ message ]);

    useEffect(() =>
    {
        const controller = new AbortController();
        void fetchStatus(controller.signal);

        return () =>
        {
            controller.abort();
        };
    }, [ fetchStatus ]);

    // ── Job actions ──────────────────────────────────────────────

    const executeAction = useCallback(async (key: string, action: () => Promise<unknown>, successMsg: string) =>
    {
        setActionLoading((prev) => ({ ...prev, [key]: true }));

        try
        {
            await action();
            void message.success(successMsg);
            void fetchStatus();
        }
        catch
        {
            void message.error(`Action failed: ${key}`);
        }
        finally
        {
            setActionLoading((prev) => ({ ...prev, [key]: false }));
        }
    }, [ fetchStatus, message ]);

    const handlePauseJob = useCallback((jobId: number) =>
    {
        void executeAction(`pause-${String(jobId)}`, () => SchedulerService.pauseJob(jobId), "Job paused");
    }, [ executeAction ]);

    const handleResumeJob = useCallback((jobId: number) =>
    {
        void executeAction(`resume-${String(jobId)}`, () => SchedulerService.resumeJob(jobId), "Job resumed");
    }, [ executeAction ]);

    const handleTriggerJob = useCallback((jobId: number) =>
    {
        void executeAction(`trigger-${String(jobId)}`, () => SchedulerService.triggerJob(jobId), "Job triggered");
    }, [ executeAction ]);

    const handleStopJob = useCallback((jobId: number) =>
    {
        void executeAction(`stop-${String(jobId)}`, () => SchedulerService.stopJob(jobId), "Job stopped");
    }, [ executeAction ]);

    // ── Global actions ───────────────────────────────────────────

    const handlePauseAll = useCallback(() =>
    {
        void executeAction("pauseAll", () => SchedulerService.pauseAll(), "All jobs paused");
    }, [ executeAction ]);

    const handleResumeAll = useCallback(() =>
    {
        void executeAction("resumeAll", () => SchedulerService.resumeAll(), "All jobs resumed");
    }, [ executeAction ]);

    const handleReload = useCallback(() =>
    {
        void executeAction("reload", () => SchedulerService.reload(), "Jobs reloaded from database");
    }, [ executeAction ]);

    const handleRefresh = useCallback(() =>
    {
        void fetchStatus();
    }, [ fetchStatus ]);

    // ── Table columns ────────────────────────────────────────────

    const columns: ColumnsType<JobStatusResponse> = [
        {
            title: "Job Name",
            dataIndex: "SCH_JobName",
            key: "SCH_JobName",
            render: (name: string) => <Text strong>{name}</Text>,
        },
        {
            title: "State",
            dataIndex: "State",
            key: "State",
            width: 110,
            filters: [ ...new Set(status?.Jobs.map((j) => j.State) ?? []) ].map((s) => ({ text: s, value: s })),
            onFilter: (value, record) => record.State === value,
            render: (state: string) => (
                <Tag color={stateColors[state] ?? "default"}>{state}</Tag>
            ),
        },
        {
            title: "Cron",
            dataIndex: "CronExpression",
            key: "CronExpression",
            width: 140,
            render: (cron: string) => <Text code>{cron}</Text>,
        },
        {
            title: "Next Fire",
            dataIndex: "NextFireTimeUtc",
            key: "NextFireTimeUtc",
            width: 220,
            render: (val: string | null) => (
                <Space>
                    <ClockCircleOutlined />
                    <Text>{formatDateTime(val)}</Text>
                </Space>
            ),
        },
        {
            title: "Previous Fire",
            dataIndex: "PreviousFireTimeUtc",
            key: "PreviousFireTimeUtc",
            width: 220,
            render: (val: string | null) => <Text type="secondary">{formatDateTime(val)}</Text>,
        },
        {
            title: "Last Run",
            dataIndex: "LastRunStatus",
            key: "LastRunStatus",
            width: 110,
            render: (runStatus: string | null, record: JobStatusResponse) =>
            {
                if (!runStatus)
                {
                    return <Text type="secondary">—</Text>;
                }

                const tagColor = runStatusColors[runStatus] ?? "default";

                return record.LastErrorMessage
                    ? (
                        <Tooltip title={record.LastErrorMessage}>
                            <Tag color={tagColor} icon={<WarningOutlined />}>{runStatus}</Tag>
                        </Tooltip>
                    )
                    : <Tag color={tagColor}>{runStatus}</Tag>;
            },
        },
        {
            title: "Actions",
            key: "actions",
            width: 180,
            render: (_: unknown, record: JobStatusResponse) =>
            {
                const id = record.SCH_JobId;
                const isPaused = record.State === "Paused";

                return (
                    <Space size={4}>
                        {isPaused
                            ? (
                                <Tooltip title="Resume">
                                    <Button
                                        type="text"
                                        size="small"
                                        icon={<PlayCircleOutlined />}
                                        loading={actionLoading[`resume-${String(id)}`] ?? false}
                                        onClick={() =>
                                        {
                                            handleResumeJob(id);
                                        }}
                                        style={{ color: token.colorSuccess }}
                                    />
                                </Tooltip>
                            )
                            : (
                                <Tooltip title="Pause">
                                    <Button
                                        type="text"
                                        size="small"
                                        icon={<PauseCircleOutlined />}
                                        loading={actionLoading[`pause-${String(id)}`] ?? false}
                                        onClick={() =>
                                        {
                                            handlePauseJob(id);
                                        }}
                                        style={{ color: token.colorWarning }}
                                    />
                                </Tooltip>
                            )}

                        <Tooltip title="Trigger Now">
                            <Button
                                type="text"
                                size="small"
                                icon={<ThunderboltOutlined />}
                                loading={actionLoading[`trigger-${String(id)}`] ?? false}
                                onClick={() =>
                                {
                                    handleTriggerJob(id);
                                }}
                                disabled={isPaused}
                                style={{ color: token.colorPrimary }}
                            />
                        </Tooltip>

                        <Popconfirm
                            title="Stop this job?"
                            description="The job will not run until the scheduler is reloaded."
                            onConfirm={() =>
                            {
                                handleStopJob(id);
                            }}
                            okText="Stop"
                            okButtonProps={{ danger: true }}
                        >
                            <Tooltip title="Stop">
                                <Button
                                    type="text"
                                    size="small"
                                    icon={<StopOutlined />}
                                    loading={actionLoading[`stop-${String(id)}`] ?? false}
                                    danger
                                />
                            </Tooltip>
                        </Popconfirm>
                    </Space>
                );
            },
        },
    ];

    return (
        <Space orientation="vertical" size="large" style={{ width: "100%" }}>
            {/* Header */}
            <Row justify="space-between" align="middle">
                <Col>
                    <Title level={4} style={{ margin: 0 }}>Scheduler</Title>
                </Col>
                <Col>
                    <Space>
                        <Popconfirm
                            title="Pause all jobs?"
                            onConfirm={handlePauseAll}
                            okText="Pause All"
                        >
                            <Button
                                icon={<PauseCircleOutlined />}
                                loading={actionLoading["pauseAll"] ?? false}
                            >
                                Pause All
                            </Button>
                        </Popconfirm>

                        <Button
                            icon={<PlayCircleOutlined />}
                            loading={actionLoading["resumeAll"] ?? false}
                            onClick={handleResumeAll}
                        >
                            Resume All
                        </Button>

                        <Popconfirm
                            title="Reload all jobs from database?"
                            onConfirm={handleReload}
                            okText="Reload"
                        >
                            <Button
                                icon={<ReloadOutlined />}
                                loading={actionLoading["reload"] ?? false}
                            >
                                Reload
                            </Button>
                        </Popconfirm>

                        <Button
                            icon={<ReloadOutlined />}
                            onClick={handleRefresh}
                        >
                            Refresh
                        </Button>
                    </Space>
                </Col>
            </Row>

            {/* Summary cards */}
            {status && (
                <Row gutter={[ 16, 16 ]}>
                    <Col xs={12} sm={6}>
                        <Card size="small">
                            <Text type="secondary">Total Jobs</Text>
                            <Title level={3} style={{ margin: 0 }}>{status.TotalJobs}</Title>
                        </Card>
                    </Col>
                    <Col xs={12} sm={6}>
                        <Card size="small">
                            <Text type="secondary">Active</Text>
                            <Title level={3} style={{ margin: 0, color: token.colorSuccess }}>{status.ActiveJobs}</Title>
                        </Card>
                    </Col>
                    <Col xs={12} sm={6}>
                        <Card size="small">
                            <Text type="secondary">Paused</Text>
                            <Title level={3} style={{ margin: 0, color: token.colorWarning }}>{status.PausedJobs}</Title>
                        </Card>
                    </Col>
                    <Col xs={12} sm={6}>
                        <Card size="small">
                            <Text type="secondary">Errors</Text>
                            <Title level={3} style={{ margin: 0, color: token.colorError }}>{status.ErrorJobs}</Title>
                        </Card>
                    </Col>
                </Row>
            )}

            {/* Jobs table */}
            <Card>
                <Table<JobStatusResponse>
                    columns={columns}
                    dataSource={status?.Jobs ?? []}
                    rowKey="SCH_JobId"
                    pagination={false}
                    size="middle"
                />
            </Card>
        </Space>
    );
}
