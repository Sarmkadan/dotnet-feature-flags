#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Constants;
using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.Services;

/// <summary>
/// Service implementation for percentage-based rollout evaluation.
/// Uses consistent hashing to provide stable, reproducible rollout decisions.
/// </summary>
public class PercentageRolloutService : IPercentageRolloutService {
    private readonly ILogger<PercentageRolloutService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PercentageRolloutService"/> class.
    /// </summary>
    /// <param name="logger">The logger used to record diagnostic information.</param>
    public PercentageRolloutService(ILogger<PercentageRolloutService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Evaluates whether the given feature flag is enabled for the specified user context.
    /// </summary>
    /// <param name="featureFlag">The feature flag to evaluate.</param>
    /// <param name="userContext">The user context to evaluate against.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the operation to complete.</param>
    /// <returns>A task that resolves to <c>true</c> if the feature flag is enabled for the user; otherwise, <c>false</c>.</returns>
    public Task<bool> EvaluateAsync(FeatureFlag featureFlag, UserContext userContext, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Evaluate(featureFlag, userContext));
    }

    private bool Evaluate(FeatureFlag featureFlag, UserContext userContext)
    {
        if (featureFlag is null)
            throw new ArgumentNullException(nameof(featureFlag));

        if (userContext is null)
            throw new ArgumentNullException(nameof(userContext));

        if (featureFlag.PercentageRollout is null)
            throw new InvalidFeatureFlagException("Feature flag does not have a percentage rollout configured");

        _logger.LogDebug("Starting EvaluateAsync for feature flag {FeatureFlagKey}", featureFlag.Key);
        try
        {
            var isEnabled = IsUserInRollout(userContext, featureFlag.Key, featureFlag.PercentageRollout.Value);

            _logger.LogDebug("Feature flag '{Key}' percentage evaluation for user {UserId}: {Result}",
                featureFlag.Key, userContext.UserId, isEnabled);

            return isEnabled;
        }
        catch (Exception ex) when (ex is not FeatureFlagException)
        {
            _logger.LogError(ex, "Error during percentage rollout evaluation for feature flag '{Key}'", featureFlag.Key);
            throw new FeatureFlagDataException("Failed to evaluate percentage rollout", ex);
        }
    }

    /// <summary>
    /// Determines whether the specified user falls within the rollout percentage for the given feature flag.
    /// </summary>
    /// <param name="userContext">The user context to evaluate.</param>
    /// <param name="featureFlagKey">The key of the feature flag being evaluated.</param>
    /// <param name="rolloutPercentage">The percentage of users to include in the rollout (0 to 100).</param>
    /// <returns><c>true</c> if the user is within the rollout; otherwise, <c>false</c>.</returns>
    public bool IsUserInRollout(UserContext userContext, string featureFlagKey, int rolloutPercentage)
    {
        if (userContext is null)
            throw new ArgumentNullException(nameof(userContext));

        if (string.IsNullOrWhiteSpace(featureFlagKey))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(featureFlagKey));

        if (rolloutPercentage < FeatureFlagConstants.MinPercentage ||
            rolloutPercentage > FeatureFlagConstants.MaxPercentage)
            throw new ArgumentException("Rollout percentage must be between 0 and 100", nameof(rolloutPercentage));

        _logger.LogDebug("IsUserInRollout called with UserId: {UserId}, FeatureFlagKey: {FeatureFlagKey}, RolloutPercentage: {RolloutPercentage}",
            userContext.UserId, featureFlagKey, rolloutPercentage);
        var bucket = GetUserBucket(userContext, featureFlagKey);
        // Percentage rollout: buckets 0 to rolloutPercentage-1 are enabled
        // This ensures strict boundary semantics: 0% enables 0 buckets, 100% enables all 100 buckets
        var result = bucket < rolloutPercentage;
        _logger.LogDebug("IsUserInRollout returning {Result}", result);
        return result;
    }

    /// <summary>
    /// Computes the consistent hash bucket for the given user and feature flag key.
    /// </summary>
    /// <param name="userContext">The user context to bucket.</param>
    /// <param name="featureFlagKey">The key of the feature flag being evaluated.</param>
    /// <returns>An integer bucket in the range 0 to 99 used to determine rollout membership.</returns>
    public int GetUserBucket(UserContext userContext, string featureFlagKey)
    {
        if (userContext is null)
            throw new ArgumentNullException(nameof(userContext));

        if (string.IsNullOrWhiteSpace(featureFlagKey))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(featureFlagKey));

        return userContext.GetConsistentHash(featureFlagKey);
    }

    Task<bool> IPercentageRolloutService.EvaluateAsync(FeatureFlag featureFlag, UserContext userContext) => EvaluateAsync(featureFlag, userContext);
}
