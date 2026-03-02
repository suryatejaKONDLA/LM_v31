import { useState, useRef, useCallback, useMemo, memo, startTransition } from "react";
import { createPortal } from "react-dom";
import { Tooltip, theme } from "antd";
import { SunOutlined, MoonOutlined } from "@ant-design/icons";
import { useThemeStore } from "@/Application/Index";
import cssModule from "./ThemeToggleButton.module.css";

/** Safe CSS-module class name accessor. */
const c = (...names: string[]): string =>
    names.map((n) => cssModule[n] ?? "").filter(Boolean).join(" ");

/** Props for the ThemeToggleButton control. */
export interface ThemeToggleButtonProps
{
    readonly className?: string;
    readonly style?: React.CSSProperties;
    readonly size?: "small" | "medium" | "large";
    readonly disabled?: boolean;
    readonly tooltipPlacement?:
        | "top" | "topLeft" | "topRight"
        | "bottom" | "bottomLeft" | "bottomRight"
        | "left" | "leftTop" | "leftBottom"
        | "right" | "rightTop" | "rightBottom";
    readonly onClick?: () => void;
}

/**
 * Circular theme toggle button with animated ripple transition.
 * Pure CSS Modules — zero runtime style overhead.
 */
function ThemeToggleButtonInner({
    className,
    style,
    size = "medium",
    disabled = false,
    tooltipPlacement = "bottom",
    onClick,
}: ThemeToggleButtonProps): React.JSX.Element
{
    const [ transitioning, setTransitioning ] = useState(false);
    const [ origin, setOrigin ] = useState({ x: 0, y: 0 });
    const [ overlayColor, setOverlayColor ] = useState("#000000");
    const lockRef = useRef(false);
    const btnRef = useRef<HTMLDivElement>(null);

    const { token } = theme.useToken();
    const { mode, isDarkMode, toggleMode } = useThemeStore();

    // Pre-compute max ripple radius once
    const maxRadius = useMemo(
        () => Math.sqrt(window.innerWidth ** 2 + window.innerHeight ** 2),
        [],
    );

    const iconColor = mode === "light" ? token.colorWarning : "#ffffff";

    const handleClick = useCallback(() =>
    {
        if (disabled || lockRef.current)
        {
            return;
        }

        onClick?.();

        // Capture click origin for ripple
        if (btnRef.current)
        {
            const rect = btnRef.current.getBoundingClientRect();
            setOrigin({
                x: rect.left + rect.width / 2,
                y: rect.top + rect.height / 2,
            });
        }

        // Overlay should be the DESTINATION color
        setOverlayColor(isDarkMode ? "#ffffff" : "#141414");
        lockRef.current = true;
        setTransitioning(true);
        document.body.classList.add("theme-transitioning");

        // Toggle theme after ripple has covered the screen (70% keyframe ≈ 350ms)
        requestAnimationFrame(() =>
        {
            setTimeout(() =>
            {
                startTransition(() =>
                {
                    toggleMode();
                });
            }, 150);
        });

        // Unlock after animation completes
        setTimeout(() =>
        {
            lockRef.current = false;
            setTransitioning(false);
            document.body.classList.remove("theme-transitioning");
        }, 550);
    }, [ disabled, isDarkMode, onClick, toggleMode ]);

    const handleKeyDown = useCallback((e: React.KeyboardEvent) =>
    {
        if (e.key === "Enter" || e.key === " ")
        {
            e.preventDefault();
            handleClick();
        }
    }, [ handleClick ]);

    const classes = [
        c("toggle"),
        c(size),
        disabled ? c("disabled") : "",
        className ?? "",
    ].filter(Boolean).join(" ");

    const tooltipTitle = disabled
        ? "Theme toggle disabled"
        : `Current: ${mode === "light" ? "Light" : "Dark"} (Click to Toggle)`;

    return (
        <>
            {/* Ripple overlay — portal to body for full-screen coverage */}
            {transitioning && createPortal(
                <div
                    className={c("ripple")}
                    style={{
                        backgroundColor: overlayColor,
                        "--ripple-origin": `at ${String(origin.x)}px ${String(origin.y)}px`,
                        "--ripple-radius": `${String(maxRadius)}px`,
                    } as React.CSSProperties}
                />,
                document.body,
            )}

            <Tooltip title={tooltipTitle} placement={tooltipPlacement}>
                <div
                    ref={btnRef}
                    className={classes}
                    style={{
                        ...style,
                        background: isDarkMode ? token.colorBgElevated : token.colorBgContainer,
                        border: `1px solid ${token.colorBorder}`,
                    }}
                    onClick={handleClick}
                    onKeyDown={handleKeyDown}
                    role="button"
                    aria-label={`Switch to ${isDarkMode ? "light" : "dark"} theme`}
                    aria-pressed={isDarkMode}
                    tabIndex={disabled ? -1 : 0}
                >
                    <span key={mode} className={c("icon")} style={{ color: iconColor, display: "flex" }}>
                        {isDarkMode ? <MoonOutlined /> : <SunOutlined />}
                    </span>
                </div>
            </Tooltip>
        </>
    );
}

export const ThemeToggleButton = memo(ThemeToggleButtonInner);
