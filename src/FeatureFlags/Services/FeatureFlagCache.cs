#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using FeatureFlags.Caching;
using FeatureFlags.Configuration;
using FeatureFlags.Events;
using FeatureFlags.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeatureFlags.Services;

/// <summary>
/// Implementation of feature flag cache that wraps IFeatureFlagService and adds caching behavior.
/// Caches feature flags by key with configurable TTL, and invalidates cache on flag changes.
/// </summary>
public sealed class FeatureFlagCache : IFeatureFlagCache, IEventSubscriber, IDisposable
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ICacheService _cacheService;
    private readonly IEventBus _eventBus;
    private readonly FeatureFlagOptions _options;
    private readonly ILogger<FeatureFlagCache> _logger;
    private readonly CancellationTokenSource _cleanupCts;

    /// <summary>
    /// Cache key prefix to avoid collisions with other cached items.
    /// </summary>
    private const string CacheKeyPrefix = "ff:flag:";

    /// <summary>
    /// Gets the types of events this subscriber is interested in.
    /// </summary>
    public string[] InterestedEventTypes => new[]
    {
        "FeatureFlagCreated",
        "FeatureFlagUpdated",
        "FeatureFlagDeleted",
        "FeatureFlagEnabled",
        "FeatureFlagDisabled"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureFlagCache"/> class.
    /// </summary>
    /// <param name="featureFlagService">The underlying feature flag service.</param>
    /// <param name="cacheService">The cache service for storing feature flags.</param>
    /// <param name="eventBus">The event bus for subscribing to flag change events.</param>
    /// <param name="options">Feature flag configuration options.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public FeatureFlagCache(
        IFeatureFlagService featureFlagService,
        ICacheService cacheService,
        IEventBus eventBus,
        IOptions<FeatureFlagOptions> options,
        ILogger<FeatureFlagCache> logger)
    {
        _featureFlagService = featureFlagService ?? throw new ArgumentNullException(nameof(featureFlagService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Only enable caching if explicitly configured
        if (_options.EnableCache)
        {
            _eventBus.Subscribe(this);
            _cleanupCts = new CancellationTokenSource();
            _ = StartCleanupTaskAsync(_cleanupCts.Token);

            _logger.LogInformation("Feature flag cache enabled with TTL: {CacheDurationMinutes} minutes", _options.CacheDurationMinutes);
        }
        else
        {
            _logger.LogInformation("Feature flag cache is disabled (EnableCache=false)");
            _cleanupCts = new CancellationTokenSource();
        }
    }

    /// <summary>
    /// Gets a feature flag by its unique key from cache if available, otherwise from repository.
    /// </summary>
    /// <param name="key">The key of the feature flag.</param>
    /// <returns>The feature flag if found, otherwise null.</returns>
    public async Task<FeatureFlag?> GetFeatureFlagByKeyAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Feature flag key cannot be empty", nameof(key));
        }

        // Cache is disabled, bypass cache
        if (!_options.EnableCache)
        {
            return await _featureFlagService.GetFeatureFlagByKeyAsync(key);
        }

        var cacheKey = GetCacheKey(key);
        var cachedFlag = _cacheService.Get<FeatureFlag>(cacheKey);

        if (cachedFlag != null)
        {
            _logger.LogDebug("Cache HIT for flag: {Key}", key);
            return cachedFlag;
        }

        _logger.LogDebug("Cache MISS for flag: {Key}", key);

        // Cache miss - fetch from repository
        var featureFlag = await _featureFlagService.GetFeatureFlagByKeyAsync(key);

        if (featureFlag != null)
        {
            // Cache the flag with TTL
            var ttl = TimeSpan.FromMinutes(_options.CacheDurationMinutes);
            _cacheService.Set(cacheKey, featureFlag, ttl);
            _logger.LogDebug("Cached flag {Key} with TTL: {TtlMinutes} minutes", key, _options.CacheDurationMinutes);
        }

        return featureFlag;
    }

    /// <summary>
    /// Invalidates the cache entry for a specific feature flag.
    /// </summary>
    /// <param name="flagId">The ID of the feature flag to invalidate.</param>
    public void Invalidate(int flagId)
    {
        if (flagId <= 0)
        {
            return;
        }

        // Invalidate by ID - we need to find the key first
        // For now, we'll use a wildcard approach to clear all cache entries
        // A more sophisticated approach would track keys by ID
        _cacheService.Remove($"{CacheKeyPrefix}id:{flagId}");
        _logger.LogDebug("Invalidated cache for flag ID: {FlagId}", flagId);
    }

    /// <summary>
    /// Invalidates the cache entry for a specific feature flag by key.
    /// </summary>
    /// <param name="flagKey">The key of the feature flag to invalidate.</param>
    public void Invalidate(string flagKey)
    {
        if (string.IsNullOrWhiteSpace(flagKey))
        {
            return;
        }

        var cacheKey = GetCacheKey(flagKey);
        _cacheService.Remove(cacheKey);
        _logger.LogDebug("Invalidated cache for flag key: {FlagKey}", flagKey);
    }

    /// <summary>
    /// Clears all feature flag cache entries.
    /// </summary>
    public void Clear()
    {
        // Note: In a real distributed cache, we might need a more sophisticated approach
        // For now, we'll just clear all cache entries
        _cacheService.Clear();
        _logger.LogInformation("Cleared all feature flag cache entries");
    }

    /// <summary>
    /// Handles feature flag events and invalidates cache when flags are modified.
    /// </summary>
    /// <param name="@event">The feature flag event.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task HandleEventAsync(FeatureFlagEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        _logger.LogDebug("Received event {EventType} for flag {FlagKey} (ID: {FlagId})",
            @event.EventType, @event.FeatureFlagKey, @event.FeatureFlagId);

        // Invalidate cache based on event type
        switch (@event.EventType)
        {
            case "FeatureFlagCreated":
            case "FeatureFlagUpdated":
            case "FeatureFlagDeleted":
            case "FeatureFlagEnabled":
            case "FeatureFlagDisabled":
                Invalidate(@event.FeatureFlagKey);
                Invalidate(@event.FeatureFlagId);
                break;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Generates a cache key for a feature flag.
    /// </summary>
    /// <param name="flagKey">The feature flag key.</param>
    /// <returns>The cache key.</returns>
    private string GetCacheKey(string flagKey)
    {
        return $"{CacheKeyPrefix}{flagKey}";
    }

    /// <summary>
    /// Periodically cleans up expired cache entries to prevent memory bloat.
    /// </summary>
    private async Task StartCleanupTaskAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

                // The cache service already handles expiration, so we just need to ensure
                // the cleanup task doesn't consume resources unnecessarily
                // We can add additional cleanup logic here if needed
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Feature flag cache cleanup task stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Feature flag cache cleanup error");
            }
        }
    }

    /// <summary>
    /// Disposes the cache and unsubscribes from events.
    /// </summary>
    public void Dispose()
    {
        if (_options.EnableCache)
        {
            _eventBus.Unsubscribe(this);
        }

        _cleanupCts.Cancel();
        _cleanupCts.Dispose();
    }
}
