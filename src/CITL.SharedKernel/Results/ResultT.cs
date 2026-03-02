using System.Diagnostics.CodeAnalysis;

namespace CITL.SharedKernel.Results;

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="T"/>.
/// Access <see cref="Value"/> only when <see cref="Result.IsSuccess"/> is <see langword="true"/>.
/// </summary>
/// <typeparam name="T">The type of the success value.</typeparam>
/// <remarks>
/// Supports implicit conversion from <typeparamref name="T"/> for clean return statements:
/// <code>
/// public Result&lt;UserDto&gt; GetUser() => userDto;  // implicit success
/// </code>
/// </remarks>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{T}"/> class.
    /// </summary>
    /// <param name="value">The success value (can be null for failures).</param>
    /// <param name="isSuccess">Whether the operation succeeded.</param>
    /// <param name="error">The error if failed.</param>
    internal Result(T? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the success value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when accessing the value of a failed result.
    /// Always check <see cref="Result.IsSuccess"/> before accessing this property.
    /// </exception>
    [NotNull]
    public T Value
    {
        get
        {
            if (!IsSuccess)
            {
                throw new InvalidOperationException(
                    $"Cannot access the value of a failed result. Error: {Error.Code} — {Error.Description}");
            }

            return _value!;
        }
    }

    /// <summary>
    /// Applies a transformation to the success value, or propagates the error.
    /// </summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="mapper">The transformation function.</param>
    /// <returns>A new result with the mapped value, or the original error.</returns>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess
            ? Result.Success(mapper(_value!))
            : Result.Failure<TOut>(Error);

    /// <summary>
    /// Applies a transformation that itself returns a Result, flattening the output.
    /// </summary>
    /// <typeparam name="TOut">The output type.</typeparam>
    /// <param name="binder">The transformation function returning a Result.</param>
    /// <returns>The bound result, or the original error.</returns>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder) =>
        IsSuccess
            ? binder(_value!)
            : Result.Failure<TOut>(Error);

    /// <summary>
    /// Pattern-matches the result into a single return value.
    /// </summary>
    /// <typeparam name="TOut">The return type.</typeparam>
    /// <param name="onSuccess">Function to call on success.</param>
    /// <param name="onFailure">Function to call on failure.</param>
    /// <returns>The matched result.</returns>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<Error, TOut> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(Error);

    /// <summary>
    /// Implicitly converts a value to a successful <see cref="Result{T}"/>.
    /// Returns a failure with <see cref="Error.NullValue"/> if the value is <see langword="null"/>.
    /// </summary>
    /// <param name="value">The value to wrap.</param>
    public static implicit operator Result<T>(T? value) =>
        value is not null
            ? new(value, true, Error.None)
            : new(default, false, Error.NullValue);
}
