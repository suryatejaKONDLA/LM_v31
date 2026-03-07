import { useState, useEffect, useMemo, useCallback } from "react";
import {
    Card, Row, Col, Divider, Flex, Button, Checkbox, Empty, Badge, Spin, Result,
    Typography, Space, Switch, Tag, theme,
} from "antd";
import { SwapOutlined } from "@ant-design/icons";
import { useForm } from "react-hook-form";
import { useLocation } from "react-router-dom";
import { SearchBox, DropDown, type DropDownItem } from "@/Presentation/Controls/Index";
import { type MappingsRequest, type MappingsResponse } from "@/Domain/Index";
import { MappingsService, RoleMasterService } from "@/Infrastructure/Index";
import { V, ModalDialog } from "@/Shared/Index";

const { Title, Text } = Typography;

// ─── Mapping config ─────────────────────────────────────────────────────────

interface MappingConfig
{
    title: string;
    leftLabel: string;
    rightLabel: string;
    leftPlaceholder: string;
}

const DefaultMappingConfig: MappingConfig = {
    title: "Login Role Mapping",
    leftLabel: "Login",
    rightLabel: "Role",
    leftPlaceholder: "Select a login *",
};

const MappingConfigs: Record<string, MappingConfig> = {
    "010703": DefaultMappingConfig,
};

const isAnchorValid = (v: number | string | null | undefined): v is number | string =>
    v !== null && v !== undefined && v !== "" && v !== 0;

interface MappingFormValues
{
    anchorId: number | string | null;
}

// ─── Component ───────────────────────────────────────────────────────────────

