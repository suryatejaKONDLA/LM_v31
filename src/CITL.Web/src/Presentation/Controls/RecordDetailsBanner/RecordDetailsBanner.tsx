import { Divider, Tag, Typography, theme } from "antd";
import { InfoCircleOutlined, UserOutlined, EditOutlined } from "@ant-design/icons";
import { memo, useMemo } from "react";

const { Text } = Typography;

/**
 * Props for the RecordDetailsBanner control.
 */
export interface RecordDetailsBannerProps
{
    /** Array of [RecNo, CreatedBy, ModifiedBy] values. */
    fields: [string | null, string | null, string | null] | string[];
    /** Query string key — "0" indicates a new record (banner hidden). */
    queryString1?: string;
}

/**
 * RecordDetailsBanner — Displays record metadata (ID, Created by, Modified by).
 *
 * Auto-hides for new records (queryString1 === "0") or empty fields.
 * Uses color-coded tags and theme-aware styling.
 */
function RecordDetailsBannerInner({
    fields,
    queryString1,
}: RecordDetailsBannerProps): React.ReactNode
{
    const { token } = theme.useToken();

    const recNo = fields[0] ?? null;
    const createdBy = fields[1] ?? null;
    const modifiedBy = fields[2] ?? null;

    const notApplicable = "N/A";

    const tagStyle = useMemo<React.CSSProperties>(
        () => ({ fontSize: 12, padding: "2px 8px", borderRadius: 4, fontWeight: 600 }),
        [],
    );

    const labelStyle = useMemo<React.CSSProperties>(
        () => ({ color: token.colorTextSecondary, fontSize: 12, whiteSpace: "nowrap" as const }),
        [ token.colorTextSecondary ],
    );

    const itemStyle = useMemo<React.CSSProperties>(
        () => ({ display: "flex", alignItems: "center", gap: 6, flex: "1 1 auto" }),
        [],
    );

    const containerStyle = useMemo<React.CSSProperties>(
        () => ({
            padding: "6px 10px",
            background: token.colorBgLayout,
            borderRadius: token.borderRadius,
            marginBottom: 10,
            display: "flex",
            flexWrap: "wrap" as const,
            gap: "6px 12px",
            justifyContent: "space-between",
        }),
        [ token.colorBgLayout, token.borderRadius ],
    );

    // Hide for new records or empty fields
    if (queryString1 === "0" || fields.length === 0 || !recNo)
    {
        return null;
    }

    return (
        <>
            <div style={containerStyle}>
                {/* Record Number */}
                <div style={itemStyle}>
                    <InfoCircleOutlined style={{ color: token.colorPrimary, fontSize: 14 }} />
                    <Text style={labelStyle}>Rec No:</Text>
                    <Tag color="blue" style={tagStyle}>{recNo}</Tag>
                </div>

                {/* Created By */}
                <div style={itemStyle}>
                    <UserOutlined style={{ color: token.colorSuccess, fontSize: 14 }} />
                    <Text style={labelStyle}>Created:</Text>
                    <Tag color="green" style={tagStyle}>{createdBy ?? notApplicable}</Tag>
                </div>

                {/* Modified By */}
                <div style={itemStyle}>
                    <EditOutlined style={{ color: token.colorWarning, fontSize: 14 }} />
                    <Text style={labelStyle}>Modified:</Text>
                    <Tag color="orange" style={tagStyle}>{modifiedBy ?? notApplicable}</Tag>
                </div>
            </div>

            <Divider style={{ marginTop: 0, marginBottom: 12 }} />
        </>
    );
}

export const RecordDetailsBanner = memo(RecordDetailsBannerInner);
