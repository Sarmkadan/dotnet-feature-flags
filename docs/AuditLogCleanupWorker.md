# AuditLogCleanupWorker

The `AuditLogCleanupWorker` class provides a background service mechanism for automating the maintenance of audit log storage. It periodically scans the audit logs and removes entries that have exceeded the defined retention threshold, ensuring that database growth is controlled and storage resources are used efficiently.

## API

### Constructor

*   `public AuditLogCleanupWorker()`
    Initializes a new instance of the `AuditLogCleanupWorker` class with default configuration values.

### Properties

*   `public int RetentionDays`
    Gets or sets the maximum number of days to retain audit log records. Records older than this value are eligible for removal during the next cleanup cycle.

*   `public int CleanupIntervalHours`
    Gets or sets the frequency, in hours, at which the automated cleanup operation runs. A smaller interval increases the frequency of cleanups but also increases resource utilization.

*   `public bool Enabled`
    Gets or sets a value indicating whether the automated cleanup process is currently active. When `false`, the worker will not execute any cleanup tasks.

## Usage

### Registering the worker with default settings
```csharp
// Assuming standard Dependency Injection configuration
services.AddSingleton<AuditLogCleanupWorker>(new AuditLogCleanupWorker
{
    RetentionDays = 30,
    CleanupIntervalHours = 24,
    Enabled = true
});
```

### Dynamically toggling the worker
```csharp
// Example of disabling the cleanup worker based on application state
public void DisableCleanup(AuditLogCleanupWorker worker)
{
    if (worker.Enabled)
    {
        worker.Enabled = false;
    }
}
```

## Notes

*   **Retention Configuration:** Setting `RetentionDays` to zero or a negative value may result in the immediate removal of all audit logs, depending on the implementation logic. Ensure this value is set to a non-negative integer appropriate for organizational data policies.
*   **Thread Safety:** The properties `RetentionDays`, `CleanupIntervalHours`, and `Enabled` are intended for configuration. Modifications to these properties while the worker is actively performing a cleanup operation may lead to race conditions unless the implementation uses appropriate synchronization primitives. Users should ensure configuration is finalized during application startup or use thread-safe configuration patterns.
