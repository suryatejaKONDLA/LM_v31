import { useState, useEffect, useCallback } from "react";
import { Card, Row, Col, Table, Tag, Button, Space, Typography, Tooltip, Descriptions, theme } from "antd";
import {
    ReloadOutlined,
    CheckCircleOutlined,
    ExclamationCircleOutlined,
    CloseCircleOutlined,
    ClockCircleOutlined,
    DatabaseOutlined,
    CloudServerOutlined,
    HddOutlined,
    MailOutlined,
    ThunderboltOutlined,
    BarChartOutlined,
    ApiOutlined,
    WifiOutlined,
    CloudOutlined,
    FundOutlined,
} from "@ant-design/icons";
import type { ColumnsType } from "antd/es/table";
import { HealthService } from "@/Infrastructure/Index";
import type { HealthCheckResponse, ServiceHealthEntry, HealthStatus } from "@/Infrastructure/Services/HealthService";
import { GlobalSpinner } from "@/Presentation/Controls/Index";

const { Title, Text } = Typography;

// ── Status helpers ───────────────────────────────────────────────────────────

const statusConfig: Record<HealthStatus, { color: string; icon: React.ReactNode }>  = {
    Healthy: { color: "success", icon: <CheckCircleOutlined /> },
    Degraded: { color: "warning", icon: <ExclamationCircleOutlined /> },
    Unhealthy: { color: "error", icon: <CloseCircleOutlined /> },
};

function StatusTag({ status }: { status: HealthStatus }): React.ReactNode
{
    const cfg = statusConfig[status];

    return (
        <Tag color={cfg.color} icon={cfg.icon} style={{ fontWeight: 600 }}>
            {status}
        </Tag>
    );
}

function formatDuration(ms: number): string
{
    if (ms < 1)
    {
        return "<1 ms";
    }

    if (ms < 1000)
    {
        return `${String(Math.round(ms))} ms`;
    }

    return `${(ms / 1000).toFixed(2)} s`;
}

const Months = [ "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" ] as const;

function formatTimestamp(ts: string): string
{
    const d = new Date(ts);
    const day = String(d.getDate()).padStart(2, "0");
    const mon = Months[d.getMonth()] ?? "";
    const year = String(d.getFullYear());
    const time = [ d.getHours(), d.getMinutes(), d.getSeconds() ]
        .map((n) => String(n).padStart(2, "0"))
        .join(":");

    return `${day}-${mon}-${year} ${time}`;
}

// ── Service icon by name ─────────────────────────────────────────────────────

function serviceIcon(name: string): React.ReactNode
{
    const lower = name.toLowerCase();

    if (lower.includes("sql") || lower.includes("database"))
    {
        return <DatabaseOutlined />;
    }

    if (lower.includes("redis") || lower.includes("cache"))
    {
        return <ThunderboltOutlined />;
    }

    if (lower.includes("disk") || lower.includes("storage") && lower.includes("disk"))
    {
        return <HddOutlined />;
    }

    if (lower.includes("r2") || lower.includes("s3") || lower.includes("storage"))
    {
        return <CloudOutlined />;
    }

    if (lower.includes("mail") || lower.includes("smtp") || lower.includes("email"))
    {
        return <MailOutlined />;
    }

    if (lower.includes("quartz") || lower.includes("scheduler") || lower.includes("job"))
    {
        return <ClockCircleOutlined />;
    }

    if (lower.includes("grafana") || lower.includes("dashboard") || lower.includes("monitor"))
    {
        return <BarChartOutlined />;
    }

    if (lower.includes("otlp") || lower.includes("collector") || lower.includes("telemetry") || lower.includes("otel"))
    {
        return <ApiOutlined />;
    }

    if (lower.includes("signalr") || lower.includes("websocket") || lower.includes("hub"))
    {
        return <WifiOutlined />;
    }

    if (lower.includes("memory") || lower.includes("process"))
    {
        return <FundOutlined />;
    }

    return <CloudServerOutlined />;
}

