#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.BackgroundJobs;
using FeatureFlags.Caching;
using FeatureFlags.Events;
using FeatureFlags.Integration;
using FeatureFlags.Middleware;
using FeatureFlags.Repository;

namespace FeatureFlags.Configuration;

/// <summary>
/// Dependency injection extensions for Phase 2 components.
/// Registers all new middleware, services, repositories, and background workers.
/// </summary>
public static class Phase2DependencyInjectionExtensions
{
    /// <summary>
    /// Registers all Phase 2 services including middleware, caching, webhooks, and background jobs.
    /// </summary>
    public static IServiceCollection AddPhase2Services(this IServiceCollection services, IConfiguration configuration)
    {
        // Register middleware
        services.AddSingleton<ErrorHandlingMiddleware>();
        services.AddSingleton<RequestLoggingMiddleware>();
        services.AddSingleton<RateLimitingMiddleware>();
        services.AddSingleton<AuthenticationMiddleware>();

        // Register caching
        var cacheProvider = configuration.GetValue("Cache:Provider", "InMemory").ToLower();
        if (cacheProvider == "distributed")
        {
            services.AddStackExchangeRedisCache(options =>
            {
                var connection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
                options.Configuration = connection;
            });
            services.AddScoped<ICacheService, DistributedCacheService>();
        }
        else
        {
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        }

        // Register webhook services
        services.AddScoped<IWebhookService, WebhookService>();
        services.AddScoped<IWebhookRepository, WebhookRepository>();
        services.AddScoped<IWebhookDeliveryRepository, WebhookDeliveryRepository>();
        services.AddScoped<HttpApiClient>();

        // Register HTTP client factory
        services.AddFeatureFlagHttpClients();

        // Register event system
        services.AddEventSystem();

        // Register background workers
        var auditCleanupOptions = new AuditLogCleanupOptions();
        configuration.GetSection("AuditLogCleanup").Bind(auditCleanupOptions);
        services.AddSingleton(auditCleanupOptions);
        services.AddHostedService<AuditLogCleanupWorker>();

        var webhookRetryOptions = new WebhookRetryOptions();
        configuration.GetSection("WebhookRetry").Bind(webhookRetryOptions);
        services.AddSingleton(webhookRetryOptions);

        if (webhookRetryOptions.Enabled)
        {
            services.AddHostedService<WebhookRetryWorker>();
        }

        var cacheSyncOptions = new CacheSyncOptions();
        configuration.GetSection("CacheSync").Bind(cacheSyncOptions);
        services.AddSingleton(cacheSyncOptions);

        if (cacheSyncOptions.Enabled)
        {
            services.AddHostedService<CacheSyncWorker>();
        }

        // Register rate limiting options
        var rateLimitOptions = new RateLimitOptions();
        configuration.GetSection("RateLimit").Bind(rateLimitOptions);
        services.AddSingleton(rateLimitOptions);

        // Register authentication options
        var authOptions = new AuthenticationOptions();
        configuration.GetSection("Authentication").Bind(authOptions);
        services.AddSingleton(authOptions);

        return services;
    }

    /// <summary>
    /// Adds Phase 2 middleware to the application pipeline.
    /// </summary>
    public static IApplicationBuilder UsePhase2Middleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();
        app.UseMiddleware<AuthenticationMiddleware>();

        return app;
    }

    /// <summary>
    /// Registers webhook and event subscribers.
    /// </summary>
    public static IApplicationBuilder InitializeEventSubscribers(this IApplicationBuilder app)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        // Register logging subscriber
        var loggingSubscriber = app.ApplicationServices.GetRequiredService<EventLoggingSubscriber>();
        eventBus.Subscribe(loggingSubscriber);

        // Register webhook subscriber if webhook service is available
        var webhookService = app.ApplicationServices.GetService<IWebhookService>();
        if (webhookService is not null)
        {
            var webhookSubscriber = new WebhookEventSubscriber(webhookService, app.ApplicationServices.GetRequiredService<ILogger<WebhookEventSubscriber>>());
            eventBus.Subscribe(webhookSubscriber);
        }

        return app;
    }
}

/// <summary>
/// Configuration options container for Phase 2 components.
/// </summary>
public sealed class Phase2Options
{
    /// <summary>
    /// Cache provider type (InMemory or Distributed).
    /// </summary>
    public string CacheProvider { get; set; } = "InMemory";

    /// <summary>
    /// Enable webhook functionality.
    /// </summary>
    public bool EnableWebhooks { get; set; } = true;

    /// <summary>
    /// Enable event system.
    /// </summary>
    public bool EnableEvents { get; set; } = true;

    /// <summary>
    /// Enable background workers.
    /// </summary>
    public bool EnableBackgroundWorkers { get; set; } = true;

    /// <summary>
    /// Enable rate limiting.
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Enable request logging middleware.
    /// </summary>
    public bool EnableRequestLogging { get; set; } = true;

    /// <summary>
    /// Enable API key authentication.
    /// </summary>
    public bool EnableApiKeyAuth { get; set; } = true;
}
