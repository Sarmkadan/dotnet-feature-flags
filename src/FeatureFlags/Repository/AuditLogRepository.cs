#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Constants;
using FeatureFlags.Data;
using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Repository;

/// <summary>
/// Implementation of audit log repository providing persistence and retrieval of audit records.
/// Supports comprehensive querying for audit trails and compliance reporting.
/// </summary>
public sealed class AuditLogRepository {
    private readonly FeatureFlagDbContext _context;

    public AuditLogRepository(FeatureFlagDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLog?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        return await _context.AuditLogs.OrderByDescending(a => a.ChangedAt).ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByFeatureFlagIdAsync(int featureFlagId)
    {
        if (featureFlagId <= 0)
            throw new ArgumentException("FeatureFlagId must be > 0", nameof(featureFlagId));

        return await _context.AuditLogs
            .Where(a => a.FeatureFlagId == featureFlagId)
            .OrderByDescending(a => a.ChangedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByChangedByAsync(string changedBy)
    {
        if (string.IsNullOrWhiteSpace(changedBy))
            throw new ArgumentException("ChangedBy cannot be empty", nameof(changedBy));

        return await _context.AuditLogs
            .Where(a => a.ChangedBy == changedBy)
            .OrderByDescending(a => a.ChangedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetSinceAsync(DateTime dateTime)
    {
        return await _context.AuditLogs
            .Where(a => a.ChangedAt >= dateTime)
            .OrderByDescending(a => a.ChangedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetPagedAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be >= 1", nameof(pageNumber));
        if (pageSize < 1 || pageSize > FeatureFlagConstants.MaxPageSize)
            throw new ArgumentException($"Page size must be between 1 and {FeatureFlagConstants.MaxPageSize}", nameof(pageSize));

        return await _context.AuditLogs
            .OrderByDescending(a => a.ChangedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByFeatureFlagIdPagedAsync(int featureFlagId, int pageNumber, int pageSize)
    {
        if (featureFlagId <= 0)
            throw new ArgumentException("FeatureFlagId must be > 0", nameof(featureFlagId));
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be >= 1", nameof(pageNumber));
        if (pageSize < 1 || pageSize > FeatureFlagConstants.MaxPageSize)
            throw new ArgumentException($"Page size must be between 1 and {FeatureFlagConstants.MaxPageSize}", nameof(pageSize));

        return await _context.AuditLogs
            .Where(a => a.FeatureFlagId == featureFlagId)
            .OrderByDescending(a => a.ChangedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountByFeatureFlagIdAsync(int featureFlagId, CancellationToken cancellationToken = default)
    {
        if (featureFlagId <= 0)
            throw new ArgumentException("FeatureFlagId must be > 0", nameof(featureFlagId));

        return await _context.AuditLogs.CountAsync(a => a.FeatureFlagId == featureFlagId);
    }

    public async Task<AuditLog?> GetLastChangeAsync(int featureFlagId, CancellationToken cancellationToken = default)
    {
        if (featureFlagId <= 0)
            throw new ArgumentException("FeatureFlagId must be > 0", nameof(featureFlagId));

        return await _context.AuditLogs
            .Where(a => a.FeatureFlagId == featureFlagId)
            .OrderByDescending(a => a.ChangedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetChangesInRangeAsync(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date must be before or equal to end date");

        return await _context.AuditLogs
            .Where(a => a.ChangedAt >= startDate && a.ChangedAt <= endDate)
            .OrderByDescending(a => a.ChangedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetByActionAsync(string action)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty", nameof(action));

        return await _context.AuditLogs
            .Where(a => a.Action.ToString() == action)
            .OrderByDescending(a => a.ChangedAt)
            .ToListAsync();
    }

    public async Task<AuditLog> AddAsync(AuditLog entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        if (!entity.IsValid())
            throw new FeatureFlagDataException("Audit log is invalid");

        var result = _context.AuditLogs.Add(entity);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task UpdateAsync(AuditLog entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        var existing = await GetByIdAsync(entity.Id);
        if (existing is null)
            throw new FeatureFlagDataException($"Audit log with id {entity.Id} not found");

        _context.AuditLogs.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id);
        if (entity is null)
            throw new FeatureFlagDataException($"Audit log with id {id} not found");

        _context.AuditLogs.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditLogs.AnyAsync(a => a.Id == id);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync();
    }

    public async Task CleanupOldLogsAsync(int retentionDays, CancellationToken cancellationToken = default)
    {
        if (retentionDays < 1)
            throw new ArgumentException("Retention days must be >= 1", nameof(retentionDays));

        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
        var logsToDelete = await _context.AuditLogs
            .Where(a => a.ChangedAt < cutoffDate)
            .ToListAsync();

        if (logsToDelete.Any())
        {
            _context.AuditLogs.RemoveRange(logsToDelete);
            await _context.SaveChangesAsync();
        }
    }
}
