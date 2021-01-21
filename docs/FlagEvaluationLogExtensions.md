# FlagEvaluationLogExtensions

Provides extension methods for formatting, matching, and manipulating `FlagEvaluationLog` instances to support feature flag evaluation scenarios.

## API

### `ToFormattedString`

Formats the `FlagEvaluationLog` into a human-readable string representation.

- **Parameters**
  - `log` (`FlagEvaluationLog`): The log instance to format.
- **Return value**
  - `string`: A formatted string containing key evaluation details.
- **Exceptions**
  - Throws `ArgumentNullException` if `log` is `null`.

---

### `MatchesResult`

Determines whether the evaluation result in the log matches the expected outcome.

- **Parameters**
  - `log` (`FlagEvaluationLog`): The log to evaluate.
  - `expectedResult` (`bool`): The expected boolean result to compare against.
- **Return value**
  - `bool`: `true` if the log's result matches `expectedResult`; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `log` is `null`.

---

### `WithResult`

Creates a new `FlagEvaluationLog` with the specified result while preserving other properties.

- **Parameters**
  - `log` (`FlagEvaluationLog`): The original log instance.
  - `result` (`bool`): The new result value to set.
- **Return value**
  - `FlagEvaluationLog`: A new log instance with the updated result.
- **Exceptions**
  - Throws `ArgumentNullException` if `log` is `null`.

---

### `IsWithinTimeRange`

Checks whether the log's evaluation timestamp falls within a specified time range.

- **Parameters**
  - `log` (`FlagEvaluationLog`): The log to check.
  - `start` (`DateTime`): The start of the time range (inclusive).
  - `end` (`DateTime`): The end of the time range (inclusive).
- **Return value**
  - `bool`: `true` if the log's timestamp is within the range; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `log` is `null`.

## Usage
