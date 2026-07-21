#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
//
// Comprehensive tests for ConditionOperator evaluation covering all enum values
// including edge cases like type mismatches, null values, and case sensitivity.
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Models;
using Xunit;
using FluentAssertions;

namespace FeatureFlags.Tests;

/// <summary>
/// Comprehensive tests for all ConditionOperator enum values.
/// Tests include: happy paths, case sensitivity, type mismatches, null handling,
/// and edge cases for each operator.
/// </summary>
public sealed class ConditionOperatorEvaluationTests
{
    [Fact]
    public void Evaluate_EqualsOperator_ExactMatch()
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

        // Act & Assert
        condition.Evaluate("us").Should().BeTrue();
        condition.Evaluate("Us").Should().BeTrue();
        condition.Evaluate("uS").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_EqualsOperator_NoMatch()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.Equals,
            ExpectedValue = "US"
        };

        // Act & Assert
        condition.Evaluate("CA").Should().BeFalse();
        condition.Evaluate("UK").Should().BeFalse();
        condition.Evaluate("Germany").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_EqualsOperator_NullContextValue()
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
    public void Evaluate_NotEqualsOperator_Match()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.NotEquals,
            ExpectedValue = "US"
        };

        // Act & Assert
        condition.Evaluate("CA").Should().BeTrue();
        condition.Evaluate("UK").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_NotEqualsOperator_NoMatch()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.NotEquals,
            ExpectedValue = "US"
        };

        // Act
        var result = condition.Evaluate("US");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_NotEqualsOperator_CaseInsensitive()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.NotEquals,
            ExpectedValue = "US"
        };

        // Act & Assert
        condition.Evaluate("us").Should().BeFalse();
        condition.Evaluate("Us").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_NotEqualsOperator_NullContextValue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.NotEquals,
            ExpectedValue = "US"
        };

        // Act
        var result = condition.Evaluate(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_ContainsOperator_Match()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "email",
            Operator = ConditionOperator.Contains,
            ExpectedValue = "example.com"
        };

        // Act & Assert
        condition.Evaluate("user@example.com").Should().BeTrue();
        condition.Evaluate("admin@example.com").Should().BeTrue();
        condition.Evaluate("contact@example.com").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ContainsOperator_CaseInsensitive()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "email",
            Operator = ConditionOperator.Contains,
            ExpectedValue = "EXAMPLE.COM"
        };

        // Act & Assert
        condition.Evaluate("user@example.com").Should().BeTrue();
        condition.Evaluate("user@Example.Com").Should().BeTrue();
        condition.Evaluate("user@EXAMPLE.COM").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ContainsOperator_NoMatch()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "email",
            Operator = ConditionOperator.Contains,
            ExpectedValue = "example.com"
        };

        // Act & Assert
        condition.Evaluate("user@test.com").Should().BeFalse();
        condition.Evaluate("example.org").Should().BeFalse();
        condition.Evaluate("noreply@example.net").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_ContainsOperator_NullContextValue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "email",
            Operator = ConditionOperator.Contains,
            ExpectedValue = "example.com"
        };

        // Act
        var result = condition.Evaluate(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_StartsWithOperator_Match()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "path",
            Operator = ConditionOperator.StartsWith,
            ExpectedValue = "/api/v1"
        };

        // Act & Assert
        condition.Evaluate("/api/v1/users").Should().BeTrue();
        condition.Evaluate("/api/v1/products").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_StartsWithOperator_CaseInsensitive()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "path",
            Operator = ConditionOperator.StartsWith,
            ExpectedValue = "/API/V1"
        };

        // Act & Assert
        condition.Evaluate("/api/v1/users").Should().BeTrue();
        condition.Evaluate("/Api/V1/Products").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_StartsWithOperator_NoMatch()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "path",
            Operator = ConditionOperator.StartsWith,
            ExpectedValue = "/api/v1"
        };

        // Act & Assert
        condition.Evaluate("/api/v2/users").Should().BeFalse();
        condition.Evaluate("/admin/api/v1").Should().BeFalse();
        condition.Evaluate("api/v1/users").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_StartsWithOperator_NullContextValue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "path",
            Operator = ConditionOperator.StartsWith,
            ExpectedValue = "/api/v1"
        };

        // Act
        var result = condition.Evaluate(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_EndsWithOperator_Match()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "filename",
            Operator = ConditionOperator.EndsWith,
            ExpectedValue = ".pdf"
        };

        // Act & Assert
        condition.Evaluate("document.pdf").Should().BeTrue();
        condition.Evaluate("report.PDF").Should().BeTrue();
        condition.Evaluate("invoice.Pdf").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_EndsWithOperator_NoMatch()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "filename",
            Operator = ConditionOperator.EndsWith,
            ExpectedValue = ".pdf"
        };

        // Act & Assert
        condition.Evaluate("document.docx").Should().BeFalse();
        condition.Evaluate("image.jpg").Should().BeFalse();
        condition.Evaluate("archive.zip").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_EndsWithOperator_NullContextValue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "filename",
            Operator = ConditionOperator.EndsWith,
            ExpectedValue = ".pdf"
        };

        // Act
        var result = condition.Evaluate(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_GreaterThanOperator_NumericStrings()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "age",
            Operator = ConditionOperator.GreaterThan,
            ExpectedValue = "25"
        };

        // Act & Assert
        condition.Evaluate("26").Should().BeTrue();
        condition.Evaluate("30").Should().BeTrue();
        condition.Evaluate("100").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_GreaterThanOperator_DecimalValues()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "price",
            Operator = ConditionOperator.GreaterThan,
            ExpectedValue = "19.99"
        };

        // Act & Assert
        condition.Evaluate("20.00").Should().BeTrue();
        condition.Evaluate("25.50").Should().BeTrue();
        condition.Evaluate("100.99").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_GreaterThanOperator_NoMatch()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "age",
            Operator = ConditionOperator.GreaterThan,
            ExpectedValue = "25"
        };

        // Act & Assert
        condition.Evaluate("25").Should().BeFalse();
        condition.Evaluate("20").Should().BeFalse();
        condition.Evaluate("0").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_GreaterThanOperator_TypeMismatch_StringVsNumber()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "age",
            Operator = ConditionOperator.GreaterThan,
            ExpectedValue = "25"
        };

        // Act & Assert - When context value is not a valid number, should return false
        condition.Evaluate("old").Should().BeFalse();
        condition.Evaluate("25 years").Should().BeFalse();
        condition.Evaluate("").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_GreaterThanOperator_NullContextValue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "age",
            Operator = ConditionOperator.GreaterThan,
            ExpectedValue = "25"
        };

        // Act
        var result = condition.Evaluate(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_LessThanOperator_NumericStrings()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "age",
            Operator = ConditionOperator.LessThan,
            ExpectedValue = "25"
        };

        // Act & Assert
        condition.Evaluate("24").Should().BeTrue();
        condition.Evaluate("20").Should().BeTrue();
        condition.Evaluate("18").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_LessThanOperator_DecimalValues()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "price",
            Operator = ConditionOperator.LessThan,
            ExpectedValue = "20.00"
        };

        // Act & Assert
        condition.Evaluate("19.99").Should().BeTrue();
        condition.Evaluate("15.50").Should().BeTrue();
        condition.Evaluate("0.99").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_LessThanOperator_NoMatch()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "age",
            Operator = ConditionOperator.LessThan,
            ExpectedValue = "25"
        };

        // Act & Assert
        condition.Evaluate("25").Should().BeFalse();
        condition.Evaluate("30").Should().BeFalse();
        condition.Evaluate("100").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_LessThanOperator_TypeMismatch_StringVsNumber()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "age",
            Operator = ConditionOperator.LessThan,
            ExpectedValue = "25"
        };

        // Act & Assert - When context value is not a valid number, should return false
        condition.Evaluate("young").Should().BeFalse();
        condition.Evaluate("25 years").Should().BeFalse();
        condition.Evaluate("").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_LessThanOperator_NullContextValue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "age",
            Operator = ConditionOperator.LessThan,
            ExpectedValue = "25"
        };

        // Act
        var result = condition.Evaluate(null);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_InOperator_Match()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "tier",
            Operator = ConditionOperator.In,
            ExpectedValue = "premium,gold,platinum"
        };

        // Act & Assert
        condition.Evaluate("premium").Should().BeTrue();
        condition.Evaluate("gold").Should().BeTrue();
        condition.Evaluate("platinum").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_InOperator_CaseInsensitive()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "tier",
            Operator = ConditionOperator.In,
            ExpectedValue = "PREMIUM,GOLD,PLATINUM"
        };

        // Act & Assert
        condition.Evaluate("Premium").Should().BeTrue();
        condition.Evaluate("GOLD").Should().BeTrue();
        condition.Evaluate("platiNum").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_InOperator_NoMatch()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "tier",
            Operator = ConditionOperator.In,
            ExpectedValue = "premium,gold,platinum"
        };

        // Act & Assert
        condition.Evaluate("basic").Should().BeFalse();
        condition.Evaluate("silver").Should().BeFalse();
        condition.Evaluate("free").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_InOperator_EmptyList()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "tier",
            Operator = ConditionOperator.In,
            ExpectedValue = ""
        };

        // Act & Assert
        condition.Evaluate("anything").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_InOperator_NullContextValue()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "tier",
            Operator = ConditionOperator.In,
            ExpectedValue = "premium,gold,platinum"
        };

        // Act
        var result = condition.Evaluate(null);

        // Assert
        result.Should().BeFalse();
    }

}