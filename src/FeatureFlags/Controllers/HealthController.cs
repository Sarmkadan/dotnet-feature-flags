#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Data;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FeatureFlags.Controllers;

/// <summary>
/// Health check endpoints for monitoring application status and dependencies.
/// </summary>
[ApiController]
[Route("[controller]")]
{public sealed class HealthController {
    private readonly FeatureFlagDbContext _dbContext;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        FeatureFlagDbContext dbContext,
        IFeatureFlagService featureFlagService,
        ILogger<HealthController> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _featureFlagService = featureFlagService ?? throw new ArgumentNullException(nameof(featureFlagService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Basic liveness check - returns 200 if the application is running.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult GetLiveness()
    {
        return Ok(new HealthResponse
        {
            Status = "healthy",
            Timestamp = DateTime.UtcNow,
            Version = "2.0.0",
            Uptime = GetUptime()
        });
    }

    /// <summary>
    /// Readiness check - verifies that all dependencies are available.
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetReadiness(CancellationToken cancellationToken = default)
    {
        var healthResponse = new HealthResponse
        {
            Status = "checking",
            Timestamp = DateTime.UtcNow,
            Version = "2.0.0",
            Uptime = GetUptime()
        };

        var dependencies = new Dictionary<string, bool>();
        var allHealthy = true;

        // Check database connectivity
        try
        {
            await _dbContext.Database.CanConnectAsync();
            dependencies["database"] = true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed");
            dependencies["database"] = false;
            allHealthy = false;
        }

        // Check feature flag service
        try
        {
            var flags = await _featureFlagService.GetFeatureFlagsAsync();
            dependencies["feature-flag-service"] = flags is not null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Feature flag service health check failed");
            dependencies["feature-flag-service"] = false;
            allHealthy = false;
        }

        healthResponse.Dependencies = dependencies;
        healthResponse.Status = allHealthy ? "healthy" : "unhealthy";

        if (allHealthy)
        {
            return Ok(healthResponse);
        }
        else
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, healthResponse);
        }
    }

    private string GetUptime()
    {
        // Simple uptime calculation - in a real app, you'd track start time
        return $"{(DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime()).TotalSeconds:F0}s";
    }
}

public sealed class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Uptime { get; set; } = string.Empty;
    public Dictionary<string, bool>? Dependencies { get; set; }
}