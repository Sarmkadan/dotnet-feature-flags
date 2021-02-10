#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Repository;
using FeatureFlags.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        // Register services
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
        services.AddScoped<IRuleEvaluationService, RuleEvaluationService>();
        services.AddScoped<IPercentageRolloutService, PercentageRolloutService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddSingleton<IFlagEvaluationLogService, FlagEvaluationLogService>();

        // Register configuration
        services.Configure<FeatureFlagOptions>(configuration.GetSection("FeatureFlags"));

        return services;
    }
}
