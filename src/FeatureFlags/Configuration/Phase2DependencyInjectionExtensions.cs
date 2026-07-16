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
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> or <paramref name="configuration"/> is null.</exception>
    public static IServiceCollection AddPhase2Services(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Middleware here is convention-based (RequestDelegate ctor), so it is
        // activated by UseMiddleware<T>() in UsePhase2Middleware - registering
        // the types in DI would never resolve (RequestDelegate is not in the
        // container) and is intentionally not done.

        // Register caching
        var cacheProvider = configuration.GetValue("Cache:Provider", "InMemory");
        if (string.Equals(cacheProvider, "distributed", StringComparison.OrdinalIgnoreCase))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                var connection = configuration.GetConnectionString("Redis");
                ArgumentException.ThrowIfNullOrEmpty(connection);
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
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The configured application builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="app"/> is null.</exception>
    public static IApplicationBuilder UsePhase2Middleware(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        app.UseMiddleware<ErrorHandlingMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();
        app.UseMiddleware<AuthenticationMiddleware>();

        return app;
    }

    /// <summary>
    /// Registers webhook and event subscribers.
    /// </summary>
    /// <param name="app">The application builder to configure.</param>
    /// <returns>The configured application builder.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="app"/> is null.</exception>
    public static IApplicationBuilder InitializeEventSubscribers(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

        // Register logging subscriber
        var loggingSubscriber = app.ApplicationServices.GetRequiredService<EventLoggingSubscriber>();
        eventBus.Subscribe(loggingSubscriber);

        // Register webhook subscriber if webhook service is available
        var webhookService = app.ApplicationServices.GetService<IWebhookService>();
        if (webhookService is not null)
        {
            var webhookSubscriber = new WebhookEventSubscriber(
                webhookService,
                app.ApplicationServices.GetRequiredService<ILogger<WebhookEventSubscriber>>());
            eventBus.Subscribe(webhookSubscriber);
        }

        return app;
    }
}
