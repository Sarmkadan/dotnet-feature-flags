# DateTimeExtensionsValidation

Extension methods for validating `DateTime` and `DateTimeOffset` values, ensuring they meet common application requirements such as being non-default, within a valid range, or not in the future.

## API

### `public static IReadOnlyList<string> Validate(DateTime value)`

Validates a `DateTime` value to ensure it is not the default value (`DateTime.MinValue`).
- **Parameters**
  - `value` – The `DateTime` to validate.
- **Returns**
  - A read-only list of validation error messages. Empty if the value is valid.
- **Throws**
  - No exceptions.

### `public static IReadOnlyList<string> ValidateRange(DateTime value, DateTime min, DateTime max)`

Validates a `DateTime` value to ensure it lies within the specified inclusive range.
- **Parameters**
  - `value` – The `DateTime` to validate.
  - `min` – The minimum allowed value (inclusive).
  - `max` – The maximum allowed value (inclusive).
- **Returns**
  - A read-only list of validation error messages. Empty if the value is valid.
- **Throws**
  - `ArgumentException` if `min` is greater than `max`.

### `public static IReadOnlyList<string> Validate(DateTimeOffset value)`

Validates a `DateTimeOffset` value to ensure it is not the default value (`DateTimeOffset.MinValue`).
- **Parameters**
  - `value` – The `DateTimeOffset` to validate.
- **Returns**
  - A read-only list of validation error messages. Empty if the value is valid.
- **Throws**
  - No exceptions.

### `public static bool IsValid(DateTime value)`

Checks whether a `DateTime` value is not the default value (`DateTime.MinValue`).
- **Parameters**
  - `value` – The `DateTime` to validate.
- **Returns**
  - `true` if the value is valid; otherwise, `false`.
- **Throws**
  - No exceptions.

### `public static bool IsValidRange(DateTime value, DateTime min, DateTime max)`

Checks whether a `DateTime` value lies within the specified inclusive range.
- **Parameters**
  - `value` – The `DateTime` to validate.
  - `min` – The minimum allowed value (inclusive).
  - `max` – The maximum allowed value (inclusive).
- **Returns**
  - `true` if the value is valid; otherwise, `false`.
- **Throws**
  - `ArgumentException` if `min` is greater than `max`.

### `public static bool IsValid(DateTimeOffset value)`

Checks whether a `DateTimeOffset` value is not the default value (`DateTimeOffset.MinValue`).
- **Parameters**
  - `value` – The `DateTimeOffset` to validate.
- **Returns**
  - `true` if the value is valid; otherwise, `false`.
- **Throws**
  - No exceptions.

### `public static void EnsureValid(DateTime value)`

Throws an `ArgumentException` if the `DateTime` value is the default value (`DateTime.MinValue`).
- **Parameters**
  - `value` – The `DateTime` to validate.
- **Throws**
  - `ArgumentException` if the value is invalid.

### `public static void EnsureValidRange(DateTime value, DateTime min, DateTime max)`

Throws an `ArgumentException` if the `DateTime` value is outside the specified inclusive range.
- **Parameters**
  - `value` – The `DateTime` to validate.
  - `min` – The minimum allowed value (inclusive).
  - `max` – The maximum allowed value (inclusive).
- **Throws**
  - `ArgumentException` if the value is invalid.
  - `ArgumentException` if `min` is greater than `max`.

### `public static void EnsureValid(DateTimeOffset value)`

Throws an `ArgumentException` if the `DateTimeOffset` value is the default value (`DateTimeOffset.MinValue`).
- **Parameters**
  - `value` – The `DateTimeOffset` to validate.
- **Throws**
  - `ArgumentException` if the value is invalid.

## Usage
