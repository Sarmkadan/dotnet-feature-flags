#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Data;
using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.Repository;

/// <summary>
/// Implementation of feature flag repository providing database persistence operations.
/// Handles complex queries including eager loading of related entities.
/// </summary>
public class FeatureFlagRepository : IFeatureFlagRepository {
    private readonly FeatureFlagDbContext _context;
    private readonly ILogger<FeatureFlagRepository> _logger;

    public FeatureFlagRepository(FeatureFlagDbContext context, ILogger<FeatureFlagRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<FeatureFlag?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags.FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<FeatureFlag?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        return await _context.FeatureFlags.FirstOrDefaultAsync(f => f.Key == key);
    }

    public async Task<IEnumerable<FeatureFlag>> GetAllAsync()
    {
        return await _context.FeatureFlags.ToListAsync();
    }

    public async Task<IEnumerable<FeatureFlag>> GetEnabledAsync()
    {
        return await _context.FeatureFlags.Where(f => f.IsEnabled).ToListAsync();
    }

    public async Task<IEnumerable<FeatureFlag>> GetByCreatorAsync(string createdBy)
    {
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be empty", nameof(createdBy));

        return await _context.FeatureFlags
            .Where(f => f.CreatedBy == createdBy)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<FeatureFlag>> GetModifiedSinceAsync(DateTime dateTime)
    {
        return await _context.FeatureFlags
            .Where(f => f.UpdatedAt >= dateTime)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags.CountAsync();
    }

    public async Task<IEnumerable<FeatureFlag>> GetPagedAsync(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("Page number must be >= 1", nameof(pageNumber));
        if (pageSize < 1)
            throw new ArgumentException("Page size must be >= 1", nameof(pageSize));

        return await _context.FeatureFlags
            .OrderByDescending(f => f.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<FeatureFlag>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync();

        var term = searchTerm.ToLower();
        return await _context.FeatureFlags
            .Where(f => f.Key.ToLower().Contains(term) ||
                       f.DisplayName.ToLower().Contains(term) ||
                       f.Description.ToLower().Contains(term))
            .ToListAsync();
    }

    public async Task<FeatureFlag?> GetWithRulesAsync(int featureFlagId, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.Rules)
            .ThenInclude(r => r.Conditions)
            .FirstOrDefaultAsync(f => f.Id == featureFlagId);
    }

    public async Task<FeatureFlag?> GetWithVariantsAsync(int featureFlagId, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.Variants)
            .FirstOrDefaultAsync(f => f.Id == featureFlagId);
    }

    public async Task<FeatureFlag?> GetWithAuditLogsAsync(int featureFlagId, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.AuditLogs)
            .FirstOrDefaultAsync(f => f.Id == featureFlagId);
    }

    public async Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return await _context.FeatureFlags.AnyAsync(f => f.Key == key);
    }

    public async Task<IEnumerable<FeatureFlag>> GetRecentlyModifiedAsync(int count)
    {
        if (count < 1)
            throw new ArgumentException("Count must be >= 1", nameof(count));

        return await _context.FeatureFlags
            .OrderByDescending(f => f.UpdatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<FeatureFlag> AddAsync(FeatureFlag entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        if (!entity.IsValid())
            throw new InvalidFeatureFlagException("Feature flag configuration is invalid");

        try
        {
            var result = _context.FeatureFlags.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return result.Entity;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while adding feature flag {Key}", entity.Key);
            throw new FeatureFlagDataException("Failed to add feature flag due to database error", ex);
        }
        catch (Exception ex) when (ex is not FeatureFlagException)
        {
            _logger.LogError(ex, "Unexpected error while adding feature flag {Key}", entity.Key);
            throw new FeatureFlagDataException("Failed to add feature flag", ex);
        }
    }

    public async Task UpdateAsync(FeatureFlag entity, CancellationToken cancellationToken = default)
    {
        if (entity is null)
            throw new ArgumentNullException(nameof(entity));

        if (!entity.IsValid())
            throw new InvalidFeatureFlagException("Feature flag configuration is invalid");

        try
        {
            var existing = await GetByIdAsync(entity.Id, cancellationToken);
            if (existing is null)
                throw new FeatureFlagNotFoundException(entity.Key);

            _context.FeatureFlags.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while updating feature flag {Key}", entity.Key);
            throw new FeatureFlagDataException("Failed to update feature flag due to database error", ex);
        }
        catch (Exception ex) when (ex is not FeatureFlagException)
        {
            _logger.LogError(ex, "Unexpected error while updating feature flag {Key}", entity.Key);
            throw new FeatureFlagDataException("Failed to update feature flag", ex);
        }
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await GetByIdAsync(id, cancellationToken);
            if (entity is null)
                throw new FeatureFlagNotFoundException(id.ToString());

            _context.FeatureFlags.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while deleting feature flag with id {Id}", id);
            throw new FeatureFlagDataException("Failed to delete feature flag due to database error", ex);
        }
        catch (Exception ex) when (ex is not FeatureFlagException)
        {
            _logger.LogError(ex, "Unexpected error while deleting feature flag with id {Id}", id);
            throw new FeatureFlagDataException("Failed to delete feature flag", ex);
        }
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags.AnyAsync(f => f.Id == id);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync();
    }

    Task<FeatureFlag?> IRepository<FeatureFlag>.GetByIdAsync(int id) => GetByIdAsync(id);
    Task<FeatureFlag?> IFeatureFlagRepository.GetByKeyAsync(string key) => GetByKeyAsync(key);
    Task<int> IFeatureFlagRepository.GetTotalCountAsync() => GetTotalCountAsync();
    Task<FeatureFlag?> IFeatureFlagRepository.GetWithRulesAsync(int featureFlagId) => GetWithRulesAsync(featureFlagId);
    Task<FeatureFlag?> IFeatureFlagRepository.GetWithVariantsAsync(int featureFlagId) => GetWithVariantsAsync(featureFlagId);
    Task<FeatureFlag?> IFeatureFlagRepository.GetWithAuditLogsAsync(int featureFlagId) => GetWithAuditLogsAsync(featureFlagId);
    Task<bool> IFeatureFlagRepository.KeyExistsAsync(string key) => KeyExistsAsync(key);
    Task<FeatureFlag> IRepository<FeatureFlag>.AddAsync(FeatureFlag entity) => AddAsync(entity);
    Task IRepository<FeatureFlag>.UpdateAsync(FeatureFlag entity) => UpdateAsync(entity);
    Task IRepository<FeatureFlag>.DeleteAsync(int id) => DeleteAsync(id);
    Task<bool> IRepository<FeatureFlag>.ExistsAsync(int id) => ExistsAsync(id);
    Task IRepository<FeatureFlag>.SaveChangesAsync() => SaveChangesAsync();
}
