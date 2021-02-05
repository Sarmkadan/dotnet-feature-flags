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
    /// <param name="service">The percentage rollout service instance.</param>
    /// <param name="featureFlags">Collection of feature flags to evaluate.</param>
    /// <param name="userContext">User context containing user identifier and optional properties.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Dictionary mapping feature flag keys to their evaluation results.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="featureFlags"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="userContext"/> is <see langword="null"/>.</exception>
    public static async Task<Dictionary<string, bool>> EvaluateMultipleAsync(
        this PercentageRolloutService service,
        IEnumerable<FeatureFlag> featureFlags,
        UserContext userContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(featureFlags);
        ArgumentNullException.ThrowIfNull(userContext);

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
    /// <param name="service">The percentage rollout service instance.</param>
    /// <param name="featureFlagKey">Feature flag key.</param>
    /// <param name="rolloutPercentage">Rollout percentage to evaluate.</param>
    /// <param name="bucketValue">Specific bucket value to test against rollout percentage.</param>
    /// <returns>True if the bucket value falls within the rollout percentage, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="featureFlagKey"/> is <see langword="null"/>, empty, or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rolloutPercentage"/> is not between 0 and 100.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bucketValue"/> is not between 0 and 100.</exception>
    public static bool IsBucketInRollout(
        this PercentageRolloutService service,
        string featureFlagKey,
        int rolloutPercentage,
        int bucketValue)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(featureFlagKey);

        if (rolloutPercentage is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(rolloutPercentage), "Rollout percentage must be between 0 and 100");

        if (bucketValue is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(bucketValue), "Bucket value must be between 0 and 100");

        return rolloutPercentage switch
        {
            100 => true,
            0 => false,
            _ => bucketValue < rolloutPercentage
        };
    }

    /// <summary>
    /// Gets the bucket value for a user without requiring a full UserContext object.
    /// </summary>
    /// <param name="service">The percentage rollout service instance.</param>
    /// <param name="userId">User identifier.</param>
    /// <param name="featureFlagKey">Feature flag key.</param>
    /// <returns>Bucket value between 0 and 99.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="userId"/> is <see langword="null"/>, empty, or whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="featureFlagKey"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public static int GetUserBucket(
        this PercentageRolloutService service,
        string userId,
        string featureFlagKey)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(featureFlagKey);

        var userContext = new UserContext { UserId = userId };
        return service.GetUserBucket(userContext, featureFlagKey);
    }

    /// <summary>
    /// Determines if a specific user should have a feature enabled based on user ID alone.
    /// </summary>
    /// <param name="service">The percentage rollout service instance.</param>
    /// <param name="userId">User identifier.</param>
    /// <param name="featureFlagKey">Feature flag key.</param>
    /// <param name="rolloutPercentage">Rollout percentage to evaluate.</param>
    /// <returns>True if the user should have the feature enabled, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="service"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="userId"/> is <see langword="null"/>, empty, or whitespace.</exception>
    /// <exception cref="ArgumentException"><paramref name="featureFlagKey"/> is <see langword="null"/>, empty, or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rolloutPercentage"/> is not between 0 and 100.</exception>
    public static bool IsUserInRollout(
        this PercentageRolloutService service,
        string userId,
        string featureFlagKey,
        int rolloutPercentage)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(featureFlagKey);

        if (rolloutPercentage is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(rolloutPercentage), "Rollout percentage must be between 0 and 100");

        return rolloutPercentage switch
        {
            100 => true,
            0 => false,
            _ => service.GetUserBucket(userId, featureFlagKey) < rolloutPercentage
        };
    }
}