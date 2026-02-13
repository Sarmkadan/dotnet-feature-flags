// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Service interface for percentage-based feature flag rollout evaluation.
/// Uses consistent hashing to ensure stable rollout decisions per user.
/// </summary>
public interface IPercentageRolloutService
{
    Task<bool> EvaluateAsync(FeatureFlag featureFlag, UserContext userContext);

    bool IsUserInRollout(UserContext userContext, string featureFlagKey, int rolloutPercentage);

    int GetUserBucket(UserContext userContext, string featureFlagKey);
}
