#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace FeatureFlags.Controllers;

/// <summary>
/// API controller for feature flag operations.
/// Provides endpoints for evaluating, managing, and auditing feature flags.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FeatureFlagController : ControllerBase {
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

    public string FeatureFlagKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? Tier { get; set; }
    public string? Region { get; set; }

    public override string ToString() => $"FeatureFlagController {{ FeatureFlagKey = {FeatureFlagKey}, UserId = {UserId}, Email = {Email}, Country = {Country}, Tier = {Tier}, Region = {Region} }}";

    /// <summary>
    /// Checks if a feature flag is enabled for the given user context.
    /// </summary>
    [HttpPost("evaluate")]
    public async Task<IActionResult> EvaluateFeatureFlag([FromBody] EvaluationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

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
        ArgumentNullException.ThrowIfNull(request);

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
        ArgumentException.ThrowIfNullOrEmpty(key);

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
        ArgumentNullException.ThrowIfNull(featureFlag);

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
        ArgumentNullException.ThrowIfNull(featureFlag);

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

    /// <summary>
    /// Evaluates all active feature flags for a given user context in a single batch.
    /// This endpoint is optimized for session initialization where clients need all flag states.
    /// </summary>
    /// <param name="request">The bulk evaluation request containing user context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of flag evaluations keyed by flag name.</returns>
    [HttpPost("evaluate/all")]
    public async Task<IActionResult> EvaluateAll([FromBody] BulkEvaluationRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request is null)
            return BadRequest("Request body is required");

        if (request.UserContext is null || !request.UserContext.IsValid())
            return BadRequest("User context must have userId and email");

        try
        {
            var results = await _featureFlagService.EvaluateAllAsync(
                request.UserContext,
                request.IncludeVariants,
                request.IncludeReasons,
                cancellationToken
            );

            // Convert service results to controller response format
            var responseResults = new Dictionary<string, FlagEvaluationResult>();
            foreach (var kvp in results)
            {
                responseResults[kvp.Key] = new FlagEvaluationResult
                {
                    Enabled = kvp.Value.Enabled,
                    Variant = kvp.Value.Variant,
                    Reason = kvp.Value.Reason,
                    Percentage = kvp.Value.Percentage
                };
            }

            var response = new BulkEvaluationResponse
            {
                Results = responseResults,
                ETag = request.IncludeETag ? await _featureFlagService.GetFeatureFlagsETagAsync(cancellationToken) : null
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk feature flag evaluation");
            return StatusCode(500, "Error during bulk feature flag evaluation");
        }
    }

    /// <summary>
    /// Evaluates all active feature flags for a given user context using GET method.
    /// This endpoint supports ETag caching for efficient session initialization.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="email">The user email.</param>
    /// <param name="country">Optional user country.</param>
    /// <param name="tier">Optional user tier.</param>
    /// <param name="region">Optional user region.</param>
    /// <param name="includeVariants">Whether to include variant information.</param>
    /// <param name="includeReasons">Whether to include evaluation reasons.</param>
    /// <param name="includeETag">Whether to include ETag for caching.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary of flag evaluations keyed by flag name.</returns>
    [HttpGet("evaluate/all")]
    public async Task<IActionResult> EvaluateAllGet(
        [FromQuery] string userId,
        [FromQuery] string email,
        [FromQuery] string? country = null,
        [FromQuery] string? tier = null,
        [FromQuery] string? region = null,
        [FromQuery] bool includeVariants = false,
        [FromQuery] bool includeReasons = false,
        [FromQuery] bool includeETag = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentException.ThrowIfNullOrEmpty(email);

        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(email))
            return BadRequest("userId and email query parameters are required");

        try
        {
            var userContext = new UserContext
            {
                UserId = userId,
                Email = email,
                Country = country,
                Tier = tier,
                Region = region,
                CustomAttributes = new Dictionary<string, string>()
            };

            // Check ETag for caching
            if (includeETag)
            {
                var currentETag = await _featureFlagService.GetFeatureFlagsETagAsync(cancellationToken);
                if (Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) && ifNoneMatch == currentETag)
                {
                    return StatusCode(304); // Not Modified
                }

                Response.Headers.Append("ETag", currentETag);
            }

            var results = await _featureFlagService.EvaluateAllAsync(
                userContext,
                includeVariants,
                includeReasons,
                cancellationToken
            );

            // Convert service results to controller response format
            var responseResults = new Dictionary<string, FlagEvaluationResult>();
            foreach (var kvp in results)
            {
                responseResults[kvp.Key] = new FlagEvaluationResult
                {
                    Enabled = kvp.Value.Enabled,
                    Variant = kvp.Value.Variant,
                    Reason = kvp.Value.Reason,
                    Percentage = kvp.Value.Percentage
                };
            }

            var response = new BulkEvaluationResponse
            {
                Results = responseResults,
                ETag = includeETag ? await _featureFlagService.GetFeatureFlagsETagAsync(cancellationToken) : null
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bulk feature flag evaluation (GET)");
            return StatusCode(500, "Error during bulk feature flag evaluation");
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
    public Dictionary<string, string>? CustomAttributes { get; set; }
}

/// <summary>
/// Request model for bulk evaluation of all feature flags for a user.
/// </summary>
public sealed class BulkEvaluationRequest
{
    /// <summary>
    /// The user context containing user identity and attributes.
    /// </summary>
    [JsonRequired]
    public required UserContext UserContext { get; set; }

    /// <summary>
    /// Optional flag to include variant information in the response.
    /// When true, includes variant data for A/B test flags.
    /// Defaults to false.
    /// </summary>
    public bool IncludeVariants { get; set; } = false;

    /// <summary>
    /// Optional flag to include detailed evaluation reasons in the response.
    /// When true, includes the reason each flag was enabled/disabled.
    /// Defaults to false.
    /// </summary>
    public bool IncludeReasons { get; set; } = false;

    /// <summary>
    /// Optional flag to include the feature flag configuration hash in the response.
    /// When true, includes an ETag for caching purposes.
    /// Defaults to false.
    /// </summary>
    public bool IncludeETag { get; set; } = false;
}

/// <summary>
/// Response model for bulk feature flag evaluation.
/// </summary>
public sealed class BulkEvaluationResponse
{
    /// <summary>
    /// Dictionary mapping feature flag keys to their evaluation results.
    /// </summary>
    [JsonRequired]
    public required Dictionary<string, FlagEvaluationResult> Results { get; set; }

    /// <summary>
    /// Optional ETag/hash of the feature flag configuration for caching.
    /// Only included if IncludeETag was set to true in the request.
    /// </summary>
    public string? ETag { get; set; }
}

/// <summary>
/// Result of evaluating a single feature flag.
/// </summary>
public sealed class FlagEvaluationResult
{
    /// <summary>
    /// Indicates whether the feature flag is enabled for the user.
    /// </summary>
    [JsonRequired]
    public required bool Enabled { get; set; }

    /// <summary>
    /// The variant key if this is an A/B test flag, otherwise null.
    /// Only included if IncludeVariants was set to true in the request.
    /// </summary>
    public string? Variant { get; set; }

    /// <summary>
    /// The reason for the evaluation result.
    /// Only included if IncludeReasons was set to true in the request.
    /// Examples: "PercentageRollout", "RulesBased", "ABTest", "Full", "FlagDisabled", "FlagNotFound"
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// The percentage rollout value if applicable (0-100).
    /// Only included if IncludeReasons was set to true and the flag uses percentage rollout.
    /// </summary>
    public int? Percentage { get; set; }
}
