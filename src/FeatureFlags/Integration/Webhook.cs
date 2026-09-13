#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Integration;

/// <summary>
/// Webhook entity that represents a registered webhook endpoint for receiving feature flag events.
/// Webhooks are triggered when feature flags are created, updated, or deleted.
/// </summary>
public sealed class Webhook
{
    /// <summary>
    /// Gets or sets the unique identifier of the webhook.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the endpoint URL that receives webhook events.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable description of the webhook.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the webhook is active and can be triggered.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the event types that trigger this webhook.
    /// </summary>
    public WebhookEventType EventTypes { get; set; } = WebhookEventType.All;

    /// <summary>
    /// Optional: The key of the specific feature flag this webhook is interested in.
    /// If null or empty, the webhook triggers for any flag matching EventTypes.
    /// </summary>
    public string? FeatureFlagKey { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the webhook was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the webhook was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the webhook.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    // Retry policy
    /// <summary>
    /// Gets or sets the maximum number of delivery attempts before giving up.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the base delay in seconds between retry attempts.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 60;

    // Authentication
    /// <summary>
    /// Gets or sets the optional authorization header value sent with webhook requests.
    /// </summary>
    public string? AuthorizationHeader { get; set; }

    /// <summary>
    /// Secret key used for HMAC signature verification of webhook payloads.
    /// </summary>
    public string? Secret { get; set; }

    // Tracking
    /// <summary>
    /// Gets or sets the number of successful webhook deliveries.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Gets or sets the number of failed webhook deliveries.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the webhook was last triggered.
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// Validates webhook configuration.
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Url))
        {
            return false;
        }

        if (!Uri.TryCreate(Url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns a string representation of the webhook.
    /// </summary>
    public override string ToString() => $"Webhook {{ Id = {Id}, Url = {Url}, Description = {Description}, IsActive = {IsActive}, EventTypes = {EventTypes}, FeatureFlagKey = {FeatureFlagKey} }}";

    /// <summary>
    /// Checks if webhook should be triggered for specified event type.
    /// </summary>
    public bool ShouldTrigger(WebhookEventType eventType)
    {
        if (!IsActive)
        {
            return false;
        }

        return EventTypes.HasFlag(eventType) || EventTypes == WebhookEventType.All;
    }
}

/// <summary>
/// Webhook event types that can trigger webhook calls.
/// </summary>
[Flags]
public enum WebhookEventType
{
    FeatureFlagCreated = 1,
    FeatureFlagUpdated = 2,
    FeatureFlagDeleted = 4,
    FeatureFlagEnabled = 8,
    FeatureFlagDisabled = 16,
    RuleAdded = 32,
    RuleRemoved = 64,
    VariantUpdated = 128,
    All = FeatureFlagCreated | FeatureFlagUpdated | FeatureFlagDeleted | FeatureFlagEnabled | FeatureFlagDisabled | RuleAdded | RuleRemoved | VariantUpdated
}

/// <summary>
/// Payload sent to webhook endpoints when events occur.
/// </summary>
public sealed class WebhookPayload
{
    public string EventType { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? FeatureFlagKey { get; set; }

    public int? FeatureFlagId { get; set; }

    public string? ChangedBy { get; set; }

    public Dictionary<string, object?> Data { get; set; } = new();

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    /// <summary>
    /// Creates a webhook payload from a feature flag change event.
    /// </summary>
    public static WebhookPayload FromFeatureFlagEvent(string eventType, Models.FeatureFlag flag, string changedBy, Dictionary<string, object?>? data = null)
    {
        ArgumentNullException.ThrowIfNull(eventType);
        ArgumentNullException.ThrowIfNull(flag);
        ArgumentNullException.ThrowIfNull(changedBy);

        return new WebhookPayload
        {
            EventType = eventType,
            FeatureFlagKey = flag.Key,
            FeatureFlagId = flag.Id,
            ChangedBy = changedBy,
            Data = data ?? new Dictionary<string, object?>()
        };
    }
}

/// <summary>
/// Represents a webhook delivery attempt and its result.
/// </summary>
public sealed class WebhookDelivery
{
    public int Id { get; set; }

    public int WebhookId { get; set; }

    public Webhook? Webhook { get; set; }

    public string Payload { get; set; } = string.Empty;

    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

    public int? ResponseStatusCode { get; set; }

    public string? ResponseBody { get; set; }

    public int RetryCount { get; set; }

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime? NextRetryAt { get; set; }

    /// <summary>
    /// Marks this delivery as failed and schedules retry if retries remain.
    /// </summary>
    public void MarkFailed(string errorMessage, int maxRetries, int retryDelaySeconds)
    {
        ArgumentNullException.ThrowIfNull(errorMessage);

        IsSuccess = false;
        ErrorMessage = errorMessage;
        RetryCount++;

        if (RetryCount < maxRetries)
        {
            NextRetryAt = DateTime.UtcNow.AddSeconds(retryDelaySeconds * RetryCount);
        }
    }

    /// <summary>
    /// Marks this delivery as successful.
    /// </summary>
    public void MarkSuccessful(int? statusCode, string? responseBody)
    {
        IsSuccess = true;
        ResponseStatusCode = statusCode;
        ResponseBody = responseBody;
        NextRetryAt = null;
    }
}
