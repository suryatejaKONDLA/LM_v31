namespace CITL.SharedKernel.Exceptions;

/// <summary>
/// Thrown when a requested resource could not be found.
/// Maps to HTTP 404 Not Found.
/// </summary>
public sealed class NotFoundException : AppException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class
    /// with a standardized message for entity lookups.
    /// </summary>
    /// <param name="entityName">The name of the entity type.</param>
    /// <param name="key">The lookup key value.</param>
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
    }
}
