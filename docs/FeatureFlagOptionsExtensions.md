# FeatureFlagOptionsExtensions

Provides utility methods for validating, cloning, merging, and inspecting `FeatureFlagOptions` instances. This static class centralizes common operations performed on feature flag configuration objects, ensuring consistency across the application.

## API

### Validate

```csharp
public static void Validate(this FeatureFlagOptions options)
```

Validates the supplied `FeatureFlagOptions` instance, ensuring all required properties are set and their values fall within acceptable ranges. Throws an `ArgumentException` or `ArgumentNullException` if validation fails. This method is intended to be called before the options are used to initialize a feature flag service.

**Parameters:**
- `options` — The `FeatureFlagOptions` instance to validate.

**Returns:** Nothing.

**Exceptions:**
- `ArgumentNullException` — Thrown when `options` is `null`.
- `ArgumentException` — Thrown when one or more properties contain invalid values (e.g., negative cache durations, missing required endpoints).

---

### Clone

```csharp
public static FeatureFlagOptions Clone(this FeatureFlagOptions options)
```

Creates a deep copy of the given `FeatureFlagOptions` instance. The returned object is a completely independent clone; modifications to the original or the clone do not affect each other.

**Parameters:**
- `options` — The `FeatureFlagOptions` instance to clone. Must not be `null`.

**Returns:** A new `FeatureFlagOptions` instance with identical property values.

**Exceptions:**
- `ArgumentNullException` — Thrown when `options` is `null`.

---

### MergeWith

```csharp
public static FeatureFlagOptions MergeWith(this FeatureFlagOptions baseOptions, FeatureFlagOptions overrideOptions)
```

Merges two `FeatureFlagOptions` instances, producing a new instance where properties from `overrideOptions` take precedence over those in `baseOptions`. Properties not explicitly set in `overrideOptions` retain their values from `baseOptions`. The original instances remain unchanged.

**Parameters:**
- `baseOptions` — The baseline configuration. Must not be `null`.
- `overrideOptions` — The overriding configuration. Must not be `null`.

**Returns:** A new `FeatureFlagOptions` instance representing the merged result.

**Exceptions:**
- `ArgumentNullException` — Thrown when either parameter is `null`.

---

### IsAuditLoggingConfigured

```csharp
public static bool IsAuditLoggingConfigured(this FeatureFlagOptions options)
```

Determines whether audit logging has been properly configured within the given `FeatureFlagOptions`. Returns `true` if all necessary audit logging settings are present and valid; otherwise, `false`.

**Parameters:**
- `options` — The `FeatureFlagOptions` instance to inspect. Must not be `null`.

**Returns:** `true` if audit logging is configured; `false` otherwise.

**Exceptions:**
- `ArgumentNullException` — Thrown when `options` is `null`.

---

### GetCacheDurationSeconds

```csharp
public static int GetCacheDurationSeconds(this FeatureFlagOptions options)
```

Extracts the cache duration in seconds from the provided `FeatureFlagOptions`. Returns a non-negative integer representing the number of seconds cached feature flag evaluations should be retained.

**Parameters:**
- `options` — The `FeatureFlagOptions` instance to read from. Must not be `null`.

**Returns:** An `int` representing the cache duration in seconds (zero or greater).

**Exceptions:**
- `ArgumentNullException` — Thrown when `options` is `null`.

---

## Usage

### Example 1: Validating and Cloning Options Before Service Initialization

```csharp
FeatureFlagOptions originalOptions = new FeatureFlagOptions
{
    CacheDurationSeconds = 300,
    AuditLoggingEnabled = true,
    AuditEndpoint = "https://audit.example.com/log"
};

// Validate before use
originalOptions.Validate();

// Create a defensive copy for a scoped service
FeatureFlagOptions scopedOptions = originalOptions.Clone();
scopedOptions.CacheDurationSeconds = 120; // Override for this scope only

// Original remains unchanged
Console.WriteLine(originalOptions.CacheDurationSeconds); // 300
Console.WriteLine(scopedOptions.CacheDurationSeconds);   // 120
```

### Example 2: Merging Base Configuration with Environment-Specific Overrides

```csharp
FeatureFlagOptions baseOptions = new FeatureFlagOptions
{
    CacheDurationSeconds = 600,
    AuditLoggingEnabled = false
};

FeatureFlagOptions stagingOverrides = new FeatureFlagOptions
{
    AuditLoggingEnabled = true,
    AuditEndpoint = "https://staging-audit.example.com/log"
};

FeatureFlagOptions mergedOptions = baseOptions.MergeWith(stagingOverrides);

Console.WriteLine(mergedOptions.CacheDurationSeconds);    // 600 (from base)
Console.WriteLine(mergedOptions.AuditLoggingEnabled);     // true (from override)
Console.WriteLine(mergedOptions.AuditEndpoint);           // "https://staging-audit.example.com/log"

if (mergedOptions.IsAuditLoggingConfigured())
{
    int cacheSeconds = mergedOptions.GetCacheDurationSeconds();
    Console.WriteLine($"Audit logging active with {cacheSeconds}s cache.");
}
```

## Notes

- All methods throw `ArgumentNullException` when receiving `null` arguments. Callers should perform null checks before invocation if the origin of the options instance is uncertain.
- `Clone` performs a deep copy. If `FeatureFlagOptions` contains reference-type properties, those are recursively cloned to prevent shared references.
- `MergeWith` treats unset or default-valued properties in `overrideOptions` as non-overriding; only explicitly configured values take precedence. The exact definition of "unset" depends on the internal design of `FeatureFlagOptions` (e.g., nullable fields or sentinel defaults).
- `IsAuditLoggingConfigured` may return `false` even when `AuditLoggingEnabled` is `true` if other required audit settings (such as endpoints or credentials) are missing or invalid.
- `GetCacheDurationSeconds` returns the raw configured value. It does not account for runtime adjustments or external caching layers. A return value of zero typically indicates caching is disabled.
- These methods are static extension methods and are inherently thread-safe, provided the `FeatureFlagOptions` instances passed to them are not concurrently mutated during execution. `Clone` and `MergeWith` operate on snapshots and return new instances, avoiding shared-state issues.
