#nullable enable
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
        /// <summary>Gets a feature flag by its unique key.</summary>
        /// <param name="key">The key of the feature flag.</param>
        /// <returns>The feature flag if found, otherwise null.</returns>
        Task<FeatureFlag?> GetByKeyAsync(string key);

        /// <summary>Gets all enabled feature flags.</summary>
        /// <returns>An enumerable collection of enabled feature flags.</returns>
        Task<IEnumerable<FeatureFlag>> GetEnabledAsync();

        /// <summary>Gets feature flags created by a specific user.</summary>
        /// <param name="createdBy">The creator's username or email.</param>
        /// <returns>An enumerable collection of feature flags.</returns>
        Task<IEnumerable<FeatureFlag>> GetByCreatorAsync(string createdBy);

        /// <summary>Gets feature flags modified since a specific date and time.</summary>
        /// <param name="dateTime">The cutoff date and time.</param>
        /// <returns>An enumerable collection of feature flags.</returns>
        Task<IEnumerable<FeatureFlag>> GetModifiedSinceAsync(DateTime dateTime);

        /// <summary>Gets the total count of feature flags.</summary>
        /// <returns>The total number of feature flags.</returns>
        Task<int> GetTotalCountAsync();

        /// <summary>Gets a paginated collection of feature flags.</summary>
        /// <param name="pageNumber">The page number (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>An enumerable collection of feature flags for the specified page.</returns>
        Task<IEnumerable<FeatureFlag>> GetPagedAsync(int pageNumber, int pageSize);

        /// <summary>Searches for feature flags based on a search term.</summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <returns>An enumerable collection of matching feature flags.</returns>
        Task<IEnumerable<FeatureFlag>> SearchAsync(string searchTerm);

        /// <summary>Gets a feature flag by ID, including its rules.</summary>
        /// <param name="featureFlagId">The ID of the feature flag.</param>
        /// <returns>The feature flag if found, otherwise null.</returns>
        Task<FeatureFlag?> GetWithRulesAsync(int featureFlagId);

        /// <summary>Gets a feature flag by ID, including its A/B test variants.</summary>
        /// <param name="featureFlagId">The ID of the feature flag.</param>
        /// <returns>The feature flag if found, otherwise null.</returns>
        Task<FeatureFlag?> GetWithVariantsAsync(int featureFlagId);

        /// <summary>Gets a feature flag by ID, including its audit logs.</summary>
        /// <param name="featureFlagId">The ID of the feature flag.</param>
        /// <returns>The feature flag if found, otherwise null.</returns>
        Task<FeatureFlag?> GetWithAuditLogsAsync(int featureFlagId);

        /// <summary>Checks if a feature flag with the given key exists.</summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key exists, false otherwise.</returns>
        Task<bool> KeyExistsAsync(string key);

        /// <summary>Gets the recently modified feature flags.</summary>
        /// <param name="count">The number of items to return.</param>
        /// <returns>An enumerable collection of recently modified feature flags.</returns>
        Task<IEnumerable<FeatureFlag>> GetRecentlyModifiedAsync(int count);
    }

