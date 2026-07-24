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
/// Unit tests for RolloutStrategyExtensions extension methods.
/// Tests type checking, percentage calculations, and description generation.
/// </summary>
public sealed class RolloutStrategyExtensionsTests
{
    [Fact]
    public void IsPercentageBased_PercentageType_ReturnsTrue()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.Percentage };
        strategy.IsPercentageBased().Should().BeTrue();
    }

    [Fact]
    public void IsPercentageBased_NonPercentageType_ReturnsFalse()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.RulesBased };
        strategy.IsPercentageBased().Should().BeFalse();
    }

    [Fact]
    public void IsRulesBased_RulesBasedType_ReturnsTrue()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.RulesBased };
        strategy.IsRulesBased().Should().BeTrue();
    }

    [Fact]
    public void IsRulesBased_NonRulesBasedType_ReturnsFalse()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.Percentage };
        strategy.IsRulesBased().Should().BeFalse();
    }

    [Fact]
    public void IsABTest_ABTestType_ReturnsTrue()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.ABTest };
        strategy.IsABTest().Should().BeTrue();
    }

    [Fact]
    public void IsABTest_NonABTestType_ReturnsFalse()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.Full };
        strategy.IsABTest().Should().BeFalse();
    }

    [Fact]
    public void IsFullRollout_FullType_ReturnsTrue()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.Full };
        strategy.IsFullRollout().Should().BeTrue();
    }

    [Fact]
    public void IsFullRollout_NonFullType_ReturnsFalse()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.None };
        strategy.IsFullRollout().Should().BeFalse();
    }

    [Fact]
    public void IsNoRollout_NoneType_ReturnsTrue()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.None };
        strategy.IsNoRollout().Should().BeTrue();
    }

    [Fact]
    public void IsNoRollout_NonNoneType_ReturnsFalse()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.Full };
        strategy.IsNoRollout().Should().BeFalse();
    }

    [Fact]
    public void GetEffectivePercentage_PercentageType_ReturnsCurrentPercentage()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Percentage,
            StartPercentage = 50,
            EndPercentage = 50,
            IsGradual = false
        };
        strategy.GetEffectivePercentage().Should().Be(50);
    }

    [Fact]
    public void GetEffectivePercentage_ABTestWithEndPercentage_ReturnsEndPercentage()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.ABTest,
            StartPercentage = 30,
            EndPercentage = 70,
            IsGradual = false
        };
        strategy.GetEffectivePercentage().Should().Be(70);
    }

    [Fact]
    public void GetEffectivePercentage_ABTestWithoutEndPercentage_ReturnsCurrentPercentage()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.ABTest,
            StartPercentage = 45,
            EndPercentage = null,
            IsGradual = false
        };
        strategy.GetEffectivePercentage().Should().Be(45);
    }

    [Fact]
    public void GetEffectivePercentage_FullRolloutActive_Returns100()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Full,
            StartPercentage = 100,
            EndPercentage = 100,
            IsGradual = false
        };
        strategy.GetEffectivePercentage().Should().Be(100);
    }

    [Fact]
    public void GetEffectivePercentage_FullRolloutInactive_Returns0()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Full,
            StartPercentage = 100,
            EndPercentage = 100,
            IsGradual = false
        };
        strategy.StartDate = DateTime.UtcNow.AddDays(-10);
        strategy.EndDate = DateTime.UtcNow.AddDays(-5);
        strategy.GetEffectivePercentage().Should().Be(0);
    }

    [Fact]
    public void GetEffectivePercentage_RulesBasedActive_Returns100()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.RulesBased,
            StartPercentage = 0,
            EndPercentage = 100,
            IsGradual = false,
            FeatureFlagId = 1
        };
        strategy.GetEffectivePercentage().Should().Be(100);
    }

    [Fact]
    public void GetEffectivePercentage_NoRollout_Returns0()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.None,
            StartPercentage = 0,
            EndPercentage = 0,
            IsGradual = false
        };
        strategy.GetEffectivePercentage().Should().Be(0);
    }

    [Fact]
    public void GetProgressPercentage_GradualRolloutInProgress_ReturnsProgress()
    {
        var startDate = DateTime.UtcNow.AddDays(-5);
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Percentage,
            StartPercentage = 0,
            EndPercentage = 100,
            IsGradual = true,
            StartDate = startDate,
            DailyIncrement = 10
        };
        var result = strategy.GetProgressPercentage();
        result.Should().BeGreaterThan(0);
        result.Should().BeLessThanOrEqualTo(100);
    }

    [Fact]
    public void GetProgressPercentage_NonGradualRollout_Returns0()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Percentage,
            StartPercentage = 50,
            EndPercentage = 50,
            IsGradual = false
        };
        strategy.GetProgressPercentage().Should().Be(0);
    }

    [Fact]
    public void GetProgressPercentage_GradualRolloutNotStarted_Returns0()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Percentage,
            StartPercentage = 0,
            EndPercentage = 100,
            IsGradual = true,
            StartDate = DateTime.UtcNow.AddDays(1),
            DailyIncrement = 10
        };
        strategy.GetProgressPercentage().Should().Be(0);
    }

    [Fact]
    public void GetProgressPercentage_GradualRolloutCompleted_Returns100()
    {
        var startDate = DateTime.UtcNow.AddDays(-20);
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Percentage,
            StartPercentage = 0,
            EndPercentage = 100,
            IsGradual = true,
            StartDate = startDate,
            DailyIncrement = 10
        };
        strategy.GetProgressPercentage().Should().Be(100);
    }

    [Fact]
    public void GetProgressPercentage_StartEqualsEnd_Returns100WhenReached()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Percentage,
            StartPercentage = 50,
            EndPercentage = 50,
            IsGradual = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            DailyIncrement = 0
        };
        strategy.GetProgressPercentage().Should().Be(100);
    }

    [Fact]
    public void HasReachedTarget_ActiveStrategyBelowTarget_ReturnsFalse()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Percentage,
            StartPercentage = 0,
            EndPercentage = 100,
            IsGradual = false,
            FeatureFlagId = 1
        };
        strategy.HasReachedTarget().Should().BeFalse();
    }

    [Fact]
    public void HasReachedTarget_ActiveStrategyAtTarget_ReturnsTrue()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Percentage,
            StartPercentage = 100,
            EndPercentage = 100,
            IsGradual = false,
            FeatureFlagId = 1
        };
        strategy.HasReachedTarget().Should().BeTrue();
    }

    [Fact]
    public void HasReachedTarget_ActiveStrategyReachedTarget_ReturnsTrue()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Percentage,
            StartPercentage = 100,
            EndPercentage = 100,
            IsGradual = false,
            FeatureFlagId = 1
        };
        strategy.HasReachedTarget().Should().BeTrue();
    }

    [Fact]
    public void HasReachedTarget_InactiveStrategy_ReturnsFalse()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Full,
            StartPercentage = 100,
            EndPercentage = 100,
            IsGradual = false
        };
        strategy.StartDate = DateTime.UtcNow.AddDays(-10);
        strategy.EndDate = DateTime.UtcNow.AddDays(-5);
        strategy.HasReachedTarget().Should().BeFalse();
    }

    [Fact]
    public void HasReachedTarget_InvalidStrategy_ReturnsFalse()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Percentage,
            StartPercentage = 100,
            EndPercentage = 100,
            IsGradual = false,
            FeatureFlagId = -1
        };
        strategy.HasReachedTarget().Should().BeFalse();
    }

    [Fact]
    public void GetDescription_PercentageType_ReturnsCorrectDescription()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.Percentage };
        strategy.GetDescription().Should().Be("Percentage-based rollout using consistent hashing");
    }

    [Fact]
    public void GetDescription_RulesBasedType_ReturnsCorrectDescription()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.RulesBased };
        strategy.GetDescription().Should().Be("Rules-based rollout with targeting conditions");
    }

    [Fact]
    public void GetDescription_ABTestType_ReturnsCorrectDescription()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.ABTest };
        strategy.GetDescription().Should().Be("A/B test with multiple variants");
    }

    [Fact]
    public void GetDescription_FullType_ReturnsCorrectDescription()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.Full };
        strategy.GetDescription().Should().Be("Full rollout to all users (100%)");
    }

    [Fact]
    public void GetDescription_NoneType_ReturnsCorrectDescription()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.None };
        strategy.GetDescription().Should().Be("No rollout (0%)");
    }

    [Fact]
    public void GetDescription_UnknownType_ReturnsFallback()
    {
        var strategy = new RolloutStrategy { Type = (RolloutType)999 };
        strategy.GetDescription().Should().StartWith("Unknown rollout type:");
    }

    [Fact]
    public void IsPercentageBased_WithNullStrategy_ThrowsArgumentNullException()
    {
        RolloutStrategy? strategy = null;
        Assert.Throws<ArgumentNullException>(() => strategy!.IsPercentageBased());
    }

    [Fact]
    public void IsRulesBased_WithNullStrategy_ThrowsArgumentNullException()
    {
        RolloutStrategy? strategy = null;
        Assert.Throws<ArgumentNullException>(() => strategy!.IsRulesBased());
    }

    [Fact]
    public void IsABTest_WithNullStrategy_ThrowsArgumentNullException()
    {
        RolloutStrategy? strategy = null;
        Assert.Throws<ArgumentNullException>(() => strategy!.IsABTest());
    }

    [Fact]
    public void IsFullRollout_WithNullStrategy_ThrowsArgumentNullException()
    {
        RolloutStrategy? strategy = null;
        Assert.Throws<ArgumentNullException>(() => strategy!.IsFullRollout());
    }

    [Fact]
    public void IsNoRollout_WithNullStrategy_ThrowsArgumentNullException()
    {
        RolloutStrategy? strategy = null;
        Assert.Throws<ArgumentNullException>(() => strategy!.IsNoRollout());
    }

    [Fact]
    public void GetEffectivePercentage_WithNullStrategy_ThrowsArgumentNullException()
    {
        RolloutStrategy? strategy = null;
        Assert.Throws<ArgumentNullException>(() => strategy!.GetEffectivePercentage());
    }

    [Fact]
    public void GetProgressPercentage_WithNullStrategy_ThrowsArgumentNullException()
    {
        RolloutStrategy? strategy = null;
        Assert.Throws<ArgumentNullException>(() => strategy!.GetProgressPercentage());
    }

    [Fact]
    public void HasReachedTarget_WithNullStrategy_ThrowsArgumentNullException()
    {
        RolloutStrategy? strategy = null;
        Assert.Throws<ArgumentNullException>(() => strategy!.HasReachedTarget());
    }

    [Fact]
    public void GetDescription_WithNullStrategy_ThrowsArgumentNullException()
    {
        RolloutStrategy? strategy = null;
        Assert.Throws<ArgumentNullException>(() => strategy!.GetDescription());
    }
}