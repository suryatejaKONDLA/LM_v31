import { useEffect, useState } from "react";
import { Navigate, Outlet } from "react-router-dom";
import { Spin } from "antd";
import { useMenuStore, useCompanyStore, useAuthStore, useThemeStore } from "@/Application/Index";
import { AuthStorage, AuthService, MenuService, CompanyMasterService, ThemeService, TokenRefreshManager } from "@/Infrastructure/Index";
import { ApiResponseCode } from "@/Shared/Index";

/**
 * Route guard that requires authentication.
 * On page refresh, Zustand stores are lost but JWT persists in localStorage.
 * Restores user session via refresh-token, then fetches menus + company.
 */
export default function ProtectedRoute(): React.JSX.Element
{
    const isAuthenticated = AuthStorage.isAuthenticated();
    const { menus, isLoaded, setMenus, clearMenus } = useMenuStore();
    const { initialized: companyInitialized, setCompany } = useCompanyStore();
    const { user, setUser } = useAuthStore();
    const setCustomTokens = useThemeStore((s) => s.setCustomTokens);
    const setCompact = useThemeStore((s) => s.setCompact);
    const setHappyWork = useThemeStore((s) => s.setHappyWork);

    const [ loading, setLoading ] = useState(false);

    useEffect(() =>
    {
        const state = { cancelled: false };

        const allLoaded = user !== null && isLoaded && companyInitialized;

        // Skip if stores already populated (login pre-loaded them)
        if (allLoaded)
        {
            TokenRefreshManager.start();
            return;
        }

        if (!isAuthenticated)
        {
            clearMenus();
            return;
        }

        const initStores = async (): Promise<void> =>
        {
            setLoading(true);

            try
            {
                // Step 1: Restore user session if auth store is empty (F5 scenario)
                let loginId = user?.loginId;

                if (!loginId)
                {
                    const refreshToken = AuthStorage.getRefreshToken();
                    const loginUser = AuthStorage.getLoginUser();

                    if (!refreshToken || !loginUser)
                    {
                        // Can't restore session — force re-login
                        AuthStorage.clear();
                        window.location.href = `${import.meta.env.BASE_URL}Login`;
                        return;
                    }

                    const refreshResult = await AuthService.refreshToken({
                        Refresh_Token: refreshToken,
                        Login_User: loginUser,
                    });

                    if (state.cancelled)
                    {
                        return;
                    }

                    if (refreshResult.Code !== ApiResponseCode.Success)
                    {
                        AuthStorage.clear();
                        window.location.href = `${import.meta.env.BASE_URL}Login`;
                        return;
                    }

                    const res = refreshResult.Data;

                    // Update tokens
                    AuthStorage.setTokens(res.Access_Token, res.Refresh_Token);

                    // Restore auth store
                    setUser({
                        loginId: res.Login_Id,
                        loginUser: res.Login_User,
                        loginName: res.Login_Name,
                        roles: res.Roles,
                        branches: res.Branches,
                    });

                    loginId = res.Login_Id;
                }

                if (state.cancelled)
                {
                    return;
                }

                // Step 2: Fetch menus + company + theme in parallel
                const needsMenus = !isLoaded || menus.length === 0;
                const needsCompany = !companyInitialized;

                const [ menuResult, companyResult, themeResult ] = await Promise.all([
                    needsMenus ? MenuService.getMenus(loginId, true) : Promise.resolve(null),
                    needsCompany ? CompanyMasterService.get() : Promise.resolve(null),
                    ThemeService.getTheme(),
                ]);

                // eslint-disable-next-line @typescript-eslint/no-unnecessary-condition -- mutated by cleanup
                if (state.cancelled)
                {
                    return;
                }

                if (menuResult?.Code === ApiResponseCode.Success)
                {
                    setMenus(menuResult.Data);
                }
                else if (needsMenus)
                {
                    setMenus([]);
                }

                if (companyResult?.Code === ApiResponseCode.Success)
                {
                    setCompany(companyResult.Data);
                }

                if (themeResult.Code === ApiResponseCode.Success && themeResult.Data.Theme_Json)
                {
                    try
                    {
                        const parsed = JSON.parse(themeResult.Data.Theme_Json) as { tokens?: Record<string, unknown>; isCompact?: boolean; isHappyWork?: boolean };
                        if (parsed.tokens)
                        {
                            setCustomTokens(parsed.tokens);
                        }
                        if (parsed.isCompact !== undefined)
                        {
                            setCompact(parsed.isCompact);
                        }
                        if (parsed.isHappyWork !== undefined)
                        {
                            setHappyWork(parsed.isHappyWork);
                        }
                    }
                    catch
                    { /* invalid JSON — keep current theme */ }
                }
            }
            catch
            {
                if (!state.cancelled)
                {
                    // Session restoration failed — force re-login
                    AuthStorage.clear();
                    window.location.href = `${import.meta.env.BASE_URL}Login`;
                }
            }
            finally
            {
                if (!state.cancelled)
                {
                    setLoading(false);
                    TokenRefreshManager.start();
                }
            }
        };

        void initStores();

        return () =>
        {
            state.cancelled = true;
            TokenRefreshManager.stop();
        };
    }, [ isAuthenticated, isLoaded, companyInitialized, user, menus.length, setUser, setMenus, clearMenus, setCompany, setCustomTokens, setCompact, setHappyWork ]);

    if (!isAuthenticated)
    {
        return <Navigate to="/Login" replace />;
    }

    if (loading)
    {
        return (
            <div style={{ display: "flex", height: "100vh", width: "100%", alignItems: "center", justifyContent: "center" }}>
                <Spin size="large" />
            </div>
        );
    }

    return <Outlet />;
}
