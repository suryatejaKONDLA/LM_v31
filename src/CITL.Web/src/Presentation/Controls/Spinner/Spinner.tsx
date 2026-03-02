import { memo } from "react";
import { Spin } from "antd";
import cssModule from "./Spinner.module.css";

const c = (...names: string[]): string =>
    names.map((n) => cssModule[n] ?? "").filter(Boolean).join(" ");

/**
 * Props for the Spinner component.
 */
export interface SpinnerProps
{
    /** Whether to show the spinner. */
    loading: boolean;
    /** Show as fullscreen overlay. Defaults to true. */
    fullScreen?: boolean;
    /** Loading tip text. Defaults to "Loading…". */
    tip?: string;
}

/**
 * Spinner — Global loading overlay.
 *
 * Renders a fixed fullscreen overlay with an Ant Design Spin indicator.
 * Returns `null` when not loading for zero DOM footprint.
 *
 * @example
 * ```tsx
 * <Spinner loading={isSubmitting} tip="Saving…" />
 * ```
 */
function SpinnerInner({
    loading,
    fullScreen = true,
    tip = "Loading\u2026",
}: SpinnerProps): React.ReactNode
{
    if (!loading)
    {
        return null;
    }

    return (
        <div className={c("overlay")}>
            <Spin
                spinning
                fullscreen={fullScreen}
                size="large"
                description={tip}
            />
        </div>
    );
}

export const Spinner = memo(SpinnerInner);
