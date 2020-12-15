# Rule

The `Rule` class represents a conditional activation constraint for a feature flag within the `dotnet-feature-flags` system. It defines specific logic and evaluation criteria, such as priority and activity status, which must be satisfied for the associated `FeatureFlag` to be considered active.

## API

*   **`public int Id`**
    The unique identifier for the rule instance.
*   **`public int FeatureFlagId`**
    The foreign key reference to the associated `FeatureFlag`.
*   **`public string Name`**
    A human-readable label identifying the rule.
*   **`public string Description`**
    A detailed explanation of the rule's intent and evaluation criteria.
*   **`public int Priority`**
    An integer value determining the order of evaluation; higher values typically indicate higher precedence during flag resolution.
*   **`public bool IsActive`**
    A flag indicating whether this rule is currently enabled for evaluation.
*   **`public string ConditionLogic`**
    A serialized or defined string representing the logical composition (e.g., AND/OR) of the underlying conditions.
*   **`public DateTime CreatedAt`**
    The timestamp indicating when the rule was initially created.
*   **`public DateTime UpdatedAt`**
    The timestamp indicating when the rule was last modified.
*   **`public FeatureFlag? FeatureFlag`**
    The navigation property to the parent `FeatureFlag` entity, which may be null if not loaded.
*   **`public ICollection<Condition> Conditions`**
    The collection of `Condition` entities that constitute the evaluation logic of this rule.
*   **`public bool IsValid`**
    Computes whether the rule's current configuration and associated conditions meet validation requirements.
*   **`public int GetActiveConditionCount`**
    Returns the total count of enabled `Condition` entities currently associated with this rule.
*   **`public int GetEvaluationPriority`**
    Calculates the effective priority of the rule, taking into account both the `Priority` property and any inherited or contextual constraints.

## Usage

```csharp
// Example 1: Creating and associating a new rule
var newRule = new Rule
{
    Name = "Beta Users Only",
    Description = "Enables feature for users in the beta group.",
    Priority = 10,
    IsActive = true,
    ConditionLogic = "ALL",
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = DateTime.UtcNow
};
featureFlag.Rules.Add(newRule);
```

```csharp
// Example 2: Checking rule validity and active condition count
if (rule.IsValid)
{
    int activeCount = rule.GetActiveConditionCount();
    Console.WriteLine($"Rule '{rule.Name}' has {activeCount} active conditions.");
}
```

## Notes

*   **Thread-Safety:** This class is not inherently thread-safe. If rules are modified in a multi-threaded environment (e.g., during concurrent evaluation and updates), external locking mechanisms must be employed to ensure consistency, particularly when accessing the `Conditions` collection.
*   **Validation:** The `IsValid` property depends on the state of the `Conditions` collection. If the collection is modified, subsequent calls to `IsValid` will reflect the updated state. Ensure `Conditions` are fully loaded if using Entity Framework to avoid false validation results.
*   **Navigation Properties:** The `FeatureFlag` property may be null if the object graph was not fully materialized from the persistence layer. Always perform null checks before accessing `FeatureFlag` members.
