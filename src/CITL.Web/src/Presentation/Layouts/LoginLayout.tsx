import { Outlet } from "react-router-dom";

/**
 * Minimal layout for login and unauthenticated pages.
 */
export default function LoginLayout(): React.JSX.Element
{
    return (
        <div style={{
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            minHeight: "100vh",
        }}>
            <Outlet />
        </div>
    );
}
