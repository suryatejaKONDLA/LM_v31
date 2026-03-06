import { useMemo, Suspense, type ReactNode } from "react";
import { Route } from "react-router-dom";
import { Spin } from "antd";
import { useMenuStore } from "@/Application/Index";
import { type Menu } from "@/Domain/Index";
import { getLazyComponent } from "./pageModules";

const FallbackSpinner = <Spin size="large" style={{ display: "flex", justifyContent: "center", marginTop: "40vh" }} />;

function flattenMenus(menus: Menu[]): Menu[]
{
    const result: Menu[] = [];

    const traverse = (items: Menu[]): void =>
    {
        for (const item of items)
        {
            if (item.MENU_URL1)
            {
                result.push(item);
            }

            if (item.Children.length > 0)
            {
                traverse(item.Children);
            }
        }
    };

    traverse(menus);
    return result;
}

function extractRoutePath(menuUrl: string): string
{
    const base = menuUrl.split("?")[ 0 ] ?? "";
    return base.startsWith("/") ? base : `/${base}`;
}

/**
 * Returns dynamic Route elements derived from the menu store.
 * Each menu's MENU_URL1 is matched to a page component via import.meta.glob.
 * Must be called inside a component that is a child of Routes — React Router
 * requires Route elements as direct children (components are not traversed).
 */
export function useDynamicRoutes(): ReactNode[]
{
    const menus = useMenuStore((s) => s.menus);

    const allMenus = useMemo(() => flattenMenus(menus), [ menus ]);

    return useMemo(() =>
        allMenus.map((menu) =>
        {
            if (!menu.MENU_URL1)
            {
                return null;
            }

            const routePath = extractRoutePath(menu.MENU_URL1);
            const fileName = routePath.split("/").pop();

            if (!fileName)
            {
                return null;
            }

            const Element = getLazyComponent(fileName);

            if (!Element)
            {
                return null;
            }

            return (
                <Route
                    key={menu.MENU_ID}
                    path={routePath}
                    element={
                        <Suspense fallback={FallbackSpinner}>
                            <Element />
                        </Suspense>
                    }
                />
            );
        }),
    [ allMenus ]);
}
