# FlagEvaluationLog

`FlagEvaluationLog` is a plain data object that records the outcome of a single feature flag evaluation. It captures the flag name, the user for whom the flag was evaluated, the boolean result, the timestamp of the evaluation, and a human-readable reason explaining the result. This type is typically used for audit trails, telemetry, or debugging purposes within the `dotnet-feature-flags` library.

## API

- **`public string FlagName`**  
  Gets or sets the name of the evaluated feature flag. This value is never `null` when the object is properly constructed, but the property itself does not enforce non-null constraints.

- **`public string UserId`**  
  Gets or sets the identifier of the user for whom the flag was evaluated. May be `null` or empty if the evaluation was performed without a specific user context.

- **`public bool Result`**  
  Gets or sets the evaluation result. `true` indicates the flag is enabled for the given user; `false` indicates it is disabled.

- **`public DateTime Timestamp`**  
  Gets or sets the UTC timestamp when the evaluation occurred. The default value is `DateTime.MinValue` if not explicitly set.

- **`public string Reason`**  
  Gets or sets a textual explanation of why the flag evaluated to the given result. Common values include `"default"`, `"targeting rule matched"`, or `"user override"`. May be `null` or empty.

None of these members throw exceptions. They are simple read/write properties that accept any value of their respective types.

## Usage

The following example creates a `FlagEvaluationLog` instance after evaluating a feature flag and then logs it to the console.

```csharp
var log = new FlagEvaluationLog
{
    FlagName = "new-checkout-flow",
    UserId = "user-42",
    Result = true,
    Timestamp = DateTime.UtcNow,
    Reason = "targeting rule matched"
};

Console.WriteLine($"[{log.Timestamp:O}] Flag '{log.FlagName}' evaluated to {log.Result} for user '{log.UserId}': {log.Reason}");
```

The next example demonstrates how evaluation logs can be collected in a list for batch processing or storage.

```csharp
var logs = new List<FlagEvaluationLog>();

foreach (var userId in new[] { "alice", "bob", "charlie" })
{
    bool flagResult = EvaluateFlag("beta-feature", userId);
    logs.Add(new FlagEvaluationLog
    {
        FlagName = "beta-feature",
        UserId = userId,
        Result = flagResult,
        Timestamp = DateTime.UtcNow,
        Reason = flagResult ? "enabled for user" : "not in rollout group"
    });
}

// Persist logs (e.g., to a database or log file)
SaveEvaluationLogs(logs);
```

## Notes

- **Null and empty strings**: `FlagName`, `UserId`, and `Reason` are not validated by the type itself. Consumers should treat `null` or empty values as possible, especially when the log is deserialized from external sources or constructed without explicit assignment.
- **Timestamp semantics**: The `Timestamp` property is intended to be set in UTC. Using local time may cause ambiguity when logs are aggregated across time zones. The default value (`DateTime.MinValue`) should be avoided in production code.
- **Thread safety**: `FlagEvaluationLog` is a mutable reference type. Instances are not inherently thread-safe. If the same instance is read and written concurrently (e.g., from multiple threads), external synchronization is required. For typical usage—creating an instance, populating it, and then reading it—no synchronization is needed.
- **Equality**: No custom equality or hashing is implemented. Two instances with identical field values are not considered equal by default. Use structural comparison if needed (e.g., via records or custom `IEquatable<T>`).
