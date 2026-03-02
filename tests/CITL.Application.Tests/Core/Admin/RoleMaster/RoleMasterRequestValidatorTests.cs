using CITL.Application.Core.Admin.RoleMaster;

namespace CITL.Application.Tests.Core.Admin.RoleMaster;

/// <summary>
/// Unit tests for <see cref="RoleMasterRequestValidator"/>.
/// </summary>
public sealed class RoleMasterRequestValidatorTests
{
    private readonly RoleMasterRequestValidator _validator = new();

    private static RoleMasterRequest CreateRequest(
        int roleId = 0,
        string roleName = "Admin",
        int branchCode = 1) => new()
        {
            RoleId = roleId,
            RoleName = roleName,
            BranchCode = branchCode
        };

    [Fact]
    public async Task Validate_WithValidRequest_IsValid()
    {
        var result = await _validator.ValidateAsync(CreateRequest());
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithEmptyRoleName_IsInvalid()
    {
        var request = CreateRequest(roleName: "");
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RoleName");
    }

    [Fact]
    public async Task Validate_WithRoleNameExceeding40Chars_IsInvalid()
    {
        var request = CreateRequest(roleName: new string('r', 41));
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithRoleNameAtMax40Chars_IsValid()
    {
        var request = CreateRequest(roleName: new string('r', 40));
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithZeroBranchCode_IsInvalid()
    {
        var request = CreateRequest(branchCode: 0);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "BranchCode");
    }

    [Fact]
    public async Task Validate_WithNegativeBranchCode_IsInvalid()
    {
        var request = CreateRequest(branchCode: -1);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithMultipleInvalidFields_ReturnsAllErrors()
    {
        var request = new RoleMasterRequest { RoleName = "", BranchCode = 0 };
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }
}
