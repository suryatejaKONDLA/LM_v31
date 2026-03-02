namespace CITL.WebApi.Responses;

/// <summary>
/// String-based response category for the frontend to display or style on.
/// </summary>
public static class ApiResponseType
{
    /// <summary>Indicates the operation completed successfully.</summary>
    public const string Success = "success";

    /// <summary>Indicates the operation failed.</summary>
    public const string Error = "error";

    /// <summary>Indicates a warning condition.</summary>
    public const string Warning = "warning";

    /// <summary>Indicates an informational message.</summary>
    public const string Info = "info";
}
