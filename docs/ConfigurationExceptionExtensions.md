# ConfigurationExceptionExtensions

`ConfigurationExceptionExtensions` provides utility methods for analyzing and formatting `ConfigurationException` instances within the `dotnet-feature-flags` framework. These extension methods facilitate the identification of specific configuration error types, extraction of root cause messages, and generation of human-readable formatted error strings to aid in debugging and logging scenarios.

## API

### IsDatabaseConfigurationError

Determines whether the specified `ConfigurationException` represents an error related to database configuration.

**Parameters**
- `exception` (`ConfigurationException`): The exception to evaluate.

**Returns**
- `bool`: `true` if the exception indicates a database configuration issue; otherwise, `false`.

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` is `null`.

---

### IsHttpClientConfigurationError

Determines whether the specified `ConfigurationException` represents an error related to HTTP client configuration.

**Parameters**
- `exception` (`ConfigurationException`): The exception to evaluate.

**Returns**
- `bool`: `true` if the exception indicates an HTTP client configuration issue; otherwise, `false`.

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` is `null`.

---

### GetRootCauseMessage

Retrieves the root cause message from the specified `ConfigurationException` by traversing its inner exception chain.

**Parameters**
- `exception` (`ConfigurationException`): The exception to analyze.

**Returns**
- `string`: The message of the innermost exception in the chain, or the original exception message if no inner exceptions exist.

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` is `null`.

---

### ToFormattedMessage

Generates a formatted string representation of the specified `ConfigurationException`, including its type, message, and stack trace.

**Parameters**
- `exception` (`ConfigurationException`): The exception to format.

**Returns**
- `string`: A multi-line string containing the exception details in a structured format.

**Exceptions**
- `ArgumentNullException`: Thrown when `exception` is `null`.

## Usage

```csharp
try
{
    // Attempt to load feature flag configuration
    var config = ConfigurationLoader.LoadFromDatabase();
}
catch (ConfigurationException ex)
{
    if (ex.IsDatabaseConfigurationError())
    {
        Console.WriteLine("Database configuration failed: " + ex.GetRootCauseMessage());
    }
    else
    {
        Console.WriteLine("Unexpected configuration error: " + ex.ToFormattedMessage());
    }
}
```

```csharp
try
{
    // Attempt to initialize HTTP-based feature flag provider
    var client = new FeatureFlagHttpClient();
}
catch (ConfigurationException ex) when (ex.IsHttpClientConfigurationError())
{
    // Log detailed error information for HTTP client misconfiguration
    logger.LogError("HTTP client setup failed:\n" + ex.ToFormattedMessage());
}
```

## Notes

- All methods require a non-null `ConfigurationException` instance and will throw `ArgumentNullException` if passed `null`.
- `GetRootCauseMessage` recursively inspects `InnerException` properties to locate the originating error message. If the exception chain is deeply nested or circular, this may result in a stack overflow.
- `ToFormattedMessage` produces output that includes the exception type name, message, and stack trace. This method is not optimized for performance and should be used primarily in diagnostic contexts.
- These extension methods are stateless and thread-safe, as they do not modify shared state or rely on mutable static fields.
- The classification methods (`IsDatabaseConfigurationError`, `IsHttpClientConfigurationError`) may not detect errors from custom or third-party configuration sources unless those exceptions explicitly inherit from recognized types or include matching metadata.
