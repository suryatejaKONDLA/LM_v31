using CITL.Application.Core.Admin.AppMaster;

namespace CITL.Application.Tests.Core.Admin.AppMaster;

/// <summary>
/// Unit tests for <see cref="AppMasterRequestValidator"/>.
/// Pure validator tests — no mocking needed.
/// </summary>
public sealed class AppMasterRequestValidatorTests
{
    private readonly AppMasterRequestValidator _validator = new();

    private static AppMasterRequest ValidRequest => new()
    {
        AppCode = 1,
        AppHeader1 = "CITL Company",
        AppHeader2 = "CITL",
        SessionId = 10,
        BranchCode = 1
    };

    // ── AppHeader1 ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithValidRequest_IsValid()
    {
        // Act
        var result = _validator.Validate(ValidRequest);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_WithEmptyAppHeader1_HasValidationError()
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

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.AppHeader1));
    }

    [Fact]
    public void Validate_WithAppHeader1ExceedingMaxLength_HasValidationError()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = new('A', 61),
            AppHeader2 = "CITL",
            SessionId = 10,
            BranchCode = 1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(request.AppHeader1) &&
            e.ErrorMessage.Contains("60", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_WithAppHeader1AtMaxLength_IsValid()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = new('A', 60),
            AppHeader2 = "CITL",
            SessionId = 10,
            BranchCode = 1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    // ── AppHeader2 ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithEmptyAppHeader2_HasValidationError()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = "CITL Company",
            AppHeader2 = "",
            SessionId = 10,
            BranchCode = 1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.AppHeader2));
    }

    [Fact]
    public void Validate_WithAppHeader2Exceeding7Chars_HasValidationError()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = "CITL Company",
            AppHeader2 = "TOOLONG1", // 8 chars
            SessionId = 10,
            BranchCode = 1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(request.AppHeader2) &&
            e.ErrorMessage.Contains('7'));
    }

    [Theory]
    [InlineData("CITL CO")] // space
    [InlineData("CITL-1")] // hyphen
    [InlineData("CITL.1")] // dot
    [InlineData("CITL@1")] // special char
    public void Validate_WithNonAlphanumericAppHeader2_HasValidationError(string header2)
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = "CITL Company",
            AppHeader2 = header2,
            SessionId = 10,
            BranchCode = 1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.AppHeader2));
    }

    [Theory]
    [InlineData("CITL")]   // 4 letters
    [InlineData("ABC123")]  // 6 alphanumeric
    [InlineData("1234567")] // 7 digits — at max length
    [InlineData("A")]       // single char
    public void Validate_WithValidAppHeader2_IsValid(string header2)
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = "CITL Company",
            AppHeader2 = header2,
            SessionId = 10,
            BranchCode = 1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
    }

    // ── SessionId ─────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithZeroSessionId_HasValidationError()
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

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.SessionId));
    }

    [Fact]
    public void Validate_WithNegativeSessionId_HasValidationError()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = "CITL Company",
            AppHeader2 = "CITL",
            SessionId = -1,
            BranchCode = 1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.SessionId));
    }

    // ── BranchCode ────────────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithZeroBranchCode_HasValidationError()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = "CITL Company",
            AppHeader2 = "CITL",
            SessionId = 10,
            BranchCode = 0
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.BranchCode));
    }

    [Fact]
    public void Validate_WithNegativeBranchCode_HasValidationError()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 1,
            AppHeader1 = "CITL Company",
            AppHeader2 = "CITL",
            SessionId = 10,
            BranchCode = -5
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.BranchCode));
    }

    // ── Multiple errors ───────────────────────────────────────────────────────

    [Fact]
    public void Validate_WithMultipleInvalidFields_ReturnsAllErrors()
    {
        // Arrange
        var request = new AppMasterRequest
        {
            AppCode = 0,
            AppHeader1 = "",
            AppHeader2 = "",
            SessionId = 0,
            BranchCode = 0
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 4); // header1, header2, sessionId, branchCode
    }
}
