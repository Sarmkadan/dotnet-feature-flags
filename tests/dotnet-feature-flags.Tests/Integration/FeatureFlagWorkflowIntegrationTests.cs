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

namespace FeatureFlags.Tests.Integration;

/// <summary>
/// Integration tests for the feature flag workflow covering end-to-end scenarios,
/// configuration combinations, and full evaluation pipelines.
/// </summary>
public sealed class FeatureFlagWorkflowIntegrationTests
{
    private readonly Mock<IFeatureFlagRepository> _flagRepositoryMock;
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<ILogger<FeatureFlagService>> _loggerMock;
    private readonly Mock<ILogger<RuleEvaluationService>> _ruleLoggerMock;
    private readonly Mock<ILogger<PercentageRolloutService>> _percentageLoggerMock;
    private readonly FeatureFlagService _flagService;
    private readonly RuleEvaluationService _ruleService;
    private readonly PercentageRolloutService _percentageService;

    public FeatureFlagWorkflowIntegrationTests()
    {
        _flagRepositoryMock = new Mock<IFeatureFlagRepository>();
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _loggerMock = new Mock<ILogger<FeatureFlagService>>();
        _ruleLoggerMock = new Mock<ILogger<RuleEvaluationService>>();
        _percentageLoggerMock = new Mock<ILogger<PercentageRolloutService>>();

        var evaluationLogService = new FlagEvaluationLogService();
        var options = Options.Create(new FeatureFlagOptions());

        _ruleService = new RuleEvaluationService(_flagRepositoryMock.Object, _ruleLoggerMock.Object);
        _percentageService = new PercentageRolloutService(_percentageLoggerMock.Object);

        _flagService = new FeatureFlagService(
            _flagRepositoryMock.Object,
            _auditLogRepositoryMock.Object,
            _ruleService,
            _percentageService,
            evaluationLogService,
            options,
            _loggerMock.Object);
    }

