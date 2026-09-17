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
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a feature flag by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the feature flag.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching feature flag, or <c>null</c> if not found.</returns>
    public async Task<FeatureFlag?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags.FirstOrDefaultAsync(f => f.Id == id);
    }

    /// <summary>
    /// Retrieves a feature flag by its unique key.
    /// </summary>
    /// <param name="key">The unique key of the feature flag.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matching feature flag, or <c>null</c> if not found.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is null or whitespace.</exception>
    public async Task<FeatureFlag?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        return await _context.FeatureFlags.FirstOrDefaultAsync(f => f.Key == key);
    }

    /// <summary>
    /// Retrieves all feature flags.
    /// </summary>
    /// <returns>A collection of all feature flags.</returns>
    public async Task<IEnumerable<FeatureFlag>> GetAllAsync()
    {
        return await _context.FeatureFlags.ToListAsync();
    }

    /// <summary>
    /// Retrieves all enabled feature flags.
    /// </summary>
    /// <returns>A collection of enabled feature flags.</returns>
    public async Task<IEnumerable<FeatureFlag>> GetEnabledAsync()
    {
        return await _context.FeatureFlags.Where(f => f.IsEnabled).ToListAsync();
    }

    /// <summary>
    /// Retrieves feature flags created by a specific user.
    /// </summary>
    /// <param name="createdBy">The creator identifier to filter by.</param>
    /// <returns>A collection of feature flags created by the specified user, ordered by creation date descending.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="createdBy"/> is null or whitespace.</exception>
    public async Task<IEnumerable<FeatureFlag>> GetByCreatorAsync(string createdBy)
    {
        if (string.IsNullOrWhiteSpace(createdBy))
            throw new ArgumentException("CreatedBy cannot be empty", nameof(createdBy));

        return await _context.FeatureFlags
            .Where(f => f.CreatedBy == createdBy)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves feature flags modified since the specified date and time.
    /// </summary>
    /// <param name="dateTime">The cutoff date and time.</param>
    /// <returns>A collection of feature flags updated at or after the cutoff, ordered by update date descending.</returns>
    public async Task<IEnumerable<FeatureFlag>> GetModifiedSinceAsync(DateTime dateTime)
    {
        return await _context.FeatureFlags
            .Where(f => f.UpdatedAt >= dateTime)
            .OrderByDescending(f => f.UpdatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves the total number of feature flags.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The total count of feature flags.</returns>
    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags.CountAsync();
    }

    /// <summary>
    /// Retrieves a page of feature flags ordered by creation date descending.
    /// </summary>
    /// <param name="pageNumber">The one-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A collection of feature flags for the requested page.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="pageNumber"/> or <paramref name="pageSize"/> is less than 1.</exception>
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

    /// <summary>
    /// Searches feature flags by key, display name, or description.
    /// </summary>
    /// <param name="searchTerm">The search term. If null or whitespace, all feature flags are returned.</param>
    /// <returns>A collection of feature flags matching the search term.</returns>
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

    /// <summary>
    /// Retrieves a feature flag including its rules and their conditions.
    /// </summary>
    /// <param name="featureFlagId">The unique identifier of the feature flag.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The feature flag with rules and conditions eagerly loaded, or <c>null</c> if not found.</returns>
    public async Task<FeatureFlag?> GetWithRulesAsync(int featureFlagId, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.Rules)
            .ThenInclude(r => r.Conditions)
            .FirstOrDefaultAsync(f => f.Id == featureFlagId);
    }

    /// <summary>
    /// Retrieves a feature flag including its variants.
    /// </summary>
    /// <param name="featureFlagId">The unique identifier of the feature flag.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The feature flag with variants eagerly loaded, or <c>null</c> if not found.</returns>
    public async Task<FeatureFlag?> GetWithVariantsAsync(int featureFlagId, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.Variants)
            .FirstOrDefaultAsync(f => f.Id == featureFlagId);
    }

    /// <summary>
    /// Retrieves a feature flag including its audit logs.
    /// </summary>
    /// <param name="featureFlagId">The unique identifier of the feature flag.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The feature flag with audit logs eagerly loaded, or <c>null</c> if not found.</returns>
    public async Task<FeatureFlag?> GetWithAuditLogsAsync(int featureFlagId, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags
            .Include(f => f.AuditLogs)
            .FirstOrDefaultAsync(f => f.Id == featureFlagId);
    }

    /// <summary>
    /// Determines whether a feature flag with the specified key exists.
    /// </summary>
    /// <param name="key">The key to check for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if a feature flag with the key exists; otherwise, <c>false</c>.</returns>
    public async Task<bool> KeyExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        return await _context.FeatureFlags.AnyAsync(f => f.Key == key);
    }

    /// <summary>
    /// Retrieves the most recently modified feature flags.
    /// </summary>
    /// <param name="count">The maximum number of feature flags to return.</param>
    /// <returns>A collection of the most recently modified feature flags.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="count"/> is less than 1.</exception>
    public async Task<IEnumerable<FeatureFlag>> GetRecentlyModifiedAsync(int count)
    {
        if (count < 1)
            throw new ArgumentException("Count must be >= 1", nameof(count));

        return await _context.FeatureFlags
            .OrderByDescending(f => f.UpdatedAt)
            .Take(count)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves feature flags that have not been updated within the specified time span.
    /// </summary>
    /// <param name="olderThan">The minimum age of a feature flag to be considered stale.</param>
    /// <returns>A collection of stale feature flags ordered by update date ascending.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="olderThan"/> is negative.</exception>
    public async Task<IEnumerable<FeatureFlag>> GetStaleFlagsAsync(TimeSpan olderThan)
    {
        if (olderThan < TimeSpan.Zero)
            throw new ArgumentException("Time span must be non-negative", nameof(olderThan));

        var cutoffDate = DateTime.UtcNow - olderThan;
        return await _context.FeatureFlags
            .Where(f => f.UpdatedAt < cutoffDate)
            .OrderBy(f => f.UpdatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new feature flag to the repository.
    /// </summary>
    /// <param name="entity">The feature flag to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The added feature flag.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="InvalidFeatureFlagException">Thrown when the feature flag configuration is invalid.</exception>
    /// <exception cref="FeatureFlagDataException">Thrown when a database error occurs.</exception>
    public async Task<FeatureFlag> AddAsync(FeatureFlag entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

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

    /// <summary>
    /// Updates an existing feature flag in the repository.
    /// </summary>
    /// <param name="entity">The feature flag to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entity"/> is null.</exception>
    /// <exception cref="InvalidFeatureFlagException">Thrown when the feature flag configuration is invalid.</exception>
    /// <exception cref="FeatureFlagNotFoundException">Thrown when the feature flag does not exist.</exception>
    /// <exception cref="FeatureFlagDataException">Thrown when a database error occurs.</exception>
    public async Task UpdateAsync(FeatureFlag entity, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);

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

    /// <summary>
    /// Deletes a feature flag by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the feature flag to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FeatureFlagNotFoundException">Thrown when the feature flag does not exist.</exception>
    /// <exception cref="FeatureFlagDataException">Thrown when a database error occurs.</exception>
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

    /// <summary>
    /// Determines whether a feature flag with the specified identifier exists.
    /// </summary>
    /// <param name="id">The unique identifier to check for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if a feature flag with the identifier exists; otherwise, <c>false</c>.</returns>
    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.FeatureFlags.AnyAsync(f => f.Id == id);
    }

    /// <summary>
    /// Persists all pending changes to the underlying database.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
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
