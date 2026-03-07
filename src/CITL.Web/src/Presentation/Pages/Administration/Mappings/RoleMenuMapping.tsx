import React, { useState, useEffect, useMemo, useCallback, useRef } from "react";
import { Card, Tree, Empty, Typography, Space, Badge, Row, Col, Divider, Flex, theme, Button, Spin, type TreeDataNode } from "antd";
import { MenuOutlined } from "@ant-design/icons";
import { AxiosError } from "axios";
import { useMasterForm } from "@/Application/Hooks/useMasterForm";
import { SearchBox } from "@/Presentation/Controls/SearchBox/SearchBox";
import { DropDown } from "@/Presentation/Controls/DropDown/DropDown";
import {
    type RoleMenuMappingFormValues,
    type Menu,
    type RoleMenuMappingRequest,
    type RoleMenuMappingResponse,
    type DropDownItem,
} from "@/Domain/Index";
import { roleMenuMappingService, RoleMasterService, MenuService } from "@/Infrastructure/Index";
import { V } from "@/Shared/Helpers/ValidationMessages";
import { ApiResponseCode } from "@/Shared/Index";

const { Title, Text } = Typography;

// ============================================================
// Helpers
// ============================================================
function convertToTreeData(menus: Menu[], searchQuery: string, badgeColor: string): TreeDataNode[]
{
    const filterMenus = (items: Menu[]): Menu[] =>
    {
        if (!searchQuery.trim())
        {
            return items;
        }
        const q = searchQuery.toLowerCase();
        return items.reduce<Menu[]>((acc, item) =>
        {
            const matches = item.MENU_Name.toLowerCase().includes(q);
            const filteredChildren = filterMenus(item.Children);
            if (matches || filteredChildren.length > 0)
            {
                acc.push({ ...item, Children: filteredChildren.length > 0 ? filteredChildren : item.Children });
            }
            return acc;
        }, []);
    };

    const buildTree = (items: Menu[]): TreeDataNode[] =>
        items.map((menu): TreeDataNode =>
        {
            const children = menu.Children;
            const hasChildren = children.length > 0;
            return {
                key: menu.MENU_ID,
                title: (
                    <Space size={4}>
                        {menu.MENU_Icon1 && <span className={menu.MENU_Icon1} style={{ fontSize: 14, opacity: 0.8 }} />}
                        <span>{menu.MENU_Name}</span>
                        {hasChildren && <Badge count={children.length} size="small" style={{ backgroundColor: badgeColor, fontSize: 10, marginLeft: 4 }} />}
                    </Space>
                ),
                isLeaf: !hasChildren,
                ...(hasChildren ? { children: buildTree(children) } : {}),
            };
        });

    return buildTree(filterMenus(menus));
}

function getAllMenuIds(menus: Menu[]): string[]
{
    const ids: string[] = [];
    const traverse = (items: Menu[]) =>
    {
        items.forEach((m) =>
        {
            ids.push(m.MENU_ID);
            if (m.Children.length > 0)
            {
                traverse(m.Children);
            }
        });
    };
    traverse(menus);
    return ids;
}

function getAllParentKeys(menus: Menu[]): string[]
{
    const keys: string[] = [];
    const traverse = (items: Menu[]) =>
    {
        items.forEach((m) =>
        {
            if (m.Children.length > 0)
            {
                keys.push(m.MENU_ID);
                traverse(m.Children);
            }
        });
    };
    traverse(menus);
    return keys;
}

function getLeafMenuData(menus: Menu[], checkedKeys?: string[]): { ids: string[]; count: number; }
{
    const leafIds: string[] = [];
    const parentIds = new Set<string>();
    const traverse = (items: Menu[]) =>
    {
        items.forEach((m) =>
        {
            if (m.Children.length > 0)
            {
                parentIds.add(m.MENU_ID);
                traverse(m.Children);
            }
            else
            {
                leafIds.push(m.MENU_ID);
            }
        });
    };
    traverse(menus);
    if (checkedKeys)
    {
        const checkedLeafs = checkedKeys.filter((k) => !parentIds.has(k));
        return { ids: checkedLeafs, count: checkedLeafs.length };
    }
    return { ids: leafIds, count: leafIds.length };
}

