import { Button, Popconfirm, Space } from "antd";
import { memo } from "react";
import { type ResetConfirmProps } from "@/Shared/Index";

export type { ResetConfirmProps };

/**
 * Props for the FormActionButtons control.
 */
export interface FormActionButtonsProps
{
    /** Whether the form submission is in progress. */
    submitting?: boolean;
    /** Callback for the reset / cancel button (used when no resetConfirm). */
    onReset?: () => void;
    /** When provided, wraps the reset button in a Popconfirm instead of calling onReset directly. */
    resetConfirm?: ResetConfirmProps | undefined;
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
 *
 * When `resetConfirm` is provided, the reset button shows an
 * inline Popconfirm before executing the action.
 */
function FormActionButtonsInner({
    submitting = false,
    onReset,
    resetConfirm,
    submitText = "Save",
    resetText = "Reset",
    hideReset = false,
    disableSubmit = false,
}: FormActionButtonsProps): React.JSX.Element
{
    const resetButton = (
        <Button
            onClick={resetConfirm ? undefined : onReset}
            disabled={submitting}
        >
            {resetText}
        </Button>
    );

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
                resetConfirm
                    ? (
                        <Popconfirm
                            title={resetConfirm.title}
                            description={resetConfirm.description}
                            onConfirm={resetConfirm.onConfirm}
                            okText="Yes"
                            cancelText="No"
                        >
                            {resetButton}
                        </Popconfirm>
                    )
                    : resetButton
            )}
        </Space>
    );
}

export const FormActionButtons = memo(FormActionButtonsInner);
