#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;

namespace FeatureFlags.Models;

/// <summary>
/// Defines the strategy for rolling out a feature to users.
/// Supports percentage-based, rule-based, and A/B test rollout strategies.
/// </summary>
public sealed class RolloutStrategy
{
    /// <summary>
    /// Gets or sets the unique identifier of the rollout strategy.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the feature flag this strategy applies to.
    /// </summary>
    public int FeatureFlagId { get; set; }

    /// <summary>
    /// Gets or sets the type of rollout strategy (percentage, rule-based, or A/B test).
    /// </summary>
    public RolloutType Type { get; set; }

    /// <summary>
    /// Gets or sets the starting percentage of users included in the rollout.
    /// </summary>
    public int? StartPercentage { get; set; }

    /// <summary>
    /// Gets or sets the ending percentage of users included in the rollout.
    /// </summary>
    public int? EndPercentage { get; set; }

    /// <summary>
    /// Gets or sets the date when the rollout begins.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets the date when the rollout ends.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rollout increases gradually over time.
    /// </summary>
    public bool IsGradual { get; set; }

    /// <summary>
    /// Gets or sets the daily percentage increment used for gradual rollouts.
    /// </summary>
    public int? DailyIncrement { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the rollout strategy was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the rollout strategy was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    /// <summary>
    /// Gets or sets the feature flag associated with this rollout strategy.
    /// </summary>
    public FeatureFlag? FeatureFlag { get; set; }

    /// <summary>
    /// Returns a concise summary of the rollout type and percentage.
    /// </summary>
    public override string ToString()
        => $"RolloutStrategy {{ Type = {Type}, Percentage = {StartPercentage?.ToString() ?? "null"} }}";

    /// <summary>
    /// Calculates the current percentage allocation based on time and gradual rollout settings.
    /// </summary>
    public int GetCurrentPercentage()
    {
        if (!IsGradual || StartDate is null || DailyIncrement is null)
            return StartPercentage ?? 0;

        var daysElapsed = (DateTime.UtcNow - StartDate.Value).Days;
        var currentPercentage = (StartPercentage ?? 0) + (daysElapsed * DailyIncrement.Value);

        return Math.Min(currentPercentage, EndPercentage ?? 100);
    }

    /// <summary>
    /// Determines if the rollout is currently active based on date constraints.
    /// </summary>
    public bool IsActive()
    {
        var now = DateTime.UtcNow;

        if (StartDate.HasValue && now < StartDate)
            return false;

        if (EndDate.HasValue && now > EndDate)
            return false;

        return true;
    }

    /// <summary>
    /// Validates the rollout strategy configuration for consistency.
    /// </summary>
    public bool IsValid()
    {
        if (FeatureFlagId <= 0)
            return false;

        if (!Enum.IsDefined(typeof(RolloutType), Type))
            return false;

        if (StartPercentage.HasValue && (StartPercentage < 0 || StartPercentage > 100))
            return false;

        if (EndPercentage.HasValue && (EndPercentage < 0 || EndPercentage > 100))
            return false;

        if (StartPercentage.HasValue && EndPercentage.HasValue && StartPercentage > EndPercentage)
            return false;

        if (StartDate.HasValue && EndDate.HasValue && StartDate > EndDate)
            return false;

        return true;
    }

    /// <summary>
    /// Determines the remaining days until the rollout ends.
    /// </summary>
    public int GetRemainingDays()
    {
        if (!EndDate.HasValue)
            return int.MaxValue;

        var remaining = (EndDate.Value - DateTime.UtcNow).Days;
        return Math.Max(0, remaining);
    }
}
