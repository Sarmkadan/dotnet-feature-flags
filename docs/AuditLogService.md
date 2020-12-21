# AuditLogService

`AuditLogService` provides a concrete implementation of `IAuditLogService` for recording, querying, and maintaining an audit trail of feature flag changes. It tracks who changed what, when, and the nature of the modification, enabling compliance, debugging, and operational visibility into flag lifecycle events.

## API

### AuditLogService

Initializes a new instance of the service. The constructor is parameterless; it relies on injected dependencies or ambient configuration resolved at runtime for storage access and logging.

### GetAuditLogsAsync

```csharp
public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync()
```

Retrieves all audit log entries in the system, ordered by timestamp descending. Returns an empty collection if no logs exist.

- **Returns:** `Task<IEnumerable<AuditLog>>` — a task whose result is the complete set of audit records.
- **Throws:** May throw if the underlying data store is unavailable or a serialization error occurs.

### GetAuditLogsPagedAsync

```csharp
public async Task<IEnumerable<AuditLog>> GetAuditLogsPagedAsync(int page, int pageSize)
```

Fetches a paginated subset of audit logs. Page numbering is 1-based. If the requested page exceeds available data, an empty collection is returned.

- **Parameters:**
  - `page` — the 1-based page number to retrieve.
  - `pageSize` — the maximum number of records per page.
- **Returns:** `Task<IEnumerable<AuditLog>>` — the records for the requested page.
- **Throws:** `ArgumentOutOfRangeException` when `page` is less than 1 or `pageSize` is less than or equal to 0. May throw storage-related exceptions.

### GetAuditLogsByUserAsync

```csharp
public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(string userId)
```

Filters audit logs to those initiated by a specific user identity. Matching is case-sensitive and exact.

- **Parameters:**
  - `userId` — the unique identifier of the user whose actions to retrieve.
- **Returns:** `Task<IEnumerable<AuditLog>>` — all audit entries associated with the given user.
- **Throws:** `ArgumentNullException` when `userId` is null or whitespace. Storage exceptions may propagate.

### GetRecentAuditLogsAsync

```csharp
public async Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count)
```

Returns the most recent `count` audit log entries, ordered by timestamp descending. Useful for dashboards or quick inspection of latest activity.

- **Parameters:**
  - `count` — the maximum number of recent entries to return.
- **Returns:** `Task<IEnumerable<AuditLog>>` — up to `count` of the newest audit records.
- **Throws:** `ArgumentOutOfRangeException` when `count` is less than 1. Storage failures may also throw.

### GetLastChangeAsync

```csharp
public async Task<AuditLog?> GetLastChangeAsync(string featureFlagId)
```

Retrieves the single most recent audit entry for a specific feature flag. Returns `null` if the flag has never been modified or has no recorded history.

- **Parameters:**
  - `featureFlagId` — the identifier of the feature flag whose last change is queried.
- **Returns:** `Task<AuditLog?>` — the latest audit record for the flag, or `null`.
- **Throws:** `ArgumentNullException` when `featureFlagId` is null or whitespace. Storage exceptions may propagate.

### GetChangeHistoryAsync

```csharp
public async Task<IEnumerable<AuditLog>> GetChangeHistoryAsync(string featureFlagId)
```

Returns the complete chronological history of changes for a single feature flag, ordered by timestamp descending.

- **Parameters:**
  - `featureFlagId` — the identifier of the feature flag whose history is requested.
- **Returns:** `Task<IEnumerable<AuditLog>>` — all audit entries for the specified flag.
- **Throws:** `ArgumentNullException` when `featureFlagId` is null or whitespace. Storage exceptions may propagate.

### CleanupOldLogsAsync

```csharp
public async Task CleanupOldLogsAsync(TimeSpan retentionPeriod)
```

Removes audit log entries older than the specified retention period from the current point in time. This is a destructive operation; deleted records cannot be recovered through this service.

- **Parameters:**
  - `retentionPeriod` — the age threshold beyond which logs are purged.
- **Returns:** `Task` — a task representing the asynchronous cleanup operation.
- **Throws:** `ArgumentOutOfRangeException` when `retentionPeriod` is negative or zero. Storage exceptions may propagate.

## Usage

### Example 1: Displaying the last change for a flag on a dashboard

```csharp
var auditService = new AuditLogService();
string flagId = "checkout-redesign";

AuditLog? lastChange = await auditService.GetLastChangeAsync(flagId);

if (lastChange is not null)
{
    Console.WriteLine($"Flag '{flagId}' last modified by {lastChange.UserId} at {lastChange.Timestamp:g}");
    Console.WriteLine($"Change: {lastChange.OldValue} → {lastChange.NewValue}");
}
else
{
    Console.WriteLine($"Flag '{flagId}' has no recorded changes.");
}
```

### Example 2: Paginated audit review with retention enforcement

```csharp
var auditService = new AuditLogService();

// Enforce a 90-day retention policy before browsing logs
await auditService.CleanupOldLogsAsync(TimeSpan.FromDays(90));

const int pageSize = 20;
int page = 1;
IEnumerable<AuditLog> pageEntries;

do
{
    pageEntries = await auditService.GetAuditLogsPagedAsync(page, pageSize);

    foreach (var entry in pageEntries)
    {
        Console.WriteLine($"[{entry.Timestamp:O}] {entry.UserId} changed '{entry.FeatureFlagId}'");
    }

    page++;
} while (pageEntries.Any());
```

## Notes

- All query methods return empty collections rather than null when no matching records exist, except `GetLastChangeAsync`, which returns `null` to distinguish “no history” from “history exists.”
- Pagination in `GetAuditLogsPagedAsync` is 1-based. Passing `page = 0` throws `ArgumentOutOfRangeException`.
- `CleanupOldLogsAsync` applies the retention period relative to the instant it executes. Repeated calls in quick succession are idempotent in effect but each incurs storage cost.
- The service does not expose a synchronous API; all operations are asynchronous and should be awaited to avoid blocking behavior.
- Thread safety depends on the underlying storage implementation. The service itself holds no mutable shared state beyond what the data store provides, making it safe for concurrent use as long as the storage layer supports concurrent access.
- No built-in change notification or event raising occurs when logs are written or purged. Consumers needing reactivity must implement their own polling or messaging layer.
