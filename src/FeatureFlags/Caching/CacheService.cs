#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Distributed;

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
public sealed class InMemoryCacheService : ICacheService, IDisposable {
    private const string DefaultTtlValidationMessage = "Default TTL must be greater than zero.";
    private const string CacheKeyValidationMessage = "Cache key cannot be null or whitespace.";
    private const string TtlValidationMessage = "TTL must be greater than zero.";
    private const string CacheSetLogMessage = "Cache SET: {Key} (TTL: {Ttl}ms)";
    private const string CacheRemoveLogMessage = "Cache REMOVE: {Key}";
    private const string CacheClearedLogMessage = "Cache cleared ({Count} entries removed)";
    private const string CacheCleanupLogMessage = "Cache cleanup: removed {Count} expired entries";
    private const string CacheCleanupStoppingLogMessage = "Cache cleanup task stopping due to cancellation";
    private const string CacheCleanupErrorLogMessage = "Cache cleanup error";
    private const int DefaultTtlMinutes = 5;
    private const int CleanupIntervalMinutes = 1;

    private readonly ConcurrentDictionary<string, CacheEntry> _cache;
    private readonly ILogger<InMemoryCacheService> _logger;
    private readonly TimeSpan _defaultTtl;
    private readonly CancellationTokenSource _cleanupCts;

    public InMemoryCacheService(ILogger<InMemoryCacheService> logger, TimeSpan? defaultTtl = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        if (defaultTtl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultTtl), DefaultTtlValidationMessage);
        }

        _cache = new ConcurrentDictionary<string, CacheEntry>();
        _logger = logger;
        _defaultTtl = defaultTtl ?? TimeSpan.FromMinutes(DefaultTtlMinutes);
        _cleanupCts = new CancellationTokenSource();

        // Start cleanup task with cancellation support
        _ = StartCleanupTaskAsync(_cleanupCts.Token);
    }

    public T? Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(CacheKeyValidationMessage, nameof(key));
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
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(CacheKeyValidationMessage, nameof(key));
        }

        // Simulate async operation
        await Task.Yield();
        return Get<T>(key);
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(CacheKeyValidationMessage, nameof(key));
        }

        ArgumentNullException.ThrowIfNull(value);
        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), TtlValidationMessage);
        }

        var actualTtl = ttl ?? _defaultTtl;
        var entry = new CacheEntry
        {
            Value = value,
            ExpiresAt = DateTime.UtcNow.Add(actualTtl),
            CreatedAt = DateTime.UtcNow
        };

        _cache.AddOrUpdate(key, entry, (_, _) => entry);
        _logger.LogDebug(CacheSetLogMessage, key, actualTtl.TotalMilliseconds);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(CacheKeyValidationMessage, nameof(key));
        }

        ArgumentNullException.ThrowIfNull(value);
        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), TtlValidationMessage);
        }

        await Task.Yield();
        Set(key, value, ttl);
    }

    public void Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(CacheKeyValidationMessage, nameof(key));
        }

        if (_cache.TryRemove(key, out _))
        {
            _logger.LogDebug(CacheRemoveLogMessage, key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(CacheKeyValidationMessage, nameof(key));
        }

        await Task.Yield();
        Remove(key);
    }

    public void Clear()
    {
        var count = _cache.Count;
        _cache.Clear();
        _logger.LogInformation(CacheClearedLogMessage, count);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        Clear();
    }

    /// <summary>
    /// Periodically removes expired cache entries to prevent memory bloat.
    /// </summary>
    private async Task StartCleanupTaskAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(CleanupIntervalMinutes), stoppingToken);

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
                    _logger.LogDebug(CacheCleanupLogMessage, removedCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(CacheCleanupStoppingLogMessage);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, CacheCleanupErrorLogMessage);
            }
        }
    }

    public void Dispose()
    {
        _cleanupCts.Cancel();
        _cleanupCts.Dispose();
    }

    Task ICacheService.RemoveAsync(string key) => RemoveAsync(key);
    Task ICacheService.ClearAsync() => ClearAsync();

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
public sealed class DistributedCacheService : ICacheService {
    private const string DefaultTtlValidationMessage = "Default TTL must be greater than zero.";
    private const string CacheKeyValidationMessage = "Cache key cannot be null or whitespace.";
    private const string TtlValidationMessage = "TTL must be greater than zero.";
    private const string CacheDeserializationErrorLogMessage = "Cache deserialization error for key: {Key}";
    private const string CacheSetLogMessage = "Distributed cache SET: {Key} (TTL: {Ttl}ms)";
    private const string CacheSetErrorLogMessage = "Distributed cache set error for key: {Key}";
    private const string CacheRemoveLogMessage = "Distributed cache REMOVE: {Key}";
    private const string CacheClearWarningLogMessage = "Distributed cache clear requested (full clear may not be supported by all providers)";
    private const int DefaultTtlMinutes = 5;

    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<DistributedCacheService> _logger;
    private readonly TimeSpan _defaultTtl;

    public DistributedCacheService(IDistributedCache distributedCache, ILogger<DistributedCacheService> logger, TimeSpan? defaultTtl = null)
    {
        ArgumentNullException.ThrowIfNull(distributedCache);
        ArgumentNullException.ThrowIfNull(logger);
        if (defaultTtl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultTtl), DefaultTtlValidationMessage);
        }

        _distributedCache = distributedCache;
        _logger = logger;
        _defaultTtl = defaultTtl ?? TimeSpan.FromMinutes(DefaultTtlMinutes);
    }

    public T? Get<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(CacheKeyValidationMessage, nameof(key));
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
            _logger.LogError(ex, CacheDeserializationErrorLogMessage, key);
            return default;
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(CacheKeyValidationMessage, nameof(key));
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
            _logger.LogError(ex, CacheDeserializationErrorLogMessage, key);
            return default;
        }
    }

    public void Set<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(CacheKeyValidationMessage, nameof(key));
        }

        ArgumentNullException.ThrowIfNull(value);
        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), TtlValidationMessage);
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
            _logger.LogDebug(CacheSetLogMessage, key, actualTtl.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, CacheSetErrorLogMessage, key);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(CacheKeyValidationMessage, nameof(key));
        }

        ArgumentNullException.ThrowIfNull(value);
        if (ttl <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(ttl), TtlValidationMessage);
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
            _logger.LogDebug(CacheSetLogMessage, key, actualTtl.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, CacheSetErrorLogMessage, key);
        }
    }

    public void Remove(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(CacheKeyValidationMessage, nameof(key));
        }

        _distributedCache.Remove(key);
        _logger.LogDebug(CacheRemoveLogMessage, key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(CacheKeyValidationMessage, nameof(key));
        }

        await _distributedCache.RemoveAsync(key);
        _logger.LogDebug(CacheRemoveLogMessage, key);
    }

    public void Clear()
    {
        _logger.LogWarning(CacheClearWarningLogMessage);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        Clear();
    }

    Task ICacheService.RemoveAsync(string key) => RemoveAsync(key);
    Task ICacheService.ClearAsync() => ClearAsync();
}
