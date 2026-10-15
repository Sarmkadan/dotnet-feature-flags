#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Configuration;
using FeatureFlags.Repository;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeatureFlags.HealthChecks;

/// <summary>
/// Reports the health of the feature flag store.
/// Healthy: the store is reachable and at least one flag is loaded (and, when caching is enabled, refreshed recently).
/// Degraded: no flags are loaded, or the last refresh is older than <see cref="FeatureStoreHealthCheckOptions.StaleAfter"/>.
/// Unhealthy: the store cannot be queried.
/// </summary>
public sealed class FeatureStoreHealthCheck : IHealthCheck
{
    /// <summary>Data key for the number of feature flags in the store.</summary>
    public const string FlagCountKey = "FlagCount";

    /// <summary>Data key for the UTC time of the last flag refresh.</summary>
    public const string LastRefreshUtcKey = "LastRefreshUtc";

    private readonly IFeatureFlagRepository _repository;
    private readonly FeatureStoreRefreshTracker _refreshTracker;
    private readonly IOptions<FeatureFlagOptions> _featureFlagOptions;
    private readonly IOptions<FeatureStoreHealthCheckOptions> _healthCheckOptions;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<FeatureStoreHealthCheck> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureStoreHealthCheck"/> class.
    /// </summary>
    public FeatureStoreHealthCheck(
        IFeatureFlagRepository repository,
        FeatureStoreRefreshTracker refreshTracker,
        IOptions<FeatureFlagOptions> featureFlagOptions,
        IOptions<FeatureStoreHealthCheckOptions> healthCheckOptions,
        TimeProvider timeProvider,
        ILogger<FeatureStoreHealthCheck> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _refreshTracker = refreshTracker ?? throw new ArgumentNullException(nameof(refreshTracker));
        _featureFlagOptions = featureFlagOptions ?? throw new ArgumentNullException(nameof(featureFlagOptions));
        _healthCheckOptions = healthCheckOptions ?? throw new ArgumentNullException(nameof(healthCheckOptions));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var lastRefresh = _refreshTracker.LastRefreshedUtc;

        int flagCount;
        try
        {
            flagCount = await _repository.GetTotalCountAsync();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Feature store health check could not query the store");
            return HealthCheckResult.Unhealthy("Feature store is unreachable.", ex, BuildData(null, lastRefresh));
        }

        var data = BuildData(flagCount, lastRefresh);

        if (flagCount == 0)
        {
            return HealthCheckResult.Degraded("Feature store is reachable but contains no feature flags.", data: data);
        }

        // Refresh age only means something when flags are cached in memory.
        if (!_featureFlagOptions.Value.EnableCache)
        {
            return HealthCheckResult.Healthy($"{flagCount} feature flag(s) loaded. Caching is disabled, so refresh age is not checked.", data);
        }

        var staleAfter = _healthCheckOptions.Value.StaleAfter;

        if (lastRefresh is null)
        {
            return HealthCheckResult.Degraded("No feature flag refresh has been recorded since startup.", data: data);
        }

        var age = _timeProvider.GetUtcNow() - lastRefresh.Value;
        if (age > staleAfter)
        {
            return HealthCheckResult.Degraded(
                $"Last feature flag refresh was {age} ago, exceeding the {staleAfter} threshold.",
                data: data);
        }

        return HealthCheckResult.Healthy($"{flagCount} feature flag(s) loaded.", data);
    }

    private static IReadOnlyDictionary<string, object> BuildData(int? flagCount, DateTimeOffset? lastRefresh)
    {
        return new Dictionary<string, object>
        {
            [FlagCountKey] = flagCount ?? (object)"unknown",
            [LastRefreshUtcKey] = lastRefresh?.ToString("O") ?? "never"
        };
    }
}
