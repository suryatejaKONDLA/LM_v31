namespace CITL.SharedKernel.Results;

/// <summary>
/// Represents an error with a code and description.
/// Use as a value object to describe what went wrong without throwing exceptions.
/// </summary>
/// <param name="Code">A machine-readable error code (e.g., "User.NotFound").</param>
/// <param name="Description">A human-readable error description.</param>
#pragma warning disable CA1716 // Type name 'Error' conflicts with reserved keyword — intentional for Result pattern
public sealed record Error(string Code, string Description)
#pragma warning restore CA1716
{
    /// <summary>
    /// Represents no error. Used internally by successful <see cref="Result"/> instances.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Represents an error caused by a null value.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    /// <summary>
    /// Represents a generic server error.
    /// </summary>
    public static readonly Error ServerError = new("Error.ServerError", "An unexpected server error occurred.");

    /// <summary>
    /// Creates an error for a missing resource.
    /// </summary>
    /// <param name="entityName">The entity type name.</param>
    /// <param name="key">The lookup key.</param>
    /// <returns>A new <see cref="Error"/> instance.</returns>
    public static Error NotFound(string entityName, object key) =>
        new($"{entityName}.NotFound", $"{entityName} with key '{key}' was not found.");

    /// <summary>
    /// Creates an error for a validation failure.
    /// </summary>
    /// <param name="fieldName">The field that failed validation.</param>
    /// <param name="description">The validation error description.</param>
    /// <returns>A new <see cref="Error"/> instance.</returns>
    public static Error Validation(string fieldName, string description) =>
        new($"Validation.{fieldName}", description);

    /// <summary>
    /// Creates an error for a conflict (duplicate resource).
    /// </summary>
    /// <param name="entityName">The entity type name.</param>
    /// <param name="description">The conflict description.</param>
    /// <returns>A new <see cref="Error"/> instance.</returns>
    public static Error Conflict(string entityName, string description) =>
        new($"{entityName}.Conflict", description);
}
