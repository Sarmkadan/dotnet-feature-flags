#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;

namespace FeatureFlags.Models;

/// <summary>
/// Provides extension methods for <see cref="FeatureFlag"/> to enhance functionality
/// with common evaluation and inspection operations.
/// </summary>
public static class FeatureFlagExtensions
{
    /// <summary>
    /// Determines whether the feature flag is currently enabled and has a rollout strategy
    /// that is actively distributing traffic.
    /// </summary>
    /// <param name="flag">The feature flag to check.</param>
    /// <returns>True if the flag is enabled and its rollout strategy is active; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="flag"/> is null.</exception>
    public static bool IsActive(this FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag);

        return flag.IsEnabled && flag.RolloutType != RolloutType.None;
    }

    /// <summary>
    /// Determines whether the feature flag uses a percentage-based rollout strategy.
    /// </summary>
    /// <param name="flag">The feature flag to check.</param>
    /// <returns>True if the flag uses a percentage-based rollout; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="flag"/> is null.</exception>
    public static bool IsPercentageBased(this FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag);

        return flag.RolloutType == RolloutType.Percentage;
    }

    /// <summary>
    /// Determines whether the feature flag uses an A/B test rollout strategy.
    /// </summary>
    /// <param name="flag">The feature flag to check.</param>
    /// <returns>True if the flag uses an A/B test rollout; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="flag"/> is null.</exception>
    public static bool IsABTest(this FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag);

        return flag.RolloutType == RolloutType.ABTest;
    }

    /// <summary>
    /// Determines whether the feature flag uses a rules-based rollout strategy.
    /// </summary>
    /// <param name="flag">The feature flag to check.</param>
    /// <returns>True if the flag uses a rules-based rollout; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="flag"/> is null.</exception>
    public static bool IsRulesBased(this FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag);

        return flag.RolloutType == RolloutType.RulesBased;
    }

    /// <summary>
    /// Gets the number of active targeting rules associated with the feature flag.
    /// </summary>
    /// <param name="flag">The feature flag to inspect.</param>
    /// <returns>The count of active rules.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="flag"/> is null.</exception>
    public static int GetActiveRuleCount(this FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag);

        return flag.Rules?.Count(r => r.IsActive) ?? 0;
    }

    /// <summary>
    /// Gets the number of A/B test variants associated with the feature flag.
    /// </summary>
    /// <param name="flag">The feature flag to inspect.</param>
    /// <returns>The count of variants.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="flag"/> is null.</exception>
    public static int GetVariantCount(this FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag);

        return flag.Variants?.Count ?? 0;
    }

    /// <summary>
    /// Determines whether the feature flag has any targeting rules defined.
    /// </summary>
    /// <param name="flag">The feature flag to check.</param>
    /// <returns>True if the flag has at least one rule; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="flag"/> is null.</exception>
    public static bool HasRules(this FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag);

        return flag.Rules?.Any() ?? false;
    }

    /// <summary>
    /// Determines whether the feature flag has any A/B test variants defined.
    /// </summary>
    /// <param name="flag">The feature flag to check.</param>
    /// <returns>True if the flag has at least one variant; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="flag"/> is null.</exception>
    public static bool HasVariants(this FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag);

        return flag.Variants?.Any() ?? false;
    }

    /// <summary>
    /// Gets the effective rollout percentage for the feature flag, considering the rollout type.
    /// </summary>
    /// <param name="flag">The feature flag to inspect.</param>
    /// <returns>The effective percentage (0-100).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="flag"/> is null.</exception>
    public static int GetEffectivePercentage(this FeatureFlag flag)
    {
        ArgumentNullException.ThrowIfNull(flag);

        if (!flag.IsEnabled)
            return 0;

        return flag.RolloutType switch
        {
            RolloutType.Percentage => flag.PercentageRollout ?? 0,
            RolloutType.Full => 100,
            RolloutType.None => 0,
            _ => 0
        };
    }
}