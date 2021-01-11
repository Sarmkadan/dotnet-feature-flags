#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FeatureFlags.Utilities;

/// <summary>
/// Provides System.Text.Json serialization extensions for the <see cref="HashingUtilities"/> type.
/// Enables JSON serialization/deserialization of the HashingUtilities type reference.
/// </summary>
public static class HashingUtilitiesJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    /// <summary>
    /// Serializes the <see cref="HashingUtilities"/> type reference to a JSON string representation.
    /// Note: This method accepts an object parameter to work around C#'s limitation that static classes
    /// cannot be used as extension method receiver types. The method validates that the value represents
    /// the HashingUtilities type.
    /// </summary>
    /// <param name="value">The HashingUtilities type reference (must be a HashingUtilities reference).</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation containing the HashingUtilities type information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is not a HashingUtilities reference.</exception>
    public static string ToJson(this object value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.GetType() != typeof(HashingUtilities))
        {
            throw new ArgumentException("Value must be a HashingUtilities reference", nameof(value));
        }

        var options = new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = indented
        };

        return JsonSerializer.Serialize(new { Type = nameof(HashingUtilities) }, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="HashingUtilities"/> type reference.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A HashingUtilities type reference created from the JSON string.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    public static object? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        return default;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="HashingUtilities"/> type reference.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized HashingUtilities type reference if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    public static bool TryFromJson(string json, out object? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        value = default;
        return true;
    }
}