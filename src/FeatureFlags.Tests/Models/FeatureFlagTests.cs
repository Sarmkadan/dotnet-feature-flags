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
/// Unit tests for the diagnostic <see cref="FeatureFlag.ToString"/> format and its debugger display.
/// </summary>
public sealed class FeatureFlagTests
{
    [Fact]
    public void ToString_ReturnsKeyAndEnabledState()
    {
        var flag = new FeatureFlag { Key = "checkout-v2", IsEnabled = true };

        flag.ToString().Should().Be("FeatureFlag { Key = checkout-v2, IsEnabled = True }");
    }

    [Fact]
    public void DebuggerDisplay_ShowsKeyEnabledStateAndRollout()
    {
        var attribute = typeof(FeatureFlag).GetCustomAttribute<DebuggerDisplayAttribute>();

        attribute.Should().NotBeNull();
        attribute!.Value.Should().Be("{Key,nq} (enabled={IsEnabled}, rollout={RolloutType})");
    }
}
