# RuleEvaluationServiceTestsExtensions

Utility class providing factory methods and assertion helpers for constructing and verifying rule evaluation scenarios in unit tests. It simplifies the creation of `UserContext`, `Condition`, and `Rule` instances, and offers synchronous and asynchronous assertion methods to validate evaluation outcomes against expected results.

## API

### CreateUserContext

```csharp
public static UserContext CreateUserContext(...)
```

Creates a `UserContext` instance configured for use in rule evaluation tests. The exact parameters are determined by the `UserContext` constructor overloads available in the project, typically including user attributes such as identifiers, roles, or custom properties that conditions may reference.

- **Returns:** A fully initialized `UserContext` object.
- **Throws:** May throw `ArgumentNullException` or `ArgumentException` if required parameters are null or invalid, depending on the underlying `UserContext` constructor.

### CreateCondition

```csharp
public static Condition CreateCondition(...)
```

Constructs a `Condition` object representing a single evaluable criterion within a rule. Parameters typically specify the condition type, target property, operator, and expected value.

- **Returns:** A `Condition` instance ready for inclusion in a `Rule`.
- **Throws:** May throw `ArgumentException` if the condition definition is malformed or unsupported.

### CreateRule

```csharp
public static Rule CreateRule(...)
```

Builds a `Rule` instance composed of one or more conditions and an evaluation strategy (e.g., `All` or `Any`). Accepts conditions and optional metadata such as rule name or enabled status.

- **Returns:** A `Rule` object that can be passed to `IRuleEvaluationService` methods.
- **Throws:** May throw `ArgumentNullException` if the conditions collection is null, or `ArgumentException` if the rule configuration is invalid.

### CreateSingleConditionRule

```csharp
public static Rule CreateSingleConditionRule(...)
```

Convenience method that creates a `Rule` containing exactly one condition. Useful for isolating the behavior of a single condition during testing.

- **Returns:** A `Rule` with a single condition and a default evaluation strategy (typically `All`).
- **Throws:** Propagates any exceptions from the underlying `CreateCondition` or `CreateRule` calls.

### AssertConditionResult

```csharp
public static void AssertConditionResult(...)
```

Evaluates a condition against a given `UserContext` synchronously and asserts that the result matches the expected boolean outcome. Internally invokes the evaluation service and performs an equality assertion.

- **Parameters:**
  - `condition`: The `Condition` to evaluate.
  - `userContext`: The `UserContext` against which the condition is tested.
  - `expectedResult`: The expected boolean result (`true` or `false`).
- **Throws:** Throws an assertion exception (e.g., via NUnit or xUnit) if the actual result differs from `expectedResult`. May throw `InvalidOperationException` if the evaluation service is unavailable.

### AssertRuleResultAsync

```csharp
public static async Task AssertRuleResultAsync(...)
```

Asynchronously evaluates a `Rule` against a `UserContext` and asserts that the result matches the expected boolean outcome. Designed for rules that may involve asynchronous condition evaluation.

- **Parameters:**
  - `rule`: The `Rule` to evaluate.
  - `userContext`: The `UserContext` against which the rule is tested.
  - `expectedResult`: The expected boolean result (`true` or `false`).
- **Returns:** A `Task` representing the asynchronous assertion operation.
- **Throws:** Throws an assertion exception if the actual result differs from `expectedResult`. May throw `InvalidOperationException` if the evaluation service is unavailable, or `TimeoutException` if the async evaluation exceeds a configured timeout.

## Usage

### Example 1: Testing a Single Condition Synchronously

```csharp
[Test]
public void UserFromMarketingDepartment_SatisfiesDepartmentCondition()
{
    // Arrange
    var user = RuleEvaluationServiceTestsExtensions.CreateUserContext(
        department: "Marketing",
        region: "EMEA"
    );
    var condition = RuleEvaluationServiceTestsExtensions.CreateCondition(
        property: "Department",
        op: Operator.Equal,
        value: "Marketing"
    );

    // Act & Assert
    RuleEvaluationServiceTestsExtensions.AssertConditionResult(
        condition,
        user,
        expectedResult: true
    );
}
```

### Example 2: Testing a Rule with Multiple Conditions Asynchronously

```csharp
[Test]
public async Task FeatureFlagRule_EvaluatesAllConditionsCorrectly()
{
    // Arrange
    var user = RuleEvaluationServiceTestsExtensions.CreateUserContext(
        betaAccess: true,
        subscriptionTier: "Premium"
    );
    var betaCondition = RuleEvaluationServiceTestsExtensions.CreateCondition(
        property: "BetaAccess",
        op: Operator.Equal,
        value: true
    );
    var tierCondition = RuleEvaluationServiceTestsExtensions.CreateCondition(
        property: "SubscriptionTier",
        op: Operator.Equal,
        value: "Premium"
    );
    var rule = RuleEvaluationServiceTestsExtensions.CreateRule(
        conditions: new[] { betaCondition, tierCondition },
        strategy: EvaluationStrategy.All
    );

    // Act & Assert
    await RuleEvaluationServiceTestsExtensions.AssertRuleResultAsync(
        rule,
        user,
        expectedResult: true
    );
}
```

## Notes

- **Assertion Framework Dependency:** The assertion methods rely on the test framework's assertion mechanism (e.g., `Assert.AreEqual`). Test failures manifest as assertion exceptions rather than return values.
- **Evaluation Service Lifetime:** Both `AssertConditionResult` and `AssertRuleResultAsync` internally resolve an `IRuleEvaluationService`. Ensure the service is registered and accessible within the test context; otherwise, an `InvalidOperationException` is thrown.
- **Thread Safety:** The factory methods (`CreateUserContext`, `CreateCondition`, `CreateRule`, `CreateSingleConditionRule`) are static and stateless, making them safe to call concurrently. The assertion methods are also stateless but depend on the thread safety of the underlying evaluation service and assertion framework.
- **Async Evaluation Timeouts:** `AssertRuleResultAsync` may be subject to timeout behavior if the evaluation service or individual conditions perform long-running asynchronous operations. Configure appropriate test timeouts at the test runner level.
- **Edge Cases:**
  - Passing a null `UserContext` to assertion methods will likely cause a `NullReferenceException` or `ArgumentNullException` during evaluation.
  - A `Rule` with an empty conditions collection may evaluate to `true` (vacuous truth for `All`) or `false` (for `Any`), depending on the evaluation strategy implementation. Verify the expected behavior when writing such tests.
  - `CreateSingleConditionRule` is a pure convenience method; its output is indistinguishable from calling `CreateRule` with a single-element conditions array.
