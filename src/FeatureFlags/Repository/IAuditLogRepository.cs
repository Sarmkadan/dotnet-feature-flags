// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;

namespace FeatureFlags.Repository;

/// <summary>
/// Repository interface for audit log data access operations.
/// Provides queries for retrieving audit trails and change history.
/// </summary>
public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetByFeatureFlagIdAsync(int featureFlagId);

    Task<IEnumerable<AuditLog>> GetByChangedByAsync(string changedBy);

    Task<IEnumerable<AuditLog>> GetSinceAsync(DateTime dateTime);

    Task<IEnumerable<AuditLog>> GetPagedAsync(int pageNumber, int pageSize);

    Task<IEnumerable<AuditLog>> GetByFeatureFlagIdPagedAsync(int featureFlagId, int pageNumber, int pageSize);

    Task<int> GetCountByFeatureFlagIdAsync(int featureFlagId);

    Task<AuditLog?> GetLastChangeAsync(int featureFlagId);

    Task<IEnumerable<AuditLog>> GetChangesInRangeAsync(DateTime startDate, DateTime endDate);

    Task<IEnumerable<AuditLog>> GetByActionAsync(string action);

    Task CleanupOldLogsAsync(int retentionDays);
}