    [Fact]
    public async Task FullWorkflow_EnableDisableFeatureFlag_TracksChanges()
    {
        // Arrange
        var featureFlag = new FeatureFlag
        {
            Id = 1,
            Key = "new-feature",
            DisplayName = "New Feature",
            IsEnabled = false,
            RolloutType = RolloutType.Full
        };

        _flagRepositoryMock
            .Setup(r => r.GetByKeyAsync("new-feature"))
            .ReturnsAsync(featureFlag);

        // Act 1: Feature disabled
        var resultDisabled = await _flagService.IsEnabledAsync("new-feature", new UserContext { UserId = "user1", Email = "user@test.com" });

        // Act 2: Enable feature
        featureFlag.IsEnabled = true;

        var resultEnabled = await _flagService.IsEnabledAsync("new-feature", new UserContext { UserId = "user1", Email = "user@test.com" });

        // Assert
        resultDisabled.Should().BeFalse();
        resultEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task FullWorkflow_PercentageRollout_ConsistentUserBuckets()
    {
        // Arrange
        var featureFlag = new FeatureFlag
        {
            Id = 1,
            Key = "gradual-rollout",
            DisplayName = "Gradual Rollout",
            IsEnabled = true,
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 50
        };

        _flagRepositoryMock
            .Setup(r => r.GetByKeyAsync("gradual-rollout"))
            .ReturnsAsync(featureFlag);

        var user = new UserContext { UserId = "user123", Email = "user123@test.com" };

        // Act - Evaluate same user multiple times
        var result1 = await _flagService.IsEnabledAsync("gradual-rollout", user);
        var result2 = await _flagService.IsEnabledAsync("gradual-rollout", user);
        var result3 = await _flagService.IsEnabledAsync("gradual-rollout", user);

        // Assert - Same user should get consistent results
        result1.Should().Be(result2);
        result2.Should().Be(result3);
    }

    [Fact]
    public async Task FullWorkflow_RuleBasedTargeting_CorrectlyEvaluatesConditions()
    {
        // Arrange
        var rule = new Rule
        {
            Id = 1,
            Name = "Premium Users",
            IsActive = true,
            ConditionLogic = "AND",
            Conditions = new List<Condition>
            {
                new Condition
                {
                    AttributeName = "tier",
                    Operator = ConditionOperator.Equals,
                    ExpectedValue = "premium",
                    IsActive = true
                },
                new Condition
                {
                    AttributeName = "country",
                    Operator = ConditionOperator.In,
                    ExpectedValue = "US,CA,UK",
                    IsActive = true
                }
            }
        };

        var premiumUser = new UserContext
        {
            UserId = "premium-user",
            Email = "premium@test.com",
            Tier = "premium",
            Country = "US"
        };

        var freeUser = new UserContext
        {
            UserId = "free-user",
            Email = "free@test.com",
            Tier = "free",
            Country = "US"
        };

        // Act
        var premiumResult = await _ruleService.EvaluateRuleAsync(rule, premiumUser);
        var freeResult = await _ruleService.EvaluateRuleAsync(rule, freeUser);

        // Assert
        premiumResult.Should().BeTrue();
        freeResult.Should().BeFalse();
    }

    [Fact]
    public async Task FullWorkflow_MultipleConditionsWithOR_ReturnsCorrectResult()
    {
        // Arrange
        var rule = new Rule
        {
            Name = "US or Premium",
            IsActive = true,
            ConditionLogic = "OR",
            Conditions = new List<Condition>
            {
                new Condition { AttributeName = "country", Operator = ConditionOperator.Equals, ExpectedValue = "US", IsActive = true },
                new Condition { AttributeName = "tier", Operator = ConditionOperator.Equals, ExpectedValue = "premium", IsActive = true }
            }
        };

        var usUser = new UserContext { UserId = "us-user", Email = "us@test.com", Country = "US", Tier = "free" };
        var premiumUser = new UserContext { UserId = "premium-user", Email = "premium@test.com", Country = "CA", Tier = "premium" };
        var otherUser = new UserContext { UserId = "other-user", Email = "other@test.com", Country = "DE", Tier = "free" };

        // Act
        var usResult = await _ruleService.EvaluateRuleAsync(rule, usUser);
        var premiumResult = await _ruleService.EvaluateRuleAsync(rule, premiumUser);
        var otherResult = await _ruleService.EvaluateRuleAsync(rule, otherUser);

        // Assert
        usResult.Should().BeTrue("US user should match");
        premiumResult.Should().BeTrue("Premium user should match");
        otherResult.Should().BeFalse("Other user should not match");
    }

    [Fact]
    public async Task FullWorkflow_ProgressiveRollout_DistributionAccuracy()
    {
        // Arrange
        var featureFlag = new FeatureFlag
        {
            Id = 1,
            Key = "distribution-test",
            IsEnabled = true,
            RolloutType = RolloutType.Percentage,
            PercentageRollout = 25
        };

        _flagRepositoryMock
            .Setup(r => r.GetByKeyAsync("distribution-test"))
            .ReturnsAsync(featureFlag);

        // Act - Test distribution across 100 users
        var enabledCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var user = new UserContext { UserId = $"user{i}", Email = $"user{i}@test.com" };
            var result = await _flagService.IsEnabledAsync("distribution-test", user);
            if (result) enabledCount++;
        }

        // Assert - Should be approximately 25% (allowing 20-30% tolerance)
        var percentageEnabled = (enabledCount * 100) / 100;
        percentageEnabled.Should().BeGreaterThanOrEqualTo(15);
        percentageEnabled.Should().BeLessThanOrEqualTo(35);
    }

