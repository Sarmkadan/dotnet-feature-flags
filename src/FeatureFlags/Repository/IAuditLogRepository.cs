#nullable enable
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
    /// <summary>
    /// Retrieves all audit log entries associated with the specified feature flag.
    /// </summary>
    /// <param name="featureFlagId">The identifier of the feature flag.</param>
    /// <returns>A collection of audit log entries for the feature flag.</returns>
    Task<IEnumerable<AuditLog>> GetByFeatureFlagIdAsync(int featureFlagId);

    /// <summary>
    /// Retrieves all audit log entries created by the specified user.
    /// </summary>
    /// <param name="changedBy">The name of the user who made the change.</param>
    /// <returns>A collection of audit log entries created by the user.</returns>
    Task<IEnumerable<AuditLog>> GetByChangedByAsync(string changedBy);

    /// <summary>
    /// Retrieves all audit log entries created on or after the specified date and time.
    /// </summary>
    /// <param name="dateTime">The date and time to filter from.</param>
    /// <returns>A collection of audit log entries created since the given date and time.</returns>
    Task<IEnumerable<AuditLog>> GetSinceAsync(DateTime dateTime);

    /// <summary>
    /// Retrieves a page of audit log entries.
    /// </summary>
    /// <param name="pageNumber">The one-based page number to retrieve.</param>
    /// <param name="pageSize">The number of entries per page.</param>
    /// <returns>A collection of audit log entries for the requested page.</returns>
    Task<IEnumerable<AuditLog>> GetPagedAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Retrieves a page of audit log entries for the specified feature flag.
    /// </summary>
    /// <param name="featureFlagId">The identifier of the feature flag.</param>
    /// <param name="pageNumber">The one-based page number to retrieve.</param>
    /// <param name="pageSize">The number of entries per page.</param>
    /// <returns>A collection of audit log entries for the requested page.</returns>
    Task<IEnumerable<AuditLog>> GetByFeatureFlagIdPagedAsync(int featureFlagId, int pageNumber, int pageSize);

    /// <summary>
    /// Retrieves the total number of audit log entries for the specified feature flag.
    /// </summary>
    /// <param name="featureFlagId">The identifier of the feature flag.</param>
    /// <returns>The count of audit log entries for the feature flag.</returns>
    Task<int> GetCountByFeatureFlagIdAsync(int featureFlagId);

    /// <summary>
    /// Retrieves the most recent change for the specified feature flag.
    /// </summary>
    /// <param name="featureFlagId">The identifier of the feature flag.</param>
    /// <returns>The latest audit log entry, or <c>null</c> if none exists.</returns>
    Task<AuditLog?> GetLastChangeAsync(int featureFlagId);

    /// <summary>
    /// Retrieves all audit log entries within the specified date range.
    /// </summary>
    /// <param name="startDate">The inclusive start of the date range.</param>
    /// <param name="endDate">The inclusive end of the date range.</param>
    /// <returns>A collection of audit log entries within the date range.</returns>
    Task<IEnumerable<AuditLog>> GetChangesInRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Retrieves all audit log entries matching the specified action.
    /// </summary>
    /// <param name="action">The action to filter by.</param>
    /// <returns>A collection of audit log entries for the action.</returns>
    Task<IEnumerable<AuditLog>> GetByActionAsync(string action);

    /// <summary>
    /// Removes audit log entries older than the specified retention period.
    /// </summary>
    /// <param name="retentionDays">The number of days of audit history to retain.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CleanupOldLogsAsync(int retentionDays);
}