// ── Table columns ────────────────────────────────────────────────────────────

const columns: ColumnsType<ServiceHealthEntry> = [
    {
        title: "Service",
        dataIndex: "Name",
        key: "Name",
        render: (name: string) => (
            <Space>
                {serviceIcon(name)}
                <Text strong>{name}</Text>
            </Space>
        ),
    },
    {
        title: "Status",
        dataIndex: "Status",
        key: "Status",
        width: 130,
        filters: [
            { text: "Healthy", value: "Healthy" },
            { text: "Degraded", value: "Degraded" },
            { text: "Unhealthy", value: "Unhealthy" },
        ],
        onFilter: (value, record) => record.Status === value,
        render: (status: HealthStatus) => <StatusTag status={status} />,
    },
    {
        title: "Duration",
        dataIndex: "DurationMs",
        key: "DurationMs",
        width: 120,
        sorter: (a, b) => a.DurationMs - b.DurationMs,
        render: (ms: number) => (
            <Space>
                <ClockCircleOutlined />
                <Text>{formatDuration(ms)}</Text>
            </Space>
        ),
    },
    {
        title: "Description",
        dataIndex: "Description",
        key: "Description",
        ellipsis: { showTitle: false },
        render: (desc: string | null) =>
            desc
                ? <Tooltip title={desc}><Text type="secondary">{desc}</Text></Tooltip>
                : <Text type="secondary">—</Text>,
    },
];

// ── Expanded row ─────────────────────────────────────────────────────────────

function DataValue({ value }: { value: unknown }): React.ReactNode
{
    if (value === null || value === undefined)
    {
        return <Text type="secondary">—</Text>;
    }

    // Nested object — render as inline tag list (e.g. tenant health entries)
    if (typeof value === "object" && !Array.isArray(value))
    {
        const entries = Object.entries(value as Record<string, unknown>);

        return (
            <Space size={4} wrap>
                {entries.map(([ k, v ]) =>
                {
                    const label = typeof v === "string" || typeof v === "number" || typeof v === "boolean"
                        ? `${k}: ${String(v)}`
                        : k;
                    const isHealthy = v === "Healthy" || v === true;
                    const isUnhealthy = v === "Unhealthy" || v === false;
                    const color = isHealthy ? "success" : isUnhealthy ? "error" : "default";

                    return <Tag key={k} color={color}>{label}</Tag>;
                })}
            </Space>
        );
    }

    if (typeof value === "boolean")
    {
        return <Tag color={value ? "success" : "error"}>{String(value)}</Tag>;
    }

    if (typeof value === "string")
    {
        return <Text>{value}</Text>;
    }

    return <Text>{JSON.stringify(value)}</Text>;
}

function ExpandedRow({ record }: { record: ServiceHealthEntry }): React.ReactNode
{
    const items: React.ReactNode[] = [];

    if (record.Error)
    {
        items.push(
            <Descriptions.Item key="error" label="Error" span={3}>
                <Text type="danger" style={{ whiteSpace: "pre-wrap" }}>{record.Error}</Text>
            </Descriptions.Item>,
        );
    }

    if (record.Data && Object.keys(record.Data).length > 0)
    {
        for (const [ key, value ] of Object.entries(record.Data))
        {
            items.push(
                <Descriptions.Item key={key} label={key}>
                    <DataValue value={value} />
                </Descriptions.Item>,
            );
        }
    }

    if (items.length === 0)
    {
        return <Text type="secondary">No additional details.</Text>;
    }

    return (
        <Descriptions bordered size="small" column={3}>
            {items}
        </Descriptions>
    );
}

// ── Page ─────────────────────────────────────────────────────────────────────

