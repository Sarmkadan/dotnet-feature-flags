#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Service interface for feature flag operations.
/// Orchestrates flag evaluation, management, and rule application.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Evaluates if a feature flag is enabled for a given user context.
    /// </summary>
    /// <param name="featureFlagKey">The key of the feature flag.</param>
    /// <param name="userContext">The context of the user for evaluation.</param>
    /// <returns>True if enabled, false otherwise.</returns>
    Task<bool> IsEnabledAsync(string featureFlagKey, UserContext userContext);

    /// <summary>
    /// Gets a feature flag by its ID.
    /// </summary>
    /// <param name="id">The ID of the feature flag.</param>
    /// <returns>The feature flag if found, otherwise null.</returns>
    Task<FeatureFlag?> GetFeatureFlagAsync(int id);

    /// <summary>
    /// Gets a feature flag by its unique key.
    /// </summary>
    /// <param name="key">The key of the feature flag.</param>
    /// <returns>The feature flag if found, otherwise null.</returns>
    Task<FeatureFlag?> GetFeatureFlagByKeyAsync(string key);

    /// <summary>
    /// Gets all feature flags.
    /// </summary>
    /// <returns>An enumerable collection of all feature flags.</returns>
    Task<IEnumerable<FeatureFlag>> GetAllFeatureFlagsAsync();

    /// <summary>
    /// Gets all enabled feature flags.
    /// </summary>
    /// <returns>An enumerable collection of enabled feature flags.</returns>
    Task<IEnumerable<FeatureFlag>> GetEnabledFeatureFlagsAsync();

    /// <summary>
    /// Creates a new feature flag.
    /// </summary>
    /// <param name="featureFlag">The feature flag to create.</param>
    /// <param name="createdBy">The user who created the flag.</param>
    /// <returns>The created feature flag.</returns>
    Task<FeatureFlag> CreateFeatureFlagAsync(FeatureFlag featureFlag, string createdBy);

    /// <summary>
    /// Updates an existing feature flag.
    /// </summary>
    /// <param name="featureFlag">The feature flag to update.</param>
    /// <param name="updatedBy">The user who updated the flag.</param>
    Task UpdateFeatureFlagAsync(FeatureFlag featureFlag, string updatedBy);

    /// <summary>
    /// Deletes a feature flag by its ID.
    /// </summary>
    /// <param name="id">The ID of the feature flag to delete.</param>
    /// <param name="deletedBy">The user who deleted the flag.</param>
    Task DeleteFeatureFlagAsync(int id, string deletedBy);

    /// <summary>
    /// Enables a feature flag by its ID.
    /// </summary>
    /// <param name="id">The ID of the feature flag to enable.</param>
    /// <param name="modifiedBy">The user who enabled the flag.</param>
    Task EnableFeatureFlagAsync(int id, string modifiedBy);

    /// <summary>
    /// Disables a feature flag by its ID.
    /// </summary>
    /// <param name="id">The ID of the feature flag to disable.</param>
    /// <param name="modifiedBy">The user who disabled the flag.</param>
    Task DisableFeatureFlagAsync(int id, string modifiedBy);

    /// <summary>
    /// Gets the A/B test variant for a given user context.
    /// </summary>
    /// <param name="featureFlagKey">The key of the feature flag.</param>
    /// <param name="userContext">The context of the user.</param>
    /// <returns>The name of the variant, or null if no variant is applicable.</returns>
    Task<string?> GetVariantAsync(string featureFlagKey, UserContext userContext);

    /// <summary>
    /// Searches for feature flags based on a search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <returns>An enumerable collection of matching feature flags.</returns>
    Task<IEnumerable<FeatureFlag>> SearchFeatureFlagsAsync(string searchTerm);
}

