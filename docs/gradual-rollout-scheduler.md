# Gradual Rollout Scheduler Service

## Overview

The `GradualRolloutSchedulerService` is responsible for managing time-based gradual rollouts of feature flags. It processes active rollout strategies, advances percentage allocations according to configured schedules, and provides status information about ongoing rollouts.

## Key Responsibilities

1. **Process Scheduled Rollouts**: Automatically advances feature flag percentages based on time-based schedules
2. **Get Rollout Status**: Retrieves current status and progress of gradual rollouts for specific feature flags
3. **Manual Rollout Advancement**: Allows manual advancement of a feature flag's rollout percentage
4. **Audit Logging**: Records all rollout changes for tracking and compliance

## How It Works

The service operates by:

1. Fetching all active gradual rollout strategies from the database that have a start date configured
2. For each strategy, checking if it's currently active (within start/end date range)
3. Calculating the current rollout percentage based on:
   - Start date
   - End date (optional)
   - Daily increment value
   - Current date/time
4. If the calculated percentage differs from the feature flag's current percentage:
   - Updates the feature flag's `PercentageRollout` property
   - Sets `UpdatedAt` and `UpdatedBy` fields
   - Saves changes to the database
   - Creates an audit log entry recording the change

## Core Methods

### ProcessScheduledRolloutsAsync

```csharp
public async Task<int> ProcessScheduledRolloutsAsync(CancellationToken cancellationToken = default)
```

The main worker method that processes all active gradual rollout strategies. It:
- Logs the start of processing
- Retrieves all gradual strategies with start dates
- For each strategy, checks if active and applies rollout progress
- Returns the count of feature flags that were updated
- Handles exceptions per strategy without stopping the entire process

### GetScheduleStatusAsync

```csharp
public async Task<RolloutScheduleStatus?> GetScheduleStatusAsync(int featureFlagId, CancellationToken cancellationToken = default)
```

Retrieves detailed schedule status for a specific feature flag, including:
- Current and target percentages
- Daily increment value
- Start and end dates
- Whether the rollout is active and complete
- Estimated days remaining until completion

### AdvanceRolloutAsync

```csharp
public async Task<bool> AdvanceRolloutAsync(int featureFlagId, string advancedBy, CancellationToken cancellationToken = default)
```

Manually advances the rollout for a feature flag by:
- Validating input parameters
- Finding the gradual rollout strategy for the flag
- Applying the current computed percentage (same logic as the scheduler)
- Returning true if the rollout was advanced, false otherwise

### ApplyRolloutProgressAsync (Private)

```csharp
private async Task<bool> ApplyRolloutProgressAsync(RolloutStrategy strategy, string modifiedBy, CancellationToken cancellationToken)
```

Core logic that:
- Computes the current percentage using the strategy's `GetCurrentPercentage()` method
- Compares with the feature flag's current percentage
- If different, updates the flag and saves changes
- Creates an audit log entry
- Returns true if an update occurred

### RecordAuditAsync (Private)

```csharp
private async Task RecordAuditAsync(FeatureFlag flag, string changedBy, string oldSnapshot)
```

Creates an audit log entry for rollout changes, capturing:
- Feature flag ID
- Action type (RolloutChanged)
- Who made the change
- Timestamp
- Old and new flag state snapshots
- Description of the change

## Dependencies

- `FeatureFlagDbContext`: Entity Framework Core database context
- `IAuditLogRepository`: Repository for audit log persistence
- `ILogger<GradualRolloutSchedulerService>: Logger for service operations

## Usage Notes

### Scheduling

The service is designed to be run periodically (e.g., via a timer, cron job, or background worker service). Each execution processes all active gradual rollouts and advances them according to their schedules.

### Concurrency

The method accepts a `CancellationToken` to support graceful shutdown and prevent processing during application termination.

### Error Handling

- Individual strategy failures are logged but don't stop processing of other strategies
- Audit logging failures are logged but don't affect the rollout update
- Null parameter checks are performed on all public methods

### Data Flow

1. Scheduler fetches strategies from database
2. For each strategy:
   - Checks if active (`IsActive()` method)
   - Computes current percentage (`GetCurrentPercentage()` method)
   - Compares with feature flag's current `PercentageRollout`
   - If different, updates flag and saves to database
   - Creates audit log entry

## Related Components

- `RolloutStrategy`: Defines the gradual rollout configuration (start/end dates, daily increment, etc.)
- `FeatureFlag`: Represents the feature flag being rolled out
- `AuditLog`: Tracks all changes made to feature flags
- `IGradualRolloutSchedulerService`: Interface that this service implements

## Example Usage

```csharp
// In a background worker or timer service
public async Task DoWork(CancellationToken stoppingToken)
{
    var updated = await _gradualRolloutScheduler.ProcessScheduledRolloutsAsync(stoppingToken);
    _logger.LogInformation("Updated {Count} feature flags via gradual rollout scheduler", updated);
}

// Manual advancement
await _gradualRolloutScheduler.AdvanceRolloutAsync(123, "admin-user", CancellationToken.None);

// Get status
var status = await _gradualRolloutScheduler.GetScheduleStatusAsync(123, CancellationToken.None);
if (status != null)
{
    Console.WriteLine($"Rollout for {status.FeatureFlagKey}: {status.CurrentPercentage}% → {status.TargetPercentage}%");
}
```