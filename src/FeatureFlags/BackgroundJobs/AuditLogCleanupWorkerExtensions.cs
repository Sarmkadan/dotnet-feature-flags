#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace FeatureFlags.BackgroundJobs;

/// <summary>
/// Extension methods for <see cref="AuditLogCleanupWorker"/> that provide additional functionality
/// for managing audit log cleanup operations and configuration.
/// </summary>
public static class AuditLogCleanupWorkerExtensions
{
    /// <summary>
    /// Registers the AuditLogCleanupWorker with the service collection using default options.
    /// </summary>
    /// <param name="services">The service collection to register with</param>
    /// <returns>The modified service collection</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null</exception>
    public static IServiceCollection AddAuditLogCleanupWorker(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<AuditLogCleanupOptions>()
            .Configure(options =>
            {
                options.RetentionDays = 90;
                options.CleanupIntervalHours = 24;
                options.Enabled = true;
            });

        services.AddHostedService<AuditLogCleanupWorker>();

        return services;
    }

    /// <summary>
    /// Registers the AuditLogCleanupWorker with custom configuration options.
    /// </summary>
    /// <param name="services">The service collection to register with</param>
    /// <param name="configureOptions">Action to configure the cleanup options</param>
    /// <returns>The modified service collection</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configureOptions"/> is null</exception>
    public static IServiceCollection AddAuditLogCleanupWorker(
        this IServiceCollection services,
        Action<AuditLogCleanupOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddHostedService<AuditLogCleanupWorker>();

        return services;
    }

    /// <summary>
    /// Configures the audit log cleanup with the specified retention period.
    /// </summary>
    /// <param name="options">The cleanup options to configure</param>
    /// <param name="retentionDays">Number of days to retain audit logs</param>
    /// <returns>The configured options instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null</exception>
    public static AuditLogCleanupOptions WithRetentionDays(this AuditLogCleanupOptions options, int retentionDays)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.RetentionDays = retentionDays;
        return options;
    }

    /// <summary>
    /// Configures the audit log cleanup with the specified cleanup interval.
    /// </summary>
    /// <param name="options">The cleanup options to configure</param>
    /// <param name="intervalHours">Interval in hours between cleanup operations</param>
    /// <returns>The configured options instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null</exception>
    public static AuditLogCleanupOptions WithCleanupIntervalHours(this AuditLogCleanupOptions options, int intervalHours)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.CleanupIntervalHours = intervalHours;
        return options;
    }

    /// <summary>
    /// Configures whether the audit log cleanup worker should be enabled.
    /// </summary>
    /// <param name="options">The cleanup options to configure</param>
    /// <param name="enabled">Whether the worker should be enabled</param>
    /// <returns>The configured options instance for method chaining</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null</exception>
    public static AuditLogCleanupOptions WithEnabled(this AuditLogCleanupOptions options, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.Enabled = enabled;
        return options;
    }

    /// <summary>
    /// Gets the effective cleanup interval in seconds for monitoring purposes.
    /// </summary>
    /// <param name="worker">The audit log cleanup worker instance</param>
    /// <returns>Cleanup interval in seconds</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="worker"/> is null</exception>
    public static int GetCleanupIntervalSeconds(this AuditLogCleanupWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        return worker.CleanupIntervalSeconds;
    }

    /// <summary>
    /// Gets the effective retention period in days.
    /// </summary>
    /// <param name="worker">The audit log cleanup worker instance</param>
    /// <returns>Retention period in days</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="worker"/> is null</exception>
    public static int GetRetentionDays(this AuditLogCleanupWorker worker)
    {
        ArgumentNullException.ThrowIfNull(worker);

        return worker.RetentionDays;
    }
}