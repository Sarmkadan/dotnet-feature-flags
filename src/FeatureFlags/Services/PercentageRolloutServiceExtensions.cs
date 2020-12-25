#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;

namespace FeatureFlags.Services;

/// <summary>
/// Extension methods for <see cref="PercentageRolloutService"/> providing additional functionality
/// for percentage-based feature flag rollout evaluation.
/// </summary>
public static class PercentageRolloutServiceExtensions
{
    /// <summary>
    /// Evaluates multiple feature flags asynchronously for a given user.
    /// </summary>
    /// <param name="service">The percentage rollout service instance</param>
    /// <param name="featureFlags">Collection of feature flags to evaluate</param>
    /// <param name="userContext">User context containing user identifier and optional properties</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary mapping feature flag keys to their evaluation results</returns>
    public static async Task<Dictionary<string, bool>> EvaluateMultipleAsync(
        this PercentageRolloutService service,
        IEnumerable<FeatureFlag> featureFlags,
        UserContext userContext,
        CancellationToken cancellationToken = default)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (featureFlags is null)
            throw new ArgumentNullException(nameof(featureFlags));

        if (userContext is null)
            throw new ArgumentNullException(nameof(userContext));

        var results = new Dictionary<string, bool>(StringComparer.Ordinal);

        foreach (var featureFlag in featureFlags)
        {
            if (featureFlag.PercentageRollout is null)
                continue;

            var isEnabled = await service.EvaluateAsync(featureFlag, userContext, cancellationToken);
            results[featureFlag.Key] = isEnabled;
        }

        return results;
    }

    /// <summary>
    /// Determines if a feature flag should be enabled based on a specific bucket value.
    /// </summary>
    /// <param name="service">The percentage rollout service instance</param>
    /// <param name="featureFlagKey">Feature flag key</param>
    /// <param name="rolloutPercentage">Rollout percentage to evaluate</param>
    /// <param name="bucketValue">Specific bucket value to test against rollout percentage</param>
    /// <returns>True if the bucket value falls within the rollout percentage, false otherwise</returns>
    public static bool IsBucketInRollout(
        this PercentageRolloutService service,
        string featureFlagKey,
        int rolloutPercentage,
        int bucketValue)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrWhiteSpace(featureFlagKey))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(featureFlagKey));

        if (rolloutPercentage < 0 || rolloutPercentage > 100)
            throw new ArgumentException("Rollout percentage must be between 0 and 100", nameof(rolloutPercentage));

        if (bucketValue < 0 || bucketValue > 100)
            throw new ArgumentException("Bucket value must be between 0 and 100", nameof(bucketValue));

        if (rolloutPercentage == 100)
            return true;

        if (rolloutPercentage == 0)
            return false;

        return bucketValue < rolloutPercentage;
    }

    /// <summary>
    /// Gets the bucket value for a user without requiring a full UserContext object.
    /// </summary>
    /// <param name="service">The percentage rollout service instance</param>
    /// <param name="userId">User identifier</param>
    /// <param name="featureFlagKey">Feature flag key</param>
    /// <returns>Bucket value between 0 and 99</returns>
    public static int GetUserBucket(
        this PercentageRolloutService service,
        string userId,
        string featureFlagKey)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(featureFlagKey))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(featureFlagKey));

        var userContext = new UserContext { UserId = userId };
        return service.GetUserBucket(userContext, featureFlagKey);
    }

    /// <summary>
    /// Determines if a specific user should have a feature enabled based on user ID alone.
    /// </summary>
    /// <param name="service">The percentage rollout service instance</param>
    /// <param name="userId">User identifier</param>
    /// <param name="featureFlagKey">Feature flag key</param>
    /// <param name="rolloutPercentage">Rollout percentage to evaluate</param>
    /// <returns>True if the user should have the feature enabled, false otherwise</returns>
    public static bool IsUserInRollout(
        this PercentageRolloutService service,
        string userId,
        string featureFlagKey,
        int rolloutPercentage)
    {
        if (service is null)
            throw new ArgumentNullException(nameof(service));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (string.IsNullOrWhiteSpace(featureFlagKey))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(featureFlagKey));

        if (rolloutPercentage < 0 || rolloutPercentage > 100)
            throw new ArgumentException("Rollout percentage must be between 0 and 100", nameof(rolloutPercentage));

        if (rolloutPercentage == 100)
            return true;

        if (rolloutPercentage == 0)
            return false;

        var bucket = service.GetUserBucket(userId, featureFlagKey);
        return bucket < rolloutPercentage;
    }
}