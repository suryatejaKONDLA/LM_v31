import { Input } from "antd";
import { memo, useCallback } from "react";

const { TextArea } = Input;

/**
 * Props for the TextAreaBox control.
 * Standalone textarea — no react-hook-form integration.
 */
export interface TextAreaBoxProps
{
    /** Current value. */
    value: string;
    /** Change handler receiving the new string value. */
    onChange: (value: string) => void;
    /** Placeholder text. */
    placeholder?: string;
    /** Inline style override. */
    style?: React.CSSProperties;
    /** Disable the textarea. */
    disabled?: boolean;
    /** Maximum character length. */
    maxLength?: number;
    /** Number of visible text rows. Defaults to 4. */
    rows?: number;
    /** Show remaining characters count. */
    showCount?: boolean;
    /** Auto-resize configuration. `true` or `{ minRows, maxRows }`. */
    autoSize?: boolean | { minRows?: number; maxRows?: number };
}

/**
 * TextAreaBox — Standalone multiline text input wrapping Ant Design Input.TextArea.
 *
 * Use for simple textareas outside of react-hook-form.
 */
function TextAreaBoxInner({
    value,
    onChange,
    placeholder = "",
    style,
    disabled = false,
    maxLength,
    rows = 4,
    showCount = false,
    autoSize,
}: TextAreaBoxProps): React.JSX.Element
{
    const handleChange = useCallback(
        (e: React.ChangeEvent<HTMLTextAreaElement>) =>
        {
            onChange(e.target.value);
        },
        [ onChange ],
    );

    return (
        <TextArea
            value={value}
            onChange={handleChange}
            placeholder={placeholder}
            disabled={disabled}
            rows={rows}
            showCount={showCount}
            {...(style ? { style } : {})}
            {...(maxLength !== undefined ? { maxLength } : {})}
            {...(autoSize !== undefined ? { autoSize } : {})}
        />
    );
}

export const TextAreaBox = memo(TextAreaBoxInner);
