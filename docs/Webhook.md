# Webhook
The `Webhook` type in the `dotnet-feature-flags` project represents a webhook configuration, which is used to send notifications to a specified URL when certain events occur. This allows for automated communication and integration with external systems, enabling features such as real-time updates and alerts.

## API
The `Webhook` type has the following public members:
* `Id`: A unique identifier for the webhook, represented as an integer.
* `Url`: The URL that the webhook will send notifications to, represented as a string.
* `Description`: A human-readable description of the webhook, represented as a string.
* `IsActive`: A boolean indicating whether the webhook is currently active.
* `EventTypes`: An enumeration of the types of events that will trigger the webhook, represented as a `WebhookEventType`.
* `FeatureFlagKey`: The key of the feature flag associated with the webhook, represented as a nullable string.
* `CreatedAt` and `UpdatedAt`: The dates and times when the webhook was created and last updated, respectively, represented as `DateTime` objects.
* `CreatedBy`: The user who created the webhook, represented as a string.
* `MaxRetries`: The maximum number of times the webhook will retry sending a notification if it fails, represented as an integer.
* `RetryDelaySeconds`: The delay in seconds between retries, represented as an integer.
* `AuthorizationHeader` and `Secret`: Optional authorization header and secret used for authentication, represented as nullable strings.
* `SuccessCount` and `FailureCount`: The number of successful and failed notifications sent by the webhook, respectively, represented as integers.
* `LastTriggeredAt`: The date and time when the webhook was last triggered, represented as a nullable `DateTime` object.
* `IsValid`: A boolean indicating whether the webhook is valid.
* `ShouldTrigger`: A boolean indicating whether the webhook should be triggered.
* `EventType`: The type of event that triggered the webhook, represented as a string.
* `Timestamp`: The date and time when the webhook was triggered, represented as a `DateTime` object.

## Usage
Here are two examples of using the `Webhook` type in C#:
```csharp
// Create a new webhook
var webhook = new Webhook
{
    Url = "https://example.com/webhook",
    Description = "My Webhook",
    IsActive = true,
    EventTypes = WebhookEventType.FeatureFlagUpdated,
    MaxRetries = 3,
    RetryDelaySeconds = 10
};

// Trigger the webhook
webhook.ShouldTrigger = true;
Console.WriteLine($"Webhook triggered at {webhook.Timestamp}");
```

```csharp
// Get an existing webhook
var existingWebhook = GetWebhook(1);

// Update the webhook's URL
existingWebhook.Url = "https://example.com/new-webhook";
existingWebhook.UpdatedAt = DateTime.Now;

// Save the updated webhook
SaveWebhook(existingWebhook);
```

## Notes
When using the `Webhook` type, consider the following edge cases:
* If `MaxRetries` is set to 0, the webhook will not retry sending notifications if it fails.
* If `RetryDelaySeconds` is set to 0, the webhook will retry sending notifications immediately if it fails.
* If `AuthorizationHeader` or `Secret` is not provided, the webhook may not be able to authenticate with the target system.
* The `IsValid` property may be false if the webhook's configuration is invalid, such as if the `Url` is empty.
* The `ShouldTrigger` property may be false if the webhook is not active or if the event type does not match the webhook's configuration.
* The `Webhook` type is not thread-safe, so it should not be accessed concurrently by multiple threads.
