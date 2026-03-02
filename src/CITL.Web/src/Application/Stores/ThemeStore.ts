import { create } from "zustand";
import type { SeedToken } from "antd/es/theme/internal";
import { ThemeConstants } from "@/Shared/Index";

type ThemeMode = "light" | "dark";

/**
 * Theme state store.
 * Replaces the old React Context ThemeProvider with a zero-overhead zustand store.
 * No provider nesting — any component can read/write the theme without re-render cascades.
 */
interface ThemeState
{
    readonly mode: ThemeMode;
    readonly isDarkMode: boolean;
    readonly customTokens: Partial<SeedToken>;
    readonly isCompact: boolean;
    readonly isHappyWork: boolean;

    toggleMode: () => void;
    setMode: (mode: ThemeMode) => void;
    setCustomTokens: (tokens: Partial<SeedToken>) => void;
    resetTokens: () => void;
    setCompact: (value: boolean) => void;
    setHappyWork: (value: boolean) => void;
}

/** Read persisted mode from localStorage (default: light). */
const readStoredMode = (): ThemeMode =>
{
    const stored = localStorage.getItem(ThemeConstants.StorageKey);
    return stored === "dark" ? "dark" : "light";
};

/** Read persisted custom tokens from localStorage (default: empty). */
const readStoredTokens = (): Partial<SeedToken> =>
{
    try
    {
        const raw = localStorage.getItem(ThemeConstants.CustomTokensStorageKey);
        if (raw)
        {
            return JSON.parse(raw) as Partial<SeedToken>;
        }
    }
    catch
    {
        /* invalid JSON — ignore */
    }
    return {};
};

/** Read a persisted boolean flag from localStorage (default: false). */
const readStoredBool = (key: string): boolean =>
{
    return localStorage.getItem(key) === "true";
};

/** Apply the theme class to <html> synchronously. */
const applyHtmlClass = (mode: ThemeMode): void =>
{
    const html = document.documentElement;
    html.classList.remove("theme-light", "theme-dark");
    html.classList.add(`theme-${mode}`);
};

// Apply immediately on module load so first paint has the correct class.
const initialMode = readStoredMode();
applyHtmlClass(initialMode);

export const useThemeStore = create<ThemeState>((set) => ({
    mode: initialMode,
    isDarkMode: initialMode === "dark",
    customTokens: readStoredTokens(),
    isCompact: readStoredBool(ThemeConstants.CompactStorageKey),
    isHappyWork: readStoredBool(ThemeConstants.HappyWorkStorageKey),

    toggleMode: () =>
    {
        set((s) =>
        {
            const next: ThemeMode = s.mode === "light" ? "dark" : "light";
            localStorage.setItem(ThemeConstants.StorageKey, next);
            applyHtmlClass(next);
            return { mode: next, isDarkMode: next === "dark" };
        });
    },

    setMode: (mode: ThemeMode) =>
    {
        localStorage.setItem(ThemeConstants.StorageKey, mode);
        applyHtmlClass(mode);
        set({ mode, isDarkMode: mode === "dark" });
    },

    setCustomTokens: (tokens: Partial<SeedToken>) =>
    {
        localStorage.setItem(ThemeConstants.CustomTokensStorageKey, JSON.stringify(tokens));
        set({ customTokens: tokens });
    },

    resetTokens: () =>
    {
        localStorage.removeItem(ThemeConstants.CustomTokensStorageKey);
        set({ customTokens: {} });
    },

    setCompact: (value: boolean) =>
    {
        localStorage.setItem(ThemeConstants.CompactStorageKey, String(value));
        set({ isCompact: value });
    },

    setHappyWork: (value: boolean) =>
    {
        localStorage.setItem(ThemeConstants.HappyWorkStorageKey, String(value));
        set({ isHappyWork: value });
    },
}));
