# Phase2DependencyInjectionExtensions

The `Phase2DependencyInjectionExtensions` class provides a set of static extension methods and configuration properties designed to streamline the registration of Phase 2 services within the .NET dependency injection container and ASP.NET Core request pipeline. It acts as a centralized entry point for enabling advanced feature flag capabilities, including webhook integrations, event sourcing, background processing, and security middleware, ensuring consistent configuration across the application lifecycle.

## API

### AddPhase2Services
Registers the core Phase 2 services into the specified `IServiceCollection`. This method configures the underlying caching mechanisms, event handlers, and background workers based on the current configuration state.
*   **Parameters**: `IServiceCollection services` - The collection of service descriptors to add services to.
*   **Return Value**: `IServiceCollection` - The same service collection instance to allow for method chaining.
*   **Throws**: `ArgumentNullException` if the `services` argument is null.

### UsePhase2Middleware
Adds the Phase 2 middleware to the application's request pipeline. This middleware is responsible for intercepting requests to evaluate feature flags, enforce rate limiting, and handle API key authentication if enabled.
*   **Parameters**: `IApplicationBuilder app` - The application builder used to configure the pipeline.
*   **Return Value**: `IApplicationBuilder` - The same application builder instance to allow for method chaining.
*   **Throws**: `ArgumentNullException` if the `app` argument is null.

### InitializeEventSubscribers
Scans the application context and initializes subscribers for Phase 2 events. This ensures that any components listening for feature flag change events or system lifecycle events are correctly wired up before the application starts processing traffic.
*   **Parameters**: `IApplicationBuilder app` - The application builder used to resolve scoped services and initialize subscribers.
*   **Return Value**: `IApplicationBuilder` - The same application builder instance to allow for method chaining.
*   **Throws**: `InvalidOperationException` if called before `AddPhase2Services` has been invoked in the service collection.

### CacheProvider
Gets or sets the identifier for the caching mechanism used by the Phase 2 services. This string value determines which distributed or in-memory cache implementation is utilized for storing feature flag states.
*   **Type**: `string`
*   **Remarks**: Must be set before calling `AddPhase2Services` to take effect. If null or empty, a default in-memory provider is typically selected.

### EnableWebhooks
Gets or sets a boolean value indicating whether outgoing webhook notifications are enabled for feature flag changes.
*   **Type**: `bool`
*   **Default**: `false`

### EnableEvents
Gets or sets a boolean value indicating whether the internal event bus is active. When enabled, state changes within the feature flag system raise domain events.
*   **Type**: `bool`
*   **Default**: `false`

### EnableBackgroundWorkers
Gets or sets a boolean value indicating whether background services for asynchronous flag evaluation and cache warming are started.
*   **Type**: `bool`
*   **Default**: `false`

### EnableRateLimiting
Gets or sets a boolean value indicating whether the middleware should enforce rate limiting rules on incoming requests associated with feature evaluations.
*   **Type**: `bool`
*   **Default**: `false`

### EnableRequestLogging
Gets or sets a boolean value indicating whether detailed logging of feature flag evaluation requests is enabled within the middleware.
*   **Type**: `bool`
*   **Default**: `false`

### EnableApiKeyAuth
Gets or sets a boolean value indicating whether API key authentication is required for accessing feature flag management endpoints.
*   **Type**: `bool`
*   **Default**: `false`

## Usage

### Example 1: Basic Configuration
The following example demonstrates a minimal setup where Phase 2 services are added with default caching and only request logging enabled.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DotNetFeatureFlags.Extensions;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure specific flags
        Phase2DependencyInjectionExtensions.CacheProvider = "Memory";
        Phase2DependencyInjectionExtensions.EnableRequestLogging = true;

        // Register services
        builder.Services.AddPhase2Services();

        var app = builder.Build();

        // Add middleware to the pipeline
        app.UsePhase2Middleware();
        
        // Initialize event listeners
        app.InitializeEventSubscribers();

        app.Run();
    }
}
```

### Example 2: Advanced Feature Set
This example enables the full suite of Phase 2 capabilities, including webhooks, background workers, and security features, utilizing a distributed cache provider.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using DotNetFeatureFlags.Extensions;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Define configuration
        Phase2DependencyInjectionExtensions.CacheProvider = "Redis";
        Phase2DependencyInjectionExtensions.EnableWebhooks = true;
        Phase2DependencyInjectionExtensions.EnableEvents = true;
        Phase2DependencyInjectionExtensions.EnableBackgroundWorkers = true;
        Phase2DependencyInjectionExtensions.EnableRateLimiting = true;
        Phase2DependencyInjectionExtensions.EnableApiKeyAuth = true;

        // Register the Phase 2 ecosystem
        services.AddPhase2Services();
        
        // Other service registrations...
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Insert Phase 2 middleware early in the pipeline for auth and rate limiting
        app.UsePhase2Middleware();
        
        // Wire up event subscribers after the service provider is built
        app.InitializeEventSubscribers();

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
```

## Notes

*   **Configuration Timing**: The static boolean properties (e.g., `EnableWebhooks`, `CacheProvider`) must be assigned values prior to invoking `AddPhase2Services`. Changes made to these properties after the service collection has been built will not affect the already registered singleton instances.
*   **Initialization Order**: `InitializeEventSubscribers` relies on the `IServiceProvider` being fully constructed. Consequently, it must be called on the `IApplicationBuilder` instance only after the `WebApplication` has been built (i.e., after `builder.Build()`), but before `app.Run()`. Calling it prematurely may result in an `InvalidOperationException` due to unresolved scoped dependencies.
*   **Thread Safety**: The static configuration properties are not thread-safe for concurrent writes during the application startup phase. It is recommended to set these properties in a single-threaded context (such as the `Main` method or `Startup` constructor) before the DI container begins resolving services. Once the application is running, these properties should be treated as read-only.
*   **Middleware Dependencies**: If `EnableApiKeyAuth` is set to `true`, ensure that the necessary authentication schemes are configured in the `Authentication` section of the service collection, as the middleware will attempt to validate credentials against the configured scheme. Failure to do so may result in runtime authorization failures.
