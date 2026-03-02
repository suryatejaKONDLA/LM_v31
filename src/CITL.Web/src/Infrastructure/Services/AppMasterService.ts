import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";
import { type AppMasterResponse, type AppMasterRequest } from "@/Domain/Index";

/**
 * AppMaster API service.
 * Mirrors CITL.WebApi.Controllers.Core.Admin.AppMasterController.
 */
export const AppMasterService =
{
    /** GET AppMaster — retrieves the application master configuration. */
    get(signal?: AbortSignal): Promise<ApiResponseWithData<AppMasterResponse>>
    {
        return apiClient.get<AppMasterResponse>(
            ApiRoutes.AppMaster,
            undefined,
            signal,
        );
    },

    /** POST AppMaster — creates or updates the application master configuration. */
    addOrUpdate(request: AppMasterRequest): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(
            ApiRoutes.AppMaster,
            request,
        );
    },
} as const;
