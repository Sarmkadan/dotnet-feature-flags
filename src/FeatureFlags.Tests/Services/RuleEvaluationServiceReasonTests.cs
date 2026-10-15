#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using FeatureFlags.Repository;
using FeatureFlags.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FeatureFlags.Tests.Services;

/// <summary>
/// Unit tests for the reasons returned by <see cref="RuleEvaluationService.EvaluateWithReasonAsync"/>.
/// </summary>
public sealed class RuleEvaluationServiceReasonTests
{
    private readonly Mock<IFeatureFlagRepository> _repositoryMock = new();
    private readonly RuleEvaluationService _service;

    public RuleEvaluationServiceReasonTests()
    {
        _service = new RuleEvaluationService(_repositoryMock.Object, Mock.Of<ILogger<RuleEvaluationService>>());
    }

    private static Rule CountryRule(int id, int priority, string country, bool isActive = true) => new()
    {
        Id = id,
        Name = $"rule-{id}",
        Priority = priority,
        IsActive = isActive,
        ConditionLogic = "AND",
        Conditions = new List<Condition>
        {
            new() { AttributeName = "country", Operator = ConditionOperator.Equals, ExpectedValue = country, IsActive = true }
        }
    };

    private void SetupFlagWithRules(params Rule[] rules)
    {
        var flag = new FeatureFlag { Id = 1, Key = "checkout-v2", Rules = rules.ToList() };
        _repositoryMock.Setup(r => r.GetWithRulesAsync(1)).ReturnsAsync(flag);
    }

    private static readonly FeatureFlag Flag = new() { Id = 1, Key = "checkout-v2" };

    [Fact]
    public async Task EvaluateWithReasonAsync_RuleMatches_ReturnsMatchedRuleId()
    {
        SetupFlagWithRules(CountryRule(7, 10, "US"));
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com", Country = "US" };

        var result = await _service.EvaluateWithReasonAsync(Flag, userContext);

        result.Enabled.Should().BeTrue();
        result.Reason.Should().Be("matched rule 7");
        result.MatchedRuleId.Should().Be(7);
    }

    [Fact]
    public async Task EvaluateWithReasonAsync_MultipleMatches_ReportsHighestPriorityRule()
    {
        SetupFlagWithRules(CountryRule(3, 1, "US"), CountryRule(7, 10, "US"));
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com", Country = "US" };

        var result = await _service.EvaluateWithReasonAsync(Flag, userContext);

        result.Reason.Should().Be("matched rule 7");
    }

    [Fact]
    public async Task EvaluateWithReasonAsync_NoRuleMatches_ReturnsFallthrough()
    {
        SetupFlagWithRules(CountryRule(5, 10, "DE"));
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com", Country = "US" };

        var result = await _service.EvaluateWithReasonAsync(Flag, userContext);

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be("fallthrough: no rule matched");
        result.MatchedRuleId.Should().BeNull();
    }

    [Fact]
    public async Task EvaluateWithReasonAsync_OnlyInactiveRules_ReturnsFallthrough()
    {
        SetupFlagWithRules(CountryRule(5, 10, "US", isActive: false));
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com", Country = "US" };

        var result = await _service.EvaluateWithReasonAsync(Flag, userContext);

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be("fallthrough: no rule matched");
    }

    [Fact]
    public async Task EvaluateWithReasonAsync_NoRules_ReturnsFallthrough()
    {
        SetupFlagWithRules();
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com", Country = "US" };

        var result = await _service.EvaluateWithReasonAsync(Flag, userContext);

        result.Enabled.Should().BeFalse();
        result.Reason.Should().Be("fallthrough: no rule matched");
    }

    [Fact]
    public async Task EvaluateAsync_DelegatesToReasonEvaluation()
    {
        SetupFlagWithRules(CountryRule(7, 10, "US"));
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com", Country = "US" };

        var enabled = await _service.EvaluateAsync(Flag, userContext);

        enabled.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateWithReasonAsync_FlagMissing_ThrowsRuleEvaluationException()
    {
        _repositoryMock.Setup(r => r.GetWithRulesAsync(1)).ReturnsAsync((FeatureFlag?)null);
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com", Country = "US" };

        var act = () => _service.EvaluateWithReasonAsync(Flag, userContext);

        await act.Should().ThrowAsync<RuleEvaluationException>();
    }
}
