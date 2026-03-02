/**
 * Guard clause utilities mirroring CITL.SharedKernel.Guards.Guard.
 * Throw early at function entry points to enforce preconditions.
 */
export const Guard = {
    /** Throws if the value is null or undefined. */
    notNull<T>(value: T | null | undefined, paramName: string): asserts value is T
    {
        if (value === null || value === undefined)
        {
            throw new Error(`${paramName} must not be null or undefined.`);
        }
    },

    /** Throws if the string is null, undefined, or empty. */
    notEmpty(value: string | null | undefined, paramName: string): asserts value is string
    {
        if (value === null || value === undefined || value.trim().length === 0)
        {
            throw new Error(`${paramName} must not be empty.`);
        }
    },

    /** Throws if the number is not positive. */
    positive(value: number, paramName: string): void
    {
        if (value <= 0)
        {
            throw new Error(`${paramName} must be positive. Got: ${String(value)}`);
        }
    },
} as const;
