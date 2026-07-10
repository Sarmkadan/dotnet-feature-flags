# IGradualRolloutSchedulerService

The `IGradualRolloutSchedulerService` interface defines the contract for managing the state and progression of a gradual feature flag rollout. It exposes properties that track the current rollout percentage, target goals, temporal constraints, and calculated metrics such as estimated completion time, enabling monitoring and control systems to adjust feature availability incrementally over a defined period.

## API

### FeatureFlagId
```csharp
public int FeatureFlagId { get; }
```
Gets the unique numeric identifier associated with the feature flag being rolled out. This value is immutable for the lifetime of the scheduler instance and is used for database lookups and logging correlation.

### FeatureFlagKey
```csharp
public string FeatureFlagKey { get; }
```
Gets the human-readable string key of the feature flag. This key corresponds to the identifier used in application code to check feature status and is immutable.

### CurrentPercentage
```csharp
public int CurrentPercentage { get; }
```
Gets the current rollout percentage, representing the proportion of users or traffic currently exposed to the feature. The value ranges from 0 to 100. This property reflects the state at the last calculation cycle.

### TargetPercentage
```csharp
public int TargetPercentage { get; }
```
Gets the intended final rollout percentage. Once `CurrentPercentage` reaches this value, the rollout is considered logically complete regarding exposure, though the scheduler may remain active until the end date.

### DailyIncrement
```csharp
public int? DailyIncrement { get; }
```
Gets the configured percentage points to add to the `CurrentPercentage` every 24 hours. If `null`, the rollout does not follow an automatic linear progression schedule, and percentage updates must be managed externally.

### StartDate
```csharp
public DateTime? StartDate { get; }
```
Gets the scheduled start time for the rollout progression. If the current time is before this value, the `IsActive` property typically returns `false`, and no increment calculations occur. If `null`, the rollout is considered to have started immediately upon creation.

### EndDate
```csharp
public DateTime? EndDate { get; }
```
Gets the scheduled termination time for the rollout process. If the current time exceeds this value, the scheduler stops incrementing, and `IsActive` typically returns `false`. If `null`, the rollout continues until `TargetPercentage` is reached without a hard time cutoff.

### IsActive
```csharp
public bool IsActive { get; }
```
Gets a value indicating whether the rollout schedule is currently active. This returns `true` only if the current time is within the `StartDate` and `EndDate` window (if defined) and the `TargetPercentage` has not yet been reached.

### IsComplete
```csharp
public bool IsComplete { get; }
```
Gets a value indicating whether the rollout has finished. This returns `true` if `CurrentPercentage` is greater than or equal to `TargetPercentage`, or if the `EndDate` has passed.

### EstimatedDaysRemaining
```csharp
public int EstimatedDaysRemaining { get; }
```
Gets the calculated number of days remaining until the `TargetPercentage` is reached, based on the `DailyIncrement` rate. If `DailyIncrement` is null, zero, or if the rollout is already complete, this property returns 0.

## Usage

### Monitoring Rollout Progress
The following example demonstrates how to inspect the current state of a rollout to determine if manual intervention is required or if the process is proceeding as expected.

```csharp
public void MonitorRollout(IGradualRolloutSchedulerService scheduler)
{
    if (!scheduler.IsActive)
    {
        if (scheduler.IsComplete)
        {
            Console.WriteLine($"Rollout for '{scheduler.FeatureFlagKey}' is complete.");
        }
        else
        {
            Console.WriteLine($"Rollout for '{scheduler.FeatureFlagKey}' is not yet active or has ended prematurely.");
        }
        return;
    }

    Console.WriteLine($"Feature: {scheduler.FeatureFlagKey} (ID: {scheduler.FeatureFlagId})");
    Console.WriteLine($"Progress: {scheduler.CurrentPercentage}% / {scheduler.TargetPercentage}%");
    Console.WriteLine($"Estimated days remaining: {scheduler.EstimatedDaysRemaining}");

    if (scheduler.DailyIncrement.HasValue && scheduler.DailyIncrement.Value == 0)
    {
        Console.WriteLine("Warning: Daily increment is zero; rollout will not progress automatically.");
    }
}
```

### Validating Configuration Before Activation
This example shows how to validate the temporal and incremental configuration of a scheduler before allowing it to be persisted or activated in a production environment.

```csharp
public bool ValidateRolloutConfiguration(IGradualRolloutSchedulerService scheduler)
{
    if (scheduler.TargetPercentage < scheduler.CurrentPercentage)
    {
        throw new InvalidOperationException("Target percentage cannot be less than the current percentage.");
    }

    if (scheduler.StartDate.HasValue && scheduler.EndDate.HasValue)
    {
        if (scheduler.EndDate.Value <= scheduler.StartDate.Value)
        {
            return false; // Invalid time window
        }
    }

    if (scheduler.DailyIncrement.HasValue && scheduler.DailyIncrement.Value <= 0)
    {
        if (scheduler.CurrentPercentage < scheduler.TargetPercentage)
        {
            // Non-zero increment required for automatic progression
            return false; 
        }
    }

    return true;
}
```

## Notes

*   **Immutability**: All members exposed by this interface are read-only properties. The state of the rollout is expected to be managed by a concrete implementation that updates these values internally or via a separate mutation service not exposed through this interface.
*   **Null Handling**: `DailyIncrement`, `StartDate`, and `EndDate` are nullable. Consumers must check for `HasValue` before performing arithmetic operations or date comparisons to avoid `InvalidOperationException`.
*   **Calculation Logic**: The `EstimatedDaysRemaining` property is a derived value. If `DailyIncrement` is null or 0, the result is 0 regardless of the difference between current and target percentages. Implementations should handle division by zero internally.
*   **Thread Safety**: As this interface exposes only data accessors, thread safety depends entirely on the underlying implementation. If the concrete service updates state concurrently, callers should expect that reading multiple properties (e.g., `CurrentPercentage` and `EstimatedDaysRemaining`) may not represent an atomic snapshot of a single point in time.
*   **State Consistency**: The `IsActive` and `IsComplete` flags are derived from the relationship between the current time, dates, and percentages. In high-latency scenarios, there may be a brief window where `IsActive` is true but `CurrentPercentage` has not yet been updated to reflect the new day's increment.
