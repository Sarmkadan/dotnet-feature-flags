# IWebhookService

Provides a contract for managing and dispatching webhooks in response to feature flag events. Implementations handle registration, lifecycle management, and reliable delivery with retry semantics for HTTP callbacks.

## API

### RegisterWebhookAsync

```csharp
Task<Webhook> RegisterWebhookAsync(WebhookRegistration registration, CancellationToken cancellationToken = default);
```

Registers a new webhook endpoint. The registration includes the target URL, event filters, and optional headers or secrets for payload signing.

**Parameters**
- `registration`: The webhook configuration including URL, events to subscribe to, and optional custom headers.
- `cancellationToken`: Token to cancel the operation.

**Returns**
The created `Webhook` instance with assigned identifier and timestamps.

**Throws**
- `ArgumentNullException` if `registration` is null.
- `ArgumentException` if the URL is invalid or event filters are empty.
- `WebhookConflictException` if a webhook with the same URL and event set already exists.

---

### GetWebhookAsync

```csharp
Task<Webhook?> GetWebhookAsync(Guid id, CancellationToken cancellationToken = default);
```

Retrieves a webhook by its unique identifier.

**Parameters**
- `id`: The webhook identifier.
- `cancellationToken`: Token to cancel the operation.

**Returns**
The `Webhook` if found; otherwise `null`.

**Throws**
- `ArgumentException` if `id` is empty.

---

### GetActiveWebhooksAsync

```csharp
Task<List<Webhook>> GetActiveWebhooksAsync(CancellationToken cancellationToken = default);
```

Returns all webhooks that are currently enabled and not marked for deletion.

**Parameters**
- `cancellationToken`: Token to cancel the operation.

**Returns**
A list of active `Webhook` instances. Returns an empty list if none exist.

**Throws**
- `OperationCanceledException` if the token is triggered.

---

### UpdateWebhookAsync

```csharp
Task<bool> UpdateWebhookAsync(Guid id, WebhookUpdate update, CancellationToken cancellationToken = default);
```

Updates an existing webhook's configuration. Only non-null properties in `update` are applied.

**Parameters**
- `id`: The webhook identifier.
- `update`: Partial update containing new URL, event filters, headers, secret, or enabled state.
- `cancellationToken`: Token to cancel the operation.

**Returns**
`true` if the webhook was found and updated; `false` if not found.

**Throws**
- `ArgumentException` if `id` is empty or `update` is null.
- `ArgumentException` if the new URL is invalid.

---

### DeleteWebhookAsync

```csharp
Task<bool> DeleteWebhookAsync(Guid id, CancellationToken cancellationToken = default);
```

Permanently removes a webhook and its delivery history.

**Parameters**
- `id`: The webhook identifier.
- `cancellationToken`: Token to cancel the operation.

**Returns**
`true` if the webhook was found and deleted; `false` if not found.

**Throws**
- `ArgumentException` if `id` is empty.

---

### TriggerWebhooksAsync

```csharp
Task TriggerWebhooksAsync(FeatureFlagEvent @event, CancellationToken cancellationToken = default);
```

Dispatches the given event to all active webhooks whose event filters match. Deliveries are executed asynchronously with individual error handling; failures are recorded for retry.

**Parameters**
- `@event`: The feature flag event to deliver.
- `cancellationToken`: Token to cancel the operation.

**Throws**
- `ArgumentNullException` if `@event` is null.
- `OperationCanceledException` if the token is triggered before dispatch begins.

---

### RetryFailedDeliveriesAsync

```csharp
Task RetryFailedDeliveriesAsync(TimeSpan? maxAge = null, CancellationToken cancellationToken = default);
```

Re-attempts delivery for all failed webhook payloads within the specified age window. Each retry follows the webhook's configured backoff policy.

**Parameters**
- `maxAge`: Optional maximum age of failed deliveries to retry. Defaults to 24 hours.
- `cancellationToken`: Token to cancel the operation.

**Throws**
- `ArgumentOutOfRangeException` if `maxAge` is negative.
- `OperationCanceledException` if the token is triggered.

## Usage

### Registering and triggering a webhook

```csharp
using DotNetFeatureFlags.Webhooks;

var registration = new WebhookRegistration
{
    Url = new Uri("https://api.example.com/flags/changed"),
    Events = new[] { FeatureFlagEventType.Created, FeatureFlagEventType.Updated },
    Headers = new Dictionary<string, string>
    {
        ["X-Custom-Source"] = "feature-flags"
    },
    Secret = "hmac-shared-secret"
};

Webhook webhook = await webhookService.RegisterWebhookAsync(registration);

var flagEvent = new FeatureFlagEvent
{
    Type = FeatureFlagEventType.Updated,
    FlagKey = "new-ui",
    PreviousState = false,
    CurrentState = true,
    Timestamp = DateTimeOffset.UtcNow
};

await webhookService.TriggerWebhooksAsync(flagEvent);
```

### Updating a webhook and retrying failed deliveries

```csharp
var update = new WebhookUpdate
{
    Enabled = false,
    Events = new[] { FeatureFlagEventType.Deleted }
};

bool updated = await webhookService.UpdateWebhookAsync(webhook.Id, update);

if (!updated)
{
    throw new InvalidOperationException($"Webhook {webhook.Id} not found");
}

await webhookService.RetryFailedDeliveriesAsync(TimeSpan.FromHours(6));
```

## Notes

- **Thread safety**: All methods are safe for concurrent calls. Implementations typically use fine-grained locking or concurrent collections to protect webhook state.
- **Idempotency**: `RegisterWebhookAsync` throws `WebhookConflictException` on duplicate URL+event combinations; callers should handle this to support idempotent registration flows.
- **Delivery guarantees**: `TriggerWebhooksAsync` fires deliveries in parallel and does not await their completion. Failed deliveries are persisted and surfaced via `RetryFailedDeliveriesAsync`. There is no built-in ordering guarantee across webhooks.
- **Retry policy**: `RetryFailedDeliveriesAsync` respects each webhook's configured `RetryPolicy` (exponential backoff, max attempts). The `maxAge` parameter prevents retrying stale failures indefinitely.
- **Deletion semantics**: `DeleteWebhookAsync` removes the webhook and all associated delivery records. In-flight deliveries for that webhook may still complete but will not be retried.
- **Null returns**: `GetWebhookAsync` returns `null` rather than throwing for missing identifiers, allowing callers to distinguish "not found" from errors without exception overhead.
