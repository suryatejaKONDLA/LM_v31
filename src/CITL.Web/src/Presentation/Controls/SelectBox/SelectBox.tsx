import { Select } from "antd";
import { memo, useCallback, useMemo } from "react";

/**
 * Option shape for SelectBox.
 */
export interface SelectBoxOption<V = string | number>
{
    /** Option value. */
    value: V;
    /** Display label. */
    label: string;
}

/**
 * Props for the SelectBox control.
 * Standalone dropdown — no react-hook-form integration.
 */
export interface SelectBoxProps<V = string | number>
{
    /** Current value. */
    value: V | null;
    /** Change handler receiving the new value (or null when cleared). */
    onChange: (value: V | null) => void;
    /** Array of options. */
    options: SelectBoxOption<V>[];
    /** Placeholder text. Defaults to "Select…". */
    placeholder?: string;
    /** Inline style override. */
    style?: React.CSSProperties;
    /** Disable the select. */
    disabled?: boolean;
    /** Show a loading spinner. */
    loading?: boolean;
    /** Show a clear button. Defaults to true. */
    allowClear?: boolean;
    /** Enable search/filter. Defaults to true. */
    showSearch?: boolean;
    /** Input size. Defaults to "middle". */
    size?: "small" | "middle" | "large";
}

/**
 * SelectBox — Standalone select wrapping Ant Design Select.
 *
 * Use for simple selections outside of react-hook-form.
 * For form-integrated dropdowns, use `DropDown` instead.
 */
function SelectBoxInner<V = string | number>({
    value,
    onChange,
    options,
    placeholder = "Select\u2026",
    style,
    disabled = false,
    loading = false,
    allowClear = true,
    showSearch = true,
    size = "middle",
}: SelectBoxProps<V>): React.JSX.Element
{
    const handleChange = useCallback(
        (val: V | undefined) =>
        {
            onChange(val ?? null);
        },
        [ onChange ],
    );

    const searchConfig = useMemo(
        () =>
            showSearch
                ? {
                    filterOption: (input: string, option?: { label?: unknown }): boolean =>
                    {
                        const label = typeof option?.label === "string" ? option.label : "";
                        return label.toLowerCase().includes(input.toLowerCase());
                    },
                }
                : false,
        [ showSearch ],
    );

    return (
        <Select
            style={{ width: "100%", ...style }}
            placeholder={placeholder}
            value={value ?? undefined}
            onChange={handleChange}
            disabled={disabled}
            loading={loading}
            allowClear={allowClear}
            size={size}
            showSearch={searchConfig}
            options={options}
        />
    );
}

export const SelectBox = memo(SelectBoxInner) as typeof SelectBoxInner;
