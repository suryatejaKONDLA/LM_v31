import axios from "axios";

/**
 * Determines whether an error represents a cancelled/aborted HTTP request.
 * Covers both native AbortController (DOMException) and axios cancellation.
 */
export function isCancelledRequest(error: unknown): boolean
{
    if (error instanceof DOMException && error.name === "AbortError")
    {
        return true;
    }

    return axios.isCancel(error);
}

