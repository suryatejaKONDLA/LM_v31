namespace CITL.Application.Common.Models;

/// <summary>
/// Represents the standard output from CITL stored procedures that return
/// <c>@ResultVal</c>, <c>@ResultType</c>, and <c>@ResultMessage</c> output parameters.
/// </summary>
/// <remarks>
/// All CITL stored procedures follow the same output contract.
/// Use <see cref="SpResultExtensions"/> to convert to <see cref="SharedKernel.Results.Result"/>
/// without boilerplate.
/// </remarks>
public sealed class SpResult
{
    /// <summary>Gets the result value (e.g., the inserted/updated record key).</summary>
    public int ResultVal { get; init; }

    /// <summary>Gets the result type (e.g., "SUCCESS", "ERROR", "WARNING").</summary>
    public string ResultType { get; init; } = string.Empty;

    /// <summary>Gets the human-readable result message.</summary>
    public string ResultMessage { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the stored procedure completed successfully.
    /// </summary>
    public bool IsSuccess => string.Equals(ResultType, "SUCCESS", StringComparison.OrdinalIgnoreCase);
}
