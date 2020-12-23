#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using FeatureFlags.Enums;

namespace FeatureFlags.Models;

/// <summary>
/// Provides extension methods for AuditLog to enhance functionality for common scenarios
/// such as change analysis, rollback operations, and audit trail queries.
/// </summary>
public static class AuditLogExtensions
{
    /// <summary>
    /// Determines if this audit log entry represents a state change (as opposed to creation or deletion).
    /// </summary>
    /// <param name="log">The audit log entry to check</param>
    /// <returns>True if the action is Updated, Enabled, or Disabled; false otherwise</returns>
    public static bool IsStateChange(this AuditLog log)
    {
        return log.Action is AuditAction.Updated or AuditAction.Enabled or AuditAction.Disabled;
    }

    /// <summary>
    /// Determines if this audit log entry represents a creation event.
    /// </summary>
    /// <param name="log">The audit log entry to check</param>
    /// <returns>True if the action is Created; false otherwise</returns>
    public static bool IsCreation(this AuditLog log)
    {
        return log.Action == AuditAction.Created;
    }

    /// <summary>
    /// Determines if this audit log entry represents a deletion event.
    /// </summary>
    /// <param name="log">The audit log entry to check</param>
    /// <returns>True if the action is Deleted; false otherwise</returns>
    public static bool IsDeletion(this AuditLog log)
    {
        return log.Action == AuditAction.Deleted;
    }

    /// <summary>
    /// Gets the duration since this change was made in a human-readable format.
    /// </summary>
    /// <param name="log">The audit log entry</param>
    /// <param name="format">Optional format string for TimeSpan (default: "d' days, 'h' hours, 'm' minutes ago")</param>
    /// <returns>Formatted time duration string</returns>
    public static string GetTimeSinceChange(this AuditLog log, string format = "d' days, 'h' hours, 'm' minutes ago")
    {
        var duration = DateTime.UtcNow - log.ChangedAt;

        if (duration.TotalMinutes < 1)
        {
            return "just now";
        }

        if (duration.TotalHours < 1)
        {
            return $"{duration.Minutes} minutes ago";
        }

        if (duration.TotalDays < 1)
        {
            return $"{duration.Hours} hours ago";
        }

        return format.Replace("d", duration.Days.ToString())
                     .Replace("h", duration.Hours.ToString())
                     .Replace("m", duration.Minutes.ToString());
    }

    /// <summary>
    /// Creates a detailed change description that includes both the summary and change details.
    /// </summary>
    /// <param name="log">The audit log entry</param>
    /// <returns>Formatted string with full change details</returns>
    public static string GetDetailedChangeDescription(this AuditLog log)
    {
        var summary = log.GetSummary();
        var (oldState, newState) = log.GetChangeDetails();

        return $"{summary}\nOld: {oldState}\nNew: {newState}";
    }

    /// <summary>
    /// Determines if this audit log entry is recent (within the specified time threshold).
    /// </summary>
    /// <param name="log">The audit log entry to check</param>
    /// <param name="thresholdMinutes">Time threshold in minutes (default: 30)</param>
    /// <returns>True if the change was made within the threshold; false otherwise</returns>
    public static bool IsRecent(this AuditLog log, int thresholdMinutes = 30)
    {
        return DateTime.UtcNow - log.ChangedAt <= TimeSpan.FromMinutes(thresholdMinutes);
    }

    /// <summary>
    /// Gets a simplified action name suitable for UI display.
    /// </summary>
    /// <param name="log">The audit log entry</param>
    /// <returns>Simplified action name (e.g., "Updated", "Created", "Deleted")</returns>
    public static string GetActionDisplayName(this AuditLog log)
    {
        return log.Action switch
        {
            AuditAction.Created => "Created",
            AuditAction.Updated => "Updated",
            AuditAction.Enabled => "Enabled",
            AuditAction.Disabled => "Disabled",
            AuditAction.Deleted => "Deleted",
            _ => log.Action.ToString()
        };
    }
}