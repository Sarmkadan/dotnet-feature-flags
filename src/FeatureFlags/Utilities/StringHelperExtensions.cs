#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace FeatureFlags.Utilities;

/// <summary>
/// Additional extension methods for string operations.
/// </summary>
public static class StringHelperExtensions
{
    /// <summary>
    /// Truncates string to specified length.
    /// </summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxLength">The maximum length of the resulting string.</param>
    /// <returns>The truncated string, or the original if it's already short enough.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxLength"/> is negative.</exception>
    public static string Truncate(this string input, int maxLength)
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

        return input[..maxLength];
    }
}