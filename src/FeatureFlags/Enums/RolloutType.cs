// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Enums;

/// <summary>
/// Defines the types of rollout strategies supported by the feature flag engine.
/// </summary>
public enum RolloutType
{
    /// <summary>
    /// Feature is rolled out to a percentage of users based on consistent hashing.
    /// </summary>
    Percentage = 0,

    /// <summary>
    /// Feature is rolled out based on rule evaluation and targeting conditions.
    /// </summary>
    RulesBased = 1,

    /// <summary>
    /// Feature is run as an A/B test with multiple variants and allocation percentages.
    /// </summary>
    ABTest = 2,

    /// <summary>
    /// Feature is available to all users (100% rollout).
    /// </summary>
    Full = 3,

    /// <summary>
    /// Feature is disabled for all users (0% rollout).
    /// </summary>
    None = 4
}
