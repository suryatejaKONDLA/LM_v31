/**
 * Imperative global spinner API.
 *
 * Provides static `show()` / `hide()` methods that set module-scoped state.
 * Consumers should render `<GlobalSpinnerHolder />` once (in Presentation)
 * and control visibility via `GlobalSpinner.show()` / `GlobalSpinner.hide()`.
 */
let _globalCallback: ((visible: boolean, tip?: string) => void) | null = null;

export const GlobalSpinner = {
    /**
     * Show the global spinner overlay.
     * @param tip Optional loading text. Defaults to "Loading…".
     */
    show: (tip?: string): void =>
    {
        _globalCallback?.(true, tip);
    },

    /** Hide the global spinner overlay. */
    hide: (): void =>
    {
        _globalCallback?.(false);
    },

    /**
     * Register the setter callback. Called internally by the holder component.
     * @internal
     */
    _register: (callback: (visible: boolean, tip?: string) => void): void =>
    {
        _globalCallback = callback;
    },

    /**
     * Unregister the setter callback. Called internally on unmount.
     * @internal
     */
    _unregister: (): void =>
    {
        _globalCallback = null;
    },
};
