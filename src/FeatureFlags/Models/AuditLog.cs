#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;

namespace FeatureFlags.Models;

/// <summary>
/// Records all changes to feature flags for compliance, debugging, and audit trail requirements.
/// Tracks who made what changes and when, enabling rollback analysis and change history review.
/// </summary>
public sealed class AuditLog
{
    /// <summary>
    /// Gets or sets the unique identifier of the audit log entry.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the feature flag that was changed.
    /// </summary>
    public int FeatureFlagId { get; set; }

    /// <summary>
    /// Gets or sets the type of action performed on the feature flag.
    /// </summary>
    public AuditAction Action { get; set; }

    /// <summary>
    /// Gets or sets the identifier or name of the user who made the change.
    /// </summary>
    public string ChangedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC timestamp when the change was made.
    /// </summary>
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the value of the feature flag before the change.
    /// </summary>
    public string OldValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the feature flag after the change.
    /// </summary>
    public string NewValue { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable description of the change.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the IP address from which the change was made.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the user agent of the client that made the change.
    /// </summary>
    public string? UserAgent { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the feature flag associated with this audit log entry.
    /// </summary>
    public FeatureFlag? FeatureFlag { get; set; }

    /// <summary>
    /// Creates a human-readable summary of the change for logging and UI display.
    /// </summary>
    public string GetSummary()
    {
        return $"{Action} by {ChangedBy} at {ChangedAt:yyyy-MM-dd HH:mm:ss}: {Description}";
    }

    /// <summary>
    /// Determines if this change is a rollback compared to another audit log entry.
    /// </summary>
    public bool IsRollbackOf(AuditLog? previousLog)
    {
        ArgumentNullException.ThrowIfNull(previousLog);

        return NewValue == previousLog.OldValue && OldValue == previousLog.NewValue;
    }

    /// <summary>
    /// Validates audit log has required fields for storage and retrieval.
    /// </summary>
    public bool IsValid()
    {
        if (FeatureFlagId <= 0)
            return false;

        if (string.IsNullOrWhiteSpace(ChangedBy))
            return false;

        if (!Enum.IsDefined(typeof(AuditAction), Action))
            return false;

        return true;
    }

    /// <summary>
    /// Generates a change summary comparing old and new values for analysis.
    /// </summary>
    public (string oldState, string newState) GetChangeDetails()
    {
        return (OldValue, NewValue);
    }

    public override string ToString() => $"AuditLog {{ FeatureFlagId = {FeatureFlagId}, Action = {Action}, ChangedAt = {ChangedAt:yyyy-MM-dd HH:mm:ss} }}";
}
