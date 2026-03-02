/**
 * Fixed theme tokens — cannot be overridden by the theme editor.
 * Custom user tokens (colorPrimary, fontSize, etc.) merge on top of these.
 */
export const ThemeConstants = {
    /** localStorage key for dark / light preference. */
    StorageKey: "citl-theme-mode",

    /** localStorage key for user-customized Ant Design seed tokens. */
    CustomTokensStorageKey: "citl-custom-theme-tokens",

    /** localStorage key for compact mode preference. */
    CompactStorageKey: "citl-theme-compact",

    /** localStorage key for happy work effect preference. */
    HappyWorkStorageKey: "citl-theme-happy-work",

    /** Immutable seed tokens applied to every theme. */
    FixedTokens: {
        fontFamily: "'Outfit', sans-serif",
    },
} as const;
