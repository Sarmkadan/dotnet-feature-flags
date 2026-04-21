#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace FeatureFlags.Caching;

/// <summary>
/// Interface for cache operations with support for different TTL values and cache invalidation.
/// Allows feature flag evaluation results and configurations to be cached for performance.
/// </summary>
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

/// <summary>
/// In-memory implementation of cache service using concurrent dictionary.
/// Suitable for single-server deployments. For distributed scenarios, use DistributedCacheService.
/// </summary>
{public sealed class InMemoryCacheService {
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;
    private readonly ILogger<InMemoryCacheService> _logger;
    private readonly TimeSpan _defaultTtl;

    public InMemoryCacheService(ILogger<InMemoryCacheService> logger, TimeSpan? defaultTtl = null)
    {
        _cache = new ConcurrentDictionary<string, CacheEntry>();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultTtl = defaultTtl ?? TimeSpan.FromMinutes(5);

        // Start cleanup task
        _ = StartCleanupTaskAsync();
    }

    public T? Get<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return default;
        }

        if (_cache.TryGetValue(key, out var entry))
        {
            // Check if expired
            if (entry.ExpiresAt.HasValue && entry.ExpiresAt < DateTime.UtcNow)
            {
                _cache.TryRemove(key, out _);
                return default;
            }

            return (T?)entry.Value;
        }

        return default;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        // Simulate async operation
        await Task.Yield();
        return Get<T>(key);
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        var actualTtl = ttl ?? _defaultTtl;
        var entry = new CacheEntry
        {
            Value = value,
            ExpiresAt = DateTime.UtcNow.Add(actualTtl),
            CreatedAt = DateTime.UtcNow
        };

        _cache.AddOrUpdate(key, entry, (_, _) => entry);
        _logger.LogDebug("Cache SET: {Key} (TTL: {Ttl}ms)", key, actualTtl.TotalMilliseconds);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        await Task.Yield();
        Set(key, value, ttl);
    }

    public void Remove(string key)
    {
        if (_cache.TryRemove(key, out _))
        {
            _logger.LogDebug("Cache REMOVE: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        Remove(key);
    }

    public void Clear()
    {
        var count = _cache.Count;
        _cache.Clear();
        _logger.LogInformation("Cache cleared ({Count} entries removed)", count);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        Clear();
    }

    /// <summary>
    /// Periodically removes expired cache entries to prevent memory bloat.
    /// </summary>
    private async Task StartCleanupTaskAsync()
    {
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1));

                var expiredKeys = _cache
                    .Where(kvp => kvp.Value.ExpiresAt.HasValue && kvp.Value.ExpiresAt < DateTime.UtcNow)
                    .Select(kvp => kvp.Key)
                    .ToList();

                var removedCount = 0;
                foreach (var key in expiredKeys)
                {
                    if (_cache.TryRemove(key, out _))
                    {
                        removedCount++;
                    }
                }

                if (removedCount > 0)
                {
                    _logger.LogDebug("Cache cleanup: removed {Count} expired entries", removedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache cleanup error");
            }
        }
    }

    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

/// <summary>
/// Distributed cache service using IDistributedCache for multi-server deployments.
/// Typically backed by Redis or similar distributed cache.
/// </summary>
{public sealed class DistributedCacheService {
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<DistributedCacheService> _logger;
    private readonly TimeSpan _defaultTtl;

    public DistributedCacheService(IDistributedCache distributedCache, ILogger<DistributedCacheService> logger, TimeSpan? defaultTtl = null)
    {
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultTtl = defaultTtl ?? TimeSpan.FromMinutes(5);
    }

    public T? Get<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return default;
        }

        var data = _distributedCache.Get(key);
        if (data is null)
        {
            return default;
        }

        try
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache deserialization error for key: {Key}", key);
            return default;
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return default;
        }

        var data = await _distributedCache.GetAsync(key);
        if (data is null)
        {
            return default;
        }

        try
        {
            var json = System.Text.Encoding.UTF8.GetString(data);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache deserialization error for key: {Key}", key);
            return default;
        }
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrEmpty(key) || value is null)
        {
            return;
        }

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(value);
            var data = System.Text.Encoding.UTF8.GetBytes(json);
            var actualTtl = ttl ?? _defaultTtl;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = actualTtl
            };

            _distributedCache.Set(key, data, options);
            _logger.LogDebug("Distributed cache SET: {Key} (TTL: {Ttl}ms)", key, actualTtl.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Distributed cache set error for key: {Key}", key);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrEmpty(key) || value is null)
        {
            return;
        }

        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(value);
            var data = System.Text.Encoding.UTF8.GetBytes(json);
            var actualTtl = ttl ?? _defaultTtl;

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = actualTtl
            };

            await _distributedCache.SetAsync(key, data, options);
            _logger.LogDebug("Distributed cache SET: {Key} (TTL: {Ttl}ms)", key, actualTtl.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Distributed cache set error for key: {Key}", key);
        }
    }

    public void Remove(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        _distributedCache.Remove(key);
        _logger.LogDebug("Distributed cache REMOVE: {Key}", key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
        {
            return;
        }

        await _distributedCache.RemoveAsync(key);
        _logger.LogDebug("Distributed cache REMOVE: {Key}", key);
    }

    public void Clear()
    {
        _logger.LogWarning("Distributed cache clear requested (full clear may not be supported by all providers)");
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        Clear();
    }
}
