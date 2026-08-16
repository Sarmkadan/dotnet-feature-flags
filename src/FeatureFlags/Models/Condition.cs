#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using FeatureFlags.Enums;

namespace FeatureFlags.Models;

/// <summary>
/// Represents a single condition within a rule that evaluates context attributes.
/// Supports various operators (Equals, Contains, GreaterThan, etc.) for flexible targeting.
/// </summary>
public sealed class Condition : IEquatable<Condition>
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

    public bool Equals(Condition? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Id == other.Id &&
               RuleId == other.RuleId &&
               AttributeName == other.AttributeName &&
               Operator == other.Operator &&
               ExpectedValue == other.ExpectedValue &&
               IsActive == other.IsActive &&
               CreatedAt == other.CreatedAt &&
               EqualityComparer<Rule?>.Default.Equals(Rule, other.Rule);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Condition);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, RuleId, AttributeName, Operator, ExpectedValue, IsActive, CreatedAt, Rule);
    }

    public static bool operator ==(Condition? left, Condition? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Condition? left, Condition? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Evaluates the condition against a provided value from the user context.
    /// Supports string comparisons including case-insensitive matching.
    /// </summary>
    public bool Evaluate(string? contextValue)
    {
        if (contextValue is null)
            return false;

        return Operator switch
        {
            ConditionOperator.Equals => contextValue.Equals(ExpectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.NotEquals => !contextValue.Equals(ExpectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.Contains => contextValue.Contains(ExpectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.StartsWith => contextValue.StartsWith(ExpectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.EndsWith => contextValue.EndsWith(ExpectedValue, StringComparison.OrdinalIgnoreCase),
            ConditionOperator.GreaterThan => double.TryParse(contextValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var cv)
                && double.TryParse(ExpectedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var ev)
                && cv > ev,
            ConditionOperator.LessThan => double.TryParse(contextValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var cv2)
                && double.TryParse(ExpectedValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var ev2)
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
        if (string.IsNullOrWhiteSpace(AttributeName) || AttributeName.Length > 100)
            return false;

        if (string.IsNullOrWhiteSpace(ExpectedValue) || ExpectedValue.Length > 1000)
            return false;

        return Enum.IsDefined(typeof(ConditionOperator), Operator);
    }
}
