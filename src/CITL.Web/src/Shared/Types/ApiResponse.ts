import { type ApiResponseCode } from "./ApiResponseCode";
import { type ApiResponseType } from "./ApiResponseType";
import { type FieldError } from "./FieldError";

/**
 * Unified API response envelope matching CITL.WebApi.Responses.ApiResponse.
 * Every backend endpoint returns this shape.
 */
export interface ApiResponse
{
    RequestId?: string;
    Code: ApiResponseCode;
    Type: ApiResponseType;
    Message: string;
    Timestamp: string;
}

/**
 * API response with typed data payload.
 * Mirrors CITL.WebApi.Responses.ApiResponse{T}.
 */
export interface ApiResponseWithData<T> extends ApiResponse
{
    Data: T;
}

/**
 * API response for validation failures (HTTP 400).
 * Mirrors CITL.WebApi.Responses.ApiValidationResponse.
 */
export interface ApiValidationResponse extends ApiResponse
{
    Errors: FieldError[];
}

/**
 * Exception detail included only in Development environments.
 * Mirrors CITL.WebApi.Responses.ExceptionDetail.
 */
export interface ExceptionDetail
{
    Type: string;
    Message: string;
    StackTrace?: string;
}

/**
 * API response for server errors (HTTP 500).
 * Mirrors CITL.WebApi.Responses.ApiErrorResponse.
 */
export interface ApiErrorResponse extends ApiResponse
{
    Exception?: ExceptionDetail;
}