export default function Mappings()
{
    const { token } = theme.useToken();
    const location = useLocation();

    // ─── Query string from URL (?QString1=010703) ───────────────────────────
    const queryString = useMemo(() =>
    {
        const params = new URLSearchParams(location.search);
        return params.get("QString1") ?? "010703";
    }, [ location.search ]);

    const config = useMemo(() => MappingConfigs[queryString] ?? DefaultMappingConfig, [ queryString ]);
    const isUnsupported = useMemo(() => !(queryString in MappingConfigs), [ queryString ]);

    // ─── State ───────────────────────────────────────────────────────────────
    const [ leftOptions, setLeftOptions ] = useState<DropDownItem[]>([]);
    const [ rightOptions, setRightOptions ] = useState<DropDownItem[]>([]);
    const [ checkedIds, setCheckedIds ] = useState<string[]>([]);
    const [ originalCheckedIds, setOriginalCheckedIds ] = useState<string[]>([]);
    const [ searchQuery, setSearchQuery ] = useState("");
    const [ swapFlag, setSwapFlag ] = useState(0);
    const [ loadingDropdowns, setLoadingDropdowns ] = useState(true);
    const [ loadingMappings, setLoadingMappings ] = useState(false);
    const [ submitting, setSubmitting ] = useState(false);

    // ─── Form ────────────────────────────────────────────────────────────────
    const { control, watch, reset } = useForm<MappingFormValues>({
        defaultValues: { anchorId: null },
        mode: "onBlur",
    });
    const selectedAnchorId = watch("anchorId");

    // ─── Dirty detection ─────────────────────────────────────────────────────
    const isDirty = useMemo(() =>
    {
        if (!isAnchorValid(selectedAnchorId))
        {
            return false;
        }
        if (checkedIds.length !== originalCheckedIds.length)
        {
            return true;
        }
        const a = [ ...checkedIds ].map(String).sort();
        const b = [ ...originalCheckedIds ].map(String).sort();
        return !a.every((id, i) => id === b[i]);
    }, [ checkedIds, originalCheckedIds, selectedAnchorId ]);

    // ─── Filtered right options ──────────────────────────────────────────────
    const filteredRightOptions = useMemo(() =>
    {
        if (!searchQuery.trim())
        {
            return rightOptions;
        }
        const q = searchQuery.toLowerCase();
        return rightOptions.filter((o) => o.Col2.toLowerCase().includes(q));
    }, [ rightOptions, searchQuery ]);

    const totalItems = rightOptions.length;
    const selectedCount = checkedIds.length;

    // ─── Load dropdowns ──────────────────────────────────────────────────────
    const loadDropdowns = useCallback(async () =>
    {
        setLoadingDropdowns(true);
        try
        {
            if (queryString === "010703")
            {
                const [ loginRes, roleRes ] = await Promise.all([
                    MappingsService.getLoginDropDown(),
                    RoleMasterService.getDropDown(true),
                ]);
                const left = swapFlag === 0 ? loginRes.Data : roleRes.Data;
                const right = swapFlag === 0 ? roleRes.Data : loginRes.Data;
                setLeftOptions(left);
                setRightOptions(right);
            }
        }
        catch
        {
            // silent — dropdowns simply stay empty
        }
        finally
        {
            setLoadingDropdowns(false);
        }
    }, [ queryString, swapFlag ]);

    useEffect(() =>
    {
        void loadDropdowns();
    }, [ loadDropdowns ]);

    // ─── Reset on query string change ────────────────────────────────────────
    useEffect(() =>
    {
        reset({ anchorId: null });
        setCheckedIds([]);
        setOriginalCheckedIds([]);
        setSearchQuery("");
        setSwapFlag(0);
    }, [ queryString, reset ]);

    // ─── Fetch mappings on anchor change ─────────────────────────────────────
    const fetchMappings = useCallback(async (anchorId: number | string) =>
    {
        setLoadingMappings(true);
        try
        {
            const result = await MappingsService.getByQueryString(queryString, String(anchorId), swapFlag);
            const mapped = result.Data.map((m: MappingsResponse) => m.Right_Column);
            setCheckedIds(mapped);
            setOriginalCheckedIds(mapped);
        }
        catch
        {
            setCheckedIds([]);
            setOriginalCheckedIds([]);
        }
        finally
        {
            setLoadingMappings(false);
        }
    }, [ queryString, swapFlag ]);

    useEffect(() =>
    {
        if (isAnchorValid(selectedAnchorId))
        {
            void fetchMappings(selectedAnchorId);
        }
        else
        {
            setCheckedIds([]);
            setOriginalCheckedIds([]);
        }
    }, [ selectedAnchorId, fetchMappings ]);

    // ─── Checkbox handlers ───────────────────────────────────────────────────
    const handleToggle = useCallback((id: number | string, checked: boolean) =>
    {
        const str = String(id);
        setCheckedIds((prev) => checked ? [ ...prev, str ] : prev.filter((x) => x !== str));
    }, []);

    const handleCheckAll = useCallback(() =>
    {
        setCheckedIds(rightOptions.map((o) => String(o.Col1)));
    }, [ rightOptions ]);

    const handleUncheckAll = useCallback(() =>
    {
        setCheckedIds([]);
    }, []);

    // ─── Swap ────────────────────────────────────────────────────────────────
    const handleSwap = useCallback((checked: boolean) =>
    {
        setSwapFlag(checked ? 1 : 0);
        reset({ anchorId: null });
        setCheckedIds([]);
        setOriginalCheckedIds([]);
        setSearchQuery("");
    }, [ reset ]);

    // ─── Submit ──────────────────────────────────────────────────────────────
    const doSubmit = useCallback(async () =>
    {
        if (!isAnchorValid(selectedAnchorId))
        {
            return;
        }

        setSubmitting(true);
        try
        {
            const request: MappingsRequest = {
                queryString,
                swapFlag,
                anchorId: String(selectedAnchorId),
                mappingIds: checkedIds,
            };

            const result = await MappingsService.insert(request);

            ModalDialog.successResult(result.Message, () =>
            {
                reset({ anchorId: null });
                setCheckedIds([]);
                setOriginalCheckedIds([]);
            });
        }
        catch
        {
            ModalDialog.showResult("error", "An unexpected error occurred.");
        }
        finally
        {
            setSubmitting(false);
        }
    }, [ queryString, swapFlag, selectedAnchorId, checkedIds, reset ]);

    const handleSubmit = useCallback(async () =>
    {
        if (!isAnchorValid(selectedAnchorId))
        {
            ModalDialog.warning({
                title: "Validation Error",
                content: `Please select a ${swapFlag === 0 ? config.leftLabel.toLowerCase() : config.rightLabel.toLowerCase()} first.`,
            });
            return;
        }

        if (checkedIds.length === 0)
        {
            ModalDialog.confirm({
                title: "No Items Selected",
                content: `Are you sure you want to remove all ${swapFlag === 0 ? config.rightLabel.toLowerCase() : config.leftLabel.toLowerCase()} access?`,
                onOk: () => void doSubmit(),
            });
            return;
        }

        await doSubmit();
    }, [ selectedAnchorId, checkedIds, swapFlag, config, doSubmit ]);

    const handleReset = useCallback(() =>
    {
        ModalDialog.confirm({
            title: "Reset Changes",
            content: "Are you sure you want to reset all changes?",
            onOk: () =>
            {
                reset({ anchorId: null });
                setCheckedIds([]);
                setSearchQuery("");
            },
        });
    }, [ reset ]);

    // ─── Dynamic labels ───────────────────────────────────────────────────────
    const leftLabel = swapFlag === 0 ? config.leftLabel : config.rightLabel;
    const rightLabel = swapFlag === 0 ? config.rightLabel : config.leftLabel;
    const leftPlaceholder = swapFlag === 0 ? config.leftPlaceholder : `Select a ${config.rightLabel.toLowerCase()} *`;

    // ─── Render ───────────────────────────────────────────────────────────────────
    if (isUnsupported)
    {
        return (
            <Result
                status="warning"
                title="Mapping Type Not Configured"
                subTitle={`The mapping type “${queryString}” is not supported. Please check the URL and try again.`}
            />
        );
    }
    return (
        <div className="max-w-6xl mx-auto flex flex-col gap-4">
            <Card
                title={
                    <Flex gap={8} align="center" justify="space-between" style={{ width: "100%" }}>
                        <Space size="small">
                            <Title level={5} style={{ margin: 0 }}>{config.title}</Title>
                            {isDirty && <Tag color="warning">Unsaved Changes</Tag>}
                        </Space>
                        <Flex gap={8} align="center">
                            <Text type="secondary" style={{ fontSize: 13 }}>Swap</Text>
                            <Switch
                                checked={swapFlag === 1}
                                onChange={handleSwap}
                                checkedChildren={<SwapOutlined />}
                                unCheckedChildren={<SwapOutlined />}
                            />
                        </Flex>
                    </Flex>
                }
                variant="borderless"
                styles={{ body: { padding: "16px 20px" } }}
                className="shadow-sm"
            >
                <Row gutter={[ 24, 16 ]}>
                    {/* ─── Left panel: anchor + stats + actions ───────────── */}
                    <Col xs={24} md={8} lg={6}>
                        <Card
                            size="small"
                            style={{ background: token.colorBgLayout, borderRadius: token.borderRadiusLG }}
                            styles={{ body: { padding: 16 } }}
                        >
                            <div className="flex flex-col gap-6">
                                <DropDown
                                    name="anchorId"
                                    label={leftLabel}
                                    control={control}
                                    dataSource={leftOptions}
                                    validation={{ required: V.Required }}
                                    placeholder={leftPlaceholder}
                                    required
                                    disabled={submitting}
                                />

                                <Divider style={{ margin: "16px 0" }} />

                                {isAnchorValid(selectedAnchorId) && (
                                    <div style={{
                                        background: `linear-gradient(135deg, ${token.colorPrimaryBg} 0%, ${token.colorBgContainer} 100%)`,
                                        borderRadius: token.borderRadius,
                                        padding: 16,
                                        marginBottom: 16,
                                        border: `1px solid ${token.colorPrimaryBorder}`,
                                    }}>
                                        <Flex vertical gap={8} style={{ width: "100%" }}>
                                            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                                                <Text type="secondary" style={{ fontSize: 12 }}>Total {rightLabel}s</Text>
                                                <Badge count={totalItems} showZero style={{ backgroundColor: token.colorTextSecondary }} />
                                            </div>
                                            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                                                <Text type="secondary" style={{ fontSize: 12 }}>Selected</Text>
                                                <Badge count={selectedCount} showZero style={{ backgroundColor: token.colorSuccess }} />
                                            </div>
                                            <div style={{ marginTop: 8, height: 6, background: token.colorBgLayout, borderRadius: 3, overflow: "hidden" }}>
                                                <div style={{
                                                    width: `${String(totalItems > 0 ? Math.round((selectedCount / totalItems) * 100) : 0)}%`,
                                                    height: "100%",
                                                    background: `linear-gradient(90deg, ${token.colorPrimary}, ${token.colorSuccess})`,
                                                    borderRadius: 3,
                                                    transition: "width 0.3s ease",
                                                }} />
                                            </div>
                                        </Flex>
                                    </div>
                                )}

                                <Flex vertical gap={12} style={{ width: "100%" }}>
                                    <Button
                                        type="primary"
                                        onClick={() => void handleSubmit()}
                                        loading={submitting}
                                        disabled={!isAnchorValid(selectedAnchorId)}
                                        block
                                        size="middle"
                                    >
                                        SAVE
                                    </Button>
                                    <Button onClick={handleReset} disabled={submitting} block size="middle">
                                        RESET
                                    </Button>
                                </Flex>
                            </div>
                        </Card>
                    </Col>

                    {/* ─── Right panel: checkbox list ─────────────────────── */}
                    <Col xs={24} md={16} lg={18}>
                        <Card
                            size="small"
                            title={
                                <Flex gap={8} align="center" justify="space-between" style={{ width: "100%" }}>
                                    <Text strong>Select {rightLabel}s</Text>
                                    <Flex gap={8}>
                                        <Button
                                            type="text"
                                            size="small"
                                            onClick={handleCheckAll}
                                            disabled={!isAnchorValid(selectedAnchorId) || rightOptions.length === 0}
                                        >
                                            Select All
                                        </Button>
                                        <Button
                                            type="text"
                                            size="small"
                                            onClick={handleUncheckAll}
                                            disabled={!isAnchorValid(selectedAnchorId) || checkedIds.length === 0}
                                        >
                                            Clear All
                                        </Button>
                                    </Flex>
                                </Flex>
                            }
                            style={{ borderRadius: token.borderRadiusLG, minHeight: 400 }}
                            styles={{ body: { padding: 0 } }}
                        >
                            {/* Search */}
                            <div style={{
                                padding: "12px 16px",
                                borderBottom: `1px solid ${token.colorBorderSecondary}`,
                                background: token.colorBgLayout,
                            }}>
                                <SearchBox
                                    placeholder={`Search ${rightLabel.toLowerCase()}s...`}
                                    value={searchQuery}
                                    onChange={(v) =>
                                    {
                                        setSearchQuery(v);
                                    }}
                                    onClear={() =>
                                    {
                                        setSearchQuery("");
                                    }}
                                    maxWidth={400}
                                />
                            </div>

                            {/* Checkbox list */}
                            <div style={{
                                padding: 16,
                                maxHeight: "calc(100vh - 380px)",
                                minHeight: 300,
                                overflowY: "auto",
                            }}>
                                {loadingDropdowns ? (
                                    <div style={{ textAlign: "center", padding: 40 }}>
                                        <Text type="secondary">Loading...</Text>
                                    </div>
                                ) : filteredRightOptions.length === 0 ? (
                                    <Empty
                                        image={Empty.PRESENTED_IMAGE_SIMPLE}
                                        description={
                                            <Text type="secondary">
                                                {searchQuery
                                                    ? `No ${rightLabel.toLowerCase()}s match your search`
                                                    : `No ${rightLabel.toLowerCase()}s available`}
                                            </Text>
                                        }
                                        style={{ padding: 40 }}
                                    />
                                ) : (
                                    <Spin spinning={loadingMappings}>
                                        <div style={{ display: "flex", flexDirection: "column", gap: 4 }}>
                                            {filteredRightOptions.map((option, index) =>
                                            {
                                                const isChecked = checkedIds.includes(String(option.Col1));
                                                return (
                                                    <div
                                                        key={option.Col1}
                                                        onClick={() =>
                                                        {
                                                            handleToggle(option.Col1, !isChecked);
                                                        }}
                                                        style={{
                                                            display: "flex",
                                                            alignItems: "center",
                                                            padding: "12px 16px",
                                                            borderRadius: token.borderRadius,
                                                            background: index % 2 === 0 ? token.colorBgContainer : token.colorBgLayout,
                                                            border: `1px solid ${isChecked ? token.colorPrimary : "transparent"}`,
                                                            cursor: "pointer",
                                                            transition: "all 0.2s ease",
                                                            userSelect: "none",
                                                        }}
                                                        onMouseEnter={(e) =>
                                                        {
                                                            e.currentTarget.style.background = isChecked
                                                                ? token.colorPrimaryBg
                                                                : token.colorFillSecondary;
                                                            e.currentTarget.style.borderColor = token.colorPrimary;
                                                        }}
                                                        onMouseLeave={(e) =>
                                                        {
                                                            e.currentTarget.style.background = index % 2 === 0 ? token.colorBgContainer : token.colorBgLayout;
                                                            e.currentTarget.style.borderColor = isChecked ? token.colorPrimary : "transparent";
                                                        }}
                                                    >
                                                        <Checkbox checked={isChecked} style={{ pointerEvents: "none" }} />
                                                        <Text
                                                            strong={isChecked}
                                                            style={{
                                                                marginLeft: 12,
                                                                fontSize: 14,
                                                                color: isChecked ? token.colorPrimary : token.colorText,
                                                                flex: 1,
                                                            }}
                                                        >
                                                            {option.Col2}
                                                        </Text>
                                                    </div>
                                                );
                                            })}
                                        </div>
                                    </Spin>
                                )}
                            </div>
                        </Card>
                    </Col>
                </Row>
            </Card>
        </div>
    );
}
