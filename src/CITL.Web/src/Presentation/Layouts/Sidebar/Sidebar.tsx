import { memo, useCallback, useEffect, useMemo, useState } from "react";
import { Avatar, Button, Drawer, Layout, Menu, Space, Spin, Typography, theme as antTheme, type MenuProps } from "antd";
import type { ItemType } from "antd/es/menu/interface";
import { CloseOutlined, DatabaseFilled, MinusOutlined, RightOutlined } from "@ant-design/icons";
import { useLocation, useNavigate } from "react-router-dom";
import { useMenuStore, useCompanyStore, useThemeStore } from "@/Application/Index";
import type { Menu as MenuType, BranchInfo } from "@/Domain/Index";
import { buildLookupMaps, QuerySensitivePages, type MenuLookupMaps } from "./MenuLookupMaps";
import c from "./Sidebar.module.css";

// ── Constants ─────────────────────────────────────────────────────
const SiderWidth = 300;
const SiderCollapsedWidth = 80;
const DrawerWidth = 300;

const IconStyleDefault: React.CSSProperties = { fontSize: 14, width: 18 };

// ── Build AntD menu items from tree ───────────────────────────────
function buildMenuItems(menus: MenuType[], lookupMaps: MenuLookupMaps): ItemType[]
{
    const build = (items: MenuType[]): ItemType[] =>
        items.map((menu) =>
        {
            const hasKids = lookupMaps.hasChildren.has(menu.MENU_ID);
            const icon = hasKids
                ? <RightOutlined style={IconStyleDefault} />
                : <MinusOutlined style={IconStyleDefault} />;

            return {
                key: menu.MENU_ID,
                icon,
                label: menu.MENU_Name,
                children: hasKids && menu.Children.length > 0 ? build(menu.Children) : undefined,
            };
        });

    return build(menus);
}

// ── Props ─────────────────────────────────────────────────────────
interface SidebarProps
{
    collapsed: boolean;
    isMobile: boolean;
    mobileDrawerOpen: boolean;
    onClose: () => void;
    activeBranch: BranchInfo | null;
}

