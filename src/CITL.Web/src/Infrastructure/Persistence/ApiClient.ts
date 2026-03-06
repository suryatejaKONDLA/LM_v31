import axios, { AxiosHeaders, type AxiosError, type AxiosInstance, type AxiosRequestConfig, type AxiosResponse, type InternalAxiosRequestConfig } from "axios";
import { type ApiResponseWithData, ApiResponseCode, ApiResponseType, HttpStatusCode, MediaTypeConstants, TenantConstants, AppConfig } from "@/Shared/Index";
import { AuthStorage, TokenRefreshManager } from "@/Infrastructure/Authentication/Index";
import { ApiRoutes } from "./ApiRoutes";

const AUTH_HEADER = "Authorization";
const DEFAULT_TIMEOUT = 30000;

type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

/**
 * Centralised HTTP client matching BaseApiService from the old React app.
 * Mirrors CITL.Infrastructure.Persistence — a single gateway for all API calls.
 */
export class ApiClient
{
    private readonly client: AxiosInstance;

    /**
     * Mutex for token refresh — prevents multiple simultaneous refresh attempts.
     * When a 401 arrives, the first caller triggers the refresh. Subsequent 401s
     * wait on this promise and then retry with the new token.
     */
    private refreshPromise: Promise<boolean> | null = null;

    constructor()
    {
        this.client = axios.create({
            baseURL: AppConfig.ApiBaseUrl,
            timeout: DEFAULT_TIMEOUT,
            headers: {
                "Content-Type": MediaTypeConstants.Json,
                [ TenantConstants.HeaderName ]: AppConfig.TenantId,
            },
        });

        this.setupInterceptors();
    }

    // ─── Public HTTP methods ───────────────────────────────────────

    public get<T>(url: string, config?: AxiosRequestConfig, signal?: AbortSignal): Promise<ApiResponseWithData<T>>
    {
        const requestConfig = signal ? { ...config, signal } : config;
        return this.sendAsync<T>("GET", url, { config: requestConfig });
    }

    public post<TRes>(url: string, data: unknown, config?: AxiosRequestConfig, signal?: AbortSignal): Promise<ApiResponseWithData<TRes>>
    {
        const requestConfig = signal ? { ...config, signal } : config;
        return this.sendAsync<TRes>("POST", url, { data, config: requestConfig });
    }

    public put<TRes>(url: string, data: unknown, config?: AxiosRequestConfig): Promise<ApiResponseWithData<TRes>>
    {
        return this.sendAsync<TRes>("PUT", url, { data, config });
    }

    public patch<TRes>(url: string, data: unknown, config?: AxiosRequestConfig): Promise<ApiResponseWithData<TRes>>
    {
        return this.sendAsync<TRes>("PATCH", url, { data, config });
    }

    public delete<T>(url: string, config?: AxiosRequestConfig): Promise<ApiResponseWithData<T>>
    {
        return this.sendAsync<T>("DELETE", url, { config });
    }

    public postFormData<TRes>(url: string, formData: FormData, config?: AxiosRequestConfig): Promise<ApiResponseWithData<TRes>>
    {
        return this.sendAsync<TRes>("POST", url, {
            data: formData,
            config: ApiClient.buildFormDataConfig(config),
        });
    }

    // ─── Internals ─────────────────────────────────────────────────

    private setupInterceptors(): void
    {
        this.client.interceptors.request.use(
            (config) => this.onRequest(config),
            (error: unknown) => Promise.reject(error instanceof Error ? error : new Error("Request setup failed")),
        );

        this.client.interceptors.response.use(
            (response) => response,
            (error: unknown) =>
            {
                if (axios.isAxiosError(error))
                {
                    return this.onResponseError(error);
                }

                return Promise.reject(error instanceof Error ? error : new Error("Unknown response error"));
            },
        );
    }

    private onRequest(config: InternalAxiosRequestConfig): InternalAxiosRequestConfig
    {
        const token = AuthStorage.getAccessToken();

        if (token)
        {
            config.headers.set(AUTH_HEADER, `Bearer ${token}`);
        }

        return config;
    }

    private async onResponseError(error: AxiosError): Promise<AxiosResponse>
    {
        const status = error.response?.status;
        const originalRequest = error.config;

        // On 401 — attempt silent token refresh before redirecting
        if (
            status === HttpStatusCode.Unauthorized &&
            originalRequest &&
            !ApiClient.isRefreshRequest(originalRequest)
        )
        {
            const refreshed = await this.tryRefreshToken();

            if (refreshed)
            {
                // Retry the original request with the new token
                const newToken = AuthStorage.getAccessToken();

                if (newToken)
                {
                    originalRequest.headers.set(AUTH_HEADER, `Bearer ${newToken}`);
                }

                return this.client.request(originalRequest);
            }

            // Refresh failed — redirect to 403
            ApiClient.handleAuthError(HttpStatusCode.Unauthorized);
            return Promise.reject(error);
        }

        if (status === HttpStatusCode.Forbidden)
        {
            ApiClient.handleAuthError(status);
        }

        if (!error.response && error.code !== "ERR_CANCELED")
        {
            window.dispatchEvent(new CustomEvent("api-network-error"));
        }

        return Promise.reject(error);
    }

    /**
     * Attempts to refresh the access token using the stored refresh token.
     * Uses a mutex so only one refresh request fires at a time.
     * Returns true if tokens were successfully refreshed.
     */
    private async tryRefreshToken(): Promise<boolean>
    {
        // If a refresh is already in progress, wait for it
        if (this.refreshPromise)
        {
            return this.refreshPromise;
        }

        this.refreshPromise = this.executeTokenRefresh();

        try
        {
            return await this.refreshPromise;
        }
        finally
        {
            this.refreshPromise = null;
        }
    }

