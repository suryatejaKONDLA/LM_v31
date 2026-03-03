using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.MailMaster;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CITL.Application.Tests.Core.Admin.MailMaster;

/// <summary>
/// Unit tests for <see cref="MailMasterService"/>.
/// Dependencies: IMailMasterRepository, ICurrentUser, ICacheService, ITenantContext (mocked),
/// real validator, NullLogger.
/// </summary>
public sealed class MailMasterServiceTests
{
    // ── Fixtures ──────────────────────────────────────────────────────

    private readonly IMailMasterRepository _repository = Substitute.For<IMailMasterRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly MailMasterService _service;

    public MailMasterServiceTests()
    {
        _currentUser.LoginId.Returns(1);
        _tenantContext.TenantId.Returns("T1");

        _service = new(
            _repository,
            _currentUser,
            _cacheService,
            _tenantContext,
            new MailMasterRequestValidator(),
            NullLogger<MailMasterService>.Instance);
    }

    private static MailMasterRequest ValidRequest => new()
    {
        MailSNo = 0,
        MailBranchCode = 1,
        MailFromAddress = "noreply@example.com",
        MailFromPassword = "password123",
        MailDisplayName = "CITL Mailer",
        MailHost = "smtp.example.com",
        MailPort = 587,
        MailSslEnabled = true,
        MailMaxRecipients = 50,
        MailRetryAttempts = 3,
        MailRetryIntervalMinutes = 5,
        MailIsActive = true,
        MailIsDefault = true
    };

    private static MailMasterResponse SampleMail => new()
    {
        MailSNo = 1,
        MailBranchCode = 1,
        MailFromAddress = "noreply@example.com",
        MailDisplayName = "CITL Mailer",
        MailHost = "smtp.example.com",
        MailPort = 587,
        MailSslEnabled = true,
        MailIsActive = true
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
        ResultMessage = "Duplicate configuration."
    };

    // ── GetAllAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsCachedOrFreshItems()
    {
        // Arrange
        IReadOnlyList<MailMasterResponse> items = [SampleMail];
        _cacheService.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, Task<IReadOnlyList<MailMasterResponse>>>>(),
            Arg.Any<CacheEntryOptions>(),
            Arg.Any<CancellationToken>()).Returns(items);

        // Act
        var result = await _service.GetAllAsync(true, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("noreply@example.com", result.Value[0].MailFromAddress);
    }

    // ── GetDropDownAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetDropDownAsync_ReturnsCachedOrFreshItems()
    {
        // Arrange
        IReadOnlyList<DropDownResponse<int>> items = [new() { Value = 1, Text = "noreply@example.com" }];
        _cacheService.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, Task<IReadOnlyList<DropDownResponse<int>>>>>(),
            Arg.Any<CacheEntryOptions>(),
            Arg.Any<CancellationToken>()).Returns(items);

        // Act
        var result = await _service.GetDropDownAsync(true, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_CacheHit_ReturnsFromCacheWithoutCallingRepository()
    {
        // Arrange
        _cacheService.GetAsync<MailMasterResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(SampleMail);

        // Act
        var result = await _service.GetByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("noreply@example.com", result.Value.MailFromAddress);
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_CacheMiss_ReturnsFromRepositoryAndCaches()
    {
        // Arrange
        _cacheService.GetAsync<MailMasterResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(default(MailMasterResponse?));
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(SampleMail);

        // Act
        var result = await _service.GetByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _cacheService.Received(1).SetAsync(
            Arg.Any<string>(), Arg.Any<MailMasterResponse>(), Arg.Any<CacheEntryOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_CacheMissAndNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        _cacheService.GetAsync<MailMasterResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(default(MailMasterResponse?));
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns(default(MailMasterResponse?));

        // Act
        var result = await _service.GetByIdAsync(99, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("NotFound", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    // ── AddOrUpdateAsync ─────────────────────────────────────────────

    [Fact]
    public async Task AddOrUpdateAsync_ValidationFails_ReturnsFailureWithoutCallingRepository()
    {
        // Arrange — empty MailFromAddress
        var request = new MailMasterRequest
        {
            MailSNo = 0,
            MailBranchCode = 1,
            MailFromAddress = "",
            MailFromPassword = "pwd",
            MailDisplayName = "Test",
            MailHost = "smtp.test.com",
            MailPort = 587,
            MailSslEnabled = true,
            MailMaxRecipients = 50,
            MailRetryAttempts = 3,
            MailRetryIntervalMinutes = 5,
            MailIsActive = true,
            MailIsDefault = false
        };

        // Act
        var result = await _service.AddOrUpdateAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Validation", result.Error.Code, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive()
            .AddOrUpdateAsync(Arg.Any<MailMasterRequest>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateAsync_SpSucceeds_ReturnsSuccessAndInvalidatesCache()
    {
        // Arrange
        _repository.AddOrUpdateAsync(Arg.Any<MailMasterRequest>(), 1, Arg.Any<CancellationToken>())
            .Returns(SuccessSpResult);

        // Act
        var result = await _service.AddOrUpdateAsync(ValidRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _cacheService.Received().RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateAsync_SpFails_ReturnsFailureWithoutInvalidatingCache()
    {
        // Arrange
        _repository.AddOrUpdateAsync(Arg.Any<MailMasterRequest>(), 1, Arg.Any<CancellationToken>())
            .Returns(FailureSpResult);

        // Act
        var result = await _service.AddOrUpdateAsync(ValidRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("MailMaster", result.Error.Code, StringComparison.OrdinalIgnoreCase);
        await _cacheService.DidNotReceive().RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    // ── DeleteAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_SpSucceeds_ReturnsSuccessAndInvalidatesCache()
    {
        // Arrange
        _repository.DeleteAsync(1, Arg.Any<CancellationToken>()).Returns(SuccessSpResult);

        // Act
        var result = await _service.DeleteAsync(1, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        await _cacheService.Received().RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_SpFails_ReturnsFailureWithoutInvalidatingCache()
    {
        // Arrange
        _repository.DeleteAsync(1, Arg.Any<CancellationToken>()).Returns(FailureSpResult);

        // Act
        var result = await _service.DeleteAsync(1, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("MailMaster", result.Error.Code, StringComparison.OrdinalIgnoreCase);
        await _cacheService.DidNotReceive().RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateAsync_PassesCurrentUserLoginIdAsSessionId()
    {
        // Arrange
        _currentUser.LoginId.Returns(42);
        _repository.AddOrUpdateAsync(Arg.Any<MailMasterRequest>(), 42, Arg.Any<CancellationToken>())
            .Returns(SuccessSpResult);

        // Act
        await _service.AddOrUpdateAsync(ValidRequest, CancellationToken.None);

        // Assert
        await _repository.Received(1)
            .AddOrUpdateAsync(Arg.Any<MailMasterRequest>(), 42, Arg.Any<CancellationToken>());
    }
}
