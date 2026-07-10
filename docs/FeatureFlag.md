# FeatureFlag

The `FeatureFlag` class serves as the primary domain model representing a configurable feature toggle within the `dotnet-feature-flags` system. It encapsulates the metadata, state, and rollout configuration required to determine whether a specific feature is active for a given context. This entity supports various activation strategies, including simple boolean switches, percentage-based rollouts, and complex rule sets, while maintaining a comprehensive audit trail of changes and supporting A/B testing variations.

## API

### Properties

#### `public int Id`
Gets the unique numerical identifier for the feature flag within the persistence store. This value is typically auto-generated upon creation and serves as the primary key for database operations.

#### `public string Key`
Gets the unique string identifier used to reference this feature flag in code. The key is immutable after creation and is the standard lookup mechanism for evaluating flag state at runtime.

#### `public string DisplayName`
Gets the human-readable name of the feature flag, intended for display in administrative dashboards or management UIs.

#### `public string Description`
Gets the detailed description explaining the purpose, scope, and expected behavior of the feature associated with this flag.

#### `public bool IsEnabled`
Gets or sets the global enabled state of the feature flag. When `true`, the feature is generally active subject to specific rules; when `false`, the feature is disabled regardless of other configurations unless overridden by specific logic.

#### `public RolloutType RolloutType`
Gets or sets the enumeration value defining the strategy used to evaluate this flag (e.g., `Standard`, `Percentage`, `RulesBased`). This property dictates which other properties (such as `PercentageRollout` or `Rules`) are relevant during evaluation.

#### `public int? PercentageRollout`
Gets or sets the integer percentage (0-100) of users or requests that should see the feature enabled when `RolloutType` is configured for percentage-based rollout. This value is nullable and ignored if the rollout type does not support percentages.

#### `public DateTime CreatedAt`
Gets the timestamp indicating when the feature flag record was initially created in the system.

#### `public DateTime UpdatedAt`
Gets the timestamp of the most recent modification made to the feature flag's properties.

#### `public string CreatedBy`
Gets the identifier (usually a username or service principal ID) of the entity that created the feature flag.

#### `public string UpdatedBy`
Gets the identifier of the entity that performed the last update to the feature flag.

#### `public ICollection<Rule> Rules`
Gets the collection of evaluation rules associated with this flag. These rules define complex conditions (such as user claims, headers, or context values) that must be met for the flag to evaluate to true.

#### `public ICollection<ABTestVariant> Variants`
Gets the collection of A/B test variants linked to this feature flag. This allows different user segments to experience different implementations of the same feature.

#### `public ICollection<AuditLog> AuditLogs`
Gets the collection of audit log entries recording historical changes to this feature flag, including who made the change and what data was modified.

#### `public bool IsValid`
Gets a boolean value indicating whether the current configuration of the feature flag passes all internal validation checks. This ensures that conflicting settings (e.g., a percentage rollout without a defined percentage) are detected before evaluation.

### Methods

#### `public string GetSnapshot`
Generates and returns a serialized string representation of the current state of the feature flag.
*   **Return Value**: A string containing the snapshot data, typically in JSON or a similar format, suitable for logging, caching, or replication.
*   **Exceptions**: May throw exceptions if the internal state is corrupted or if serialization fails due to invalid data types within nested collections.

## Usage

### Example 1: Inspecting Flag Configuration and Validity
This example demonstrates retrieving a flag and verifying its configuration validity before attempting to use it in a critical path.

```csharp
public void CheckFlagIntegrity(FeatureFlag flag)
{
    if (!flag.IsValid)
    {
        Console.WriteLine($"Configuration error detected for flag '{flag.Key}'.");
        Console.WriteLine($"Rollout Type: {flag.RolloutType}");
        Console.WriteLine($"Percentage: {flag.PercentageRollout?.ToString() ?? "N/A"}");
        return;
    }

    if (flag.IsEnabled && flag.RolloutType == RolloutType.Percentage)
    {
        Console.WriteLine($"Flag '{flag.DisplayName}' is active for {flag.PercentageRollout}% of traffic.");
    }
}
```

### Example 2: Generating an Audit Snapshot
This example illustrates how to capture the current state of a flag for archival purposes prior to performing an update.

```csharp
public void ArchiveFlagState(FeatureFlag flag, string auditorId)
{
    // Capture the current state before modification
    string snapshot = flag.GetSnapshot();
    
    // Log the snapshot along with the current metadata
    Console.WriteLine($"Archiving state for {flag.Key} by {auditorId}");
    Console.WriteLine($"Snapshot Data: {snapshot}");
    Console.WriteLine($"Previous Update By: {flag.UpdatedBy} at {flag.UpdatedAt}");
    
    // In a real scenario, 'snapshot' would be persisted to an external store
}
```

## Notes

*   **Validation Logic**: The `IsValid` property is a computed state based on the consistency of other properties. For instance, if `RolloutType` is set to `Percentage` but `PercentageRollout` is null or out of range, `IsValid` will return `false`. Consumers should always check this property before relying on the flag's evaluation logic.
*   **Collection Mutability**: The properties `Rules`, `Variants`, and `AuditLogs` return `ICollection<T>` interfaces. While the reference to the collection is public, direct modification of these collections outside of controlled service methods may bypass business logic or trigger missing audit events. It is recommended to treat these collections as read-only in standard execution flows unless within a dedicated update transaction.
*   **Thread Safety**: The `FeatureFlag` class appears to be a Plain Old CLR Object (POCO) with mutable properties. It is **not** inherently thread-safe for write operations. If an instance is shared across multiple threads, external synchronization is required when modifying properties like `IsEnabled`, `RolloutType`, or the contents of the collections. Reading properties concurrently is generally safe provided the underlying runtime memory model guarantees atomic reads for the specific data types (which holds for `int`, `bool`, and reference assignments in .NET).
*   **Snapshot Consistency**: The `GetSnapshot` method captures the state at the moment of invocation. If properties are modified immediately before or after this call without locking, the snapshot may represent a transient or inconsistent state relative to the live object.
