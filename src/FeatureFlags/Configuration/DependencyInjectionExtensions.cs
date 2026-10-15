#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Caching;
using FeatureFlags.HealthChecks;
using FeatureFlags.Repository;
using FeatureFlags.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FeatureFlags.Configuration;

/// <summary>
/// Extension methods for dependency injection configuration.
/// Registers all feature flag services and repositories.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds all feature flag services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured service collection.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> or <paramref name="configuration"/> is null.
    /// </exception>
    public static IServiceCollection AddFeatureFlagServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Register repositories
        services.AddScoped<IFeatureFlagRepository, FeatureFlagRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Register cache service
        services.AddScoped<ICacheService, InMemoryCacheService>();
        services.AddScoped<IFeatureFlagCache, FeatureFlagCache>();

        // Register services
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
        services.AddScoped<IRuleEvaluationService, RuleEvaluationService>();
        services.AddScoped<IPercentageRolloutService, PercentageRolloutService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddSingleton<IFlagEvaluationLogService, FlagEvaluationLogService>();

        // Register refresh tracking used by the feature store health check
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<FeatureStoreRefreshTracker>();

        // Register configuration
        services.Configure<FeatureFlagOptions>(configuration.GetSection("FeatureFlags"));

        return services;
    }

    /// <summary>
    /// Registers <see cref="FeatureFlagOptions"/> with validation that runs at host startup.
    /// A misconfigured option makes the host fail to start, with a message naming the property and value.
    /// </summary>
    /// <param name="services">The service collection to register options with.</param>
    /// <param name="configure">Action that configures the feature flag options.</param>
    /// <returns>The configured service collection.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="services"/> or <paramref name="configure"/> is null.
    /// </exception>
    public static IServiceCollection AddFeatureFlags(this IServiceCollection services, Action<FeatureFlagOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.AddOptions<FeatureFlagOptions>()
            .Configure(configure)
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<FeatureFlagOptions>, FeatureFlagOptionsValidator>();

        return services;
    }
}
