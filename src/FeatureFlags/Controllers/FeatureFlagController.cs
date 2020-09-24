#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Controllers;

/// <summary>
/// API controller for feature flag operations.
/// Provides endpoints for evaluating, managing, and auditing feature flags.
/// </summary>
[ApiController]
[Route("api/[controller]")]
{public sealed class FeatureFlagController {
    private readonly IFeatureFlagService _featureFlagService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<FeatureFlagController> _logger;

    public FeatureFlagController(
        IFeatureFlagService featureFlagService,
        IAuditLogService auditLogService,
        ILogger<FeatureFlagController> logger)
    {
        _featureFlagService = featureFlagService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    /// <summary>
    /// Checks if a feature flag is enabled for the given user context.
    /// </summary>
    [HttpPost("evaluate")]
    public async Task<IActionResult> EvaluateFeatureFlag([FromBody] EvaluationRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrEmpty(request.FeatureFlagKey))
            return BadRequest("Feature flag key is required");

        try
        {
            var userContext = new UserContext
            {
                UserId = request.UserId,
                Email = request.Email,
                Country = request.Country,
                Tier = request.Tier,
                Region = request.Region,
                CustomAttributes = request.CustomAttributes ?? new Dictionary<string, string>()
            };

            if (!userContext.IsValid())
                return BadRequest("User context must have userId and email");

            var isEnabled = await _featureFlagService.IsEnabledAsync(request.FeatureFlagKey, userContext);
            return Ok(new { enabled = isEnabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating feature flag");
            return StatusCode(500, "Error evaluating feature flag");
        }
    }

    /// <summary>
    /// Gets the A/B test variant for a user.
    /// </summary>
    [HttpPost("variant")]
    public async Task<IActionResult> GetVariant([FromBody] EvaluationRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrEmpty(request.FeatureFlagKey))
            return BadRequest("Feature flag key is required");

        try
        {
            var userContext = new UserContext
            {
                UserId = request.UserId,
                Email = request.Email,
                Country = request.Country,
                Tier = request.Tier,
                Region = request.Region,
                CustomAttributes = request.CustomAttributes ?? new Dictionary<string, string>()
            };

            if (!userContext.IsValid())
                return BadRequest("User context must have userId and email");

            var variant = await _featureFlagService.GetVariantAsync(request.FeatureFlagKey, userContext);
            return Ok(new { variant });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving variant");
            return StatusCode(500, "Error retrieving variant");
        }
    }

    /// <summary>
    /// Gets all feature flags.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
    {
        try
        {
            var flags = await _featureFlagService.GetAllFeatureFlagsAsync();
            return Ok(flags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature flags");
            return StatusCode(500, "Error retrieving feature flags");
        }
    }

    /// <summary>
    /// Gets a feature flag by key.
    /// </summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(key))
            return BadRequest("Key is required");

        try
        {
            var flag = await _featureFlagService.GetFeatureFlagByKeyAsync(key);
            if (flag is null)
                return NotFound($"Feature flag '{key}' not found");

            return Ok(flag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feature flag");
            return StatusCode(500, "Error retrieving feature flag");
        }
    }

    /// <summary>
    /// Creates a new feature flag.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FeatureFlag featureFlag, CancellationToken cancellationToken = default)
    {
        if (featureFlag is null || !featureFlag.IsValid())
            return BadRequest("Invalid feature flag configuration");

        try
        {
            var created = await _featureFlagService.CreateFeatureFlagAsync(featureFlag, User.Identity?.Name ?? "System");
            return CreatedAtAction(nameof(GetByKey), new { key = created.Key }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feature flag");
            return StatusCode(500, "Error creating feature flag");
        }
    }

    /// <summary>
    /// Updates a feature flag.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] FeatureFlag featureFlag, CancellationToken cancellationToken = default)
    {
        if (featureFlag is null || featureFlag.Id != id)
            return BadRequest("Invalid feature flag");

        try
        {
            await _featureFlagService.UpdateFeatureFlagAsync(featureFlag, User.Identity?.Name ?? "System");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feature flag");
            return StatusCode(500, "Error updating feature flag");
        }
    }

    /// <summary>
    /// Enables a feature flag.
    /// </summary>
    [HttpPost("{id}/enable")]
    public async Task<IActionResult> Enable(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _featureFlagService.EnableFeatureFlagAsync(id, User.Identity?.Name ?? "System");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling feature flag");
            return StatusCode(500, "Error enabling feature flag");
        }
    }

    /// <summary>
    /// Disables a feature flag.
    /// </summary>
    [HttpPost("{id}/disable")]
    public async Task<IActionResult> Disable(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _featureFlagService.DisableFeatureFlagAsync(id, User.Identity?.Name ?? "System");
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling feature flag");
            return StatusCode(500, "Error disabling feature flag");
        }
    }

    /// <summary>
    /// Gets audit logs for a feature flag.
    /// </summary>
    [HttpGet("{id}/audit")]
    public async Task<IActionResult> GetAuditLogs(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var logs = await _auditLogService.GetAuditLogsAsync(id);
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs");
            return StatusCode(500, "Error retrieving audit logs");
        }
    }
}

/// <summary>
/// Request model for feature flag evaluation.
/// </summary>
public sealed class EvaluationRequest
{
    public string FeatureFlagKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Tier { get; set; }
    public string? Region { get; set; }
}
