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