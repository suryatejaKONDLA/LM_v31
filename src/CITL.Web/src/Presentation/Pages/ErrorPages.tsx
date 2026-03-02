import { Result } from "antd";
import { useSearchParams } from "react-router-dom";

/**
 * 404 Not Found error page.
 */
export default function NotFound(): React.JSX.Element
{
    return (
        <Result
            status="404"
            title="404"
            subTitle="The page you visited does not exist."
        />
    );
}

/**
 * 403 Forbidden error page.
 */
export function Forbidden(): React.JSX.Element
{
    const [ searchParams ] = useSearchParams();
    const message = searchParams.get("msg") ?? "You do not have permission to access this page.";

    return (
        <Result
            status="403"
            title="403"
            subTitle={message}
        />
    );
}
