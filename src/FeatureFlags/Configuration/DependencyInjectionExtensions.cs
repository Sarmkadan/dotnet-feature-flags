#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Repository;
using FeatureFlags.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
    public static IServiceCollection AddFeatureFlagServices(this IServiceCollection services, IConfiguration configuration)
    {
        if (services is null)
            throw new ArgumentNullException(nameof(services));

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
