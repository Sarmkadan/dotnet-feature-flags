# RolloutStrategy

Represents a rollout configuration for a feature flag, defining how and when the flag’s enabled percentage changes over time. It stores scheduling details, gradual rollout parameters, and provides helper methods to evaluate the current state of the rollout.

## API

| Member | Type | Purpose | Parameters | Return Value | Throws |
|--------|------|---------|------------|--------------|--------|
| `Id` | `int` | Primary key identifier for the rollout record. | – | The unique identifier. | None |
| `FeatureFlagId` | `Id` | Foreign key linking the feature flag. | – | The identifier of the associated feature flag. | None |
| `Type` | `RolloutType` | Enum indicating the rollout strategy (e.g., instant, gradual, scheduled). | Foreign key referencing the associated feature flag. | – | The ID of the feature flag this rollout belongs to. | None |
| `Type` | `RolloutType` | Specifies the rollout mode (instant, gradual, scheduled, etc.). | – | The current rollout type. | None |
| `StartPercentage` | `int?` | Initial percentage of users for whom the flag is enabled (0‑100). Nullable to allow omission when not applicable. | – | The start percentage, or `null` if not set. | None |
| `EndPercentage` | `int?` | Target percentage of users for whom the flag is enabled (0‑100). Nullable when the rollout does not define an end target. | – | The end percentage, or `null` if not set. | None |
| `StartDate` | `DateTime?` | Date and time when the rollout begins. Nullable for rollouts that start immediately. | – | The start timestamp, or `null` if not scheduled. | None |
| `EndDate` | `DateTime?` | Date and time when the rollout ends. Nullable for open‑ended rollouts. | – | The end timestamp, or `null` if not scheduled. | None |
| `IsGradual | Indicates whether the rollout changes the enabled. | – | `bool` | Flag indicating whether the rollout uses a gradual increment over time. | – | `true` if the rollout changes percentage daily; otherwise `false`. | None |
| `DailyIncrement` | `int?` | Amount (in percentage points) to increase the enabled users each day for a gradual rollout. Nullable when `IsGradual` is `false`. | – | The daily increment value, or `null`. | None |
| `CreatedAt` | `DateTime` | Timestamp when the rollout record was created. | – | The creation date/time (UTC). | None |
| `UpdatedAt` | `DateTime` | Timestamp when the rollout record was last modified. | – | The last update date/time (UTC). | None |
| `FeatureFlag` | `FeatureFlag?` | Navigation property to the related feature flag entity. May be `null` if not loaded. | – | The associated `FeatureFlag` instance, or `null`. | None |
| `GetCurrentPercentage` | `int` | Calculates the effective enabled percentage for the current moment based on the rollout configuration. | – | An integer between 0 and 100 representing the current rollout percentage. | Throws `InvalidOperationException` if the rollout data is inconsistent (e.g., missing required percentages or dates). |
| `IsActive` | `bool` | Determines whether the rollout is presently active (i.e., the current time falls within the scheduled window and the rollout has not completed). | – | `true` if the rollout should be applying its percentage; otherwise `false`. | None |
| `IsValid` | `bool` | Validates the rollout configuration (percentage bounds, date ordering, gradual increment consistency). | – | `true` if all fields satisfy business rules; otherwise `false`. | None |
| `GetRemainingDays` | `int` | Returns the number of whole days left until the rollout ends or reaches its target percentage. For open‑ended rollouts returns `-1`. | – | Days remaining as a non‑negative integer, or `-1` if no end date is defined. | Throws `InvalidOperationException` if the rollout lacks sufficient data to compute a remainder (e.g., missing `EndDate` when `EndPercentage` is set). |

## Usage

### Example 1: Creating and evaluating a gradual rollout

```csharp
using System;
using DotNetFeatureFlags.Models; // Adjust namespace as needed

var rollout = new RolloutStrategy
{
    FeatureFlagId = 42,
    Type = RolloutType.Gradual,
    StartPercentage = 10,
    EndPercentage = 90,
    StartDate = DateTime.UtcNow.AddDays(-2),
    EndDate   = DateTime.UtcNow.AddDays(8),
    IsGradual   = true,
    DailyIncrement = 10,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};

if (rollout.IsValid)
{
    Console.WriteLine($"Rollout is active: {rollout.IsActive}");
    Console.WriteLine($"Current percentage: {rollout.GetCurrentPercentage}%");
    Console.WriteLine($"Days remaining: {rollout.GetRemainingDays}");
}
else
{
    Console.WriteLine("Rollout configuration is invalid.");
}
```

### Example 2: Checking an instant rollout with no schedule

```csharp
using System;
using DotNetFeatureFlags.Models;

var instantRollout = new RolloutStrategy
{
    FeatureFlagId = 7,
    Type = RolloutType.Instant,
    StartPercentage = 100,
    EndPercentage   = null,
    StartDate       = null,
    EndDate         = null,
    IsGradual       = false,
    DailyIncrement  = null,
    CreatedAt       = DateTime.UtcNow,
    UpdatedAt       = DateTime.UtcNow
};

Console.WriteLine($"Is active: {instantRollout.IsActive}"); // true if StartPercentage > 0
Console.WriteLine($"Current percentage: {instantRollout.GetCurrentPercentage}%");
```

## Notes

- **Percentage validation**: `StartPercentage` and `EndPercentage` must be between 0 and 100 when not null. `GetCurrentPercentage` will throw if these values are out of range or if a gradual rollout lacks a `DailyIncrement`.
- **Date ordering**: When both `StartDate` and `EndDate` are supplied, `StartDate` must precede `EndDate`. `IsValid` returns `false` otherwise, and `GetRemainingDays` may throw if the dates are inconsistent.
- **Gradual rollout logic**: If `IsGradual` is `true`, `DailyIncrement` must have a positive value; the system assumes the percentage changes each day by that amount, clamped between `StartPercentage` and `EndPercentage`.
- **Thread safety**: The type contains mutable properties and relies on `DateTime.UtcNow` for time‑dependent calculations. Concurrent modification of its fields from multiple threads without external synchronization can lead to race conditions and inconsistent state. Read‑only access after construction is safe, but any mutation should be guarded by locks or performed on a single thread.
- **Navigation property**: `FeatureFlag` may be `null` if the entity is not eagerly loaded; accessing its members without checking for null will cause a `NullReferenceException`.
- **Open‑ended rollouts**: Leaving `EndDate` null indicates the rollout does not have a scheduled end; `GetRemainingDays` returns `-1` in this case, and `IsActive` depends solely on `StartDate` and the percentage logic.
