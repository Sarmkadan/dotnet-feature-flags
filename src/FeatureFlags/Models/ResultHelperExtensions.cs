namespace FeatureFlags.Models;

/// <summary>
/// Provides additional extension methods for the <see cref="Result"/> and <see cref="Result{T}"/> types.
/// </summary>
public static class ResultHelperExtensions
{
    /// <summary>
    /// Binds the success value of a <see cref="Result{T}"/> to a new result using the provided binder.
    /// If the result is a failure, the failure is propagated.
    /// </summary>
    /// <typeparam name="T">The type of the success value.</typeparam>
    /// <typeparam name="TOut">The type of the success value of the bound result.</typeparam>
    /// <param name="result">The result to bind.</param>
    /// <param name="binder">The binder to apply to the success value.</param>
    /// <returns>A new <see cref="Result{TOut}"/> that represents the bound result.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="result"/> or <paramref name="binder"/> is null.</exception>
    public static Result<TOut> Bind<T, TOut>(this Result<T> result, Func<T, Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(binder);

        return result.IsSuccess
            ? binder(result.Data)
            : Result<TOut>.Failure(result.Error, result.ErrorCode);
    }
}