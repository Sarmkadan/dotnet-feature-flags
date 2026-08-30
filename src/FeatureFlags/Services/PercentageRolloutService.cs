#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

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

    public PercentageRolloutService(ILogger<PercentageRolloutService> logger)
    {
        _logger = logger;
    }

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

    public bool IsUserInRollout(UserContext userContext, string featureFlagKey, int rolloutPercentage)
    {
        if (userContext is null)
            throw new ArgumentNullException(nameof(userContext));

        if (string.IsNullOrWhiteSpace(featureFlagKey))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(featureFlagKey));

        if (rolloutPercentage < 0 || rolloutPercentage > 100)
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
