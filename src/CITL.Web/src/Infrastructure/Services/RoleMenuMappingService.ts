import { apiClient } from "../Persistence/ApiClient";
import { ApiRoutes } from "../Persistence/ApiRoutes";
import { type RoleMenuMappingRequest, type RoleMenuMappingResponse } from "../../Domain/Entities/RoleMenuMapping";

class RoleMenuMappingService
{
    /**
   * Gets the mapped menus for a specific role.
   * @param roleId The Role ID
   */
    async getByRoleId(roleId: number): Promise<RoleMenuMappingResponse[]>
    {
        const response = await apiClient.get<RoleMenuMappingResponse[]>(`${ApiRoutes.RoleMenuMapping}/${String(roleId)}`);
        return response.Data;
    }

    /**
   * Saves role menu mappings in bulk.
   * @param request The complete role mapping state
   */
    async addOrUpdate(request: RoleMenuMappingRequest): Promise<string>
    {
        const response = await apiClient.post<string>(ApiRoutes.RoleMenuMapping as string, request);
        return response.Data || "Success";
    }
}

export const roleMenuMappingService = new RoleMenuMappingService();
