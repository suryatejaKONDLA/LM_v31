/**
 * Machine-readable numeric codes matching CITL.WebApi.Responses.ApiResponseCode.
 * 1 = success, 0 = warning, -1 = error.
 */
export const ApiResponseCode = {
    Success: 1,
    Warning: 0,
    Error: -1,
} as const;

export type ApiResponseCode = (typeof ApiResponseCode)[ keyof typeof ApiResponseCode ];
