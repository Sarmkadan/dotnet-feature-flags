#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json.Serialization;

namespace FeatureFlags.Models;

/// <summary>
/// Request model for bulk evaluation of all feature flags for a user.
/// Contains user context and optional parameters for the evaluation.
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
/// Contains the evaluation results for all active feature flags.
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

    /// <summary>
    /// Timestamp when the evaluation was performed.
    /// </summary>
    [JsonIgnore]
    public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;
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
