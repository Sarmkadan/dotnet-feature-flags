# ValidationExtensions

The `ValidationExtensions` class provides a collection of static helper methods designed to enforce data integrity and argument validity within the `dotnet-feature-flags` library. These extensions focus on common validation scenarios such as null checks, string formatting, numeric range verification, and collection consistency, offering both boolean-returning predicates and void methods that throw exceptions upon validation failure to streamline defensive programming patterns.

## API

### IsNullOrEmpty<T>
Determines whether a specified generic object is null or represents an empty collection/string.
*   **Parameters**: `T value` – The object to validate.
*   **Returns**: `bool` – `true` if the value is null or empty; otherwise, `false`.
*   **Throws**: None.

### ThrowIfNullOrEmpty<T>
Validates that a specified generic object is not null or empty, throwing an exception if the condition is met.
*   **Parameters**: `T value` – The object to validate.
*   **Returns**: `void`.
*   **Throws**: `ArgumentException` (or derived type) if the value is null or empty.

### ThrowIfNullOrWhiteSpace
Validates that a specified string is not null, empty, or composed entirely of white-space characters.
*   **Parameters**: `string value` – The string to validate.
*   **Returns**: `void`.
*   **Throws**: `ArgumentException` (or derived type) if the string is null, empty, or white-space.

### IsValidNonNegativeInteger
Checks if a specified integer value is non-negative (greater than or equal to zero).
*   **Parameters**: `int value` – The integer to validate.
*   **Returns**: `bool` – `true` if the value is >= 0; otherwise, `false`.
*   **Throws**: None.

### IsValidPercentage
Determines if a specified numeric value represents a valid percentage (typically within the 0.0 to 100.0 range).
*   **Parameters**: `double value` – The numeric value to validate.
*   **Returns**: `bool` – `true` if the value is a valid percentage; otherwise, `false`.
*   **Throws**: None.

### IsInRange
Verifies whether a specified value falls within a defined inclusive range.
*   **Parameters**: `T value`, `T min`, `T max` – The value to check and the boundary limits.
*   **Returns**: `bool` – `true` if the value is between `min` and `max` (inclusive); otherwise, `false`.
*   **Throws**: None.

### ThrowIfNotValidPercentage
Validates that a numeric value is a valid percentage, throwing an exception if it is not.
*   **Parameters**: `double value` – The numeric value to validate.
*   **Returns**: `void`.
*   **Throws**: `ArgumentOutOfRangeException` if the value is outside the valid percentage range.

### HasDuplicates<T>
Checks if a specified collection contains duplicate entries.
*   **Parameters**: `IEnumerable<T> collection` – The collection to inspect.
*   **Returns**: `bool` – `true` if duplicates exist; otherwise, `false`.
*   **Throws**: None.

### IsLengthValid
Determines if a string's length falls within a specified range.
*   **Parameters**: `string value`, `int minLength`, `int maxLength` – The string and the length boundaries.
*   **Returns**: `bool` – `true` if the string length is within the range; otherwise, `false`.
*   **Throws**: None.

### IsValidKeyFormat
Validates whether a string adheres to the specific formatting rules required for feature flag keys.
*   **Parameters**: `string key` – The feature flag key to validate.
*   **Returns**: `bool` – `true` if the format is valid; otherwise, `false`.
*   **Throws**: None.

### IsAlphanumeric
Checks if a string consists exclusively of alphanumeric characters.
*   **Parameters**: `string value` – The string to validate.
*   **Returns**: `bool` – `true` if all characters are alphanumeric; otherwise, `false`.
*   **Throws**: None.

### ThrowIfNull<T>
Validates that a specified generic object is not null, throwing an exception if it is.
*   **Parameters**: `T value` – The object to validate.
*   **Returns**: `void`.
*   **Throws**: `ArgumentNullException` if the value is null.

### IsDefault<T>
Determines if a generic value is equal to the default value of its type (e.g., `null` for reference types, `0` for integers).
*   **Parameters**: `T value` – The object to validate.
*   **Returns**: `bool` – `true` if the value equals `default(T)`; otherwise, `false`.
*   **Throws**: None.

### IsEmpty
Checks if a specified collection or string is empty.
*   **Parameters**: `object value` – The object (typically IEnumerable or string) to check.
*   **Returns**: `bool` – `true` if the object is empty; otherwise, `false`.
*   **Throws**: None.

### ThrowIfEmpty
Validates that a specified collection or string is not empty, throwing an exception if it is.
*   **Parameters**: `object value` – The object to validate.
*   **Returns**: `void`.
*   **Throws**: `ArgumentException` if the object is empty.

## Usage

### Example 1: Validating Feature Flag Configuration
This example demonstrates validating a percentage-based rollout configuration and ensuring the feature key format is correct before registration.

```csharp
using Microsoft.FeatureFlags.Validation; // Hypothetical namespace

public void RegisterFeature(string key, double rolloutPercentage)
{
    // Validate key format and throw if invalid
    if (!ValidationExtensions.IsValidKeyFormat(key))
    {
        throw new ArgumentException("Invalid feature flag key format.", nameof(key));
    }

    // Validate percentage using the throwing helper
    ValidationExtensions.ThrowIfNotValidPercentage(rolloutPercentage);

    // Proceed with registration logic
    Console.WriteLine($"Feature '{key}' registered with {rolloutPercentage}% rollout.");
}
```

### Example 2: Sanitizing and Checking Collection Inputs
This example shows how to ensure a list of variant names contains no duplicates and is not empty before processing.

```csharp
using System.Collections.Generic;
using Microsoft.FeatureFlags.Validation; // Hypothetical namespace

public void ProcessVariants(List<string> variantNames)
{
    // Ensure the list is not null or empty
    ValidationExtensions.ThrowIfNullOrEmpty(variantNames);

    // Check for duplicates using the predicate
    if (ValidationExtensions.HasDuplicates(variantNames))
    {
        throw new InvalidOperationException("Variant names must be unique.");
    }

    // Additional string validation per item
    foreach (var name in variantNames)
    {
        if (!ValidationExtensions.IsAlphanumeric(name))
        {
            throw new FormatException($"Variant name '{name}' must be alphanumeric.");
        }
    }
}
```

## Notes

*   **Thread Safety**: As all members are static methods operating solely on provided input parameters without maintaining internal mutable state, the `ValidationExtensions` class is inherently thread-safe.
*   **Generic Constraints**: Methods utilizing `<T>` (such as `IsNullOrEmpty<T>` or `IsDefault<T>`) rely on the runtime type of the argument. When used with reference types, `null` checks are performed; for value types, `default(T)` comparisons are used where applicable.
*   **Exception Behavior**: The `ThrowIf...` methods do not return a value. They are designed to halt execution immediately via exception if validation fails. Callers should ensure these are used in contexts where an exception is the desired control flow for invalid data, rather than inside conditional logic expecting a boolean result.
*   **Percentage Precision**: The `IsValidPercentage` and `ThrowIfNotValidPercentage` methods typically enforce a range of `[0.0, 100.0]`. Floating-point precision edge cases (e.g., `100.00000000000001`) will result in validation failure.
*   **Key Format Specifics**: `IsValidKeyFormat` enforces library-specific constraints for feature flags (often restricting special characters to ensure compatibility with configuration providers). It is distinct from `IsAlphanumeric` which allows any combination of letters and numbers without structural rules.
