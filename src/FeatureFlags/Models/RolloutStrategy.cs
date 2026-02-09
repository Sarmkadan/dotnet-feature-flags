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
public class RolloutStrategy
{
    public int Id { get; set; }

    public int FeatureFlagId { get; set; }

    public RolloutType Type { get; set; }

    public int? StartPercentage { get; set; }

    public int? EndPercentage { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public bool IsGradual { get; set; }

    public int? DailyIncrement { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public FeatureFlag? FeatureFlag { get; set; }

    /// <summary>
    /// Calculates the current percentage allocation based on time and gradual rollout settings.
    /// </summary>
    public int GetCurrentPercentage()
    {
        if (!IsGradual || StartDate == null || DailyIncrement == null)
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
