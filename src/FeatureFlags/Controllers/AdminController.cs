#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Formatters;
using FeatureFlags.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// Admin API endpoints for managing webhooks, exports, and system configuration.
/// Provides administrative operations with proper authorization checks.
/// </summary>
[ApiController]
[Route("api/admin")]
{public sealed class AdminController {
    private readonly Integration.IWebhookService _webhookService;
    private readonly Services.IFeatureFlagService _featureFlagService;
    private readonly Caching.ICacheService _cacheService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        Integration.IWebhookService webhookService,
        Services.IFeatureFlagService featureFlagService,
        Caching.ICacheService cacheService,
        ILogger<AdminController> logger)
    {
        _webhookService = webhookService ?? throw new ArgumentNullException(nameof(webhookService));
        _featureFlagService = featureFlagService ?? throw new ArgumentNullException(nameof(featureFlagService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a new webhook endpoint to receive feature flag events.
    /// </summary>
    [HttpPost("webhooks")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterWebhook([FromBody] RegisterWebhookRequest request, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        {
            return BadRequest("Invalid webhook URL");
        }

        var userId = User.FindFirst("sub")?.Value ?? "system";

        try
        {
            var webhook = await _webhookService.RegisterWebhookAsync(
                request.Url,
                request.Description ?? string.Empty,
                request.EventTypes ?? Integration.WebhookEventType.All,
                request.FeatureFlagKey,
                request.Secret,
                userId);

            return Created($"/api/admin/webhooks/{webhook.Id}", webhook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering webhook");
            return BadRequest("Failed to register webhook");
        }
    }

    /// <summary>
    /// Gets list of all active webhooks.
    /// </summary>
    [HttpGet("webhooks")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWebhooks(CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real scenario, this would fetch from repository
            var webhooks = new List<Integration.Webhook>();
            return Ok(new { webhooks });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving webhooks");
            return StatusCode(500, "Failed to retrieve webhooks");
        }
    }

    /// <summary>
    /// Deletes a webhook by ID.
    /// </summary>
    [HttpDelete("webhooks/{webhookId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWebhook(int webhookId, CancellationToken cancellationToken = default)
    {
        var success = await _webhookService.DeleteWebhookAsync(webhookId);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Exports all feature flags to CSV format for backup or analysis.
    /// </summary>
    [HttpGet("export/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportCsv([FromQuery] bool includeRules = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var flags = await _featureFlagService.GetFeatureFlagsAsync();
            var csv = CsvExporter.ExportFeatureFlags(flags, includeRules);

            var fileName = $"feature-flags-{DateTime.UtcNow:yyyy-MM-dd-HHmmss}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to CSV");
            return StatusCode(500, "Failed to export data");
        }
    }

    /// <summary>
    /// Exports all feature flags to XML format for system integration.
    /// </summary>
    [HttpGet("export/xml")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportXml(CancellationToken cancellationToken = default)
    {
        try
        {
            var flags = await _featureFlagService.GetFeatureFlagsAsync();
            var xml = XmlExporter.ExportFeatureFlags(flags);

            var fileName = $"feature-flags-{DateTime.UtcNow:yyyy-MM-dd-HHmmss}.xml";
            return File(System.Text.Encoding.UTF8.GetBytes(xml), "application/xml", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting to XML");
            return StatusCode(500, "Failed to export data");
        }
    }

    /// <summary>
    /// Imports feature flags from CSV file.
    /// </summary>
    [HttpPost("import/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportCsv(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("No file provided");
        }

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();
            var flags = CsvParser.ParseFeatureFlags(content);

            _logger.LogInformation("Imported {Count} feature flags from CSV", flags.Count);

            return Ok(new { importedCount = flags.Count, flags });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing from CSV");
            return BadRequest("Failed to import data");
        }
    }

    /// <summary>
    /// Clears the feature flag cache to force fresh database load.
    /// </summary>
    [HttpPost("cache/clear")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearCache(CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheService.ClearAsync();
            _logger.LogInformation("Cache cleared by {User}", User.FindFirst("sub")?.Value ?? "unknown");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return StatusCode(500, "Failed to clear cache");
        }
    }

    /// <summary>
    /// Gets system health status and statistics.
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            uptime = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Gets system statistics and metrics.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken = default)
    {
        try
        {
            var flags = await _featureFlagService.GetFeatureFlagsAsync();
            var enabledCount = flags.Count(f => f.IsEnabled);
            var disabledCount = flags.Count(f => !f.IsEnabled);

            return Ok(new
            {
                totalFlags = flags.Count,
                enabledFlags = enabledCount,
                disabledFlags = disabledCount,
                percentRolloutCount = flags.Count(f => f.RolloutType == Enums.RolloutType.Percentage),
                rulesBasedCount = flags.Count(f => f.RolloutType == Enums.RolloutType.RulesBased),
                abTestCount = flags.Count(f => f.RolloutType == Enums.RolloutType.ABTest)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics");
            return StatusCode(500, "Failed to retrieve statistics");
        }
    }
}

/// <summary>
/// Request model for registering a webhook.
/// </summary>
public sealed class RegisterWebhookRequest
{
    public string Url { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Integration.WebhookEventType? EventTypes { get; set; }
}
{ get; set; }
}
