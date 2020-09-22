#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;

namespace FeatureFlags.Models;

/// <summary>
/// Represents a feature flag with rollout strategies and targeting rules.
/// Supports percentage-based rollouts, user targeting, and A/B testing variants.
/// </summary>
public sealed class FeatureFlag
{
    public int Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public RolloutType RolloutType { get; set; } = RolloutType.Percentage;

    public int? PercentageRollout { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public string UpdatedBy { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<Rule> Rules { get; set; } = new List<Rule>();

    public ICollection<ABTestVariant> Variants { get; set; } = new List<ABTestVariant>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    /// <summary>
    /// Validates the feature flag configuration ensuring consistency between rollout type and settings.
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Key) || Key.Length > 100)
            return false;

        if (string.IsNullOrWhiteSpace(DisplayName) || DisplayName.Length > 200)
            return false;

        if (Description?.Length > 1000)
            return false;

        if (CreatedBy?.Length > 100)
            return false;

        if (UpdatedBy?.Length > 100)
            return false;

        if (RolloutType == RolloutType.Percentage && (PercentageRollout < 0 || PercentageRollout > 100))
            return false;

        if (RolloutType == RolloutType.ABTest && (!Variants?.Any() ?? true))
            return false;

        return true;
    }

    /// <summary>
    /// Creates a snapshot of the current feature flag state for auditing purposes.
    /// </summary>
    public string GetSnapshot()
    {
        return $"Key:{Key}|IsEnabled:{IsEnabled}|RolloutType:{RolloutType}|Percentage:{PercentageRollout}|VariantCount:{Variants?.Count}";
    }
}
