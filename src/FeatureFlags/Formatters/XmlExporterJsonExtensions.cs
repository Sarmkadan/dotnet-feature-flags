#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using FeatureFlags.Models;

namespace FeatureFlags.Formatters;

/// <summary>
/// Provides System.Text.Json serialization extensions for working with XmlExporter
/// data types to enable conversion between XML and JSON formats.
/// </summary>
public static class XmlExporterJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Converts a collection of FeatureFlag objects to JSON format.
    /// </summary>
    /// <param name="flags">The FeatureFlag collection to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the FeatureFlag collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="flags"/> is null.</exception>
    public static string ToJson(this IEnumerable<FeatureFlag> flags, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(flags);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true,
                IndentSize = 2
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(flags, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a collection of FeatureFlag objects.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A collection of FeatureFlag objects populated from the JSON data, or null if parsing fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static IReadOnlyList<FeatureFlag>? FromJsonToFeatureFlags(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<List<FeatureFlag>>(json, _jsonOptions)?.AsReadOnly();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a collection of FeatureFlag objects.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized FeatureFlag collection if successful.</param>
    /// <returns>True if deserialization succeeded; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static bool TryFromJson(string json, out IReadOnlyList<FeatureFlag>? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            var result = JsonSerializer.Deserialize<List<FeatureFlag>>(json, _jsonOptions);
            value = result?.AsReadOnly();
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }

    /// <summary>
    /// Converts a collection of AuditLog objects to JSON format.
    /// </summary>
    /// <param name="logs">The AuditLog collection to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the AuditLog collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logs"/> is null.</exception>
    public static string ToJson(this IEnumerable<AuditLog> logs, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(logs);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true,
                IndentSize = 2
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(logs, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a collection of AuditLog objects.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A collection of AuditLog objects populated from the JSON data, or null if parsing fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static IReadOnlyList<AuditLog>? FromJsonToAuditLogs(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<List<AuditLog>>(json, _jsonOptions)?.AsReadOnly();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Converts a collection of Rule objects to JSON format.
    /// </summary>
    /// <param name="rules">The Rule collection to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the Rule collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rules"/> is null.</exception>
    public static string ToJson(this IEnumerable<Rule> rules, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(rules);

        var options = indented
            ? new JsonSerializerOptions(_jsonOptions)
            {
                WriteIndented = true,
                IndentSize = 2
            }
            : _jsonOptions;

        return JsonSerializer.Serialize(rules, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a collection of Rule objects.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A collection of Rule objects populated from the JSON data, or null if parsing fails.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is empty or whitespace.</exception>
    public static IReadOnlyList<Rule>? FromJsonToRules(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);

        try
        {
            return JsonSerializer.Deserialize<List<Rule>>(json, _jsonOptions)?.AsReadOnly();
        }
        catch (JsonException)
        {
            return null;
        }
    }
}