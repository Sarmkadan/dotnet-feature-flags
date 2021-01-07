# XmlExporter

`XmlExporter` is a static utility class responsible for serializing and deserializing feature-flag configuration data to and from XML format. It provides methods to export feature flags, audit logs, and rules as XML strings, as well as a parser to reconstruct a list of `FeatureFlag` objects from an XML string.

## API

### `ExportFeatureFlags`

```csharp
public static string ExportFeatureFlags(IEnumerable<FeatureFlag> flags)
```

Serializes a collection of `FeatureFlag` instances into an XML string. Each flag’s properties are mapped to XML elements and attributes.

**Parameters:**
- `flags` (`IEnumerable<FeatureFlag>`) — The feature flags to export. Must not be null.

**Returns:**
- `string` — The XML representation of the supplied feature flags.

**Throws:**
- `ArgumentNullException` — if `flags` is null.
- `InvalidOperationException` — if any flag in the collection contains data that cannot be serialized (e.g., a property value that violates XML content rules).

---

### `ExportAuditLogs`

```csharp
public static string ExportAuditLogs(IEnumerable<AuditLog> logs)
```

Serializes a collection of `AuditLog` entries into an XML string, preserving the chronological order of the input sequence.

**Parameters:**
- `logs` (`IEnumerable<AuditLog>`) — The audit log entries to export. Must not be null.

**Returns:**
- `string` — The XML representation of the supplied audit logs.

**Throws:**
- `ArgumentNullException` — if `logs` is null.
- `InvalidOperationException` — if any log entry contains non-serializable data.

---

### `ExportRules`

```csharp
public static string ExportRules(IEnumerable<Rule> rules)
```

Serializes a collection of `Rule` objects into an XML string. Rules are exported with their conditions, targets, and metadata intact.

**Parameters:**
- `rules` (`IEnumerable<Rule>`) — The rules to export. Must not be null.

**Returns:**
- `string` — The XML representation of the supplied rules.

**Throws:**
- `ArgumentNullException` — if `rules` is null.
- `InvalidOperationException` — if any rule contains non-serializable data.

---

### `ParseFeatureFlags`

```csharp
public static List<FeatureFlag> ParseFeatureFlags(string xml)
```

Deserializes an XML string into a list of `FeatureFlag` objects. The XML must conform to the schema produced by `ExportFeatureFlags`.

**Parameters:**
- `xml` (`string`) — The XML string to parse. Must not be null or empty.

**Returns:**
- `List<FeatureFlag>` — The deserialized feature flags in the order they appear in the XML.

**Throws:**
- `ArgumentNullException` — if `xml` is null.
- `ArgumentException` — if `xml` is empty or consists only of whitespace.
- `XmlException` — if the input is not well-formed XML.
- `InvalidOperationException` — if the XML is well-formed but does not match the expected schema.

## Usage

### Example 1: Exporting and re-importing feature flags

```csharp
var flags = new List<FeatureFlag>
{
    new FeatureFlag("dark-mode", true, "Global dark mode rollout"),
    new FeatureFlag("beta-search", false, "New search engine")
};

// Export to XML
string xml = XmlExporter.ExportFeatureFlags(flags);
Console.WriteLine(xml);

// Later: parse back into objects
List<FeatureFlag> restored = XmlExporter.ParseFeatureFlags(xml);
foreach (var flag in restored)
{
    Console.WriteLine($"{flag.Name} = {flag.Enabled}");
}
```

### Example 2: Exporting audit logs alongside rules

```csharp
var logs = AuditService.GetRecentLogs();
var rules = RuleEngine.GetActiveRules();

// Persist both as XML strings
string logsXml = XmlExporter.ExportAuditLogs(logs);
string rulesXml = XmlExporter.ExportRules(rules);

// Write to files or transmit over a wire
await File.WriteAllTextAsync("audit.xml", logsXml);
await File.WriteAllTextAsync("rules.xml", rulesXml);
```

## Notes

- All export methods perform the serialization synchronously and are not thread-safe only in the sense that they do not protect the collections passed to them from concurrent modification. If a collection is mutated while an export is in progress, the resulting XML may be inconsistent or an exception may be thrown.
- `ParseFeatureFlags` is a pure deserialization method; it does not mutate any shared state and is safe to call concurrently from multiple threads provided each call supplies its own XML string.
- The XML format used by the export methods is an internal implementation detail. Consumers should not attempt to modify the XML manually and should always use `ParseFeatureFlags` to reconstruct objects, as schema changes between versions are not guaranteed to be backward-compatible.
- When `ParseFeatureFlags` encounters an XML document that is well-formed but contains unexpected elements or attributes, it throws `InvalidOperationException` rather than silently ignoring the unrecognized content. This ensures that partial or corrupted data is not inadvertently loaded.
