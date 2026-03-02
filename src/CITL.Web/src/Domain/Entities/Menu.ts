/**
 * Menu item entity matching backend MenuResponse.
 * Property names match backend [JsonPropertyName] attributes exactly.
 * Mirrors CITL.Application.Core.Account.Menus.MenuResponse.
 */
export interface Menu
{
    MENU_ID: string;
    MENU_Name: string;
    MENU_Description: string | null;
    MENU_Parent_ID: string | null;
    MENU_URL1: string | null;
    MENU_URL2: string | null;
    MENU_URL3: string | null;
    MENU_Flag: string | null;
    MENU_Icon1: string | null;
    MENU_Icon2: string | null;
    MENU_Startup_Flag: boolean;
    Children: Menu[];
}
