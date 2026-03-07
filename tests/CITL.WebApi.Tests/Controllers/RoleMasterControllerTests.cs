using CITL.Application.Common.Models;
using CITL.Application.Core.Admin.RoleMaster;
using CITL.SharedKernel.Results;
using CITL.WebApi.Controllers.Core.Admin;
using CITL.WebApi.Responses;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace CITL.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="RoleMasterController"/>.
/// Verifies correct delegation to <see cref="IRoleMasterService"/> and HTTP response mapping.
/// </summary>
public sealed class RoleMasterControllerTests
{
    private readonly IRoleMasterService _service = Substitute.For<IRoleMasterService>();
    private readonly RoleMasterController _controller;

    public RoleMasterControllerTests()
    {
        _controller = new(_service);
    }

    // ── GetAllAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_Returns200WithRoles()
    {
        // Arrange
        IReadOnlyList<RoleResponse> roles = [new() { RoleId = 1, RoleName = "Admin" }];
        _service.GetAllAsync(true, Arg.Any<CancellationToken>())
            .Returns(Result.Success(roles));

        // Act
        var actionResult = await _controller.GetAllAsync(true, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<IReadOnlyList<RoleResponse>>>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
        Assert.Single(apiResponse.Data!);
    }

    // ── GetDropDownAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetDropDownAsync_Returns200WithItems()
    {
        // Arrange
        IReadOnlyList<DropDownResponse<int>> items = [new() { Col1 = 1, Col2 = "Admin" }];
        _service.GetDropDownAsync(true, Arg.Any<CancellationToken>())
            .Returns(Result.Success(items));

        // Act
        var actionResult = await _controller.GetDropDownAsync(true, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.IsType<ApiResponse<IReadOnlyList<DropDownResponse<int>>>>(okResult.Value);
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WhenFound_Returns200()
    {
        // Arrange
        var role = new RoleResponse { RoleId = 1, RoleName = "Admin" };
        _service.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(Result.Success(role));

        // Act
        var actionResult = await _controller.GetByIdAsync(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse<RoleResponse>>(okResult.Value);
        Assert.Equal("Admin", apiResponse.Data!.RoleName);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_Returns404()
    {
        // Arrange
        _service.GetByIdAsync(99, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<RoleResponse>(Error.NotFound("Role", "99")));

        // Act
        var actionResult = await _controller.GetByIdAsync(99, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    // ── AddOrUpdateAsync ─────────────────────────────────────────────

    [Fact]
    public async Task AddOrUpdateAsync_WhenSuccess_Returns200()
    {
        // Arrange
        var request = new RoleMasterRequest { RoleId = 0, RoleName = "Admin", BranchCode = 1 };
        _service.AddOrUpdateAsync(Arg.Any<RoleMasterRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<string>("Role saved."));

        // Act
        var actionResult = await _controller.AddOrUpdateAsync(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.Equal(ApiResponseCode.Success, apiResponse.Code);
    }

    [Fact]
    public async Task AddOrUpdateAsync_WhenFailure_Returns400()
    {
        // Arrange
        var request = new RoleMasterRequest { RoleId = 0, RoleName = "", BranchCode = 1 };
        _service.AddOrUpdateAsync(Arg.Any<RoleMasterRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string>(Error.Validation("RoleName", "Required.")));

        // Act
        var actionResult = await _controller.AddOrUpdateAsync(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(400, objectResult.StatusCode);
    }

    // ── DeleteAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_WhenSuccess_Returns200()
    {
        // Arrange
        _service.DeleteAsync(1, Arg.Any<CancellationToken>())
            .Returns(Result.Success<string>("Role deleted."));

        // Act
        var actionResult = await _controller.DeleteAsync(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.IsType<ApiResponse>(okResult.Value);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_Returns404()
    {
        // Arrange
        _service.DeleteAsync(99, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string>(Error.NotFound("Role", "99")));

        // Act
        var actionResult = await _controller.DeleteAsync(99, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(404, objectResult.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_DelegatesToService()
    {
        // Arrange
        _service.DeleteAsync(5, Arg.Any<CancellationToken>())
            .Returns(Result.Success<string>("Role deleted."));

        // Act
        await _controller.DeleteAsync(5, CancellationToken.None);

        // Assert
        await _service.Received(1).DeleteAsync(5, Arg.Any<CancellationToken>());
    }
}
