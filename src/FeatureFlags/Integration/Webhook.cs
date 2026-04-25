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
public class Webhook
{
    public int Id { get; set; }

    public string Url { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public WebhookEventType EventTypes { get; set; } = WebhookEventType.All;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    // Retry policy
    public int MaxRetries { get; set; } = 3;

    public int RetryDelaySeconds { get; set; } = 60;

    // Authentication
    public string? AuthorizationHeader { get; set; }

    // Tracking
    public int SuccessCount { get; set; }

    public int FailureCount { get; set; }

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
public class WebhookPayload
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
public class WebhookDelivery
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
