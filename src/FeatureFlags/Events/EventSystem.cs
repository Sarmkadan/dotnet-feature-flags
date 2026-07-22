#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Options;

namespace FeatureFlags.Events;

/// <summary>
/// Represents a feature flag event that occurred in the system.
/// Events are published to all registered subscribers for processing.
/// </summary>
public sealed class FeatureFlagEvent
{
    public string EventType { get; set; } = string.Empty;
    public int FeatureFlagId { get; set; }
    public string FeatureFlagKey { get; set; } = string.Empty;
    public string TriggeredBy { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object?> Metadata { get; set; } = new();
}

/// <summary>
/// Interface for objects that subscribe to feature flag events.
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Gets the types of events this subscriber is interested in.
    /// </summary>
    string[] InterestedEventTypes { get; }

    /// <summary>
    /// Handles the event when it occurs.
    /// </summary>
    /// <param name="@event">The event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task HandleEventAsync(FeatureFlagEvent @event, CancellationToken cancellationToken = default);
}

/// <summary>
/// Error handling mode for event bus dispatch.
/// </summary>
public enum EventBusErrorMode
{
    /// <summary>
    /// Fail-fast: propagate exceptions immediately (default behavior for backward compatibility).
    /// </summary>
    FailFast,

    /// <summary>
    /// Isolate errors: catch and log exceptions, allowing other subscribers to continue processing.
    /// </summary>
    Isolate
}

/// <summary>
/// Configuration options for the event bus.
/// </summary>
public sealed class EventBusOptions
{
    /// <summary>
    /// Error handling mode. Defaults to FailFast for backward compatibility.
    /// </summary>
    public EventBusErrorMode ErrorMode { get; set; } = EventBusErrorMode.FailFast;

    /// <summary>
    /// Maximum number of retry attempts for transient failures. Defaults to 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay in milliseconds for exponential backoff. Defaults to 100ms.
    /// </summary>
    public int BaseRetryDelayMs { get; set; } = 100;
}

/// <summary>
/// Delegate for handling subscriber dispatch errors.
/// </summary>
/// <param name="subscriber">The subscriber that failed</param>
/// <param name="event">The event being processed</param>
/// <param name="exception">The exception that was thrown</param>
/// <param name="attempt">The retry attempt number (1-based)</param>
public delegate void SubscriberErrorCallback(IEventSubscriber subscriber, FeatureFlagEvent @event, Exception exception, int attempt);

