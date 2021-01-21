# AuditLogExtensions

Provides a set of static extension methods for interpreting `AuditLogEntry` instances produced by the feature‑flag system. These helpers simplify common queries such as determining the type of change, formatting timestamps, and obtaining human‑readable descriptions without exposing internal audit‑log details.

## API

### IsStateChange
```csharp
public static bool IsStateChange(this AuditLogEntry entry)
```
**Purpose** – Returns `true` when the audit log entry corresponds to a modification of the feature flag’s enabled/disabled state.  
**Parameters**  
- `entry`: The audit log entry to evaluate.  
**Return value** – `true` if the entry represents a state change; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `entry` is `null`.

### IsCreation
```csharp
public static bool IsCreation(this AuditLogEntry entry)
```
**Purpose** – Indicates whether the entry records the creation of a feature flag.  
**Parameters**  
- `entry`: The audit log entry to evaluate.  
**Return value** – `true` for creation events; `false` otherwise.  
**Exceptions** – Throws `ArgumentNullException` if `entry` is `null`.

### IsDeletion
```csharp
public static bool IsDeletion(this AuditLogEntry entry)
```
**Purpose** – Indicates whether the entry records the deletion of a feature flag.  
**Parameters**  
- `entry`: The audit log entry to evaluate.  
**Return value** – `true` for deletion events; `false` otherwise.  
**Exceptions** – Throws `ArgumentNullException` if `entry` is `null`.

### GetTimeSinceChange
```csharp
public static string GetTimeSinceChange(this AuditLogEntry entry)
```
**Purpose** – Produces a human‑readable string describing the elapsed time since the change occurred (e.g., “3 minutes ago”, “2 days ago”).  
**Parameters**  
- `entry`: The audit log entry containing a timestamp.  
**Return value** – A formatted relative time string. If the timestamp is unavailable, returns an empty string.  
**Exceptions** – Throws `ArgumentNullException` if `entry` is `null`.

### GetDetailedChangeDescription
```csharp
public static string GetDetailedChangeDescription(this AuditLogEntry entry)
```
**Purpose** – Returns a detailed textual description of what changed in the audit log entry (e.g., “Enabled flag changed from false to true”).  
**Parameters**  
- `entry`: The audit log entry to describe.  
**Return value** – A description string; empty if no change details are present.  
**Exceptions** – Throws `ArgumentNullException` if `entry` is `null`.

### IsRecent
```csharp
public static bool IsRecent(this AuditLogEntry entry)
```
**Purpose** – Determines whether the change occurred within a recent window (default 24 hours).  
**Parameters**  
- `entry`: The audit log entry to evaluate.  
**Return value** – `true` if the timestamp is within the recent window; otherwise `false`.  
**Exceptions** – Throws `ArgumentNullException` if `entry` is `null`.

### GetActionDisplayName
```csharp
public static string GetActionDisplayName(this AuditLogEntry entry)
```
**Purpose** – Provides a localized, user‑friendly name for the action represented by the entry (e.g., “Create”, “Update”, “Delete”).  
**Parameters**  
- `entry`: The audit log entry to interpret.  
**Return value** – The display name of the action; empty string if the action cannot be determined.  
**Exceptions** – Throws `ArgumentNullException` if `entry` is `null`.

## Usage

```csharp
using FeatureFlags.Audit;

// Assume `logEntry` is an AuditLogEntry retrieved from the store.
if (logEntry.IsCreation())
{
    Console.WriteLine($"Flag {logEntry.FeatureName} was created at {logEntry.Timestamp}.");
}

string ago = logEntry.GetTimeSinceChange();
Console.WriteLine($"The change happened {ago}.");
```

```csharp
using FeatureFlags.Audit;

var recentChanges = auditLog
    .Where(e => e.IsRecent())
    .Select(e => new
    {
        Action = e.GetActionDisplayName(),
        Description = e.GetDetailedChangeDescription(),
        When = e.GetTimeSinceChange()
    });

foreach (var change in recentChanges)
{
    Console.WriteLine($"{change.Action}: {change.Description} ({change.When})");
}
```

## Notes

- All extension methods are pure; they do not modify the supplied `AuditLogEntry` instance and rely only on its properties. Consequently, they are thread‑safe for concurrent invocation on different entries.
- Passing `null` for the `entry` argument results in an `ArgumentNullException`; callers should validate or guard against null values before invoking these helpers.
- If an entry lacks a timestamp (e.g., due to incomplete logging), `GetTimeSinceChange` returns an empty string rather than throwing.
- The “recent” window used by `IsRecent` is fixed at 24 hours and is not configurable via the method signature; callers needing a different threshold must compute the comparison manually.
- These methods are intended for presentation or light‑weight filtering scenarios. For complex audit‑log queries, consider accessing the underlying properties directly or using a dedicated query library.
