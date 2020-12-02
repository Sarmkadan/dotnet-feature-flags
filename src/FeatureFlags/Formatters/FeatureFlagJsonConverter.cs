#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using FeatureFlags.Models;

namespace FeatureFlags.Formatters;

/// <summary>
/// Custom JSON converter for FeatureFlag entities that handles circular references and optimizes serialization.
/// Provides cleaner JSON output by excluding unnecessary navigation properties and formatting timestamps consistently.
/// </summary>
public sealed class FeatureFlagJsonConverter : JsonConverter<FeatureFlag>
{
    public override FeatureFlag Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var flag = new FeatureFlag();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                break;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            var propertyName = reader.GetString();
            reader.Read();

            switch (propertyName?.ToLower())
            {
                case "id":
                    flag.Id = reader.GetInt32();
                    break;
                case "key":
                    flag.Key = reader.GetString() ?? string.Empty;
                    break;
                case "displayname":
                    flag.DisplayName = reader.GetString() ?? string.Empty;
                    break;
                case "description":
                    flag.Description = reader.GetString() ?? string.Empty;
                    break;
                case "isenabled":
                    flag.IsEnabled = reader.GetBoolean();
                    break;
                case "rollouttype":
                    flag.RolloutType = (Enums.RolloutType)reader.GetInt32();
                    break;
                case "percentagerollout":
                    if (reader.TokenType != JsonTokenType.Null)
                    {
                        flag.PercentageRollout = reader.GetInt32();
                    }
                    break;
                case "createdat":
                    if (reader.TryGetDateTime(out var createdAt))
                    {
                        flag.CreatedAt = createdAt;
                    }
                    break;
                case "updatedat":
                    if (reader.TryGetDateTime(out var updatedAt))
                    {
                        flag.UpdatedAt = updatedAt;
                    }
                    break;
                case "createdby":
                    flag.CreatedBy = reader.GetString() ?? string.Empty;
                    break;
                case "updatedby":
                    flag.UpdatedBy = reader.GetString() ?? string.Empty;
                    break;
            }
        }

        return flag;
    }

    public override void Write(Utf8JsonWriter writer, FeatureFlag value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartObject();

        writer.WriteNumber("id", value.Id);
        writer.WriteString("key", value.Key);
        writer.WriteString("displayName", value.DisplayName);
        writer.WriteString("description", value.Description);
        writer.WriteBoolean("isEnabled", value.IsEnabled);
        writer.WriteNumber("rolloutType", (int)value.RolloutType);

        if (value.PercentageRollout.HasValue)
        {
            writer.WriteNumber("percentageRollout", value.PercentageRollout.Value);
        }
        else
        {
            writer.WriteNull("percentageRollout");
        }

        writer.WriteString("createdAt", value.CreatedAt.ToString("O"));
        writer.WriteString("updatedAt", value.UpdatedAt.ToString("O"));
        writer.WriteString("createdBy", value.CreatedBy);
        writer.WriteString("updatedBy", value.UpdatedBy);

        // Include rules count
        writer.WriteNumber("rulesCount", value.Rules?.Count ?? 0);

        // Include variants count
        writer.WriteNumber("variantsCount", value.Variants?.Count ?? 0);

        writer.WriteEndObject();
    }
}

/// <summary>
/// Factory for creating JSON serializer options with custom converters and settings.
/// Ensures consistent JSON formatting across the application.
/// </summary>
public static class JsonFormatterOptions
{
    public static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };

        options.Converters.Add(new FeatureFlagJsonConverter());
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        return options;
    }

    public static JsonSerializerOptions CreateCompactOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        options.Converters.Add(new FeatureFlagJsonConverter());
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        return options;
    }
}
