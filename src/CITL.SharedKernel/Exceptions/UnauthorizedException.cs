namespace CITL.SharedKernel.Exceptions;

/// <summary>
/// Thrown when authentication is required but not provided or invalid.
/// Maps to HTTP 401 Unauthorized.
/// </summary>
public sealed class UnauthorizedException : AppException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public UnauthorizedException(string message = "Authentication is required.")
        : base(message)
    {
    }
}
