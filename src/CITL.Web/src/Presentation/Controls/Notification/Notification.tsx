import { notification } from "antd";
import type { NotificationInstance } from "antd/es/notification/interface";

/**
 * Configuration options for a notification popup.
 */
export interface NotifyOptions
{
    /** Duration in seconds before auto-close. 0 = persist until dismissed. Defaults to 4.5. */
    duration?: number;
    /** Callback fired when the notification closes. */
    onClose?: () => void;
    /** Placement on screen. Defaults to "topRight". */
    placement?: "topLeft" | "topRight" | "bottomLeft" | "bottomRight";
}

/**
 * Module-scoped reference set by the Holder component.
 */
let notificationApi: NotificationInstance | null = null;

/**
 * Show a notification of the given type.
 */
function show(
    type: "success" | "error" | "warning" | "info",
    title: string,
    description?: string,
    options?: NotifyOptions,
): void
{
    notificationApi?.[type]({
        message: title,
        description,
        duration: options?.duration ?? 4.5,
        ...(options?.onClose ? { onClose: options.onClose } : {}),
        placement: options?.placement ?? "topRight",
    });
}

/**
 * Notify — Imperative notification API.
 *
 * Mount `<Notify.Holder />` once at the app root to enable static
 * methods (`Notify.success`, `.error`, …).
 *
 * @example
 * ```tsx
 * <Notify.Holder />
 *
 * Notify.success("Done", "Record saved successfully");
 * Notify.error("Failed", "Could not connect to server", { duration: 0 });
 * ```
 */
export const Notify = {
    /** Display a success notification. */
    success: (title: string, description?: string, options?: NotifyOptions): void =>
    {
        show("success", title, description, options);
    },

    /** Display an error notification. */
    error: (title: string, description?: string, options?: NotifyOptions): void =>
    {
        show("error", title, description, options);
    },

    /** Display a warning notification. */
    warning: (title: string, description?: string, options?: NotifyOptions): void =>
    {
        show("warning", title, description, options);
    },

    /** Display an informational notification. */
    info: (title: string, description?: string, options?: NotifyOptions): void =>
    {
        show("info", title, description, options);
    },

    /**
     * Context holder — renders the Ant Design notification container
     * and captures the imperative API reference.
     * Mount exactly once in the root layout.
     */
    Holder: (): React.ReactNode =>
    {
        const [ api, contextHolder ] = notification.useNotification();
        notificationApi = api;
        return contextHolder;
    },
};
