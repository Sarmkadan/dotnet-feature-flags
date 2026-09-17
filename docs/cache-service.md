# Cache Service Documentation

## Overview

The cache service provides a flexible caching abstraction for the Feature Flags system, supporting both in-memory and distributed caching scenarios. It allows feature flag evaluation results and configurations to be cached for improved performance.

## Cache Service Abstraction

### ICacheService Interface

The `ICacheService` interface defines the contract for cache operations:

```csharp
public interface ICacheService
{
    T? Get<T>(string key);
    Task<T?> GetAsync<T>(string key);
    void Set<T>(string key, T value, TimeSpan? ttl = null);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
    void Remove(string key);
    Task RemoveAsync(string key);
    void Clear();
    Task ClearAsync();
}
```

### Implementations

Two implementations are provided:

1. **InMemoryCacheService** - For single-server deployments using `ConcurrentDictionary`
2. **DistributedCacheService** - For multi-server deployments using `IDistributedCache` (typically Redis)

## Caching Strategy

### In-Memory Cache (InMemoryCacheService)

- Uses `ConcurrentDictionary<string, CacheEntry>` for thread-safe storage
- Automatic cleanup of expired entries via background task (runs every minute)
- Configurable default TTL (5 minutes by default)
- Thread-safe operations without locking
- Memory-efficient with automatic removal of expired entries

### Distributed Cache (DistributedCacheService)

- Uses `IDistributedCache` abstraction (typically backed by Redis)
- Serializes/deserializes objects using `System.Text.Json`
- UTF-8 encoding for cache values
- Distributed cache entry options with absolute expiration
- Graceful handling of serialization errors

## Cache Keys

The cache service itself is generic and doesn't enforce specific key patterns. However, in the Feature Flags context, the following key patterns are used:

### Feature Flag Cache Keys

When used through the `IFeatureFlagCache` interface (implemented by `FeatureFlagCache`), the following key patterns are employed:

1. **Feature Flag by Key**: `flag:{flagKey}`
   - Used to cache individual feature flag objects
   - Example: `flag:enable-new-ui`

2. **All Feature Flags**: `flags:all`
   - Used to cache the complete list of feature flags
   - Example: `flags:all`

3. **Enabled Feature Flags**: `flags:enabled`
   - Used to cache the list of enabled feature flags
   - Example: `flags:enabled`

### Cache Key Characteristics

- Keys are case-sensitive strings
- Should not contain null, empty, or whitespace-only values
- Recommended to use colon-separated namespaces for organization
- Maximum length depends on the underlying cache provider

## TTL (Time To Live) Configuration

### Default TTL

- Both cache implementations use a default TTL of 5 minutes
- Configurable via constructor parameter
- Can be overridden per cache operation

### TTL Validation

- TTL must be greater than zero
- Zero or negative values throw `ArgumentOutOfRangeException`
- Null TTL uses the default TTL

## Cache Operations

### Get Operations

- `Get<T>(string key)` - Synchronous retrieval
- `GetAsync<T>(string key)` - Asynchronous retrieval
- Returns default(T) if key doesn't exist or entry is expired
- Automatically removes expired entries during retrieval (in-memory cache)

### Set Operations

- `Set<T>(string key, T value, TimeSpan? ttl = null)` - Synchronous storage
- `SetAsync<T>(string key, T value, TimeSpan? ttl = null)` - Asynchronous storage
- Value cannot be null (throws `ArgumentNullException`)
- TTL validation applied
- Logging of cache set operations at Debug level

### Remove Operations

- `Remove(string key)` - Synchronous removal
- `RemoveAsync(string key)` - Asynchronous removal
- Safe to call on non-existent keys
- Logging of cache remove operations at Debug level

### Clear Operations

- `Clear()` - Synchronous clearing of all entries
- `ClearAsync()` - Asynchronous clearing
- In-memory cache: removes all entries and logs count
- Distributed cache: logs warning (full clear may not be supported)

## Background Cleanup (In-Memory Cache Only)

The `InMemoryCacheService` includes a background cleanup task that:

- Runs every minute (configurable via `CleanupIntervalMinutes`)
- Identifies and removes expired cache entries
- Handles cancellation gracefully via `CancellationToken`
- Logs cleanup operations at Debug level
- Handles exceptions without terminating the cleanup loop

## Thread Safety

- **InMemoryCacheService**: Thread-safe via `ConcurrentDictionary`
- **DistributedCacheService**: Depends on the underlying `IDistributedCache` implementation
- All public methods are safe for concurrent access

## Error Handling

- Cache operations catch and log exceptions but don't propagate them
- Get operations return default(T) on error
- Set/Remove/Clear operations log errors but don't throw
- Distributed cache serialization errors are caught and logged
- Background cleanup exceptions are logged but don't stop the cleanup loop

## Logging

Both implementations use `ILogger<T>` for structured logging:

- Cache SET operations: Logged at Debug level with key and TTL
- Cache REMOVE operations: Logged at Debug level with key
- Cache CLEAR operations: Logged at Information level with count
- Cache cleanup operations: Logged at Debug level
- Errors: Logged at Error level with exception details
- Warnings: Logged at Warning level (e.g., distributed cache clear)

## Usage Example

```csharp
// Setting a value
cache.Set("feature_flag:enable_new_ui", true, TimeSpan.FromMinutes(10));

// Getting a value
var isEnabled = cache.Get<bool>("feature_flag:enable_new_ui");

// Asynchronous operations
await cache.SetAsync("feature_flag:beta_feature", false);
var betaEnabled = await cache.GetAsync<bool>("feature_flag:beta_feature");

// Removing a value
cache.Remove("feature_flag:old_feature");

// Clearing all cache
cache.Clear();
```

## Configuration

The cache service is typically configured through dependency injection:

```csharp
// In-memory cache (default)
services.AddSingleton<ICacheService, InMemoryCacheService>();

// Distributed cache (Redis example)
services.AddSingleton<IDistributedCache, RedisCache>();
services.AddSingleton<ICacheService, DistributedCacheService>();

// With custom default TTL
services.AddSingleton<ICacheService>(sp => 
    new InMemoryCacheService(
        sp.GetRequiredService<ILogger<InMemoryCacheService>>(),
        TimeSpan.FromMinutes(10)));
```

## Performance Characteristics

### In-Memory Cache

- Get/Set/O(1) average case for dictionary operations
- Memory usage grows with number of cached items
- Background cleanup prevents unbounded memory growth
- No network latency
- Suitable for single-server applications

### Distributed Cache

- Get/Set performance depends on network and Redis performance
- Serialization/deserialization overhead
- Network latency involved
- Suitable for multi-server/farm deployments
- Automatic sharing across servers

## Limitations

### In-Memory Cache

- Memory limited to single server
- Not suitable for web farms without sticky sessions
- Background cleanup consumes minimal CPU/memory
- Cache not persisted across application restarts

### Distributed Cache

- Requires external cache infrastructure (Redis, etc.)
- Serialization overhead for complex objects
- Network dependency
- Potential for cache stampede if not properly designed

## Security Considerations

- Cache keys should not contain sensitive information
- Cached values are serialized as-is (consider encryption for sensitive data)
- Distributed cache access should be secured according to infrastructure policies
- No built-in encryption or access control in the cache service itself