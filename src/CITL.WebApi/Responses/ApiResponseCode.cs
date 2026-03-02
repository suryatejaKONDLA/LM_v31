namespace CITL.WebApi.Responses;

/// <summary>
/// Machine-readable numeric codes for the frontend to branch on.
/// </summary>
public static class ApiResponseCode
{
    /// <summary>Operation completed successfully.</summary>
    public const int Success = 1;

    /// <summary>Operation completed with a warning.</summary>
    public const int Warning = 0;

    /// <summary>Operation failed.</summary>
    public const int Error = -1;
}
