// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Data;
using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Repository;

/// <summary>
/// Implementation of feature flag repository providing database persistence operations.
/// Handles complex queries including eager loading of related entities.
/// </summary>
public class FeatureFlagRepository : IFeatureFlagRepository
{
    private readonly FeatureFlagDbContext _context;

    public FeatureFlagRepository(FeatureFlagDbContext context)
    {
        _context = context;
    }

    public async Task<FeatureFlag?> GetByIdAsync(int id)
    {
        return await _context.FeatureFlags.FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<FeatureFlag?> GetByKeyAsync(string key)
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

    public async Task<int> GetTotalCountAsync()
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

    public async Task<FeatureFlag?> GetWithRulesAsync(int featureFlagId)
    {
        return await _context.FeatureFlags
            .Include(f => f.Rules)
            .ThenInclude(r => r.Conditions)
            .FirstOrDefaultAsync(f => f.Id == featureFlagId);
    }

    public async Task<FeatureFlag?> GetWithVariantsAsync(int featureFlagId)
    {
        return await _context.FeatureFlags
            .Include(f => f.Variants)
            .FirstOrDefaultAsync(f => f.Id == featureFlagId);
    }

    public async Task<FeatureFlag?> GetWithAuditLogsAsync(int featureFlagId)
    {
        return await _context.FeatureFlags
            .Include(f => f.AuditLogs)
            .FirstOrDefaultAsync(f => f.Id == featureFlagId);
    }

    public async Task<bool> KeyExistsAsync(string key)
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

    public async Task<FeatureFlag> AddAsync(FeatureFlag entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (!entity.IsValid())
            throw new InvalidFeatureFlagException("Feature flag configuration is invalid");

        var result = _context.FeatureFlags.Add(entity);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    public async Task UpdateAsync(FeatureFlag entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (!entity.IsValid())
            throw new InvalidFeatureFlagException("Feature flag configuration is invalid");

        var existing = await GetByIdAsync(entity.Id);
        if (existing == null)
            throw new FeatureFlagNotFoundException(entity.Key);

        _context.FeatureFlags.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            throw new FeatureFlagNotFoundException(id.ToString());

        _context.FeatureFlags.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.FeatureFlags.AnyAsync(f => f.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
