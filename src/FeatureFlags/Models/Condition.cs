// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;

namespace FeatureFlags.Models;

/// <summary>
/// Represents a single condition within a rule that evaluates context attributes.
/// Supports various operators (Equals, Contains, GreaterThan, etc.) for flexible targeting.
/// </summary>
public class Condition
{
    public int Id { get; set; }

    public int RuleId { get; set; }

    public string AttributeName { get; set; } = string.Empty;

    public ConditionOperator Operator { get; set; }

    public string ExpectedValue { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Rule? Rule { get; set; }

    /// <summary>
    /// Evaluates the condition against a provided value from the user context.
    /// Supports string comparisons including case-insensitive matching.
    /// </summary>
    public bool Evaluate(string? contextValue)
    {
        if (contextValue == null)
            return false;

        return Operator switch
        {
            ConditionOperator.Equals => contextValue.Equals(ExpectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.NotEquals => !contextValue.Equals(ExpectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.Contains => contextValue.Contains(ExpectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.StartsWith => contextValue.StartsWith(ExpectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.EndsWith => contextValue.EndsWith(ExpectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.GreaterThan => double.TryParse(contextValue, out var cv)
                && double.TryParse(ExpectedValue, out var ev)
                && cv > ev,
            ConditionOperator.LessThan => double.TryParse(contextValue, out var cv2)
                && double.TryParse(ExpectedValue, out var ev2)
                && cv2 < ev2,
            ConditionOperator.In => ExpectedValue.Split(',').Any(v => v.Trim().Equals(contextValue, StringComparison.OrdinalIgnoreCase)),
            _ => false
        };
    }

    /// <summary>
    /// Validates that the condition has all required properties and valid operator.
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(AttributeName))
            return false;

        if (string.IsNullOrWhiteSpace(ExpectedValue))
            return false;

        return Enum.IsDefined(typeof(ConditionOperator), Operator);
    }
}
