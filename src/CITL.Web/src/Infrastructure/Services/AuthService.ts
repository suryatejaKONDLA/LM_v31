import { apiClient, ApiRoutes } from "@/Infrastructure/Persistence/Index";
import { type ApiResponseWithData } from "@/Shared/Index";
import
{
    type LoginRequest,
    type LoginResponse,
    type RefreshTokenRequest,
    type CaptchaRequest,
    type CaptchaResponse,
    type ForgotPasswordRequest,
    type ResetPasswordRequest,
    type ResendVerificationRequest,
    type VerifyEmailRequest,
} from "@/Domain/Index";

/**
 * Authentication API service.
 * Mirrors CITL.WebApi.Controllers.Core.Authentication.AuthenticationController.
 */
export const AuthService =
{
    /** POST Auth/Captcha — generates CAPTCHA if threshold exceeded. */
    generateCaptcha(request: CaptchaRequest, signal?: AbortSignal): Promise<ApiResponseWithData<CaptchaResponse>>
    {
        return apiClient.post<CaptchaResponse>(
            `${ApiRoutes.Auth}/Captcha`,
            request,
            undefined,
            signal,
        );
    },

    /** POST Auth/Login — authenticates user and returns JWT tokens. */
    login(request: LoginRequest, signal?: AbortSignal): Promise<ApiResponseWithData<LoginResponse>>
    {
        return apiClient.post<LoginResponse>(
            `${ApiRoutes.Auth}/Login`,
            request,
            undefined,
            signal,
        );
    },

    /** POST Auth/Refresh — rotates access + refresh tokens. */
    refreshToken(request: RefreshTokenRequest, signal?: AbortSignal): Promise<ApiResponseWithData<LoginResponse>>
    {
        return apiClient.post<LoginResponse>(
            `${ApiRoutes.Auth}/Refresh`,
            request,
            undefined,
            signal,
        );
    },

    /** POST Auth/Logout — blacklists access token and revokes refresh token. */
    logout(): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(
            `${ApiRoutes.Auth}/Logout`,
            null,
        );
    },

    /** POST Auth/ForgotPassword — initiates password reset flow. */
    forgotPassword(request: ForgotPasswordRequest, signal?: AbortSignal): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(
            `${ApiRoutes.Auth}/ForgotPassword`,
            request,
            undefined,
            signal,
        );
    },

    /** POST Auth/ResetPassword — resets password using token from email. */
    resetPassword(request: ResetPasswordRequest, signal?: AbortSignal): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(
            `${ApiRoutes.Auth}/ResetPassword`,
            request,
            undefined,
            signal,
        );
    },

    /** POST Auth/ResendVerification — resends verification email. */
    resendVerification(request: ResendVerificationRequest, signal?: AbortSignal): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(
            `${ApiRoutes.Auth}/ResendVerification`,
            request,
            undefined,
            signal,
        );
    },

    /** POST Auth/VerifyEmail — verifies email using token from email link. */
    verifyEmail(request: VerifyEmailRequest, signal?: AbortSignal): Promise<ApiResponseWithData<null>>
    {
        return apiClient.post<null>(
            `${ApiRoutes.Auth}/VerifyEmail`,
            request,
            undefined,
            signal,
        );
    },
} as const;