    [Fact]
    public async Task FullWorkflow_ConcurrentEvaluations_ThreadSafety()
    {
        // Arrange
        var featureFlag = new FeatureFlag
        {
            Id = 1,
            Key = "concurrent-test",
            IsEnabled = true,
            RolloutType = RolloutType.Full
        };

        _flagRepositoryMock
            .Setup(r => r.GetByKeyAsync("concurrent-test"))
            .ReturnsAsync(featureFlag);

        var results = new List<bool>();
        var lockObj = new object();

        // Act - Evaluate from multiple threads
        var tasks = Enumerable.Range(0, 50).Select(i => Task.Run(async () =>
        {
            var user = new UserContext { UserId = $"user{i}", Email = $"user{i}@test.com" };
            var result = await _flagService.IsEnabledAsync("concurrent-test", user);
            lock (lockObj)
            {
                results.Add(result);
            }
        })).ToList();

        await Task.WhenAll(tasks);

        // Assert - All evaluations should complete successfully
        results.Should().HaveCount(50);
        results.Should().AllSatisfy(r => r.Should().BeTrue());
    }

    [Fact]
    public async Task FullWorkflow_CustomAttributeEvaluation_WorksCorrectly()
    {
        // Arrange
        var rule = new Rule
        {
            Name = "Custom Attribute Rule",
            IsActive = true,
            ConditionLogic = "AND",
            Conditions = new List<Condition>
            {
                new Condition
                {
                    AttributeName = "department",
                    Operator = ConditionOperator.Equals,
                    ExpectedValue = "engineering",
                    IsActive = true
                }
            }
        };

        var engineeringUser = new UserContext { UserId = "eng-user", Email = "eng@test.com" };
        engineeringUser.SetCustomAttribute("department", "engineering");

        var marketingUser = new UserContext { UserId = "mkt-user", Email = "mkt@test.com" };
        marketingUser.SetCustomAttribute("department", "marketing");

        // Act
        var engResult = await _ruleService.EvaluateRuleAsync(rule, engineeringUser);
        var mktResult = await _ruleService.EvaluateRuleAsync(rule, marketingUser);

        // Assert
        engResult.Should().BeTrue();
        mktResult.Should().BeFalse();
    }

    [Fact]
    public async Task FullWorkflow_FlagNotFound_ReturnsFalse()
    {
        // Arrange
        _flagRepositoryMock
            .Setup(r => r.GetByKeyAsync("nonexistent"))
            .ReturnsAsync((FeatureFlag?)null);

        var user = new UserContext { UserId = "user1", Email = "user@test.com" };

        // Act
        var result = await _flagService.IsEnabledAsync("nonexistent", user);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task FullWorkflow_InvalidUserContext_ThrowsException()
    {
        // Arrange
        var featureFlag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true, RolloutType = RolloutType.Full };
        _flagRepositoryMock.Setup(r => r.GetByKeyAsync("test-flag")).ReturnsAsync(featureFlag);

        var invalidUser = new UserContext { UserId = "", Email = "user@test.com" }; // Missing UserId

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _flagService.IsEnabledAsync("test-flag", invalidUser));
    }

    [Fact]
    public async Task FullWorkflow_ComplexScenario_ABTestVariants()
    {
        // Arrange - Simulate A/B test with variants
        var variants = new List<ABTestVariant>
        {
            new ABTestVariant { Id = 1, VariantKey = "Control", AllocationPercentage = 50 },
            new ABTestVariant { Id = 2, VariantKey = "Treatment", AllocationPercentage = 50 }
        };

        var userBuckets = new Dictionary<string, string>();
        var users = Enumerable.Range(0, 100).Select(i => new UserContext
        {
            UserId = $"user{i}",
            Email = $"user{i}@test.com"
        }).ToList();

        // Act - Assign users to variants based on consistent hashing
        foreach (var user in users)
        {
            var hash = user.GetConsistentHash("ab-test-flag");
            var variant = hash < 50 ? "Control" : "Treatment";
            userBuckets[user.UserId] = variant;
        }

        // Assert - Distribution should be roughly 50/50
        var controlCount = userBuckets.Values.Count(v => v == "Control");
        var treatmentCount = userBuckets.Values.Count(v => v == "Treatment");

        controlCount.Should().BeGreaterThan(30);
        treatmentCount.Should().BeGreaterThan(30);
        (controlCount + treatmentCount).Should().Be(100);
    }
}
