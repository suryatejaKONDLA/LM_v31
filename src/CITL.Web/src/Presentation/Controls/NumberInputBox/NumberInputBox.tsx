import { InputNumber } from "antd";
import { memo, useCallback } from "react";

/**
 * Props for the NumberInputBox control.
 * Standalone numeric input — no react-hook-form integration.
 */
export interface NumberInputBoxProps
{
    /** Current value. */
    value: number | null;
    /** Change handler receiving the new numeric value (or null). */
    onChange: (value: number | null) => void;
    /** Placeholder text. */
    placeholder?: string;
    /** Inline style override. */
    style?: React.CSSProperties;
    /** Disable the input. */
    disabled?: boolean;
    /** Minimum value. */
    min?: number;
    /** Maximum value. */
    max?: number;
    /** Step increment. Defaults to 1. */
    step?: number;
    /** Decimal precision. Defaults to 0. */
    precision?: number;
    /** Prefix element. */
    prefix?: React.ReactNode;
    /** Suffix element. */
    suffix?: React.ReactNode;
    /** Input size. Defaults to "middle". */
    size?: "small" | "middle" | "large";
}

/**
 * NumberInputBox — Standalone numeric input wrapping Ant Design InputNumber.
 *
 * Use this for simple numeric inputs outside of react-hook-form.
 * For form-integrated inputs, use `NumberBox` instead.
 */
function NumberInputBoxInner({
    value,
    onChange,
    placeholder = "",
    style,
    disabled = false,
    min,
    max,
    step = 1,
    precision = 0,
    prefix,
    suffix,
    size = "middle",
}: NumberInputBoxProps): React.JSX.Element
{
    const handleChange = useCallback(
        (val: number | string | null) =>
        {
            onChange(typeof val === "number" ? val : null);
        },
        [ onChange ],
    );

    return (
        <InputNumber<number>
            value={value}
            onChange={handleChange}
            placeholder={placeholder}
            style={{ width: "100%", ...style }}
            disabled={disabled}
            step={step}
            precision={precision}
            size={size}
            {...(min !== undefined ? { min } : {})}
            {...(max !== undefined ? { max } : {})}
            {...(prefix ? { prefix } : {})}
            {...(suffix ? { suffix } : {})}
        />
    );
}

export const NumberInputBox = memo(NumberInputBoxInner);
