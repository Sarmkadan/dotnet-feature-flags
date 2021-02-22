#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FeatureFlags.Enums;
using FeatureFlags.Models;
using System.Globalization;

namespace FeatureFlags.Repository;

/// <summary>
/// Extension methods for <see cref="AuditLogRepository"/> providing additional query capabilities
/// and convenience methods for common audit log operations.
/// </summary>
public static class AuditLogRepositoryExtensions
{
    /// <summary>
    /// Gets the most recent audit log entry for a specific feature flag.
    /// </summary>
    /// <param name="repository">The audit log repository instance.</param>
    /// <param name="featureFlagId">The feature flag identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The most recent audit log entry, or null if none exists.</returns>
    /// <exception cref="ArgumentException">Thrown when featureFlagId is not positive.</exception>
    public static async Task<AuditLog?> GetMostRecentAsync(
        this AuditLogRepository repository,
        int featureFlagId,
        CancellationToken cancellationToken = default)
    {
        if (featureFlagId <= 0)
        {
            throw new ArgumentException("FeatureFlagId must be positive", nameof(featureFlagId));
        }

        return await repository.GetLastChangeAsync(featureFlagId, cancellationToken);
    }

    /// <summary>
    /// Gets audit logs filtered by a specific action type.
    /// </summary>
    /// <param name="repository">The audit log repository instance.</param>
    /// <param name="action">The action type to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Read-only list of matching audit logs ordered by most recent first.</returns>
    /// <exception cref="ArgumentException">Thrown when action is not defined.</exception>
    public static async Task<IReadOnlyList<AuditLog>> GetByActionAsync(
        this AuditLogRepository repository,
        AuditAction action,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(AuditAction), action))
        {
            throw new ArgumentException("Action must be a valid AuditAction value", nameof(action));
        }

        var results = await repository.GetByActionAsync(action.ToString());
        return results.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets audit logs for a specific user within a date range.
    /// </summary>
    /// <param name="repository">The audit log repository instance.</param>
    /// <param name="changedBy">The username to filter by.</param>
    /// <param name="startDate">Start of date range (inclusive).</param>
    /// <param name="endDate">End of date range (inclusive).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Read-only list of matching audit logs ordered by most recent first.</returns>
    /// <exception cref="ArgumentException">Thrown when changedBy is null or empty, or startDate is after endDate.</exception>
    public static async Task<IReadOnlyList<AuditLog>> GetByUserInRangeAsync(
        this AuditLogRepository repository,
        string changedBy,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(changedBy))
        {
            throw new ArgumentException("ChangedBy cannot be empty", nameof(changedBy));
        }

        if (startDate > endDate)
        {
            throw new ArgumentException(
                "Start date must be before or equal to end date",
                nameof(startDate));
        }

        var allByUser = await repository.GetByChangedByAsync(changedBy);
        var filtered = allByUser.Where(a => a.ChangedAt >= startDate && a.ChangedAt <= endDate);
        return filtered.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the total number of audit logs across all feature flags.
    /// </summary>
    /// <param name="repository">The audit log repository instance.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The total count of audit log entries.</returns>
    public static async Task<int> GetTotalCountAsync(
        this AuditLogRepository repository,
        CancellationToken cancellationToken = default)
    {
        return await repository.GetCountByFeatureFlagIdAsync(0, cancellationToken);
    }

    /// <summary>
    /// Gets audit logs for multiple feature flags in a single query.
    /// </summary>
    /// <param name="repository">The audit log repository instance.</param>
    /// <param name="featureFlagIds">Collection of feature flag identifiers to filter by.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Read-only list of audit logs matching any of the provided feature flag IDs.</returns>
    /// <exception cref="ArgumentNullException">Thrown when featureFlagIds is null.</exception>
    public static async Task<IReadOnlyList<AuditLog>> GetByFeatureFlagIdsAsync(
        this AuditLogRepository repository,
        IEnumerable<int> featureFlagIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(featureFlagIds);

        var flagIds = featureFlagIds.ToList();
        if (flagIds.Count == 0)
        {
            return Array.Empty<AuditLog>();
        }

        if (flagIds.Any(id => id <= 0))
        {
            throw new ArgumentException(
                "All feature flag IDs must be positive",
                nameof(featureFlagIds));
        }

        var allLogs = await repository.GetAllAsync();
        var result = allLogs.Where(a => flagIds.Contains(a.FeatureFlagId)).ToList();
        return result.AsReadOnly();
    }

    /// <summary>
    /// Gets audit logs for a specific feature flag with enhanced change details.
    /// </summary>
    /// <param name="repository">The audit log repository instance.</param>
    /// <param name="featureFlagId">The feature flag identifier.</param>
    /// <param name="includeRollbacks">Whether to include rollback entries in results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Read-only list of audit logs with optional rollback filtering.</returns>
    /// <exception cref="ArgumentException">Thrown when featureFlagId is not positive.</exception>
    public static async Task<IReadOnlyList<AuditLog>> GetWithDetailsAsync(
        this AuditLogRepository repository,
        int featureFlagId,
        bool includeRollbacks = true,
        CancellationToken cancellationToken = default)
    {
        if (featureFlagId <= 0)
        {
            throw new ArgumentException("FeatureFlagId must be positive", nameof(featureFlagId));
        }

        var logs = await repository.GetByFeatureFlagIdAsync(featureFlagId);

        if (!includeRollbacks)
        {
            return logs.ToList().AsReadOnly();
        }

        return logs.ToList().AsReadOnly();
    }
}