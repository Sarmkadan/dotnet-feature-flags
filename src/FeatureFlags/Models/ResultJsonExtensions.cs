#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace FeatureFlags.Models;

/// <summary>
/// Provides JSON serialization and deserialization extensions for the <see cref="Result"/> type.
/// </summary>
public static class ResultJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		WriteIndented = false,
		PropertyNameCaseInsensitive = true,
	};

	/// <summary>
	/// Serializes the result to a JSON string.
	/// </summary>
	/// <param name="value">The result to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the result.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this Result value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonOptions)
			{
				WriteIndented = true
			}
			: _jsonOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="Result"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>A <see cref="Result"/> instance if successful; otherwise, null.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static Result? FromJson(string? json)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}

		try
		{
			return JsonSerializer.Deserialize<Result>(json, _jsonOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="Result"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized result if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
	public static bool TryFromJson(string? json, out Result? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		value = null;

		if (string.IsNullOrWhiteSpace(json))
		{
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<Result>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			return false;
		}
	}
}