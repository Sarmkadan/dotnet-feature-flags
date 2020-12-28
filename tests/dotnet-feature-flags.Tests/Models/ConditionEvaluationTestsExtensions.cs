#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Models;
using FluentAssertions;
using Xunit;

namespace FeatureFlags.Tests.Models;

/// <summary>
/// Extension methods for ConditionEvaluationTests to provide additional test utilities
/// for evaluating condition logic with various scenarios.
/// </summary>
public static class ConditionEvaluationTestsExtensions
{
    /// <summary>
    /// Creates a condition with the specified parameters for testing.
    /// </summary>
    /// <param name="attributeName">The attribute name to evaluate</param>
    /// <param name="operator">The condition operator</param>
    /// <param name="expectedValue">The expected value for comparison</param>
    /// <param name="isActive">Whether the condition is active</param>
    /// <returns>A configured Condition object</returns>
    public static Condition CreateCondition(
        this ConditionEvaluationTests _,
        string attributeName,
        ConditionOperator op,
        string expectedValue,
        bool isActive = true)
    {
        return new Condition
        {
            AttributeName = attributeName,
            Operator = op,
            ExpectedValue = expectedValue,
            IsActive = isActive
        };
    }

    /// <summary>
    /// Asserts that a condition evaluates to true with the given context value.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="condition">The condition to evaluate</param>
    /// <param name="contextValue">The context value to test against</param>
    public static void ShouldEvaluateToTrue(
        this ConditionEvaluationTests test,
        Condition condition,
        string contextValue)
    {
        var result = condition.Evaluate(contextValue);
        result.Should().BeTrue($"Condition with attribute '{condition.AttributeName}' and operator '{condition.Operator}' should evaluate to true with context value '{contextValue}'");
    }

    /// <summary>
    /// Asserts that a condition evaluates to false with the given context value.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="condition">The condition to evaluate</param>
    /// <param name="contextValue">The context value to test against</param>
    public static void ShouldEvaluateToFalse(
        this ConditionEvaluationTests test,
        Condition condition,
        string contextValue)
    {
        var result = condition.Evaluate(contextValue);
        result.Should().BeFalse($"Condition with attribute '{condition.AttributeName}' and operator '{condition.Operator}' should evaluate to false with context value '{contextValue}'");
    }

    /// <summary>
    /// Creates a collection of conditions for testing multiple scenarios.
    /// </summary>
    /// <param name="test">The test instance</param>
    /// <param name="conditions">Array of condition configurations</param>
    /// <returns>Array of Condition objects</returns>
    public static Condition[] CreateConditions(
        this ConditionEvaluationTests _,
        params (string AttributeName, ConditionOperator Operator, string ExpectedValue, bool IsActive)[] conditions)
    {
        return conditions.Select(c => new Condition
        {
            AttributeName = c.AttributeName,
            Operator = c.Operator,
            ExpectedValue = c.ExpectedValue,
            IsActive = c.IsActive
        }).ToArray();
    }
}