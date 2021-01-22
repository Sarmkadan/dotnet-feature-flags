# StringExtensionTestsExtensions

The `StringExtensionTestsExtensions` static class provides a collection of predefined test data strings and string arrays used to validate the behavior of string extension methods within the `dotnet-feature-flags` project. Each member returns a deterministic value or set of values designed to cover common input categories such as valid and invalid emails, case conversions, numeric parsing, substring containment, and edge cases like null or empty strings. These helpers simplify unit test setup by centralizing test inputs and ensuring consistency across test suites.

## API

### `public static string GetSha256TestString`
Returns a fixed string whose SHA‑256 hash is known and can be used to verify hashing extension methods.  
**Returns:** A non-null, non-empty string.  
**Throws:** Never.

### `public static string[] GetTestStrings`
Returns an array of strings representing a variety of typical input values (e.g., short, long, mixed-case, alphanumeric).  
**Returns:** A non-null array of non-null strings.  
**Throws:** Never.

### `public static string GetValidEmail`
Returns a string that conforms to a standard email address format (e.g., `user@example.com`).  
**Returns:** A non-null, non-empty string.  
**Throws:** Never.

### `public static string GetInvalidEmail`
Returns a string that does **not** conform to a valid email address format (e.g., missing `@` or domain).  
**Returns:** A non-null, non-empty string.  
**Throws:** Never.

### `public static string GetSnakeCaseString`
Returns a string in snake_case format (e.g., `hello_world_test`).  
**Returns:** A non-null, non-empty string.  
**Throws:** Never.

### `public static string GetPascalCaseString`
Returns a string in PascalCase format (e.g., `HelloWorldTest`).  
**Returns:** A non-null, non-empty string.  
**Throws:** Never.

### `public static string GetLongString`
Returns a string with a length greater than typical short inputs (e.g., 500+ characters).  
**Returns:** A non-null, non-empty string.  
**Throws:** Never.

### `public static string GetIntString`
Returns a string that can be successfully parsed as an integer (e.g., `"12345"`).  
**Returns:** A non-null, non-empty string.  
**Throws:** Never.

### `public static string GetNonIntString`
Returns a string that cannot be parsed as an integer (e.g., `"abc"` or `"12.5"`).  
**Returns:** A non-null, non-empty string.  
**Throws:** Never.

### `public static string[] GetSubstringsForContains`
Returns an array of strings that are expected to be found as substrings within the string returned by `GetLongString` (or another designated source).  
**Returns:** A non-null array of non-null strings.  
**Throws:** Never.

### `public static string GetRepeatableString`
Returns a string that is guaranteed to be identical on every call, useful for deterministic tests.  
**Returns:** A non-null, non-empty string.  
**Throws:** Never.

### `public static string GetSpecialCharsString`
Returns a string containing special characters (e.g., punctuation, symbols, whitespace).  
**Returns:** A non-null, non-empty string.  
**Throws:** Never.

### `public static string? GetNullOrEmptyString`
Returns either `null` or `string.Empty` on each call. The exact value is not guaranteed to be consistent across invocations.  
**Returns:** A nullable string that may be `null` or `""`.  
**Throws:** Never.

## Usage

The following examples demonstrate typical usage in xUnit test methods.

**Example 1: Testing a snake_case converter**

```csharp
using Xunit;

public class StringExtensionTests
{
    [Fact]
    public void ToSnakeCase_ConvertsPascalCase()
    {
        // Arrange
        var input = StringExtensionTestsExtensions.GetPascalCaseString();
        var expected = StringExtensionTestsExtensions.GetSnakeCaseString();

        // Act
        var result = input.ToSnakeCase();

        // Assert
        Assert.Equal(expected, result);
    }
}
```

**Example 2: Testing email validation and integer parsing**

```csharp
using Xunit;

public class StringExtensionTests
{
    [Fact]
    public void IsValidEmail_ReturnsTrueForValidEmail()
    {
        // Arrange
        var validEmail = StringExtensionTestsExtensions.GetValidEmail();

        // Act
        var result = validEmail.IsValidEmail();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TryParseInt_ReturnsFalseForNonIntString()
    {
        // Arrange
        var nonInt = StringExtensionTestsExtensions.GetNonIntString();

        // Act
        var success = int.TryParse(nonInt, out _);

        // Assert
        Assert.False(success);
    }
}
```

## Notes

- All methods are static and return new string instances or new arrays on each invocation. No shared mutable state is involved, making the class inherently thread‑safe.
- `GetNullOrEmptyString` may return `null` or `string.Empty` unpredictably. Tests that depend on a specific value should handle both cases or use a separate assertion strategy (e.g., checking for `null` or empty).
- The strings returned by `GetLongString` and `GetSubstringsForContains` are designed to be used together: each substring in the array is guaranteed to appear at least once in the long string.
- The values returned by `GetSha256TestString`, `GetRepeatableString`, and other deterministic methods are constant across all calls within the same application domain, but may change between library versions. Tests should not rely on the exact content beyond the documented characteristics.
- No method throws exceptions under normal usage. Callers should not need try-catch blocks when invoking these helpers.
