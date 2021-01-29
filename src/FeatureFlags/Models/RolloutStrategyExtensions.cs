#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Models;

namespace FeatureFlags.Models;

/// <summary>
/// Provides extension methods for <see cref="RolloutStrategy"/> to enhance functionality
/// with common rollout operations and validations.
/// </summary>
public static class RolloutStrategyExtensions
{
    /// <summary>
    /// Determines if the rollout strategy is a percentage-based rollout.
    /// </summary>
    /// <param name="strategy">The rollout strategy to check.</param>
    /// <returns>True if the strategy is percentage-based; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
    public static bool IsPercentageBased(this RolloutStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        return strategy.Type == RolloutType.Percentage;
    }

    /// <summary>
    /// Determines if the rollout strategy is rules-based.
    /// </summary>
    /// <param name="strategy">The rollout strategy to check.</param>
    /// <returns>True if the strategy is rules-based; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
    public static bool IsRulesBased(this RolloutStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        return strategy.Type == RolloutType.RulesBased;
    }

    /// <summary>
    /// Determines if the rollout strategy is an A/B test.
    /// </summary>
    /// <param name="strategy">The rollout strategy to check.</param>
    /// <returns>True if the strategy is an A/B test; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
    public static bool IsABTest(this RolloutStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        return strategy.Type == RolloutType.ABTest;
    }

    /// <summary>
    /// Determines if the rollout strategy represents a full rollout (100%).
    /// </summary>
    /// <param name="strategy">The rollout strategy to check.</param>
    /// <returns>True if the strategy represents full rollout; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
    public static bool IsFullRollout(this RolloutStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        return strategy.Type == RolloutType.Full;
    }

    /// <summary>
    /// Determines if the rollout strategy represents no rollout (0%).
    /// </summary>
    /// <param name="strategy">The rollout strategy to check.</param>
    /// <returns>True if the strategy represents no rollout; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
    public static bool IsNoRollout(this RolloutStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        return strategy.Type == RolloutType.None;
    }

    /// <summary>
    /// Gets the effective percentage for this rollout strategy, considering
    /// both percentage-based and time-based gradual rollout scenarios.
    /// </summary>
    /// <param name="strategy">The rollout strategy.</param>
    /// <returns>The effective percentage (0-100).</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
    public static int GetEffectivePercentage(this RolloutStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);

        // For percentage-based strategies, return the current percentage
        if (strategy.IsPercentageBased())
        {
            return strategy.GetCurrentPercentage();
        }

        // For A/B tests, return the end percentage if available, otherwise current
        if (strategy.IsABTest())
        {
            return strategy.EndPercentage ?? strategy.GetCurrentPercentage();
        }

        // For rules-based and full rollout, return 100 if active
        if (strategy.IsFullRollout() || strategy.IsRulesBased())
        {
            return strategy.IsActive() ? 100 : 0;
        }

        // For no rollout
        return 0;
    }

    /// <summary>
    /// Calculates the progress percentage of the gradual rollout.
    /// Returns 0 for non-gradual strategies.
    /// </summary>
    /// <param name="strategy">The rollout strategy.</param>
    /// <returns>Progress percentage (0-100), or 0 if not applicable.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
    public static int GetProgressPercentage(this RolloutStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);

        if (!strategy.IsGradual || strategy.StartPercentage is null || strategy.EndPercentage is null)
        {
            return 0;
        }

        var current = strategy.GetCurrentPercentage();
        var start = strategy.StartPercentage.Value;
        var end = strategy.EndPercentage.Value;

        // Handle edge case where start equals end
        if (start >= end)
        {
            return current >= end ? 100 : 0;
        }

        // Prevent division by zero and ensure progress stays within bounds
        var range = end - start;
        var progress = current - start;

        return range > 0 ? Math.Min(100, (int)Math.Round((double)progress / range * 100)) : 0;
    }

    /// <summary>
    /// Determines if the rollout strategy has reached its target percentage.
    /// </summary>
    /// <param name="strategy">The rollout strategy.</param>
    /// <returns>True if the strategy has reached or exceeded its target; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
    public static bool HasReachedTarget(this RolloutStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);

        if (!strategy.IsActive() || !strategy.IsValid())
        {
            return false;
        }

        var current = strategy.GetCurrentPercentage();
        var end = strategy.EndPercentage ?? 100;

        return current >= end;
    }

    /// <summary>
    /// Gets a human-readable description of the rollout strategy type.
    /// </summary>
    /// <param name="strategy">The rollout strategy.</param>
    /// <returns>A description string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
    public static string GetDescription(this RolloutStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);

        return strategy.Type switch
        {
            RolloutType.Percentage => "Percentage-based rollout using consistent hashing",
            RolloutType.RulesBased => "Rules-based rollout with targeting conditions",
            RolloutType.ABTest => "A/B test with multiple variants",
            RolloutType.Full => "Full rollout to all users (100%)",
            RolloutType.None => "No rollout (0%)",
            _ => $"Unknown rollout type: {strategy.Type}"
        };
    }
}