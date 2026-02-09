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
    Task<bool> IsEnabledAsync(string featureFlagKey, UserContext userContext);

    Task<FeatureFlag?> GetFeatureFlagAsync(int id);

    Task<FeatureFlag?> GetFeatureFlagByKeyAsync(string key);

    Task<IEnumerable<FeatureFlag>> GetAllFeatureFlagsAsync();

    Task<IEnumerable<FeatureFlag>> GetEnabledFeatureFlagsAsync();

    Task<FeatureFlag> CreateFeatureFlagAsync(FeatureFlag featureFlag, string createdBy);

    Task UpdateFeatureFlagAsync(FeatureFlag featureFlag, string updatedBy);

    Task DeleteFeatureFlagAsync(int id, string deletedBy);

    Task EnableFeatureFlagAsync(int id, string modifiedBy);

    Task DisableFeatureFlagAsync(int id, string modifiedBy);

    Task<string?> GetVariantAsync(string featureFlagKey, UserContext userContext);

    Task<IEnumerable<FeatureFlag>> SearchFeatureFlagsAsync(string searchTerm);
}
