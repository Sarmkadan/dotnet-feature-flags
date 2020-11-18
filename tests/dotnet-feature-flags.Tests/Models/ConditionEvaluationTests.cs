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
/// Unit tests for Condition evaluation logic covering all supported operators.
/// </summary>
public sealed class ConditionEvaluationTests
{
    [Fact]
    public void Evaluate_WithEqualsOperator_CaseInsensitiveMatch_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.Equals,
            ExpectedValue = "US",
            IsActive = true
        };

        // Act
        var result = condition.Evaluate("us");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WithGreaterThanOperator_NumericContextValue_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "accountAge",
            Operator = ConditionOperator.GreaterThan,
            ExpectedValue = "30",
            IsActive = true
        };

        // Act
        var result = condition.Evaluate("90");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WithInOperator_ValueExistsInCommaDelimitedList_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "tier",
            Operator = ConditionOperator.In,
            ExpectedValue = "gold,platinum,enterprise",
            IsActive = true
        };

        // Act
        var result = condition.Evaluate("platinum");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WithNullContextValue_ReturnsFalse()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "region",
            Operator = ConditionOperator.Equals,
            ExpectedValue = "EU",
            IsActive = true
        };

        // Act
        var result = condition.Evaluate(null);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("feature", "feat", ConditionOperator.StartsWith, true)]
    [InlineData("user@corp.com", "corp.com", ConditionOperator.EndsWith, true)]
    [InlineData("premium_plan", "premium", ConditionOperator.Contains, true)]
    [InlineData("US", "CA", ConditionOperator.NotEquals, true)]
    public void Evaluate_WithStringOperators_ReturnsExpectedResult(
        string contextValue, string expectedValue, ConditionOperator op, bool expected)
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "attr",
            Operator = op,
            ExpectedValue = expectedValue,
            IsActive = true
        };

        // Act
        var result = condition.Evaluate(contextValue);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void IsValid_WithEmptyExpectedValue_ReturnsFalse()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.Equals,
            ExpectedValue = string.Empty
        };

        // Act
        var result = condition.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithAllRequiredFields_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "tier",
            Operator = ConditionOperator.Equals,
            ExpectedValue = "premium"
        };

        // Act
        var result = condition.IsValid();

        // Assert
        result.Should().BeTrue();
    }
}
