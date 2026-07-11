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

        return JsonSerializer.Serialize(
            new { Type = "WebhookRepository", Assembly = typeof(WebhookRepository).Assembly.GetName().Name },
            options);
    }

    /// <summary>
    /// Deserializes a JSON string to a WebhookRepository instance.
    /// </summary>
    /// <remarks>
    /// WebhookRepository requires dependency injection and cannot be deserialized from JSON.
    /// This method always returns <see langword="null"/>.
    /// </remarks>
    /// <param name="json">JSON string to deserialize.</param>
    /// <returns><see langword="null"/> as WebhookRepository requires dependency injection.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="json"/> is <see langword="null"/>.</exception>
    public static WebhookRepository? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        return null;
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a WebhookRepository instance.
    /// </summary>
    /// <param name="json">JSON string to deserialize.</param>
    /// <param name="value">Output parameter for the deserialized instance.</param>
    /// <returns>True if deserialization succeeded; otherwise false.</returns>
    public static bool TryFromJson(string json, out WebhookRepository? value)
    {
        value = null;
        ArgumentNullException.ThrowIfNull(json);

        return false;
    }
}
