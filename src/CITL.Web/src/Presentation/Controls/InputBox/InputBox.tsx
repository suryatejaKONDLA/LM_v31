import { Input } from "antd";
import { memo, useCallback } from "react";

/**
 * Props for the InputBox control.
 * Standalone input — no react-hook-form integration.
 */
export interface InputBoxProps
{
    /** Current value. */
    value: string;
    /** Change handler receiving the new string value. */
    onChange: (value: string) => void;
    /** Placeholder text. */
    placeholder?: string;
    /** Inline style override. */
    style?: React.CSSProperties;
    /** HTML input type. Defaults to "text". */
    type?: "text" | "email" | "tel" | "url" | "search";
    /** Disable the input. */
    disabled?: boolean;
    /** Maximum character length. */
    maxLength?: number;
    /** Callback fired on Enter key press. */
    onPressEnter?: () => void;
    /** Prefix icon or text. */
    prefix?: React.ReactNode;
    /** Suffix icon or text. */
    suffix?: React.ReactNode;
    /** Show the clear (X) button. Defaults to false. */
    allowClear?: boolean;
    /** Input size. Defaults to "middle". */
    size?: "small" | "middle" | "large";
}

/**
 * InputBox — Standalone text input wrapping Ant Design Input.
 *
 * Use this for simple inputs outside of react-hook-form.
 * For form-integrated inputs, use `TextBox` instead.
 */
function InputBoxInner({
    value,
    onChange,
    placeholder = "",
    style,
    type = "text",
    disabled = false,
    maxLength,
    onPressEnter,
    prefix,
    suffix,
    allowClear = false,
    size = "middle",
}: InputBoxProps): React.JSX.Element
{
    const handleChange = useCallback(
        (e: React.ChangeEvent<HTMLInputElement>) =>
        {
            onChange(e.target.value);
        },
        [ onChange ],
    );

    return (
        <Input
            type={type}
            value={value}
            onChange={handleChange}
            placeholder={placeholder}
            style={style}
            disabled={disabled}
            size={size}
            allowClear={allowClear}
            {...(maxLength !== undefined ? { maxLength } : {})}
            {...(onPressEnter ? { onPressEnter } : {})}
            {...(prefix ? { prefix } : {})}
            {...(suffix ? { suffix } : {})}
        />
    );
}

export const InputBox = memo(InputBoxInner);
