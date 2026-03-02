using CITL.Application.Common.Models;

namespace CITL.Application.Tests.SharedKernel;

/// <summary>
/// Unit tests for <see cref="SpResult"/> and <see cref="SpResultExtensions"/>.
/// </summary>
public sealed class SpResultTests
{
    // ── SpResult.IsSuccess ────────────────────────────────────────────────────

    [Theory]
    [InlineData("SUCCESS", true)]
    [InlineData("success", true)]
    [InlineData("Success", true)]
    [InlineData("ERROR", false)]
    [InlineData("WARNING", false)]
    [InlineData("", false)]
    public void IsSuccess_ReturnsCorrectValueForResultType(string resultType, bool expected)
    {
        // Arrange
        var spResult = new SpResult { ResultType = resultType, ResultMessage = "msg" };

        // Assert
        Assert.Equal(expected, spResult.IsSuccess);
    }

    // ── SpResultExtensions.ToResult ───────────────────────────────────────────

    [Fact]
    public void ToResult_WhenSuccess_ReturnsSuccessResult()
    {
        // Arrange
        var spResult = new SpResult
        {
            ResultVal = 1,
            ResultType = "SUCCESS",
            ResultMessage = "Saved."
        };

        // Act
        var result = spResult.ToResult("Test.Failed");

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ToResult_WhenError_ReturnsFailureWithErrorCode()
    {
        // Arrange
        var spResult = new SpResult
        {
            ResultVal = 0,
            ResultType = "ERROR",
            ResultMessage = "Duplicate entry."
        };

        // Act
        var result = spResult.ToResult("AppMaster.SaveFailed");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("AppMaster.SaveFailed", result.Error.Code);
        Assert.Equal("Duplicate entry.", result.Error.Description);
    }

    [Fact]
    public void ToResult_WhenWarning_ReturnsFailureResult()
    {
        // Arrange
        var spResult = new SpResult
        {
            ResultVal = 0,
            ResultType = "WARNING",
            ResultMessage = "Record already exists."
        };

        // Act
        var result = spResult.ToResult("Test.Failed");

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void ToResult_PreservesSpResultMessageInErrorDescription()
    {
        // Arrange
        const string expectedMessage = "Business rule violation: header too long.";
        var spResult = new SpResult
        {
            ResultVal = 0,
            ResultType = "ERROR",
            ResultMessage = expectedMessage
        };

        // Act
        var result = spResult.ToResult("Test.Error");

        // Assert
        Assert.Equal(expectedMessage, result.Error.Description);
    }

    // ── SpResultExtensions.ToResult() (generic → Result<int>) ─────────────────

    [Fact]
    public void ToResultGeneric_WhenSuccess_ReturnsSuccessWithResultVal()
    {
        // Arrange
        var spResult = new SpResult
        {
            ResultVal = 42,
            ResultType = "SUCCESS",
            ResultMessage = "Created."
        };

        // Act
        var result = spResult.ToResult();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ToResultGeneric_WhenError_ReturnsFailure()
    {
        // Arrange
        var spResult = new SpResult
        {
            ResultVal = 0,
            ResultType = "ERROR",
            ResultMessage = "Insert failed."
        };

        // Act
        var result = spResult.ToResult();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("StoredProcedure.Failed", result.Error.Code);
        Assert.Equal("Insert failed.", result.Error.Description);
    }
}
