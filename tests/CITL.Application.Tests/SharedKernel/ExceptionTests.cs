using CITL.SharedKernel.Exceptions;

namespace CITL.Application.Tests.SharedKernel;

/// <summary>
/// Smoke tests for all custom exception types in SharedKernel.
/// Verifies constructors, messages, and properties.
/// </summary>
public sealed class ExceptionTests
{
    // ── NotFoundException ─────────────────────────────────────────────────────

    [Fact]
    public void NotFoundException_WithMessage_SetsMessage()
    {
        // Act
        var ex = new NotFoundException("Item not found.");

        // Assert
        Assert.Equal("Item not found.", ex.Message);
        Assert.IsAssignableFrom<AppException>(ex);
    }

    [Fact]
    public void NotFoundException_WithEntityAndKey_FormatsMessage()
    {
        // Act
        var ex = new NotFoundException("User", 42);

        // Assert
        Assert.Contains("User", ex.Message, StringComparison.Ordinal);
        Assert.Contains("42", ex.Message, StringComparison.Ordinal);
    }

    // ── ValidationException ───────────────────────────────────────────────────

    [Fact]
    public void ValidationException_WithDictionary_SetsErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = ["Name is required."],
            ["Age"] = ["Must be positive.", "Must be under 200."]
        };

        // Act
        var ex = new ValidationException(errors);

        // Assert
        Assert.Equal(2, ex.Errors.Count);
        Assert.Single(ex.Errors["Name"]);
        Assert.Equal(2, ex.Errors["Age"].Length);
        Assert.Contains("validation", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValidationException_WithSingleField_SetsErrors()
    {
        // Act
        var ex = new ValidationException("Email", "Invalid format.");

        // Assert
        Assert.Single(ex.Errors);
        Assert.Contains("Email", ex.Errors.Keys);
        Assert.Equal("Invalid format.", ex.Errors["Email"][0]);
    }

    // ── TenantException ───────────────────────────────────────────────────────

    [Fact]
    public void TenantException_WithMessage_SetsMessage()
    {
        // Act
        var ex = new TenantException("Tenant not resolved.");

        // Assert
        Assert.Equal("Tenant not resolved.", ex.Message);
        Assert.IsAssignableFrom<AppException>(ex);
    }

    [Fact]
    public void TenantException_WithInnerException_PreservesInner()
    {
        // Arrange
        var inner = new InvalidOperationException("inner");

        // Act
        var ex = new TenantException("outer", inner);

        // Assert
        Assert.Same(inner, ex.InnerException);
    }

    // ── ForbiddenException ────────────────────────────────────────────────────

    [Fact]
    public void ForbiddenException_DefaultMessage_ContainsPermission()
    {
        // Act
        var ex = new ForbiddenException();

        // Assert
        Assert.Contains("permission", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ForbiddenException_CustomMessage_SetsMessage()
    {
        // Act
        var ex = new ForbiddenException("No access to admin panel.");

        // Assert
        Assert.Equal("No access to admin panel.", ex.Message);
    }

    // ── ConflictException ─────────────────────────────────────────────────────

    [Fact]
    public void ConflictException_WithMessage_SetsMessage()
    {
        // Act
        var ex = new ConflictException("Duplicate key.");

        // Assert
        Assert.Equal("Duplicate key.", ex.Message);
    }

    [Fact]
    public void ConflictException_WithEntityAndField_FormatsMessage()
    {
        // Act
        var ex = new ConflictException("User", "Email", "john@test.com");

        // Assert
        Assert.Contains("User", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Email", ex.Message, StringComparison.Ordinal);
    }

    // ── UnauthorizedException ─────────────────────────────────────────────────

    [Fact]
    public void UnauthorizedException_DefaultMessage_ContainsAuthentication()
    {
        // Act
        var ex = new UnauthorizedException();

        // Assert
        Assert.Contains("Authentication", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void UnauthorizedException_CustomMessage_SetsMessage()
    {
        // Act
        var ex = new UnauthorizedException("Token expired.");

        // Assert
        Assert.Equal("Token expired.", ex.Message);
    }

    // ── Hierarchy ─────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(typeof(NotFoundException))]
    [InlineData(typeof(ForbiddenException))]
    [InlineData(typeof(ConflictException))]
    [InlineData(typeof(UnauthorizedException))]
    [InlineData(typeof(TenantException))]
    [InlineData(typeof(ValidationException))]
    public void AllExceptions_DeriveFromAppException(Type exceptionType)
    {
        // Assert
        Assert.True(typeof(AppException).IsAssignableFrom(exceptionType));
    }
}
