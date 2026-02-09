// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Models;

/// <summary>
/// Generic result wrapper class that represents the outcome of an operation.
/// Provides a consistent way to return success/failure with data or error messages.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? Error { get; }
    public int? ErrorCode { get; }

    private Result(bool isSuccess, T? data, string? error, int? errorCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a successful result with data.
    /// </summary>
    public static Result<T> Success(T data)
    {
        return new Result<T>(true, data, null, null);
    }

    /// <summary>
    /// Creates a failed result with error message.
    /// </summary>
    public static Result<T> Failure(string error, int? errorCode = null)
    {
        return new Result<T>(false, default, error, errorCode);
    }

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    public static Result<T> FromException(Exception ex)
    {
        return new Result<T>(false, default, ex.Message, null);
    }

    /// <summary>
    /// Executes a func and wraps the result.
    /// </summary>
    public static async Task<Result<T>> Try(Func<Task<T>> operation)
    {
        try
        {
            var data = await operation();
            return Success(data);
        }
        catch (Exception ex)
        {
            return FromException(ex);
        }
    }

    /// <summary>
    /// Transforms the result value if successful.
    /// </summary>
    public Result<TOut> Map<TOut>(Func<T, TOut> transform)
    {
        if (!IsSuccess)
        {
            return Result<TOut>.Failure(Error ?? "Unknown error", ErrorCode);
        }

        try
        {
            return Result<TOut>.Success(transform(Data!));
        }
        catch (Exception ex)
        {
            return Result<TOut>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Chains operations together (monadic bind).
    /// </summary>
    public async Task<Result<TOut>> BindAsync<TOut>(Func<T, Task<Result<TOut>>> operation)
    {
        if (!IsSuccess)
        {
            return Result<TOut>.Failure(Error ?? "Unknown error", ErrorCode);
        }

        return await operation(Data!);
    }

    /// <summary>
    /// Executes a side effect if the result is successful.
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (IsSuccess)
        {
            action(Data!);
        }

        return this;
    }

    /// <summary>
    /// Executes a side effect if the result failed.
    /// </summary>
    public Result<T> OnFailure(Action<string> action)
    {
        if (!IsSuccess)
        {
            action(Error ?? "Unknown error");
        }

        return this;
    }

    /// <summary>
    /// Gets the data or throws an exception if failed.
    /// </summary>
    public T GetOrThrow()
    {
        if (!IsSuccess)
        {
            throw new InvalidOperationException(Error ?? "Operation failed");
        }

        return Data!;
    }

    /// <summary>
    /// Gets the data or a default value if failed.
    /// </summary>
    public T GetOrDefault(T defaultValue)
    {
        return IsSuccess ? Data! : defaultValue;
    }
}

/// <summary>
/// Non-generic result class for operations that don't return data.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public int? ErrorCode { get; }

    private Result(bool isSuccess, string? error, int? errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success()
    {
        return new Result(true, null, null);
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static Result Failure(string error, int? errorCode = null)
    {
        return new Result(false, error, errorCode);
    }

    /// <summary>
    /// Creates a failed result from an exception.
    /// </summary>
    public static Result FromException(Exception ex)
    {
        return new Result(false, ex.Message, null);
    }

    /// <summary>
    /// Executes an operation and wraps the result.
    /// </summary>
    public static async Task<Result> Try(Func<Task> operation)
    {
        try
        {
            await operation();
            return Success();
        }
        catch (Exception ex)
        {
            return FromException(ex);
        }
    }

    /// <summary>
    /// Converts void result to generic result with data.
    /// </summary>
    public Result<T> ToGeneric<T>(T data)
    {
        if (!IsSuccess)
        {
            return Result<T>.Failure(Error ?? "Unknown error", ErrorCode);
        }

        return Result<T>.Success(data);
    }

    /// <summary>
    /// Executes a side effect if successful.
    /// </summary>
    public Result OnSuccess(Action action)
    {
        if (IsSuccess)
        {
            action();
        }

        return this;
    }

    /// <summary>
    /// Executes a side effect if failed.
    /// </summary>
    public Result OnFailure(Action<string> action)
    {
        if (!IsSuccess)
        {
            action(Error ?? "Unknown error");
        }

        return this;
    }

    /// <summary>
    /// Throws an exception if the result failed.
    /// </summary>
    public void ThrowIfFailed()
    {
        if (!IsSuccess)
        {
            throw new InvalidOperationException(Error ?? "Operation failed");
        }
    }
}
