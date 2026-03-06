import { Suspense, lazy } from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import { Spin } from "antd";
import { useDynamicRoutes } from "./useDynamicRoutes";

// Lazy-loaded route guards
const ProtectedRoute = lazy(() => import("@/Presentation/Routing/ProtectedRoute"));
const PublicOnlyRoute = lazy(() => import("@/Presentation/Routing/PublicOnlyRoute"));

// Lazy-loaded layouts
const MainLayout = lazy(() => import("@/Presentation/Layouts/MainLayout"));
const LoginLayout = lazy(() => import("@/Presentation/Layouts/LoginLayout"));
const NetworkErrorGuard = lazy(() => import("@/Presentation/Layouts/NetworkErrorGuard"));

// Lazy-loaded pages
const Login = lazy(() => import("@/Presentation/Pages/Authentication/Login"));
const Home = lazy(() => import("@/Presentation/Pages/Home"));
const Profile = lazy(() => import("@/Presentation/Pages/Administration/Profile"));
const ChangePassword = lazy(() => import("@/Presentation/Pages/Administration/ChangePassword"));
const ThemeEditor = lazy(() => import("@/Presentation/Pages/Administration/ThemeEditor"));
const SystemHealth = lazy(() => import("@/Presentation/Pages/Administration/SystemHealth"));
const SchedulerConfiguration = lazy(() => import("@/Presentation/Pages/Administration/SchedulerConfiguration"));
const NotFound = lazy(() => import("@/Presentation/Pages/ErrorPages"));
const Forbidden = lazy(() => import("@/Presentation/Pages/ErrorPages").then((m) => ({ default: m.Forbidden })));
const NetworkError = lazy(() => import("@/Presentation/Pages/NetworkError/NetworkError"));

const FallbackSpinner = <Spin size="large" style={{ display: "flex", justifyContent: "center", marginTop: "40vh" }} />;

/**
 * Application-level route definitions.
 * Layers: PublicOnlyRoute (login) | ProtectedRoute (authenticated) | standalone (errors).
 */
export default function AppRoutes(): React.JSX.Element
{
    const dynamicRoutes = useDynamicRoutes();

    return (
        <Suspense fallback={FallbackSpinner}>
            <Routes>
                {/* Redirect root to Login (unauthenticated) or Home */}
                <Route path="/" element={<Navigate to="/Login" replace />} />

                {/* Network Error — standalone, OUTSIDE guard to avoid redirect loops */}
                <Route path="/NetworkError" element={<NetworkError />} />

                {/* All other routes wrapped in NetworkErrorGuard (5s offline → /NetworkError) */}
                <Route element={<NetworkErrorGuard />}>
                    {/* Public-only routes — redirect to /Home if already authenticated */}
                    <Route element={<PublicOnlyRoute />}>
                        <Route element={<LoginLayout />}>
                            <Route path="/Login" element={<Login />} />
                            <Route path="/403" element={<Forbidden />} />
                        </Route>
                    </Route>

                    {/* Protected routes — redirect to /Login if not authenticated */}
                    <Route element={<ProtectedRoute />}>
                        <Route element={<MainLayout />}>
                            <Route path="/Home" element={<Home />} />
                            <Route path="/Admin/Profile" element={<Profile />} />
                            <Route path="/Admin/ChangePassword" element={<ChangePassword />} />
                            <Route path="/Admin/ThemeEditor" element={<ThemeEditor />} />
                            <Route path="/Admin/SystemHealth" element={<SystemHealth />} />
                            <Route path="/Admin/Scheduler" element={<SchedulerConfiguration />} />

                            {/* Dynamically generated routes from DB menus */}
                            {dynamicRoutes}

                            {/* 404 catch-all (must be last inside protected group) */}
                            <Route path="*" element={<NotFound />} />
                        </Route>
                    </Route>
                </Route>
            </Routes>
        </Suspense>
    );
}
