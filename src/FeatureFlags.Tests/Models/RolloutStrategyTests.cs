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
/// Unit tests for the diagnostic <see cref="RolloutStrategy.ToString"/> format and its debugger display.
/// </summary>
public sealed class RolloutStrategyTests
{
    [Fact]
    public void ToString_PercentageRollout_IncludesFlagKeyAsSalt()
    {
        var strategy = new RolloutStrategy
        {
            Type = RolloutType.Percentage,
            StartPercentage = 30,
            FeatureFlag = new FeatureFlag { Key = "checkout-v2" }
        };

        strategy.ToString().Should().Be("30% of users by userId, salt=checkout-v2");
    }

    [Fact]
    public void ToString_PercentageRolloutWithoutFlag_OmitsSalt()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.Percentage, StartPercentage = 30 };

        strategy.ToString().Should().Be("30% of users by userId");
    }

    [Fact]
    public void ToString_PercentageRolloutWithoutStartPercentage_ReportsZero()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.Percentage };

        strategy.ToString().Should().Be("0% of users by userId");
    }

    [Fact]
    public void ToString_NonPercentageRollout_KeepsTypeAndPercentage()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.RulesBased, StartPercentage = 30 };

        strategy.ToString().Should().Be("RolloutStrategy { Type = RulesBased, Percentage = 30 }");
    }

    [Fact]
    public void ToString_NonPercentageRolloutWithoutPercentage_ReportsNull()
    {
        var strategy = new RolloutStrategy { Type = RolloutType.Full };

        strategy.ToString().Should().Be("RolloutStrategy { Type = Full, Percentage = null }");
    }

    [Fact]
    public void DebuggerDisplay_UsesToString()
    {
        var attribute = typeof(RolloutStrategy).GetCustomAttribute<DebuggerDisplayAttribute>();

        attribute.Should().NotBeNull();
        attribute!.Value.Should().Be("{ToString(),nq}");
    }
}
