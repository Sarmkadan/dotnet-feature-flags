// existing content ...

## AuditLogCleanupWorker

The `AuditLogCleanupWorker` is a background worker that periodically cleans up old audit logs based on the retention policy. It helps manage database size and comply with data retention regulations.

Example usage:
```csharp
var serviceProvider = new ServiceCollection()
    .AddFeatureFlags()
    .BuildServiceProvider();

var logger = serviceProvider.GetService<ILogger<AuditLogCleanupWorker>>();
var options = serviceProvider.GetService<AuditLogCleanupOptions>();

var worker = new AuditLogCleanupWorker(serviceProvider, logger, options);
worker.CleanupIntervalHours = 24;
worker.RetentionDays = 90;
worker.Enabled = true;

await worker.StartAsync();
```
