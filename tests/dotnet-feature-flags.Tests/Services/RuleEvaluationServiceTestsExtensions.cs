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
    /// <exception cref="ArgumentNullException"><paramref name="userId"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="email"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="country"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="tier"/> is null.</exception>
    public static UserContext CreateUserContext(
        string userId = "test-user",
        string email = "test@example.com",
        string country = "US",
        string tier = "standard")
    {
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(country);
        ArgumentNullException.ThrowIfNull(tier);

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
    /// <exception cref="ArgumentNullException"><paramref name="attributeName"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="expectedValue"/> is null.</exception>
    public static Condition CreateCondition(
        string attributeName = "country",
        ConditionOperator @operator = ConditionOperator.Equals,
        string expectedValue = "US",
        bool isActive = true)
    {
        ArgumentNullException.ThrowIfNull(attributeName);
        ArgumentNullException.ThrowIfNull(expectedValue);

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
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="conditionLogic"/> is null.</exception>
    public static Rule CreateRule(
        string name = "Test Rule",
        bool isActive = true,
        string conditionLogic = "AND",
        List<Condition>? conditions = null)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(conditionLogic);

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
    /// <exception cref="ArgumentNullException"><paramref name="condition"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="userContext"/> is null.</exception>
    public static void AssertConditionResult(
        this bool actual,
        bool expected,
        Condition condition,
        UserContext userContext)
    {
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(userContext);

        actual.Should().Be(expected,
            $"Condition evaluation failed for attribute '{condition.AttributeName}' with operator '{condition.Operator}' and expected value '{condition.ExpectedValue}'. " +
            $"User context: UserId='{userContext.UserId}', Country='{userContext.Country}', Tier='{userContext.Tier}'");
    }

    /// <summary>
    /// Asserts that a rule evaluation returns the expected result.
    /// </summary>
    /// <param name="actualTask">The actual result task from rule evaluation.</param>
    /// <param name="expected">The expected result.</param>
    /// <param name="rule">The rule that was evaluated.</param>
    /// <param name="userContext">The user context used for evaluation.</param>
    /// <exception cref="ArgumentNullException"><paramref name="actualTask"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="rule"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="userContext"/> is null.</exception>
    public static async Task AssertRuleResultAsync(
        this Task<bool> actualTask,
        bool expected,
        Rule rule,
        UserContext userContext)
    {
        ArgumentNullException.ThrowIfNull(actualTask);
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(userContext);

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
    /// <exception cref="ArgumentNullException"><paramref name="attributeName"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="expectedValue"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="ruleName"/> is null.</exception>
    public static Rule CreateSingleConditionRule(
        string attributeName = "country",
        ConditionOperator @operator = ConditionOperator.Equals,
        string expectedValue = "US",
        bool isActive = true,
        string ruleName = "Single Condition Rule",
        bool isRuleActive = true,
        string conditionLogic = "AND")
    {
        ArgumentNullException.ThrowIfNull(attributeName);
        ArgumentNullException.ThrowIfNull(expectedValue);
        ArgumentNullException.ThrowIfNull(ruleName);
        ArgumentNullException.ThrowIfNull(conditionLogic);

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