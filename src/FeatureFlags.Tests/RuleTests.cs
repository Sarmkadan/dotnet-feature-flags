#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Models;
using FluentAssertions;
using Xunit;

namespace FeatureFlags.Tests;

public class RuleTests
{
    private static Condition Cond(string attribute, bool isActive = true) => new()
    {
        AttributeName = attribute,
        Operator = ConditionOperator.Equals,
        ExpectedValue = "x",
        IsActive = isActive
    };

    private static Rule ValidRule() => new()
    {
        Name = "beta-users",
        ConditionLogic = "AND",
        Priority = 0,
        Conditions = new List<Condition> { Cond("plan") }
    };

    [Fact]
    public void Defaults_NewRule_HasExpectedInitialValues()
    {
        var rule = new Rule();

        rule.Id.Should().Be(0);
        rule.Name.Should().BeEmpty();
        rule.Description.Should().BeEmpty();
        rule.IsActive.Should().BeTrue();
        rule.ConditionLogic.Should().Be("AND");
        rule.Conditions.Should().BeEmpty();
        rule.FeatureFlag.Should().BeNull();
    }

    [Fact]
    public void Properties_AssignedValues_AreReturned()
    {
        var created = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var flag = new FeatureFlag { Id = 7 };

        var rule = new Rule
        {
            Id = 42,
            FeatureFlagId = 7,
            Name = "eu-traffic",
            Description = "EU traffic only",
            Priority = 5,
            IsActive = false,
            ConditionLogic = "OR",
            CreatedAt = created,
            UpdatedAt = created.AddDays(1),
            FeatureFlag = flag
        };

        rule.Id.Should().Be(42);
        rule.FeatureFlagId.Should().Be(7);
        rule.Name.Should().Be("eu-traffic");
        rule.Description.Should().Be("EU traffic only");
        rule.Priority.Should().Be(5);
        rule.IsActive.Should().BeFalse();
        rule.ConditionLogic.Should().Be("OR");
        rule.CreatedAt.Should().Be(created);
        rule.UpdatedAt.Should().Be(created.AddDays(1));
        rule.FeatureFlag.Should().BeSameAs(flag);
    }

    [Theory]
    [InlineData("AND")]
    [InlineData("OR")]
    public void IsValid_NamedRuleWithConditionsAndSupportedLogic_ReturnsTrue(string logic)
    {
        var rule = ValidRule();
        rule.ConditionLogic = logic;

        rule.IsValid().Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValid_BlankName_ReturnsFalse(string name)
    {
        var rule = ValidRule();
        rule.Name = name;

        rule.IsValid().Should().BeFalse();
    }

    [Fact]
    public void IsValid_NoConditionsOrNullConditions_ReturnsFalse()
    {
        var rule = ValidRule();

        rule.Conditions = new List<Condition>();
        rule.IsValid().Should().BeFalse();

        rule.Conditions = null!;
        rule.IsValid().Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("and")] // comparison is case-sensitive
    [InlineData("XOR")]
    public void IsValid_UnsupportedConditionLogic_ReturnsFalse(string logic)
    {
        var rule = ValidRule();
        rule.ConditionLogic = logic;

        rule.IsValid().Should().BeFalse();
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(int.MaxValue, true)]
    public void Priority_Boundaries_ControlValidityAndEvaluationOrder(int priority, bool expectedValid)
    {
        var rule = ValidRule();
        rule.Priority = priority;

        rule.IsValid().Should().Be(expectedValid);
        rule.GetEvaluationPriority().Should().Be(priority);
    }

    [Fact]
    public void GetActiveConditionCount_CountsOnlyActiveConditions()
    {
        var rule = ValidRule();
        rule.Conditions = new List<Condition> { Cond("a"), Cond("b", isActive: false), Cond("c") };
        rule.GetActiveConditionCount().Should().Be(2);

        rule.Conditions = new List<Condition>();
        rule.GetActiveConditionCount().Should().Be(0);

        rule.Conditions = null!;
        rule.GetActiveConditionCount().Should().Be(0);
    }
}
