import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";
import { type DropDownItem } from "@/Domain/Index";

/**
 * GenderMaster API service.
 * Mirrors CITL.WebApi.Controllers.Core.Common.GenderMasterController.
 */
export const GenderMasterService =
{
    /** GET GenderMaster/DropDown */
    getDropDown(signal?: AbortSignal): Promise<ApiResponseWithData<DropDownItem<string>[]>>
    {
        return apiClient.get<DropDownItem<string>[]>(
            `${ApiRoutes.GenderMaster}/DropDown`,
            undefined,
            signal,
        );
    },
} as const;
