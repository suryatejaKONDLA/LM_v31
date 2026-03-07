import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";
import { type LoginMasterResponse, type DropDownItem } from "@/Domain/Index";

/**
 * LoginMaster API service.
 * Mirrors CITL.WebApi.Controllers.Core.Admin.LoginMasterController.
 */
export const LoginMasterService =
{
    /** GET LoginMaster/{id} */
    getById(id: number, signal?: AbortSignal): Promise<ApiResponseWithData<LoginMasterResponse>>
    {
        return apiClient.get<LoginMasterResponse>(
            `${ApiRoutes.LoginMaster}/${String(id)}`,
            undefined,
            signal,
        );
    },

    /** GET LoginMaster/DropDown?isApproved={isApproved} */
    getDropDown(isApproved = true, signal?: AbortSignal): Promise<ApiResponseWithData<DropDownItem<number>[]>>
    {
        return apiClient.get<DropDownItem<number>[]>(
            `${ApiRoutes.LoginMaster}/DropDown?isApproved=${String(isApproved)}`,
            undefined,
            signal,
        );
    },

    /** POST LoginMaster — creates or updates a login. */
    addOrUpdate(request: unknown): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(
            ApiRoutes.LoginMaster,
            request,
        );
    },
} as const;
