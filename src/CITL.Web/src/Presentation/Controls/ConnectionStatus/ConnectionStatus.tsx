import { memo, useMemo } from "react";
import { Tooltip } from "antd";
import { SyncOutlined, DisconnectOutlined } from "@ant-design/icons";
import { useConnectivityStore } from "@/Application/Index";
import { PingHubConnection } from "@/Infrastructure/Index";
import styles from "./ConnectionStatus.module.css";

type OverallStatus = "connected" | "reconnecting" | "disconnected";

function deriveOverallStatus(
    hubStatus: string,
    networkOnline: boolean,
    apiReachable: boolean,
): OverallStatus
{
    if (!networkOnline)
    {
        return "disconnected";
    }

    if (hubStatus === "reconnecting")
    {
        return "reconnecting";
    }

    if (hubStatus === "disconnected" || !apiReachable)
    {
        return "disconnected";
    }

    return "connected";
}

const BarConfig: Record<Exclude<OverallStatus, "connected">, { icon: React.ReactNode; message: string; className: string }> = {
    reconnecting: {
        icon: <SyncOutlined spin style={{ fontSize: 13 }} />,
        message: "Reconnecting — please wait\u2026",
        className: styles["warn"] ?? "",
    },
    disconnected: {
        icon: <DisconnectOutlined style={{ fontSize: 13 }} />,
        message: "You are offline. Click to retry.",
        className: styles["error"] ?? "",
    },
};

/**
 * Slim notification bar shown only when disconnected or reconnecting.
 * Auto-hides when connected. Tooltip shows Hub/Network/API breakdown.
 */
function ConnectionStatusInner(): React.JSX.Element | null
{
    const { hubStatus, networkOnline, apiReachable } = useConnectivityStore();
    const overall = deriveOverallStatus(hubStatus, networkOnline, apiReachable);

    const tooltipContainerStyle = useMemo<React.CSSProperties>(
        () => ({ display: "flex", flexDirection: "column", gap: 6, padding: "2px 0" }),
        [],
    );

    if (overall === "connected")
    {
        return null;
    }

    const config = BarConfig[overall];

    const tooltipContent = (
        <div style={tooltipContainerStyle}>
            <TooltipRow label="Hub" value={hubStatus} ok={hubStatus === "connected"} warn={hubStatus === "reconnecting"} />
            <TooltipRow label="Network" value={networkOnline ? "online" : "offline"} ok={networkOnline} warn={false} />
            <TooltipRow label="API" value={apiReachable ? "reachable" : "unreachable"} ok={apiReachable} warn={false} />
        </div>
    );

    return (
        <Tooltip title={tooltipContent} placement="bottom">
            <div
                className={[ styles["bar"], config.className ].join(" ")}
                role={overall === "disconnected" ? "button" : undefined}
                tabIndex={overall === "disconnected" ? 0 : undefined}
                onClick={overall === "disconnected" ? () => void PingHubConnection.restart() : undefined}
                onKeyDown={
                    overall === "disconnected"
                        ? (e) =>
                        {
                            if (e.key === "Enter")
                            {
                                void PingHubConnection.restart();
                            }
                        }
                        : undefined
                }
            >
                {config.icon}
                <span>{config.message}</span>
            </div>
        </Tooltip>
    );
}

// ── Tooltip row ───────────────────────────────────────────────────────────────

const tooltipRowStyle: React.CSSProperties = { display: "flex", alignItems: "center", justifyContent: "space-between", gap: 20 };
const tooltipLabelStyle: React.CSSProperties = { color: "#a0a0a0", fontSize: 12, fontWeight: 500, letterSpacing: "0.02em" };

interface TooltipRowProps
{
    label: string;
    value: string;
    ok: boolean;
    warn: boolean;
}

function TooltipRow({ label, value, ok, warn }: TooltipRowProps): React.JSX.Element
{
    const dotColor = ok ? "#52c41a" : warn ? "#faad14" : "#ff4d4f";
    const badgeBg  = ok ? "#162312" : warn ? "#2b2111" : "#2a1215";
    const badgeFg  = ok ? "#49aa19" : warn ? "#d89614" : "#e84749";

    return (
        <div style={tooltipRowStyle}>
            <span style={tooltipLabelStyle}>
                {label}
            </span>
            <span
                style={{
                    display: "inline-flex",
                    alignItems: "center",
                    gap: 5,
                    padding: "1px 8px",
                    borderRadius: 10,
                    background: badgeBg,
                    fontSize: 11,
                    fontWeight: 600,
                    color: badgeFg,
                    letterSpacing: "0.03em",
                    textTransform: "capitalize",
                }}
            >
                <span
                    style={{
                        width: 6,
                        height: 6,
                        borderRadius: "50%",
                        backgroundColor: dotColor,
                    }}
                />
                {value}
            </span>
        </div>
    );
}

export const ConnectionStatus = memo(ConnectionStatusInner);
