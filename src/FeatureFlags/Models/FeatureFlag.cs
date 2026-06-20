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
    /// <summary>Gets or sets the unique identifier of the feature flag.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the unique key of the feature flag.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Gets or sets the display name of the feature flag.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Gets or sets the description of the feature flag.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the feature flag is enabled.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Gets or sets the type of rollout strategy used for this flag.</summary>
    public RolloutType RolloutType { get; set; } = RolloutType.Percentage;

    /// <summary>Gets or sets the percentage for percentage-based rollouts (0-100).</summary>
    public int? PercentageRollout { get; set; }

    /// <summary>Gets or sets the date and time when the feature flag was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Gets or sets the date and time when the feature flag was last updated.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>Gets or sets the user who created the feature flag.</summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets the user who last updated the feature flag.</summary>
    public string UpdatedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets the collection of targeting rules associated with this flag.</summary>
    public ICollection<Rule> Rules { get; set; } = new List<Rule>();

    /// <summary>Gets or sets the collection of A/B test variants associated with this flag.</summary>
    public ICollection<ABTestVariant> Variants { get; set; } = new List<ABTestVariant>();

    /// <summary>Gets or sets the collection of audit logs for this flag.</summary>
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    /// <summary>
    /// Validates the feature flag configuration ensuring consistency between rollout type and settings.
    /// </summary>
    /// <returns>True if the configuration is valid, false otherwise.</returns>
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
