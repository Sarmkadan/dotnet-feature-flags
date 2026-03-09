#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Enums;

/// <summary>
/// Defines the types of actions that are logged in the audit trail.
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// Feature flag was created.
    /// </summary>
    Created = 0,

    /// <summary>
    /// Feature flag was enabled.
    /// </summary>
    Enabled = 1,

    /// <summary>
    /// Feature flag was disabled.
    /// </summary>
    Disabled = 2,

    /// <summary>
    /// Feature flag configuration was updated.
    /// </summary>
    Updated = 3,

    /// <summary>
    /// Rollout percentage was changed.
    /// </summary>
    RolloutChanged = 4,

    /// <summary>
    /// Targeting rule was added.
    /// </summary>
    RuleAdded = 5,

    /// <summary>
    /// Targeting rule was removed.
    /// </summary>
    RuleRemoved = 6,

    /// <summary>
    /// Feature flag was deleted.
    /// </summary>
    Deleted = 7,

    /// <summary>
    /// A/B test variant was updated.
    /// </summary>
    VariantUpdated = 8,

    /// <summary>
    /// Feature flag was evaluated.
    /// </summary>
    Evaluated = 9
}
