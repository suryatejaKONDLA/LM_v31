import { useEffect, useRef } from "react";
import { useNavigationGuardStore } from "../Stores/NavigationGuardStore";

/**
 * Warns the user before leaving the page when there are unsaved changes.
 *
 * - **Browser close / refresh**: `beforeunload` listener.
 * - **In-app navigation**: blocks via `NavigationGuardStore` — all components
 *   that use `useGuardedNavigate` will show a confirmation dialog.
 *
 * Works with `BrowserRouter` — no data router required.
 *
 * @param isDirty - Whether the form has been modified.
 * @param pageTitle - Page name shown in the dialog (optional).
 */
export function useUnsavedChanges(isDirty: boolean, pageTitle?: string): void
{
    const blockNavigation = useNavigationGuardStore((s) => s.blockNavigation);
    const unblockNavigation = useNavigationGuardStore((s) => s.unblockNavigation);
    const isBlockedByUs = useRef(false);

    // ─── Browser close / refresh ───────────────────────────────
    useEffect(() =>
    {
        if (!isDirty)
        {
            return;
        }

        const handler = (e: BeforeUnloadEvent): void =>
        {
            e.preventDefault();
        };

        window.addEventListener("beforeunload", handler);

        return () =>
        {
            window.removeEventListener("beforeunload", handler);
        };
    }, [ isDirty ]);

    // ─── In-app navigation (NavigationGuardStore) ──────────────
    useEffect(() =>
    {
        if (isDirty && !isBlockedByUs.current)
        {
            isBlockedByUs.current = true;
            blockNavigation({
                dialogTitle: "Unsaved Changes",
                dialogContent: pageTitle
                    ? `You have unsaved changes in ${pageTitle}. If you leave, your changes will be lost.`
                    : "You have unsaved changes. If you leave, your changes will be lost.",
            });
        }
        else if (!isDirty && isBlockedByUs.current)
        {
            isBlockedByUs.current = false;
            unblockNavigation();
        }
    }, [ isDirty, pageTitle, blockNavigation, unblockNavigation ]);

    // ─── Cleanup on unmount ────────────────────────────────────
    useEffect(() =>
    {
        return () =>
        {
            if (isBlockedByUs.current)
            {
                unblockNavigation();
                isBlockedByUs.current = false;
            }
        };
    }, [ unblockNavigation ]);
}
