import { useCallback, useMemo } from "react";
import { DatePicker, Form } from "antd";
import dayjs from "dayjs";
import { Controller, type Control, type FieldValues, type Path, type RegisterOptions } from "react-hook-form";

/**
 * Props for the DatePickerBox control.
 * Wraps Ant Design DatePicker with react-hook-form Controller.
 */
export interface DatePickerBoxProps<T extends FieldValues>
{
    /** React Hook Form control object. */
    control: Control<T>;
    /** Field name in the form. */
    name: Path<T>;
    /** Label text above the picker. */
    label?: string;
    /** Placeholder text. Defaults to "Select date". */
    placeholder?: string;
    /** Mark field as required (visual asterisk). */
    required?: boolean;
    /** Disable the picker. */
    disabled?: boolean;
    /** Show a clear button. Defaults to true. */
    allowClear?: boolean;
    /** Display format string. Defaults to "DD-MMM-YYYY". */
    format?: string;
    /** Earliest selectable date (ISO string or dayjs). */
    minDate?: string | dayjs.Dayjs;
    /** Latest selectable date (ISO string or dayjs). */
    maxDate?: string | dayjs.Dayjs;
    /** CSS class name. */
    className?: string;
    /** Show the "Today" button. Defaults to true. */
    showNow?: boolean;
    /** Validation rules forwarded to react-hook-form Controller. */
    validation?: RegisterOptions<T>;
}

/**
 * DatePickerBox — Date picker with react-hook-form integration.
 *
 * Stores the value as an ISO date string ("YYYY-MM-DD") in form state.
 * Supports min/max date constraints via `disabledDate`.
 */
export function DatePickerBox<T extends FieldValues>({
    control,
    name,
    label,
    placeholder = "Select date",
    required = false,
    disabled = false,
    allowClear = true,
    format = "DD-MMM-YYYY",
    minDate,
    maxDate,
    className,
    showNow = true,
    validation,
}: DatePickerBoxProps<T>): React.JSX.Element
{
    const minDayJs = useMemo(() => minDate ? dayjs(minDate) : null, [ minDate ]);
    const maxDayJs = useMemo(() => maxDate ? dayjs(maxDate) : null, [ maxDate ]);

    const disabledDate = useCallback(
        (current: dayjs.Dayjs): boolean =>
        {
            if (minDayJs && current.isBefore(minDayJs, "day"))
            {
                return true;
            }

            if (maxDayJs && current.isAfter(maxDayJs, "day"))
            {
                return true;
            }

            return false;
        },
        [ minDayJs, maxDayJs ],
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
                    <DatePicker
                        value={field.value ? dayjs(field.value) : null}
                        onChange={(date) =>
                        {
                            field.onChange(date ? date.format("YYYY-MM-DD") : null);
                        }}
                        onBlur={field.onBlur}
                        ref={field.ref}
                        placeholder={placeholder}
                        disabled={disabled}
                        format={format}
                        allowClear={allowClear}
                        showNow={showNow}
                        size="large"
                        style={{ width: "100%" }}
                        {...(className ? { className } : {})}
                        disabledDate={disabledDate}
                    />
                </Form.Item>
            )}
        />
    );
}
