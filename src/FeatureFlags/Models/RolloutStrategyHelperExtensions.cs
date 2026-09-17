#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Models;

namespace FeatureFlags.Models;

/// <summary>
/// Provides helper extension methods for <see cref="RolloutStrategy"/>.
/// </summary>
public static class RolloutStrategyHelperExtensions
{
    /// <summary>
    /// Determines if the rollout strategy represents a fully rolled out state.
    /// This is true when the strategy is active and represents a full rollout (100%).
    /// </summary>
    /// <param name="strategy">The rollout strategy to check.</param>
    /// <returns>True if the strategy is fully rolled out; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> is null.</exception>
    public static bool IsFullyRolledOut(this RolloutStrategy strategy)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        return strategy.IsActive() && strategy.Type == RolloutType.Full;
    }
}