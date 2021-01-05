namespace FeatureFlags.Models;

/// <summary>
/// Provides extension methods for the <see cref="Result"/> and <see cref="Result{T}"/> types.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps the success value of a <see cref="Result{T}"/> to a new value using the provided selector.
    /// If the result is a failure, the failure is propagated.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TOut">The type of the mapped value.</typeparam>
    /// <param name="result">The result to map.</param>
    /// <param name="selector">The selector to apply to the success value.</param>
    /// <returns>A new <see cref="Result{TOut}"/> that represents the mapped result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="selector"/> is null.</exception>
    public static Result<TOut> Map<T, TOut>(this Result<T> result, Func<T, TOut> selector)
    {
        ArgumentNullException.ThrowIfNull(selector);

        return result.IsSuccess
            ? Result<TOut>.Success(selector(result.Data!))
            : Result<TOut>.Failure(result.Error, result.ErrorCode);
    }

    /// <summary>
    /// Executes the provided action if the <see cref="Result{T}"/> is a success.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="action">The action to execute if the result is a success.</param>
    /// <returns>The original <see cref="Result{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (result.IsSuccess)
        {
            action(result.Data!);
        }

        return result;
    }

    /// <summary>
    /// Executes the provided action if the <see cref="Result"/> or <see cref="Result{T}"/> is a failure.
    /// </summary>
    /// <typeparam name="T">The type of the success value, if any.</typeparam>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="action">The action to execute if the result is a failure.</param>
    /// <returns>The original <see cref="Result{T}"/> or <see cref="Result"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<string?, int?> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!result.IsSuccess)
        {
            action(result.Error, result.ErrorCode);
        }

        return result;
    }

    /// <summary>
    /// Executes the provided action if the <see cref="Result"/> is a failure.
    /// </summary>
    /// <param name="result">The result to evaluate.</param>
    /// <param name="action">The action to execute if the result is a failure.</param>
    /// <returns>The original <see cref="Result"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
    public static Result OnFailure(this Result result, Action<string?, int?> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!result.IsSuccess)
        {
            action(result.Error, result.ErrorCode);
        }

        return result;
    }
}
