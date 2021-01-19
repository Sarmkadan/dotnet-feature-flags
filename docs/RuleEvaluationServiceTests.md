# RuleEvaluationServiceTests

The `RuleEvaluationServiceTests` class contains unit tests for the `RuleEvaluationService`, verifying the behavior of condition and rule evaluation logic. Each test method exercises a specific scenario, such as handling inactive conditions, null arguments, or combinations of AND/OR logic across multiple conditions. The tests are designed to be run with a standard test framework (e.g., xUnit or MSTest) and assert expected outcomes including return values and exceptions.

## API

### `public RuleEvaluationServiceTests()`

Initializes a new instance of the test class. No parameters or return value.

### `public void EvaluateCondition_WithInactiveCondition_ReturnsFalse()`

Tests that `EvaluateCondition` returns `false` when the condition is inactive.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: No exceptions expected.

### `public void EvaluateCondition_WithNullCondition_ThrowsArgumentNullException()`

Tests that `EvaluateCondition` throws an `ArgumentNullException` when the condition is `null`.  
- **Parameters**: None.  
- **Returns**: `void`.  
- **Throws**: `ArgumentNullException` when the condition argument is `null`.

### `public async Task EvaluateRuleAsync_WithInactiveRule_ReturnsFalse()`

Tests that `EvaluateRuleAsync` returns `false` when the rule is inactive.  
- **Parameters**: None.  
- **Returns**: `Task` (void).  
- **Throws**: No exceptions expected.

### `public async Task EvaluateRuleAsync_WithAndLogic_AllConditionsMatch_ReturnsTrue()`

Tests that `EvaluateRuleAsync` returns `true` when the rule uses AND logic and all conditions match.  
- **Parameters**: None.  
- **Returns**: `Task` (void).  
- **Throws**: No exceptions expected.

### `public async Task EvaluateRuleAsync_WithAndLogic_OneConditionFails_ReturnsFalse()`

Tests that `EvaluateRuleAsync` returns `false` when the rule uses AND logic and at least one condition fails.  
- **Parameters**: None.  
- **Returns**: `Task` (void).  
- **Throws**: No exceptions expected.

### `public async Task EvaluateRuleAsync_WithOrLogic_OneConditionMatches_ReturnsTrue()`

Tests that `EvaluateRuleAsync` returns `true` when the rule uses OR logic and at least one condition matches.  
- **Parameters**: None.  
- **Returns**: `Task` (void).  
- **Throws**: No exceptions expected.

### `public async Task EvaluateRuleAsync_WithOrLogic_NoConditionsMatch_ReturnsFalse()`

Tests that `EvaluateRuleAsync` returns `false` when the rule uses OR logic and no conditions match.  
- **Parameters**: None.  
- **Returns**: `Task` (void).  
- **Throws**: No exceptions expected.

## Usage

The following examples demonstrate how to use the test class in a typical unit testing workflow. The first example shows a test method that verifies AND logic with all conditions matching. The second example shows a test that expects an `ArgumentNullException` when a null condition is passed.

```csharp
// Example 1: Testing AND logic with all conditions matching
[Fact]
public async Task EvaluateRuleAsync_WithAndLogic_AllConditionsMatch_ReturnsTrue()
{
    // Arrange
    var service = new RuleEvaluationService();
    var rule = new Rule
    {
        Logic = LogicType.And,
        Conditions = new List<Condition>
        {
            new Condition { IsActive = true, Evaluate = () => true },
            new Condition { IsActive = true, Evaluate = () => true }
        }
    };

    // Act
    bool result = await service.EvaluateRuleAsync(rule);

    // Assert
    Assert.True(result);
}
```

```csharp
// Example 2: Testing null condition throws ArgumentNullException
[Fact]
public void EvaluateCondition_WithNullCondition_ThrowsArgumentNullException()
{
    // Arrange
    var service = new RuleEvaluationService();

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() => service.EvaluateCondition(null));
}
```

## Notes

- **Edge Cases**:  
  - Inactive conditions or rules always cause evaluation to return `false`, regardless of the condition’s logic or other active conditions.  
  - A `null` condition passed to `EvaluateCondition` immediately throws `ArgumentNullException`; no further evaluation is attempted.  
  - For AND logic, evaluation short-circuits on the first failing condition. For OR logic, evaluation short-circuits on the first matching condition.  
  - Async test methods must be awaited to ensure the test framework captures exceptions and completion correctly.

- **Thread Safety**:  
  - The test class itself is not thread-safe; it is intended to be used by a single test runner thread.  
  - The `RuleEvaluationService` under test should be thread-safe if it is used concurrently in production, but these tests do not verify concurrent behavior.  
  - Each test method creates its own service instance and test data, avoiding shared state between tests.
