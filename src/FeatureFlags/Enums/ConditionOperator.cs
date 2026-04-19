#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Enums;

/// <summary>
/// Defines the operators available for condition evaluation in targeting rules.
/// </summary>
public enum ConditionOperator
{
    /// <summary>
    /// Exact match comparison (case-insensitive).
    /// </summary>
    Equals = 0,

    /// <summary>
    /// Not equal comparison.
    /// </summary>
    NotEquals = 1,

    /// <summary>
    /// String contains comparison.
    /// </summary>
    Contains = 2,

    /// <summary>
    /// String starts with comparison.
    /// </summary>
    StartsWith = 3,

    /// <summary>
    /// String ends with comparison.
    /// </summary>
    EndsWith = 4,

    /// <summary>
    /// Numeric greater than comparison.
    /// </summary>
    GreaterThan = 5,

    /// <summary>
    /// Numeric less than comparison.
    /// </summary>
    LessThan = 6,

    /// <summary>
    /// Value is in a comma-separated list.
    /// </summary>
    In = 7
}
