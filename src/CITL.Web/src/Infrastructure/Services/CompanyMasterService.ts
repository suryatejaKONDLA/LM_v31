import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";
import { type CompanyMasterResponse, type CompanyMasterRequest } from "@/Domain/Index";

/**
 * CompanyMaster API service.
 * Mirrors CITL.WebApi.Controllers.Core.Admin.CompanyMasterController.
 */
export const CompanyMasterService =
{
    /** GET CompanyMaster — retrieves the company master configuration. */
    get(signal?: AbortSignal): Promise<ApiResponseWithData<CompanyMasterResponse>>
    {
        return apiClient.get<CompanyMasterResponse>(
            ApiRoutes.CompanyMaster,
            undefined,
            signal,
        );
    },

    /** POST CompanyMaster — creates or updates the company master configuration. */
    addOrUpdate(request: CompanyMasterRequest): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(
            ApiRoutes.CompanyMaster,
            request,
        );
    },
} as const;
