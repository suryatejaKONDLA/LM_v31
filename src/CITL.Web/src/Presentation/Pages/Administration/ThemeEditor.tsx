import React, { Suspense, useEffect, useRef, useState, useCallback } from "react";
import { enUS, type Theme as AntdTheme } from "antd-token-previewer";
import type { ThemeEditorMode } from "antd-token-previewer/lib/ThemeEditor";
import { Card, Row, Col, Space, Typography, Tag, Divider, Select, Skeleton, ConfigProvider, theme, Button, Tooltip, Switch } from "antd";
import { SettingOutlined, SaveOutlined, UndoOutlined, SunOutlined, MoonOutlined, InfoCircleOutlined, EditOutlined, CompressOutlined, SmileOutlined } from "@ant-design/icons";
import { ModalDialog, GlobalSpinner } from "@/Presentation/Controls/Index";
import { ThemeService } from "@/Infrastructure/Index";
import { useThemeStore } from "@/Application/Index";
import { ApiResponseCode, ThemeConstants } from "@/Shared/Index";

const AntdThemeEditor = React.lazy(() => import("antd-token-previewer/es/ThemeEditor"));

const fontOptions = [
    { value: "'Outfit', sans-serif", label: "✨ Outfit (Default)" },
    { value: "'Inter', sans-serif", label: "Inter" },
    { value: "'Lato', sans-serif", label: "Lato" },
    { value: "'Montserrat', sans-serif", label: "Montserrat" },
    { value: "'Noto Sans', sans-serif", label: "Noto Sans" },
    { value: "'Nunito', sans-serif", label: "Nunito" },
    { value: "'Open Sans', sans-serif", label: "Open Sans" },
    { value: "'Poppins', sans-serif", label: "Poppins" },
    { value: "'Roboto', sans-serif", label: "Roboto" },
    { value: "'Space Grotesk', sans-serif", label: "Space Grotesk" },
    { value: "'Arial', sans-serif", label: "Arial (System)" },
    { value: "'Helvetica Neue', Helvetica, sans-serif", label: "Helvetica (System)" },
    { value: "system-ui, -apple-system, 'Segoe UI', sans-serif", label: "System UI" },
    { value: "Georgia, serif", label: "Georgia (Serif)" },
    { value: "'Times New Roman', serif", label: "Times New Roman (Serif)" },
    { value: "'Courier New', monospace", label: "Courier New (Mono)" },
    { value: "'Fira Code', monospace", label: "Fira Code (Mono)" },
];

interface SavedThemeData
{
    tokens: Record<string, unknown>;
    isCompact?: boolean;
    isHappyWork?: boolean;
}

