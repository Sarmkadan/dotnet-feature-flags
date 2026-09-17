# Webhook Service Integration

## Overview
The Webhook Service provides a robust mechanism for notifying external systems about feature flag events. It handles registration, validation, asynchronous delivery, retry logic, and detailed tracking.

## Architecture & Components
- `Webhook`: Entity representing a registered endpoint.
- `WebhookEventType`: Flagged enum defining triggerable events.
- `WebhookPayload`: DTO for event data sent to endpoints.
- `WebhookDelivery`: Tracks individual delivery attempts and outcomes.
- `IWebhookService` / `WebhookService`: Core service managing lifecycle and dispatch.
- `IWebhookRepository` / `IWebhookDeliveryRepository`: Data access interfaces.

## Integration Flow
1. **Registration**: Webhooks are registered via `RegisterWebhookAsync(...)`. The service validates the URL and configuration before persisting.
2. **Event Triggering**: When a feature flag changes, `TriggerWebhooksAsync(...)` is invoked.
3. **Filtering**: `GetActiveWebhooksAsync(...)` retrieves active webhooks matching the `WebhookEventType` and optional `FeatureFlagKey`.
4. **Payload Generation**: `WebhookPayload.FromFeatureFlagEvent(...)` constructs the JSON payload.
5. **Asynchronous Dispatch**: Each matching webhook triggers `SendWebhookAsync(...)` in a fire-and-forget task.
6. **Delivery & Response Handling**: The service sends an HTTP POST request. Success updates counters and timestamps. Failures trigger retry logic.
7. **Persistence**: Delivery records and updated webhook states are saved to the database.
8. **Retry Mechanism**: `RetryFailedDeliveriesAsync(...)` periodically processes pending retries based on `NextRetryAt`.

## Component Details

### Webhook Entity (`Webhook`)
Stores configuration for external endpoints.
- **Properties**: `Url`, `Description`, `IsActive`, `EventTypes`, `FeatureFlagKey`, `MaxRetries`, `RetryDelaySeconds`, `AuthorizationHeader`, `Secret`.
- **Validation**: `IsValid()` ensures the URL is absolute and uses HTTP/HTTPS.
- **Filtering**: `ShouldTrigger(WebhookEventType)` checks if the webhook should fire for a given event.

### Event Types (`WebhookEventType`)
A `[Flags]` enum supporting granular or broad event subscriptions:
- `FeatureFlagCreated`, `FeatureFlagUpdated`, `FeatureFlagDeleted`
- `FeatureFlagEnabled`, `FeatureFlagDisabled`
- `RuleAdded`, `RuleRemoved`, `VariantUpdated`
- `All` (subscribes to all events)

### Payload (`WebhookPayload`)
Standardized JSON structure sent to endpoints:
- `EventType`, `Timestamp`, `FeatureFlagKey`, `FeatureFlagId`, `ChangedBy`, `Data`, `OldValue`, `NewValue`.
- Factory method: `FromFeatureFlagEvent(...)` simplifies creation during triggers.

### Delivery Tracking (`WebhookDelivery`)
Records each attempt to deliver a payload.
- Tracks `RetryCount`, `IsSuccess`, `ErrorMessage`, `NextRetryAt`.
- `MarkFailed(...)`: Increments retry count, calculates next retry time, and logs failure.
- `MarkSuccessful(...)`: Records status code/response, clears retry schedule.

### Service Operations (`WebhookService`)
- **Lifecycle Management**: `RegisterWebhookAsync`, `UpdateWebhookAsync`, `DeleteWebhookAsync`, `GetWebhookAsync`.
- **Dispatch**: `TriggerWebhooksAsync` handles event broadcasting.
- **Retry Scheduler**: `RetryFailedDeliveriesAsync` processes backlogged deliveries.
- **Core Delivery**: `SendWebhookAsync` handles HTTP communication, header injection, and state updates.

## Security & Headers
- **Authorization**: If `AuthorizationHeader` is set on the `Webhook`, it's added to the request headers.
- **HMAC Signature**: If a `Secret` is provided, the service computes an SHA-256 HMAC of the payload and attaches it via the `X-Hub-Signature-256` header.

## Error Handling & Logging
- **HTTP Errors**: Non-2xx responses trigger `MarkFailed`, incrementing `FailureCount` and scheduling a retry.
- **Exceptions**: Network timeouts, circuit breakers, and unexpected exceptions are caught, logged, and handled gracefully without crashing the main thread.
- **Logging**: Detailed debug logs for successes, warning/error logs for failures including attempt numbers and elapsed times.

## Repository Interfaces
- `IWebhookRepository`: CRUD operations for webhook configurations.
- `IWebhookDeliveryRepository`: Creates delivery records and fetches pending retries via `GetPendingRetriesAsync()`.

## Usage Example
