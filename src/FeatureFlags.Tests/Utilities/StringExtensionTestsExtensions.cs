#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Utilities;
using Xunit;

namespace FeatureFlags.Tests.Utilities;

/// <summary>
/// Extension methods for <see cref="StringExtensionTests"/> to provide additional testing utilities.
/// Adds helper methods for generating test data, validating test scenarios,
/// and creating edge cases for string extension method testing.
/// </summary>
public static class StringExtensionTestsExtensions
{
    /// <summary>
    /// Generates a consistent test string for SHA-256 hashing tests.
    /// Always returns the same value to ensure deterministic test results.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <param name="prefix">Optional prefix to prepend to the base string</param>
    /// <returns>A consistent test string for hashing operations</returns>
    public static string GetSha256TestString(this StringExtensionTests tests, string? prefix = null)
    {
        return $"{prefix ?? "test"}-input-for-sha256-hashing";
    }

    /// <summary>
    /// Generates multiple test strings with different patterns for comprehensive testing.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <param name="count">Number of test strings to generate</param>
    /// <returns>Array of test strings with varied patterns</returns>
    public static string[] GetTestStrings(this StringExtensionTests tests, int count)
    {
        var result = new string[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = $"test-string-{i}-{Guid.NewGuid():N}";
        }
        return result;
    }

    /// <summary>
    /// Creates an email address that should pass validation.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <param name="domain">Optional custom domain to use</param>
    /// <returns>A valid email address string</returns>
    public static string GetValidEmail(this StringExtensionTests tests, string? domain = null)
    {
        var userPart = Guid.NewGuid().ToString("N").Substring(0, 8);
        return $"user{userPart}@{domain ?? "example.com"}";
    }

    /// <summary>
    /// Creates an email address that should fail validation.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <returns>An invalid email address string</returns>
    public static string GetInvalidEmail(this StringExtensionTests tests)
    {
        return "not-an-email-address";
    }

    /// <summary>
    /// Generates a snake_case string for testing case conversion.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <param name="parts">Number of parts to include in the snake_case string</param>
    /// <returns>A snake_case formatted test string</returns>
    public static string GetSnakeCaseString(this StringExtensionTests tests, int parts = 3)
    {
        var partsList = new List<string>();
        for (int i = 0; i < parts; i++)
        {
            partsList.Add($"part{i + 1}");
        }
        return string.Join("_", partsList);
    }

    /// <summary>
    /// Generates a PascalCase string for testing case conversion.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <param name="parts">Number of parts to include in the PascalCase string</param>
    /// <returns>A PascalCase formatted test string</returns>
    public static string GetPascalCaseString(this StringExtensionTests tests, int parts = 3)
    {
        var partsList = new List<string>();
        for (int i = 0; i < parts; i++)
        {
            partsList.Add($"Part{i + 1}");
        }
        return string.Concat(partsList);
    }

    /// <summary>
    /// Creates a string that needs truncation for testing the Truncate method.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <param name="length">Desired length of the string</param>
    /// <returns>A string longer than the specified length</returns>
    public static string GetLongString(this StringExtensionTests tests, int length)
    {
        return new string('x', length + 10);
    }

    /// <summary>
    /// Creates a string that can be parsed as an integer.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <param name="value">The integer value to convert to string</param>
    /// <returns>A string representation of an integer</returns>
    public static string GetIntString(this StringExtensionTests tests, int value)
    {
        return value.ToString();
    }

    /// <summary>
    /// Creates a string that cannot be parsed as an integer.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <returns>A non-numeric string</returns>
    public static string GetNonIntString(this StringExtensionTests tests)
    {
        return "not-a-number-123";
    }

    /// <summary>
    /// Creates an array of substrings to test with ContainsAny method.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <param name="matchingCount">Number of matching substrings to include</param>
    /// <param name="nonMatchingCount">Number of non-matching substrings to include</param>
    /// <returns>Array of strings for ContainsAny testing</returns>
    public static string[] GetSubstringsForContains(this StringExtensionTests tests, int matchingCount = 2, int nonMatchingCount = 2)
    {
        var result = new List<string>();

        // Add matching substrings
        for (int i = 0; i < matchingCount; i++)
        {
            result.Add($"match{i}");
        }

        // Add non-matching substrings
        for (int i = 0; i < nonMatchingCount; i++)
        {
            result.Add($"nomatch{i}");
        }

        return result.ToArray();
    }

    /// <summary>
    /// Creates a string that can be repeated for testing the Repeat method.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <param name="repeatCount">Number of times to repeat the string</param>
    /// <returns>A string suitable for repetition testing</returns>
    public static string GetRepeatableString(this StringExtensionTests tests, int repeatCount = 5)
    {
        return "test-";
    }

    /// <summary>
    /// Creates a string that contains various special characters for edge case testing.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <returns>A string with special characters</returns>
    public static string GetSpecialCharsString(this StringExtensionTests tests)
    {
        return "test-string_with.different@chars#123";
    }

    /// <summary>
    /// Creates a string that is null or empty for testing edge cases.
    /// </summary>
    /// <param name="tests">The StringExtensionTests instance</param>
    /// <param name="isNull">Whether to return null or empty string</param>
    /// <returns>Null or empty string</returns>
    public static string? GetNullOrEmptyString(this StringExtensionTests tests, bool isNull = false)
    {
        return isNull ? null : string.Empty;
    }
}