import { useMemo } from "react";
import { useLocation } from "react-router-dom";
import "@/Presentation/Layouts/CSSAnimations.css";

interface PageTransitionProps
{
    children: React.ReactNode;
}

/**
 * CSS-based page transition wrapper.
 * Uses key-based remounting to trigger CSS animation on route change.
 */
export default function PageTransition({ children }: PageTransitionProps): React.ReactNode
{
    const location = useLocation();

    const animationKey = useMemo(() =>
    {
        const searchParams = new URLSearchParams(location.search);
        const token = searchParams.get("token");

        if (token)
        {
            return `${location.pathname}?${decodeURIComponent(token)}`;
        }

        return location.pathname + location.search;
    }, [ location.pathname, location.search ]);

    return (
        <div key={animationKey} className="page-transition-wrapper page-transition-enter">
            {children}
        </div>
    );
}