export default function SystemHealth(): React.JSX.Element
{
    const { token } = theme.useToken();

    const [ health, setHealth ] = useState<HealthCheckResponse | null>(null);
    const [ error, setError ] = useState<string | null>(null);

    const fetchHealth = useCallback(async (signal?: AbortSignal) =>
    {
        GlobalSpinner.show("Loading health status...");
        setError(null);

        try
        {
            const data = await HealthService.getHealth(signal);
            setHealth(data);
        }
        catch (err: unknown)
        {
            if (err instanceof DOMException && err.name === "AbortError")
            {
                return;
            }

            setError(err instanceof Error ? err.message : "Failed to fetch health status");
        }
        finally
        {
            if (signal?.aborted !== true)
            {
                GlobalSpinner.hide();
            }
        }
    }, []);

    useEffect(() =>
    {
        const controller = new AbortController();
        void fetchHealth(controller.signal);

        return () =>
        {
            controller.abort();
        };
    }, [ fetchHealth ]);

    const handleRefresh = useCallback(() =>
    {
        void fetchHealth();
    }, [ fetchHealth ]);

    // ── Summary counts ───────────────────────────────────────────

    const healthyCnt = health?.Services.filter((s) => s.Status === "Healthy").length ?? 0;
    const degradedCnt = health?.Services.filter((s) => s.Status === "Degraded").length ?? 0;
    const unhealthyCnt = health?.Services.filter((s) => s.Status === "Unhealthy").length ?? 0;

    return (
        <Space orientation="vertical" size="large" style={{ width: "100%" }}>
            {/* Header */}
            <Row justify="space-between" align="middle">
                <Col>
                    <Space align="center" size="middle">
                        <Title level={4} style={{ margin: 0 }}>System Health</Title>
                        {health && <StatusTag status={health.Status} />}
                    </Space>
                </Col>
                <Col>
                    <Button
                        icon={<ReloadOutlined />}
                        onClick={handleRefresh}
                    >
                        Refresh
                    </Button>
                </Col>
            </Row>

            {/* Error banner */}
            {error && (
                <Card size="small" style={{ borderColor: token.colorErrorBorder, background: token.colorErrorBg }}>
                    <Text type="danger">{error}</Text>
                </Card>
            )}

            {/* Summary cards */}
            {health && (
                <Row gutter={[ 16, 16 ]}>
                    <Col xs={12} sm={6}>
                        <Card size="small">
                            <Text type="secondary">Total Services</Text>
                            <Title level={3} style={{ margin: 0 }}>{health.Services.length}</Title>
                        </Card>
                    </Col>
                    <Col xs={12} sm={6}>
                        <Card size="small">
                            <Text type="secondary">Healthy</Text>
                            <Title level={3} style={{ margin: 0, color: token.colorSuccess }}>{healthyCnt}</Title>
                        </Card>
                    </Col>
                    <Col xs={12} sm={6}>
                        <Card size="small">
                            <Text type="secondary">Degraded</Text>
                            <Title level={3} style={{ margin: 0, color: token.colorWarning }}>{degradedCnt}</Title>
                        </Card>
                    </Col>
                    <Col xs={12} sm={6}>
                        <Card size="small">
                            <Text type="secondary">Unhealthy</Text>
                            <Title level={3} style={{ margin: 0, color: token.colorError }}>{unhealthyCnt}</Title>
                        </Card>
                    </Col>
                </Row>
            )}

            {/* Meta info */}
            {health && (
                <Space separator={<Text type="secondary">|</Text>}>
                    <Text type="secondary">
                        Total Duration: <Text strong>{formatDuration(health.TotalDurationMs)}</Text>
                    </Text>
                    <Text type="secondary">
                        Last Checked: <Text strong>{formatTimestamp(health.Timestamp)}</Text>
                    </Text>
                </Space>
            )}

            {/* Services table */}
            <Card>
                <Table<ServiceHealthEntry>
                    columns={columns}
                    dataSource={health?.Services ?? []}
                    rowKey="Name"
                    pagination={false}
                    size="middle"
                    expandable={{
                        expandedRowRender: (record) => <ExpandedRow record={record} />,
                        rowExpandable: (record) => Boolean(record.Error ?? (record.Data && Object.keys(record.Data).length > 0)),
                    }}
                />
            </Card>
        </Space>
    );
}
