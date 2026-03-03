using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.AppMaster;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CITL.Application.Tests.Core.Admin.AppMaster;

/// <summary>
/// Unit tests for <see cref="AppMasterService"/>.
/// Dependencies: IAppMasterRepository (mocked), real validator, NullLogger.
/// </summary>
public sealed class AppMasterServiceTests
{
    // ── Fixtures ──────────────────────────────────────────────────────────────

    private static AppMasterRequest ValidRequest => new()
    {
        AppCode = 1,
        AppHeader1 = "CITL Company",
        AppHeader2 = "CITL",
        SessionId = 10,
        BranchCode = 1
    };

    private static AppMasterResponse SampleResponse => new()
    {
        AppCode = 1,
        AppHeader1 = "CITL Company",
        AppHeader2 = "CITL",
        AppLink = "https://www.citl.co.in/",
        AppCreatedId = 1,
        AppCreatedDate = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    private static SpResult SuccessSpResult => new()
    {
        ResultVal = 1,
        ResultType = "SUCCESS",
        ResultMessage = "Saved successfully."
    };

    private static SpResult FailureSpResult => new()
    {
        ResultVal = 0,
        ResultType = "ERROR",
        ResultMessage = "Duplicate record."
    };

    private static AppMasterService CreateService(IAppMasterRepository? repository = null)
    {
        repository ??= Substitute.For<IAppMasterRepository>();
        var validator = new AppMasterRequestValidator();
        var logger = NullLogger<AppMasterService>.Instance;
        return new(repository, validator, logger);
    }

    // ── GetAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_WhenDataExists_ReturnsSuccessWithResponse()
    {
        // Arrange
        var repository = Substitute.For<IAppMasterRepository>();
        repository.GetAsync(Arg.Any<CancellationToken>()).Returns(SampleResponse);
        var service = CreateService(repository);

        // Act
        var result = await service.GetAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("CITL Company", result.Value.AppHeader1);
        Assert.Equal("CITL", result.Value.AppHeader2);
    }

    [Fact]
    public async Task GetAsync_WhenRepositoryReturnsNull_ReturnsNotFoundFailure()
    {
        // Arrange
        var repository = Substitute.For<IAppMasterRepository>();
        repository.GetAsync(Arg.Any<CancellationToken>()).Returns(default(AppMasterResponse));
        var service = CreateService(repository);

        // Act
        var result = await service.GetAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("NotFound", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetAsync_WhenDataExists_RepositoryCalledExactlyOnce()
    {
        // Arrange
        var repository = Substitute.For<IAppMasterRepository>();
        repository.GetAsync(Arg.Any<CancellationToken>()).Returns(SampleResponse);
        var service = CreateService(repository);

        // Act
        await service.GetAsync(CancellationToken.None);

        // Assert
        await repository.Received(1).GetAsync(Arg.Any<CancellationToken>());
    }

    // ── AddOrUpdateAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task AddOrUpdateAsync_WithValidRequest_WhenSpSucceeds_ReturnsSuccess()
    {
        // Arrange
        var repository = Substitute.For<IAppMasterRepository>();
        repository.AddOrUpdateAsync(Arg.Any<AppMasterRequest>(), Arg.Any<CancellationToken>())
            .Returns(SuccessSpResult);
        var service = CreateService(repository);

        // Act
        var result = await service.AddOrUpdateAsync(ValidRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AddOrUpdateAsync_WithValidRequest_WhenSpFails_ReturnsFailure()
    {
        // Arrange
        var repository = Substitute.For<IAppMasterRepository>();
        repository.AddOrUpdateAsync(Arg.Any<AppMasterRequest>(), Arg.Any<CancellationToken>())
            .Returns(FailureSpResult);
        var service = CreateService(repository);

        // Act
        var result = await service.AddOrUpdateAsync(ValidRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("AppMaster", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddOrUpdateAsync_WithEmptyAppHeader1_ReturnsValidationFailure_WithoutCallingRepository()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = "",
            AppHeader2 = "CITL",
            SessionId = 10,
            BranchCode = 1
        };
        var repository = Substitute.For<IAppMasterRepository>();
        var service = CreateService(repository);

        // Act
        var result = await service.AddOrUpdateAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Validation", result.Error.Code, StringComparison.OrdinalIgnoreCase);
        await repository.DidNotReceive().AddOrUpdateAsync(Arg.Any<AppMasterRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateAsync_WithZeroSessionId_ReturnsValidationFailure_WithoutCallingRepository()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = "CITL Company",
            AppHeader2 = "CITL",
            SessionId = 0,
            BranchCode = 1
        };
        var repository = Substitute.For<IAppMasterRepository>();
        var service = CreateService(repository);

        // Act
        var result = await service.AddOrUpdateAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Validation", result.Error.Code, StringComparison.OrdinalIgnoreCase);
        await repository.DidNotReceive().AddOrUpdateAsync(Arg.Any<AppMasterRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateAsync_WithValidRequest_RepositoryCalledExactlyOnce()
    {
        // Arrange
        var repository = Substitute.For<IAppMasterRepository>();
        repository.AddOrUpdateAsync(Arg.Any<AppMasterRequest>(), Arg.Any<CancellationToken>())
            .Returns(SuccessSpResult);
        var service = CreateService(repository);

        // Act
        await service.AddOrUpdateAsync(ValidRequest, CancellationToken.None);

        // Assert
        await repository.Received(1).AddOrUpdateAsync(
            Arg.Is<AppMasterRequest>(r => r.AppHeader1 == "CITL Company"),
            Arg.Any<CancellationToken>());
    }
}
