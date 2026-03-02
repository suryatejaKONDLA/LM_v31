import { message } from "antd";
import type { MessageInstance } from "antd/es/message/interface";

/**
 * Configuration options for a message toast.
 */
export interface MessageOptions
{
    /** Duration in seconds before auto-close. Defaults to 3. */
    duration?: number;
    /** Callback fired when the message closes. */
    onClose?: () => void;
}

/**
 * Module-scoped reference set by the Holder component.
 * Avoids static `message.xxx()` which requires App context.
 */
let messageApi: MessageInstance | null = null;

/**
 * Show a message toast of the given type.
 * No-ops silently if the Holder is not mounted.
 */
function show(
    type: "success" | "error" | "warning" | "info" | "loading",
    content: string,
    options?: MessageOptions,
): void
{
    messageApi?.[type]({
        content,
        duration: options?.duration ?? 3,
        ...(options?.onClose ? { onClose: options.onClose } : {}),
    });
}

/**
 * Message — Imperative toast message API.
 *
 * Mount `<Message.Holder />` once at the app root to enable the static
 * methods (`Message.success`, `.error`, …).
 *
 * Performance notes:
 * - Zero re-renders in consuming components — calls are fire-and-forget.
 * - Module-scoped API ref avoids Context look-ups on every call.
 *
 * @example
 * ```tsx
 * // Root layout — mount once
 * <Message.Holder />
 *
 * // Anywhere in the app
 * Message.success("Saved successfully");
 * Message.error("Something went wrong", { duration: 5 });
 * ```
 */
export const Message = {
    /** Display a success toast. */
    success: (content: string, options?: MessageOptions): void =>
    {
        show("success", content, options);
    },

    /** Display an error toast. */
    error: (content: string, options?: MessageOptions): void =>
    {
        show("error", content, options);
    },

    /** Display a warning toast. */
    warning: (content: string, options?: MessageOptions): void =>
    {
        show("warning", content, options);
    },

    /** Display an informational toast. */
    info: (content: string, options?: MessageOptions): void =>
    {
        show("info", content, options);
    },

    /** Display a loading spinner toast (persists until manually closed). */
    loading: (content: string, options?: MessageOptions): void =>
    {
        show("loading", content, options);
    },

    /**
     * Context holder — renders the Ant Design message container and
     * captures the imperative API reference.
     * Mount exactly once in the root layout.
     */
    Holder: (): React.ReactNode =>
    {
        const [ api, contextHolder ] = message.useMessage();
        messageApi = api;
        return contextHolder;
    },
};
