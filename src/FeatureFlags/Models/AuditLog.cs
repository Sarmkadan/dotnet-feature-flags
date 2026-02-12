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
public class AuditLog
{
    public int Id { get; set; }

    public int FeatureFlagId { get; set; }

    public AuditAction Action { get; set; }

    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public string OldValue { get; set; } = string.Empty;

    public string NewValue { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    // Navigation properties
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
        if (previousLog == null)
            return false;

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
}
