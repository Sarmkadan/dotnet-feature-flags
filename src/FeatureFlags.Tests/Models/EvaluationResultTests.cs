#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using FluentAssertions;
using Xunit;

namespace FeatureFlags.Tests.Models;

/// <summary>
/// Unit tests for <see cref="EvaluationResult"/> factories and their exact <see cref="EvaluationResult.Reason"/> strings.
/// </summary>
public sealed class EvaluationResultTests
{
    [Fact]
    public void FlagNotFound_IsDisabledWithReason()
    {
        var result = EvaluationResult.FlagNotFound();

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be("flag not found");
    }

    [Fact]
    public void FlagDisabled_IsDisabledWithReason()
    {
        var result = EvaluationResult.FlagDisabled();

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be("flag disabled");
    }

    [Fact]
    public void RolloutNone_IsDisabledWithReason()
    {
        var result = EvaluationResult.RolloutNone();

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be("rollout type none");
    }

    [Fact]
    public void FullRollout_IsEnabledWithReason()
    {
        var result = EvaluationResult.FullRollout();

        result.Enabled.Should().BeTrue();
        result.Reason.Should().Be("full rollout");
    }

    [Fact]
    public void RuleMatched_IsEnabledAndCarriesRuleId()
    {
        var result = EvaluationResult.RuleMatched(42);

        result.Enabled.Should().BeTrue();
        result.Reason.Should().Be("matched rule 42");
        result.MatchedRuleId.Should().Be(42);
        result.Bucket.Should().BeNull();
        result.Percentage.Should().BeNull();
    }

    [Fact]
    public void Fallthrough_IsDisabledWithoutRuleId()
    {
        var result = EvaluationResult.Fallthrough();

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be("fallthrough: no rule matched");
        result.MatchedRuleId.Should().BeNull();
    }

    [Fact]
    public void RolloutBucket_BelowPercentage_IsEnabled()
    {
        var result = EvaluationResult.RolloutBucket(12, 30);

        result.Enabled.Should().BeTrue();
        result.Reason.Should().Be("rollout bucket 12 < 30%");
        result.Bucket.Should().Be(12);
        result.Percentage.Should().Be(30);
    }

    [Fact]
    public void RolloutBucket_AtPercentage_IsDisabled()
    {
        // Boundary: bucket 30 is outside a 30% rollout (buckets 0 to 29 are enabled).
        var result = EvaluationResult.RolloutBucket(30, 30);

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be("rollout bucket 30 >= 30%");
    }

    [Fact]
    public void RolloutBucket_AboveBucketWithPercentage_IsDisabled()
    {
        var result = EvaluationResult.RolloutBucket(47, 30);

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be("rollout bucket 47 >= 30%");
    }

    [Fact]
    public void ToString_ReturnsReason()
    {
        EvaluationResult.RuleMatched(7).ToString().Should().Be("matched rule 7");
    }
}
