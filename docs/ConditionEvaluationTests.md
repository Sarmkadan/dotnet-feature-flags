# ConditionEvaluationTests

Provides a set of unit tests that verify the behavior of the `ConditionEvaluation` logic used in the feature‑flags system. Each test method focuses on a specific operator or validation scenario, asserting that the evaluation returns the expected Boolean result or that validation behaves correctly.

## API

### `public void Evaluate_WithEqualsOperator_CaseInsensitiveMatch_ReturnsTrue`
- **Purpose**: Confirms that the equality operator (`==`) performs a case‑insensitive comparison when evaluating a string condition.
- **Parameters**: None.
- **Return Value**: `void`. The test passes if the evaluation yields `true`; otherwise the unit‑test framework throws an assertion exception.
- **Throws**: May throw an assertion exception (e.g., `AssertFailedException`) if the evaluated result is not `true`.

### `public void Evaluate_WithGreaterThanOperator_NumericContextValue_ReturnsTrue`
- **Purpose**: Verifies that the greater‑than operator (`>`) correctly compares numeric context values.
- **Parameters**: None.
- **Return Value**: `void`. Success is indicated by the test completing without assertion failures.
- **Throws**: May throw an assertion exception if the result is not `true`.

### `public void Evaluate_WithInOperator_ValueExistsInCommaDelimitedList_ReturnsTrue`
- **Purpose**: Ensures that the `in` operator detects a value present within a comma‑delimited list.
- **Parameters**: None.
- **Return Value**: `void`. Passes when the evaluation returns `true`.
- **Throws**: May throw an assertion exception on an unexpected result.

### `public void Evaluate_WithNullContextValue_ReturnsFalse`
- **Purpose**: Checks that evaluating a condition with a `null` context value yields `false`.
- **Parameters**: None.
- **Return Value**: `void`. Passes when the result is `false`.
- **Throws**: May throw an assertion exception if the result is not `false`.

### `public void Evaluate_WithStringOperators_ReturnsExpectedResult`
- **Purpose**: Validates that various string‑based operators (e.g., `==`, `!=`, `contains`, `startswith`, `endswith`) produce the expected Boolean outcomes.
- **Parameters**: None.
- **Return Value**: `void`. Passes when all asserted outcomes match expectations.
- **Throws**: May throw an assertion exception for any mismatched result.

### `public void IsValid_WithEmptyExpectedValue_ReturnsFalse`
- **Purpose**: Confirms that the validation logic treats an empty expected value as invalid.
- **Parameters**: None.
- **Return Value**: `void`. Passes when `IsValid` returns `false`.
- **Throws**: May throw an assertion exception if the result is not `false`.

### `public void IsValid_WithAllRequiredFields_ReturnsTrue`
- **Purpose**: Ensures that when all required fields are supplied, the validation returns `true`.
- **Parameters**: None.
- **Return Value**: `void`. Passes when the validation result is `true`.
- **Throws**: May throw an assertion exception if the result is not `true`.

## Usage

These methods are intended to be executed by a unit‑test runner (e.g., xUnit, NUnit, MSTest). The following examples illustrate how they appear in a test class and how the underlying `ConditionEvaluation` API might be used in production code.

```csharp
using Xunit;
using DotNetFeatureFlags.Evaluation; // hypothetical namespace

public class ConditionEvaluationTests
{
    [Fact]
    public void Evaluate_WithEqualsOperator_CaseInsensitiveMatch_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition { Operator = "==", ExpectedValue = "Red" };
        var context = new Dictionary<string, object> { { "Color", "red" } };

        // Act
        bool result = ConditionEvaluation.Evaluate(condition, context);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithAllRequiredFields_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition { Operator = "in", ExpectedValue = "apple,banana,cherry" };

        // Act
        bool isValid = ConditionEvaluation.IsValid(condition);

        // Assert
        Assert.True(isValid);
    }
}
```

In production code, the same evaluation logic can be invoked directly:

```csharp
var condition = new Condition { Operator = ">", ExpectedValue = "10" };
var context = new Dictionary<string, object> { { "Score", 15 } };

bool passes = ConditionEvaluation.Evaluate(condition, context); // returns true
```

## Notes

- **Null handling**: The `Evaluate` method treats a `null` context value as a mismatch for all operators except those explicitly defined to handle nulls; consequently, tests such as `Evaluate_WithNullContextValue_ReturnsFalse` assert a `false` outcome.
- **Empty expected values**: Validation (`IsValid`) considers an empty or whitespace‑only expected value invalid for operators that require a non‑empty literal (e.g., `==`, `in`). This is reflected in `IsValid_WithEmptyExpectedValue_ReturnsFalse`.
- **Case sensitivity**: Equality and inequality operators are implemented to be case‑insensitive for string comparisons, as verified by `Evaluate_WithEqualsOperator_CaseInsensitiveMatch_ReturnsTrue`.
- **Numeric parsing**: Operators that perform numeric comparisons (`>`, `<`, `>=`, `<=`) attempt to parse context and expected values as `double`. Invalid formats result in a `false` evaluation.
- **Thread safety**: `ConditionEvaluation` does not maintain mutable state; all methods rely solely on their input parameters. Therefore, the class is thread‑safe and can be invoked concurrently from multiple threads without additional synchronization. The test methods themselves are also stateless and safe to run in parallel test execution.
