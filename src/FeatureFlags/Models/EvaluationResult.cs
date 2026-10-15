#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace FeatureFlags.Models;

/// <summary>
/// Outcome of evaluating a feature flag for a user, together with a human-readable
/// <see cref="Reason"/> explaining why that value was returned.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public sealed class EvaluationResult
{
    private EvaluationResult(bool enabled, string reason, int? matchedRuleId = null, int? bucket = null, int? percentage = null)
    {
        Enabled = enabled;
        Reason = reason;
        MatchedRuleId = matchedRuleId;
        Bucket = bucket;
        Percentage = percentage;
    }

    /// <summary>
    /// Gets a value indicating whether the flag is enabled for the user.
    /// </summary>
    public bool Enabled { get; }

    /// <summary>
    /// Gets the reason the value was returned, for example "matched rule 42" or "flag disabled".
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Gets the identifier of the targeting rule that matched, when the decision came from a rule.
    /// </summary>
    public int? MatchedRuleId { get; }

    /// <summary>
    /// Gets the rollout bucket (0 to 99) the user was assigned to, when the decision came from a percentage rollout.
    /// </summary>
    public int? Bucket { get; }

    /// <summary>
    /// Gets the rollout percentage the bucket was compared against, when the decision came from a percentage rollout.
    /// </summary>
    public int? Percentage { get; }

    /// <summary>
    /// Creates a result for a flag that does not exist.
    /// </summary>
    public static EvaluationResult FlagNotFound() => new(false, "flag not found");

    /// <summary>
    /// Creates a result for a flag that is switched off.
    /// </summary>
    public static EvaluationResult FlagDisabled() => new(false, "flag disabled");

    /// <summary>
    /// Creates a result for a flag whose rollout type is <c>None</c>.
    /// </summary>
    public static EvaluationResult RolloutNone() => new(false, "rollout type none");

    /// <summary>
    /// Creates a result for a flag that is fully rolled out.
    /// </summary>
    public static EvaluationResult FullRollout() => new(true, "full rollout");

    /// <summary>
    /// Creates a result for a targeting rule that matched the user.
    /// </summary>
    /// <param name="ruleId">The identifier of the matched rule.</param>
    public static EvaluationResult RuleMatched(int ruleId) => new(true, $"matched rule {ruleId}", matchedRuleId: ruleId);

    /// <summary>
    /// Creates a result for a flag where no targeting rule matched, so the default (disabled) value applies.
    /// </summary>
    public static EvaluationResult Fallthrough() => new(false, "fallthrough: no rule matched");

    /// <summary>
    /// Creates a result for a percentage rollout. The user is enabled when their bucket is below the percentage.
    /// </summary>
    /// <param name="bucket">The user's rollout bucket (0 to 99).</param>
    /// <param name="percentage">The rollout percentage (0 to 100).</param>
    public static EvaluationResult RolloutBucket(int bucket, int percentage)
    {
        var enabled = bucket < percentage;
        var comparison = enabled ? "<" : ">=";
        return new(enabled, $"rollout bucket {bucket} {comparison} {percentage}%", bucket: bucket, percentage: percentage);
    }

    /// <summary>
    /// Returns the <see cref="Reason"/> of this result.
    /// </summary>
    public override string ToString() => Reason;
}
