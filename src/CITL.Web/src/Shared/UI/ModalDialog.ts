import React from "react";
import type { HookAPI } from "antd/es/modal/useModal";
import type { ModalFuncProps } from "antd/es/modal/interface";
import { CheckCircleOutlined } from "@ant-design/icons";
import type { FieldError } from "@/Shared/Types/FieldError";

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

let modalApi: HookAPI | null = null;

/**
 * ModalDialog — Imperative modal / confirm dialog API.
 *
 * The Holder component (`ModalDialogHolder`) must be mounted once
 * at the app root (lives in Presentation layer).
 */
export const ModalDialog = {
    success: (config: ModalFuncProps): void =>
    {
        modalApi?.success({ centered: true, ...config });
    },

    error: (config: ModalFuncProps): void =>
    {
        modalApi?.error({ centered: true, ...config });
    },

    warning: (config: ModalFuncProps): void =>
    {
        modalApi?.warning({ centered: true, ...config });
    },

    info: (config: ModalFuncProps): void =>
    {
        modalApi?.info({ centered: true, ...config });
    },

    confirm: (config: ModalFuncProps): void =>
    {
        modalApi?.confirm({ centered: true, ...config });
    },

    successResult: (message: string, onOk?: () => void): void =>
    {
        const { title, detail } = parseResultMessage(message);
        const hasHtml = detail && /<[^>]+>/.test(detail);

        modalApi?.success({
            title,
            content: detail
                ? hasHtml
                    ? React.createElement("span", { dangerouslySetInnerHTML: { __html: detail }, style: { fontSize: 25 } })
                    : React.createElement("span", { style: { fontSize: 16 } }, detail)
                : undefined,
            centered: true,
            okText: "OK",
            ...(onOk && { onOk }),
        });
    },

    showResult: (type: "success" | "error" | "warning" | "info", message: string): void =>
    {
        const { title, detail } = parseResultMessage(message);
        const hasHtml = detail && /<[^>]+>/.test(detail);

        modalApi?.[type]({
            title,
            content: detail
                ? hasHtml
                    ? React.createElement("span", { dangerouslySetInnerHTML: { __html: detail }, style: { fontSize: 16 } })
                    : React.createElement("span", { style: { fontSize: 16 } }, detail)
                : undefined,
            centered: true,
            okText: "OK",
        });
    },

    showValidationErrors: (errors: FieldError[]): void =>
    {
        if (errors.length === 0)
        {
            return;
        }

        modalApi?.error({
            title: "Validation Errors",
            content: React.createElement(
                "div",
                { style: { maxHeight: 300, overflowY: "auto" as const } },
                errors.map((err, idx) =>
                    React.createElement(
                        "div",
                        { key: idx, style: { marginBottom: idx < errors.length - 1 ? 8 : 0 } },
                        React.createElement("strong", { style: { fontSize: 13 } }, err.Field),
                        React.createElement(
                            "ul",
                            { style: { margin: "4px 0 0 16px", paddingLeft: 0 } },
                            err.Messages.map((msg, msgIdx) =>
                                React.createElement("li", { key: msgIdx, style: { fontSize: 14 } }, msg),
                            ),
                        ),
                    ),
                ),
            ),
            centered: true,
            okText: "OK",
            width: 450,
        });
    },

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

    showSuccessPrint: (message: string, onPrint: () => void, onDismiss?: () => void): void =>
    {
        const { title, detail } = parseResultMessage(message);

        modalApi?.confirm({
            title,
            icon: React.createElement(CheckCircleOutlined, { style: { color: "#52c41a" } }),
            content: React.createElement(
                "div",
                null,
                detail
                    ? React.createElement(
                        "div",
                        { style: { marginBottom: 12 } },
                        React.createElement("span", { style: { fontSize: 16, fontWeight: 500 } }, detail),
                    )
                    : null,
                React.createElement(
                    "div",
                    { style: { fontSize: 14, color: "#595959" } },
                    "Do you want to print this record?",
                ),
            ),
            okText: "Yes, Print",
            cancelText: "No",
            centered: true,
            onOk: onPrint,
            ...(onDismiss ? { onCancel: onDismiss } : {}),
        });
    },

    /** @internal Called by ModalDialogHolder to register the Ant Design API. */
    _register: (api: HookAPI): void =>
    {
        modalApi = api;
    },
};
