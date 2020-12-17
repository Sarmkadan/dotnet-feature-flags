# ErrorHandlingMiddleware
The `ErrorHandlingMiddleware` class is designed to handle errors in a .NET application, providing a centralized mechanism for catching and processing exceptions. This middleware can be used to standardize error handling across an application, making it easier to manage and respond to errors in a consistent manner.

## API
* `public ErrorHandlingMiddleware`: The constructor for the `ErrorHandlingMiddleware` class, used to create a new instance of the middleware.
* `public async Task InvokeAsync`: This method is the core of the middleware, responsible for invoking the next middleware in the pipeline and handling any exceptions that occur. It is an asynchronous method that returns a `Task`.
* `public int StatusCode`: A property that gets the HTTP status code associated with the error.
* `public string Message`: A property that gets a human-readable message describing the error.
* `public string ErrorCode`: A property that gets a unique code identifying the type of error that occurred.
* `public DateTime Timestamp`: A property that gets the date and time when the error occurred.

## Usage
Here are two examples of using the `ErrorHandlingMiddleware` in a .NET application:
```csharp
// Example 1: Using the middleware in a ASP.NET Core application
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ErrorHandlingMiddleware>();
var app = builder.Build();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.Run();
```

```csharp
// Example 2: Using the middleware to handle exceptions in a custom pipeline
var middleware = new ErrorHandlingMiddleware();
try
{
    // Code that may throw an exception
    await middleware.InvokeAsync(next);
}
catch (Exception ex)
{
    // Handle the exception using the middleware
    var error = new ErrorHandlingMiddleware();
    Console.WriteLine($"Error {error.ErrorCode} at {error.Timestamp}: {error.Message}");
}
```

## Notes
When using the `ErrorHandlingMiddleware`, it's essential to consider the following edge cases:
* The middleware will only catch exceptions that occur during the invocation of the next middleware in the pipeline. Any exceptions that occur outside of this pipeline will not be caught.
* The `StatusCode`, `Message`, `ErrorCode`, and `Timestamp` properties will only be populated if an exception is caught by the middleware.
* The `ErrorHandlingMiddleware` is not thread-safe, meaning that it should not be shared across multiple threads. Each thread should have its own instance of the middleware to ensure proper error handling.
* The `InvokeAsync` method will throw an `ArgumentNullException` if the `next` parameter is null. It's crucial to ensure that this parameter is properly initialized before invoking the method.
