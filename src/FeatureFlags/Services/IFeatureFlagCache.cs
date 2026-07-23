#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Interface for caching feature flags to avoid repeated database queries during evaluation.
/// Implements the decorator pattern to wrap IFeatureFlagService with caching behavior.
/// </summary>
public interface IFeatureFlagCache
{
    /// <summary>
    /// Gets a feature flag by its unique key from cache if available, otherwise from repository.
    /// </summary>
    /// <param name="key">The key of the feature flag.</param>
    /// <returns>The feature flag if found, otherwise null.</returns>
    Task<FeatureFlag?> GetFeatureFlagByKeyAsync(string key);

    /// <summary>
    /// Invalidates the cache entry for a specific feature flag.
    /// </summary>
    /// <param name="flagId">The ID of the feature flag to invalidate.</param>
    void Invalidate(int flagId);

    /// <summary>
    /// Invalidates the cache entry for a specific feature flag by key.
    /// </summary>
    /// <param name="flagKey">The key of the feature flag to invalidate.</param>
    void Invalidate(string flagKey);

    /// <summary>
    /// Clears all feature flag cache entries.
    /// </summary>
    void Clear();
}
