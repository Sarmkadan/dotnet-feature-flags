// existing content ...

## StringExtensionTestsExtensions

The `StringExtensionTestsExtensions` class provides a set of extension methods for testing string-related functionality. These methods simplify creating test scenarios for various string operations, including hashing, case conversion, and string manipulation.

Below is a realistic usage example demonstrating the most commonly used extension methods:

```csharp
// Assuming you have an instance of StringExtensionTests
var stringTests = new StringExtensionTests();

// Generate a consistent test string for SHA-256 hashing tests
var sha256TestString = stringTests.GetSha256TestString("test-prefix");

// Create an array of test strings with different patterns
var testStrings = stringTests.GetTestStrings(5);

// Create a valid email address
var validEmail = stringTests.GetValidEmail();

// Create an invalid email address
var invalidEmail = stringTests.GetInvalidEmail();

// Generate a snake_case string
var snakeCaseString = stringTests.GetSnakeCaseString(3);

// Generate a PascalCase string
var pascalCaseString = stringTests.GetPascalCaseString(3);

// Create a long string that needs truncation
var longString = stringTests.GetLongString(20);

// Create a string that can be parsed as an integer
var intString = stringTests.GetIntString(123);

// Create a string that cannot be parsed as an integer
var nonIntString = stringTests.GetNonIntString();

// Create an array of substrings for ContainsAny testing
var substrings = stringTests.GetSubstringsForContains(2, 2);

// Create a repeatable string
var repeatableString = stringTests.GetRepeatableString(5);

// Create a string with special characters
var specialCharsString = stringTests.GetSpecialCharsString();

// Get a null or empty string
var nullOrEmptyString = stringTests.GetNullOrEmptyString(true);
```
