// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;

namespace FeatureFlags.Repository;

/// <summary>
/// Repository interface for feature flag data access operations.
/// Extends generic repository with feature flag specific queries.
/// </summary>
public interface IFeatureFlagRepository : IRepository<FeatureFlag>
{
    Task<FeatureFlag?> GetByKeyAsync(string key);

    Task<IEnumerable<FeatureFlag>> GetEnabledAsync();

    Task<IEnumerable<FeatureFlag>> GetByCreatorAsync(string createdBy);

    Task<IEnumerable<FeatureFlag>> GetModifiedSinceAsync(DateTime dateTime);

    Task<int> GetTotalCountAsync();

    Task<IEnumerable<FeatureFlag>> GetPagedAsync(int pageNumber, int pageSize);

    Task<IEnumerable<FeatureFlag>> SearchAsync(string searchTerm);

    Task<FeatureFlag?> GetWithRulesAsync(int featureFlagId);

    Task<FeatureFlag?> GetWithVariantsAsync(int featureFlagId);

    Task<FeatureFlag?> GetWithAuditLogsAsync(int featureFlagId);

    Task<bool> KeyExistsAsync(string key);

    Task<IEnumerable<FeatureFlag>> GetRecentlyModifiedAsync(int count);
}
