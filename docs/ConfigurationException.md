# ConfigurationException

ConfigurationException and its derived types represent configuration errors in the dotnet-feature-flags library. They are thrown when required configuration is missing, invalid, or cannot be loaded from the underlying source (database, HTTP endpoint, etc.). These exceptions wrap the original failure to provide context about which configuration subsystem failed.

## API

### ConfigurationException

#### `public ConfigurationException(string message) : base(message)`
Initializes a new instance with a descriptive error message.

**Parameters**  
- `message`: The reason the configuration is invalid or missing.

**Returns**  
A new ConfigurationException instance.

**Throws**  
Does not throw.

#### `public ConfigurationException(string message, Exception innerException) : base(message, innerException)`
Initializes a new instance with a descriptive error message and the underlying cause.

**Parameters**  
- `message`: The reason the configuration is invalid or missing.  
- `innerException`: The exception that caused this configuration failure.

**Returns**  
A new ConfigurationException instance.

**Throws**  
Does not throw.

### DatabaseConfigurationException

#### `public DatabaseConfigurationException(string message) : base(message)`
Initializes a new instance indicating a database-specific configuration failure.

**Parameters**  
- `message`: Details about the database configuration problem (e.g., connection string missing, schema mismatch).

**Returns**  
A new DatabaseConfigurationException instance.

**Throws**  
Does not throw.

#### `public DatabaseConfigurationException(string message, Exception innerException) : base(message, innerException)`
Initializes a new instance with a descriptive error message and the underlying database-related cause.

**Parameters**  
- `message`: Details about the database configuration problem.  
- `innerException`: The original exception from the database provider or connection attempt.

**Returns**  
A new DatabaseConfigurationException instance.

**Throws**  
Does not throw.

### HttpClientConfigurationException

#### `public HttpClientConfigurationException(string message) : base(message)`
Initializes a new instance indicating an HTTP client configuration failure.

**Parameters**  
- `message`: Details about the HTTP client configuration problem (e.g., base address missing, handler misconfigured).

**Returns**  
A new HttpClientConfigurationException instance.

**Throws**  
Does not throw.

#### `public HttpClientConfigurationException(string message, Exception innerException) : base(message, innerException)`
Initializes a new instance with a descriptive error message and the underlying HTTP-related cause.

**Parameters**  
- `message`: Details about the HTTP client configuration problem.  
- `innerException`: The original exception from HttpClient construction or request pipeline setup.

**Returns**  
A new HttpClientConfigurationException instance.

**Throws**  
Does not throw.

## Usage

```csharp
// Validating required configuration at startup
public void ConfigureServices(IServiceCollection services, IConfiguration config)
{
    var connectionString = config.GetConnectionString("FeatureFlags");
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new DatabaseConfigurationException(
            "Feature flag database connection string 'FeatureFlags' is not configured.");
    }

    var httpBaseAddress = config["FeatureFlags:Http:BaseAddress"];
    if (string.IsNullOrEmpty(httpBaseAddress))
    {
        throw new HttpClientConfigurationException(
            "Feature flag HTTP client base address is not configured.");
    }

    // ... register services
}
```

```csharp
// Wrapping lower-level failures with configuration context
public async Task<FeatureFlag[]> LoadFlagsAsync(CancellationToken ct)
{
    try
    {
        return await _dbContext.FeatureFlags.ToArrayAsync(ct);
    }
    catch (SqlException ex) when (ex.Number == 4060) // Cannot open database
    {
        throw new DatabaseConfigurationException(
            "Cannot open the configured feature flag database. Verify the connection string and database existence.",
            ex);
    }
    catch (HttpRequestException ex)
    {
        throw new HttpClientConfigurationException(
            "Failed to reach the feature flag configuration endpoint. Check network connectivity and base address.",
            ex);
    }
}
```

## Notes

- All three exception types are immutable after construction; they expose only the standard `Exception` properties (`Message`, `InnerException`, `StackTrace`, etc.).
- They do not introduce additional public properties or methods beyond those inherited from `Exception`.
- Thread safety: constructing and throwing these exceptions is thread-safe. The exception objects themselves are not intended for concurrent mutation (they are effectively read-only after creation).
- When catching, prefer catching the specific derived type (`DatabaseConfigurationException` or `HttpClientConfigurationException`) to handle configuration issues per subsystem. Catching the base `ConfigurationException` works for generic fallback logging or user-facing error reporting.
- The `innerException` parameter should always be supplied when wrapping a caught exception to preserve the original stack trace and diagnostic information.
