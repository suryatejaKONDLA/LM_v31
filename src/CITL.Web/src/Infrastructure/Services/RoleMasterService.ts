import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";
import { type RoleResponse, type RoleMasterRequest, type DropDownItem } from "@/Domain/Index";

/**
 * RoleMaster API service.
 * Mirrors CITL.WebApi.Controllers.Core.Admin.RoleMasterController.
 */
export const RoleMasterService =
{
    /** GET RoleMaster?isApproved={isApproved} — retrieves all roles. */
    getAll(isApproved = true, signal?: AbortSignal): Promise<ApiResponseWithData<RoleResponse[]>>
    {
        return apiClient.get<RoleResponse[]>(
            `${ApiRoutes.RoleMaster}?isApproved=${String(isApproved)}`,
            undefined,
            signal,
        );
    },

    /** GET RoleMaster/DropDown?isApproved={isApproved} — dropdown list of roles. */
    getDropDown(isApproved = true, signal?: AbortSignal): Promise<ApiResponseWithData<DropDownItem<number>[]>>
    {
        return apiClient.get<DropDownItem<number>[]>(
            `${ApiRoutes.RoleMaster}/DropDown?isApproved=${String(isApproved)}`,
            undefined,
            signal,
        );
    },

    /** GET RoleMaster/{id} — retrieves a single role by ID. */
    getById(id: number, signal?: AbortSignal): Promise<ApiResponseWithData<RoleResponse>>
    {
        return apiClient.get<RoleResponse>(
            `${ApiRoutes.RoleMaster}/${String(id)}`,
            undefined,
            signal,
        );
    },

    /** POST RoleMaster — creates or updates a role. */
    addOrUpdate(request: RoleMasterRequest): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(
            ApiRoutes.RoleMaster,
            request,
        );
    },

    /** DELETE RoleMaster/{id} — deletes a role by ID. */
    delete(id: number): Promise<ApiResponseWithData<null>>
    {
        return apiClient.delete<null>(
            `${ApiRoutes.RoleMaster}/${String(id)}`,
        );
    },
} as const;
