#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Data;
using FeatureFlags.Integration;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Repository;

/// <summary>
/// Repository for managing webhook persistence and queries.
/// Handles CRUD operations and specialized queries for webhook management.
/// </summary>
{public sealed class WebhookRepository {
    private readonly FeatureFlagDbContext _context;
    private readonly ILogger<WebhookRepository> _logger;

    public WebhookRepository(FeatureFlagDbContext context, ILogger<WebhookRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Webhook> CreateAsync(Webhook webhook)
    {
        if (webhook is null)
        {
            throw new ArgumentNullException(nameof(webhook));
        }

        webhook.CreatedAt = DateTime.UtcNow;
        webhook.UpdatedAt = DateTime.UtcNow;

        _context.Webhooks.Add(webhook);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Webhook created: {WebhookId} - {Url}", webhook.Id, webhook.Url);

        return webhook;
    }

    public async Task<Webhook?> GetByIdAsync(int id)
    {
        return await _context.Webhooks
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<List<Webhook>> GetActiveAsync()
    {
        return await _context.Webhooks
            .AsNoTracking()
            .Where(w => w.IsActive)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Webhook>> GetByEventTypeAsync(WebhookEventType eventType)
    {
        return await _context.Webhooks
            .AsNoTracking()
            .Where(w => w.IsActive && (w.EventTypes == WebhookEventType.All || w.EventTypes.HasFlag(eventType)))
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Webhook>> GetRecentFailuresAsync(int maxAgeHours = 24)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-maxAgeHours);

        return await _context.Webhooks
            .AsNoTracking()
            .Where(w => w.IsActive && w.LastTriggeredAt.HasValue && w.LastTriggeredAt > cutoffTime && w.FailureCount > 0)
            .OrderByDescending(w => w.LastTriggeredAt)
            .ToListAsync();
    }

    public async Task<bool> UpdateAsync(Webhook webhook)
    {
        if (webhook is null)
        {
            throw new ArgumentNullException(nameof(webhook));
        }

        var existing = await _context.Webhooks.FindAsync(webhook.Id);
        if (existing is null)
        {
            return false;
        }

        webhook.UpdatedAt = DateTime.UtcNow;

        _context.Entry(existing).CurrentValues.SetValues(webhook);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Webhook updated: {WebhookId}", webhook.Id);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var webhook = await _context.Webhooks.FindAsync(id);
        if (webhook is null)
        {
            return false;
        }

        _context.Webhooks.Remove(webhook);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Webhook deleted: {WebhookId}", id);

        return true;
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Webhooks.CountAsync();
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _context.Webhooks.CountAsync(w => w.IsActive);
    }
}

/// <summary>
/// Repository for managing webhook delivery attempts.
/// Tracks delivery history and supports retry operations.
/// </summary>
{public sealed class WebhookDeliveryRepository {
    private readonly FeatureFlagDbContext _context;
    private readonly ILogger<WebhookDeliveryRepository> _logger;

    public WebhookDeliveryRepository(FeatureFlagDbContext context, ILogger<WebhookDeliveryRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WebhookDelivery> CreateAsync(WebhookDelivery delivery)
    {
        if (delivery is null)
        {
            throw new ArgumentNullException(nameof(delivery));
        }

        _context.WebhookDeliveries.Add(delivery);
        await _context.SaveChangesAsync();

        return delivery;
    }

    public async Task<WebhookDelivery?> GetByIdAsync(int id)
    {
        return await _context.WebhookDeliveries
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<List<WebhookDelivery>> GetByWebhookIdAsync(int webhookId)
    {
        return await _context.WebhookDeliveries
            .AsNoTracking()
            .Where(d => d.WebhookId == webhookId)
            .OrderByDescending(d => d.TriggeredAt)
            .ToListAsync();
    }

    public async Task<List<WebhookDelivery>> GetPendingRetriesAsync()
    {
        var now = DateTime.UtcNow;

        return await _context.WebhookDeliveries
            .Include(d => d.Webhook)
            .Where(d => d.NextRetryAt.HasValue && d.NextRetryAt <= now && !d.IsSuccess)
            .OrderBy(d => d.NextRetryAt)
            .ToListAsync();
    }

    public async Task<List<WebhookDelivery>> GetRecentDeliveriesAsync(int webhookId, int days = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        return await _context.WebhookDeliveries
            .AsNoTracking()
            .Where(d => d.WebhookId == webhookId && d.TriggeredAt >= cutoffDate)
            .OrderByDescending(d => d.TriggeredAt)
            .ToListAsync();
    }

    public async Task<(int Successful, int Failed)> GetDeliveryStatsAsync(int webhookId, int days = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        var successful = await _context.WebhookDeliveries
            .CountAsync(d => d.WebhookId == webhookId && d.TriggeredAt >= cutoffDate && d.IsSuccess);

        var failed = await _context.WebhookDeliveries
            .CountAsync(d => d.WebhookId == webhookId && d.TriggeredAt >= cutoffDate && !d.IsSuccess);

        return (successful, failed);
    }

    public async Task<bool> UpdateAsync(WebhookDelivery delivery)
    {
        if (delivery is null)
        {
            throw new ArgumentNullException(nameof(delivery));
        }

        var existing = await _context.WebhookDeliveries.FindAsync(delivery.Id);
        if (existing is null)
        {
            return false;
        }

        _context.Entry(existing).CurrentValues.SetValues(delivery);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> CleanupOldDeliveriesAsync(int retentionDays = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        var deliveries = await _context.WebhookDeliveries
            .Where(d => d.TriggeredAt < cutoffDate)
            .ToListAsync();

        if (deliveries.Count == 0)
        {
            return 0;
        }

        _context.WebhookDeliveries.RemoveRange(deliveries);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Cleaned up {Count} old webhook deliveries", deliveries.Count);

        return deliveries.Count;
    }
}
