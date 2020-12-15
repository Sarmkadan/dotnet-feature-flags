# ApiResponse

The `ApiResponse` class and its generic variant `ApiResponse<T>` provide a standardized structure for encapsulating operation results, API responses, and associated metadata within the `dotnet-feature-flags` project. These types ensure consistency in API responses by packaging operation success status, returned data, informational messages, error details, and execution context into a unified envelope, facilitating predictable handling of service results across the application.

## API

### ApiResponse

The non-generic `ApiResponse` is used for operations that do not return a data payload.

*   **`bool Success`**: Indicates whether the operation was successful.
*   **`string? Message`**: Contains optional informational messages regarding the operation.
*   **`string? Error`**: Contains optional error details if the operation failed.
*   **`ApiMetadata? Metadata`**: Associated metadata for the response.
*   **`DateTime Timestamp`**: The date and time when the response was generated.
*   **`string? RequestId`**: The unique identifier for the request.
*   **`long? ExecutionTimeMs`**: The duration of the operation in milliseconds.
*   **`int? PageNumber`**: The current page number if the response is paginated.
*   **`int? PageSize`**: The size of the page if the response is paginated.
*   **`static ApiResponse Ok()`**: Returns a new `ApiResponse` instance marked as successful.
*   **`static ApiResponse Fail()`**: Returns a new `ApiResponse` instance marked as failed.

### ApiResponse\<T>

The generic `ApiResponse<T>` extends the base functionality to include a data payload of type `T`.

*   **`bool Success`**: Indicates whether the operation was successful.
*   **`T? Data`**: The data payload returned by the operation, if successful.
*   **`string? Message`**: Contains optional informational messages regarding the operation.
*   **`string? Error`**: Contains optional error details if the operation failed.
*   **`ApiMetadata? Metadata`**: Associated metadata for the response.
*   **`DateTime Timestamp`**: The date and time when the response was generated.
*   **`static ApiResponse<T> Ok(T data)`**: Returns a new `ApiResponse<T>` instance containing the provided data and marked as successful.
*   **`static ApiResponse<T> Fail()`**: Returns a new `ApiResponse<T>` instance marked as failed.
*   **`static ApiResponse<T> FromResult(T data)`**: Creates a new `ApiResponse<T>` instance containing the provided result data.

## Usage

### Returning a Successful Data Payload

```csharp
public ApiResponse<FeatureFlag> GetFeatureFlag(string flagName)
{
    var flag = _featureService.GetFlag(flagName);
    if (flag == null)
    {
        return ApiResponse<FeatureFlag>.Fail();
    }
    
    return ApiResponse<FeatureFlag>.Ok(flag);
}
```

### Returning a Generic Failure

```csharp
public ApiResponse DeleteConfiguration(string configId)
{
    if (!_configService.Exists(configId))
    {
        return ApiResponse.Fail();
    }
    
    _configService.Delete(configId);
    return ApiResponse.Ok();
}
```

## Notes

*   **Immutability**: The `ApiResponse` types are designed to be immutable once created. State changes should be achieved by constructing a new instance using the provided static factory methods.
*   **Nullability**: Members marked with `?` (e.g., `Data`, `Message`, `Error`) may be null. Consumers should implement appropriate null checks before accessing these properties to avoid `NullReferenceException` at runtime.
*   **Thread Safety**: These types are thread-safe for read operations as they do not permit modification of their state after initialization.
