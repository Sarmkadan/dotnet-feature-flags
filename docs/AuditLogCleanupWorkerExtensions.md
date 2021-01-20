# AuditLogCleanupWorkerExtensions

Provides extension methods for registering and configuring the `AuditLogCleanupWorker` background service in a .NET application's dependency injection container. This worker is responsible for periodically cleaning up audit log entries based on retention policies and intervals.

## API

### `AddAuditLogCleanupWorker(IServiceCollection)`

Registers the `AuditLogCleanupWorker` as a hosted service in the DI container with default configuration.

- **Parameters**:  
  - `services` (`IServiceCollection`): The service collection to add the worker to.
- **Returns**:  
  - `IServiceCollection`: The modified service collection for method chaining.
- **Exceptions**:  
  - None explicitly thrown by this method.

---

### `AddAuditLogCleanupWorker(IServiceCollection, Action<AuditLogCleanupOptions>)`

Registers the `AuditLogCleanupWorker` with custom configuration provided via an options lambda.

- **Parameters**:  
  - `services` (`IServiceCollection`): The service collection to add the worker to.  
  - `configureOptions` (`Action<AuditLogCleanupOptions>`): A delegate to configure `AuditLogCleanupOptions`.
- **Returns**:  
  - `IServiceCollection`: The modified service collection for method chaining.
- **Exceptions**:  
  - None explicitly thrown by this method.

---

### `WithRetentionDays(AuditLogCleanupOptions, int)`

Configures the number of days to retain audit log entries before cleanup.

- **Parameters**:  
  - `options` (`AuditLogCleanupOptions`): The options instance to modify.  
  - `days` (`int`): The retention period in days. Must be a positive integer.
- **Returns**:  
  - `AuditLogCleanupOptions`: The modified options instance for method chaining.
- **Exceptions**:  
  - `ArgumentOutOfRangeException` if `days` is less than or equal to zero.

---

### `WithCleanupIntervalHours(AuditLogCleanupOptions, int)`

Configures the interval (in hours) between cleanup operations.

- **Parameters**:  
  - `options` (`AuditLogCleanupOptions`): The options instance to modify.  
  - `hours` (`int`): The cleanup interval in hours. Must be a positive integer.
- **Returns**:  
  - `AuditLogCleanupOptions`: The modified options instance for method chaining.
- **Exceptions**:  
  - `ArgumentOutOfRangeException` if `hours` is less than or equal to zero.

---

### `WithEnabled(AuditLogCleanupOptions, bool)`

Enables or disables the audit log cleanup worker.

- **Parameters**:  
  - `options` (`AuditLogCleanupOptions`): The options instance to modify.  
  - `enabled` (`bool`): Whether the worker is active.
- **Returns**:  
  - `AuditLogCleanupOptions`: The modified options instance for method chaining.
- **Exceptions**:  
  - None explicitly thrown by this method.

---

### `GetCleanupIntervalSeconds(AuditLogCleanupOptions)`

Retrieves the configured cleanup interval converted to seconds.

- **Parameters**:  
  - `options` (`AuditLogCleanupOptions`): The options instance to read from.
- **Returns**:  
  - `int`: The cleanup interval in seconds.
- **Exceptions**:  
  - None explicitly thrown by this method.

---

### `GetRetentionDays(AuditLogCleanupOptions)`

Retrieves the configured retention period in days.

- **Parameters**:  
  - `options` (`AuditLogCleanupOptions`): The options instance to read from.
- **Returns**:  
  - `int`: The retention period in days.
- **Exceptions**:  
  - None explicitly thrown by this method.

---

## Usage

### Basic Registration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuditLogCleanupWorker();
}
```

### Custom Configuration

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuditLogCleanupWorker(options =>
    {
        options.WithRetentionDays(30)
               .WithCleanupIntervalHours(24)
               .WithEnabled(true);
    });
}
```

---

## Notes

- **Configuration Timing**: Options must be configured during application startup. Changes to `AuditLogCleanupOptions` after the worker has started will not affect its behavior.
- **Thread Safety**: The extension methods are thread-safe during configuration, as they operate on isolated `AuditLogCleanupOptions` instances. However, concurrent modifications to shared options instances are not supported.
- **Validation**: Invalid values (e.g., non-positive retention days or intervals) will throw `ArgumentOutOfRangeException` during configuration.
- **Default Behavior**: If `AddAuditLogCleanupWorker` is called without explicit configuration, default values for retention days and cleanup intervals are used (implementation-defined).
