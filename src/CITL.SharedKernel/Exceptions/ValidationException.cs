namespace CITL.SharedKernel.Exceptions;

/// <summary>
/// Thrown when one or more validation errors occur.
/// Maps to HTTP 400 Bad Request.
/// </summary>
public sealed class ValidationException : AppException
{
    /// <summary>
    /// Gets the validation errors grouped by field name.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="errors">The validation errors keyed by field name.</param>
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class
    /// with a single field error.
    /// </summary>
    /// <param name="fieldName">The field that failed validation.</param>
    /// <param name="errorMessage">The validation error message.</param>
    public ValidationException(string fieldName, string errorMessage)
        : base("One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>
        {
            [fieldName] = [errorMessage]
        };
    }
}
