#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Services;

/// <summary>
/// Service interface for scheduling and managing gradual feature flag rollouts.
/// Supports time-based percentage advancement with configurable daily increment steps.
/// </summary>
public interface IGradualRolloutSchedulerService
{
    /// <summary>
    /// Processes all active gradual rollout strategies and advances percentage allocations
    /// based on elapsed time and configured daily increment values.
    /// Returns the number of feature flags that had their rollout percentage updated.
    /// </summary>
    Task<int> ProcessScheduledRolloutsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current schedule status for a specific feature flag's gradual rollout.
    /// Returns <c>null</c> if the flag has no active gradual rollout strategy.
    /// </summary>
    Task<RolloutScheduleStatus?> GetScheduleStatusAsync(int featureFlagId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually advances the rollout percentage for a feature flag to its computed current value.
    /// Returns <c>true</c> if the percentage was updated; <c>false</c> if already at the computed target.
    /// </summary>
    Task<bool> AdvanceRolloutAsync(int featureFlagId, string advancedBy, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the current status and progress of a gradual rollout schedule for a feature flag.
/// </summary>
public sealed class RolloutScheduleStatus
{
    /// <summary>
    /// The feature flag identifier this schedule belongs to.
    /// </summary>
    public int FeatureFlagId { get; set; }

    /// <summary>
    /// Unique string key of the feature flag.
    /// </summary>
    public string FeatureFlagKey { get; set; } = string.Empty;

    /// <summary>
    /// The percentage currently applied to the feature flag in the database.
    /// </summary>
    public int CurrentPercentage { get; set; }

    /// <summary>
    /// The target end percentage the gradual rollout is advancing toward.
    /// </summary>
    public int TargetPercentage { get; set; }

    /// <summary>
    /// Percentage points added to the rollout each day.
    /// </summary>
    public int? DailyIncrement { get; set; }

    /// <summary>
    /// Scheduled start date when the gradual rollout begins.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Scheduled end date when the rollout should reach the target percentage.
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Indicates whether the rollout is currently within its active date window.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Indicates whether the rollout has reached or exceeded its target percentage.
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Estimated remaining days until the rollout reaches the target percentage at the current increment.
    /// </summary>
    public int EstimatedDaysRemaining { get; set; }
}
