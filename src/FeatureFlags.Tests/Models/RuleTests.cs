#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Reflection;
using FeatureFlags.Enums;
using FeatureFlags.Models;
using FluentAssertions;
using Xunit;

namespace FeatureFlags.Tests.Models;

/// <summary>
/// Unit tests for the diagnostic <see cref="Rule.ToString"/> format and its debugger display.
/// </summary>
public sealed class RuleTests
{
    private static Condition Cond(string attribute, ConditionOperator op, string value, bool isActive = true) => new()
    {
        AttributeName = attribute,
        Operator = op,
        ExpectedValue = value,
        IsActive = isActive
    };

    [Fact]
    public void ToString_InOperator_ListsValuesInBrackets()
    {
        var rule = new Rule { ConditionLogic = "AND", Conditions = new List<Condition> { Cond("country", ConditionOperator.In, "DE,AT") } };

        rule.ToString().Should().Be("country IN [DE, AT]");
    }

    [Fact]
    public void ToString_InOperator_TrimsWhitespaceAroundValues()
    {
        var rule = new Rule { Conditions = new List<Condition> { Cond("country", ConditionOperator.In, " DE ,  AT ") } };

        rule.ToString().Should().Be("country IN [DE, AT]");
    }

    [Fact]
    public void ToString_AndLogic_JoinsConditionsWithAnd()
    {
        var rule = new Rule
        {
            ConditionLogic = "AND",
            Conditions = new List<Condition>
            {
                Cond("country", ConditionOperator.Equals, "US"),
                Cond("plan", ConditionOperator.Equals, "pro")
            }
        };

        rule.ToString().Should().Be("country = US AND plan = pro");
    }

    [Fact]
    public void ToString_OrLogic_JoinsConditionsWithOr()
    {
        var rule = new Rule
        {
            ConditionLogic = "OR",
            Conditions = new List<Condition>
            {
                Cond("country", ConditionOperator.Equals, "US"),
                Cond("plan", ConditionOperator.Equals, "pro")
            }
        };

        rule.ToString().Should().Be("country = US OR plan = pro");
    }

    [Fact]
    public void ToString_SkipsInactiveConditions()
    {
        var rule = new Rule
        {
            ConditionLogic = "AND",
            Conditions = new List<Condition>
            {
                Cond("country", ConditionOperator.Equals, "US"),
                Cond("plan", ConditionOperator.Equals, "pro", isActive: false)
            }
        };

        rule.ToString().Should().Be("country = US");
    }

    [Fact]
    public void ToString_WithNoActiveConditions_ReturnsPlaceholder()
    {
        var rule = new Rule { Conditions = new List<Condition> { Cond("country", ConditionOperator.Equals, "US", isActive: false) } };

        rule.ToString().Should().Be("(no active conditions)");
    }

    [Theory]
    [InlineData(ConditionOperator.Equals, "country = US")]
    [InlineData(ConditionOperator.NotEquals, "country != US")]
    [InlineData(ConditionOperator.Contains, "country CONTAINS US")]
    [InlineData(ConditionOperator.StartsWith, "country STARTS WITH US")]
    [InlineData(ConditionOperator.EndsWith, "country ENDS WITH US")]
    [InlineData(ConditionOperator.GreaterThan, "country > US")]
    [InlineData(ConditionOperator.LessThan, "country < US")]
    public void ToString_FormatsEachOperator(ConditionOperator op, string expected)
    {
        var rule = new Rule { Conditions = new List<Condition> { Cond("country", op, "US") } };

        rule.ToString().Should().Be(expected);
    }

    [Fact]
    public void DebuggerDisplay_UsesToString()
    {
        var attribute = typeof(Rule).GetCustomAttribute<DebuggerDisplayAttribute>();

        attribute.Should().NotBeNull();
        attribute!.Value.Should().Be("{ToString(),nq}");
    }
}
