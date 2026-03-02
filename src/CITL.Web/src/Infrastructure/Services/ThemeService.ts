import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";

/** Theme response from the backend. */
export interface ThemeResponse
{
    Login_ID: number;
    Theme_Json: string;
}

/** Request body for saving a theme. */
export interface SaveThemeRequest
{
    Theme_Json: string;
}

/**
 * Theme API service.
 * Mirrors CITL.WebApi.Controllers.Core.Account.ThemeController.
 */
export const ThemeService =
{
    /** GET Account/Theme — retrieves the current user's theme. */
    getTheme(signal?: AbortSignal): Promise<ApiResponseWithData<ThemeResponse>>
    {
        return apiClient.get<ThemeResponse>(
            `${ApiRoutes.Account}/Theme`,
            undefined,
            signal,
        );
    },

    /** PUT Account/Theme — saves the current user's theme. */
    saveTheme(request: SaveThemeRequest): Promise<ApiResponseWithData<null>>
    {
        return apiClient.put<null>(
            `${ApiRoutes.Account}/Theme`,
            request,
        );
    },
} as const;
