import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";
import { type MappingsRequest, type MappingsResponse, type DropDownItem } from "@/Domain/Index";

/**
 * Generic Mappings API service.
 * Mirrors CITL.WebApi.Controllers.Core.Admin.Mappings.MappingsController.
 */
export const MappingsService =
{
    /** GET Mappings?queryString=&anchorId=&swapFlag= — fetches existing mappings. */
    getByQueryString(
        queryString: string,
        anchorId: string,
        swapFlag: number,
        signal?: AbortSignal,
    ): Promise<ApiResponseWithData<MappingsResponse[]>>
    {
        return apiClient.get<MappingsResponse[]>(
            `${ApiRoutes.Mappings}?queryString=${encodeURIComponent(queryString)}&anchorId=${encodeURIComponent(anchorId)}&swapFlag=${String(swapFlag)}`,
            undefined,
            signal,
        );
    },

    /** GET Mappings/LoginDropDown — dropdown list of logins. */
    getLoginDropDown(signal?: AbortSignal): Promise<ApiResponseWithData<DropDownItem<number>[]>>
    {
        return apiClient.get<DropDownItem<number>[]>(
            `${ApiRoutes.Mappings}/LoginDropDown`,
            undefined,
            signal,
        );
    },

    /** POST Mappings — inserts or replaces mappings. */
    insert(request: MappingsRequest): Promise<ApiResponseWithData<string>>
    {
        return apiClient.post<string>(
            ApiRoutes.Mappings,
            request,
        );
    },
} as const;
