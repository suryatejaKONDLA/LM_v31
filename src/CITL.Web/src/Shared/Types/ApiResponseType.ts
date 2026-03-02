/**
 * String-based response category matching CITL.WebApi.Responses.ApiResponseType.
 */
export const ApiResponseType = {
    Success: "success",
    Error: "error",
    Warning: "warning",
    Info: "info",
} as const;

export type ApiResponseType = (typeof ApiResponseType)[keyof typeof ApiResponseType];
