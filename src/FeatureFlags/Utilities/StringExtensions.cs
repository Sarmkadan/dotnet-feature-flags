#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace FeatureFlags.Utilities;

/// <summary>
/// Extension methods for string operations including hashing, validation, and transformation.
/// Provides common string utilities used throughout the feature flag engine.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Computes SHA-256 hash of the input string for consistent user bucketing in rollouts.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>The SHA-256 hash as a lowercase hexadecimal string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="input"/> is empty.</exception>
    public static string ToSha256(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        }

        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashedBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Converts string to a numeric hash value suitable for percentage calculations (0-99).
    /// Uses consistent hashing to ensure same input always produces same output.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>A value between 0 and 99 inclusive.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="input"/> is empty.</exception>
    public static int ToHash32(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        }

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        // Use first 4 bytes as integer, mod 100 to get 0-99
        return Math.Abs(BitConverter.ToInt32(hash, 0)) % 100;
    }

    /// <summary>
    /// Checks if string is a valid email address format.
    /// </summary>
    /// <param name="input">The email address string to validate.</param>
    /// <returns><see langword="true"/> if the string is a valid email address; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static bool IsValidEmail(this string input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(input);
            return addr.Address.Equals(input, StringComparison.Ordinal);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Converts snake_case string to PascalCase for identifier normalization.
    /// Example: "feature_flag_key" -> "FeatureFlagKey"
    /// </summary>
    /// <param name="input">The snake_case string to convert.</param>
    /// <returns>The PascalCase equivalent.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string SnakeCaseToPascalCase(this string input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var textInfo = CultureInfo.InvariantCulture.TextInfo;
        var parts = input.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(p => textInfo.ToTitleCase(p)));
    }

    /// <summary>
    /// Converts PascalCase or camelCase string to snake_case for normalization.
    /// Example: "FeatureFlagKey" -> "feature_flag_key"
    /// </summary>
    /// <param name="input">The PascalCase or camelCase string to convert.</param>
    /// <returns>The snake_case equivalent.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string ToSnakeCase(this string input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new StringBuilder(input.Length * 2);
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && i > 0)
            {
                sb.Append('_');
            }

            sb.Append(char.ToLowerInvariant(input[i]));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Truncates string to specified length, optionally adding ellipsis suffix.
    /// </summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxLength">The maximum length of the resulting string.</param>
    /// <param name="addEllipsis">Whether to append ellipsis if truncation occurs.</param>
    /// <returns>The truncated string, or the original if it's already short enough.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative.</exception>
    public static string Truncate(this string input, int maxLength, bool addEllipsis = true)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (maxLength < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length cannot be negative");
        }

        if (input.Length <= maxLength)
        {
            return input;
        }

        var truncated = input[..maxLength];
        return addEllipsis ? truncated + "..." : truncated;
    }

    /// <summary>
    /// Safely parses string to integer, returning default value if parse fails.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <param name="defaultValue">The value to return if parsing fails.</param>
    /// <returns>The parsed integer or the default value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static int ToIntOrDefault(this string input, int defaultValue = 0)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        return int.TryParse(input, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Safely parses string to double, returning default value if parse fails.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <param name="defaultValue">The value to return if parsing fails.</param>
    /// <returns>The parsed double or the default value.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static double ToDoubleOrDefault(this string input, double defaultValue = 0.0)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        return double.TryParse(input, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Checks if string contains any of the provided substrings (case-insensitive).
    /// </summary>
    /// <param name="input">The string to search within.</param>
    /// <param name="values">The substrings to search for.</param>
    /// <returns><see langword="true"/> if any substring is found; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="input"/> is <see langword="null"/>.
    /// -or-
    /// <paramref name="values"/> is <see langword="null"/>.
    /// </exception>
    public static bool ContainsAny(this string input, params string[] values)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        return values.Any(v => input.Contains(v, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Repeats string the specified number of times.
    /// </summary>
    /// <param name="input">The string to repeat.</param>
    /// <param name="count">The number of times to repeat the string.</param>
    /// <returns>A new string containing the input repeated <paramref name="count"/> times.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
    public static string Repeat(this string input, int count)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative");
        }

        return string.Concat(Enumerable.Repeat(input, count));
    }
}