export default function ThemeEditor(): React.JSX.Element
{
    const { token } = theme.useToken();
    const { customTokens, setCustomTokens, isDarkMode, toggleMode, isCompact, setCompact, isHappyWork, setHappyWork } = useThemeStore();

    const [ editorMode, setEditorMode ] = useState<ThemeEditorMode>("global");
    const [ isAdvanced, setIsAdvanced ] = useState(false);
    const [ hasUnsavedChanges, setHasUnsavedChanges ] = useState(false);

    const originalThemeRef = useRef({
        tokens: { ...customTokens },
        isDarkMode,
    });

    const [ editorTheme, setEditorTheme ] = useState<AntdTheme>(() => ({
        name: "Custom Theme",
        key: "citl-theme",
        config: {
            token: customTokens,
            algorithm: isDarkMode ? theme.darkAlgorithm : theme.defaultAlgorithm,
        },
    }));

    // Set page title
    useEffect(() =>
    {
        document.title = "Theme Editor";
    }, []);

    // Sync dark mode toggle to editor
    useEffect(() =>
    {
        setEditorTheme((prev) => ({
            ...prev,
            config: {
                ...prev.config,
                algorithm: isDarkMode ? theme.darkAlgorithm : theme.defaultAlgorithm,
            },
        }));
    }, [ isDarkMode ]);

    // Warn user before leaving page with unsaved changes
    useEffect(() =>
    {
        const handleBeforeUnload = (e: BeforeUnloadEvent): void =>
        {
            if (hasUnsavedChanges)
            {
                e.preventDefault();
            }
        };

        window.addEventListener("beforeunload", handleBeforeUnload);
        return () =>
        {
            window.removeEventListener("beforeunload", handleBeforeUnload);
        };
    }, [ hasUnsavedChanges ]);

    const handleThemeChange = useCallback((newTheme: AntdTheme): void =>
    {
        setEditorTheme(newTheme);
        setHasUnsavedChanges(true);
    }, []);

    const handleSave = useCallback(async (): Promise<void> =>
    {
        const tokens = editorTheme.config.token ?? {};
        setCustomTokens(tokens);

        // Detect dark/light mode from algorithm
        const algorithm = editorTheme.config.algorithm;
        const isNowDark = Array.isArray(algorithm)
            ? algorithm.includes(theme.darkAlgorithm)
            : algorithm === theme.darkAlgorithm;

        if (isNowDark !== isDarkMode)
        {
            toggleMode();
        }

        originalThemeRef.current = {
            tokens: { ...tokens },
            isDarkMode: isNowDark,
        };

        const themeToSave: SavedThemeData = { tokens, isCompact, isHappyWork };

        GlobalSpinner.show();

        try
        {
            const response = await ThemeService.saveTheme({
                Theme_Json: JSON.stringify(themeToSave),
            });

            if (response.Code === ApiResponseCode.Success)
            {
                setHasUnsavedChanges(false);
                ModalDialog.successResult("Theme Saved | Your theme changes have been applied and saved to your profile.");
            }
            else
            {
                setHasUnsavedChanges(false);
                ModalDialog.warning({
                    title: "Theme Applied Locally",
                    content: `Theme applied to this session but failed to save to server. ${response.Message}`,
                });
            }
        }
        catch
        {
            setHasUnsavedChanges(false);
            ModalDialog.warning({
                title: "Theme Applied Locally",
                content: "Theme applied to this session but failed to save to server.",
            });
        }
        finally
        {
            GlobalSpinner.hide();
        }
    }, [ editorTheme, isDarkMode, isCompact, isHappyWork, setCustomTokens, toggleMode ]);

    const handleReset = useCallback((): void =>
    {
        ModalDialog.confirm({
            title: "Reset Theme to Defaults?",
            content: "This will reset all theme settings to their default values. This action cannot be undone.",
            okText: "Reset",
            cancelText: "Cancel",
            okButtonProps: { danger: true },
            onOk: () =>
            {
                const resetTheme: AntdTheme = {
                    name: "Custom Theme",
                    key: "citl-theme",
                    config: {
                        token: {},
                        algorithm: isDarkMode ? theme.darkAlgorithm : theme.defaultAlgorithm,
                    },
                };
                setEditorTheme(resetTheme);
                setHasUnsavedChanges(true);
                ModalDialog.info({
                    title: "Theme Reset",
                    content: "Theme has been reset to defaults. Click 'Save Theme' to apply the changes.",
                });
            },
        });
    }, [ isDarkMode ]);

    const handleFontChange = useCallback((value: string): void =>
    {
        setEditorTheme((prev) => ({
            ...prev,
            config: {
                ...prev.config,
                token: {
                    ...prev.config.token,
                    fontFamily: value,
                },
            },
        }));
        setHasUnsavedChanges(true);
    }, []);

    return (
        <div style={{ height: "calc(100vh - 120px)", display: "flex", flexDirection: "column" }}>
            {/* Header */}
            <Card style={{ marginBottom: token.marginMD, flexShrink: 0 }}>
                <Row justify="space-between" align="middle">
                    <Col>
                        <Space orientation="vertical" size={0}>
                            <Space size="small">
                                <Typography.Title level={3} style={{ margin: 0 }}>
                                    <SettingOutlined style={{ marginRight: 8 }} />
                                    Theme Editor
                                </Typography.Title>
                                {hasUnsavedChanges && (
                                    <Tag color="warning">Unsaved Changes</Tag>
                                )}
                            </Space>
                            <Typography.Text type="secondary">
                                Customize your application&apos;s appearance with the Theme Editor
                            </Typography.Text>
                        </Space>
                    </Col>
                    <Col>
                        <Space>
                            <Tooltip title="Toggle Dark/Light Mode">
                                <Button
                                    icon={isDarkMode ? <SunOutlined /> : <MoonOutlined />}
                                    onClick={toggleMode}
                                >
                                    {isDarkMode ? "Light Mode" : "Dark Mode"}
                                </Button>
                            </Tooltip>
                            <Button icon={<UndoOutlined />} onClick={handleReset}>
                                Reset
                            </Button>
                            <Button type="primary" icon={<SaveOutlined />} onClick={() =>
                            {
                                void handleSave();
                            }}>
                                Save Theme
                            </Button>
                        </Space>
                    </Col>
                </Row>

                {/* Font Selector */}
                <Divider style={{ margin: "12px 0" }} />
                <Row gutter={16} align="middle">
                    <Col span={4}>
                        <Typography.Text strong>Font Family:</Typography.Text>
                    </Col>
                    <Col span={20}>
                        <Select
                            style={{ width: "100%", maxWidth: 400 }}
                            value={editorTheme.config.token?.fontFamily ?? ThemeConstants.FixedTokens.fontFamily}
                            onChange={handleFontChange}
                            options={fontOptions}
                        />
                    </Col>
                </Row>
                <Row gutter={16} align="middle" style={{ marginTop: 12 }}>
                    <Col>
                        <Space size="large">
                            <Space size="small">
                                <CompressOutlined />
                                <Typography.Text strong>Compact Mode</Typography.Text>
                                <Switch size="small" checked={isCompact} onChange={setCompact} />
                            </Space>
                            <Space size="small">
                                <SmileOutlined />
                                <Typography.Text strong>Happy Work Effect</Typography.Text>
                                <Switch size="small" checked={isHappyWork} onChange={setHappyWork} />
                            </Space>
                        </Space>
                    </Col>
                </Row>
            </Card>

            {/* Theme Editor */}
            <div style={{ flex: 1, minHeight: 0, overflow: "hidden" }}>
                <Suspense fallback={
                    <Card style={{ height: "100%" }}>
                        <Skeleton active paragraph={{ rows: 15 }} />
                    </Card>
                }>
                    <ConfigProvider
                        theme={{
                            algorithm: theme.defaultAlgorithm,
                            token: editorTheme.config.token ?? {},
                        }}
                    >
                        <AntdThemeEditor
                            advanced={isAdvanced}
                            hideAdvancedSwitcher={false}
                            onAdvancedChange={setIsAdvanced}
                            theme={editorTheme}
                            darkAlgorithm={theme.darkAlgorithm}
                            mode={editorMode}
                            onModeChange={setEditorMode}
                            style={{
                                height: "100%",
                                borderRadius: token.borderRadiusLG,
                                overflow: "hidden",
                            }}
                            onThemeChange={handleThemeChange}
                            locale={enUS}
                        />
                    </ConfigProvider>
                </Suspense>
            </div>

            {/* Info Card */}
            <Card style={{ marginTop: token.marginMD, flexShrink: 0 }}>
                <Row gutter={[ 24, 8 ]} align="middle">
                    <Col xs={24} sm={12} md={4}>
                        <Space size={8}>
                            <InfoCircleOutlined style={{ color: token.colorPrimary }} />
                            <Typography.Text type="secondary">Preview before saving</Typography.Text>
                        </Space>
                    </Col>
                    <Col xs={24} sm={12} md={4}>
                        <Space size={8}>
                            {isDarkMode
                                ? <MoonOutlined style={{ color: token.colorPrimary }} />
                                : <SunOutlined style={{ color: token.colorWarning }} />
                            }
                            <Tag color={isDarkMode ? "purple" : "gold"} style={{ margin: 0 }}>
                                {isDarkMode ? "Dark Mode" : "Light Mode"}
                            </Tag>
                        </Space>
                    </Col>
                    <Col xs={24} sm={12} md={4}>
                        <Space size={8}>
                            <CompressOutlined style={{ color: token.colorPrimary }} />
                            <Tag color={isCompact ? "blue" : "default"} style={{ margin: 0 }}>
                                {isCompact ? "Compact" : "Default Size"}
                            </Tag>
                        </Space>
                    </Col>
                    <Col xs={24} sm={12} md={4}>
                        <Space size={8}>
                            <SmileOutlined style={{ color: token.colorPrimary }} />
                            <Tag color={isHappyWork ? "green" : "default"} style={{ margin: 0 }}>
                                {isHappyWork ? "Happy Work" : "Standard"}
                            </Tag>
                        </Space>
                    </Col>
                    <Col xs={24} sm={12} md={4}>
                        <Space size={8}>
                            <EditOutlined style={{ color: token.colorPrimary }} />
                            <Typography.Text type="secondary" ellipsis style={{ maxWidth: 180 }}>
                                {(editorTheme.config.token?.fontFamily ?? ThemeConstants.FixedTokens.fontFamily).replace(/'/g, "").split(",")[0]}
                            </Typography.Text>
                        </Space>
                    </Col>
                    <Col xs={24} sm={12} md={4}>
                        <Space size={8}>
                            <SettingOutlined style={{ color: token.colorPrimary }} />
                            <div
                                style={{
                                    width: 16,
                                    height: 16,
                                    borderRadius: 4,
                                    backgroundColor: editorTheme.config.token?.colorPrimary ?? token.colorPrimary,
                                    border: `1px solid ${token.colorBorder}`,
                                }}
                            />
                            <Typography.Text type="secondary">Primary Color</Typography.Text>
                        </Space>
                    </Col>
                </Row>
            </Card>
        </div>
    );
}
