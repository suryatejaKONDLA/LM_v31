import { create } from "zustand";

// ============================================
// TYPES
// ============================================

/** Logical grouping for keyboard shortcuts. */
export type ShortcutCategory = "global" | "navigation" | "form" | "theme" | "custom";

/** A single keyboard shortcut definition (metadata only). */
export interface ShortcutDefinition
{
    readonly id: string;
    readonly keys: string;
    readonly description: string;
    readonly category: ShortcutCategory;
    readonly enabled: boolean;
    readonly preventDefault: boolean;
}

/** Registration payload — includes the handler callback. */
export interface ShortcutRegistration
{
    readonly id: string;
    readonly keys: string;
    readonly description: string;
    readonly category: ShortcutCategory;
    readonly handler: () => void;
    readonly preventDefault?: boolean;
}

// ============================================
// INTERNAL — Key parsing & matching
// ============================================

interface ParsedKey
{
    key: string;
    ctrl: boolean;
    alt: boolean;
    shift: boolean;
    meta: boolean;
}

function parseKeyCombination(keys: string): ParsedKey
{
    const parts = keys.toLowerCase().split("+");
    const result: ParsedKey = { key: "", ctrl: false, alt: false, shift: false, meta: false };

    for (const part of parts)
    {
        const trimmed = part.trim();
        switch (trimmed)
        {
            case "ctrl":
            case "control":
                result.ctrl = true;
                break;
            case "alt":
                result.alt = true;
                break;
            case "shift":
                result.shift = true;
                break;
            case "meta":
            case "cmd":
            case "command":
                result.meta = true;
                break;
            default:
                result.key = trimmed;
        }
    }

    return result;
}

function normalizeEventKey(event: KeyboardEvent): string
{
    const key = event.key.toLowerCase();
    switch (key)
    {
        case "arrowleft": return "left";
        case "arrowright": return "right";
        case "arrowup": return "up";
        case "arrowdown": return "down";
        case " ": return "space";
        default:
            // For "/" key, also check code for keyboard layouts
            if (event.code === "Slash")
            {
                return "/";
            }
            return key;
    }
}

function matchesKeyEvent(parsed: ParsedKey, event: KeyboardEvent): boolean
{
    return (
        parsed.key === normalizeEventKey(event) &&
        parsed.ctrl === event.ctrlKey &&
        parsed.alt === event.altKey &&
        parsed.shift === event.shiftKey &&
        parsed.meta === event.metaKey
    );
}

// ============================================
// MODULE-LEVEL STATE (outside React)
// Handlers and parsed keys live here — never in Zustand state.
// Updating a handler does NOT trigger any React re-render.
// ============================================

const handlers = new Map<string, () => void>();
const parsedKeys = new Map<string, ParsedKey>();

// ============================================
// DEFAULT SHORTCUTS
// ============================================

const DefaultShortcuts: readonly Omit<ShortcutDefinition, "enabled" | "preventDefault">[] = [
    // Global
    { id: "home", keys: "ctrl+h", description: "Go to Home", category: "global" },
    { id: "search", keys: "ctrl+/", description: "Toggle Search", category: "global" },
    { id: "toggleSidebar", keys: "ctrl+b", description: "Toggle Sidebar", category: "global" },
    { id: "helpF1", keys: "f1", description: "Show Keyboard Shortcuts", category: "global" },

    // Navigation
    { id: "back", keys: "alt+left", description: "Go Back", category: "navigation" },
    { id: "forward", keys: "alt+right", description: "Go Forward", category: "navigation" },

    // Form
    { id: "save", keys: "ctrl+s", description: "Save Form", category: "form" },
    { id: "reset", keys: "ctrl+r", description: "Reset Form", category: "form" },
    { id: "cancel", keys: "escape", description: "Cancel / Close", category: "form" },
];

function buildInitialShortcuts(): Map<string, ShortcutDefinition>
{
    const map = new Map<string, ShortcutDefinition>();
    for (const s of DefaultShortcuts)
    {
        const def: ShortcutDefinition = { ...s, enabled: true, preventDefault: s.id !== "cancel" };
        map.set(s.id, def);
        parsedKeys.set(s.id, parseKeyCombination(s.keys));
    }
    return map;
}

