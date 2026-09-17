# Dependency Injection

This document describes the service registration extension methods used to configure the Feature Flags application's dependency injection container.

## Overview

The Feature Flags application uses extension methods on `IServiceCollection` to register services in a modular way. These extensions are organized by feature area and phase of development.

## Core Service Registration (`DependencyInjectionExtensions.cs`)

The `DependencyInjectionExtensions` class in `src/FeatureFlags/Configuration/DependencyInjectionExtensions.cs` registers the core feature flag services:

### Services Registered

- **Repositories**
  - `IFeatureFlagRepository` → `FeatureFlagRepository` (Scoped)
  - `IAuditLogRepository` → `AuditLogRepository` (Scoped)

- **Caching Services**
  - `ICacheService` → `InMemoryCacheService` (Scoped)
  - `IFeatureFlagCache` → `FeatureFlagCache` (Scoped)

- **Business Logic Services**
  - `IFeatureFlagService` → `FeatureFlagService` (Scoped)
  - `IRuleEvaluationService` → `RuleEvaluationService` (Scoped)
  - `IPercentageRolloutService` → `PercentageRolloutService` (Scoped)
  - `IAuditLogService` → `AuditLogService` (Scoped)
  - `IFlagEvaluationLogService` → `FlagEvaluationLogService` (Singleton)

- **Configuration**
  - `FeatureFlagOptions` bound to the "FeatureFlags" configuration section

## Phase 2 Service Registration (`Phase2DependencyInjectionExtensions.cs`)

The `Phase2DependencyInjectionExtensions` class in `src/FeatureFlags/Configuration/Phase2DependencyInjectionExtensions.cs` registers additional services introduced in Phase 2:

### Services Registered

- **Caching**
  - Configurable cache provider (Redis or InMemory) based on `Cache:Provider` setting
  - `ICacheService` → `DistributedCacheService` (Scoped) when using Redis
  - `ICacheService` → `InMemoryCacheService` (Singleton) when using InMemory

- **Webhook Services**
  - `IWebhookService` → `WebhookService` (Scoped)
  - `IWebhookRepository` → `WebhookRepository` (Scoped)
  - `IWebhookDeliveryRepository` → `WebhookDeliveryRepository` (Scoped)
  - `HttpApiClient` (Scoped)

- **HTTP Client Factory**
  - Registers typed HTTP clients via `AddFeatureFlagHttpClients()`:
    - `IHttpClientFactory` → `DefaultHttpClientFactory` (Singleton)
    - Named clients: "WebhookClient" and "ExternalApiClient"

- **Event System**
  - Registers event bus and subscribers via `AddEventSystem()`:
    - `IEventBus` → `EventBus` (Singleton)
    - `IEventSubscriber` → `EventLoggingSubscriber` (Singleton)

- **Background Workers**
  - `AuditLogCleanupOptions` (Singleton) bound to "AuditLogCleanup" configuration
  - `AuditLogCleanupWorker` (Hosted Service)
  - `WebhookRetryOptions` (Singleton) bound to "WebhookRetry" configuration
  - `WebhookRetryWorker` (Hosted Service) - only if enabled
  - `CacheSyncOptions` (Singleton) bound to "CacheSync" configuration
  - `CacheSyncWorker` (Hosted Service) - only if enabled
  - `RateLimitOptions` (Singleton) bound to "RateLimit" configuration
  - `AuthenticationOptions` (Singleton) bound to "Authentication" configuration

### Middleware Registration

The Phase 2 extensions also provide middleware registration methods:

- `UsePhase2Middleware()` - Adds core middleware to the application pipeline:
  - `ErrorHandlingMiddleware`
  - `RequestLoggingMiddleware`
  - `RateLimitingMiddleware`
  - `AuthenticationMiddleware`

- `InitializeEventSubscribers()` - Sets up event subscribers:
  - Registers `EventLoggingSubscriber` to listen for all events
  - Registers `WebhookEventSubscriber` to trigger webhooks for feature flag events

## Usage Example

In `Program.cs` or `Startup.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add core feature flag services
builder.Services.AddFeatureFlagServices(builder.Configuration);

// Add Phase 2 services
builder.Services.AddPhase2Services(builder.Configuration);

// Build the app
var app = builder.Build();

// Configure middleware pipeline
app.UsePhase2Middleware();
app.InitializeEventSubscribers();

// ... rest of configuration
```

## Service Lifetimes

- **Singleton**: Created once per application lifetime
- **Scoped**: Created once per request (or scope)
- **Transient**: Created each time it's requested

## Configuration Sections

Various features are configured through specific configuration sections:

- `FeatureFlags`: Core feature flag options
- `Cache`: Cache provider selection (`InMemory` or `distributed`)
- `ConnectionStrings:Redis`: Redis connection string when using distributed cache
- `AuditLogCleanup`: Audit log cleanup worker options
- `WebhookRetry`: Webhook retry worker options
- `CacheSync`: Cache synchronization worker options
- `RateLimit`: Rate limiting middleware options
- `Authentication`: Authentication middleware options