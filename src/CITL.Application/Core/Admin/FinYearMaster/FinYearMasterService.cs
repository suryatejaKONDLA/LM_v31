using CITL.SharedKernel.Results;
using Microsoft.Extensions.Logging;

namespace CITL.Application.Core.Admin.FinYearMaster;

/// <inheritdoc />
internal sealed partial class FinYearMasterService(
    IFinYearMasterRepository repository,
    ILogger<FinYearMasterService> logger) : IFinYearMasterService
{
    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<FinYearResponse>>> GetAllAsync(CancellationToken cancellationToken)
    {
        LogGettingAll(logger);
        var items = await repository.GetAllAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success(items);
    }

    /// <inheritdoc />
    public async Task<Result<FinYearResponse>> GetByIdAsync(int finYear, CancellationToken cancellationToken)
    {
        LogGettingById(logger, finYear);
        var item = await repository.GetByIdAsync(finYear, cancellationToken).ConfigureAwait(false);

        return item is not null
            ? Result.Success(item)
            : Result.Failure<FinYearResponse>(Error.NotFound("FinYear.NotFound", $"Financial year {finYear} not found."));
    }

    /// <inheritdoc />
    public async Task<Result<string>> AddOrUpdateAsync(FinYearMasterRequest request, CancellationToken cancellationToken)
    {
        LogSaving(logger, request.FinYear);
        var sp = await repository.AddOrUpdateAsync(request, cancellationToken).ConfigureAwait(false);

        return sp.ResultVal > 0
            ? Result.Success(sp.ResultMessage ?? "Financial year saved.")
            : Result.Failure<string>(Error.Validation("FinYear.SaveFailed", sp.ResultMessage ?? "Save failed."));
    }

    /// <inheritdoc />
    public async Task<Result<string>> DeleteAsync(int finYear, CancellationToken cancellationToken)
    {
        LogDeleting(logger, finYear);
        var sp = await repository.DeleteAsync(finYear, cancellationToken).ConfigureAwait(false);

        return sp.ResultVal > 0
            ? Result.Success(sp.ResultMessage ?? "Financial year deleted.")
            : Result.Failure<string>(Error.Validation("FinYear.DeleteFailed", sp.ResultMessage ?? "Delete failed."));
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting all financial years")]
    private static partial void LogGettingAll(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting financial year {FinYear}")]
    private static partial void LogGettingById(ILogger logger, int finYear);

    [LoggerMessage(Level = LogLevel.Information, Message = "Saving financial year {FinYear}")]
    private static partial void LogSaving(ILogger logger, int finYear);

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleting financial year {FinYear}")]
    private static partial void LogDeleting(ILogger logger, int finYear);
}
