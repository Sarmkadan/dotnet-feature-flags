using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FeatureFlags.Exceptions;

/// <summary>
/// Provides JSON serialization and deserialization extensions for <see cref="ValidationException"/>.
/// </summary>
public static class ValidationExceptionJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};

	static ValidationExceptionJsonExtensions()
	{
		_jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
	}

	/// <summary>
	/// Serializes a <see cref="ValidationException"/> instance to a JSON string.
	/// </summary>
	/// <param name="value">The exception to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>A JSON string representation of the exception.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
	public static string ToJson(this ValidationException value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		var options = indented
			? new JsonSerializerOptions(_jsonSerializerOptions)
			{
				WriteIndented = true
			}
			: _jsonSerializerOptions;

		return JsonSerializer.Serialize(value, options);
	}

	/// <summary>
	/// Deserializes a JSON string to a <see cref="ValidationException"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>The deserialized exception instance, or null if the JSON is invalid.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
	public static ValidationException? FromJson(string json)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			return JsonSerializer.Deserialize<ValidationException>(json, _jsonSerializerOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to a <see cref="ValidationException"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">Receives the deserialized exception instance if successful.</param>
	/// <returns>True if deserialization succeeded; otherwise, false.</returns>
	/// <exception cref="ArgumentException">Thrown when <paramref name="json"/> is null or empty.</exception>
	public static bool TryFromJson(string json, out ValidationException? value)
	{
		ArgumentException.ThrowIfNullOrEmpty(json);

		try
		{
			value = JsonSerializer.Deserialize<ValidationException>(json, _jsonSerializerOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}
