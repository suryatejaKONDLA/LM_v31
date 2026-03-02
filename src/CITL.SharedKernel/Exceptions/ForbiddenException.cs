namespace CITL.SharedKernel.Exceptions;

/// <summary>
/// Thrown when the authenticated user lacks permission for the requested operation.
/// Maps to HTTP 403 Forbidden.
/// </summary>
public sealed class ForbiddenException : AppException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ForbiddenException(string message = "You do not have permission to perform this action.")
        : base(message)
    {
    }
}
