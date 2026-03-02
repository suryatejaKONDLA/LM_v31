import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";
import { type Menu } from "@/Domain/Index";

/**
 * Menu API service.
 * Mirrors CITL.WebApi.Controllers.Core.Account.MenuController.
 */
export const MenuService =
{
    /**
     * GET Menu/{loginId}?asTree={asTree}
     * Retrieves navigation menus for the specified login.
     * @param loginId  The login identifier.
     * @param asTree   When true, returns parent–child tree; when false, flat list.
     * @param signal   Optional AbortSignal for cancellation.
     */
    getMenus(loginId: number, asTree = false, signal?: AbortSignal): Promise<ApiResponseWithData<Menu[]>>
    {
        return apiClient.get<Menu[]>(
            `${ApiRoutes.Menu}/${String(loginId)}?asTree=${String(asTree)}`,
            undefined,
            signal,
        );
    },
} as const;
