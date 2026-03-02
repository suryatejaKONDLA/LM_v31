import { Navigate, Outlet } from "react-router-dom";
import { AuthStorage } from "@/Infrastructure/Index";

/**
 * Route guard that only allows unauthenticated users.
 * Redirects authenticated users to /Home — prevents seeing login page when already logged in.
 */
export default function PublicOnlyRoute(): React.JSX.Element
{
    if (AuthStorage.isAuthenticated())
    {
        return <Navigate to="/Home" replace />;
    }

    return <Outlet />;
}
