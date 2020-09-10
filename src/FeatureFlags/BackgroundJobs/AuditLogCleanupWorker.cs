#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Services;

namespace FeatureFlags.BackgroundJobs;

/// <summary>
/// Background worker that periodically cleans up old audit logs based on retention policy.
/// Helps manage database size and comply with data retention regulations.
/// </summary>
{public sealed class AuditLogCleanupWorker {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditLogCleanupWorker> _logger;
    private readonly AuditLogCleanupOptions _options;
    private readonly TimeSpan _checkInterval;

    public AuditLogCleanupWorker(
        IServiceProvider serviceProvider,
        ILogger<AuditLogCleanupWorker> logger,
        AuditLogCleanupOptions options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new AuditLogCleanupOptions();
        _checkInterval = TimeSpan.FromHours(_options.CleanupIntervalHours);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Audit log cleanup worker started (runs every {Hours} hours)", _options.CleanupIntervalHours);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_checkInterval, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var auditLogService = scope.ServiceProvider.GetRequiredService<IAuditLogService>();

                var cutoffDate = DateTime.UtcNow.AddDays(-_options.RetentionDays);

                _logger.LogInformation("Starting audit log cleanup (removing logs older than {CutoffDate})", cutoffDate);

                await auditLogService.CleanupOldLogsAsync(cutoffDate);

                _logger.LogInformation("Audit log cleanup completed successfully");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Audit log cleanup worker is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Audit log cleanup error");
                // Continue with next cleanup cycle
            }
        }
    }
}

/// <summary>
/// Configuration options for audit log cleanup.
/// </summary>
public sealed class AuditLogCleanupOptions
{
    public int RetentionDays { get; set; } = 90;
    public int CleanupIntervalHours { get; set; } = 24;
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Background worker that retries failed webhook deliveries.
/// Ensures webhook events are delivered with exponential backoff retry strategy.
/// </summary>
{public sealed class WebhookRetryWorker {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebhookRetryWorker> _logger;
    private readonly WebhookRetryOptions _options;
    private readonly TimeSpan _checkInterval;

    public WebhookRetryWorker(
        IServiceProvider serviceProvider,
        ILogger<WebhookRetryWorker> logger,
        WebhookRetryOptions options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new WebhookRetryOptions();
        _checkInterval = TimeSpan.FromSeconds(_options.CheckIntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Webhook retry worker started (checks every {Seconds} seconds)", _options.CheckIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_checkInterval, stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var webhookService = scope.ServiceProvider.GetService<Integration.IWebhookService>();

                if (webhookService is not null)
                {
                    _logger.LogDebug("Checking for failed webhook deliveries to retry");
                    await webhookService.RetryFailedDeliveriesAsync();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Webhook retry worker is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook retry error");
                // Continue with next retry cycle
            }
        }
    }
}

/// <summary>
/// Configuration options for webhook retries.
/// </summary>
public sealed class WebhookRetryOptions
{
    public int CheckIntervalSeconds { get; set; } = 300; // 5 minutes
    public int MaxRetries { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 60;
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Background worker that periodically synchronizes feature flag cache with database.
/// Ensures cache stays consistent with latest data for high-traffic scenarios.
/// </summary>
{public sealed class CacheSyncWorker {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheSyncWorker> _logger;
    private readonly CacheSyncOptions _options;

    public CacheSyncWorker(
        IServiceProvider serviceProvider,
        ILogger<CacheSyncWorker> logger,
        CacheSyncOptions options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new CacheSyncOptions();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Cache sync worker is disabled");
            return;
        }

        _logger.LogInformation("Cache sync worker started (syncs every {Seconds} seconds)", _options.SyncIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.SyncIntervalSeconds), stoppingToken);

                using var scope = _serviceProvider.CreateScope();
                var cacheService = scope.ServiceProvider.GetService<Caching.ICacheService>();
                var featureFlagService = scope.ServiceProvider.GetRequiredService<IFeatureFlagService>();

                if (cacheService is not null)
                {
                    _logger.LogDebug("Syncing feature flag cache with database");

                    // Get all enabled flags and refresh their cache entries
                    var flags = await featureFlagService.SearchFeatureFlagsAsync(new() { Skip = 0, Take = 1000 });

                    foreach (var flag in flags.Items)
                    {
                        var cacheKey = $"feature_flag_{flag.Key}";
                        await cacheService.SetAsync(cacheKey, flag, TimeSpan.FromMinutes(_options.CacheTtlMinutes));
                    }

                    _logger.LogDebug("Cache sync completed ({Count} flags updated)", flags.Items.Count);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Cache sync worker is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache sync error");
            }
        }
    }
}

/// <summary>
/// Configuration options for cache synchronization.
/// </summary>
public sealed class CacheSyncOptions
{
    public int SyncIntervalSeconds { get; set; } = 300;
    public int CacheTtlMinutes { get; set; } = 5;
    public bool Enabled { get; set; } = false;
}
