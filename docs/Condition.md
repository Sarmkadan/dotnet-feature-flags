# Condition

The `Condition` class represents a logical constraint within the `dotnet-feature-flags` framework. It defines an attribute-based requirement that must be satisfied for an associated feature flag rule to be considered applicable. Each instance encapsulate the configuration for a specific check against a given attribute using a defined operator and expected value.

## API

- `public int Id`
  Unique identifier for the condition instance.

- `public int RuleId`
  Identifier of the `Rule` to which this condition is assigned.

- `public string AttributeName`
  The name of the attribute (e.g., "UserRole", "Region") to be evaluated.

- `public ConditionOperator Operator`
  The operator applied during the evaluation (e.g., `Equals`, `NotEquals`, `GreaterThan`), represented by the `ConditionOperator` enumeration.

- `public string ExpectedValue`
  The target value against which the attribute is compared.

- `public bool IsActive`
  Indicates whether the condition is currently enabled. If `false`, the condition is ignored during evaluation.

- `public DateTime CreatedAt`
  The timestamp indicating when this condition record was created.

- `public Rule? Rule`
  A reference to the associated `Rule` object. This property may be `null` if the association is not loaded.

- `public bool Evaluate`
  A property indicating the current evaluation result of the condition against the applicable context.

- `public bool IsValid`
  Indicates whether the condition is currently configured correctly and is in a valid state for evaluation.

## Usage

### Creating a new Condition
```csharp
var condition = new Condition
{
    AttributeName = "UserLevel",
    Operator = ConditionOperator.Equals,
    ExpectedValue = "Premium",
    IsActive = true,
    CreatedAt = DateTime.UtcNow
};
```

### Evaluating a Condition
```csharp
if (condition.IsActive && condition.IsValid && condition.Evaluate)
{
    // The condition is met, proceed with the feature flag logic.
}
```

## Notes

- **Evaluation Context**: The `Evaluate` property relies on the underlying evaluation engine's access to the current application or request context. Ensure all necessary context is provided to the engine before accessing this property.
- **Thread Safety**: As a data model, `Condition` is not inherently thread-safe. If multiple threads access or modify properties of a `Condition` instance concurrently, external synchronization is required.
- **Nullable Associations**: The `Rule` property is nullable (`Rule?`). Always verify that the `Rule` reference is not null before attempting to access its properties to avoid `NullReferenceException`.
- **Validation**: The `IsValid` property should be checked prior to relying on `Evaluate` to ensure that the condition's configuration (e.g., operator compatibility with the expected value) is correct.
