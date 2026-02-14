// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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
    public static string ToSha256(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        }

        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashedBytes).ToLower();
    }

    /// <summary>
    /// Converts string to a numeric hash value suitable for percentage calculations (0-99).
    /// Uses consistent hashing to ensure same input always produces same output.
    /// </summary>
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
    /// Checks if string is a valid email address format using basic regex pattern.
    /// </summary>
    public static bool IsValidEmail(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        try
        {
            var addr = new System.Net.Mail.MailAddress(input);
            return addr.Address == input;
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
    public static string SnakeCaseToPascalCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var parts = input.Split('_');
        return string.Concat(parts.Select(p => p.Length > 0
            ? char.ToUpper(p[0]) + p.Substring(1).ToLower()
            : string.Empty));
    }

    /// <summary>
    /// Converts PascalCase or camelCase string to snake_case for normalization.
    /// Example: "FeatureFlagKey" -> "feature_flag_key"
    /// </summary>
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        var sb = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && i > 0)
            {
                sb.Append('_');
            }

            sb.Append(char.ToLower(input[i]));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Truncates string to specified length, optionally adding ellipsis suffix.
    /// </summary>
    public static string Truncate(this string input, int maxLength, bool addEllipsis = true)
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
        {
            return input;
        }

        var truncated = input.Substring(0, maxLength);
        return addEllipsis ? truncated + "..." : truncated;
    }

    /// <summary>
    /// Safely parses string to integer, returning default value if parse fails.
    /// </summary>
    public static int ToIntOrDefault(this string input, int defaultValue = 0)
    {
        return int.TryParse(input, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Safely parses string to double, returning default value if parse fails.
    /// </summary>
    public static double ToDoubleOrDefault(this string input, double defaultValue = 0.0)
    {
        return double.TryParse(input, out var result) ? result : defaultValue;
    }

    /// <summary>
    /// Checks if string contains any of the provided substrings (case-insensitive).
    /// </summary>
    public static bool ContainsAny(this string input, params string[] values)
    {
        return values.Any(v => input.Contains(v, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Repeats string the specified number of times.
    /// </summary>
    public static string Repeat(this string input, int count)
    {
        if (count <= 0)
        {
            return string.Empty;
        }

        return string.Concat(Enumerable.Repeat(input, count));
    }
}
