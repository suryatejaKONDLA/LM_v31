using CITL.SharedKernel.Guards;

namespace CITL.Application.Tests.SharedKernel;

/// <summary>
/// Unit tests for <see cref="CITL.SharedKernel.Guards.Guard"/>.
/// Verifies all guard methods throw correctly and return validated values.
/// </summary>
public sealed class GuardTests
{
    // ── NotNull<T> ────────────────────────────────────────────────────────────

    [Fact]
    public void NotNull_WithNonNullValue_ReturnsValue()
    {
        // Arrange
        var value = "hello";

        // Act
        var result = Guard.NotNull(value);

        // Assert
        Assert.Equal("hello", result);
    }

    [Fact]
    public void NotNull_WithNullValue_ThrowsArgumentNullException()
    {
        // Arrange
        string? value = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Guard.NotNull(value));
    }

    // ── NotNullOrEmpty (string) ───────────────────────────────────────────────

    [Fact]
    public void NotNullOrEmpty_WithNonEmptyString_ReturnsValue()
    {
        // Act
        var result = Guard.NotNullOrEmpty("valid");

        // Assert
        Assert.Equal("valid", result);
    }

    [Fact]
    public void NotNullOrEmpty_WithNullString_ThrowsArgumentException()
    {
        // Arrange
        string? value = null;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => Guard.NotNullOrEmpty(value));
    }

    [Fact]
    public void NotNullOrEmpty_WithEmptyString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => Guard.NotNullOrEmpty(string.Empty));
    }

    // ── NotNullOrWhiteSpace ───────────────────────────────────────────────────

    [Fact]
    public void NotNullOrWhiteSpace_WithValidString_ReturnsValue()
    {
        // Act
        var result = Guard.NotNullOrWhiteSpace("abc");

        // Assert
        Assert.Equal("abc", result);
    }

    [Fact]
    public void NotNullOrWhiteSpace_WithWhitespaceString_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => Guard.NotNullOrWhiteSpace("   "));
    }

    // ── Positive (int) ────────────────────────────────────────────────────────

    [Fact]
    public void Positive_WithPositiveInt_ReturnsValue()
    {
        // Act
        var result = Guard.Positive(5);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void Positive_WithZero_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Positive(0));
    }

    [Fact]
    public void Positive_WithNegative_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Positive(-1));
    }

    // ── NotNegative ───────────────────────────────────────────────────────────

    [Fact]
    public void NotNegative_WithZero_ReturnsZero()
    {
        // Act
        var result = Guard.NotNegative(0);

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void NotNegative_WithNegative_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.NotNegative(-1));
    }

    // ── InRange ───────────────────────────────────────────────────────────────

    [Fact]
    public void InRange_WithValueInRange_ReturnsValue()
    {
        // Act
        var result = Guard.InRange(5, 1, 10);

        // Assert
        Assert.Equal(5, result);
    }

    [Fact]
    public void InRange_WithValueAtMin_ReturnsValue()
    {
        // Act
        var result = Guard.InRange(1, 1, 10);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public void InRange_WithValueAtMax_ReturnsValue()
    {
        // Act
        var result = Guard.InRange(10, 1, 10);

        // Assert
        Assert.Equal(10, result);
    }

    [Fact]
    public void InRange_WithValueBelowMin_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.InRange(0, 1, 10));
    }

    [Fact]
    public void InRange_WithValueAboveMax_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.InRange(11, 1, 10));
    }

    // ── NotDefault ────────────────────────────────────────────────────────────

    [Fact]
    public void NotDefault_WithNonDefaultGuid_ReturnsValue()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = Guard.NotDefault(guid);

        // Assert
        Assert.Equal(guid, result);
    }

    [Fact]
    public void NotDefault_WithDefaultGuid_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.NotDefault(Guid.Empty));
    }

    // ── NotNull<T> (nullable value type) ──────────────────────────────────────

    [Fact]
    public void NotNull_NullableStruct_WithValue_ReturnsUnwrappedValue()
    {
        // Arrange
        int? value = 42;

        // Act
        var result = Guard.NotNull(value);

        // Assert
        Assert.Equal(42, result);
    }

    [Fact]
    public void NotNull_NullableStruct_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        int? value = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Guard.NotNull(value));
    }

    // ── Positive (long) ───────────────────────────────────────────────────────

    [Fact]
    public void PositiveLong_WithPositiveValue_ReturnsValue()
    {
        // Act
        var result = Guard.Positive(100L);

        // Assert
        Assert.Equal(100L, result);
    }

    [Fact]
    public void PositiveLong_WithZero_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Positive(0L));
    }

    [Fact]
    public void PositiveLong_WithNegative_ThrowsArgumentOutOfRangeException()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => Guard.Positive(-1L));
    }

    // ── NotNullOrEmpty<T> (collection) ────────────────────────────────────────

    [Fact]
    public void NotNullOrEmptyCollection_WithItems_ReturnsCollection()
    {
        // Arrange
        IReadOnlyCollection<int> list = new[] { 1, 2, 3 };

        // Act
        var result = Guard.NotNullOrEmpty(list);

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void NotNullOrEmptyCollection_WithNull_ThrowsArgumentNullException()
    {
        // Arrange
        IReadOnlyCollection<string>? list = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Guard.NotNullOrEmpty(list));
    }

    [Fact]
    public void NotNullOrEmptyCollection_WithEmptyList_ThrowsArgumentException()
    {
        // Arrange
        IReadOnlyCollection<int> list = Array.Empty<int>();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => Guard.NotNullOrEmpty(list));
    }
}
