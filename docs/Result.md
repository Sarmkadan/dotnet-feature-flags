# Result

The `Result` and `Result<T>` types provide a mechanism for handling operations that can succeed or fail without relying on exceptions for control flow. By encapsulating either the successful result data or the error details (message and code), these types promote explicit handling of failure states, leading to safer and more maintainable code.

## API

### Common Properties (Result and Result<T>)
*   **IsSuccess**: `bool`. Indicates whether the operation completed successfully.
*   **Error**: `string?`. The error message if the operation failed; otherwise, `null`.
*   **ErrorCode**: `int?`. The error code associated with the failure if applicable; otherwise, `null`.

### Properties (Result<T> only)
*   **Data**: `T?`. The encapsulated data if the operation succeeded; otherwise, `null`.

### Static Factory Methods (Result<T>)
*   **Success(T data)**: `Result<T>`. Creates a successful result containing the provided data.
*   **Failure(string error, int? errorCode)**: `Result<T>`. Creates a failed result with an error message and optional code.
*   **FromException(Exception ex)**: `Result<T>`. Creates a failed result by capturing the exception details.
*   **Try(Func<Task<T>> action)**: `async Task<Result<T>>`. Executes an asynchronous action, returning a successful result if successful or a failed result capturing any thrown exception.

### Instance Methods (Result<T>)
*   **Map<TOut>(Func<T, TOut> mapper)**: `Result<TOut>`. If successful, transforms the internal data using the provided mapper. Returns a failed result if the original result failed.
*   **BindAsync<TOut>(Func<T, Task<Result<TOut>>> binder)**: `async Task<Result<TOut>>`. If successful, executes the provided asynchronous binder function. Returns a failed result if the original result failed.
*   **OnSuccess(Action<T> action)**: `Result<T>`. Executes the provided action if the result is successful, then returns the original result.
*   **OnFailure(Action<string?, int?> action)**: `Result<T>`. Executes the provided action if the result failed, then returns the original result.
*   **GetOrThrow()**: `T`. Returns the encapsulated data if successful; throws `InvalidOperationException` if the result failed.
*   **GetOrDefault(T defaultValue)**: `T`. Returns the encapsulated data if successful; otherwise, returns the provided default value.

### Static Factory Methods (Result)
*   **Success()**: `Result`. Creates a successful non-generic result.
*   **Failure(string error, int? errorCode)**: `Result`. Creates a failed non-generic result with an error message and optional code.
*   **FromException(Exception ex)**: `Result`. Creates a failed non-generic result by capturing the exception details.

## Usage

### Handling Service Results

```csharp
public Result<string> GetUserName(int userId)
{
    if (userId <= 0)
    {
        return Result<string>.Failure("Invalid user ID.", 400);
    }
    return Result<string>.Success("John Doe");
}

var result = GetUserName(123);
if (result.IsSuccess)
{
    Console.WriteLine($"Hello, {result.Data}");
}
else
{
    Console.WriteLine($"Error {result.ErrorCode}: {result.Error}");
}
```

### Chaining Asynchronous Operations

```csharp
public async Task ProcessDataAsync()
{
    var result = await Result<int>.Try(() => FetchDataAsync())
        .BindAsync(data => ProcessDataAsync(data));

    if (result.IsSuccess)
    {
        Console.WriteLine("Data processed successfully.");
    }
    else
    {
        Console.WriteLine($"Processing failed: {result.Error}");
    }
}
```

## Notes

*   **Thread Safety**: Both `Result` and `Result<T>` are immutable. Once a result instance is created, its state cannot be modified, making instances safe for use across threads.
*   **Exception Handling**: While `Result` types are intended to minimize exception-based flow, `GetOrThrow` explicitly provides a mechanism to convert a failed result back into an exception-driven flow if necessary, such as when strict adherence to a calling interface is required.
*   **Nullability**: When `IsSuccess` is false, `Data` in `Result<T>` will be `null` or the default value of `T`. Callers should check `IsSuccess` before accessing `Data` or using `GetOrThrow`.
