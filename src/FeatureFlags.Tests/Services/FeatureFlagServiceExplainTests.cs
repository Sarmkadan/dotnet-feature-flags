#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Configuration;
using FeatureFlags.Enums;
using FeatureFlags.Models;
using FeatureFlags.Repository;
using FeatureFlags.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace FeatureFlags.Tests.Services;

/// <summary>
/// Unit tests for <see cref="FeatureFlagService.ExplainAsync"/> covering the reason returned for each outcome.
/// </summary>
public sealed class FeatureFlagServiceExplainTests
{
    private readonly Mock<IFeatureFlagRepository> _featureFlagRepositoryMock = new();
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock = new();
    private readonly Mock<IRuleEvaluationService> _ruleEvaluationServiceMock = new();
    private readonly Mock<IPercentageRolloutService> _percentageRolloutServiceMock = new();
    private readonly FeatureFlagService _service;

    private static readonly UserContext User = new() { UserId = "user1", Email = "user@test.com", Country = "US" };

    public FeatureFlagServiceExplainTests()
    {
        _service = new FeatureFlagService(
            _featureFlagRepositoryMock.Object,
            _auditLogRepositoryMock.Object,
            _ruleEvaluationServiceMock.Object,
            _percentageRolloutServiceMock.Object,
            new FlagEvaluationLogService(Mock.Of<ILogger<FlagEvaluationLogService>>()),
            Options.Create(new FeatureFlagOptions()),
            Mock.Of<ILogger<FeatureFlagService>>());
    }

    [Fact]
    public async Task ExplainAsync_WithEmptyKey_ThrowsArgumentException()
    {
        var act = () => _service.ExplainAsync(" ", User);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ExplainAsync_FlagMissing_ReturnsFlagNotFound()
    {
        _featureFlagRepositoryMock.Setup(r => r.GetByKeyAsync("missing")).ReturnsAsync((FeatureFlag?)null);

        var result = await _service.ExplainAsync("missing", User);

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be("flag not found");
    }

    [Fact]
    public async Task ExplainAsync_FlagDisabled_ReturnsFlagDisabled()
    {
        _featureFlagRepositoryMock.Setup(r => r.GetByKeyAsync("off"))
            .ReturnsAsync(new FeatureFlag { Id = 2, Key = "off", IsEnabled = false, RolloutType = RolloutType.Full });

        var result = await _service.ExplainAsync("off", User);

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be("flag disabled");
    }

    [Fact]
    public async Task ExplainAsync_FullRollout_ReturnsFullRollout()
    {
        _featureFlagRepositoryMock.Setup(r => r.GetByKeyAsync("full"))
            .ReturnsAsync(new FeatureFlag { Id = 3, Key = "full", IsEnabled = true, RolloutType = RolloutType.Full });

        var result = await _service.ExplainAsync("full", User);

        result.Enabled.Should().BeTrue();
        result.Reason.Should().Be("full rollout");
    }

    [Fact]
    public async Task ExplainAsync_NoneRollout_ReturnsRolloutNone()
    {
        _featureFlagRepositoryMock.Setup(r => r.GetByKeyAsync("none"))
            .ReturnsAsync(new FeatureFlag { Id = 4, Key = "none", IsEnabled = true, RolloutType = RolloutType.None });

        var result = await _service.ExplainAsync("none", User);

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be("rollout type none");
    }

    [Fact]
    public async Task ExplainAsync_PercentageAt100_EnablesEveryUserWithBucketReason()
    {
        _featureFlagRepositoryMock.Setup(r => r.GetByKeyAsync("pct"))
            .ReturnsAsync(new FeatureFlag { Id = 5, Key = "pct", IsEnabled = true, RolloutType = RolloutType.Percentage, PercentageRollout = 100 });
        var bucket = User.GetConsistentHash("pct");

        var result = await _service.ExplainAsync("pct", User);

        result.Enabled.Should().BeTrue();
        result.Reason.Should().Be($"rollout bucket {bucket} < 100%");
        result.Bucket.Should().Be(bucket);
        result.Percentage.Should().Be(100);
    }

    [Fact]
    public async Task ExplainAsync_PercentageAt0_DisablesEveryUserWithBucketReason()
    {
        _featureFlagRepositoryMock.Setup(r => r.GetByKeyAsync("pct0"))
            .ReturnsAsync(new FeatureFlag { Id = 6, Key = "pct0", IsEnabled = true, RolloutType = RolloutType.Percentage, PercentageRollout = 0 });
        var bucket = User.GetConsistentHash("pct0");

        var result = await _service.ExplainAsync("pct0", User);

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be($"rollout bucket {bucket} >= 0%");
    }

    [Fact]
    public async Task ExplainAsync_RulesBased_ReturnsRuleReasonFromRuleService()
    {
        var flag = new FeatureFlag { Id = 7, Key = "rules", IsEnabled = true, RolloutType = RolloutType.RulesBased };
        _featureFlagRepositoryMock.Setup(r => r.GetByKeyAsync("rules")).ReturnsAsync(flag);
        _ruleEvaluationServiceMock
            .Setup(s => s.EvaluateWithReasonAsync(flag, User, It.IsAny<CancellationToken>()))
            .ReturnsAsync(EvaluationResult.RuleMatched(9));

        var result = await _service.ExplainAsync("rules", User);

        result.Enabled.Should().BeTrue();
        result.Reason.Should().Be("matched rule 9");
        result.MatchedRuleId.Should().Be(9);
    }

    [Fact]
    public async Task ExplainAsync_DoesNotWriteAuditLog()
    {
        _featureFlagRepositoryMock.Setup(r => r.GetByKeyAsync("full"))
            .ReturnsAsync(new FeatureFlag { Id = 3, Key = "full", IsEnabled = true, RolloutType = RolloutType.Full });

        await _service.ExplainAsync("full", User);

        _auditLogRepositoryMock.VerifyNoOtherCalls();
    }
}
