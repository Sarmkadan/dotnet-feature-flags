#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Service interface for audit log operations.
/// Provides audit trail queries and cleanup functionality.
/// </summary>
public interface IAuditLogService
{
    Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int featureFlagId);

    Task<IEnumerable<AuditLog>> GetAuditLogsPagedAsync(int featureFlagId, int pageNumber, int pageSize);

    Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(string changedBy);

    Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count);

    Task<AuditLog?> GetLastChangeAsync(int featureFlagId);

    Task<IEnumerable<AuditLog>> GetChangeHistoryAsync(DateTime startDate, DateTime endDate);

    Task CleanupOldLogsAsync(int retentionDays);
}
