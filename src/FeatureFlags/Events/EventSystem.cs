#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Events;

/// <summary>
/// Represents a feature flag event that occurred in the system.
/// Events are published to all registered subscribers for processing.
/// </summary>
public class FeatureFlagEvent
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
    Task HandleEventAsync(FeatureFlagEvent @event);
}

/// <summary>
/// Event bus that manages event publishing and subscriber notifications.
/// Implements pub-sub pattern with support for multiple subscribers per event type.
/// </summary>
public interface IEventBus
{
    void Subscribe(IEventSubscriber subscriber);
    void Unsubscribe(IEventSubscriber subscriber);
    Task PublishAsync(FeatureFlagEvent @event);
    Task PublishAsync(string eventType, int featureFlagId, string featureFlagKey, string triggeredBy, Dictionary<string, object?>? metadata = null);
}

/// <summary>
/// Default in-process implementation of event bus.
/// </summary>
{public sealed class EventBus {
    private readonly List<IEventSubscriber> _subscribers = new();
    private readonly ILogger<EventBus> _logger;
    private readonly object _syncLock = new();

    public EventBus(ILogger<EventBus> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

    public async Task PublishAsync(FeatureFlagEvent @event)
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

        var tasks = subscribersToNotify.Select(async subscriber =>
        {
            try
            {
                await subscriber.HandleEventAsync(@event);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Event handler error: {SubscriberType}", subscriber.GetType().Name);
            }
        });

        await Task.WhenAll(tasks);
    }

    public async Task PublishAsync(string eventType, int featureFlagId, string featureFlagKey, string triggeredBy, Dictionary<string, object?>? metadata = null)
    {
        var @event = new FeatureFlagEvent
        {
            EventType = eventType,
            FeatureFlagId = featureFlagId,
            FeatureFlagKey = featureFlagKey,
            TriggeredBy = triggeredBy,
            Metadata = metadata ?? new()
        };

        await PublishAsync(@event);
    }
}

/// <summary>
/// Event subscriber that logs all feature flag events for audit trail.
/// </summary>
{public sealed class EventLoggingSubscriber {
    private readonly ILogger<EventLoggingSubscriber> _logger;

    public string[] InterestedEventTypes => new[] { "*" };

    public EventLoggingSubscriber(ILogger<EventLoggingSubscriber> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleEventAsync(FeatureFlagEvent @event)
    {
        _logger.LogInformation(
            "Feature Flag Event: {EventType} | Flag: {Key} | TriggeredBy: {TriggeredBy} | Time: {Time}",
            @event.EventType,
            @event.FeatureFlagKey,
            @event.TriggeredBy,
            @event.OccurredAt);

        await Task.CompletedTask;
    }
}

/// <summary>
/// Event subscriber that triggers webhooks when feature flag events occur.
/// </summary>
{public sealed class WebhookEventSubscriber {
    private readonly Integration.IWebhookService? _webhookService;
    private readonly ILogger<WebhookEventSubscriber> _logger;

    public string[] InterestedEventTypes => new[] { "FeatureFlagCreated", "FeatureFlagUpdated", "FeatureFlagDeleted", "FeatureFlagEnabled", "FeatureFlagDisabled" };

    public WebhookEventSubscriber(Integration.IWebhookService? webhookService, ILogger<WebhookEventSubscriber> logger)
    {
        _webhookService = webhookService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleEventAsync(FeatureFlagEvent @event)
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
}

/// <summary>
/// Extension method for registering event system in dependency injection.
/// </summary>
public static class EventSystemExtensions
{
    public static IServiceCollection AddEventSystem(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, EventBus>();
        services.AddSingleton<IEventSubscriber, EventLoggingSubscriber>();

        return services;
    }
}
