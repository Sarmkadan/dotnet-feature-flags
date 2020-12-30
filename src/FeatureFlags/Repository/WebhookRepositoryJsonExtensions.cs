#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using FeatureFlags.Integration;

namespace FeatureFlags.Repository;

/// <summary>
/// Provides System.Text.Json serialization extensions for WebhookRepository.
/// Note: WebhookRepository is a service class with dependencies and cannot be fully serialized.
/// These extension methods provide basic serialization of the repository's type information.
/// </summary>
public static class WebhookRepositoryJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Serializes a WebhookRepository type reference to JSON string.
    /// </summary>
    /// <param name="value">The webhook repository instance (type reference only)</param>
    /// <param name="indented">Whether to format the JSON with indentation</param>
    /// <returns>JSON string representation</returns>
    public static string ToJson(this WebhookRepository value, bool indented = false)
    {
        if (value is null)
        {
            return "null";
        }

        var options = indented
            ? new JsonSerializerOptions(_jsonSerializerOptions) { WriteIndented = true }
            : _jsonSerializerOptions;

        return JsonSerializer.Serialize(new { Type = "WebhookRepository", Assembly = typeof(WebhookRepository).Assembly.GetName().Name }, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a WebhookRepository instance.
    /// Note: Returns null as WebhookRepository requires dependency injection.
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <returns>null (WebhookRepository requires dependency injection)</returns>
    public static WebhookRepository? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            // WebhookRepository cannot be deserialized as it requires dependency injection
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a WebhookRepository instance.
    /// </summary>
    /// <param name="json">JSON string to deserialize</param>
    /// <param name="value">Output parameter for the deserialized instance</param>
    /// <returns>True if deserialization succeeded; otherwise false</returns>
    public static bool TryFromJson(string json, out WebhookRepository? value)
    {
        value = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            // WebhookRepository cannot be deserialized as it requires dependency injection
            value = null;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}