// ── Component ─────────────────────────────────────────────────────
function SidebarInner({ collapsed, isMobile, mobileDrawerOpen, onClose, activeBranch }: SidebarProps): React.ReactNode
{
    const { token } = antTheme.useToken();
    const { isDarkMode } = useThemeStore();
    const navigate = useNavigate();
    const location = useLocation();

    const { menus, isLoaded } = useMenuStore();
    const { shortName } = useCompanyStore();

    // ── Lookup maps ───────────────────────────────────────────────
    const lookupMaps = useMemo(() => buildLookupMaps(menus), [ menus ]);

    const [ openKeys, setOpenKeys ] = useState<string[]>([]);
    const [ selectedKey, setSelectedKey ] = useState<string | undefined>();

    const branchShortName = activeBranch?.BRANCH_Name ?? "";

    // ── URL helpers ───────────────────────────────────────────────
    const normalizeMenuPath = useCallback((menuUrl?: string | null): string =>
    {
        if (!menuUrl)
        {
            return "";
        }
        const base = menuUrl.split("?")[0] ?? "";
        const withSlash = base.startsWith("/") ? base : `/${base}`;
        return (withSlash !== "/" ? withSlash.replace(/\/+$/, "") : withSlash).toLowerCase();
    }, []);

    const getQString = useCallback((url?: string | null): string | null =>
    {
        if (!url)
        {
            return null;
        }
        const sp = new URLSearchParams(url.split("?")[1]);
        const v = sp.get("QString1");
        return v ? v.toLowerCase() : null;
    }, []);

    // ── Auto-select active route ──────────────────────────────────
    useEffect(() =>
    {
        if (!isLoaded || menus.length === 0)
        {
            return;
        }

        const target = normalizeMenuPath(location.pathname);
        const searchParams = new URLSearchParams(location.search);
        const qs1 = searchParams.get("QString1")?.toLowerCase() ?? null;
        const isSensitive = QuerySensitivePages.includes(target);

        let matchedKey: string | undefined;
        let matchedAncestors: string[] = [];

        for (const [ key, menu ] of lookupMaps.byId)
        {
            const p = normalizeMenuPath(menu.MENU_URL1);
            if (p === target)
            {
                if (isSensitive)
                {
                    const mqs = getQString(menu.MENU_URL1);
                    if (mqs && qs1 && mqs === qs1)
                    {
                        matchedKey = key;
                        matchedAncestors = lookupMaps.ancestorChain.get(key) ?? [];
                        break;
                    }
                }
                else
                {
                    matchedKey = key;
                    matchedAncestors = lookupMaps.ancestorChain.get(key) ?? [];
                    break;
                }
            }
        }

        queueMicrotask(() =>
        {
            setSelectedKey(matchedKey);
            setOpenKeys(matchedAncestors);
        });
    }, [ isLoaded, location.pathname, location.search, menus, normalizeMenuPath, getQString, lookupMaps ]);

    // ── Restore open keys on expand ───────────────────────────────
    useEffect(() =>
    {
        if (!isMobile && !collapsed && selectedKey && openKeys.length === 0)
        {
            const ancestors = lookupMaps.ancestorChain.get(selectedKey) ?? [];
            if (ancestors.length > 0)
            {
                queueMicrotask(() =>
                {
                    setOpenKeys(ancestors);
                });
            }
        }
    }, [ collapsed, isMobile, selectedKey, openKeys.length, lookupMaps ]);

    // ── Accordion submenu toggle ──────────────────────────────────
    const handleOpenChange = useCallback<Required<MenuProps>["onOpenChange"]>(
        (keys) =>
        {
            const newlyOpened = keys.find((k) => !openKeys.includes(k));

            if (newlyOpened)
            {
                const newParent = lookupMaps.parentOf.get(newlyOpened);
                const filtered = keys.filter((k) =>
                {
                    const kParent = lookupMaps.parentOf.get(k);
                    return kParent !== newParent || k === newlyOpened ||
                        (lookupMaps.ancestorChain.get(newlyOpened) ?? []).includes(k);
                });
                setOpenKeys([ ...new Set(filtered) ]);
            }
            else
            {
                setOpenKeys(keys);
            }
        },
        [ openKeys, lookupMaps ],
    );

    // ── Menu click ────────────────────────────────────────────────
    const handleMenuClick = useCallback<Required<MenuProps>["onClick"]>(
        (info) =>
        {
            const menu = lookupMaps.byId.get(info.key);

            if (menu?.MENU_URL1)
            {
                const [ path ] = menu.MENU_URL1.split("?");

                if (path)
                {
                    setSelectedKey(info.key);
                    void navigate(menu.MENU_URL1);

                    if (isMobile)
                    {
                        onClose();
                    }
                }
            }
        },
        [ lookupMaps.byId, navigate, isMobile, onClose ],
    );

    // ── Build menu items ──────────────────────────────────────────
    const menuItems = useMemo((): ItemType[] =>
    {
        if (!isLoaded || menus.length === 0)
        {
            return [];
        }
        return buildMenuItems(menus, lookupMaps);
    }, [ menus, isLoaded, lookupMaps ]);

    // ── Loading state ─────────────────────────────────────────────
    if (!isLoaded)
    {
        return isMobile ? null : (
            <Layout.Sider
                width={SiderWidth}
                collapsed={collapsed}
                collapsedWidth={SiderCollapsedWidth}
                trigger={null}
                className={c["sider"]}
                style={{
                    background: isDarkMode ? token.colorBgContainer : token.colorBgElevated,
                    borderRight: `1px solid ${token.colorBorder}`,
                }}
            >
                <div className={c["loading"]}>
                    <Spin size="large" />
                </div>
            </Layout.Sider>
        );
    }

    // ── Shared menu content ───────────────────────────────────────
    const MenuContent = (
        <Menu
            mode="inline"
            selectedKeys={selectedKey ? [ selectedKey ] : []}
            openKeys={openKeys}
            onOpenChange={handleOpenChange}
            onClick={handleMenuClick}
            items={menuItems}
            inlineIndent={24}
            style={{ borderRight: 0, background: "transparent" }}
        />
    );

    // ── Mobile drawer ─────────────────────────────────────────────
    if (isMobile)
    {
        return (
            <Drawer
                placement="left"
                onClose={onClose}
                open={mobileDrawerOpen}
                closable={false}
                mask={{ closable: true }}
                keyboard
                destroyOnHidden={false}
                styles={{
                    body: {
                        padding: 0,
                        background: isDarkMode ? token.colorBgContainer : token.colorBgElevated,
                        display: "flex",
                        flexDirection: "column",
                        height: "100%",
                        overflow: "hidden",
                    },
                    wrapper: { width: DrawerWidth },
                }}
            >
                <div className={c["drawerHeader"]} style={{ borderBottom: `1px solid ${token.colorBorder}` }}>
                    <Space>
                        <Avatar
                            size={32}
                            style={{ backgroundColor: token.colorPrimary }}
                            icon={<DatabaseFilled />}
                        />
                        <Typography.Title level={5} style={{ margin: 0, fontSize: 14, fontWeight: 700 }}>
                            {branchShortName || shortName}
                        </Typography.Title>
                    </Space>
                    <Button
                        type="text"
                        icon={<CloseOutlined />}
                        onClick={onClose}
                        aria-label="Close navigation menu"
                        style={{ zIndex: 10 }}
                    />
                </div>
                <div className={c["menuWrapper"]}>
                    {MenuContent}
                </div>
            </Drawer>
        );
    }

    // ── Desktop sider ─────────────────────────────────────────────
    return (
        <Layout.Sider
            width={SiderWidth}
            collapsed={collapsed}
            collapsedWidth={SiderCollapsedWidth}
            trigger={null}
            className={c["sider"]}
            style={{
                background: isDarkMode ? token.colorBgContainer : token.colorBgElevated,
                borderRight: `1px solid ${token.colorBorder}`,
            }}
        >
            <div className={collapsed ? c["logoCollapsed"] : c["logo"]} style={{ borderBottom: `1px solid ${token.colorBorder}` }}>
                <Avatar
                    size={collapsed ? 32 : 40}
                    style={{ backgroundColor: token.colorPrimary }}
                    icon={<DatabaseFilled style={{ fontSize: collapsed ? 16 : 20 }} />}
                />
                {!collapsed && (
                    <div>
                        <Typography.Title level={5} style={{ margin: 0, fontSize: 14, fontWeight: 700 }}>
                            {branchShortName || `${shortName} ERP`}
                        </Typography.Title>
                        <Typography.Text type="secondary" style={{ fontSize: 11 }}>
                            {activeBranch?.BRANCH_Name ?? "Enterprise System"}
                        </Typography.Text>
                    </div>
                )}
            </div>
            <div className={c["menuWrapper"]}>
                {MenuContent}
            </div>
        </Layout.Sider>
    );
}

const Sidebar = memo(SidebarInner);
export default Sidebar;
