#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json.Serialization;

namespace FeatureFlags.Models;

/// <summary>
/// Result of evaluating a single feature flag in bulk evaluation.
/// </summary>
public sealed class BulkEvaluationResult
{
    /// <summary>
    /// Indicates whether the feature flag is enabled for the user.
    /// </summary>
    [JsonRequired]
    public required bool Enabled { get; set; }

    /// <summary>
    /// The variant key if this is an A/B test flag, otherwise null.
    /// Only populated if includeVariants was set to true in the request.
    /// </summary>
    public string? Variant { get; set; }

    /// <summary>
    /// The reason for the evaluation result.
    /// Examples: "PercentageRollout", "RulesBased", "ABTest", "Full", "FlagDisabled", "FlagNotFound"
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// The percentage rollout value if applicable (0-100).
    /// Only populated if Reason indicates percentage-based evaluation.
    /// </summary>
    public int? Percentage { get; set; }
}
