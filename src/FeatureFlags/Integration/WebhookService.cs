#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using FeatureFlags.Models;
using FeatureFlags.Utilities;

namespace FeatureFlags.Integration;

/// <summary>
/// Service for managing webhooks and triggering webhook deliveries.
/// Handles webhook registration, validation, and event dispatching.
/// </summary>
public interface IWebhookService
{
    Task<Webhook> RegisterWebhookAsync(string url, string description, WebhookEventType eventTypes, string? featureFlagKey, string? secret, string createdBy, CancellationToken cancellationToken = default);
    Task<Webhook?> GetWebhookAsync(int webhookId, CancellationToken cancellationToken = default);
    Task<List<Webhook>> GetActiveWebhooksAsync(WebhookEventType eventType, string? featureFlagKey = null);
    Task<bool> UpdateWebhookAsync(int webhookId, string? url, string? description, WebhookEventType? eventTypes, CancellationToken cancellationToken = default);
    Task<bool> DeleteWebhookAsync(int webhookId, CancellationToken cancellationToken = default);
    Task TriggerWebhooksAsync(WebhookEventType eventType, FeatureFlag flag, string changedBy, Dictionary<string, object?>? data = null, CancellationToken cancellationToken = default);
    Task RetryFailedDeliveriesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Default implementation of webhook service.
/// </summary>
public sealed class WebhookService : IWebhookService {
    private readonly IWebhookRepository _webhookRepository;
    private readonly IWebhookDeliveryRepository _deliveryRepository;
    private readonly HttpApiClient _httpClient;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        IWebhookRepository webhookRepository,
        IWebhookDeliveryRepository deliveryRepository,
        HttpApiClient httpClient,
        ILogger<WebhookService> logger)
    {
        _webhookRepository = webhookRepository ?? throw new ArgumentNullException(nameof(webhookRepository));
        _deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Webhook> RegisterWebhookAsync(string url, string description, WebhookEventType eventTypes, string? featureFlagKey, string? secret, string createdBy, CancellationToken cancellationToken = default)
    {
        var webhook = new Webhook
        {
            Url = url,
            Description = description,
            EventTypes = eventTypes,
            FeatureFlagKey = featureFlagKey,
            Secret = secret,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!webhook.IsValid())
        {
            throw new ArgumentException("Invalid webhook URL");
        }

        return await _webhookRepository.CreateAsync(webhook);
    }

    public async Task<Webhook?> GetWebhookAsync(int webhookId, CancellationToken cancellationToken = default)
    {
        return await _webhookRepository.GetByIdAsync(webhookId);
    }

    public async Task<List<Webhook>> GetActiveWebhooksAsync(WebhookEventType eventType, string? featureFlagKey = null)
    {
        var webhooks = await _webhookRepository.GetActiveAsync();
        return webhooks.Where(w => w.ShouldTrigger(eventType) && (string.IsNullOrEmpty(w.FeatureFlagKey) || w.FeatureFlagKey == featureFlagKey)).ToList();
    }

    public async Task<bool> UpdateWebhookAsync(int webhookId, string? url, string? description, WebhookEventType? eventTypes, CancellationToken cancellationToken = default)
    {
        var webhook = await _webhookRepository.GetByIdAsync(webhookId);
        if (webhook is null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(url))
        {
            webhook.Url = url;
        }

        if (!string.IsNullOrEmpty(description))
        {
            webhook.Description = description;
        }

        if (eventTypes.HasValue)
        {
            webhook.EventTypes = eventTypes.Value;
        }

        webhook.UpdatedAt = DateTime.UtcNow;

        if (!webhook.IsValid())
        {
            return false;
        }

        return await _webhookRepository.UpdateAsync(webhook);
    }

    public async Task<bool> DeleteWebhookAsync(int webhookId, CancellationToken cancellationToken = default)
    {
        return await _webhookRepository.DeleteAsync(webhookId);
    }

    public async Task TriggerWebhooksAsync(WebhookEventType eventType, FeatureFlag flag, string changedBy, Dictionary<string, object?>? data = null, CancellationToken cancellationToken = default)
    {
        var webhooks = await GetActiveWebhooksAsync(eventType, flag.Key);

        if (!webhooks.Any())
        {
            return;
        }

        var payload = WebhookPayload.FromFeatureFlagEvent(eventType.ToString(), flag, changedBy, data);
        var payloadJson = JsonSerializer.Serialize(payload);

        foreach (var webhook in webhooks)
        {
            _ = Task.Run(async () => await SendWebhookAsync(webhook, payloadJson));
        }
    }

    public async Task RetryFailedDeliveriesAsync(CancellationToken cancellationToken = default)
    {
        var failedDeliveries = await _deliveryRepository.GetPendingRetriesAsync();

        foreach (var delivery in failedDeliveries)
        {
            var webhook = await _webhookRepository.GetByIdAsync(delivery.WebhookId);
            if (webhook is not null && webhook.IsActive)
            {
                _ = Task.Run(async () => await SendWebhookAsync(webhook, delivery.Payload, delivery));
            }
        }
    }

    private async Task SendWebhookAsync(Webhook webhook, string payload, WebhookDelivery? existingDelivery = null)
    {
        var delivery = existingDelivery ?? new WebhookDelivery
        {
            WebhookId = webhook.Id,
            Payload = payload,
            TriggeredAt = DateTime.UtcNow
        };

        try
        {
            using var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            if (!string.IsNullOrEmpty(webhook.AuthorizationHeader))
            {
                client.DefaultRequestHeaders.Add("Authorization", webhook.AuthorizationHeader);
            }

            // Add HMAC signature if secret is provided
            if (!string.IsNullOrEmpty(webhook.Secret))
            {
                var signature = HashingUtilities.ComputeHmacSha256(payload, webhook.Secret);
                client.DefaultRequestHeaders.Add("X-Hub-Signature-256", $"sha256={signature}");
            }

            using var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(webhook.Url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                delivery.MarkSuccessful((int)response.StatusCode, responseBody);
                webhook.SuccessCount++;
                webhook.LastTriggeredAt = DateTime.UtcNow;

                _logger.LogInformation("Webhook delivered successfully: {WebhookId} to {Url}", webhook.Id, webhook.Url);
            }
            else
            {
                var errorMsg = $"HTTP {response.StatusCode}";
                delivery.MarkFailed(errorMsg, webhook.MaxRetries, webhook.RetryDelaySeconds);
                webhook.FailureCount++;

                _logger.LogWarning("Webhook delivery failed: {WebhookId} to {Url} - {Error}", webhook.Id, webhook.Url, errorMsg);
            }
        }
        catch (Exception ex)
        {
            delivery.MarkFailed(ex.Message, webhook.MaxRetries, webhook.RetryDelaySeconds);
            webhook.FailureCount++;

            _logger.LogError(ex, "Webhook delivery error: {WebhookId} to {Url}", webhook.Id, webhook.Url);
        }

        await _deliveryRepository.CreateAsync(delivery);
        await _webhookRepository.UpdateAsync(webhook);
    }
}

/// <summary>
/// Repository interface for webhook operations.
/// </summary>
public interface IWebhookRepository
{
    Task<Webhook> CreateAsync(Webhook webhook);
    Task<Webhook?> GetByIdAsync(int id);
    Task<List<Webhook>> GetActiveAsync();
    Task<bool> UpdateAsync(Webhook webhook);
    Task<bool> DeleteAsync(int id);
}

/// <summary>
/// Repository interface for webhook delivery tracking.
/// </summary>
public interface IWebhookDeliveryRepository
{
    Task<WebhookDelivery> CreateAsync(WebhookDelivery delivery);
    Task<List<WebhookDelivery>> GetPendingRetriesAsync();
}
