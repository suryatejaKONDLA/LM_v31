import { create } from "zustand";
import { type Menu } from "@/Domain/Index";

/**
 * Menu navigation state store.
 * Mirrors CITL.Application menu retrieval use-case.
 */
interface MenuState
{
    menus: Menu[];
    isLoaded: boolean;
    setMenus: (menus: Menu[]) => void;
    clearMenus: () => void;
}

export const useMenuStore = create<MenuState>((set) => ({
    menus: [],
    isLoaded: false,

    setMenus: (menus: Menu[]) =>
    {
        set({ menus, isLoaded: true });
    },

    clearMenus: () =>
    {
        set({ menus: [], isLoaded: false });
    },
}));
