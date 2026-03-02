import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";
import { type MailMasterResponse, type MailMasterRequest, type DropDownItem } from "@/Domain/Index";

/**
 * MailMaster API service.
 * Mirrors CITL.WebApi.Controllers.Core.Admin.MailMasterController.
 */
export const MailMasterService =
{
    /** GET MailMaster?isApproved={isApproved} — retrieves all mail configurations. */
    getAll(isApproved = true, signal?: AbortSignal): Promise<ApiResponseWithData<MailMasterResponse[]>>
    {
        return apiClient.get<MailMasterResponse[]>(
            `${ApiRoutes.MailMaster}?isApproved=${String(isApproved)}`,
            undefined,
            signal,
        );
    },

    /** GET MailMaster/DropDown?isApproved={isApproved} — dropdown list of mail configs. */
    getDropDown(isApproved = true, signal?: AbortSignal): Promise<ApiResponseWithData<DropDownItem<number>[]>>
    {
        return apiClient.get<DropDownItem<number>[]>(
            `${ApiRoutes.MailMaster}/DropDown?isApproved=${String(isApproved)}`,
            undefined,
            signal,
        );
    },

    /** GET MailMaster/{id} — retrieves a single mail configuration. */
    getById(id: number, signal?: AbortSignal): Promise<ApiResponseWithData<MailMasterResponse>>
    {
        return apiClient.get<MailMasterResponse>(
            `${ApiRoutes.MailMaster}/${String(id)}`,
            undefined,
            signal,
        );
    },

    /** POST MailMaster — creates or updates a mail configuration. */
    addOrUpdate(request: MailMasterRequest): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(
            ApiRoutes.MailMaster,
            request,
        );
    },

    /** DELETE MailMaster/{id} — deletes a mail configuration. */
    delete(id: number): Promise<ApiResponseWithData<null>>
    {
        return apiClient.delete<null>(
            `${ApiRoutes.MailMaster}/${String(id)}`,
        );
    },
} as const;
