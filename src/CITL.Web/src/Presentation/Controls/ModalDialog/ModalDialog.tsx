import React from "react";
import { Modal } from "antd";
import type { HookAPI } from "antd/es/modal/useModal";
import type { ModalFuncProps } from "antd/es/modal/interface";
import { CheckCircleOutlined } from "@ant-design/icons";
import type { FieldError } from "@/Shared/Index";

/**
 * Parse a "Title | Detail" formatted result message.
 */
function parseResultMessage(message: string): { title: string; detail: string | null }
{
    const separatorIndex = message.indexOf("|");

    if (separatorIndex === -1)
    {
        return { title: message.trim(), detail: null };
    }

    return {
        title: message.substring(0, separatorIndex).trim(),
        detail: message.substring(separatorIndex + 1).trim(),
    };
}

/**
 * Module-scoped reference set by the Holder component.
 */
let modalApi: HookAPI | null = null;

/**
 * ModalDialog — Imperative modal / confirm dialog API.
 *
 * Mount `<ModalDialog.Holder />` once at the app root.
 *
 * Features beyond the old implementation:
 * - `successResult` / `showResult` parse "Title | Detail" strings with HTML support.
 * - `showValidationErrors` renders a structured list from `FieldError[]`.
 * - `confirmReload` / `confirmReset` / `showSuccessPrint` — common prebuilt dialogs.
 *
 * @example
 * ```tsx
 * <ModalDialog.Holder />
 *
 * ModalDialog.success({ title: "Done", content: "Saved" });
 * ModalDialog.confirm({ title: "Delete?", onOk: handleDelete });
 * ModalDialog.showValidationErrors(errors);
 * ```
 */
export const ModalDialog = {
    /** Show a success modal. */
    success: (config: ModalFuncProps): void =>
    {
        modalApi?.success({ centered: true, ...config });
    },

    /** Show an error modal. */
    error: (config: ModalFuncProps): void =>
    {
        modalApi?.error({ centered: true, ...config });
    },

    /** Show a warning modal. */
    warning: (config: ModalFuncProps): void =>
    {
        modalApi?.warning({ centered: true, ...config });
    },

    /** Show an informational modal. */
    info: (config: ModalFuncProps): void =>
    {
        modalApi?.info({ centered: true, ...config });
    },

    /** Show a confirmation dialog with OK / Cancel. */
    confirm: (config: ModalFuncProps): void =>
    {
        modalApi?.confirm({ centered: true, ...config });
    },

    /**
     * Show a success result dialog parsed from "Title | Detail" format.
     * Detail supports raw HTML via `dangerouslySetInnerHTML`.
     */
    successResult: (message: string): void =>
    {
        const { title, detail } = parseResultMessage(message);

        modalApi?.success({
            title,
            content: detail ? (
                <div dangerouslySetInnerHTML={{ __html: detail }} />
            ) : undefined,
            centered: true,
            okText: "OK",
        });
    },

    /**
     * Show a result dialog whose type is driven by a status string.
     *
     * @param type - One of "success", "error", "warning", "info".
     * @param message - Result message in "Title | Detail" format.
     */
    showResult: (type: "success" | "error" | "warning" | "info", message: string): void =>
    {
        const { title, detail } = parseResultMessage(message);

        modalApi?.[type]({
            title,
            content: detail ? (
                <div dangerouslySetInnerHTML={{ __html: detail }} />
            ) : undefined,
            centered: true,
            okText: "OK",
        });
    },

    /**
     * Render a list of field-level validation errors in an error modal.
     */
    showValidationErrors: (errors: FieldError[]): void =>
    {
        if (errors.length === 0)
        {
            return;
        }

        modalApi?.error({
            title: "Validation Errors",
            content: (
                <div style={{ maxHeight: 300, overflowY: "auto" }}>
                    {errors.map((err, idx) => (
                        <div key={idx} style={{ marginBottom: idx < errors.length - 1 ? 8 : 0 }}>
                            <strong style={{ fontSize: 13 }}>{err.Field}</strong>
                            <ul style={{ margin: "4px 0 0 16px", paddingLeft: 0 }}>
                                {err.Messages.map((msg, msgIdx) => (
                                    <li key={msgIdx} style={{ fontSize: 14 }}>{msg}</li>
                                ))}
                            </ul>
                        </div>
                    ))}
                </div>
            ),
            centered: true,
            okText: "OK",
            width: 450,
        });
    },

    /** Confirm dialog to reload a record from the database. */
    confirmReload: (onConfirm: () => void): void =>
    {
        modalApi?.confirm({
            title: "Reload Record",
            content: "Reload this record from the database?",
            okText: "Yes",
            cancelText: "No",
            centered: true,
            onOk: onConfirm,
        });
    },

    /** Confirm dialog to reset a form. */
    confirmReset: (onConfirm: () => void): void =>
    {
        modalApi?.confirm({
            title: "Reset Form",
            content: "Are you sure you want to reset this form?",
            okText: "Yes",
            cancelText: "No",
            centered: true,
            onOk: onConfirm,
        });
    },

    /**
     * Show a success dialog with "Do you want to print?" confirmation.
     *
     * @param message - Result message in "Title | Detail" format.
     * @param onPrint - Callback when user confirms print.
     * @param onDismiss - Optional callback when user dismisses.
     */
    showSuccessPrint: (message: string, onPrint: () => void, onDismiss?: () => void): void =>
    {
        const { title, detail } = parseResultMessage(message);

        modalApi?.confirm({
            title,
            icon: <CheckCircleOutlined style={{ color: "#52c41a" }} />,
            content: (
                <div>
                    {detail && (
                        <div style={{ marginBottom: 12 }}>
                            <span style={{ fontSize: 16, fontWeight: 500 }}>{detail}</span>
                        </div>
                    )}
                    <div style={{ fontSize: 14, color: "#595959" }}>
                        Do you want to print this record?
                    </div>
                </div>
            ),
            okText: "Yes, Print",
            cancelText: "No",
            centered: true,
            onOk: onPrint,
            ...(onDismiss ? { onCancel: onDismiss } : {}),
        });
    },

    /**
     * Context holder — renders the Ant Design modal container
     * and captures the imperative API reference.
     * Mount exactly once in the root layout.
     */
    Holder: (): React.ReactNode =>
    {
        const [ api, contextHolder ] = Modal.useModal();
        modalApi = api;
        return contextHolder;
    },
};
