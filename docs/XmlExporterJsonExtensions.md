# XmlExporterJsonExtensions

Provides extension methods for converting XML-based feature flag artifacts to and from JSON representations. These methods are primarily used for interoperability with systems that consume or produce JSON, while the core feature flag system remains XML-based.

## API

### `ToJson(this IReadOnlyList<FeatureFlag>? flags)`

Serializes a collection of feature flags into a JSON string.

- **Parameters**
  - `flags`: The collection of feature flags to serialize. May be `null`.
- **Return Value**
  - A JSON string representing the serialized feature flags. Returns `null` if `flags` is `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `flags` is `null` when called as an extension method on a non-null instance.

### `FromJsonToFeatureFlags(this string? json)`

Deserializes a JSON string into a collection of feature flags.

- **Parameters**
  - `json`: The JSON string to deserialize. May be `null`.
- **Return Value**
  - A read-only list of deserialized feature flags, or `null` if the input is `null` or invalid.
- **Exceptions**
  - Does not throw exceptions; returns `null` on invalid input.

### `TryFromJson(this string? json, out IReadOnlyList<FeatureFlag>? flags)`

Attempts to deserialize a JSON string into a collection of feature flags.

- **Parameters**
  - `json`: The JSON string to deserialize. May be `null`.
  - `flags`: Output parameter receiving the deserialized feature flags, or `null` if deserialization fails.
- **Return Value**
  - `true` if deserialization succeeds; otherwise, `false`.
- **Exceptions**
  - Does not throw exceptions.

### `ToJson(this IReadOnlyList<AuditLog>? logs)`

Serializes a collection of audit logs into a JSON string.

- **Parameters**
  - `logs`: The collection of audit logs to serialize. May be `null`.
- **Return Value**
  - A JSON string representing the serialized audit logs. Returns `null` if `logs` is `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `logs` is `null` when called as an extension method on a non-null instance.

### `FromJsonToAuditLogs(this string? json)`

Deserializes a JSON string into a collection of audit logs.

- **Parameters**
  - `json`: The JSON string to deserialize. May be `null`.
- **Return Value**
  - A read-only list of deserialized audit logs, or `null` if the input is `null` or invalid.
- **Exceptions**
  - Does not throw exceptions; returns `null` on invalid input.

### `ToJson(this IReadOnlyList<Rule>? rules)`

Serializes a collection of rules into a JSON string.

- **Parameters**
  - `rules`: The collection of rules to serialize. May be `null`.
  - `json`: The JSON string to deserialize. May be `null`.
- **Return Value**
  - A JSON string representing the serialized rules. Returns `null` if `rules` is `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `rules` is `null` when called as an extension method on a non-null instance.

### `FromJsonToRules(this string? json)`

Deserializes a JSON string into a collection of rules.

- **Parameters**
  - `json`: The JSON string to deserialize. May be `null`.
- **Return Value**
  - A read-only list of deserialized rules, or `null` if the input is `null` or invalid.
- **Exceptions**
  - Does not throw exceptions; returns `null` on invalid input.

## Usage

```csharp
// Example 1: Export feature flags to JSON
var featureFlags = new List<FeatureFlag>
{
    new FeatureFlag { Name = "NewUI", Enabled = true },
    new FeatureFlag { Name = "BetaAccess", Enabled = false }
}.AsReadOnly();

string json = featureFlags.ToJson();
Console.WriteLine(json);

// Example 2: Import audit logs from JSON
string auditLogJson = @"[
    { ""Timestamp"": ""2024-01-01T00:00:00"", ""Action"": ""Create"", ""User"": ""admin"" },
    { ""Timestamp"": ""2024-01-02T00:00:00"", ""Action"": ""Update"", ""User"": ""editor"" }
]";

IReadOnlyList<AuditLog>? logs = auditLogJson.FromJsonToAuditLogs();
if (logs != null)
{
    foreach (var log in logs)
    {
        Console.WriteLine($"{log.Timestamp}: {log.Action} by {log.User}");
    }
}
```

## Notes

- All deserialization methods (`FromJsonToFeatureFlags`, `FromJsonToAuditLogs`, `FromJsonToRules`) return `null` for invalid input rather than throwing exceptions, making them suitable for use in pipelines where error handling is performed separately.
- The `TryFromJson` method provides a non-exceptional path for deserialization attempts, useful in high-throughput scenarios.
- Serialization methods (`ToJson`) throw `ArgumentNullException` when the input collection is `null`, enforcing explicit null handling at the call site.
- These methods are stateless and thread-safe; concurrent calls do not require synchronization.
