using CITL.Application.Common.Interfaces;
using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.RoleMaster;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace CITL.Application.Tests.Core.Admin.RoleMaster;

/// <summary>
/// Unit tests for <see cref="RoleMasterService"/>.
/// Dependencies: IRoleMasterRepository, ICurrentUser, ICacheService, ITenantContext (mocked),
/// real validator, NullLogger.
/// </summary>
public sealed class RoleMasterServiceTests
{
    // ── Fixtures ──────────────────────────────────────────────────────

    private readonly IRoleMasterRepository _repository = Substitute.For<IRoleMasterRepository>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly ICacheService _cacheService = Substitute.For<ICacheService>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly RoleMasterService _service;

    public RoleMasterServiceTests()
    {
        _currentUser.LoginId.Returns(1);
        _tenantContext.TenantId.Returns("T1");

        _service = new(
            _repository,
            _currentUser,
            _cacheService,
            _tenantContext,
            new RoleMasterRequestValidator(),
            NullLogger<RoleMasterService>.Instance);
    }

    private static RoleMasterRequest ValidRequest => new()
    {
        RoleId = 0,
        RoleName = "Admin",
        BranchCode = 1
    };

    private static RoleResponse SampleRole => new()
    {
        RoleId = 1,
        RoleName = "Admin",
        RoleBranchCode = 1,
        RoleCreatedId = 1,
        RoleCreatedDate = new(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
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
        ResultMessage = "Duplicate role name."
    };

    // ── GetAllAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsCachedOrFreshRoles()
    {
        // Arrange
        IReadOnlyList<RoleResponse> roles = [SampleRole];
        _cacheService.GetOrSetAsync(
            Arg.Any<string>(),
            Arg.Any<Func<CancellationToken, Task<IReadOnlyList<RoleResponse>>>>(),
            Arg.Any<CacheEntryOptions>(),
            Arg.Any<CancellationToken>()).Returns(roles);

        // Act
        var result = await _service.GetAllAsync(true, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal("Admin", result.Value[0].RoleName);
    }

    // ── GetDropDownAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetDropDownAsync_ReturnsCachedOrFreshItems()
    {
        // Arrange
        IReadOnlyList<DropDownResponse<int>> items = [new() { Value = 1, Text = "Admin" }];
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
        _cacheService.GetAsync<RoleResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(SampleRole);

        // Act
        var result = await _service.GetByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Admin", result.Value.RoleName);
        await _repository.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_CacheMiss_ReturnsFromRepositoryAndCaches()
    {
        // Arrange
        _cacheService.GetAsync<RoleResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(default(RoleResponse?));
        _repository.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(SampleRole);

        // Act
        var result = await _service.GetByIdAsync(1, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Admin", result.Value.RoleName);
        await _cacheService.Received(1).SetAsync(
            Arg.Any<string>(), Arg.Any<RoleResponse>(), Arg.Any<CacheEntryOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByIdAsync_CacheMissAndNotFound_ReturnsNotFoundFailure()
    {
        // Arrange
        _cacheService.GetAsync<RoleResponse>(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(default(RoleResponse?));
        _repository.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns(default(RoleResponse?));

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
        // Arrange
        var request = new RoleMasterRequest { RoleId = 0, RoleName = "", BranchCode = 1 };

        // Act
        var result = await _service.AddOrUpdateAsync(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Validation", result.Error.Code, StringComparison.OrdinalIgnoreCase);
        await _repository.DidNotReceive()
            .AddOrUpdateAsync(Arg.Any<RoleMasterRequest>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateAsync_SpSucceeds_ReturnsSuccessAndInvalidatesCache()
    {
        // Arrange
        _repository.AddOrUpdateAsync(Arg.Any<RoleMasterRequest>(), 1, Arg.Any<CancellationToken>())
            .Returns(SuccessSpResult);

        // Act
        var result = await _service.AddOrUpdateAsync(ValidRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        // Verify cache invalidation happened (at least the list keys)
        await _cacheService.Received().RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateAsync_SpFails_ReturnsFailureWithoutInvalidatingCache()
    {
        // Arrange
        _repository.AddOrUpdateAsync(Arg.Any<RoleMasterRequest>(), 1, Arg.Any<CancellationToken>())
            .Returns(FailureSpResult);

        // Act
        var result = await _service.AddOrUpdateAsync(ValidRequest, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("Role", result.Error.Code, StringComparison.OrdinalIgnoreCase);
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
        Assert.Contains("Role", result.Error.Code, StringComparison.OrdinalIgnoreCase);
        await _cacheService.DidNotReceive().RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddOrUpdateAsync_PassesCurrentUserLoginIdAsSessionId()
    {
        // Arrange
        _currentUser.LoginId.Returns(42);
        _repository.AddOrUpdateAsync(Arg.Any<RoleMasterRequest>(), 42, Arg.Any<CancellationToken>())
            .Returns(SuccessSpResult);

        // Act
        await _service.AddOrUpdateAsync(ValidRequest, CancellationToken.None);

        // Assert
        await _repository.Received(1)
            .AddOrUpdateAsync(Arg.Any<RoleMasterRequest>(), 42, Arg.Any<CancellationToken>());
    }
}
