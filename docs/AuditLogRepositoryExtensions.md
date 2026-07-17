# AuditLogRepositoryExtensions

Provides extension methods for querying and retrieving audit log entries from a feature flag repository. These methods facilitate filtering, pagination, and detailed retrieval of audit records associated with feature flag changes and user actions.

## API

### GetMostRecentAsync
Retrieves the single most recent audit log entry based on timestamp.

**Parameters:**  
None.

**Returns:**  
`Task<AuditLog?>` — The latest `AuditLog` entry, or `null` if no entries exist.

**Throws:**  
- `ArgumentNullException` — If the repository instance is null.
- `DbException` — If a database error occurs during query execution.

---

### GetByActionAsync
Retrieves all audit log entries matching a specific action type.

**Parameters:**  
- `action` (`string`) — The action type to filter by (e.g., "Enable", "Disable").

**Returns:**  
`Task<IReadOnlyList<AuditLog>>` — A list of matching `AuditLog` entries.

**Throws:**  
- `ArgumentNullException` — If the repository or action parameter is null.
- `ArgumentException` — If the action string is empty or whitespace.

---

### GetByUserInRangeAsync
Retrieves audit log entries for a specific user within a specified date range.

**Parameters:**  
- `userId` (`string`) — The user identifier to filter by.
- `start` (`DateTimeOffset`) — The start of the date range (inclusive).
- `end` (`DateTimeOffset`) — The end of the date range (inclusive).

**Returns:**  
`Task<IReadOnlyList<AuditLog>>` — All audit entries for the user within the specified range.

**Throws:**  
- `ArgumentNullException` — If the repository or userId is null.
- `ArgumentException` — If start is later than end.

---

### GetTotalCountAsync
Retrieves the total number of audit log entries in the repository.

**Parameters:**  
None.

**Returns:**  
`Task<int>` — The count of all audit log entries.

**Throws:**  
- `ArgumentNullException` — If the repository is null.
- `DbException` — If a database error occurs.

---

### GetByFeatureFlagIdsAsync
Retrieves audit log entries associated with one or more feature flag identifiers.

**Parameters:**  
- `flagIds` (`IEnumerable<string>`) — Collection of feature flag identifiers to filter by.

**Returns:**  
`Task<IReadOnlyList<AuditLog>>` — All audit entries related to the specified flags.

**Throws:**  
- `ArgumentNullException` — If the repository or flagIds is null.
- `ArgumentException` — If flagIds is empty.

---

### GetWithDetailsAsync
Retrieves audit log entries with enriched details (e.g., resolved user names, flag metadata).

**Parameters:**  
None.

**Returns:**  
`Task<IReadOnlyList<AuditLog>>` — All audit entries with additional contextual data populated.

**Throws:**  
- `ArgumentNullException` — If the repository is null.
- `InvalidOperationException` — If required detail resolution services are unavailable.

## Usage

```csharp
// Retrieve the most recent audit entry
var latestLog = await repository.GetMostRecentAsync();
if (latestLog != null)
{
    Console.WriteLine($"Last action: {latestLog.Action} by {latestLog.UserId}");
}
```

```csharp
// Get all "Enable" actions for a user in the last week
var userId = "user-123";
var start = DateTimeOffset.UtcNow.AddDays(-7);
var end = DateTimeOffset.UtcNow;

var logs = await repository.GetByUserInRangeAsync(userId, start, end);
foreach (var log in logs.Where(l => l.Action == "Enable"))
{
    Console.WriteLine($"{log.Timestamp}: {log.FeatureFlagId} enabled");
}
```

## Notes

- All methods require a non-null repository instance; behavior is undefined if the repository is improperly initialized.
- Date range queries in `GetByUserInRangeAsync` are inclusive of both start and end boundaries.
- `GetWithDetailsAsync` may perform additional lookups or joins internally, potentially impacting performance with large datasets.
- Thread safety depends on the underlying repository implementation; callers must ensure external synchronization if required.
- Methods returning `IReadOnlyList<T>` will return an empty list rather than null when no results match.
