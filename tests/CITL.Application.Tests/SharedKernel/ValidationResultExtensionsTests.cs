using CITL.Application.Common.Validation;
using CITL.SharedKernel.Results;
using FluentValidation.Results;

namespace CITL.Application.Tests.SharedKernel;

/// <summary>
/// Unit tests for <see cref="ValidationResultExtensions"/>.
/// Tests the bridge between FluentValidation and the CITL Result pattern.
/// </summary>
public sealed class ValidationResultExtensionsTests
{
    // ── ToResult (non-generic) ────────────────────────────────────────────────

    [Fact]
    public void ToResult_WhenValid_ReturnsSuccess()
    {
        // Arrange
        var validationResult = new ValidationResult();

        // Act
        var result = validationResult.ToResult();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void ToResult_WhenInvalid_ReturnsFailureWithFirstError()
    {
        // Arrange
        var validationResult = new ValidationResult(
        [
            new("Name", "Name is required."),
            new ValidationFailure("Age", "Age must be positive.")
        ]);

        // Act
        var result = validationResult.ToResult();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Validation.Name", result.Error.Code);
        Assert.Equal("Name is required.", result.Error.Description);
    }

    [Fact]
    public void ToResult_WhenInvalid_OnlyFirstErrorIsReported()
    {
        // Arrange
        var validationResult = new ValidationResult(
        [
            new("Email", "Invalid email."),
            new ValidationFailure("Phone", "Invalid phone.")
        ]);

        // Act
        var result = validationResult.ToResult();

        // Assert
        Assert.Equal("Validation.Email", result.Error.Code);
        Assert.DoesNotContain("Phone", result.Error.Code, StringComparison.Ordinal);
    }

    // ── ToResult<T> (generic) ─────────────────────────────────────────────────

    [Fact]
    public void ToResultT_WhenValid_ThrowsInvalidOperationException()
    {
        // Arrange
        var validationResult = new ValidationResult();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(validationResult.ToResult<int>);
    }

    [Fact]
    public void ToResultT_WhenInvalid_ReturnsTypedFailure()
    {
        // Arrange
        var validationResult = new ValidationResult(
        [
            new ValidationFailure("Code", "Code is required.")
        ]);

        // Act
        var result = validationResult.ToResult<string>();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("Validation.Code", result.Error.Code);
    }

    // ── ToValidationException ─────────────────────────────────────────────────

    [Fact]
    public void ToValidationException_WhenValid_ThrowsInvalidOperationException()
    {
        // Arrange
        var validationResult = new ValidationResult();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(validationResult.ToValidationException);
    }

    [Fact]
    public void ToValidationException_WhenInvalid_ReturnsExceptionWithGroupedErrors()
    {
        // Arrange
        var validationResult = new ValidationResult(
        [
            new("Name", "Required."),
            new("Name", "Too short."),
            new ValidationFailure("Email", "Invalid format.")
        ]);

        // Act
        var ex = validationResult.ToValidationException();

        // Assert
        Assert.Equal(2, ex.Errors.Count); // Name and Email
        Assert.Equal(2, ex.Errors["Name"].Length);
        Assert.Single(ex.Errors["Email"]);
    }

    [Fact]
    public void ToValidationException_PreservesErrorMessages()
    {
        // Arrange
        var validationResult = new ValidationResult(
        [
            new ValidationFailure("Field1", "Error message one.")
        ]);

        // Act
        var ex = validationResult.ToValidationException();

        // Assert
        Assert.Equal("Error message one.", ex.Errors["Field1"][0]);
    }
}
