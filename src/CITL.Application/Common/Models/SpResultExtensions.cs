using CITL.SharedKernel.Results;

namespace CITL.Application.Common.Models;

/// <summary>
/// Extension methods for converting <see cref="SpResult"/> to <see cref="Result"/>
/// without per-service boilerplate.
/// </summary>
public static class SpResultExtensions
{
    /// <summary>
    /// Converts a stored procedure result to a <see cref="Result"/>.
    /// Returns <see cref="Result.Success()"/> if the SP succeeded,
    /// otherwise returns a failure with the SP error message.
    /// </summary>
    /// <param name="spResult">The stored procedure result.</param>
    /// <param name="errorCode">The error code prefix (e.g., "AppMaster.SaveFailed").</param>
    /// <returns>A <see cref="Result"/> mapped from the SP output.</returns>
    public static Result ToResult(this SpResult spResult, string errorCode) =>
        spResult.IsSuccess
            ? Result.Success()
            : Result.Failure(new(errorCode, spResult.ResultMessage));

    /// <summary>
    /// Converts a stored procedure result to a <see cref="Result{T}"/> with the
    /// <see cref="SpResult.ResultVal"/> as the success value.
    /// </summary>
    /// <param name="spResult">The stored procedure result.</param>
    /// <returns>A <see cref="Result{T}"/> with the ResultVal on success, or the error.</returns>
    public static Result<int> ToResult(this SpResult spResult) =>
        spResult.IsSuccess
            ? Result.Success(spResult.ResultVal)
            : Result.Failure<int>(new("StoredProcedure.Failed", spResult.ResultMessage));

    /// <summary>
    /// Converts a stored procedure result to a <see cref="Result{T}"/> with the
    /// <see cref="SpResult.ResultMessage"/> as the success value.
    /// Used when the backend needs to pass the SP's dynamic success message to the API.
    /// </summary>
    /// <param name="spResult">The stored procedure result.</param>
    /// <param name="errorCode">The error code prefix (e.g., "AppMaster.SaveFailed").</param>
    /// <returns>A <see cref="Result{T}"/> mapped from the SP output.</returns>
    public static Result<string> ToMessageResult(this SpResult spResult, string errorCode) =>
        spResult.IsSuccess
            ? Result.Success(spResult.ResultMessage)
            : Result.Failure<string>(new(errorCode, spResult.ResultMessage));
}
