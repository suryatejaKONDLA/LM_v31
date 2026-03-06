import { memo, useCallback, useMemo } from "react";
import { Button, Dropdown, Tooltip, theme, type MenuProps } from "antd";
import {
    MenuFoldOutlined,
    MenuUnfoldOutlined,
    MenuOutlined,
    SearchOutlined,
    UserOutlined,
    LockOutlined,
    LogoutOutlined,
    DownOutlined,
    BgColorsOutlined,
    HeartOutlined,
    ClockCircleOutlined,
} from "@ant-design/icons";
import { useGuardedNavigate, useAuthStore, useThemeStore } from "@/Application/Index";
import { ThemeToggleButton } from "@/Presentation/Controls/Index";
import { AuthStorage, TokenRefreshManager } from "@/Infrastructure/Index";
import type { BranchInfo } from "@/Domain/Index";
import c from "./Header.module.css";

interface HeaderProps
{
    collapsed: boolean;
    onToggle: () => void;
    isMobile: boolean;
    onSearchClick: () => void;
    activeBranch: BranchInfo | null;
    onBranchSelect: (branch: BranchInfo) => void;
}

function HeaderInner({ collapsed, onToggle, isMobile, onSearchClick, activeBranch, onBranchSelect }: HeaderProps): React.ReactNode
{
    const { token } = theme.useToken();
    const { isDarkMode } = useThemeStore();
    const navigate = useGuardedNavigate();
    const user = useAuthStore((s) => s.user);

    const branches = useMemo(() => user?.branches ?? [], [ user?.branches ]);

    // ── Branch menu items ─────────────────────────────────────────
    const branchMenuItems = useMemo(() =>
        branches.map((b: BranchInfo) => ({
            key: b.BRANCH_Code.toString(),
            label: b.BRANCH_Name,
        })),
    [ branches ],
    );

    const handleBranchSelect = useCallback<NonNullable<MenuProps["onClick"]>>(({ key }) =>
    {
        const selected = branches.find((b: BranchInfo) => b.BRANCH_Code.toString() === key);
        if (selected)
        {
            onBranchSelect(selected);
        }
    }, [ branches, onBranchSelect ]);

    // ── User menu ─────────────────────────────────────────────────
    const handleUserMenuClick = useCallback<NonNullable<MenuProps["onClick"]>>(({ key }) =>
    {
        if (key === "profile")
        {
            navigate("/Admin/Profile");
        }
        else if (key === "changePassword")
        {
            navigate("/Admin/ChangePassword");
        }
        else if (key === "themeEditor")
        {
            navigate("/Admin/ThemeEditor");
        }
        else if (key === "systemHealth")
        {
            navigate("/Admin/SystemHealth");
        }
        else if (key === "scheduler")
        {
            navigate("/Admin/Scheduler");
        }
        else if (key === "logout")
        {
            void (async () =>
            {
                try
                {
                    const { AuthService } = await import("@/Infrastructure/Index");
                    await AuthService.logout();
                }
                catch
                { /* continue */ }
                finally
                {
                    TokenRefreshManager.stop();
                    AuthStorage.clear();
                    sessionStorage.clear();
                    window.location.href = `${import.meta.env.BASE_URL}Login`;
                }
            })();
        }
    }, [ navigate ]);

    const userMenuItems = useMemo<MenuProps["items"]>(() =>
    {
        const items: NonNullable<MenuProps["items"]> = [
            { key: "profile", icon: <UserOutlined />, label: "Profile" },
            { key: "changePassword", icon: <LockOutlined />, label: "Change Password" },
            { key: "themeEditor", icon: <BgColorsOutlined />, label: "Theme Editor" },
        ];

        if (user?.loginId === 1)
        {
            items.push(
                { key: "systemHealth", icon: <HeartOutlined />, label: "System Health" },
                { key: "scheduler", icon: <ClockCircleOutlined />, label: "Scheduler" },
            );
        }

        items.push(
            { type: "divider" as const },
            { key: "logout", icon: <LogoutOutlined />, label: "Logout", danger: true },
        );

        return items;
    }, [ user?.loginId ]);

    const headerBg = isDarkMode ? token.colorBgContainer : token.colorBgElevated;

    return (
        <header
            className={c["header"]}
            style={{
                background: headerBg,
                borderBottom: `1px solid ${token.colorBorder}`,
            }}
        >
            {/* LEFT */}
            <div className={c["left"]}>
                {!isMobile && (
                    <Button
                        type="text"
                        icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
                        onClick={onToggle}
                        aria-label={collapsed ? "Expand sidebar" : "Collapse sidebar"}
                        aria-expanded={!collapsed}
                        style={{ fontSize: 18, width: 48, height: 48 }}
                    />
                )}
                {isMobile && (
                    <Button
                        type="text"
                        icon={<MenuOutlined />}
                        onClick={onToggle}
                        aria-label="Open navigation menu"
                        style={{ fontSize: 20, width: 40, height: 40 }}
                    />
                )}
            </div>

            {/* RIGHT */}
            <div className={c["right"]}>
                {/* Branch selector */}
                {branches.length > 1 && (
                    <Dropdown
                        menu={{
                            items: branchMenuItems,
                            selectable: true,
                            selectedKeys: activeBranch ? [ activeBranch.BRANCH_Code.toString() ] : [],
                            onClick: handleBranchSelect,
                        }}
                        placement="bottomRight"
                        trigger={[ "click" ]}
                    >
                        <Button
                            type="text"
                            style={{ fontSize: 14, fontWeight: 500, display: "flex", alignItems: "center", gap: 4 }}
                        >
                            {isMobile
                                ? (activeBranch?.BRANCH_Name.slice(0, 10) ?? "Branch")
                                : (activeBranch?.BRANCH_Name ?? "Select Branch")}
                            <DownOutlined style={{ fontSize: 12 }} />
                        </Button>
                    </Dropdown>
                )}

                {/* Search — desktop only */}
                {!isMobile && (
                    <Button
                        type="text"
                        icon={<SearchOutlined />}
                        aria-label="Search"
                        style={{ fontSize: 18 }}
                        onClick={onSearchClick}
                    />
                )}

                {/* Theme toggle */}
                <ThemeToggleButton size="medium" tooltipPlacement="bottomRight" />

                {/* User profile */}
                <Dropdown
                    menu={{ items: userMenuItems ?? [], onClick: handleUserMenuClick }}
                    trigger={[ "click" ]}
                >
                    <Tooltip title={`Hello, ${user?.loginName ?? "User"}`} placement="bottom">
                        <div
                            className={c["userAvatar"]}
                            role="button"
                            aria-label="User menu"
                            aria-haspopup="true"
                            tabIndex={0}
                            style={{ border: `1px solid ${token.colorBorder}` }}
                        >
                            <UserOutlined style={{ fontSize: 20, color: token.colorPrimary }} />
                        </div>
                    </Tooltip>
                </Dropdown>
            </div>
        </header>
    );
}

const Header = memo(HeaderInner);
export default Header;
