#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Models;
using FluentAssertions;

namespace FeatureFlags.Tests.Services;

/// <summary>
/// Extension methods for RuleEvaluationServiceTests providing reusable test utilities.
/// </summary>
public static class RuleEvaluationServiceTestsExtensions
{
    /// <summary>
    /// Creates a standard user context with common properties for testing.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="email">The user email.</param>
    /// <param name="country">The user country.</param>
    /// <param name="tier">The user tier.</param>
    /// <returns>A configured UserContext instance.</returns>
    public static UserContext CreateUserContext(
        string userId = "test-user",
        string email = "test@example.com",
        string country = "US",
        string tier = "standard")
    {
        return new UserContext
        {
            UserId = userId,
            Email = email,
            Country = country,
            Tier = tier
        };
    }

    /// <summary>
    /// Creates a condition with the specified parameters.
    /// </summary>
    /// <param name="attributeName">The attribute name to evaluate.</param>
    /// <param name="operator">The comparison operator.</param>
    /// <param name="expectedValue">The expected value for the condition.</param>
    /// <param name="isActive">Whether the condition is active.</param>
    /// <returns>A configured Condition instance.</returns>
    public static Condition CreateCondition(
        string attributeName = "country",
        ConditionOperator @operator = ConditionOperator.Equals,
        string expectedValue = "US",
        bool isActive = true)
    {
        return new Condition
        {
            AttributeName = attributeName,
            Operator = @operator,
            ExpectedValue = expectedValue,
            IsActive = isActive
        };
    }

    /// <summary>
    /// Creates a rule with the specified parameters.
    /// </summary>
    /// <param name="name">The rule name.</param>
    /// <param name="isActive">Whether the rule is active.</param>
    /// <param name="conditionLogic">The condition logic (AND/OR).</param>
    /// <param name="conditions">The list of conditions.</param>
    /// <returns>A configured Rule instance.</returns>
    public static Rule CreateRule(
        string name = "Test Rule",
        bool isActive = true,
        string conditionLogic = "AND",
        List<Condition>? conditions = null)
    {
        return new Rule
        {
            Name = name,
            IsActive = isActive,
            ConditionLogic = conditionLogic,
            Conditions = conditions ?? new List<Condition>()
        };
    }

    /// <summary>
    /// Asserts that a condition evaluation returns the expected result.
    /// </summary>
    /// <param name="actual">The actual result from condition evaluation.</param>
    /// <param name="expected">The expected result.</param>
    /// <param name="condition">The condition that was evaluated.</param>
    /// <param name="userContext">The user context used for evaluation.</param>
    public static void AssertConditionResult(
        this bool actual,
        bool expected,
        Condition condition,
        UserContext userContext)
    {
        actual.Should().Be(expected,
            $"Condition evaluation failed for attribute '{condition.AttributeName}' with operator '{condition.Operator}' and expected value '{condition.ExpectedValue}'. " +
            $"User context: UserId='{userContext.UserId}', Country='{userContext.Country}', Tier='{userContext.Tier}'");
    }

    /// <summary>
    /// Asserts that a rule evaluation returns the expected result.
    /// </summary>
    /// <param name="actual">The actual result from rule evaluation.</param>
    /// <param name="expected">The expected result.</param>
    /// <param name="rule">The rule that was evaluated.</param>
    /// <param name="userContext">The user context used for evaluation.</param>
    public static async Task AssertRuleResultAsync(
        this Task<bool> actualTask,
        bool expected,
        Rule rule,
        UserContext userContext)
    {
        var actual = await actualTask;
        actual.Should().Be(expected,
            $"Rule evaluation failed for rule '{rule.Name}' with logic '{rule.ConditionLogic}'. " +
            $"Rule is active: {rule.IsActive}, Conditions count: {rule.Conditions.Count}. " +
            $"User context: UserId='{userContext.UserId}', Country='{userContext.Country}', Tier='{userContext.Tier}'");
    }

    /// <summary>
    /// Creates a rule with a single condition for quick testing.
    /// </summary>
    /// <param name="attributeName">The attribute name.</param>
    /// <param name="operator">The comparison operator.</param>
    /// <param name="expectedValue">The expected value.</param>
    /// <param name="isActive">Whether the condition is active.</param>
    /// <param name="ruleName">The rule name.</param>
    /// <param name="isRuleActive">Whether the rule is active.</param>
    /// <param name="conditionLogic">The condition logic (AND/OR).</param>
    /// <returns>A configured Rule instance with a single condition.</returns>
    public static Rule CreateSingleConditionRule(
        string attributeName = "country",
        ConditionOperator @operator = ConditionOperator.Equals,
        string expectedValue = "US",
        bool isActive = true,
        string ruleName = "Single Condition Rule",
        bool isRuleActive = true,
        string conditionLogic = "AND")
    {
        return new Rule
        {
            Name = ruleName,
            IsActive = isRuleActive,
            ConditionLogic = conditionLogic,
            Conditions = new List<Condition>
            {
                CreateCondition(attributeName, @operator, expectedValue, isActive)
            }
        };
    }
}