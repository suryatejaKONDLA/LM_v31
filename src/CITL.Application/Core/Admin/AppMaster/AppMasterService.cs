using CITL.Application.Common.Models;
using CITL.Application.Common.Validation;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Admin.AppMaster;

/// <summary>
/// Application service for App Master operations.
/// Uses FluentValidation for input validation and <see cref="SpResultExtensions"/>
/// for stored procedure result mapping — zero manual boilerplate.
/// </summary>
/// <param name="repository">The app master repository.</param>
/// <param name="validator">The FluentValidation validator for add/update requests.</param>
/// <param name="logger">The logger.</param>
public sealed partial class AppMasterService(
    IAppMasterRepository repository,
    IValidator<AppMasterRequest> validator,
    ILogger<AppMasterService> logger) : IAppMasterService
{
    /// <inheritdoc />
    public async Task<Result> AddOrUpdateAsync(
        AppMasterRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!validation.IsValid)
        {
            return validation.ToResult();
        }

        var spResult = await repository.AddOrUpdateAsync(request, cancellationToken).ConfigureAwait(false);

        if (!spResult.IsSuccess)
        {
            LogAddOrUpdateFailed(logger, spResult.ResultType, spResult.ResultMessage);
        }
        else
        {
            LogAddOrUpdateSucceeded(logger, spResult.ResultVal);
        }

        return spResult.ToResult("AppMaster.SaveFailed");
    }

    /// <inheritdoc />
    public async Task<Result<AppMasterResponse>> GetAsync(CancellationToken cancellationToken)
    {
        var response = await repository.GetAsync(cancellationToken).ConfigureAwait(false);

        return response is not null
            ? Result.Success(response)
            : Result.Failure<AppMasterResponse>(
                Error.NotFound(nameof(AppMaster), "Application configuration not found."));
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "AppMaster add/update failed — Type: {ResultType}, Message: {ResultMessage}")]
    private static partial void LogAddOrUpdateFailed(ILogger logger, string resultType, string resultMessage);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "AppMaster add/update succeeded — ResultVal: {ResultVal}")]
    private static partial void LogAddOrUpdateSucceeded(ILogger logger, int resultVal);
}
