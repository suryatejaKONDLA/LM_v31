import { Controller, type Control, type FieldValues, type Path, type RegisterOptions } from "react-hook-form";
import { Input, Form } from "antd";
import { LockOutlined } from "@ant-design/icons";

/**
 * Props for the PasswordBox control.
 * Wraps Ant Design Input.Password with react-hook-form Controller.
 */
export interface PasswordBoxProps<T extends FieldValues>
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
    /** Disable the input. */
    disabled?: boolean;
    /** Maximum character length. */
    maxLength?: number;
    /** Show the clear (X) button. */
    allowClear?: boolean;
    /** HTML autocomplete attribute — defaults to "current-password". */
    autoComplete?: "current-password" | "new-password" | "off";
    /** Validation rules forwarded to react-hook-form Controller. */
    validation?: RegisterOptions<T>;
    /** Optional prefix icon — defaults to LockOutlined. */
    prefixIcon?: React.ReactNode;
}

/**
 * PasswordBox — Dedicated password input control.
 *
 * Uses antd Input.Password with built-in visibility toggle (eye icon)
 * and react-hook-form Controller for form state management.
 */
export function PasswordBox<T extends FieldValues>({
    control,
    name,
    label = "Password",
    placeholder = "Enter your password",
    required = false,
    disabled = false,
    maxLength,
    allowClear = false,
    autoComplete = "current-password",
    validation,
    prefixIcon = <LockOutlined />,
}: PasswordBoxProps<T>): React.JSX.Element
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
                    <Input.Password
                        {...field}
                        prefix={prefixIcon}
                        placeholder={placeholder}
                        disabled={disabled}
                        maxLength={maxLength}
                        size="large"
                        allowClear={allowClear}
                        autoComplete={autoComplete}
                    />
                </Form.Item>
            )}
        />
    );
}
