namespace CITL.SharedKernel.Exceptions;

/// <summary>
/// Base exception for all application-specific exceptions.
/// Never throw this directly — use a specific derived exception.
/// </summary>
public abstract class AppException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    protected AppException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    protected AppException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
