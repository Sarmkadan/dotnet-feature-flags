# ConversionUtilities

`ConversionUtilities` is a static utility class that provides a comprehensive set of methods for type conversion, serialization-like object-to-dictionary mapping, deep cloning, and enum conversion. It is designed to simplify common transformation tasks in feature flag management and general application code, offering null-safe operations with generic type inference.

## API

### `ConvertTo<T>`

```csharp
public static T? ConvertTo<T>(object? value)
```

Attempts to convert an arbitrary object to the specified type `T`. Returns the converted value if successful, or `default(T)` (which is `null` for reference types) if the conversion fails or the input is null. This method handles common conversions such as strings to numeric types, strings to booleans, and strings to enums. It does not throw exceptions for failed conversions; instead, it returns the default value.

**Parameters:**
- `value` (`object?`): The source object to convert.

**Returns:**
- `T?`: The converted value, or the default value of `T` if conversion is not possible.

**Throws:**
- No exceptions are thrown by design; all conversion failures are handled gracefully.

---

### `ConvertToString`

```csharp
public static string? ConvertToString(object? value)
```

Converts an object to its string representation. Returns `null` if the input is `null`. For non-null inputs, this method typically calls `ToString()` on the object, but may apply special handling for certain types to produce a more meaningful or invariant string representation.

**Parameters:**
- `value` (`object?`): The object to convert to a string.

**Returns:**
- `string?`: The string representation, or `null` if the input is `null`.

**Throws:**
- No exceptions are thrown under normal circumstances.

---

### `DictionaryToObject<T>`

```csharp
public static T? DictionaryToObject<T>(Dictionary<string, object?> dict) where T : class, new()
```

Constructs an instance of `T` from a dictionary of property names and values. Each key in the dictionary is matched to a public property of `T` (case-insensitive by default), and the corresponding value is converted to the property's type using `ConvertTo<T>`. Properties not present in the dictionary retain their default values.

**Parameters:**
- `dict` (`Dictionary<string, object?>`): The dictionary containing property names and their intended values.

**Returns:**
- `T?`: A new instance of `T` populated from the dictionary, or `null` if `dict` is `null`.

**Type Constraints:**
- `T` must be a reference type with a parameterless constructor.

**Throws:**
- No exceptions are thrown; property mismatches or conversion failures result in default values for those properties.

---

### `ObjectToDictionary`

```csharp
public static Dictionary<string, object?> ObjectToDictionary(object? obj)
```

Flattens a public object's readable properties into a dictionary where each key is the property name and each value is the property's current value. Returns an empty dictionary if the input object is `null`.

**Parameters:**
- `obj` (`object?`): The object to serialize into a dictionary.

**Returns:**
- `Dictionary<string, object?>`: A dictionary of property names to their values. Never returns `null`; returns an empty dictionary for `null` input.

**Throws:**
- No exceptions are thrown.

---

### `ObjectsToDictionaries<T>`

```csharp
public static List<Dictionary<string, object?>> ObjectsToDictionaries<T>(IEnumerable<T>? objects)
```

Converts a collection of objects into a list of dictionaries, applying `ObjectToDictionary` to each element. Returns an empty list if the input collection is `null`.

**Parameters:**
- `objects` (`IEnumerable<T>?`): The collection of objects to convert.

**Returns:**
- `List<Dictionary<string, object?>>`: A list of dictionaries, one per input object. Never returns `null`.

**Throws:**
- No exceptions are thrown.

---

### `ConvertEnum<TIn, TOut>`

```csharp
public static TOut? ConvertEnum<TIn, TOut>(TIn value) where TIn : struct, Enum where TOut : struct, Enum
```

Converts an enum value from one enum type to another by matching underlying numeric values or names. Returns `null` if no corresponding value exists in the target enum type.

**Parameters:**
- `value` (`TIn`): The source enum value to convert.

**Returns:**
- `TOut?`: The matching enum value of the target type, or `null` if no match is found.

**Type Constraints:**
- Both `TIn` and `TOut` must be enum types.

**Throws:**
- No exceptions are thrown; unmatched values return `null`.

---

### `ConvertCollection<T>`

```csharp
public static List<T> ConvertCollection<T>(IEnumerable? source)
```

