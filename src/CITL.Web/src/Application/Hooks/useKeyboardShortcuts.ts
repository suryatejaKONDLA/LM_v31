import { useEffect, useRef, useCallback } from "react";
import { useShortcutsStore, type ShortcutCategory, type ShortcutRegistration } from "../Stores/ShortcutsStore";

// ============================================
// TYPES
// ============================================

interface KeyboardShortcutsReturn
{
    /** Register a handler for an existing (default) shortcut. */
    readonly register: (id: string, handler: () => void) => void;
    /** Register a brand-new custom shortcut. */
    readonly registerCustom: (reg: Omit<ShortcutRegistration, "category"> & { category?: ShortcutCategory }) => void;
    /** Remove a shortcut entirely. */
    readonly unregister: (id: string) => void;
    /** Open the help modal. */
    readonly openHelp: () => void;
}

// ============================================
// useKeyboardShortcuts — generic hook
// ============================================

export function useKeyboardShortcuts(): KeyboardShortcutsReturn
{
    const registerShortcut = useShortcutsStore((s) => s.registerShortcut);
    const unregisterShortcut = useShortcutsStore((s) => s.unregisterShortcut);
    const openHelp = useShortcutsStore((s) => s.openHelp);

    const register = useCallback((id: string, handler: () => void) =>
    {
        const existing = useShortcutsStore.getState().shortcuts.get(id);
        if (!existing)
        {
            return;
        }

        registerShortcut({
            id: existing.id,
            keys: existing.keys,
            description: existing.description,
            category: existing.category,
            handler,
            preventDefault: existing.preventDefault,
        });
    }, [ registerShortcut ]);

    const registerCustom = useCallback(
        (reg: Omit<ShortcutRegistration, "category"> & { category?: ShortcutCategory }) =>
        {
            registerShortcut({ ...reg, category: reg.category ?? "custom" });
        },
        [ registerShortcut ],
    );

    const unregister = useCallback(
        (id: string) =>
        {
            unregisterShortcut(id);
        },
        [ unregisterShortcut ],
    );

    return { register, registerCustom, unregister, openHelp };
}

// ============================================
// useGlobalShortcuts — layout-level (Home, Search, Sidebar, Back, Forward, F1)
// ============================================

interface GlobalShortcutHandlers
{
    readonly onHome?: () => void;
    readonly onSearch?: () => void;
    readonly onToggleSidebar?: () => void;
    readonly onBack?: () => void;
    readonly onForward?: () => void;
}

export function useGlobalShortcuts(handlers: GlobalShortcutHandlers): void
{
    const registerShortcut = useShortcutsStore((s) => s.registerShortcut);
    const toggleHelp = useShortcutsStore((s) => s.toggleHelp);
    const handlersRef = useRef(handlers);
    const registeredRef = useRef(false);

    useEffect(() =>
    {
        handlersRef.current = handlers;
    });

    useEffect(() =>
    {
        if (registeredRef.current)
        {
            return;
        }
        registeredRef.current = true;

        const state = useShortcutsStore.getState();

        const wire = (id: string, key: keyof GlobalShortcutHandlers) =>
        {
            const def = state.shortcuts.get(id);
            if (!def || !handlersRef.current[key])
            {
                return;
            }
            registerShortcut({
                id: def.id,
                keys: def.keys,
                description: def.description,
                category: def.category,
                handler: () =>
                {
                    handlersRef.current[key]?.();
                },
                preventDefault: def.preventDefault,
            });
        };

        wire("home", "onHome");
        wire("search", "onSearch");
        wire("toggleSidebar", "onToggleSidebar");
        wire("back", "onBack");
        wire("forward", "onForward");

        // F1 always toggles help
        const helpDef = state.shortcuts.get("helpF1");
        if (helpDef)
        {
            registerShortcut({
                id: helpDef.id,
                keys: helpDef.keys,
                description: helpDef.description,
                category: helpDef.category,
                handler: () =>
                {
                    useShortcutsStore.getState().toggleHelp();
                },
                preventDefault: helpDef.preventDefault,
            });
        }
    }, [ registerShortcut, toggleHelp ]);
}

// ============================================
// useFormShortcuts — form-level (Save, Reset, Cancel)
// Handlers are cleared on unmount.
// ============================================

interface FormShortcutHandlers
{
    readonly onSave?: () => void;
    readonly onReset?: () => void;
    readonly onCancel?: () => void;
}

export function useFormShortcuts(handlers: FormShortcutHandlers): void
{
    const registerShortcut = useShortcutsStore((s) => s.registerShortcut);
    const handlersRef = useRef(handlers);

    useEffect(() =>
    {
        handlersRef.current = handlers;
    });

    useEffect(() =>
    {
        const state = useShortcutsStore.getState();

        const wire = (id: string, key: keyof FormShortcutHandlers) =>
        {
            const def = state.shortcuts.get(id);
            if (!def || !handlersRef.current[key])
            {
                return;
            }
            registerShortcut({
                id: def.id,
                keys: def.keys,
                description: def.description,
                category: def.category,
                handler: () =>
                {
                    handlersRef.current[key]?.();
                },
                preventDefault: def.preventDefault,
            });
        };

        wire("save", "onSave");
        wire("reset", "onReset");
        wire("cancel", "onCancel");

        return () =>
        {
            const clearHandler = (id: string) =>
            {
                const def = useShortcutsStore.getState().shortcuts.get(id);
                if (def)
                {
                    registerShortcut({
                        id: def.id,
                        keys: def.keys,
                        description: def.description,
                        category: def.category,
                        handler: () =>
                        {
                            /* cleared */
                        },
                        preventDefault: def.preventDefault,
                    });
                }
            };
            clearHandler("save");
            clearHandler("reset");
            clearHandler("cancel");
        };
    }, [ registerShortcut ]);
}

// ============================================
// useThemeShortcuts — theme toggle shortcuts (Ctrl+Alt+T/L/D)
// ============================================

interface ThemeShortcutActions
{
    readonly toggleMode: () => void;
    readonly setMode: (mode: "light" | "dark") => void;
}

export function useThemeShortcuts({ toggleMode, setMode }: ThemeShortcutActions): void
{
    const registerShortcut = useShortcutsStore((s) => s.registerShortcut);
    const unregisterShortcut = useShortcutsStore((s) => s.unregisterShortcut);
    const actionsRef = useRef({ toggleMode, setMode });

    useEffect(() =>
    {
        actionsRef.current = { toggleMode, setMode };
    });

    useEffect(() =>
    {
        registerShortcut({
            id: "theme.toggle",
            keys: "ctrl+alt+t",
            description: "Toggle Theme (Light/Dark)",
            category: "theme",
            handler: () =>
            {
                actionsRef.current.toggleMode();
            },
            preventDefault: true,
        });

        registerShortcut({
            id: "theme.light",
            keys: "ctrl+alt+l",
            description: "Switch to Light Theme",
            category: "theme",
            handler: () =>
            {
                actionsRef.current.setMode("light");
            },
            preventDefault: true,
        });

        registerShortcut({
            id: "theme.dark",
            keys: "ctrl+alt+d",
            description: "Switch to Dark Theme",
            category: "theme",
            handler: () =>
            {
                actionsRef.current.setMode("dark");
            },
            preventDefault: true,
        });

        return () =>
        {
            unregisterShortcut("theme.toggle");
            unregisterShortcut("theme.light");
            unregisterShortcut("theme.dark");
        };
    }, [ registerShortcut, unregisterShortcut ]);
}
