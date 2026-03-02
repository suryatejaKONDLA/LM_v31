/**
 * String utility functions.
 */
export const StringHelpers = {
    /** Returns true if the string is null, undefined, or whitespace-only. */
    isNullOrWhiteSpace(value: string | null | undefined): value is null | undefined
    {
        return value === null || value === undefined || value.trim().length === 0;
    },

    /** Truncates the string to the specified max length, appending ellipsis if needed. */
    truncate(value: string, maxLength: number): string
    {
        if (value.length <= maxLength)
        {
            return value;
        }

        return `${value.substring(0, maxLength)}...`;
    },
} as const;
