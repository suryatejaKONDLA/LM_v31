using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.SharedKernel.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Admin.Mappings.Mapping;

public sealed partial class MappingsService(
    IMappingsRepository repository,
    ICurrentUser currentUser,
    ITenantContext tenantContext,
    IValidator<MappingsRequest> validator,
    ILogger<MappingsService> logger) : IMappingsService
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Getting mappings for {QueryString} / AnchorId={AnchorId} (Tenant: {TenantId})")]
    private partial void LogGettingMappings(string queryString, string anchorId, string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Inserting mappings for {QueryString} (Tenant: {TenantId})")]
    private partial void LogInsertingMappings(string queryString, string tenantId);

    public async Task<Result<IReadOnlyList<MappingsResponse>>> GetByQueryStringAsync(
        string queryString,
        string anchorId,
        int swapFlag,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(queryString) || string.IsNullOrWhiteSpace(anchorId) || swapFlag is not (0 or 1))
        {
            return Result.Failure<IReadOnlyList<MappingsResponse>>(
                Error.Validation("Mappings.InvalidParams", "Invalid mapping parameters."));
        }

        LogGettingMappings(queryString, anchorId, tenantContext.TenantId);

        var result = await repository.GetByQueryStringAsync(queryString, anchorId, swapFlag, cancellationToken)
            .ConfigureAwait(false);

        return Result.Success(result);
    }

    public async Task<Result<IReadOnlyList<DropDownResponse<int>>>> GetLoginDropDownAsync(CancellationToken cancellationToken)
    {
        var result = await repository.GetLoginDropDownAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(result);
    }

    public async Task<Result<string>> InsertAsync(MappingsRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        if (!validation.IsValid)
        {
            return Result.Failure<string>(Error.Validation("Validation", validation.Errors[0].ErrorMessage));
        }

        LogInsertingMappings(request.QueryString, tenantContext.TenantId);

        var spResult = await repository.InsertAsync(request, currentUser.LoginId, cancellationToken)
            .ConfigureAwait(false);

        return spResult.ToMessageResult("Mappings.SaveFailed");
    }
}