// ============================================
// STORE
// ============================================

interface ShortcutsState
{
    /** Shortcut metadata — drives the help modal UI. */
    readonly shortcuts: ReadonlyMap<string, ShortcutDefinition>;
    /** Whether the help modal is visible. */
    readonly isHelpOpen: boolean;
    /** Master kill-switch for all shortcuts. */
    readonly globalEnabled: boolean;

    registerShortcut: (registration: ShortcutRegistration) => void;
    unregisterShortcut: (id: string) => void;
    setShortcutEnabled: (id: string, enabled: boolean) => void;
    openHelp: () => void;
    closeHelp: () => void;
    toggleHelp: () => void;
    setGlobalEnabled: (enabled: boolean) => void;
}

export const useShortcutsStore = create<ShortcutsState>((set, get) => ({
    shortcuts: buildInitialShortcuts(),
    isHelpOpen: false,
    globalEnabled: true,

    registerShortcut: (reg: ShortcutRegistration) =>
    {
        // Always update handler in module-level Map — zero re-renders.
        handlers.set(reg.id, reg.handler);
        parsedKeys.set(reg.id, parseKeyCombination(reg.keys));

        // Only update Zustand state if metadata actually changed.
        const prev = get().shortcuts.get(reg.id);
        if (
            prev?.keys === reg.keys &&
            prev.description === reg.description &&
            prev.category === reg.category &&
            prev.enabled &&
            prev.preventDefault === (reg.preventDefault ?? true)
        )
        {
            return; // No state change — no re-render.
        }

        const next = new Map(get().shortcuts);
        next.set(reg.id, {
            id: reg.id,
            keys: reg.keys,
            description: reg.description,
            category: reg.category,
            enabled: true,
            preventDefault: reg.preventDefault ?? true,
        });
        set({ shortcuts: next });
    },

    unregisterShortcut: (id: string) =>
    {
        handlers.delete(id);
        parsedKeys.delete(id);

        if (!get().shortcuts.has(id))
        {
            return;
        }

        const next = new Map(get().shortcuts);
        next.delete(id);
        set({ shortcuts: next });
    },

    setShortcutEnabled: (id: string, enabled: boolean) =>
    {
        const existing = get().shortcuts.get(id);
        if (!existing || existing.enabled === enabled)
        {
            return;
        }

        const next = new Map(get().shortcuts);
        next.set(id, { ...existing, enabled });
        set({ shortcuts: next });
    },

    openHelp: () =>
    {
        set({ isHelpOpen: true });
    },
    closeHelp: () =>
    {
        set({ isHelpOpen: false });
    },
    toggleHelp: () =>
    {
        set((s) => ({ isHelpOpen: !s.isHelpOpen }));
    },
    setGlobalEnabled: (enabled: boolean) =>
    {
        set({ globalEnabled: enabled });
    },
}));

// ============================================
// GLOBAL KEYDOWN LISTENER
// Runs once at module load — never attached/detached by React.
// Uses module-level maps + getState() — zero dependency on React lifecycle.
// ============================================

function handleKeyDown(event: KeyboardEvent): void
{
    const state = useShortcutsStore.getState();
    if (!state.globalEnabled || !event.key)
    {
        return;
    }

    // Skip when typing in inputs (except form shortcuts & search)
    const target = event.target as HTMLElement;
    const isTyping =
        target.tagName === "INPUT" ||
        target.tagName === "TEXTAREA" ||
        target.isContentEditable;

    for (const [ id, parsed ] of parsedKeys)
    {
        if (!matchesKeyEvent(parsed, event))
        {
            continue;
        }

        const def = state.shortcuts.get(id);
        if (!def?.enabled)
        {
            continue;
        }

        // Allow form shortcuts and search even when typing; skip others.
        if (isTyping && def.category !== "form" && def.id !== "search")
        {
            continue;
        }

        if (def.preventDefault)
        {
            event.preventDefault();
            event.stopPropagation();
        }

        const handler = handlers.get(id);
        handler?.();
        break;
    }
}

window.addEventListener("keydown", handleKeyDown);