    private async executeTokenRefresh(): Promise<boolean>
    {
        const refreshToken = AuthStorage.getRefreshToken();
        const loginUser = AuthStorage.getLoginUser();

        if (!refreshToken || !loginUser)
        {
            return false;
        }

        try
        {
            // Call refresh endpoint directly via Axios to avoid recursion through interceptors
            const response = await this.client.post<ApiResponseWithData<{
                Access_Token: string;
                Refresh_Token: string;
            }>>(
                `${ApiRoutes.Auth}/Refresh`,
                { Refresh_Token: refreshToken, Login_User: loginUser },
            );

            const data = response.data;

            if (ApiClient.isApiResponse(data) && data.Code === ApiResponseCode.Success)
            {
                AuthStorage.setTokens(data.Data.Access_Token, data.Data.Refresh_Token);
                TokenRefreshManager.start();
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /**
     * Checks if this request is itself a refresh token request — prevents infinite loops.
     */
    private static isRefreshRequest(config: InternalAxiosRequestConfig): boolean
    {
        const url = config.url ?? "";
        return url.includes(`${ApiRoutes.Auth}/Refresh`);
    }

    private async sendAsync<TResponse>(
        method: HttpMethod,
        url: string,
        options?: { data?: unknown; config?: AxiosRequestConfig | undefined; },
    ): Promise<ApiResponseWithData<TResponse>>
    {
        try
        {
            const headers = ApiClient.buildHeaders(options?.config?.headers);
            const response = await this.client.request<ApiResponseWithData<TResponse>>({
                method,
                url,
                data: options?.data,
                ...options?.config,
                headers,
            });

            return ApiClient.normalizeResponse(response);
        }
        catch (error: unknown)
        {
            return ApiClient.handleError<TResponse>(error);
        }
    }

    // ─── Static helpers ────────────────────────────────────────────

    private static handleAuthError(status: number): void
    {
        TokenRefreshManager.stop();
        AuthStorage.clear();

        const message = status === HttpStatusCode.Unauthorized
            ? "Session Expired"
            : "Access Denied";

        const redirectUrl = `${AppConfig.BasePath}/403?msg=${encodeURIComponent(message)}`;

        if (!window.location.pathname.endsWith("/403"))
        {
            window.location.href = redirectUrl;
        }
    }

    private static buildHeaders(existing?: AxiosRequestConfig[ "headers" ]): AxiosHeaders
    {
        const headers = new AxiosHeaders({
            "Content-Type": MediaTypeConstants.Json,
            [ TenantConstants.HeaderName ]: AppConfig.TenantId,
        });

        if (existing)
        {
            const resolved = existing instanceof AxiosHeaders
                ? existing
                : new AxiosHeaders(existing as Record<string, string>);

            for (const [ key, value ] of Object.entries(resolved))
            {
                if (value !== undefined && value !== null)
                {
                    headers.set(key, String(value));
                }
            }
        }

        return headers;
    }

    private static normalizeResponse<T>(response: AxiosResponse<ApiResponseWithData<T>>): ApiResponseWithData<T>
    {
        const data = response.data;

        if (ApiClient.isApiResponse(data))
        {
            return data;
        }

        return ApiClient.createSuccessResponse(data as unknown as T);
    }

    private static isApiResponse<T>(data: unknown): data is ApiResponseWithData<T>
    {
        return (
            typeof data === "object" &&
            data !== null &&
            "Code" in data &&
            "Type" in data &&
            "Message" in data
        );
    }

    private static createSuccessResponse<T>(data: T): ApiResponseWithData<T>
    {
        return {
            Code: ApiResponseCode.Success,
            Type: ApiResponseType.Success,
            Message: "OK",
            Data: data,
            Timestamp: new Date().toISOString(),
        } satisfies ApiResponseWithData<T>;
    }

    private static handleError<T>(error: unknown): ApiResponseWithData<T>
    {
        if (axios.isAxiosError(error))
        {
            // Re-throw cancelled requests — let callers handle AbortSignal cleanup.
            if (axios.isCancel(error) || error.code === "ERR_CANCELED")
            {
                throw error;
            }

            return ApiClient.handleAxiosError<T>(error);
        }

        return {
            Code: ApiResponseCode.Error,
            Type: ApiResponseType.Error,
            Message: "Unexpected Error",
            Data: undefined as unknown as T,
            Timestamp: new Date().toISOString(),
        };
    }

    private static handleAxiosError<T>(error: AxiosError): ApiResponseWithData<T>
    {
        const response = error.response;

        if (response?.data && ApiClient.isApiResponse<T>(response.data))
        {
            return response.data;
        }

        return {
            Code: ApiResponseCode.Error,
            Type: ApiResponseType.Error,
            Message: error.message,
            Data: undefined as unknown as T,
            Timestamp: new Date().toISOString(),
        };
    }

    private static buildFormDataConfig(config?: AxiosRequestConfig): AxiosRequestConfig
    {
        const headers = new AxiosHeaders({
            "Content-Type": MediaTypeConstants.FormData,
        });

        if (config?.headers)
        {
            const existing = config.headers instanceof AxiosHeaders
                ? config.headers
                : new AxiosHeaders(config.headers as Record<string, string>);

            for (const [ key, value ] of Object.entries(existing))
            {
                if (value !== undefined && value !== null)
                {
                    headers.set(key, String(value));
                }
            }
        }

        return {
            ...config,
            headers,
        };
    }
}

/** Singleton API client instance — use this throughout the app. */
export const apiClient = new ApiClient();
