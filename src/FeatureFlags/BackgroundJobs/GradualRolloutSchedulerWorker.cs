// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.BackgroundJobs;

/// <summary>
/// Background worker that periodically evaluates and advances gradual rollout schedules.
/// Iterates over all active time-based rollout strategies and updates feature flag
/// percentage allocations according to elapsed days and configured daily increments.
/// </summary>
public class GradualRolloutSchedulerWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GradualRolloutSchedulerWorker> _logger;
    private readonly GradualRolloutSchedulerOptions _options;
    private readonly TimeSpan _checkInterval;

    public GradualRolloutSchedulerWorker(
        IServiceProvider serviceProvider,
        ILogger<GradualRolloutSchedulerWorker> logger,
        GradualRolloutSchedulerOptions options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new GradualRolloutSchedulerOptions();
        _checkInterval = TimeSpan.FromMinutes(_options.CheckIntervalMinutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Gradual rollout scheduler worker is disabled");
            return;
        }

        _logger.LogInformation(
            "Gradual rollout scheduler worker started (checks every {Minutes} minutes)",
            _options.CheckIntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_checkInterval, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var scheduler = scope.ServiceProvider.GetRequiredService<IGradualRolloutSchedulerService>();

                _logger.LogDebug("Running gradual rollout advancement cycle");

                var updatedCount = await scheduler.ProcessScheduledRolloutsAsync(stoppingToken);

                if (updatedCount > 0)
                    _logger.LogInformation("Rollout advancement cycle complete: {Count} flags updated", updatedCount);
                else
                    _logger.LogDebug("Rollout advancement cycle complete: no flags needed advancement");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Gradual rollout scheduler worker is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gradual rollout scheduler error");
                // Continue with next cycle
            }
        }
    }
}

/// <summary>
/// Configuration options for the gradual rollout scheduler background worker.
/// </summary>
public class GradualRolloutSchedulerOptions
{
    /// <summary>
    /// Interval in minutes between consecutive rollout advancement checks. Defaults to 60 minutes.
    /// </summary>
    public int CheckIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Enables or disables the background scheduling worker entirely.
    /// </summary>
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Extension methods for registering gradual rollout scheduler services with dependency injection.
/// </summary>
public static class GradualRolloutSchedulerExtensions
{
    /// <summary>
    /// Registers <see cref="IGradualRolloutSchedulerService"/> and the background worker that
    /// drives time-based rollout advancement. Configuration is bound from the
    /// <c>GradualRolloutScheduler</c> section of <paramref name="configuration"/>.
    /// </summary>
    public static IServiceCollection AddGradualRolloutScheduler(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        services.AddScoped<IGradualRolloutSchedulerService, GradualRolloutSchedulerService>();

        var options = new GradualRolloutSchedulerOptions();
        configuration.GetSection("GradualRolloutScheduler").Bind(options);
        services.AddSingleton(options);

        if (options.Enabled)
            services.AddHostedService<GradualRolloutSchedulerWorker>();

        return services;
    }
}
