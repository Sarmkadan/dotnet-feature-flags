#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Constants;
using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using FeatureFlags.Repository;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.Services;

/// <summary>
/// Service implementation for audit log operations.
/// Manages retrieval and cleanup of audit trails for compliance and debugging.
/// </summary>
public class AuditLogService : IAuditLogService {
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(IAuditLogRepository repository, ILogger<AuditLogService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsAsync(int featureFlagId)
    {
        if (featureFlagId <= 0)
            throw new ArgumentException("Feature flag ID must be > 0", nameof(featureFlagId));

        try
        {
            return await _repository.GetByFeatureFlagIdAsync(featureFlagId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for feature flag {Id}", featureFlagId);
            throw new FeatureFlagDataException("Failed to retrieve audit logs", ex);
        }
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsPagedAsync(int featureFlagId, int pageNumber, int pageSize)
    {
        if (featureFlagId <= 0)
            throw new ArgumentException("Feature flag ID must be > 0", nameof(featureFlagId));

        if (pageNumber < 1)
            throw new ArgumentException("Page number must be >= 1", nameof(pageNumber));

        if (pageSize < 1 || pageSize > FeatureFlagConstants.MaxPageSize)
            throw new ArgumentException($"Page size must be between 1 and {FeatureFlagConstants.MaxPageSize}", nameof(pageSize));

        try
        {
            return await _repository.GetByFeatureFlagIdPagedAsync(featureFlagId, pageNumber, pageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paged audit logs for feature flag {Id}", featureFlagId);
            throw new FeatureFlagDataException("Failed to retrieve audit logs", ex);
        }
    }

    public async Task<IEnumerable<AuditLog>> GetAuditLogsByUserAsync(string changedBy)
    {
        if (string.IsNullOrWhiteSpace(changedBy))
            throw new ArgumentException("ChangedBy cannot be empty", nameof(changedBy));

        try
        {
            return await _repository.GetByChangedByAsync(changedBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving audit logs for user {User}", changedBy);
            throw new FeatureFlagDataException("Failed to retrieve audit logs", ex);
        }
    }

    public async Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(int count)
    {
        if (count < 1)
            throw new ArgumentException("Count must be >= 1", nameof(count));

        try
        {
            var allLogs = await _repository.GetAllAsync();
            return allLogs.OrderByDescending(a => a.ChangedAt).Take(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent audit logs");
            throw new FeatureFlagDataException("Failed to retrieve recent audit logs", ex);
        }
    }

    public async Task<AuditLog?> GetLastChangeAsync(int featureFlagId, CancellationToken cancellationToken = default)
    {
        if (featureFlagId <= 0)
            throw new ArgumentException("Feature flag ID must be > 0", nameof(featureFlagId));

        try
        {
            return await _repository.GetLastChangeAsync(featureFlagId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving last change for feature flag {Id}", featureFlagId);
            throw new FeatureFlagDataException("Failed to retrieve last change", ex);
        }
    }

    public async Task<IEnumerable<AuditLog>> GetChangeHistoryAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date must be before or equal to end date");

        try
        {
            return await _repository.GetChangesInRangeAsync(startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving change history between {StartDate} and {EndDate}", startDate, endDate);
            throw new FeatureFlagDataException("Failed to retrieve change history", ex);
        }
    }

    public async Task CleanupOldLogsAsync(int retentionDays, CancellationToken cancellationToken = default)
    {
        if (retentionDays < 1)
            throw new ArgumentException("Retention days must be >= 1", nameof(retentionDays));

        try
        {
            await _repository.CleanupOldLogsAsync(retentionDays);
            _logger.LogInformation("Cleaned up audit logs older than {RetentionDays} days", retentionDays);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old audit logs");
            throw new FeatureFlagDataException("Failed to cleanup old logs", ex);
        }
    }

    Task<AuditLog?> IAuditLogService.GetLastChangeAsync(int featureFlagId) => GetLastChangeAsync(featureFlagId);
    Task IAuditLogService.CleanupOldLogsAsync(int retentionDays) => CleanupOldLogsAsync(retentionDays);
}