// ============================================================
// Component
// ============================================================
export default function RoleMenuMapping()
{
    const { token } = theme.useToken();

    const [ roles, setRoles ] = useState<DropDownItem<number>[]>([]);
    const [ menus, setMenus ] = useState<Menu[]>([]);
    const menusRef = useRef<Menu[]>([]);          // stable ref — avoids fetchRoleMappings dep on menus state
    const [ searchQuery, setSearchQuery ] = useState("");
    const [ loadingMenus, setLoadingMenus ] = useState(false);
    const [ loadingMappings, setLoadingMappings ] = useState(false);
    const [ expandedKeys, setExpandedKeys ] = useState<React.Key[]>([]);
    const [ checkedKeys, setCheckedKeys ] = useState<React.Key[]>([]);
    const [ autoExpandParent, setAutoExpandParent ] = useState(true);

    const { form, submitting, handleFormSubmit } = useMasterForm<RoleMenuMappingFormValues, RoleMenuMappingResponse>({
        defaultValues: { roleId: undefined as unknown as number, menuIds: [] },
        pageTitle: "Role Menu Mapping",
        fetchById: () => Promise.resolve({ Code: ApiResponseCode.Success, Message: "", Data: {} as RoleMenuMappingResponse, Type: "success", Timestamp: new Date().toISOString() }),
        mapResponseToForm: () => ({ roleId: 0, menuIds: [] }),
        buildRequest: (values) => ({ roleId: values.roleId ?? 0, menuIds: values.menuIds }),
        buildBannerFields: () => [ null, null, null ],
        save: async (data: unknown) =>
        {
            const req = data as RoleMenuMappingRequest;
            await roleMenuMappingService.addOrUpdate(req);
            // Refresh checked keys after save
            await fetchRoleMappings(req.roleId);
            return { Code: ApiResponseCode.Success, Message: "Role-Menu mappings updated successfully!", Data: null, Type: "success", Timestamp: new Date().toISOString() };
        },
    });

    const { control, watch, reset } = form;
    const selectedRoleId = watch("roleId");

    // ─── Fetch mapped menu IDs for the selected role ────────────────────────
    const fetchRoleMappings = useCallback(async (roleId: number) =>
    {
        try
        {
            setLoadingMappings(true);
            const mappings = await roleMenuMappingService.getByRoleId(roleId);
            const mappingIds = mappings.map((m) => m.MENU_ID);
            // Use menusRef so this callback has no dependency on menus state
            const leaf = getLeafMenuData(menusRef.current, mappingIds);
            setCheckedKeys(leaf.ids);
            form.setValue("menuIds", leaf.ids, { shouldDirty: false });
        }
        catch (err)
        {
            if (err instanceof AxiosError && err.response?.status === 404)
            {
                setCheckedKeys([]);
                form.setValue("menuIds", [], { shouldDirty: false });
            }
            else
            {
                console.error("Failed to fetch role mappings", err);
            }
        }
        finally
        {
            setLoadingMappings(false);
        }
    }, [ form ]);

    // ─── On mount: load roles + all menus (once) ────────────────────────────
    useEffect(() =>
    {
        const init = async () =>
        {
            try
            {
                setLoadingMenus(true);
                const [ rolesRes, menusRes ] = await Promise.all([
                    RoleMasterService.getDropDown(),
                    MenuService.getMenus(1, true),
                ]);
                setRoles(rolesRes.Data);
                const fetchedMenus = menusRes.Data;
                menusRef.current = fetchedMenus;
                setMenus(fetchedMenus);
                setExpandedKeys(fetchedMenus.map((m) => m.MENU_ID));
            }
            catch (err)
            {
                console.error("Init failed", err);
            }
            finally
            {
                setLoadingMenus(false);
            }
        };
        void init();
    }, []);

    // ─── On role change: only fetch mapped items ────────────────────────────
    useEffect(() =>
    {
        if (!selectedRoleId)
        {
            setCheckedKeys([]);
            return;
        }
        void fetchRoleMappings(selectedRoleId);
    }, [ selectedRoleId, fetchRoleMappings ]);

    // ─── Memoised values ────────────────────────────────────────────────────
    const treeData = useMemo(
        () => convertToTreeData(menus, searchQuery, token.colorPrimary),
        [ menus, searchQuery, token.colorPrimary ],
    );
    const allMenuIds = useMemo(() => getAllMenuIds(menus), [ menus ]);
    const allParentKeys = useMemo(() => getAllParentKeys(menus), [ menus ]);
    const totalMenus = useMemo(() => getLeafMenuData(menus).count, [ menus ]);
    const selectedCount = useMemo(
        () => getLeafMenuData(menus, checkedKeys.map(String)).count,
        [ menus, checkedKeys ],
    );

    // ─── Tree handlers ───────────────────────────────────────────────────────
    const onExpand = (keys: React.Key[]) =>
    {
        setExpandedKeys(keys);
        setAutoExpandParent(false);
    };

    const onCheck = (checked: React.Key[] | { checked: React.Key[]; halfChecked: React.Key[] }) =>
    {
        const final = Array.isArray(checked)
            ? checked
            : [ ...checked.checked, ...checked.halfChecked ];
        setCheckedKeys(final);
        form.setValue("menuIds", final.map(String), { shouldDirty: true });
    };

    const handleCheckAll = useCallback(() =>
    {
        setCheckedKeys(allMenuIds);
        form.setValue("menuIds", allMenuIds, { shouldDirty: true });
    }, [ allMenuIds, form ]);

    const handleUncheckAll = useCallback(() =>
    {
        setCheckedKeys([]);
        form.setValue("menuIds", [], { shouldDirty: true });
    }, [ form ]);

    const handleExpandAll = useCallback(() =>
    {
        setExpandedKeys(allParentKeys);
        setAutoExpandParent(false);
    }, [ allParentKeys ]);

    const handleCollapseAll = useCallback(() =>
    {
        setExpandedKeys([]);
        setAutoExpandParent(false);
    }, []);

    const handleReset = () =>
    {
        reset({ roleId: undefined as unknown as number, menuIds: [] });
        setCheckedKeys([]);
        setSearchQuery("");
        setExpandedKeys(menus.map((m) => m.MENU_ID));
    };

    // ─── Render ──────────────────────────────────────────────────────────────
    return (
        <div className="max-w-6xl mx-auto flex flex-col gap-4">
            <Card
                title={
                    <Space size="small">
                        <Title level={5} style={{ margin: 0 }}>Role Menu Mapping</Title>
                    </Space>
                }
                variant="borderless"
                styles={{ body: { padding: "16px 20px" } }}
                className="shadow-sm"
            >
                <form onSubmit={(e) =>
                {
                    e.preventDefault(); handleFormSubmit();
                }}>
                    <Row gutter={[ 24, 16 ]}>
                        {/* ── Left: Role selection + stats + actions ── */}
                        <Col xs={24} md={8} lg={6}>
                            <Card
                                size="small"
                                style={{ background: token.colorBgLayout, borderRadius: token.borderRadiusLG }}
                                styles={{ body: { padding: 16 } }}
                            >
                                <div className="flex flex-col gap-6">
                                    <DropDown
                                        name="roleId"
                                        label="Select Role"
                                        control={control}
                                        dataSource={roles}
                                        validation={{ required: V.Required }}
                                        placeholder="Choose a role *"
                                        required
                                        disabled={submitting}
                                    />

                                    <Divider style={{ margin: "16px 0" }} />

                                    {(selectedRoleId ?? 0) > 0 && (
                                        <div style={{
                                            background: `linear-gradient(135deg, ${token.colorPrimaryBg} 0%, ${token.colorBgContainer} 100%)`,
                                            borderRadius: token.borderRadius,
                                            padding: 16,
                                            marginBottom: 16,
                                            border: `1px solid ${token.colorPrimaryBorder}`,
                                        }}>
                                            <Flex vertical gap={8} style={{ width: "100%" }}>
                                                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                                                    <Text type="secondary" style={{ fontSize: 12 }}>Total Menus</Text>
                                                    <Badge count={totalMenus} showZero style={{ backgroundColor: token.colorTextSecondary }} />
                                                </div>
                                                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                                                    <Text type="secondary" style={{ fontSize: 12 }}>Selected</Text>
                                                    <Badge count={selectedCount} showZero style={{ backgroundColor: token.colorSuccess }} />
                                                </div>
                                                <div style={{ marginTop: 8, height: 6, background: token.colorBgLayout, borderRadius: 3, overflow: "hidden" }}>
                                                    <div style={{
                                                        width: `${String(totalMenus > 0 ? Math.round((selectedCount / totalMenus) * 100) : 0)}%`,
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
                                        <Button type="primary" htmlType="submit" loading={submitting} disabled={!selectedRoleId} block size="middle">SAVE</Button>
                                        <Button onClick={handleReset} disabled={submitting} block size="middle">RESET</Button>
                                    </Flex>
                                </div>
                            </Card>
                        </Col>

                        {/* ── Right: Menu tree ── */}
                        <Col xs={24} md={16} lg={18}>
                            <Card
                                size="small"
                                title={
                                    <Flex gap={8} align="center">
                                        <MenuOutlined style={{ color: token.colorPrimary, fontSize: 18 }} />
                                        <Text strong>Menu Permissions</Text>
                                    </Flex>
                                }
                                extra={
                                    <Flex gap={8} wrap style={{ justifyContent: "flex-end" }}>
                                        <Button type="text" size="small" onClick={handleCheckAll} disabled={!selectedRoleId || allMenuIds.length === 0}>Select All</Button>
                                        <Button type="text" size="small" onClick={handleUncheckAll} disabled={!selectedRoleId || checkedKeys.length === 0}>Clear All</Button>
                                        <Button type="text" size="small" onClick={handleExpandAll} disabled={menus.length === 0}>Expand</Button>
                                        <Button type="text" size="small" onClick={handleCollapseAll} disabled={menus.length === 0}>Collapse</Button>
                                    </Flex>
                                }
                                style={{ borderRadius: token.borderRadiusLG, minHeight: 400 }}
                                styles={{ body: { padding: 0 } }}
                            >
                                <div style={{ padding: "12px 16px", borderBottom: `1px solid ${token.colorBorderSecondary}`, background: token.colorBgLayout }}>
                                    <SearchBox
                                        placeholder="Search menus..."
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

                                <div style={{ padding: 16, maxHeight: "calc(100vh - 380px)", minHeight: 300, overflowY: "auto" }}>
                                    {loadingMenus ? (
                                        <div style={{ textAlign: "center", padding: 40 }}>
                                            <Text type="secondary">Loading...</Text>
                                        </div>
                                    ) : treeData.length === 0 ? (
                                        <Empty
                                            image={Empty.PRESENTED_IMAGE_SIMPLE}
                                            description={<Text type="secondary">{searchQuery ? "No menus match your search" : "No menus available"}</Text>}
                                            style={{ padding: 40 }}
                                        />
                                    ) : (
                                        <Spin spinning={loadingMappings}>
                                            <Tree
                                                checkable
                                                selectable={false}
                                                checkedKeys={checkedKeys}
                                                expandedKeys={expandedKeys}
                                                autoExpandParent={autoExpandParent}
                                                onCheck={onCheck as (checked: React.Key[] | { checked: React.Key[]; halfChecked: React.Key[]; }) => void}
                                                onExpand={onExpand}
                                                treeData={treeData}
                                                showLine={{ showLeafIcon: false }}
                                                style={{ background: "transparent", fontSize: 14 }}
                                            />
                                        </Spin>
                                    )}
                                </div>
                            </Card>
                        </Col>
                    </Row>
                </form>
            </Card>
        </div>
    );
}
