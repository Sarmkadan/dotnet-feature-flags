# FeatureFlagEventExtensions

Provides extension methods for inspecting, transforming, and formatting `FeatureFlagEvent` instances. These methods enable type-safe metadata access, temporal filtering, immutable updates, and diagnostic rendering without mutating the original event.

## API

### `IsType(this FeatureFlagEvent @event, string typeName)`
Determines whether the event matches the specified type name.

**Parameters**  
- `@event` – The event to inspect.  
- `typeName` – The type name to compare against (case-sensitive).

**Returns**  
`true` if `@event.Type` equals `typeName`; otherwise `false`.

**Exceptions**  
Throws `ArgumentNullException` if `@event` is `null` or `typeName` is `null`.

---

### `HasMetadataKey(this FeatureFlagEvent @event, string key)`
Checks whether the event's metadata dictionary contains the given key.

**Parameters**  
- `@event` – The event to inspect.  
- `key` – The metadata key to look for.

**Returns**  
`true` if the metadata contains `key`; otherwise `false`.

**Exceptions**  
Throws `ArgumentNullException` if `@event` is `null` or `key` is `null`.

---

### `GetMetadataValue<T>(this FeatureFlagEvent @event, string key)`
Retrieves the metadata value associated with `key`, cast to type `T`.

**Parameters**  
- `@event` – The event to inspect.  
- `key` – The metadata key.

**Returns**  
The value cast to `T` if present and compatible; otherwise `default(T)`.

**Exceptions**  
Throws `ArgumentNullException` if `@event` is `null` or `key` is `null`.  
Throws `InvalidCastException` if the stored value cannot be cast to `T`.

---

### `WithMetadata(this FeatureFlagEvent @event, string key, object? value)`
Returns a new `FeatureFlagEvent` with the specified metadata key/value added or replaced.

**Parameters**  
- `@event` – The source event.  
- `key` – The metadata key.  
- `value` – The metadata value (may be `null` to remove the key).

**Returns**  
A new `FeatureFlagEvent` instance with updated metadata; the original is unchanged.

**Exceptions**  
Throws `ArgumentNullException` if `@event` is `null` or `key` is `null`.

---

### `WithOccurredAt(this FeatureFlagEvent @event, DateTimeOffset occurredAt)`
Returns a new `FeatureFlagEvent` with the `OccurredAt` timestamp replaced.

**Parameters**  
- `@event` – The source event.  
- `occurredAt` – The new timestamp.

**Returns**  
A new `FeatureFlagEvent` instance with the updated timestamp; the original is unchanged.

**Exceptions**  
Throws `ArgumentNullException` if `@event` is `null`.

---

### `IsTriggeredBy(this FeatureFlagEvent @event, string triggerId)`
Determines whether the event was triggered by the specified identifier.

**Parameters**  
- `@event` – The event to inspect.  
- `triggerId` – The trigger identifier to match against the event's `TriggerId` property.

**Returns**  
`true` if `@event.TriggerId` equals `triggerId`; otherwise `false`.

**Exceptions**  
Throws `ArgumentNullException` if `@event` is `null` or `triggerId` is `null`.

---

### `OccurredBetween(this FeatureFlagEvent @event, DateTimeOffset start, DateTimeOffset end)`
Checks whether the event's `OccurredAt` timestamp falls within the inclusive range `[start, end]`.

**Parameters**  
- `@event` – The event to inspect.  
- `start` – Range start (inclusive).  
- `end` – Range end (inclusive).

**Returns**  
`true` if `start <= @event.OccurredAt <= end`; otherwise `false`.

**Exceptions**  
Throws `ArgumentNullException` if `@event` is `null`.  
Throws `ArgumentException` if `start > end`.

---

### `ToLogString(this FeatureFlagEvent @event)`
Produces a compact, human-readable representation suitable for logging.

**Parameters**  
- `@event` – The event to format.

**Returns**  
A string summarizing the event's type, timestamp, trigger, flag key, and metadata.

**Exceptions**  
Throws `ArgumentNullException` if `@event` is `null`.

---

### `Clone(this FeatureFlagEvent @event)`
Creates a deep copy of the event, including a cloned metadata dictionary.

**Parameters**  
- `@event` – The event to clone.

**Returns**  
A new `FeatureFlagEvent` instance with identical data; returns `null` if `@event` is `null`.

**Exceptions**  
Does not throw.

## Usage

### Filtering and enriching events in a pipeline
```csharp
var filtered = events
    .Where(e => e.IsType("FlagEvaluated") && e.OccurredBetween(windowStart, windowEnd))
    .Select(e => e.WithMetadata("correlationId", correlationId)
                  .WithOccurredAt(DateTimeOffset.UtcNow))
    .ToList();
```

### Safe metadata extraction with fallback for diagnostics
```csharp
foreach (var evt in events)
{
    var userId = evt.GetMetadataValue<string>("userId") ?? "anonymous";
    var flagKey = evt.GetMetadataValue<string>("flagKey") ?? "<unknown>";
    logger.LogInformation("{Event} user={UserId} flag={FlagKey}",
        evt.ToLogString(), userId, flagKey);
}
```

## Notes

- All `With*` methods return new instances; `FeatureFlagEvent` is treated as immutable. The original event is never modified.
- `GetMetadataValue<T>` performs a runtime cast. Prefer `HasMetadataKey` followed by `GetMetadataValue<T>` when the key's presence is uncertain to avoid `InvalidCastException`.
- `Clone` returns `null` for a `null` input, enabling safe chaining: `evt?.Clone()?.WithMetadata(...)`.
- The extensions are pure functions with no shared state; they are thread-safe provided the underlying `FeatureFlagEvent` and its metadata dictionary are not mutated concurrently by other code.
- `OccurredBetween` uses inclusive bounds. For half-open intervals, adjust `end` by subtracting one tick or use separate comparisons.
- `ToLogString` does not serialize the full metadata dictionary; it includes a summary (key count and first few keys) to keep log lines bounded.
