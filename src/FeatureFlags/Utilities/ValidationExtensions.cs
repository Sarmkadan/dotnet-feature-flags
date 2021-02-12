#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Utilities;

/// <summary>
/// Extension methods for validation of common data types and patterns.
/// Provides guard clauses and validation helpers to ensure data integrity.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Checks if collection is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <returns>True if the collection is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? collection)
    {
        return collection?.Any() != true;
    }

    /// <summary>
    /// Throws ArgumentException if collection is null or empty.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check.</param>
    /// <param name="paramName">Name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the collection is null or empty.</exception>
    public static void ThrowIfNullOrEmpty<T>(this IEnumerable<T>? collection, string paramName)
    {
        if (collection.IsNullOrEmpty())
        {
            throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }
    }

    /// <summary>
    /// Throws ArgumentException if string is null or whitespace.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="paramName">Name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the string is null or whitespace.</exception>
    public static void ThrowIfNullOrWhiteSpace(this string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{paramName} cannot be null or whitespace", paramName);
        }
    }

    /// <summary>
    /// Checks if string is a valid non-negative integer.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid non-negative integer; otherwise, false.</returns>
    public static bool IsValidNonNegativeInteger(this string? input)
    {
        return int.TryParse(input, out var value) && value >= 0;
    }

    /// <summary>
    /// Checks if string is a valid percentage (0-100).
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is a valid percentage; otherwise, false.</returns>
    public static bool IsValidPercentage(this string? input)
    {
        return int.TryParse(input, out var value) && value >= 0 && value <= 100;
    }

    /// <summary>
    /// Checks if integer is within specified range (inclusive).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (inclusive).</param>
    /// <returns>True if the value is within range; otherwise, false.</returns>
    public static bool IsInRange(this int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Checks if integer is a valid percentage (0-100).
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is a valid percentage; otherwise, false.</returns>
    public static bool IsValidPercentage(this int value)
    {
        return value >= 0 && value <= 100;
    }

    /// <summary>
    /// Throws ArgumentException if integer is not a valid percentage.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">Name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the value is not a valid percentage.</exception>
    public static void ThrowIfNotValidPercentage(this int value, string paramName)
    {
        if (!value.IsValidPercentage())
        {
            throw new ArgumentException($"{paramName} must be between 0 and 100", paramName);
        }
    }

    /// <summary>
    /// Checks if collection contains any duplicate elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check for duplicates.</param>
    /// <returns>True if duplicates exist; otherwise, false.</returns>
    public static bool HasDuplicates<T>(this IEnumerable<T> collection)
    {
        var set = new HashSet<T>();
        return collection.Any(item => !set.Add(item));
    }

    /// <summary>
    /// Checks if string length is between min and max (inclusive).
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <param name="minLength">The minimum length (inclusive).</param>
    /// <param name="maxLength">The maximum length (inclusive).</param>
    /// <returns>True if the string length is valid; otherwise, false.</returns>
    public static bool IsLengthValid(this string? input, int minLength, int maxLength)
    {
        return !string.IsNullOrEmpty(input) &&
               input.Length >= minLength &&
               input.Length <= maxLength;
    }

    /// <summary>
    /// Checks if string matches a simple key format (alphanumeric + underscore/dash).
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string matches the key format; otherwise, false.</returns>
    public static bool IsValidKeyFormat(this string? input) =>
        !string.IsNullOrWhiteSpace(input) &&
        System.Text.RegularExpressions.Regex.IsMatch(input, @"^[a-zA-Z0-9_-]+$");

    /// <summary>
    /// Checks if string contains only alphanumeric characters.
    /// </summary>
    /// <param name="input">The string to validate.</param>
    /// <returns>True if the string is alphanumeric; otherwise, false.</returns>
    public static bool IsAlphanumeric(this string? input)
    {
        return !string.IsNullOrEmpty(input) && input.All(char.IsLetterOrDigit);
    }

    /// <summary>
    /// Validates that object is not null, throws ArgumentNullException if it is.
    /// </summary>
    /// <typeparam name="T">The type of the object.</typeparam>
    /// <param name="obj">The object to validate.</param>
    /// <param name="paramName">Name of the parameter being validated.</param>
    /// <exception cref="ArgumentNullException">Thrown when the object is null.</exception>
    public static void ThrowIfNull<T>(this T? obj, string paramName) where T : class
    {
        if (obj is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    /// <summary>
    /// Checks if value is the default value for its type.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is default; otherwise, false.</returns>
    public static bool IsDefault<T>(this T value) where T : struct
    {
        return Equals(value, default(T));
    }

    /// <summary>
    /// Checks if GUID is empty (all zeros).
    /// </summary>
    /// <param name="value">The GUID to check.</param>
    /// <returns>True if the GUID is empty; otherwise, false.</returns>
    public static bool IsEmpty(this Guid value)
    {
        return value == Guid.Empty;
    }

    /// <summary>
    /// Throws ArgumentException if GUID is empty.
    /// </summary>
    /// <param name="value">The GUID to validate.</param>
    /// <param name="paramName">Name of the parameter being validated.</param>
    /// <exception cref="ArgumentException">Thrown when the GUID is empty.</exception>
    public static void ThrowIfEmpty(this Guid value, string paramName)
    {
        if (value.IsEmpty())
        {
            throw new ArgumentException($"{paramName} cannot be empty", paramName);
        }
    }
}