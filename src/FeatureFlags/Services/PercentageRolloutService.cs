// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using Microsoft.Extensions.Logging;

namespace FeatureFlags.Services;

/// <summary>
/// Service implementation for percentage-based rollout evaluation.
/// Uses consistent hashing to provide stable, reproducible rollout decisions.
/// </summary>
public class PercentageRolloutService : IPercentageRolloutService
{
    private readonly ILogger<PercentageRolloutService> _logger;

    public PercentageRolloutService(ILogger<PercentageRolloutService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> EvaluateAsync(FeatureFlag featureFlag, UserContext userContext)
    {
        if (featureFlag == null)
            throw new ArgumentNullException(nameof(featureFlag));

        if (userContext == null)
            throw new ArgumentNullException(nameof(userContext));

        if (featureFlag.PercentageRollout == null)
            throw new InvalidOperationException("Feature flag does not have a percentage rollout configured");

        var isEnabled = IsUserInRollout(userContext, featureFlag.Key, featureFlag.PercentageRollout.Value);

        _logger.LogDebug("Feature flag '{Key}' percentage evaluation for user {UserId}: {Result}",
            featureFlag.Key, userContext.UserId, isEnabled);

        return await Task.FromResult(isEnabled);
    }

    public bool IsUserInRollout(UserContext userContext, string featureFlagKey, int rolloutPercentage)
    {
        if (userContext == null)
            throw new ArgumentNullException(nameof(userContext));

        if (string.IsNullOrWhiteSpace(featureFlagKey))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(featureFlagKey));

        if (rolloutPercentage < 0 || rolloutPercentage > 100)
            throw new ArgumentException("Rollout percentage must be between 0 and 100", nameof(rolloutPercentage));

        if (rolloutPercentage == 100)
            return true;

        if (rolloutPercentage == 0)
            return false;

        var bucket = GetUserBucket(userContext, featureFlagKey);
        return bucket < rolloutPercentage;
    }

    public int GetUserBucket(UserContext userContext, string featureFlagKey)
    {
        if (userContext == null)
            throw new ArgumentNullException(nameof(userContext));

        if (string.IsNullOrWhiteSpace(featureFlagKey))
            throw new ArgumentException("Feature flag key cannot be empty", nameof(featureFlagKey));

        return userContext.GetConsistentHash(featureFlagKey);
    }
}
