import { useState, useEffect, useMemo, useRef, useCallback } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import { Typography, Tag, Button, Card, Divider, Flex, theme } from "antd";
import {
    CheckCircleOutlined,
    SyncOutlined,
    ExclamationCircleOutlined,
    DisconnectOutlined,
    WifiOutlined,
    CloudServerOutlined,
    ApiOutlined,
    ReloadOutlined,
} from "@ant-design/icons";
import { useConnectivityStore } from "@/Application/Index";
import { PingHubConnection } from "@/Infrastructure/Index";
import styles from "./NetworkError.module.css";

type CombinedStatus = "online" | "reconnecting" | "offline";
type ServiceStatus = "online" | "connecting" | "offline";

function deriveCombinedStatus(
    hubStatus: string,
    networkOnline: boolean,
    apiReachable: boolean,
): CombinedStatus
{
    if (!networkOnline || (hubStatus === "disconnected" && !apiReachable))
    {
        return "offline";
    }

    if (hubStatus === "reconnecting")
    {
        return "reconnecting";
    }

    if (hubStatus === "connected" && apiReachable)
    {
        return "online";
    }

    return "offline";
}

function deriveServiceStatuses(
    hubStatus: string,
    networkOnline: boolean,
    apiReachable: boolean,
): { network: ServiceStatus; hub: ServiceStatus; api: ServiceStatus }
{
    return {
        network: networkOnline ? "online" : "offline",
        hub: hubStatus === "connected" ? "online" : hubStatus === "reconnecting" ? "connecting" : "offline",
        api: hubStatus === "connected" && apiReachable ? "online" : hubStatus === "reconnecting" ? "connecting" : "offline",
    };
}

function getStatusColor(status: ServiceStatus, token: { colorSuccess: string; colorWarning: string; colorError: string }): string
{
    if (status === "online")
    {
        return token.colorSuccess;
    }

    if (status === "connecting")
    {
        return token.colorWarning;
    }

    return token.colorError;
}

function getTagColor(status: ServiceStatus): "success" | "warning" | "error"
{
    if (status === "online")
    {
        return "success";
    }

    if (status === "connecting")
    {
        return "warning";
    }

    return "error";
}

function getTagIcon(status: ServiceStatus): React.ReactNode
{
    if (status === "online")
    {
        return <CheckCircleOutlined />;
    }

    if (status === "connecting")
    {
        return <SyncOutlined spin />;
    }

    return <ExclamationCircleOutlined />;
}

function getTagLabel(status: ServiceStatus): string
{
    if (status === "online")
    {
        return "Online";
    }

    if (status === "connecting")
    {
        return "Connecting";
    }

    return "Offline";
}

const COMBINED_ICON: Record<CombinedStatus, React.ReactNode> = {
    online: <CheckCircleOutlined />,
    reconnecting: <SyncOutlined spin />,
    offline: <DisconnectOutlined />,
};

const COMBINED_TITLE: Record<CombinedStatus, string> = {
    online: "Connection Restored",
    reconnecting: "Reconnecting...",
    offline: "Connection Lost",
};

const AUTO_RETRY_SECS = 30;

const COMBINED_DESCRIPTION: Record<Exclude<CombinedStatus, "offline">, string> = {
    online: "The server is back online. Redirecting you back...",
    reconnecting: "Attempting to reconnect to the server. Please wait...",
};

function getOfflineDescription(networkOnline: boolean): string
{
    if (!networkOnline)
    {
        return "Your device appears to be offline. Please check your internet connection.";
    }

    return "The server is unreachable. Please contact your administrator or try again.";
}

interface StatusRowProps
{
    icon: React.ReactNode;
    label: string;
    status: ServiceStatus;
    color: string;
}

function StatusRow({ icon, label, status, color }: StatusRowProps): React.JSX.Element
{
    return (
        <div className={styles["statusRow"]}>
            <span className={styles["statusLabel"]}>
                <span className={styles["statusIcon"]} style={{ color }}>{icon}</span>
                {label}
            </span>
            <Tag
                icon={getTagIcon(status)}
                color={getTagColor(status)}
                className={styles["statusTag"] ?? ""}
            >
                {getTagLabel(status)}
            </Tag>
        </div>
    );
}

