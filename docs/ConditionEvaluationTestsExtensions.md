# ConditionEvaluationTestsExtensions

Helper methods for writing tests that assert the evaluation of feature flag conditions. These extensions simplify the creation of conditions and the verification of their evaluation results.

## API

### `public static Condition CreateCondition`

Creates a new `Condition` with the specified evaluator and value.

**Parameters:**
- `evaluator` (`Func<object, bool>`): The evaluation function to apply to the condition's value.
- `value` (`object`): The value to be evaluated by the condition.

**Return value:**
- A new `Condition` instance configured with the provided evaluator and value.

**Exceptions:**
- Throws `ArgumentNullException` if `evaluator` is `null`.

---

### `public static void ShouldEvaluateToTrue`

Asserts that the given condition evaluates to `true` when provided with the specified context value.

**Parameters:**
- `condition` (`Condition`): The condition to evaluate.
- `contextValue` (`object`): The value to pass to the condition's evaluator.

**Exceptions:**
- Throws `XunitException` if the condition evaluates to `false`.
- Throws `ArgumentNullException` if `condition` is `null`.

---

### `public static void ShouldEvaluateToFalse`

Asserts that the given condition evaluates to `false` when provided with the specified context value.

**Parameters:**
- `condition` (`Condition`): The condition to evaluate.
- `contextValue` (`object`): The value to pass to the condition's evaluator.

**Exceptions:**
- Throws `XunitException` if the condition evaluates to `true`.
- Throws `ArgumentNullException` if `condition` is `null`.

---
### `public static Condition[] CreateConditions`

Creates an array of `Condition` instances from a collection of evaluators and values.

**Parameters:**
- `evaluatorsAndValues` (`IEnumerable<(Func<object, bool> evaluator, object value)>`): A sequence of tuples, each containing an evaluator and a corresponding value.

**Return value:**
- An array of `Condition` instances, one for each evaluator-value pair in the input sequence.

**Exceptions:**
- Throws `ArgumentNullException` if `evaluatorsAndValues` is `null`.

## Usage
