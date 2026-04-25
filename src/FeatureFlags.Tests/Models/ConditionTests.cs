#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Models;
using Xunit;
using FluentAssertions;

namespace FeatureFlags.Tests.Models;

/// <summary>
/// Unit tests for Condition model.
/// Tests all condition operators and evaluation logic.
/// </summary>
public class ConditionTests
{
    [Fact]
    public void Evaluate_EqualsOperator_ReturnsTrueForMatch()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.Equals,
            ExpectedValue = "US"
        };

        // Act
        var result = condition.Evaluate("US");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_EqualsOperator_CaseInsensitive()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.Equals,
            ExpectedValue = "US"
        };

        // Act
        var result = condition.Evaluate("us");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_NotEqualsOperator_ReturnsTrueForDifference()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.NotEquals,
            ExpectedValue = "US"
        };

        // Act
        var result = condition.Evaluate("CA");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ContainsOperator_ReturnsTrueForMatch()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "email",
            Operator = ConditionOperator.Contains,
            ExpectedValue = "example.com"
        };

        // Act
        var result = condition.Evaluate("user@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_StartsWithOperator_ReturnsTrueForMatch()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "email",
            Operator = ConditionOperator.StartsWith,
            ExpectedValue = "user"
        };

        // Act
        var result = condition.Evaluate("user@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_EndsWithOperator_ReturnsTrueForMatch()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "email",
            Operator = ConditionOperator.EndsWith,
            ExpectedValue = ".com"
        };

        // Act
        var result = condition.Evaluate("user@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_GreaterThanOperator_ReturnsTrueForGreaterValue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "account-age",
            Operator = ConditionOperator.GreaterThan,
            ExpectedValue = "30"
        };

        // Act
        var result = condition.Evaluate("45");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_LessThanOperator_ReturnsTrueForLesserValue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "account-age",
            Operator = ConditionOperator.LessThan,
            ExpectedValue = "30"
        };

        // Act
        var result = condition.Evaluate("15");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_InOperator_ReturnsTrueForValueInList()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "tier",
            Operator = ConditionOperator.In,
            ExpectedValue = "premium,gold,platinum"
        };

        // Act
        var result = condition.Evaluate("gold");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_NullContextValue_ReturnsFalse()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.Equals,
            ExpectedValue = "US"
        };

        // Act
        var result = condition.Evaluate(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithRequiredFields_ReturnsTrue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.Equals,
            ExpectedValue = "US"
        };

        // Act
        var result = condition.IsValid();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WithoutAttributeName_ReturnsFalse()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = string.Empty,
            Operator = ConditionOperator.Equals,
            ExpectedValue = "US"
        };

        // Act
        var result = condition.IsValid();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WithoutExpectedValue_ReturnsFalse()
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
}