Converts each element in a source collection to type `T` using `ConvertTo<T>`, collecting the results into a new list. Elements that fail conversion are omitted from the result. Returns an empty list if the source is `null`.

**Parameters:**
- `source` (`IEnumerable?`): The source collection of objects to convert.

**Returns:**
- `List<T>`: A list of successfully converted elements. Never returns `null`.

**Throws:**
- No exceptions are thrown.

---

### `DeepClone<T>`

```csharp
public static T? DeepClone<T>(T? source) where T : class
```

Performs a deep copy of an object by serializing it to a dictionary (using `ObjectToDictionary`) and then reconstructing a new instance (using `DictionaryToObject<T>`). Returns `null` if the source is `null`.

**Parameters:**
- `source` (`T?`): The object to clone.

**Returns:**
- `T?`: A new instance of `T` that is a deep copy of the source, or `null` if the source is `null`.

**Type Constraints:**
- `T` must be a reference type with a parameterless constructor.

**Throws:**
- No exceptions are thrown; if reconstruction fails, `null` is returned.

---

### `CanConvertTo<T>`

```csharp
public static bool CanConvertTo<T>(object? value)
```

Tests whether a given value can be successfully converted to type `T` without actually performing the full conversion. Returns `true` if the conversion is possible, `false` otherwise.

**Parameters:**
- `value` (`object?`): The value to test for convertibility.

**Returns:**
- `bool`: `true` if the value can be converted to `T`; otherwise `false`.

**Throws:**
- No exceptions are thrown.

---

## Usage

### Example 1: Converting Feature Flag Values

```csharp
// Retrieve a raw flag value from a configuration source
object rawValue = "true";

// Check if the value can be interpreted as a boolean
if (ConversionUtilities.CanConvertTo<bool>(rawValue))
{
    bool? flagValue = ConversionUtilities.ConvertTo<bool>(rawValue);
    Console.WriteLine($"Feature enabled: {flagValue}");
}

// Convert a collection of string numbers to integers
IEnumerable stringNumbers = new[] { "1", "2", "invalid", "4" };
List<int> numbers = ConversionUtilities.ConvertCollection<int>(stringNumbers);
// numbers contains [1, 2, 4] — "invalid" is omitted
```

### Example 2: Cloning and Serializing Feature Flag Configurations

```csharp
public class FeatureConfig
{
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public int Threshold { get; set; }
}

// Original configuration object
var original = new FeatureConfig
{
    Name = "NewDashboard",
    Enabled = true,
    Threshold = 75
};

// Deep clone to avoid mutating the original
FeatureConfig? clone = ConversionUtilities.DeepClone(original);

// Serialize a list of configurations to dictionaries for logging or storage
var configs = new List<FeatureConfig> { original, clone };
List<Dictionary<string, object?>> dicts = ConversionUtilities.ObjectsToDictionaries(configs);

// Reconstruct an object from a dictionary
var dict = new Dictionary<string, object?>
{
    { "Name", "OldDashboard" },
    { "Enabled", false },
    { "Threshold", 50 }
};
FeatureConfig? reconstructed = ConversionUtilities.DictionaryToObject<FeatureConfig>(dict);
```

---

## Notes

- **Null Handling:** All methods accept `null` inputs and return either `null`, an empty collection, or the default value of the return type. No `NullReferenceException` is thrown from public members.
- **Conversion Failures:** `ConvertTo<T>`, `ConvertCollection<T>`, and `DictionaryToObject<T>` silently skip or default values that cannot be converted. They do not throw exceptions for type mismatches or invalid formats.
- **Deep Cloning Limitations:** `DeepClone<T>` relies on the dictionary round-trip mechanism. It only clones public readable and writable properties. Private fields, indexed properties, and properties without setters are not cloned. The target type must have a parameterless constructor.
- **Enum Conversion:** `ConvertEnum<TIn, TOut>` matches by underlying value first; if no value match exists, it attempts a case-insensitive name match. If neither succeeds, `null` is returned.
- **Thread Safety:** All methods are static and operate on immutable inputs or produce new output instances. They do not mutate shared state and are safe to call concurrently from multiple threads without external synchronization.
- **Performance Considerations:** `ObjectToDictionary` and `DictionaryToObject<T>` use reflection to inspect properties. In performance-critical paths with large object graphs or high call frequency, consider caching property metadata externally.
