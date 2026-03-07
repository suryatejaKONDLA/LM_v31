import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";
import { type BranchResponse, type DropDownItem } from "@/Domain/Index";

/**
 * BranchMaster API service.
 * Mirrors CITL.WebApi.Controllers.Core.Admin.BranchMasterController.
 */
export const BranchMasterService =
{
    /** GET BranchMaster?isActive={isActive}&isApproved={isApproved} */
    getAll(isActive = true, isApproved = true, signal?: AbortSignal): Promise<ApiResponseWithData<BranchResponse[]>>
    {
        return apiClient.get<BranchResponse[]>(
            `${ApiRoutes.BranchMaster}?isActive=${String(isActive)}&isApproved=${String(isApproved)}`,
            undefined,
            signal,
        );
    },

    /** GET BranchMaster/DropDown?isApproved={isApproved} */
    getDropDown(isApproved = true, signal?: AbortSignal): Promise<ApiResponseWithData<DropDownItem<number>[]>>
    {
        return apiClient.get<DropDownItem<number>[]>(
            `${ApiRoutes.BranchMaster}/DropDown?isApproved=${String(isApproved)}`,
            undefined,
            signal,
        );
    },

    /** GET BranchMaster/{id} */
    getById(id: number, signal?: AbortSignal): Promise<ApiResponseWithData<BranchResponse>>
    {
        return apiClient.get<BranchResponse>(
            `${ApiRoutes.BranchMaster}/${String(id)}`,
            undefined,
            signal,
        );
    },

    /** POST BranchMaster — creates or updates a branch. */
    addOrUpdate(request: unknown): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(
            ApiRoutes.BranchMaster,
            request,
        );
    },

    /** DELETE BranchMaster/{id} */
    delete(id: number): Promise<ApiResponseWithData<null>>
    {
        return apiClient.delete<null>(
            `${ApiRoutes.BranchMaster}/${String(id)}`,
        );
    },
} as const;
