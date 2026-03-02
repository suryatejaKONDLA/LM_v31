import React, { memo } from "react";
import { useShortcutsStore } from "@/Application/Index";
import { KeyboardShortcutsHelp } from "./KeyboardShortcutsHelp";

// ============================================
// PROPS
// ============================================

export interface GlobalShortcutsHelpProps
{
    /** Override: external open state. Falls back to the store if omitted. */
    open?: boolean;
    /** Override: external close handler. Falls back to the store if omitted. */
    onClose?: () => void;
}

// ============================================
// COMPONENT
// ============================================

/**
 * App-level wrapper that reads shortcuts + modal state from the Zustand store.
 * Drop-in anywhere — no props required.
 */
function GlobalShortcutsHelpInner(
    { open, onClose }: GlobalShortcutsHelpProps = {},
): React.ReactElement
{
    const storeOpen = useShortcutsStore((s) => s.isHelpOpen);
    const storeClose = useShortcutsStore((s) => s.closeHelp);
    const shortcuts = useShortcutsStore((s) => s.shortcuts);

    return (
        <KeyboardShortcutsHelp
            open={open ?? storeOpen}
            onClose={onClose ?? storeClose}
            shortcuts={shortcuts}
        />
    );
}

export const GlobalShortcutsHelp = memo(GlobalShortcutsHelpInner);
