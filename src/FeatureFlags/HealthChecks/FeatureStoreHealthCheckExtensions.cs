#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace FeatureFlags.HealthChecks;

/// <summary>
/// Extension methods for registering the feature store health check.
/// </summary>
public static class FeatureStoreHealthCheckExtensions
{
    /// <summary>
    /// Adds <see cref="FeatureStoreHealthCheck"/> to the health checks builder.
    /// Requires feature flag services (<c>AddFeatureFlagServices</c>) to be registered.
    /// </summary>
    /// <param name="builder">The health checks builder.</param>
    /// <param name="configure">Optional action to configure <see cref="FeatureStoreHealthCheckOptions"/>.</param>
    /// <param name="name">The health check name. Defaults to <c>feature-store</c>.</param>
    /// <param name="failureStatus">The status reported on failure. Defaults to Unhealthy.</param>
    /// <param name="tags">Optional tags for filtering health endpoints.</param>
    /// <returns>The health checks builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> is null.</exception>
    public static IHealthChecksBuilder AddFeatureFlagHealthCheck(
        this IHealthChecksBuilder builder,
        Action<FeatureStoreHealthCheckOptions>? configure = null,
        string name = "feature-store",
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddOptions<FeatureStoreHealthCheckOptions>()
            .Configure(configure ?? (_ => { }))
            .Validate(options => options.StaleAfter > TimeSpan.Zero, "FeatureStoreHealthCheckOptions.StaleAfter must be greater than zero.")
            .ValidateOnStart();

        builder.Services.TryAddSingleton(TimeProvider.System);
        builder.Services.TryAddSingleton<FeatureStoreRefreshTracker>();

        return builder.AddCheck<FeatureStoreHealthCheck>(name, failureStatus, tags);
    }
}
