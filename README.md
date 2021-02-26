// existing content ...

## AuditControllerExtensions

The `AuditControllerExtensions` class provides a set of extension methods for the `AuditController` type that enable common operations for querying and analyzing audit logs. These methods allow you to easily retrieve recent activity, filter logs by action or multiple criteria, and get detailed change history.

Below is a realistic usage example demonstrating the most commonly used extension methods:

```csharp
// Assuming you have an instance of AuditController
var auditController = new AuditController();

// Get recent audit activity across all feature flags within the last 7 days
var recentActivityResult = await auditController.GetRecentActivity(
    days: 7,
    maxResults: 100
);
Console.WriteLine(recentActivityResult);

// Get audit logs filtered by specific action type
var logsByActionResult = await auditController.GetAuditLogsByAction(
    Enums.AuditAction.Updated,
    page: 1,
    pageSize: 20
);
Console.WriteLine(logsByActionResult);

// Get audit logs with enhanced filtering capabilities
var filteredLogsResult = await auditController.GetFilteredAuditLogs(
    startDate: DateTime.UtcNow.AddDays(-30),
    endDate: DateTime.UtcNow,
    action: Enums.AuditAction.Created,
    username: "john.doe"
);
Console.WriteLine(filteredLogsResult);

// Get the most recent changes across all feature flags
var mostRecentChangesResult = await auditController.GetMostRecentChanges(
    maxResults: 50
);
Console.WriteLine(mostRecentChangesResult);

// Get enhanced change history for a specific feature flag
var enhancedChangeHistoryResult = await auditController.GetEnhancedChangeHistory(
    featureFlagId: 123,
    maxEntries: 50,
    includeDetailedChanges: true
);
Console.WriteLine(enhancedChangeHistoryResult);
```

// existing content ...
