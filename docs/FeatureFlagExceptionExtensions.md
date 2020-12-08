# FeatureFlagExceptionExtensions
The `FeatureFlagExceptionExtensions` class provides a set of extension methods for working with exceptions related to feature flags. These methods allow you to inspect and handle exceptions in a more convenient and expressive way, making it easier to write robust and reliable code that interacts with feature flags.

## API
* `public static bool IsFeatureFlagNotFound`: Checks if the exception is related to a feature flag not being found. Returns `true` if the exception is a feature flag not found exception, `false` otherwise.
* `public static bool IsInvalidFeatureFlag`: Checks if the exception is related to an invalid feature flag. Returns `true` if the exception is an invalid feature flag exception, `false` otherwise.
* `public static bool IsRuleEvaluationError`: Checks if the exception is related to a rule evaluation error. Returns `true` if the exception is a rule evaluation error exception, `false` otherwise.
* `public static bool IsDataError`: Checks if the exception is related to a data error. Returns `true` if the exception is a data error exception, `false` otherwise.
* `public static string? GetErrorCode`: Retrieves the error code associated with the exception, if any. Returns the error code as a string, or `null` if no error code is available.
* `public static string GetFlattenedMessage`: Retrieves a flattened message for the exception, which can be useful for logging or display purposes. Returns the flattened message as a string.
* `public static bool HasErrorCode`: Checks if the exception has an error code associated with it. Returns `true` if the exception has an error code, `false` otherwise.

## Usage
Here are some examples of using the `FeatureFlagExceptionExtensions` class:
```csharp
try
{
    // Code that might throw a feature flag exception
}
catch (Exception ex) when (ex.IsFeatureFlagNotFound())
{
    Console.WriteLine("Feature flag not found");
}

try
{
    // Code that might throw a feature flag exception
}
catch (Exception ex)
{
    if (ex.HasErrorCode())
    {
        Console.WriteLine($"Error code: {ex.GetErrorCode()}");
    }
    Console.WriteLine($"Flattened message: {ex.GetFlattenedMessage()}");
}
```

## Notes
When using the `FeatureFlagExceptionExtensions` class, keep in mind that the methods are extension methods, which means they can be called on any instance of the `Exception` class. However, the methods will only return meaningful results if the exception is actually related to a feature flag. Additionally, the `GetErrorCode` and `GetFlattenedMessage` methods may return `null` or an empty string if no error code or message is available, so be sure to check for these cases when using these methods. The `FeatureFlagExceptionExtensions` class is thread-safe, as it only provides static methods that do not modify any shared state.
