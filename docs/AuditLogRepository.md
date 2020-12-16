# AuditLogRepository
The `AuditLogRepository` class is designed to manage and interact with audit logs, providing a range of methods for retrieving, adding, updating, and deleting log entries. It serves as a central point for audit log data access and manipulation, enabling features such as tracking changes to feature flags and other system configurations.

## API
### Constructors
- `public AuditLogRepository`: Initializes a new instance of the `AuditLogRepository` class.

### Methods
- `public async Task<AuditLog?> GetByIdAsync`: Retrieves an audit log by its ID. Returns the log entry if found, or `null` if not found.
- `public async Task<IEnumerable<AuditLog>> GetAllAsync`: Retrieves all audit log entries.
- `public async Task<IEnumerable<AuditLog>> GetByFeatureFlagIdAsync`: Retrieves audit log entries associated with a specific feature flag ID.
- `public async Task<IEnumerable<AuditLog>> GetByChangedByAsync`: Retrieves audit log entries made by a specific user or entity.
- `public async Task<IEnumerable<AuditLog>> GetSinceAsync`: Retrieves audit log entries that have occurred since a specified date and time.
- `public async Task<IEnumerable<AuditLog>> GetPagedAsync`: Retrieves a paged set of audit log entries.
- `public async Task<IEnumerable<AuditLog>> GetByFeatureFlagIdPagedAsync`: Retrieves a paged set of audit log entries associated with a specific feature flag ID.
- `public async Task<int> GetCountByFeatureFlagIdAsync`: Retrieves the number of audit log entries associated with a specific feature flag ID.
- `public async Task<AuditLog?> GetLastChangeAsync`: Retrieves the most recent audit log entry.
- `public async Task<IEnumerable<AuditLog>> GetChangesInRangeAsync`: Retrieves audit log entries that fall within a specified date and time range.
- `public async Task<IEnumerable<AuditLog>> GetByActionAsync`: Retrieves audit log entries of a specific action type.
- `public async Task<AuditLog> AddAsync`: Adds a new audit log entry.
- `public async Task UpdateAsync`: Updates an existing audit log entry.
- `public async Task DeleteAsync`: Deletes an audit log entry.
- `public async Task<bool> ExistsAsync`: Checks if an audit log entry exists.
- `public async Task SaveChangesAsync`: Saves any pending changes to the audit log repository.
- `public async Task CleanupOldLogsAsync`: Removes old audit log entries based on a predefined retention policy.

## Usage
The following examples demonstrate how to use the `AuditLogRepository` class:
```csharp
// Example 1: Retrieving all audit log entries
var repository = new AuditLogRepository();
var logs = await repository.GetAllAsync();
foreach (var log in logs)
{
    Console.WriteLine($"ID: {log.Id}, Feature Flag ID: {log.FeatureFlagId}, Changed By: {log.ChangedBy}");
}

// Example 2: Adding a new audit log entry
var newLog = new AuditLog
{
    FeatureFlagId = 1,
    ChangedBy = "John Doe",
    ChangeDate = DateTime.UtcNow,
    Action = "Updated"
};
var addedLog = await repository.AddAsync(newLog);
Console.WriteLine($"Added log entry with ID: {addedLog.Id}");
```

## Notes
- The `AuditLogRepository` class is designed to be thread-safe, allowing concurrent access and manipulation of audit log data.
- When using the `GetPagedAsync` and `GetByFeatureFlagIdPagedAsync` methods, be aware that the page size and offset parameters can significantly impact performance and data retrieval.
- The `CleanupOldLogsAsync` method is intended to be used periodically to maintain a reasonable size of the audit log repository and prevent data growth from impacting system performance.
- Error handling and logging mechanisms should be implemented when using the `AuditLogRepository` class to ensure that any exceptions or issues are properly caught and addressed.
