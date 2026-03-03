using System.Globalization;
using CITL.SharedKernel.Results;

namespace CITL.Application.Tests.SharedKernel;

/// <summary>
/// Unit tests for <see cref="Result"/> and <see cref="Result{T}"/>.
/// </summary>
public sealed class ResultTests
{
    // ── Result (non-generic) ──────────────────────────────────────────────────

    [Fact]
    public void Success_IsSuccess_IsTrue()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_WithError_IsFailure_IsTrue()
    {
        // Arrange
        var error = new Error("Test.Error", "Something went wrong.");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void Success_WithError_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
        {
            Result.Failure(Error.None); // Error.None on a failure result
        });
    }

    // ── Result<T> ─────────────────────────────────────────────────────────────

    [Fact]
    public void SuccessT_WithValue_IsSuccess_IsTrue()
    {
        // Act
        var result = Result.Success(42);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void FailureT_WithError_IsFailure_IsTrue()
    {
        // Arrange
        var error = Error.NotFound("User", 99);

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains("NotFound", result.Error.Code, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("User", result.Error.Code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SuccessT_WithNullValue_IsSuccess_IsTrue()
    {
        // Act
        var result = Result.Success<string?>(null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
    }

    // ── Error ─────────────────────────────────────────────────────────────────

    [Fact]
    public void ErrorNone_HasEmptyCodeAndDescription()
    {
        // Assert
        Assert.Equal(string.Empty, Error.None.Code);
        Assert.Equal(string.Empty, Error.None.Description);
    }

    [Fact]
    public void NotFound_CreatesCorrectErrorCode()
    {
        // Act
        var error = Error.NotFound("AppMaster", "config");

        // Assert
        Assert.Equal("AppMaster.NotFound", error.Code);
        Assert.Contains("AppMaster", error.Description, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Error_EqualityByValue()
    {
        // Arrange
        var error1 = new Error("Test.Code", "Test description.");
        var error2 = new Error("Test.Code", "Test description.");

        // Assert
        Assert.Equal(error1, error2);
    }

    [Fact]
    public void Error_DifferentCode_NotEqual()
    {
        // Arrange
        var error1 = new Error("Test.Code1", "Same description.");
        var error2 = new Error("Test.Code2", "Same description.");

        // Assert
        Assert.NotEqual(error1, error2);
    }

    // ── Error factory methods ─────────────────────────────────────────────────

    [Fact]
    public void Validation_CreatesCorrectErrorCode()
    {
        // Act
        var error = Error.Validation("Email", "Invalid email format.");

        // Assert
        Assert.Equal("Validation.Email", error.Code);
        Assert.Equal("Invalid email format.", error.Description);
    }

    [Fact]
    public void Conflict_CreatesCorrectErrorCode()
    {
        // Act
        var error = Error.Conflict("User", "Email already registered.");

        // Assert
        Assert.Equal("User.Conflict", error.Code);
        Assert.Equal("Email already registered.", error.Description);
    }

    [Fact]
    public void NullValue_HasExpectedCodeAndDescription()
    {
        // Assert
        Assert.Equal("Error.NullValue", Error.NullValue.Code);
        Assert.NotEmpty(Error.NullValue.Description);
    }

    [Fact]
    public void ServerError_HasExpectedCodeAndDescription()
    {
        // Assert
        Assert.Equal("Error.ServerError", Error.ServerError.Code);
        Assert.NotEmpty(Error.ServerError.Description);
    }

    // ── Result<T>.Value throws on failure ──────────────────────────────────────

    [Fact]
    public void FailureT_AccessingValue_ThrowsInvalidOperationException()
    {
        // Arrange
        var result = Result.Failure<int>(new("Fail", "Oops"));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }

    // ── Result<T>.Map ─────────────────────────────────────────────────────────

    [Fact]
    public void Map_WhenSuccess_TransformsValue()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var mapped = result.Map(x => x * 2);

        // Assert
        Assert.True(mapped.IsSuccess);
        Assert.Equal(10, mapped.Value);
    }

    [Fact]
    public void Map_WhenFailure_PropagatesError()
    {
        // Arrange
        var error = new Error("E", "err");
        var result = Result.Failure<int>(error);

        // Act
        var mapped = result.Map(x => x.ToString(CultureInfo.InvariantCulture));

        // Assert
        Assert.True(mapped.IsFailure);
        Assert.Equal(error, mapped.Error);
    }

    // ── Result<T>.Bind ────────────────────────────────────────────────────────

    [Fact]
    public void Bind_WhenSuccess_FlatMapsToNewResult()
    {
        // Arrange
        var result = Result.Success(10);

        // Act
        var bound = result.Bind(x => x > 0 ? Result.Success(x.ToString(CultureInfo.InvariantCulture)) : Result.Failure<string>(new("E", "neg")));

        // Assert
        Assert.True(bound.IsSuccess);
        Assert.Equal("10", bound.Value);
    }

    [Fact]
    public void Bind_WhenSuccess_BinderReturnsFailure_PropagatesBinderError()
    {
        // Arrange
        var result = Result.Success(-1);
        var bindError = new Error("Neg", "Negative");

        // Act
        var bound = result.Bind(x => x > 0 ? Result.Success("ok") : Result.Failure<string>(bindError));

        // Assert
        Assert.True(bound.IsFailure);
        Assert.Equal(bindError, bound.Error);
    }

    [Fact]
    public void Bind_WhenFailure_DoesNotCallBinder()
    {
        // Arrange
        var error = new Error("E", "err");
        var result = Result.Failure<int>(error);
        var binderCalled = false;

        // Act
        var bound = result.Bind(x => { binderCalled = true; return Result.Success("y"); });

        // Assert
        Assert.False(binderCalled);
        Assert.Equal(error, bound.Error);
    }

    // ── Result<T>.Match ───────────────────────────────────────────────────────

    [Fact]
    public void Match_WhenSuccess_CallsOnSuccess()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        var output = result.Match(
            onSuccess: v => $"OK:{v}",
            onFailure: e => $"FAIL:{e.Code}");

        // Assert
        Assert.Equal("OK:42", output);
    }

    [Fact]
    public void Match_WhenFailure_CallsOnFailure()
    {
        // Arrange
        var result = Result.Failure<int>(new("X", "x"));

        // Act
        var output = result.Match(
            onSuccess: v => $"OK:{v}",
            onFailure: e => $"FAIL:{e.Code}");

        // Assert
        Assert.Equal("FAIL:X", output);
    }

    // ── Implicit operator ─────────────────────────────────────────────────────

    [Fact]
    public void ImplicitOperator_NonNullValue_CreatesSuccessResult()
    {
        // Act
        Result<string> result = "hello";

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void ImplicitOperator_NullValue_CreatesFailureWithNullValueError()
    {
        // Arrange
        string? value = null;

        // Act
        Result<string> result = value;

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(Error.NullValue, result.Error);
    }
}
