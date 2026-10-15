#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using FeatureFlags.Enums;

namespace FeatureFlags.Models;

/// <summary>
/// Represents a targeting rule that applies conditions to determine if a feature flag should be enabled.
/// Rules can be combined using AND/OR logic to create complex targeting scenarios.
/// </summary>
[DebuggerDisplay("{ToString(),nq}")]
public sealed class Rule
{
    /// <summary>
    /// Gets or sets the unique identifier of the rule.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the feature flag this rule belongs to.
    /// </summary>
    public int FeatureFlagId { get; set; }

    /// <summary>
    /// Gets or sets the name of the rule.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of the rule.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority of the rule; rules with higher priority are evaluated first.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the rule is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the logic used to combine conditions; either "AND" or "OR".
    /// </summary>
    public string ConditionLogic { get; set; } = "AND"; // AND or OR

    /// <summary>
    /// Gets or sets the date and time when the rule was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the rule was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Gets or sets the feature flag this rule belongs to.
    /// </summary>
    public FeatureFlag? FeatureFlag { get; set; }

    /// <summary>
    /// Gets or sets the collection of conditions that make up this rule.
    /// </summary>
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

    /// <summary>
    /// Returns a diagnostic summary of the active conditions, for example "country IN [DE, AT]"
    /// or "country = US AND plan = pro".
    /// </summary>
    public override string ToString()
    {
        var active = Conditions?.Where(c => c.IsActive).ToList() ?? new List<Condition>();
        if (active.Count == 0)
            return "(no active conditions)";

        var separator = ConditionLogic.Equals("OR", StringComparison.OrdinalIgnoreCase) ? " OR " : " AND ";
        return string.Join(separator, active.Select(FormatCondition));
    }

    private static string FormatCondition(Condition condition)
    {
        var op = condition.Operator switch
        {
            ConditionOperator.Equals => "=",
            ConditionOperator.NotEquals => "!=",
            ConditionOperator.Contains => "CONTAINS",
            ConditionOperator.StartsWith => "STARTS WITH",
            ConditionOperator.EndsWith => "ENDS WITH",
            ConditionOperator.GreaterThan => ">",
            ConditionOperator.LessThan => "<",
            ConditionOperator.In => "IN",
            _ => condition.Operator.ToString()
        };

        var value = condition.Operator == ConditionOperator.In
            ? "[" + string.Join(", ", condition.ExpectedValue.Split(',').Select(v => v.Trim())) + "]"
            : condition.ExpectedValue;

        return $"{condition.AttributeName} {op} {value}";
    }
}