export default function NetworkError(): React.JSX.Element
{
    const { token } = theme.useToken();
    const navigate = useNavigate();
    const location = useLocation();
    const { hubStatus, networkOnline, apiReachable, init, destroy } = useConnectivityStore();
    const [ retrying, setRetrying ] = useState(false);
    const [ countdown, setCountdown ] = useState(AUTO_RETRY_SECS);
    const countdownRef = useRef(AUTO_RETRY_SECS);

    useEffect(() =>
    {
        init();
        return destroy;
    }, [ init, destroy ]);

    const stateData = location.state as { from?: string } | null;
    const returnPath = stateData?.from ?? "/Login";

    const combined = deriveCombinedStatus(hubStatus, networkOnline, apiReachable);
    const isOnline = combined === "online";

    const services = useMemo(
        () => deriveServiceStatuses(hubStatus, networkOnline, apiReachable),
        [ hubStatus, networkOnline, apiReachable ],
    );

    // Auto-redirect back when connection is restored
    useEffect(() =>
    {
        if (!isOnline)
        {
            return;
        }

        const timer = setTimeout(() =>
        {
            void navigate(returnPath, { replace: true });
        }, 2_000);

        return () =>
        {
            clearTimeout(timer);
        };
    }, [ isOnline, navigate, returnPath ]);

    const doRetry = useCallback((): void =>
    {
        setRetrying(true);
        countdownRef.current = AUTO_RETRY_SECS;
        setCountdown(AUTO_RETRY_SECS);
        void PingHubConnection.restart();
        setTimeout(() =>
        {
            setRetrying(false);
        }, 3_000);
    }, []);

    // Countdown timer — ticks every second, auto-retries at 0 when offline
    useEffect(() =>
    {
        if (isOnline || retrying)
        {
            countdownRef.current = AUTO_RETRY_SECS;
            return;
        }

        const tick = setInterval(() =>
        {
            countdownRef.current -= 1;

            if (countdownRef.current <= 0)
            {
                countdownRef.current = AUTO_RETRY_SECS;
                doRetry();
                return;
            }

            setCountdown(countdownRef.current);
        }, 1_000);

        return () =>
        {
            clearInterval(tick);
        };
    }, [ isOnline, retrying, doRetry ]);

    const statusColor = getStatusColor(
        combined === "online" ? "online" : combined === "reconnecting" ? "connecting" : "offline",
        token,
    );

    return (
        <div className={styles["page"]} style={{ backgroundColor: token.colorBgBase }}>
            <div className={styles["card"]}>
                {/* Animated status icon */}
                <div
                    className={[ styles["iconWrapper"], !isOnline ? styles["iconPulse"] : "" ].filter(Boolean).join(" ")}
                    style={{
                        backgroundColor: `${statusColor}10`,
                        border: `2px solid ${statusColor}30`,
                        color: statusColor,
                    }}
                >
                    {COMBINED_ICON[combined]}
                </div>

                {/* Title */}
                <Typography.Title
                    level={2}
                    style={{ margin: 0, marginBottom: 8, fontWeight: 700 }}
                >
                    {COMBINED_TITLE[combined]}
                </Typography.Title>

                {/* Description */}
                <Typography.Text type="secondary" className={styles["description"] ?? ""}>
                    {combined === "offline" ? getOfflineDescription(networkOnline) : COMBINED_DESCRIPTION[combined]}
                </Typography.Text>

                {/* Status card — 3 rows: Network, Hub, API */}
                <Card
                    style={{
                        marginBottom: 0,
                        borderRadius: token.borderRadiusLG,
                        border: `1px solid ${token.colorBorderSecondary}`,
                        textAlign: "left",
                    }}
                    styles={{ body: { padding: 0 } }}
                >
                    <StatusRow
                        icon={<WifiOutlined />}
                        label="Network"
                        status={services.network}
                        color={getStatusColor(services.network, token)}
                    />
                    <Divider style={{ margin: 0 }} />
                    <StatusRow
                        icon={<ApiOutlined />}
                        label="Hub"
                        status={services.hub}
                        color={getStatusColor(services.hub, token)}
                    />
                    <Divider style={{ margin: 0 }} />
                    <StatusRow
                        icon={<CloudServerOutlined />}
                        label="API Server"
                        status={services.api}
                        color={getStatusColor(services.api, token)}
                    />
                </Card>

                {/* Actions */}
                <Flex gap={12} className={styles["actions"]}>
                    {!isOnline ? (
                        <>
                            <Button
                                type="primary"
                                size="large"
                                icon={<ReloadOutlined />}
                                loading={retrying}
                                className={styles["actionButton"] ?? ""}
                                onClick={doRetry}
                            >
                                {retrying ? "Retrying..." : `Retry Connection (${String(countdown)}s)`}
                            </Button>
                            <Button
                                size="large"
                                className={styles["actionButton"] ?? ""}
                                onClick={() =>
                                {
                                    window.location.reload();
                                }}
                            >
                                Reload Page
                            </Button>
                        </>
                    ) : (
                        <Button
                            type="primary"
                            size="large"
                            icon={<CheckCircleOutlined />}
                            className={styles["actionButton"] ?? ""}
                            onClick={() =>
                            {
                                void navigate(returnPath, { replace: true });
                            }}
                        >
                            Continue
                        </Button>
                    )}
                </Flex>
            </div>
        </div>
    );
}
