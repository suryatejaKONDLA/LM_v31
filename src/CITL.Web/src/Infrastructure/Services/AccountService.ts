import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";
import { type ProfileResponse, type UpdateProfileRequest, type ChangePasswordRequest } from "@/Domain/Index";

/**
 * Account API service.
 * Mirrors CITL.WebApi.Controllers.Core.Account.AccountController.
 */
export const AccountService =
{
    /** GET Account/Profile — retrieves the current user's profile. */
    getProfile(signal?: AbortSignal): Promise<ApiResponseWithData<ProfileResponse>>
    {
        return apiClient.get<ProfileResponse>(
            `${ApiRoutes.Account}/Profile`,
            undefined,
            signal,
        );
    },

    /** PUT Account/Profile — updates the current user's profile. */
    updateProfile(request: UpdateProfileRequest): Promise<ApiResponseWithData<null>>
    {
        return apiClient.put<null>(
            `${ApiRoutes.Account}/Profile`,
            request,
        );
    },

    /** PUT Account/Password — changes the current user's password. */
    changePassword(request: ChangePasswordRequest): Promise<ApiResponseWithData<null>>
    {
        return apiClient.put<null>(
            `${ApiRoutes.Account}/Password`,
            request,
        );
    },
} as const;
