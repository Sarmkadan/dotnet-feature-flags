# RolloutStrategyExtensions

Provides extension methods for inspecting and evaluating `RolloutStrategy` instances, enabling callers to determine rollout type, calculate effective percentages, assess progress toward targets, and obtain human-readable descriptions without directly accessing internal strategy state.

## API

### IsPercentageBased
```csharp
public static bool IsPercentageBased(this RolloutStrategy strategy)
```
Returns `true` when the strategy distributes features based on a percentage allocation; otherwise `false`.

**Parameters**
- `strategy` – The rollout strategy to inspect.

**Returns**
- `true` if the strategy is percentage-based; `false` for rule-based, A/B test, full, or no rollout strategies.

**Exceptions**
- `ArgumentNullException` – Thrown if `strategy` is `null`.

---

### IsRulesBased
```csharp
public static bool IsRulesBased(this RolloutStrategy strategy)
```
Returns `true` when the strategy evaluates a collection of targeting rules to determine feature eligibility; otherwise `false`.

**Parameters**
- `strategy` – The rollout strategy to inspect.

**Returns**
- `true` if the strategy is rules-based; `false` otherwise.

**Exceptions**
- `ArgumentNullException` – Thrown if `strategy` is `null`.

---

### IsABTest
```csharp
public static bool IsABTest(this RolloutStrategy strategy)
```
Returns `true` when the strategy represents an A/B test configuration with distinct variant allocations; otherwise `false`.

**Parameters**
- `strategy` – The rollout strategy to inspect.

**Returns**
- `true` if the strategy is an A/B test; `false` otherwise.

**Exceptions**
- `ArgumentNullException` – Thrown if `strategy` is `null`.

---

### IsFullRollout
```csharp
public static bool IsFullRollout(this RolloutStrategy strategy)
```
Returns `true` when the strategy enables the feature for all contexts (100% rollout); otherwise `false`.

**Parameters**
- `strategy` – The rollout strategy to inspect.

**Returns**
- `true` if the strategy is a full rollout; `false` otherwise.

**Exceptions**
- `ArgumentNullException` – Thrown if `strategy` is `null`.

---

### IsNoRollout
```csharp
public static bool IsNoRollout(this RolloutStrategy strategy)
```
Returns `true` when the strategy disables the feature for all contexts (0% rollout); otherwise `false`.

**Parameters**
- `strategy` – The rollout strategy to inspect.

**Returns**
- `true` if the strategy is a no rollout; `false` otherwise.

**Exceptions**
- `ArgumentNullException` – Thrown if `strategy` is `null`.

---

### GetEffectivePercentage
```csharp
public static int GetEffectivePercentage(this RolloutStrategy strategy)
```
Calculates the effective rollout percentage for the strategy. For percentage-based strategies returns the configured percentage; for full rollout returns 100; for no rollout returns 0; for rules-based or A/B test strategies returns the aggregate percentage of contexts expected to receive the feature.

**Parameters**
- `strategy` – The rollout strategy to evaluate.

**Returns**
- An integer between 0 and 100 inclusive representing the effective rollout percentage.

**Exceptions**
- `ArgumentNullException` – Thrown if `strategy` is `null`.
- `InvalidOperationException` – Thrown if the strategy type cannot be resolved to a percentage (e.g., malformed rules-based configuration).

---

### GetProgressPercentage
```csharp
public static int GetProgressPercentage(this RolloutStrategy strategy)
```
Returns the current progress toward the strategy's target rollout percentage. For strategies with a defined target (percentage-based, A/B test), returns the ratio of current allocation to target allocation expressed as a percentage. For full/no rollout strategies, returns 100 or 0 respectively. For rules-based strategies without a numeric target, returns 0.

**Parameters**
- `strategy` – The rollout strategy to evaluate.

**Returns**
- An integer between 0 and 100 inclusive representing progress toward the target.

**Exceptions**
- `ArgumentNullException` – Thrown if `strategy` is `null`.

---

### HasReachedTarget
```csharp
public static bool HasReachedTarget(this RolloutStrategy strategy)
```
Determines whether the strategy's current allocation has met or exceeded its target percentage. Returns `true` for full rollout, `false` for no rollout, and compares current vs. target for percentage-based and A/B test strategies. Always returns `false` for rules-based strategies lacking a numeric target.

**Parameters**
- `strategy` – The rollout strategy to evaluate.

**Returns**
- `true` if the target has been reached; otherwise `false`.

**Exceptions**
- `ArgumentNullException` – Thrown if `strategy` is `null`.

---

### GetDescription
```csharp
public static string GetDescription(this RolloutStrategy strategy)
```
Generates a human-readable summary of the strategy, including its type, key parameters (percentage, variant counts, rule count), and current progress where applicable.

**Parameters**
- `strategy` – The rollout strategy to describe.

**Returns**
- A non-empty string suitable for logging or UI display.

**Exceptions**
- `ArgumentNullException` – Thrown if `strategy` is `null`.

## Usage

### Example 1: Guarding feature activation based on rollout progress
```csharp
var strategy = featureFlag.RolloutStrategy;

if (strategy.IsNoRollout())
{
    logger.LogInformation("Feature {Feature} is disabled globally", featureFlag.Name);
    return false;
}

if (!strategy.HasReachedTarget())
{
    var progress = strategy.GetProgressPercentage();
    logger.LogDebug("Feature {Feature} rollout at {Progress}% (target not reached)", featureFlag.Name, progress);
}

return strategy.IsPercentageBased() 
    ? EvaluatePercentage(context, strategy.GetEffectivePercentage())
    : EvaluateRules(context, strategy);
```

### Example 2: Building a rollout status dashboard view model
```csharp
public RolloutStatusViewModel BuildStatus(FeatureFlag flag)
{
    var strategy = flag.RolloutStrategy;
    
    return new RolloutStatusViewModel
    {
        FeatureName = flag.Name,
        StrategyType = strategy.IsABTest() ? "A/B Test"
                        : strategy.IsRulesBased() ? "Rules"
                        : strategy.IsPercentageBased() ? "Percentage"
                        : strategy.IsFullRollout() ? "Full"
                        : "None",
        EffectivePercentage = strategy.GetEffectivePercentage(),
        ProgressPercentage = strategy.GetProgressPercentage(),
        HasReachedTarget = strategy.HasReachedTarget(),
        Description = strategy.GetDescription()
    };
}
```

## Notes

- All methods throw `ArgumentNullException` if the `strategy` parameter is `null`; callers should validate before invoking when the source is untrusted.
- `GetEffectivePercentage` may throw `InvalidOperationException` for malformed rules-based strategies; wrap calls when strategy integrity cannot be guaranteed.
- The extension methods are pure functions with no side effects and no shared mutable state, making them inherently thread-safe for concurrent use across multiple threads.
- `GetProgressPercentage` and `HasReachedTarget` rely on the strategy's internal `CurrentPercentage` and `TargetPercentage` properties (where applicable). For rules-based strategies without explicit numeric targets, these members return baseline values (0 and `false` respectively).
- `GetDescription` output format is implementation-defined and may change between versions; avoid parsing it programmatically.
