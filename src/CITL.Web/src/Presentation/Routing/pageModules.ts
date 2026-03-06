import { lazy, type LazyExoticComponent, type ComponentType } from "react";

/**
 * Vite glob registry for all page components.
 * Statically-routed pages are excluded to avoid double-registration.
 */
const modules = import.meta.glob<{ default: ComponentType }>(
    [
        "../Pages/**/*.tsx",
        "!../Pages/**/Login.tsx",
        "!../Pages/**/Home.tsx",
        "!../Pages/**/ErrorPages.tsx",
        "!../Pages/**/NetworkError.tsx",
        "!../Pages/**/Profile.tsx",
        "!../Pages/**/ChangePassword.tsx",
        "!../Pages/**/ThemeEditor.tsx",
        "!../Pages/**/SystemHealth.tsx",
        "!../Pages/**/SchedulerConfiguration.tsx",
    ],
    { eager: false },
);

const lazyCache = new Map<string, LazyExoticComponent<ComponentType>>();

/**
 * Resolves a page filename to a cached lazy component.
 * Returns null when no matching module exists.
 */
export function getLazyComponent(fileName: string): LazyExoticComponent<ComponentType> | null
{
    const cached = lazyCache.get(fileName);

    if (cached)
    {
        return cached;
    }

    const lowerName = fileName.toLowerCase();

    const moduleKey = Object.keys(modules).find((k) =>
    {
        const segment = k.split("/").pop();
        const moduleName = segment ? segment.replace(".tsx", "").toLowerCase() : "";
        return moduleName === lowerName;
    });

    if (!moduleKey)
    {
        return null;
    }

    const loader = modules[moduleKey];

    if (!loader)
    {
        return null;
    }

    const component = lazy(loader);
    lazyCache.set(fileName, component);
    return component;
}
