// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace FeatureFlags.Models;

/// <summary>
/// Represents a targeting rule that applies conditions to determine if a feature flag should be enabled.
/// Rules can be combined using AND/OR logic to create complex targeting scenarios.
/// </summary>
public class Rule
{
    public int Id { get; set; }

    public int FeatureFlagId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Priority { get; set; }

    public bool IsActive { get; set; } = true;

    public string ConditionLogic { get; set; } = "AND"; // AND or OR

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public FeatureFlag? FeatureFlag { get; set; }

    public ICollection<Condition> Conditions { get; set; } = new List<Condition>();

    /// <summary>
    /// Validates that the rule has at least one condition and proper naming.
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return false;

        if (!Conditions?.Any() ?? true)
            return false;

        if (ConditionLogic != "AND" && ConditionLogic != "OR")
            return false;

        if (Priority < 0)
            return false;

        return true;
    }

    /// <summary>
    /// Gets the total number of active conditions in this rule.
    /// </summary>
    public int GetActiveConditionCount()
    {
        return Conditions?.Count(c => c.IsActive) ?? 0;
    }

    /// <summary>
    /// Determines evaluation order; rules with higher priority are evaluated first.
    /// </summary>
    public int GetEvaluationPriority()
    {
        return Priority;
    }
}
