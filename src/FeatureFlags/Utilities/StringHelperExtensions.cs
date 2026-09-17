#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;

namespace FeatureFlags.Utilities;

/// <summary>
/// Additional helper extension methods for string operations.
/// </summary>
public static class StringHelperExtensions
{
    /// <summary>
    /// Encodes the input string to a Base64 string.
    /// </summary>
    /// <param name="input">The string to encode.</param>
    /// <returns>The Base64 encoded string.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public static string ToBase64(this string input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }
}
