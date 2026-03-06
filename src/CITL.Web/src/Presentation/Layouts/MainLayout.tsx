import { useCallback, useEffect, useState } from "react";
import { Outlet } from "react-router-dom";
import { Alert, Grid, Layout } from "antd";
import { SyncOutlined } from "@ant-design/icons";
import { useAuthStore, useBranchStore, useConnectivityStore, useGlobalShortcuts, useGuardedNavigate, useThemeShortcuts, useThemeStore } from "@/Application/Index";
import { SearchDialog, GlobalShortcutsHelp } from "@/Presentation/Controls/Index";
import Header from "./Header/Header";
import Footer from "./Footer/Footer";
import Sidebar from "./Sidebar/Sidebar";
import "@/Presentation/Layouts/CSSAnimations.css";

/**
 * Main application layout with header, sidebar, and content area.
 * Wraps all authenticated routes.
 */
export default function MainLayout(): React.JSX.Element
{
    const screens = Grid.useBreakpoint();
    const navigate = useGuardedNavigate();
    const user = useAuthStore((s) => s.user);
    const { toggleMode, setMode } = useThemeStore();
    const { hubStatus, networkOnline, apiReachable } = useConnectivityStore();

    const isReconnecting = hubStatus === "reconnecting"
        || (!networkOnline && hubStatus !== "connected")
        || (hubStatus === "disconnected" && !apiReachable);

    const isMobile = !screens.xl;

    const [ collapsed, setCollapsed ] = useState(false);
    const [ mobileDrawerOpen, setMobileDrawerOpen ] = useState(false);
    const [ isSearchOpen, setIsSearchOpen ] = useState(false);
    const activeBranch = useBranchStore((s) => s.activeBranch);
    const { setBranch: setActiveBranch, initBranch } = useBranchStore();

    useEffect(() =>
    {
        if (user?.branches.length)
        {
            initBranch(user.branches);
        }
    }, [ user?.branches, initBranch ]);

    const handleToggle = useCallback(() =>
    {
        if (isMobile)
        {
            setMobileDrawerOpen((prev) => !prev);
        }
        else
        {
            setCollapsed((prev) => !prev);
        }
    }, [ isMobile ]);

    const handleDrawerClose = useCallback(() =>
    {
        setMobileDrawerOpen(false);
    }, []);

    const toggleSearch = useCallback(() =>
    {
        setIsSearchOpen((prev) => !prev);
    }, []);

    const closeSearch = useCallback(() =>
    {
        setIsSearchOpen(false);
    }, []);

    const handleBranchSelect = useCallback((branch: { BRANCH_Code: number; BRANCH_Name: string }) =>
    {
        setActiveBranch(branch);
    }, [ setActiveBranch ]);

    // ── Keyboard shortcuts ────────────────────────────────────────
    useGlobalShortcuts({
        onHome: () => 
        {
            navigate("/Home"); 
        },
        onSearch: toggleSearch,
        onToggleSidebar: handleToggle,
        onBack: () => 
        {
            navigate(-1); 
        },
        onForward: () => 
        {
            navigate(1); 
        },
    });

    useThemeShortcuts({ toggleMode, setMode });

    return (
        <Layout style={{ minHeight: "100vh" }}>
            {/* SIDEBAR */}
            <div className="entrance-sidebar">
                <Sidebar
                    collapsed={isMobile ? false : collapsed}
                    isMobile={isMobile}
                    onClose={handleDrawerClose}
                    mobileDrawerOpen={mobileDrawerOpen}
                    activeBranch={activeBranch}
                />
            </div>

            {/* MAIN CONTENT AREA */}
            <Layout>
                <div className="entrance-header">
                    <Header
                        collapsed={collapsed}
                        onToggle={handleToggle}
                        isMobile={isMobile}
                        onSearchClick={toggleSearch}
                        activeBranch={activeBranch}
                        onBranchSelect={handleBranchSelect}
                    />
                </div>

                {isReconnecting && (
                    <Alert
                        type="warning"
                        banner
                        showIcon={false}
                        title={
                            <>
                                <SyncOutlined spin style={{ marginRight: 8 }} />
                                Reconnecting to the server — please wait...
                            </>
                        }
                    />
                )}

                <Layout.Content
                    style={{
                        margin: 8,
                        padding: 8,
                        minHeight: 280,
                        background: "transparent",
                        overflow: "hidden",
                        position: "relative",
                    }}
                >
                    <Outlet />
                </Layout.Content>

                <div className="entrance-footer">
                    <Footer />
                </div>
            </Layout>

            {/* SEARCH DIALOG */}
            <SearchDialog open={isSearchOpen} onClose={closeSearch} />

            {/* KEYBOARD SHORTCUTS HELP — opened via F1 or store */}
            <GlobalShortcutsHelp />
        </Layout>
    );
}
