#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Service interface for feature flag operations. Provides flag evaluation, CRUD management,
/// targeting rules, and variant selection for A/B testing and gradual rollouts.
/// </summary>
/// <remarks>
/// <para>
/// Flag evaluation via <see cref="IsEnabledAsync"/> considers multiple factors in order:
/// <list type="number">
///   <item>Global enabled/disabled state of the flag</item>
///   <item>User targeting rules (specific user IDs, email domains)</item>
///   <item>Percentage-based rollout configuration</item>
///   <item>Environment-specific overrides</item>
/// </list>
/// All mutations (create, update, delete, enable, disable) are recorded in the audit log.
/// </para>
/// </remarks>
public interface IFeatureFlagService
{
    /// <summary>
    /// Evaluates whether a feature flag is enabled for the given user context.
    /// This is the primary entry point for feature flag checks in application code.
    /// </summary>
    /// <param name="featureFlagKey">Unique key identifying the feature flag.</param>
    /// <param name="userContext">User context containing attributes used for targeting rules.</param>
    /// <returns><c>true</c> if the feature is enabled for this user.</returns>
    Task<bool> IsEnabledAsync(string featureFlagKey, UserContext userContext);

    /// <summary>Retrieves a feature flag by its numeric ID, or <c>null</c> if not found.</summary>
    Task<FeatureFlag?> GetFeatureFlagAsync(int id);

    /// <summary>Retrieves a feature flag by its unique string key, or <c>null</c> if not found.</summary>
    Task<FeatureFlag?> GetFeatureFlagByKeyAsync(string key);

    /// <summary>Returns all feature flags regardless of status.</summary>
    Task<IEnumerable<FeatureFlag>> GetAllFeatureFlagsAsync();

    /// <summary>Returns only feature flags that are currently enabled.</summary>
    Task<IEnumerable<FeatureFlag>> GetEnabledFeatureFlagsAsync();

    /// <summary>Creates a new feature flag and records the creation in the audit log.</summary>
    /// <param name="featureFlag">The feature flag definition to create.</param>
    /// <param name="createdBy">Identifier of the user performing the action.</param>
    Task<FeatureFlag> CreateFeatureFlagAsync(FeatureFlag featureFlag, string createdBy);

    /// <summary>Updates an existing feature flag and records the change in the audit log.</summary>
    Task UpdateFeatureFlagAsync(FeatureFlag featureFlag, string updatedBy);

    /// <summary>Deletes a feature flag by ID and records the deletion in the audit log.</summary>
    Task DeleteFeatureFlagAsync(int id, string deletedBy);

    /// <summary>Enables a disabled feature flag.</summary>
    Task EnableFeatureFlagAsync(int id, string modifiedBy);

    /// <summary>Disables an enabled feature flag.</summary>
    Task DisableFeatureFlagAsync(int id, string modifiedBy);

    /// <summary>
    /// Returns the variant assignment for a feature flag (used in A/B testing).
    /// Returns <c>null</c> if the flag has no variants or the user is not in the experiment.
    /// </summary>
    Task<string?> GetVariantAsync(string featureFlagKey, UserContext userContext);

    /// <summary>Searches feature flags by name or key containing <paramref name="searchTerm"/>.</summary>
    Task<IEnumerable<FeatureFlag>> SearchFeatureFlagsAsync(string searchTerm);
}
