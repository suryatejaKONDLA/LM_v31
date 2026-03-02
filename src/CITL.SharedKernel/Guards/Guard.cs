using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CITL.SharedKernel.Guards;

/// <summary>
/// Provides static guard methods for input validation at method entry points.
/// All methods return the validated value for clean assignment patterns.
/// Built on top of .NET's optimized throw helpers for maximum JIT performance.
/// </summary>
/// <example>
/// <code>
/// _userId = Guard.Positive(userId);
/// _name = Guard.NotNullOrWhiteSpace(name);
/// _email = Guard.NotNullOrEmpty(email);
/// </code>
/// </example>
public static class Guard
{
    /// <summary>
    /// Throws <see cref="ArgumentNullException"/> if <paramref name="value"/> is <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">The parameter name (auto-captured).</param>
    /// <returns>The non-null value.</returns>
    [return: NotNull]
    public static T NotNull<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(value, paramName);
        return value;
    }

    /// <summary>
    /// Throws <see cref="ArgumentNullException"/> if <paramref name="value"/> is <see langword="null"/>.
    /// Overload for nullable value types.
    /// </summary>
    /// <typeparam name="T">The underlying value type.</typeparam>
    /// <param name="value">The nullable value to validate.</param>
    /// <param name="paramName">The parameter name (auto-captured).</param>
    /// <returns>The non-null value.</returns>
    public static T NotNull<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : struct
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }

        return value.Value;
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is <see langword="null"/> or <see cref="string.Empty"/>.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The parameter name (auto-captured).</param>
    /// <returns>The non-null, non-empty string.</returns>
    public static string NotNullOrEmpty(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(value, paramName);
        return value;
    }

    /// <summary>
    /// Throws if <paramref name="value"/> is <see langword="null"/>, empty, or whitespace-only.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="paramName">The parameter name (auto-captured).</param>
    /// <returns>The non-null, non-whitespace string.</returns>
    public static string NotNullOrWhiteSpace(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);
        return value;
    }

    /// <summary>
    /// Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">The parameter name (auto-captured).</param>
    /// <returns>The non-negative value.</returns>
    public static int NotNegative(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value, paramName);
        return value;
    }

    /// <summary>
    /// Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero or negative.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">The parameter name (auto-captured).</param>
    /// <returns>The positive value.</returns>
    public static int Positive(
        int value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, paramName);
        return value;
    }

    /// <summary>
    /// Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero or negative.
    /// Overload for <see cref="long"/>.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">The parameter name (auto-captured).</param>
    /// <returns>The positive value.</returns>
    public static long Positive(
        long value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, paramName);
        return value;
    }

    /// <summary>
    /// Throws <see cref="ArgumentException"/> if <paramref name="value"/> equals the default value for its type.
    /// Useful for catching uninitialized <see cref="Guid"/>, <see cref="DateTime"/>, etc.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">The parameter name (auto-captured).</param>
    /// <returns>The non-default value.</returns>
    public static T NotDefault<T>(
        T value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
        {
            throw new ArgumentException("Value cannot be the default value.", paramName);
        }

        return value;
    }

    /// <summary>
    /// Throws <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not within
    /// the specified inclusive range.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="paramName">The parameter name (auto-captured).</param>
    /// <returns>The value within range.</returns>
    public static int InRange(
        int value,
        int min,
        int max,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, min, paramName);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, max, paramName);
        return value;
    }

    /// <summary>
    /// Throws <see cref="ArgumentException"/> if the collection is <see langword="null"/> or empty.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="value">The collection to validate.</param>
    /// <param name="paramName">The parameter name (auto-captured).</param>
    /// <returns>The non-null, non-empty collection.</returns>
    public static IReadOnlyCollection<T> NotNullOrEmpty<T>(
        [NotNull] IReadOnlyCollection<T>? value,
        [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        ArgumentNullException.ThrowIfNull(value, paramName);

        if (value.Count == 0)
        {
            throw new ArgumentException("Collection must not be empty.", paramName);
        }

        return value;
    }
}
