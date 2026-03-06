import { useCallback } from "react";
import { useNavigate, type NavigateOptions, type To } from "react-router-dom";
import { useNavigationGuardStore } from "../Stores/NavigationGuardStore";
import { ModalDialog } from "@/Shared/Index";

/**
 * Wraps `useNavigate` with navigation-guard checking.
 * If navigation is blocked (unsaved changes), shows a confirmation dialog.
 * Works with `BrowserRouter` — no data router required.
 */
export function useGuardedNavigate(): (to: To | number, options?: NavigateOptions) => void
{
    const navigate = useNavigate();

    return useCallback((to: To | number, options?: NavigateOptions) =>
    {
        const { isBlocked, dialogTitle, dialogContent, stayButtonText, leaveButtonText, onLeaveConfirmed, unblockNavigation } =
            useNavigationGuardStore.getState();

        if (!isBlocked)
        {
            if (typeof to === "number")
            {
                void navigate(to);
            }
            else
            {
                void navigate(to, options);
            }
            return;
        }

        ModalDialog.confirm({
            title: dialogTitle,
            content: dialogContent,
            okText: stayButtonText,
            cancelText: leaveButtonText,
            centered: true,
            okButtonProps: { type: "primary" },
            cancelButtonProps: { danger: true },
            onOk: () =>
            {
                // Stay — do nothing
            },
            onCancel: () =>
            {
                onLeaveConfirmed?.();
                unblockNavigation();

                if (typeof to === "number")
                {
                    void navigate(to);
                }
                else
                {
                    void navigate(to, options);
                }
            },
        });
    }, [ navigate ]);
}
