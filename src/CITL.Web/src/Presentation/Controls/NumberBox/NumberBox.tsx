import { InputNumber, Form } from "antd";
import { memo, useCallback } from "react";
import { Controller, type Control, type FieldValues, type Path, type RegisterOptions } from "react-hook-form";

/**
 * Props for the NumberBox control.
 * Wraps Ant Design InputNumber with react-hook-form Controller.
 */
export interface NumberBoxProps<T extends FieldValues>
{
    /** React Hook Form control object. */
    control: Control<T>;
    /** Field name in the form. */
    name: Path<T>;
    /** Label text above the input. */
    label?: string;
    /** Placeholder text. */
    placeholder?: string;
    /** Mark field as required (visual asterisk). */
    required?: boolean;
    /** Disable the input. */
    disabled?: boolean;
    /** Make input read-only. */
    readOnly?: boolean;
    /** Minimum value. */
    min?: number;
    /** Maximum value. */
    max?: number;
    /** Step increment. */
    step?: number;
    /** Decimal precision. */
    precision?: number;
    /** CSS class name. */
    className?: string;
    /** Validation rules forwarded to react-hook-form Controller. */
    validation?: RegisterOptions<T>;
    /** Custom display formatter. */
    formatter?: (value: number | string | undefined) => string;
    /** Custom parser (inverse of formatter). */
    parser?: (displayValue: string | undefined) => number | string;
    /** Prefix element. */
    prefix?: React.ReactNode;
    /** Suffix element. */
    suffix?: React.ReactNode;
    /** Content before the input. */
    addonBefore?: React.ReactNode;
    /** Content after the input. */
    addonAfter?: React.ReactNode;
    /** Show +/− controls. Defaults to true. */
    controls?: boolean;
    /** Input size. Defaults to "large". */
    size?: "small" | "middle" | "large";
    /** Enable keyboard up/down. Defaults to true. */
    keyboard?: boolean;
    /** Auto-focus on mount. */
    autoFocus?: boolean;
    /** Callback when value changes. */
    onValueChange?: (value: number | null) => void;
    /** Callback on blur. */
    onBlurCallback?: () => void;
    /** Callback on Enter key. */
    onPressEnter?: () => void;
    /** Inline style override. */
    style?: React.CSSProperties;
    /** Use string mode for high-precision decimals. */
    stringMode?: boolean;
}

/**
 * NumberBox — Numeric input with react-hook-form integration.
 *
 * Wraps Ant Design InputNumber with full form error display,
 * optional callbacks, and memoization for performance.
 */
function NumberBoxInner<T extends FieldValues>({
    control,
    name,
    label,
    placeholder,
    required = false,
    disabled = false,
    readOnly = false,
    min,
    max,
    step,
    precision,
    className,
    validation,
    formatter,
    parser,
    prefix,
    suffix,
    addonBefore,
    addonAfter,
    controls = true,
    size = "large",
    keyboard = true,
    autoFocus = false,
    onValueChange,
    onBlurCallback,
    onPressEnter,
    style,
    stringMode = false,
}: NumberBoxProps<T>): React.JSX.Element
{
    const handleChange = useCallback(
        (value: number | string | null, fieldOnChange: (v: number | string | null) => void) =>
        {
            fieldOnChange(value);
            onValueChange?.(typeof value === "number" ? value : null);
        },
        [ onValueChange ],
    );

    const handleBlur = useCallback(
        (fieldOnBlur: () => void) =>
        {
            fieldOnBlur();
            onBlurCallback?.();
        },
        [ onBlurCallback ],
    );

    const handleKeyDown = useCallback(
        (e: React.KeyboardEvent<HTMLInputElement>) =>
        {
            if (e.key === "Enter")
            {
                onPressEnter?.();
            }
        },
        [ onPressEnter ],
    );

    return (
        <Controller
            name={name}
            control={control}
            {...(validation ? { rules: validation } : {})}
            render={({ field, fieldState: { error } }) => (
                <Form.Item
                    label={label}
                    style={{ textAlign: "left" }}
                    {...(error ? { validateStatus: "error" as const, help: error.message } : {})}
                    required={required}
                >
                    <InputNumber
                        id={name}
                        value={field.value ?? null}
                        onChange={(v) =>
                        {
                            handleChange(v, field.onChange);
                        }}
                        onBlur={() =>
                        {
                            handleBlur(field.onBlur);
                        }}
                        ref={field.ref}
                        name={field.name}
                        placeholder={placeholder}
                        disabled={disabled}
                        readOnly={readOnly}
                        size={size}
                        keyboard={keyboard}
                        autoFocus={autoFocus}
                        stringMode={stringMode}
                        style={{ width: "100%", ...style }}
                        controls={controls && !readOnly}
                        {...(min !== undefined ? { min } : {})}
                        {...(max !== undefined ? { max } : {})}
                        {...(step !== undefined ? { step } : {})}
                        {...(precision !== undefined ? { precision } : {})}
                        {...(className ? { className } : {})}
                        {...(formatter ? { formatter } : {})}
                        {...(parser ? { parser } : {})}
                        {...(prefix ? { prefix } : {})}
                        {...(suffix ? { suffix } : {})}
                        {...(addonBefore ? { addonBefore } : {})}
                        {...(addonAfter ? { addonAfter } : {})}
                        {...(onPressEnter ? { onKeyDown: handleKeyDown } : {})}
                    />
                </Form.Item>
            )}
        />
    );
}

export const NumberBox = memo(NumberBoxInner) as typeof NumberBoxInner;
