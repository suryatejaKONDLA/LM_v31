import { Input, theme } from "antd";
import { SearchOutlined, CloseCircleFilled } from "@ant-design/icons";
import { memo, useCallback, useMemo } from "react";

/**
 * Props for the SearchBox control.
 * Standalone search input — no react-hook-form integration.
 */
export interface SearchBoxProps
{
    /** Current search value. */
    value: string;
    /** Change handler receiving the new search string. */
    onChange: (value: string) => void;
    /** Placeholder text. Defaults to "Search…". */
    placeholder?: string;
    /** Inline style override. */
    style?: React.CSSProperties;
    /** Maximum width in pixels. Defaults to 400. */
    maxWidth?: number;
    /** Show clear button when value is non-empty. Defaults to true. */
    allowClear?: boolean;
    /** Disable the input. */
    disabled?: boolean;
    /** Callback fired when the input is cleared. */
    onClear?: () => void;
    /** Input size. Defaults to "middle". */
    size?: "small" | "middle" | "large";
}

/**
 * SearchBox — Standalone search input with prefix icon and clear button.
 *
 * Designed for filtering lists, tables, or any searchable content.
 */
function SearchBoxInner({
    value,
    onChange,
    placeholder = "Search\u2026",
    style,
    maxWidth = 400,
    allowClear = true,
    disabled = false,
    onClear,
    size = "middle",
}: SearchBoxProps): React.JSX.Element
{
    const { token } = theme.useToken();

    const handleChange = useCallback(
        (e: React.ChangeEvent<HTMLInputElement>) =>
        {
            onChange(e.target.value);
        },
        [ onChange ],
    );

    const handleClear = useCallback(() =>
    {
        onChange("");
        onClear?.();
    }, [ onChange, onClear ]);

    const prefixStyle = useMemo(
        () => ({ color: token.colorTextSecondary }),
        [ token.colorTextSecondary ],
    );

    const clearStyle = useMemo(
        () => ({ color: token.colorTextSecondary, cursor: "pointer" }),
        [ token.colorTextSecondary ],
    );

    return (
        <Input
            placeholder={placeholder}
            prefix={<SearchOutlined style={prefixStyle} />}
            suffix={
                value && allowClear
                    ? <CloseCircleFilled style={clearStyle} onClick={handleClear} />
                    : undefined
            }
            value={value}
            onChange={handleChange}
            disabled={disabled}
            size={size}
            style={{ maxWidth, ...style }}
        />
    );
}

export const SearchBox = memo(SearchBoxInner);
