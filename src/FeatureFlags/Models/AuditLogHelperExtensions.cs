#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using FeatureFlags.Enums;

namespace FeatureFlags.Models;

/// <summary>
/// Helper extension methods for AuditLog that provide additional formatting and utility functions
/// not covered by the main AuditLogExtensions class.
/// </summary>
public static class AuditLogHelperExtensions
{
    /// <summary>
    /// Gets a compact, formatted summary string suitable for logging and notifications.
    /// Format: [Action] FeatureFlagId by ChangedBy at HH:mm:ss
    /// </summary>
    /// <param name="log">The audit log entry to format.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="log"/> is <see langword="null"/>.</exception>
    /// <returns>Compact formatted summary string.</returns>
    public static string GetFormattedSummary(this AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);
        return $"[{log.Action}] FeatureFlagId={log.FeatureFlagId} by {log.ChangedBy} at {log.ChangedAt:HH:mm:ss}";
    }
}