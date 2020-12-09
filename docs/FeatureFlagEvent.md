# FeatureFlagEvent

The `FeatureFlagEvent` type represents events emitted by the feature flag system to signal state changes or actions related to feature flags. It is used by the event bus and subscribers to propagate changes across the application, enabling integration with logging, webhooks, or other external systems.

## API

### `FeatureFlagEvent` members

#### `public string EventType`
The type of event that occurred. This value indicates the nature of the change (e.g., `"FeatureFlagEnabled"`, `"FeatureFlagDisabled"`, `"FeatureFlagUpdated"`). This field is used by subscribers to determine how to react to the event.

#### `public int FeatureFlagId`
The unique identifier of the feature flag associated with this event. This value corresponds to the internal identifier of the feature flag that triggered the event.

#### `public string FeatureFlagKey`
The key of the feature flag associated with this event. This value is the human-readable key used to reference the feature flag in configuration and code.

#### `public string TriggeredBy`
The identifier of the actor or system that triggered the event. This could be a user ID, service name, or other context indicating the source of the change.

#### `public DateTime OccurredAt`
The timestamp when the event occurred. This value is set automatically when the event is created and represents the moment the state change took place.

#### `public Dictionary<string, object?> Metadata`
A dictionary of additional data related to the event. This field can contain arbitrary key-value pairs that provide context about the event, such as previous or new values of the feature flag, environment details, or other relevant information.

---

### `EventBus` members

#### `public sealed class EventBus : IEventBus`
A thread-safe event bus implementation that manages subscriptions and publishes events to registered subscribers.

#### `public EventBus()`
Constructs a new instance of the `EventBus` class. This constructor initializes an empty event bus with no subscribers.

#### `public void Subscribe(IEventSubscriber subscriber)`
Registers a subscriber to receive events from this bus.

- **Parameters**:
  - `subscriber`: The subscriber to register. Must not be `null`.
- **Throws**:
  - `ArgumentNullException`: If `subscriber` is `null`.

#### `public void Unsubscribe(IEventSubscriber subscriber)`
Removes a subscriber from receiving events from this bus.

- **Parameters**:
  - `subscriber`: The subscriber to remove. Must not be `null`.
- **Throws**:
  - `ArgumentNullException`: If `subscriber` is `null`.

#### `public async Task PublishAsync(FeatureFlagEvent @event)`
Publishes an event to all registered subscribers asynchronously.

- **Parameters**:
  - `@event`: The event to publish. Must not be `null`.
- **Throws**:
  - `ArgumentNullException`: If `@event` is `null`.
- **Remarks**:
  - This method invokes each subscriber's `HandleEventAsync` method sequentially. If any subscriber throws an exception, it will propagate and stop further processing unless handled by the subscriber.

#### `public async Task PublishAsync(FeatureFlagEvent @event, CancellationToken cancellationToken)`
Publishes an event to all registered subscribers asynchronously with cancellation support.

- **Parameters**:
  - `@event`: The event to publish. Must not be `null`.
  - `cancellationToken`: A token to monitor for cancellation requests.
- **Throws**:
  - `ArgumentNullException`: If `@event` is `null`.
  - `OperationCanceledException`: If the operation is canceled via the `cancellationToken`.
- **Remarks**:
  - This method behaves identically to the non-cancellation overload but respects cancellation requests during event processing.

---

### `EventLoggingSubscriber` members

#### `public sealed class EventLoggingSubscriber : IEventSubscriber`
A subscriber that logs feature flag events to the configured logging system.

#### `public EventLoggingSubscriber(ILogger<EventLoggingSubscriber> logger)`
Constructs a new instance of the `EventLoggingSubscriber` class.

- **Parameters**:
  - `logger`: The logger instance used to write event logs. Must not be `null`.

#### `public async Task HandleEventAsync(FeatureFlagEvent @event)`
Handles the provided feature flag event by logging it.

- **Parameters**:
  - `@event`: The event to handle. Must not be `null`.
- **Throws**:
  - `ArgumentNullException`: If `@event` is `null`.
- **Remarks**:
  - This method logs the event details at the `Information` level. The log message includes the `EventType`, `FeatureFlagKey`, and `TriggeredBy` fields.

---

### `WebhookEventSubscriber` members

#### `public sealed class WebhookEventSubscriber : IEventSubscriber`
A subscriber that sends feature flag events to a configured webhook endpoint.

#### `public WebhookEventSubscriber(HttpClient httpClient, IOptions<WebhookOptions> options)`
Constructs a new instance of the `WebhookEventSubscriber` class.

- **Parameters**:
  - `httpClient`: The `HttpClient` used to send HTTP requests. Must not be `null`.
  - `options`: The configuration options for the webhook. Must not be `null`.

#### `public async Task HandleEventAsync(FeatureFlagEvent @event)`
Handles the provided feature flag event by sending it to the configured webhook endpoint.

- **Parameters**:
  - `@event`: The event to handle. Must not be `null`.
- **Throws**:
  - `ArgumentNullException`: If `@event` is `null`.
  - `HttpRequestException`: If the HTTP request to the webhook fails.
- **Remarks**:
  - This method serializes the event to JSON and sends it as a POST request to the webhook URL specified in `WebhookOptions`. The request includes a `Content-Type: application/json` header.

---

### `AddEventSystem` members

#### `public static IServiceCollection AddEventSystem(this IServiceCollection services)`
Registers the event system components (event bus, logging subscriber, and webhook subscriber) with the dependency injection container.

- **Parameters**:
  - `services`: The `IServiceCollection` to which the services are added. Must not be `null`.
- **Returns**:
  - The `IServiceCollection` for method chaining.
- **Throws**:
  - `ArgumentNullException`: If `services` is `null`.
- **Remarks**:
  - This method registers the `EventBus` as a singleton, `EventLoggingSubscriber` as a transient service, and `WebhookEventSubscriber` as a transient service. It also configures `WebhookOptions` from the application's configuration.

## Usage

### Example 1: Basic event publishing and logging
