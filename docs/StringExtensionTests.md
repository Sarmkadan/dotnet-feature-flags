# StringExtensionTests

Unit tests for the `StringExtensions` class, validating behavior of string utility methods such as hashing, formatting, parsing, and date/time manipulation.

## API

### `ToSha256_ProducesConsistentHash`
Ensures that the `ToSha256` extension method returns a deterministic SHA-256 hash for a given input string. No parameters. Returns `void`. Throws if the input string is `null`.

### `ToHash32_ReturnsValueIn0To99`
Validates that the `ToHash32` extension method returns a numeric hash in the inclusive range `[0, 99]`. No parameters. Returns `void`. Throws if the input string is `null`.

### `IsValidEmail_AcceptsValidEmails`
Confirms that the `IsValidEmail` extension method correctly identifies syntactically valid email addresses. No parameters. Returns `void`. Throws if the input string is `null`.

### `IsValidEmail_RejectsInvalidEmails`
Verifies that the `IsValidEmail` extension method rejects malformed or invalid email strings. No parameters. Returns `void`. Throws if the input string is `null`.

### `SnakeCaseToPascalCase_ConvertsCorrectly`
Checks that the `SnakeCaseToPascalCase` extension method transforms snake_case input into PascalCase output. No parameters. Returns `void`. Throws if the input string is `null`.

### `ToSnakeCase_ConvertsCorrectly`
Ensures that the `ToSnakeCase` extension method converts PascalCase or camelCase strings into snake_case. No parameters. Returns `void`. Throws if the input string is `null`.

### `Truncate_TruncatesCorrectly`
Validates that the `Truncate` extension method truncates strings to the specified maximum length, appending an ellipsis if truncated. No parameters. Returns `void`. Throws if the input string is `null` or if `maxLength` is negative.

### `ToIntOrDefault_ParsesSuccessfully`
Tests that the `ToIntOrDefault` extension method parses numeric strings and returns the integer value or a specified default on failure. No parameters. Returns `void`. Throws if the input string is `null`.

### `ContainsAny_DetectsMatchingSubstrings`
Confirms that the `ContainsAny` extension method detects whether the string contains any of the provided substrings. No parameters. Returns `void`. Throws if the input string or the `values` collection is `null`.

### `Repeat_RepeatsStringCorrectly`
Ensures that the `Repeat` extension method repeats the string the specified number of times. No parameters. Returns `void`. Throws if the input string is `null` or if `count` is negative.

### `ToUnixTimestamp_ConvertsCorrectly`
Validates that the `ToUnixTimestamp` extension method converts a `DateTime` to a Unix timestamp in seconds. No parameters. Returns `void`. Throws if the input `DateTime` is `default`.

### `FromUnixTimestamp_ConvertsCorrectly`
Checks that the `FromUnixTimestamp` extension method converts a Unix timestamp in seconds back to a `DateTime`. No parameters. Returns `void`. Throws if the input timestamp is negative or exceeds the valid range.

### `IsBetween_DetectsRangeCorrectly`
Ensures that the `IsBetween` extension method correctly identifies whether a numeric value falls within a specified range. No parameters. Returns `void`. Throws if `min` is greater than `max`.

### `StartOfDay_ReturnsBeginningOfDay`
Validates that the `StartOfDay` extension method returns a `DateTime` representing the start of the day for the given input. No parameters. Returns `void`. Throws if the input `DateTime` is `default`.

### `EndOfDay_ReturnsEndOfDay`
Confirms that the `EndOfDay` extension method returns a `DateTime` representing the end of the day for the given input. No parameters. Returns `void`. Throws if the input `DateTime` is `default`.

### `StartOfMonth_ReturnsFirstDayOfMonth`
Ensures that the `StartOfMonth` extension method returns a `DateTime` representing the first day of the month for the given input. No parameters. Returns `void`. Throws if the input `DateTime` is `default`.

### `IsToday_DetectsCurrentDate`
Validates that the `IsToday` extension method correctly identifies whether a `DateTime` falls on the current date. No parameters. Returns `void`. Throws if the input `DateTime` is `default`.

### `IsPast_DetectsPastDates`
Checks that the `IsPast` extension method correctly identifies whether a `DateTime` is in the past relative to `DateTime.UtcNow`. No parameters. Returns `void`. Throws if the input `DateTime` is `default`.

### `IsNullOrEmpty_DetectsEmpty`
Ensures that the `IsNullOrEmpty` extension method correctly identifies `null` or empty strings. No parameters. Returns `void`. Throws if the input string is `null`.

### `IsValidPercentage_DetectsValidRange`
Validates that the `IsValidPercentage` extension method correctly identifies numeric strings representing values in the range `[0, 100]`. No parameters. Returns `void`. Throws if the input string is `null`.

## Usage
