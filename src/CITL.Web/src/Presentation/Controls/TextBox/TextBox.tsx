import { Controller, type Control, type FieldValues, type Path, type RegisterOptions } from "react-hook-form";
import { Input, Form } from "antd";

/**
 * Props for the TextBox control.
 * Wraps Ant Design Input with react-hook-form Controller.
 */
export interface TextBoxProps<T extends FieldValues>
{
    /** React Hook Form control object. */
    control: Control<T>;
    /** Field name in the form. */
    name: Path<T>;
    /** Label text above the input. */
    label?: string;
    /** Placeholder text inside the input. */
    placeholder?: string;
    /** Mark field as required (visual asterisk). */
    required?: boolean;
    /** Input type — for password fields use PasswordBox instead. */
    type?: "text" | "email" | "tel" | "url" | "search";
    /** Disable the input. */
    disabled?: boolean;
    /** Maximum character length. */
    maxLength?: number;
    /** Show the clear (X) button. */
    allowClear?: boolean;
    /** HTML autocomplete attribute for browser autofill. */
    autoComplete?: string;
    /** Validation rules forwarded to react-hook-form Controller. */
    validation?: RegisterOptions<T>;
    /** Optional prefix icon or text inside the input. */
    prefix?: React.ReactNode;
    /** Optional suffix icon or text inside the input. */
    suffix?: React.ReactNode;
}

/**
 * TextBox — Standard text input control.
 *
 * Uses antd Input + react-hook-form Controller.
 * Supports selective per-button validation via react-hook-form trigger().
 */
export function TextBox<T extends FieldValues>({
    control,
    name,
    label,
    placeholder,
    required = false,
    type = "text",
    disabled = false,
    maxLength,
    allowClear = true,
    autoComplete,
    validation,
    prefix,
    suffix,
}: TextBoxProps<T>): React.JSX.Element
{
    return (
        <Controller
            name={name}
            control={control}
            {...(validation ? { rules: validation } : {})}
            render={({ field, fieldState: { error } }) => (
                <Form.Item
                    label={label}
                    required={required}
                    validateStatus={error ? "error" : ""}
                    help={error?.message}
                    style={{ textAlign: "left" }}
                >
                    <Input
                        {...field}
                        type={type}
                        placeholder={placeholder}
                        disabled={disabled}
                        maxLength={maxLength}
                        size="large"
                        allowClear={allowClear}
                        {...(autoComplete ? { autoComplete } : {})}
                        prefix={prefix}
                        suffix={suffix}
                    />
                </Form.Item>
            )}
        />
    );
}
