# FlagEvaluationLogService

`FlagEvaluationLogService` provides an in-memory logging mechanism for recording and querying feature flag evaluation events. It captures details such as the evaluated flag name, the user identifier associated with the evaluation, and the resolved variant or value, enabling audit trails and diagnostic inspection of flag resolution behavior.

## API

### `FlagEvaluationLogService()`
Default constructor. Initializes a new instance with an empty internal log store.

### `void Log(FlagEvaluationLog log)`
Records a single `FlagEvaluationLog` entry into the internal store.

- **Parameters:**
  - `log` (`FlagEvaluationLog`): The evaluation event to persist. Must not be `null`.
- **Throws:** `ArgumentNullException` if `log` is `null`.

### `void LogEvaluation(string flagName, string userId, object evaluatedValue)`
Convenience method that constructs a `FlagEvaluationLog` from the given arguments and records it.

- **Parameters:**
  - `flagName` (`string`): The name of the feature flag that was evaluated.
  - `userId` (`string`): The identifier of the user for whom the flag was evaluated.
  - `evaluatedValue` (`object`): The resolved value or variant returned by the evaluation.
- **Throws:** `ArgumentNullException` if `flagName` or `userId` is `null`.

### `IReadOnlyList<FlagEvaluationLog> GetAll()`
Returns a snapshot of every recorded evaluation log entry, in insertion order.

- **Returns:** An `IReadOnlyList<FlagEvaluationLog>` containing all entries. Returns an empty list if no logs have been recorded.

### `IReadOnlyList<FlagEvaluationLog> GetByUserId(string userId)`
Returns all evaluation log entries associated with the specified user identifier.

- **Parameters:**
  - `userId` (`string`): The user identifier to filter by.
- **Returns:** An `IReadOnlyList<FlagEvaluationLog>` of matching entries in insertion order. Returns an empty list if no matches exist.
- **Throws:** `ArgumentNullException` if `userId` is `null`.

### `IReadOnlyList<FlagEvaluationLog> GetByFlagName(string flagName)`
Returns all evaluation log entries for the specified feature flag name.

- **Parameters:**
  - `flagName` (`string`): The flag name to filter by.
- **Returns:** An `IReadOnlyList<FlagEvaluationLog>` of matching entries in insertion order. Returns an empty list if no matches exist.
- **Throws:** `ArgumentNullException` if `flagName` is `null`.

### `IReadOnlyList<FlagEvaluationLog> GetEvaluationLogs()`
Alias for `GetAll()`. Returns a snapshot of every recorded evaluation log entry.

- **Returns:** An `IReadOnlyList<FlagEvaluationLog>` containing all entries.

### `IReadOnlyList<FlagEvaluationLog> GetEvaluationLogsForUser(string userId)`
Alias for `GetByUserId(string)`. Returns all evaluation log entries for the given user.

- **Parameters:**
  - `userId` (`string`): The user identifier to filter by.
- **Returns:** An `IReadOnlyList<FlagEvaluationLog>` of matching entries.
- **Throws:** `ArgumentNullException` if `userId` is `null`.

### `IReadOnlyList<FlagEvaluationLog> GetEvaluationLogsForFlag(string flagName)`
Alias for `GetByFlagName(string)`. Returns all evaluation log entries for the given flag name.

- **Parameters:**
  - `flagName` (`string`): The flag name to filter by.
- **Returns:** An `IReadOnlyList<FlagEvaluationLog>` of matching entries.
- **Throws:** `ArgumentNullException` if `flagName` is `null`.

### `void ClearLogs()`
Removes all recorded evaluation log entries from the internal store, resetting the service to its initial empty state.

## Usage

### Example 1: Recording and querying evaluations

```csharp
var logService = new FlagEvaluationLogService();

// Record evaluations using the convenience method
logService.LogEvaluation("new-checkout-ui", "user-42", "variant-b");
logService.LogEvaluation("dark-mode", "user-42", true);
logService.LogEvaluation("new-checkout-ui", "user-99", "control");

// Retrieve all logs for a specific flag
IReadOnlyList<FlagEvaluationLog> checkoutLogs =
    logService.GetByFlagName("new-checkout-ui");

foreach (var log in checkoutLogs)
{
    Console.WriteLine(
        $"User {log.UserId} got {log.EvaluatedValue} for flag {log.FlagName}");
}
```

### Example 2: Auditing a user's flag resolution history

```csharp
var logService = new FlagEvaluationLogService();

// Simulate multiple flag evaluations across a session
logService.LogEvaluation("beta-feature", "user-17", "enabled");
logService.LogEvaluation("regional-pricing", "user-17", "eu-west");
logService.LogEvaluation("beta-feature", "user-17", "enabled");

// Audit trail for a specific user
IReadOnlyList<FlagEvaluationLog> userLogs =
    logService.GetEvaluationLogsForUser("user-17");

Console.WriteLine($"Total evaluations for user-17: {userLogs.Count}");

// Clear logs after exporting
logService.ClearLogs();
```

## Notes

- All query methods return snapshots (as `IReadOnlyList<FlagEvaluationLog>`) of the internal state at the time of the call. Subsequent modifications to the log store are not reflected in previously returned lists.
- The `Log` and `LogEvaluation` methods, along with `ClearLogs`, mutate internal state. In multi-threaded scenarios, external synchronization is required if concurrent writes and reads must be consistent. The class itself does not guarantee thread safety.
- The aliased method pairs (`GetAll`/`GetEvaluationLogs`, `GetByUserId`/`GetEvaluationLogsForUser`, `GetByFlagName`/`GetEvaluationLogsForFlag`) are functionally identical. Either form may be used interchangeably.
- `LogEvaluation` constructs a `FlagEvaluationLog` internally. If additional metadata beyond flag name, user ID, and evaluated value is required, use the `Log(FlagEvaluationLog)` overload with a pre-constructed instance.
- `ClearLogs` is irreversible; once called, previously stored entries cannot be recovered.
