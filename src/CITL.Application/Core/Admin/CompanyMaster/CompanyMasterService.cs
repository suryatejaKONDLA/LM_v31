using CITL.Application.Common.Models;
using CITL.Application.Common.Validation;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Admin.CompanyMaster;

/// <summary>
/// Application service for Company Master operations.
/// Uses FluentValidation for input validation and <see cref="SpResultExtensions"/>
/// for stored procedure result mapping.
/// </summary>
/// <param name="repository">The company master repository.</param>
/// <param name="validator">The FluentValidation validator for add/update requests.</param>
/// <param name="logger">The logger.</param>
public sealed partial class CompanyMasterService(
    ICompanyMasterRepository repository,
    IValidator<CompanyMasterRequest> validator,
    ILogger<CompanyMasterService> logger) : ICompanyMasterService
{
    /// <inheritdoc />
    public async Task<Result> AddOrUpdateAsync(
        CompanyMasterRequest request,
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

        return spResult.ToResult("CompanyMaster.SaveFailed");
    }

    /// <inheritdoc />
    public async Task<Result<CompanyMasterResponse>> GetAsync(CancellationToken cancellationToken)
    {
        var response = await repository.GetAsync(cancellationToken).ConfigureAwait(false);

        return response is not null
            ? Result.Success(response)
            : Result.Failure<CompanyMasterResponse>(
                Error.NotFound(nameof(CompanyMaster), "Company configuration not found."));
    }

    [LoggerMessage(Level = LogLevel.Warning,
        Message = "CompanyMaster add/update failed — Type: {ResultType}, Message: {ResultMessage}")]
    private static partial void LogAddOrUpdateFailed(ILogger logger, string resultType, string resultMessage);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "CompanyMaster add/update succeeded — ResultVal: {ResultVal}")]
    private static partial void LogAddOrUpdateSucceeded(ILogger logger, int resultVal);
}
