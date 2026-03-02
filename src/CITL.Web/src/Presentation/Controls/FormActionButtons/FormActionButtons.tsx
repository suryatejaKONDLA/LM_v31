import { Button, Space } from "antd";
import { memo } from "react";

/**
 * Props for the FormActionButtons control.
 */
export interface FormActionButtonsProps
{
    /** Whether the form submission is in progress. */
    submitting?: boolean;
    /** Callback for the reset / cancel button. */
    onReset?: () => void;
    /** Submit button text. Defaults to "Save". */
    submitText?: string;
    /** Reset button text. Defaults to "Reset". */
    resetText?: string;
    /** Hide the reset button. */
    hideReset?: boolean;
    /** Disable the submit button externally (e.g. form-level validation). */
    disableSubmit?: boolean;
}

/**
 * FormActionButtons — Submit + Reset button pair.
 *
 * Designed to sit at the bottom of a form.
 * The submit button uses `htmlType="submit"` so it triggers
 * the enclosing `<form>` onSubmit.
 */
function FormActionButtonsInner({
    submitting = false,
    onReset,
    submitText = "Save",
    resetText = "Reset",
    hideReset = false,
    disableSubmit = false,
}: FormActionButtonsProps): React.JSX.Element
{
    return (
        <Space>
            <Button
                type="primary"
                htmlType="submit"
                loading={submitting}
                disabled={disableSubmit}
            >
                {submitText}
            </Button>

            {!hideReset && (
                <Button
                    onClick={onReset}
                    disabled={submitting}
                >
                    {resetText}
                </Button>
            )}
        </Space>
    );
}

export const FormActionButtons = memo(FormActionButtonsInner);
