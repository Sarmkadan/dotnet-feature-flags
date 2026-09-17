#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;

namespace FeatureFlags.Models;

/// <summary>
/// Helper extensions for Condition objects.
/// </summary>
public static class ConditionHelperExtensions
{
    /// <summary>
    /// Returns a human-readable description of the condition.
    /// </summary>
    /// <param name="condition">The condition to describe.</param>
    /// <returns>A string describing the condition in natural language.</returns>
    public static string ToHumanReadableString(this Condition condition)
    {
        if (condition == null)
            return string.Empty;

        // Convert operator to a more readable format (e.g., "Equals" -> "equals")
        string op = condition.Operator.ToString().ToLowerInvariant();

        // Handle special cases for better readability
        return op switch
        {
            "equals" => $"If {condition.AttributeName} is equal to '{condition.ExpectedValue}'",
            "notequals" => $"If {condition.AttributeName} is not equal to '{condition.ExpectedValue}'",
            "contains" => $"If {condition.AttributeName} contains '{condition.ExpectedValue}'",
            "startswith" => $"If {condition.AttributeName} starts with '{condition.ExpectedValue}'",
            "endswith" => $"If {condition.AttributeName} ends with '{condition.ExpectedValue}'",
            "greaterthan" => $"If {condition.AttributeName} is greater than '{condition.ExpectedValue}'",
            "lessthan" => $"If {condition.AttributeName} is less than '{condition.ExpectedValue}'",
            "in" => $"If {condition.AttributeName} is one of '{condition.ExpectedValue}'",
            _ => $"{condition.AttributeName} {op} {condition.ExpectedValue}"
        };
    }
}