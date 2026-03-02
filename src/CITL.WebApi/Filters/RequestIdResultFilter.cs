using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CITL.WebApi.Filters;

/// <summary>
/// Global result filter that stamps every <see cref="ApiResponse"/> with the current
/// request's correlation ID before the response is serialized.
/// </summary>
/// <remarks>
/// Registered as a global filter so that all controller actions automatically include
/// <see cref="ApiResponse.RequestId"/> without explicit code in each action method.
/// For exception responses, <see cref="Middleware.GlobalExceptionMiddleware"/> sets the ID directly.
/// </remarks>
public sealed class RequestIdResultFilter : IResultFilter
{
    /// <inheritdoc />
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult { Value: ApiResponse response })
        {
            response.RequestId = context.HttpContext.TraceIdentifier;
        }
    }

    /// <inheritdoc />
    public void OnResultExecuted(ResultExecutedContext context)
    {
        // No-op — work done in OnResultExecuting
    }
}
