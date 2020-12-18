# RuleEvaluationService

The `RuleEvaluationService` implements `IRuleEvaluationService` and provides the core logic for evaluating feature‑flag rules within the dotnet‑feature‑flags library. It resolves which rules apply to a given flag and evaluation context, evaluates individual conditions, and ultimately determines whether a flag should be enabled for a specific request.

## API

### Constructor
```csharp
public RuleEvaluationService(IRuleRepository ruleRepository, IConditionEvaluator conditionEvaluator)
```
* **Purpose** – Creates a new instance capable of evaluating rules.  
* **Parameters** –  
  * `ruleRepository`: Supplies the collection of `Rule` objects associated with feature flags.  
  * `conditionEvaluator`: Provides the logic to evaluate individual `Condition` instances.  
* **Return value** – A fully initialized `RuleEvaluationService` object.  
* **Exceptions** – Throws `ArgumentNullException` if either `ruleRepository` or `conditionEvaluator` is `null`.

### EvaluateAsync
```csharp
public async Task<bool> EvaluateAsync(string flagKey, EvaluationContext context)
```
* **Purpose** – Determines whether the feature flag identified by `flagKey` is enabled for the supplied `context`.  
* **Parameters** –  
  * `flagKey`: The unique key of the feature flag to evaluate.  
  * `context`: Contains tenant, user, and request‑specific data used during rule matching.  
* **Return value** – `true` if the flag evaluates to enabled; otherwise `false`.  
* **Exceptions** –  
  * `ArgumentNullException` if `flagKey` or `context` is `null`.  
  * `InvalidOperationException` if the flag definition cannot be found in the repository.  
  * Any exception thrown by the underlying `IConditionEvaluator` is propagated.

### EvaluateRuleAsync
```csharp
public async Task<bool> EvaluateRuleAsync(string flagKey, Rule rule, EvaluationContext context)
```
* **Purpose** – Evaluates a single `Rule` against the given `context` to decide if the rule matches.  
* **Parameters** –  
  * `flagKey`: The flag the rule belongs to (used for logging and error context).  
  * `rule`: The rule to evaluate.  
  * `context`: Evaluation data supplied by the caller.  
* **Return value** – `true` if the rule matches the context; otherwise `false`.  
* **Exceptions** –  
  * `ArgumentNullException` if any parameter is `null`.  
  * Propagates exceptions from `EvaluateCondition` or condition evaluators.

### EvaluateCondition
```csharp
public bool EvaluateCondition(Condition condition, EvaluationContext context)
```
* **Purpose** – Synchronously evaluates a single `Condition` (e.g., a comparison, segment membership, or custom predicate) against the supplied context.  
* **Parameters** –  
  * `condition`: The condition to evaluate.  
  * `context`: The evaluation context providing attribute values.  
* **Return value** – `true` if the condition is satisfied; otherwise `false`.  
* **Exceptions** –  
  * `ArgumentNullException` if `condition` or `context` is `null`.  
  * Throws `NotSupportedException` for unsupported condition types.  
  * Any exception from a custom condition evaluator is bubbled up.

### GetApplicableRulesAsync
```csharp
public async Task<IEnumerable<Rule>> GetApplicableRulesAsync(string flagKey, EvaluationContext context)
```
* **Purpose** – Retrieves all `Rule` objects associated with `flagKey` that could potentially match the given `context` (i.e., rules whose prerequisites such as rollout percentage or segment filters are satisfied).  
* **Parameters** –  
  * `flagKey`: The flag whose rules are being queried.  
  * `context`: Evaluation context used to filter rules (e.g., for segment‑based prerequisites).  
* **Return value** – An enumerable of `Rule` instances that are applicable; may be empty if no rules match.  
* **Exceptions** –  
  * `ArgumentNullException` if `flagKey` or `context` is `null`.  
  * `InvalidOperationException` if the flag definition is missing.  

## Usage

### Basic flag evaluation
```csharp
// Assuming services are registered with the DI container
var ruleEvalService = serviceProvider.GetRequiredService<RuleEvaluationService>();

var ctx = new EvaluationContext
{
    UserId = "user-123",
    Attributes = { { "role", "admin" }, { "region", "eu-west" } }
};

bool isEnabled = await ruleEvalService.EvaluateAsync("new-ui-feature", ctx);
if (isEnabled)
{
    // Enable the new UI for this request
}
```

### Retrieving and inspecting applicable rules
```csharp
var ruleEvalService = serviceProvider.GetRequiredService<RuleEvaluationService>();

var ctx = new EvaluationContext
{
    UserId = "user-456",
    Attributes = { { "tier", "premium" } }
};

IEnumerable<Rule> applicable = await ruleEvalService.GetApplicableRulesAsync("beta-feature", ctx);
foreach (var rule in applicable)
{
    Console.WriteLine($"Rule {rule.Id} (priority {rule.Priority}) is applicable.");
    // Optionally evaluate the rule individually
    bool matches = await ruleEvalService.EvaluateRuleAsync("beta-feature", rule, ctx);
    Console.WriteLine($"  Matches: {matches}");
}
```

## Notes
* The service is designed to be **stateless**; all mutable data is supplied via method parameters. Consequently, instances are safe to use concurrently by multiple threads, provided that the injected dependencies (`IRuleRepository` and `IConditionEvaluator`) are themselves thread‑safe.  
* `EvaluateCondition` is synchronous and should perform only lightweight operations (e.g., simple comparisons or lookups). Expensive work should be avoided here to prevent blocking callers that invoke the asynchronous evaluation methods.  
* If a flag has no associated rules, `GetApplicableRulesAsync` returns an empty enumerable and `EvaluateAsync` will return `false` (assuming the default disabled state).  
* Exceptions related to missing flag definitions (`InvalidOperationException`) indicate a configuration problem; callers may choose to treat such flags as disabled or to log and escalate the issue depending on application policy.  
* The service does **not** cache rule evaluation results across calls; each invocation performs fresh resolution based on the supplied `EvaluationContext`. If caching is desired, it should be implemented at a higher layer (e.g., in a feature‑flag middleware).