/// <summary>
/// Event bus that manages event publishing and subscriber notifications.
/// Implements pub-sub pattern with support for multiple subscribers per event type.
/// </summary>
public interface IEventBus
{
    void Subscribe(IEventSubscriber subscriber);
    void Unsubscribe(IEventSubscriber subscriber);
    Task PublishAsync(FeatureFlagEvent @event, CancellationToken cancellationToken = default);
    Task PublishAsync(string eventType, int featureFlagId, string featureFlagKey, string triggeredBy, Dictionary<string, object?>? metadata = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default in-process implementation of event bus.
/// </summary>
public sealed class EventBus : IEventBus
{
    private readonly List<IEventSubscriber> _subscribers = new();
    private readonly ILogger<EventBus> _logger;
    private readonly object _syncLock = new();
    private readonly EventBusOptions _options;
    private readonly SubscriberErrorCallback? _errorCallback;

    public EventBus(ILogger<EventBus> logger, EventBusOptions? options = null, SubscriberErrorCallback? errorCallback = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? new EventBusOptions();
        _errorCallback = errorCallback;
    }

    public void Subscribe(IEventSubscriber subscriber)
    {
        if (subscriber is null)
        {
            throw new ArgumentNullException(nameof(subscriber));
        }

        lock (_syncLock)
        {
            if (!_subscribers.Contains(subscriber))
            {
                _subscribers.Add(subscriber);
                _logger.LogInformation("Event subscriber registered: {SubscriberType}", subscriber.GetType().Name);
            }
        }
    }

    public void Unsubscribe(IEventSubscriber subscriber)
    {
        if (subscriber is null)
        {
            throw new ArgumentNullException(nameof(subscriber));
        }

        lock (_syncLock)
        {
            if (_subscribers.Remove(subscriber))
            {
                _logger.LogInformation("Event subscriber unregistered: {SubscriberType}", subscriber.GetType().Name);
            }
        }
    }

    public async Task PublishAsync(FeatureFlagEvent @event, CancellationToken cancellationToken = default)
    {
        if (@event is null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        _logger.LogInformation("Event published: {EventType} for flag {FeatureFlagKey}", @event.EventType, @event.FeatureFlagKey);

        List<IEventSubscriber> subscribersToNotify;

        lock (_syncLock)
        {
            subscribersToNotify = _subscribers
                .Where(s => s.InterestedEventTypes.Contains(@event.EventType) || s.InterestedEventTypes.Contains("*"))
                .ToList();
        }

        var tasks = subscribersToNotify.Select(subscriber => DispatchToSubscriberAsync(subscriber, @event, cancellationToken));

        await Task.WhenAll(tasks);
    }

    private async Task DispatchToSubscriberAsync(IEventSubscriber subscriber, FeatureFlagEvent @event, CancellationToken cancellationToken)
    {
        if (_options.ErrorMode == EventBusErrorMode.FailFast)
        {
            await subscriber.HandleEventAsync(@event, cancellationToken);
            return;
        }

        // Error isolation mode: wrap in try-catch with retry
        var attempt = 0;
        Exception? lastException = null;

        while (attempt <= _options.MaxRetryAttempts)
        {
            try
            {
                await subscriber.HandleEventAsync(@event, cancellationToken);
                return; // Success - exit retry loop
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Don't retry if operation was cancelled
                throw;
            }
            catch (Exception ex) when (attempt < _options.MaxRetryAttempts)
            {
                lastException = ex;
                attempt++;

                var delayMs = _options.BaseRetryDelayMs * (int)Math.Pow(2, attempt - 1);
                _logger.LogWarning(ex, "Subscriber {SubscriberType} failed (attempt {Attempt}/{MaxAttempts}). Retrying in {DelayMs}ms...",
                    subscriber.GetType().Name, attempt, _options.MaxRetryAttempts, delayMs);

                try
                {
                    await Task.Delay(delayMs, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // Don't retry if cancelled during delay
                    throw;
                }
            }
            catch (Exception ex)
            {
                // Final attempt failed - log and invoke error callback
                lastException = ex;
                break;
            }
        }

        // Log the error
        _logger.LogError(lastException, "Subscriber {SubscriberType} failed after {Attempt} attempts",
            subscriber.GetType().Name, attempt);

        // Invoke error callback if provided
        if (_errorCallback != null)
        {
            try
            {
                _errorCallback(subscriber, @event, lastException!, attempt);
            }
            catch (Exception callbackEx)
            {
                _logger.LogError(callbackEx, "Error callback failed for subscriber {SubscriberType}", subscriber.GetType().Name);
            }
        }
    }

    public async Task PublishAsync(string eventType, int featureFlagId, string featureFlagKey, string triggeredBy, Dictionary<string, object?>? metadata = null, CancellationToken cancellationToken = default)
    {
        var @event = new FeatureFlagEvent
        {
            EventType = eventType,
            FeatureFlagId = featureFlagId,
            FeatureFlagKey = featureFlagKey,
            TriggeredBy = triggeredBy,
            Metadata = metadata ?? new()
        };

        await PublishAsync(@event, cancellationToken);
    }

    Task IEventBus.PublishAsync(FeatureFlagEvent @event, CancellationToken cancellationToken = default) => PublishAsync(@event, cancellationToken);

    Task IEventBus.PublishAsync(string eventType, int featureFlagId, string featureFlagKey, string triggeredBy, Dictionary<string, object?>? metadata, CancellationToken cancellationToken = default)
        => PublishAsync(eventType, featureFlagId, featureFlagKey, triggeredBy, metadata, cancellationToken);
}

/// <summary>
/// Event subscriber that logs all feature flag events for audit trail.
/// </summary>
public sealed class EventLoggingSubscriber : IEventSubscriber {
    private readonly ILogger<EventLoggingSubscriber> _logger;

    public string[] InterestedEventTypes => new[] { "*" };

    public EventLoggingSubscriber(ILogger<EventLoggingSubscriber> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleEventAsync(FeatureFlagEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Feature Flag Event: {EventType} | Flag: {Key} | TriggeredBy: {TriggeredBy} | Time: {Time}",
            @event.EventType,
            @event.FeatureFlagKey,
            @event.TriggeredBy,
            @event.OccurredAt);

        await Task.CompletedTask;
    }

    Task IEventSubscriber.HandleEventAsync(FeatureFlagEvent @event, CancellationToken cancellationToken) => HandleEventAsync(@event, cancellationToken);
}

/// <summary>
/// Event subscriber that triggers webhooks when feature flag events occur.
/// </summary>
public sealed class WebhookEventSubscriber : IEventSubscriber {
    private readonly Integration.IWebhookService? _webhookService;
    private readonly ILogger<WebhookEventSubscriber> _logger;

    public string[] InterestedEventTypes => new[] { "FeatureFlagCreated", "FeatureFlagUpdated", "FeatureFlagDeleted", "FeatureFlagEnabled", "FeatureFlagDisabled" };

    public WebhookEventSubscriber(Integration.IWebhookService? webhookService, ILogger<WebhookEventSubscriber> logger)
    {
        _webhookService = webhookService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleEventAsync(FeatureFlagEvent @event, CancellationToken cancellationToken = default)
    {
        if (_webhookService is null)
        {
            return;
        }

        try
        {
            var eventTypeEnum = Enum.Parse<Integration.WebhookEventType>(@event.EventType);
            // Webhook service would be called here with the event details
            _logger.LogDebug("Webhook event triggered: {EventType} for flag {Key}", @event.EventType, @event.FeatureFlagKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook event handling error: {EventType}", @event.EventType);
        }

        await Task.CompletedTask;
    }

    Task IEventSubscriber.HandleEventAsync(FeatureFlagEvent @event, CancellationToken cancellationToken) => HandleEventAsync(@event, cancellationToken);
}

/// <summary>
/// Extension method for registering event system in dependency injection.
/// </summary>
public static class EventSystemExtensions
{
    /// <summary>
    /// Adds the event system with default configuration.
    /// </summary>
    public static IServiceCollection AddEventSystem(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton<IEventSubscriber, EventLoggingSubscriber>();

        return services;
    }

    /// <summary>
    /// Adds the event system with custom configuration options.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action for EventBusOptions</param>
    /// <param name="errorCallback">Optional error callback</param>
    public static IServiceCollection AddEventSystem(this IServiceCollection services, Action<EventBusOptions> configure, SubscriberErrorCallback? errorCallback = null)
    {
        if (configure is null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        services.Configure(configure);
        services.AddSingleton<IEventBus>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<EventBusOptions>>().Value;
            var logger = provider.GetRequiredService<ILogger<EventBus>>();
            return new EventBus(logger, options, errorCallback);
        });
        services.AddSingleton<IEventSubscriber, EventLoggingSubscriber>();

        return services;
    }

    /// <summary>
    /// Adds the event system with custom EventBus instance.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="eventBus">Custom EventBus instance</param>
    public static IServiceCollection AddEventSystem(this IServiceCollection services, IEventBus eventBus)
    {
        if (eventBus is null)
        {
            throw new ArgumentNullException(nameof(eventBus));
        }

        services.AddSingleton(eventBus);
        services.AddSingleton<IEventSubscriber, EventLoggingSubscriber>();

        return services;
    }
}
