# ConditionTests

The `ConditionTests` class serves as the verification suite for the conditional evaluation logic within the `dotnet-feature-flags` library. It validates the behavior of feature flag conditions against various operators, ensuring that equality, inequality, string manipulation, and numerical comparisons function correctly under both standard and edge-case scenarios, such as null contexts or missing required fields.

## API

### Evaluate_EqualsOperator_ReturnsTrueForMatch
Verifies that the condition evaluator returns `true` when the context value exactly matches the expected value using the equals operator.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the evaluation result is not `true`.

### Evaluate_EqualsOperator_CaseInsensitive
Confirms that the equals operator performs case-insensitive comparisons when evaluating string values within the feature flag condition.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the comparison respects case sensitivity.

### Evaluate_NotEqualsOperator_ReturnsTrueForDifference
Validates that the condition evaluator returns `true` when the context value differs from the expected value using the not-equals operator.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the evaluation result is not `true` for differing values.

### Evaluate_ContainsOperator_ReturnsTrueForMatch
Ensures the evaluator correctly identifies when a context string contains the specified substring defined in the condition.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the substring match is not detected.

### Evaluate_StartsWithOperator_ReturnsTrueForMatch
Tests that the evaluator returns `true` when the context value begins with the prefix specified in the condition.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the prefix match fails.

### Evaluate_EndsWithOperator_ReturnsTrueForMatch
Tests that the evaluator returns `true` when the context value ends with the suffix specified in the condition.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the suffix match fails.

### Evaluate_GreaterThanOperator_ReturnsTrueForGreaterValue
Validates numerical or version-based logic where the condition returns `true` if the context value is strictly greater than the expected value.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the greater-than logic fails.

### Evaluate_LessThanOperator_ReturnsTrueForLesserValue
Validates numerical or version-based logic where the condition returns `true` if the context value is strictly less than the expected value.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the less-than logic fails.

### Evaluate_InOperator_ReturnsTrueForValueInList
Verifies that the condition evaluates to `true` when the context value exists within a predefined list of acceptable values.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the value presence in the list is not recognized.

### Evaluate_NullContextValue_ReturnsFalse
Ensures robustness by confirming that the evaluator returns `false` when the provided context value is null, preventing null reference exceptions during evaluation.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if the result is not `false`.

### IsValid_WithRequiredFields_ReturnsTrue
Checks the validation logic to ensure a condition object is deemed valid when all mandatory fields (such as attribute name and expected value) are populated.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if validation fails for a complete object.

### IsValid_WithoutAttributeName_ReturnsFalse
Verifies that the validation logic correctly rejects a condition object where the attribute name is missing or empty.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if validation incorrectly passes.

### IsValid_WithoutExpectedValue_ReturnsFalse
Verifies that the validation logic correctly rejects a condition object where the expected value is missing or empty.
*   **Parameters**: None (uses test framework context).
*   **Return Value**: `void`.
*   **Throws**: Throws an assertion exception if validation incorrectly passes.

## Usage

The following examples demonstrate how the logic verified by `ConditionTests` applies to configuring and evaluating feature flags in a real-world scenario.

### Example 1: String-Based Audience Targeting
This example illustrates a condition where a feature is enabled only if the user's country code starts with "US". The underlying logic corresponds to the `Evaluate_StartsWithOperator_ReturnsTrueForMatch` test case.

```csharp
using Microsoft.FeatureFilters;

var context = new FeatureContext
{
    Name = "NewDashboard",
    Parameters = new Dictionary<string, string>
    {
        ["Requirement"] = "CountryCode starts with 'US'"
    }
};

var filterContext = new EvaluationContext
{
    ContextValues = new Dictionary<string, object>
    {
        ["CountryCode"] = "USA"
    }
};

// The evaluation engine internally performs the StartsWith check
// verified by ConditionTests.Evaluate_StartsWithOperator_ReturnsTrueForMatch
bool isEnabled = await featureManager.IsEnabledAsync("NewDashboard", filterContext);
```

### Example 2: Numerical Threshold Validation
This example demonstrates a condition based on a numerical threshold, such as enabling a beta feature for users with an account age greater than 365 days. This aligns with the `Evaluate_GreaterThanOperator_ReturnsTrueForGreaterValue` test case.

```csharp
using Microsoft.FeatureFilters;

var filterContext = new EvaluationContext
{
    ContextValues = new Dictionary<string, object>
    {
        ["AccountAgeDays"] = 400
    }
};

// Configuration expects AccountAgeDays > 365
// The engine performs the comparison verified by 
// ConditionTests.Evaluate_GreaterThanOperator_ReturnsTrueForGreaterValue
bool isBetaEligible = await featureManager.IsEnabledAsync("BetaFeatures", filterContext);

if (isBetaEligible)
{
    // Grant access to beta features
}
```

## Notes

*   **Null Safety**: The evaluation logic explicitly handles null context values by returning `false` rather than throwing a `NullReferenceException`, as verified by `Evaluate_NullContextValue_ReturnsFalse`. Consumers should still ensure context dictionaries are initialized, but individual missing keys will result in a safe failure of the condition.
*   **Case Sensitivity**: String equality checks (`Equals`) are implemented to be case-insensitive. Developers should not rely on case differences to distinguish feature flag targets when using the equality operator.
*   **Validation Requirements**: A condition object must possess both an `AttributeName` and an `ExpectedValue` to pass validation. Instantiating conditions without these fields will result in validation failures, preventing the feature flag from being evaluated.
*   **Thread Safety**: As this class represents a suite of unit tests validating stateless evaluation logic, the underlying evaluation methods it tests are designed to be thread-safe. They operate purely on input parameters without maintaining internal mutable state between calls.
*   **Type Coercion**: When using greater-than or less-than operators, ensure the context values and expected values are of compatible types (e.g., integers, decimals, or semantic versions) to avoid runtime formatting errors, though the tests assume valid type alignment.
