#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Models;

/// <summary>
/// Records a single feature flag evaluation event, capturing the flag name,
/// user identity, outcome, and reasoning for debugging "why did user X see feature Y".
/// </summary>
public sealed class FlagEvaluationLog
{
    /// <summary>The key of the feature flag that was evaluated.</summary>
    public string FlagName { get; init; } = string.Empty;

    /// <summary>The user identifier from the evaluation context.</summary>
    public string UserId { get; init; } = string.Empty;

    /// <summary>The boolean result returned to the caller.</summary>
    public bool Result { get; init; }

    /// <summary>UTC timestamp of when the evaluation occurred.</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Short explanation of why the result was produced
    /// (e.g. "FlagDisabled", "PercentageRollout", "RulesBased", "Full", "FlagNotFound").
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}
