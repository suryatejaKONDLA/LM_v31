namespace CITL.SharedKernel.Exceptions;

/// <summary>
/// Thrown when an operation conflicts with the current state of a resource.
/// Maps to HTTP 409 Conflict.
/// </summary>
public sealed class ConflictException : AppException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ConflictException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class
    /// with a standardized message for duplicate entities.
    /// </summary>
    /// <param name="entityName">The name of the entity type.</param>
    /// <param name="conflictField">The field that caused the conflict.</param>
    /// <param name="conflictValue">The value that already exists.</param>
    public ConflictException(string entityName, string conflictField, object conflictValue)
        : base($"{entityName} with {conflictField} '{conflictValue}' already exists.")
    {
    }
}
