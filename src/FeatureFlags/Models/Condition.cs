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
    /// <summary>
    /// Primary key of the condition.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the <see cref="Rule"/> this condition belongs to.
    /// </summary>
    public int RuleId { get; set; }

    /// <summary>
    /// Name of the user context attribute to evaluate (for example, "country" or "plan").
    /// Must be non-empty and at most 100 characters.
    /// </summary>
    public string AttributeName { get; set; } = string.Empty;

    /// <summary>
    /// Comparison operator applied between the context value and <see cref="ExpectedValue"/>.
    /// </summary>
    public ConditionOperator Operator { get; set; }

    /// <summary>
    /// Value the context attribute is compared against. For <see cref="ConditionOperator.In"/>,
    /// this is a comma-separated list of values. Must be non-empty and at most 1000 characters.
    /// </summary>
    public string ExpectedValue { get; set; } = string.Empty;

    /// <summary>
    /// Whether the condition participates in rule evaluation. Defaults to <c>true</c>.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp of when the condition was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Navigation property to the owning <see cref="Rule"/>.
    /// </summary>
    public Rule? Rule { get; set; }

    /// <summary>
    /// Determines whether the specified <see cref="Condition"/> is equal to this instance.
    /// Compares all persisted properties and the <see cref="Rule"/> navigation property.
    /// </summary>
    /// <param name="other">The condition to compare with.</param>
    /// <returns><c>true</c> if the conditions are equal; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Determines whether the specified object is a <see cref="Condition"/> equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns><c>true</c> if <paramref name="obj"/> is an equal <see cref="Condition"/>; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Condition);
    }

    /// <summary>
    /// Returns a hash code consistent with <see cref="Equals(Condition?)"/>.
    /// </summary>
    /// <returns>A hash code for this instance.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, RuleId, AttributeName, Operator, ExpectedValue, IsActive, CreatedAt, Rule);
    }

    /// <summary>
    /// Determines whether two <see cref="Condition"/> instances are equal.
    /// Two <c>null</c> references are considered equal.
    /// </summary>
    /// <param name="left">The first condition.</param>
    /// <param name="right">The second condition.</param>
    /// <returns><c>true</c> if the conditions are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(Condition? left, Condition? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="Condition"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first condition.</param>
    /// <param name="right">The second condition.</param>
    /// <returns><c>true</c> if the conditions are not equal; otherwise, <c>false</c>.</returns>
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

    /// <summary>
    /// Returns a human-readable representation of the condition, for example "country Equals US".
    /// </summary>
    /// <returns>A string in the form "{AttributeName} {Operator} {ExpectedValue}".</returns>
    public override string ToString() => $"{AttributeName} {Operator} {ExpectedValue}";
}
