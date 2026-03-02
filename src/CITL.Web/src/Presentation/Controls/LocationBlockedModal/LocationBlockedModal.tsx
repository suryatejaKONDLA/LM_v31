import { useCallback, memo } from "react";
import { Modal, Button, Typography, Space } from "antd";
import { EnvironmentOutlined } from "@ant-design/icons";
import { useLocationStore } from "@/Application/Index";

const GeoOptions: PositionOptions = {
    enableHighAccuracy: true,
    timeout: 10_000,
    maximumAge: 0,
};

/**
 * Global modal displayed on any page when the browser denies geolocation.
 * Render once inside the application root — reads state from `LocationStore`.
 */
function LocationBlockedModalInner(): React.JSX.Element
{
    const { isDenied, setGranted, setDenied, resetToIdle } = useLocationStore();

    const handleRetry = useCallback(() =>
    {
        resetToIdle();
        navigator.geolocation.getCurrentPosition(
            (pos) =>
            {
                setGranted(pos);
            },
            () =>
            {
                setDenied();
            },
            GeoOptions,
        );
    }, [ resetToIdle, setGranted, setDenied ]);

    return (
        <Modal
            open={isDenied}
            title={
                <Space>
                    <EnvironmentOutlined style={{ color: "#faad14", fontSize: 18 }} />
                    <span>Location Access Required</span>
                </Space>
            }
            footer={
                <Button type="primary" onClick={handleRetry}>
                    Try Again
                </Button>
            }
            closable={false}
            mask={{ closable: false }}
            centered
        >
            <Typography.Paragraph>
                This application requires your location to continue. Your browser has blocked location access.
            </Typography.Paragraph>
            <Typography.Paragraph type="secondary" style={{ marginBottom: 0 }}>
                To allow access: click the location icon in your browser's address bar → select <strong>Allow</strong> → then click <strong>Try Again</strong>.
            </Typography.Paragraph>
        </Modal>
    );
}

export const LocationBlockedModal = memo(LocationBlockedModalInner);
