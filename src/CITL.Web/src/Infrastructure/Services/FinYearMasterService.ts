import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";
import { type FinYearResponse, type FinYearMasterRequest } from "@/Domain/Index";

/**
 * FinYearMaster API service.
 */
export const FinYearMasterService =
{
    /** GET FinYearMaster */
    getAll(signal?: AbortSignal): Promise<ApiResponseWithData<FinYearResponse[]>>
    {
        return apiClient.get<FinYearResponse[]>(
            ApiRoutes.FinYearMaster,
            undefined,
            signal,
        );
    },

    /** GET FinYearMaster/{id} */
    getById(id: number, signal?: AbortSignal): Promise<ApiResponseWithData<FinYearResponse>>
    {
        return apiClient.get<FinYearResponse>(
            `${ApiRoutes.FinYearMaster}/${String(id)}`,
            undefined,
            signal,
        );
    },

    /** POST FinYearMaster */
    addOrUpdate(request: FinYearMasterRequest): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(
            ApiRoutes.FinYearMaster,
            request,
        );
    },

    /** DELETE FinYearMaster/{id} */
    delete(id: number): Promise<ApiResponseWithData<null>>
    {
        return apiClient.delete<null>(
            `${ApiRoutes.FinYearMaster}/${String(id)}`,
        );
    },
} as const;
