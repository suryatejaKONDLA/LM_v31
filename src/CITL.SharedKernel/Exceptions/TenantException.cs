namespace CITL.SharedKernel.Exceptions;

/// <summary>
/// Thrown when a tenant-related operation fails (resolution, isolation, or configuration).
/// Maps to HTTP 400 Bad Request.
/// </summary>
public sealed class TenantException : AppException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TenantException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public TenantException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
