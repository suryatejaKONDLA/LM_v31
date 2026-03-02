import { Controller, type Control, type FieldValues, type Path, type RegisterOptions } from "react-hook-form";
import { Checkbox, Form } from "antd";

/**
 * Props for the CheckBox control.
 * Wraps Ant Design Checkbox with react-hook-form Controller.
 */
export interface CheckBoxProps<T extends FieldValues>
{
    /** React Hook Form control object. */
    control: Control<T>;
    /** Field name in the form. */
    name: Path<T>;
    /** Label text displayed next to the checkbox. */
    label?: string;
    /** Form.Item label displayed above the checkbox. */
    formLabel?: string;
    /** Disable the checkbox. */
    disabled?: boolean;
    /** Validation rules forwarded to react-hook-form Controller. */
    validation?: RegisterOptions<T>;
}

/**
 * CheckBox — Standard checkbox control.
 *
 * Supports boolean and "Y"/"N" string values.
 * Uses antd Checkbox + react-hook-form Controller.
 */
export function CheckBox<T extends FieldValues>({
    control,
    name,
    label,
    formLabel,
    disabled = false,
    validation,
}: CheckBoxProps<T>): React.JSX.Element
{
    return (
        <Controller
            name={name}
            control={control}
            {...(validation ? { rules: validation } : {})}
            render={({ field, fieldState: { error } }) => (
                <Form.Item
                    label={formLabel}
                    validateStatus={error ? "error" : ""}
                    help={error?.message}
                    style={{ marginBottom: 0 }}
                >
                    <Checkbox
                        checked={field.value === true || field.value === "Y"}
                        onChange={(e) =>
                        {
                            field.onChange(e.target.checked);
                        }}
                        disabled={disabled}
                    >
                        {label}
                    </Checkbox>
                </Form.Item>
            )}
        />
    );
}
