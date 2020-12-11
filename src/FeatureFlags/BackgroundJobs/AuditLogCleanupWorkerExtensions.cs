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
    public static IServiceCollection AddAuditLogCleanupWorker(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

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
    public static IServiceCollection AddAuditLogCleanupWorker(
        this IServiceCollection services,
        Action<AuditLogCleanupOptions> configureOptions)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configureOptions == null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        services.Configure(configureOptions);
        services.AddHostedService<AuditLogCleanupWorker>();

        return services;
    }

    /// <summary>
    /// Creates a new instance of AuditLogCleanupOptions with the specified retention period.
    /// </summary>
    /// <param name="retentionDays">Number of days to retain audit logs</param>
    /// <returns>Configured AuditLogCleanupOptions instance</returns>
    public static AuditLogCleanupOptions WithRetentionDays(this AuditLogCleanupOptions options, int retentionDays)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.RetentionDays = retentionDays;
        return options;
    }

    /// <summary>
    /// Creates a new instance of AuditLogCleanupOptions with the specified cleanup interval.
    /// </summary>
    /// <param name="intervalHours">Interval in hours between cleanup operations</param>
    /// <returns>Configured AuditLogCleanupOptions instance</returns>
    public static AuditLogCleanupOptions WithCleanupIntervalHours(this AuditLogCleanupOptions options, int intervalHours)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.CleanupIntervalHours = intervalHours;
        return options;
    }

    /// <summary>
    /// Creates a new instance of AuditLogCleanupOptions with the specified enabled state.
    /// </summary>
    /// <param name="enabled">Whether the worker should be enabled</param>
    /// <returns>Configured AuditLogCleanupOptions instance</returns>
    public static AuditLogCleanupOptions WithEnabled(this AuditLogCleanupOptions options, bool enabled)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.Enabled = enabled;
        return options;
    }

    /// <summary>
    /// Gets the effective cleanup interval in seconds for monitoring purposes.
    /// </summary>
    /// <param name="worker">The audit log cleanup worker instance</param>
    /// <returns>Cleanup interval in seconds</returns>
    public static int GetCleanupIntervalSeconds(this AuditLogCleanupWorker worker)
    {
        if (worker == null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        // Access the options through the worker's field
        var optionsField = typeof(AuditLogCleanupWorker).GetField("_options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (optionsField != null)
        {
            var options = optionsField.GetValue(worker) as AuditLogCleanupOptions;
            if (options != null)
            {
                return options.CleanupIntervalHours * 3600;
            }
        }

        // Fallback to default value
        return 24 * 3600;
    }

    /// <summary>
    /// Gets the effective retention period in days.
    /// </summary>
    /// <param name="worker">The audit log cleanup worker instance</param>
    /// <returns>Retention period in days</returns>
    public static int GetRetentionDays(this AuditLogCleanupWorker worker)
    {
        if (worker == null)
        {
            throw new ArgumentNullException(nameof(worker));
        }

        // Access the options through the worker's field
        var optionsField = typeof(AuditLogCleanupWorker).GetField("_options", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (optionsField != null)
        {
            var options = optionsField.GetValue(worker) as AuditLogCleanupOptions;
            if (options != null)
            {
                return options.RetentionDays;
            }
        }

        // Fallback to default value
        return 90;
    }
}