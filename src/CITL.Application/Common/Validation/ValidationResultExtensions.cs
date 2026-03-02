using CITL.SharedKernel.Results;
using FluentValidation.Results;

namespace CITL.Application.Common.Validation;

/// <summary>
/// Extension methods that bridge FluentValidation <see cref="ValidationResult"/>
/// to the CITL <see cref="Result"/> pattern, eliminating per-service validation boilerplate.
/// </summary>
public static class ValidationResultExtensions
{
    /// <summary>
    /// Converts a FluentValidation result to a <see cref="Result"/>.
    /// Returns <see cref="Result.Success()"/> if valid, otherwise returns a failure
    /// with the first validation error.
    /// </summary>
    /// <param name="validationResult">The FluentValidation result.</param>
    /// <returns>A <see cref="Result"/> mapped from the validation outcome.</returns>
    public static Result ToResult(this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            return Result.Success();
        }

        var first = validationResult.Errors[0];
        return Result.Failure(Error.Validation(first.PropertyName, first.ErrorMessage));
    }

    /// <summary>
    /// Converts a FluentValidation result to a <see cref="Result{T}"/>.
    /// Returns a failure with the first validation error if invalid.
    /// </summary>
    /// <typeparam name="T">The expected success value type.</typeparam>
    /// <param name="validationResult">The FluentValidation result.</param>
    /// <returns>A failed <see cref="Result{T}"/> if invalid.</returns>
    public static Result<T> ToResult<T>(this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            throw new InvalidOperationException(
                "Cannot convert a valid ValidationResult to Result<T> without a value. " +
                "Use ToResult() for non-generic results, or provide a value after validation.");
        }

        var first = validationResult.Errors[0];
        return Result.Failure<T>(Error.Validation(first.PropertyName, first.ErrorMessage));
    }

    /// <summary>
    /// Converts a FluentValidation result to a <see cref="SharedKernel.Exceptions.ValidationException"/>
    /// with all field-level errors grouped by property name.
    /// </summary>
    /// <param name="validationResult">The FluentValidation result.</param>
    /// <returns>A <see cref="SharedKernel.Exceptions.ValidationException"/> with all errors.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the validation result is valid.</exception>
    public static SharedKernel.Exceptions.ValidationException ToValidationException(
        this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            throw new InvalidOperationException("Cannot create a ValidationException from a valid result.");
        }

        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return new(errors);
    }
}
