import { Radio, Space } from "antd";
import { memo, useCallback } from "react";

/**
 * Option shape for RadioGroupBox.
 */
export interface RadioGroupOption<V = string | number>
{
    /** Option value. */
    value: V;
    /** Display label. */
    label: React.ReactNode;
    /** Disable this specific option. */
    disabled?: boolean;
}

/**
 * Props for the RadioGroupBox control.
 * Standalone radio group — no react-hook-form integration.
 */
export interface RadioGroupBoxProps<V = string | number>
{
    /** Currently selected value. */
    value: V | null;
    /** Change handler receiving the new value. */
    onChange: (value: V) => void;
    /** Array of radio options. Provide either `options` or `children`, not both. */
    options?: RadioGroupOption<V>[];
    /** Manual Radio children (alternative to `options`). */
    children?: React.ReactNode;
    /** Disable the entire group. */
    disabled?: boolean;
    /** Radio appearance. Defaults to "default". */
    optionType?: "default" | "button";
    /** Button style when optionType="button". Defaults to "outline". */
    buttonStyle?: "outline" | "solid";
    /** Size of button-type radios. Defaults to "middle". */
    size?: "small" | "middle" | "large";
    /** Layout direction. Defaults to "horizontal". */
    direction?: "horizontal" | "vertical";
}

/**
 * RadioGroupBox — Standalone radio group wrapping Ant Design Radio.Group.
 *
 * Accepts either an `options` array or manual `children`.
 */
function RadioGroupBoxInner<V = string | number>({
    value,
    onChange,
    options,
    children,
    disabled = false,
    optionType = "default",
    buttonStyle = "outline",
    size = "middle",
    direction = "horizontal",
}: RadioGroupBoxProps<V>): React.JSX.Element
{
    const handleChange = useCallback(
        (e: { target: { value?: V } }) =>
        {
            if (e.target.value !== undefined)
            {
                onChange(e.target.value);
            }
        },
        [ onChange ],
    );

    if (options && options.length > 0)
    {
        return (
            <Radio.Group
                value={value}
                onChange={handleChange}
                disabled={disabled}
                optionType={optionType}
                buttonStyle={buttonStyle}
                size={size}
            >
                <Space orientation={direction}>
                    {options.map((opt) => (
                        <Radio key={String(opt.value)} value={opt.value} disabled={opt.disabled ?? false}>
                            {opt.label}
                        </Radio>
                    ))}
                </Space>
            </Radio.Group>
        );
    }

    return (
        <Radio.Group
            value={value}
            onChange={handleChange}
            disabled={disabled}
            optionType={optionType}
            buttonStyle={buttonStyle}
            size={size}
        >
            {children}
        </Radio.Group>
    );
}

export const RadioGroupBox = memo(RadioGroupBoxInner) as typeof RadioGroupBoxInner;
