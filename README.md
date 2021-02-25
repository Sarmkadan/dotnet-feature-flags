## RolloutStrategyExtensions

The `RolloutStrategyExtensions` class provides a set of extension methods for the `RolloutStrategy` type that enable common operations for filtering, metadata access, and event manipulation. These methods allow you to easily check event types, access metadata values, create modified copies of events, and format events for logging purposes.

Below is a realistic usage example demonstrating the most commonly used extension methods:

```csharp
// Create a rollout strategy
var strategy = new RolloutStrategy
{
  StartPercentage = 5,
  EndPercentage = 100,
  DailyIncrementPercentage = 10,
  StartDate = DateTime.UtcNow,
  EndDate = DateTime.UtcNow.AddDays(10)
};

// Check if strategy is percentage-based
bool isPercentageBased = strategy.IsPercentageBased();

// Check if strategy is rules-based
bool isRulesBased = strategy.IsRulesBased();

// Check if strategy is an A/B test
bool isABTest = strategy.IsABTest();

// Check if strategy is a full rollout
bool isFullRollout = strategy.IsFullRollout();

// Check if strategy is no rollout
bool isNoRollout = strategy.IsNoRollout();

// Get the effective percentage for the strategy
int effectivePercentage = strategy.GetEffectivePercentage();

// Get the progress percentage for the strategy
int progressPercentage = strategy.GetProgressPercentage();

// Check if the strategy has reached its target percentage
bool hasReachedTarget = strategy.HasReachedTarget();

// Get a human-readable description of the rollout strategy type
string description = strategy.GetDescription();
```

## AuditLogExtensions

The `AuditLogExtensions` class provides a set of extension methods for the `AuditLog` type that enable common operations for analyzing audit trail entries. These methods allow you to determine the type of change, calculate time since changes, generate human-readable descriptions, and format actions for display purposes.

Below is a realistic usage example demonstrating the most commonly used extension methods:

```csharp
// Create an audit log entry
var log = new AuditLog
{
    Action = AuditAction.Updated,
    ChangedAt = DateTime.UtcNow.AddMinutes(-45),
    Summary = "Feature flag updated",
    ChangeDetails = "Enabled state changed from false to true"
};

// Check if this represents a state change
bool isStateChange = log.IsStateChange();

// Check if this represents a creation event
bool isCreation = log.IsCreation();

// Check if this represents a deletion event
bool isDeletion = log.IsDeletion();

// Get the time since this change was made
string timeSinceChange = log.GetTimeSinceChange();

// Get a detailed change description
string detailedDescription = log.GetDetailedChangeDescription();

// Check if this change is recent (within 30 minutes)
bool isRecent = log.IsRecent();

// Get a simplified action name for UI display
string actionDisplayName = log.GetActionDisplayName();
```