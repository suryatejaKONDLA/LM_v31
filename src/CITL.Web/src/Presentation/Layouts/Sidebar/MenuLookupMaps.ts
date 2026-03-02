import type { Menu } from "@/Domain/Index";

/**
 * Precomputed lookup maps for O(1) menu operations.
 * Rebuilt only when menus change — avoids repeated tree traversal.
 */
export interface MenuLookupMaps
{
    /** Quick item lookup by MENU_ID. */
    byId: Map<string, Menu>;
    /** Parent ID for each menu item. null = root. */
    parentOf: Map<string, string | null>;
    /** Set of IDs that have children. */
    hasChildren: Set<string>;
    /** Ancestor chain from root down to (but not including) the item. */
    ancestorChain: Map<string, string[]>;
}

/** Pages where query string (QString1) must match to select the menu. */
export const QuerySensitivePages = [ "/gridviews/gridview3", "/admin/mappings/mappings" ];

/**
 * Builds lookup maps from a nested menu tree + favourites.
 * Called once via useMemo when menus change.
 */
export function buildLookupMaps(menus: Menu[]): MenuLookupMaps
{
    const byId = new Map<string, Menu>();
    const parentOf = new Map<string, string | null>();
    const hasChildren = new Set<string>();
    const ancestorChain = new Map<string, string[]>();

    const traverse = (items: Menu[], parentId: string | null, ancestors: string[]): void =>
    {
        for (const item of items)
        {
            byId.set(item.MENU_ID, item);
            parentOf.set(item.MENU_ID, parentId);
            ancestorChain.set(item.MENU_ID, ancestors);

            if (item.Children.length > 0)
            {
                hasChildren.add(item.MENU_ID);
                traverse(item.Children, item.MENU_ID, [ ...ancestors, item.MENU_ID ]);
            }
        }
    };

    traverse(menus, null, []);

    return { byId, parentOf, hasChildren, ancestorChain };
}
