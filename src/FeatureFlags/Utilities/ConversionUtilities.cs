#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections;
using System.Reflection;

namespace FeatureFlags.Utilities;

/// <summary>
/// Utility class for type conversions and transformations between different data types.
/// Provides safe conversion methods that handle null values and type mismatches gracefully.
/// </summary>
public static class ConversionUtilities
{
    /// <summary>
    /// Safely converts string to the specified type, returning default if conversion fails.
    /// </summary>
    public static T? ConvertTo<T>(string? value, T? defaultValue = default)
    {
        if (string.IsNullOrEmpty(value))
        {
            return defaultValue;
        }

        try
        {
            if (typeof(T) == typeof(string))
            {
                return (T?)(object)value;
            }

            if (typeof(T) == typeof(int))
            {
                if (int.TryParse(value, out var intValue))
                {
                    return (T?)(object)intValue;
                }
            }
            else if (typeof(T) == typeof(double))
            {
                if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var doubleValue))
                {
                    return (T?)(object)doubleValue;
                }
            }
            else if (typeof(T) == typeof(bool))
            {
                if (bool.TryParse(value, out var boolValue))
                {
                    return (T?)(object)boolValue;
                }
            }
            else if (typeof(T) == typeof(DateTime))
            {
                if (DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.RoundtripKind, out var dateValue))
                {
                    return (T?)(object)dateValue;
                }
            }
            else if (typeof(T).IsEnum)
            {
                try
                {
                    return (T?)Enum.Parse(typeof(T), value, ignoreCase: true);
                }
                catch (ArgumentException)
                {
                    // fall through to defaultValue below
                }
            }

            return defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Converts an object to string safely, handling null and special types.
    /// </summary>
    public static string? ConvertToString(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is string str)
        {
            return str;
        }

        if (value is DateTime dt)
        {
            return dt.ToString("O");
        }

        if (value is bool b)
        {
            return b.ToString().ToLower();
        }

        if (value is IEnumerable enumerable && !(value is string))
        {
            var items = new List<string>();
            foreach (var item in enumerable)
            {
                items.Add(ConvertToString(item) ?? string.Empty);
            }
            return $"[{string.Join(", ", items)}]";
        }

        return value.ToString();
    }

    /// <summary>
    /// Converts dictionary to strongly-typed object using reflection.
    /// Useful for deserializing dynamic data.
    /// </summary>
    public static T? DictionaryToObject<T>(Dictionary<string, object?> dict) where T : class, new()
    {
        try
        {
            var obj = new T();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                var key = dict.Keys.FirstOrDefault(k => k.Equals(property.Name, StringComparison.OrdinalIgnoreCase));
                if (key is not null && dict.TryGetValue(key, out var value))
                {
                    if (value is not null)
                    {
                        property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                    }
                }
            }

            return obj;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts object to dictionary using reflection.
    /// Useful for serializing objects to dynamic dictionaries.
    /// </summary>
    public static Dictionary<string, object?> ObjectToDictionary(object? obj, bool camelCase = false)
    {
        var dict = new Dictionary<string, object?>();

        if (obj is null)
        {
            return dict;
        }

        var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            if (!property.CanRead)
            {
                continue;
            }

            var key = camelCase
                ? char.ToLower(property.Name[0]) + property.Name.Substring(1)
                : property.Name;

            var value = property.GetValue(obj);
            dict[key] = value;
        }

        return dict;
    }

    /// <summary>
    /// Converts list of objects to list of dictionaries.
    /// </summary>
    public static List<Dictionary<string, object?>> ObjectsToDictionaries<T>(IEnumerable<T> objects, bool camelCase = false)
    {
        return objects.Select(obj => ObjectToDictionary(obj, camelCase)).ToList();
    }

    /// <summary>
    /// Safely converts between enum types.
    /// </summary>
    public static TOut? ConvertEnum<TIn, TOut>(TIn enumValue) where TIn : Enum where TOut : struct, Enum
    {
        try
        {
            var enumName = enumValue.ToString();
            if (Enum.TryParse<TOut>(enumName, ignoreCase: true, out var result))
            {
                return result;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Converts collection to a specific type, filtering invalid items.
    /// </summary>
    public static List<T> ConvertCollection<T>(IEnumerable<object?>? items)
    {
        if (items is null)
        {
            return new List<T>();
        }

        var result = new List<T>();

        foreach (var item in items)
        {
            if (item is T typedItem)
            {
                result.Add(typedItem);
            }
        }

        return result;
    }

    /// <summary>
    /// Deep clones an object using JSON serialization (works for serializable types).
    /// </summary>
    public static T? DeepClone<T>(T obj)
    {
        if (obj is null)
        {
            return default;
        }

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(obj);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Checks if an object can be converted to the specified type.
    /// </summary>
    public static bool CanConvertTo<T>(object? value)
    {
        if (value is null)
        {
            return !typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) is not null;
        }

        try
        {
            Convert.ChangeType(value, typeof(T));
            return true;
        }
        catch
        {
            return false;
        }
    }
}
