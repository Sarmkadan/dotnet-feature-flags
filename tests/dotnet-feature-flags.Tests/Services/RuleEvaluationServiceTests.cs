#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Models;
using FeatureFlags.Repository;
using FeatureFlags.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FeatureFlags.Tests.Services;

/// <summary>
/// Unit tests for RuleEvaluationService covering AND/OR condition logic and inactive rule handling.
/// </summary>
public class RuleEvaluationServiceTests
{
    private readonly RuleEvaluationService _service;
    private readonly Mock<IFeatureFlagRepository> _repositoryMock;
    private readonly Mock<ILogger<RuleEvaluationService>> _loggerMock;

    public RuleEvaluationServiceTests()
    {
        _repositoryMock = new Mock<IFeatureFlagRepository>();
        _loggerMock = new Mock<ILogger<RuleEvaluationService>>();
        _service = new RuleEvaluationService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void EvaluateCondition_WithInactiveCondition_ReturnsFalse()
    {
        // Arrange
        var condition = new Condition
        {
            AttributeName = "country",
            Operator = ConditionOperator.Equals,
            ExpectedValue = "US",
            IsActive = false
        };
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com", Country = "US" };

        // Act
        var result = _service.EvaluateCondition(condition, userContext);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void EvaluateCondition_WithNullCondition_ThrowsArgumentNullException()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.EvaluateCondition(null!, userContext));
    }

    [Fact]
    public async Task EvaluateRuleAsync_WithInactiveRule_ReturnsFalse()
    {
        // Arrange
        var rule = new Rule
        {
            Name = "Inactive Rule",
            IsActive = false,
            ConditionLogic = "AND",
            Conditions = new List<Condition>
            {
                new Condition { AttributeName = "country", Operator = ConditionOperator.Equals, ExpectedValue = "US", IsActive = true }
            }
        };
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com", Country = "US" };

        // Act
        var result = await _service.EvaluateRuleAsync(rule, userContext);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateRuleAsync_WithAndLogic_AllConditionsMatch_ReturnsTrue()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user1",
            Email = "user@test.com",
            Country = "US",
            Tier = "premium"
        };
        var rule = new Rule
        {
            Name = "US Premium Rule",
            IsActive = true,
            ConditionLogic = "AND",
            Conditions = new List<Condition>
            {
                new Condition { AttributeName = "country", Operator = ConditionOperator.Equals, ExpectedValue = "US", IsActive = true },
                new Condition { AttributeName = "tier", Operator = ConditionOperator.Equals, ExpectedValue = "premium", IsActive = true }
            }
        };

        // Act
        var result = await _service.EvaluateRuleAsync(rule, userContext);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateRuleAsync_WithAndLogic_OneConditionFails_ReturnsFalse()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user1",
            Email = "user@test.com",
            Country = "CA",
            Tier = "premium"
        };
        var rule = new Rule
        {
            Name = "US Premium Rule",
            IsActive = true,
            ConditionLogic = "AND",
            Conditions = new List<Condition>
            {
                new Condition { AttributeName = "country", Operator = ConditionOperator.Equals, ExpectedValue = "US", IsActive = true },
                new Condition { AttributeName = "tier", Operator = ConditionOperator.Equals, ExpectedValue = "premium", IsActive = true }
            }
        };

        // Act
        var result = await _service.EvaluateRuleAsync(rule, userContext);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateRuleAsync_WithOrLogic_OneConditionMatches_ReturnsTrue()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user1",
            Email = "user@test.com",
            Country = "CA",
            Tier = "free"
        };
        var rule = new Rule
        {
            Name = "CA or Premium Rule",
            IsActive = true,
            ConditionLogic = "OR",
            Conditions = new List<Condition>
            {
                new Condition { AttributeName = "country", Operator = ConditionOperator.Equals, ExpectedValue = "CA", IsActive = true },
                new Condition { AttributeName = "tier", Operator = ConditionOperator.Equals, ExpectedValue = "premium", IsActive = true }
            }
        };

        // Act
        var result = await _service.EvaluateRuleAsync(rule, userContext);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateRuleAsync_WithOrLogic_NoConditionsMatch_ReturnsFalse()
    {
        // Arrange
        var userContext = new UserContext
        {
            UserId = "user1",
            Email = "user@test.com",
            Country = "DE",
            Tier = "free"
        };
        var rule = new Rule
        {
            Name = "US or Premium Rule",
            IsActive = true,
            ConditionLogic = "OR",
            Conditions = new List<Condition>
            {
                new Condition { AttributeName = "country", Operator = ConditionOperator.Equals, ExpectedValue = "US", IsActive = true },
                new Condition { AttributeName = "tier", Operator = ConditionOperator.Equals, ExpectedValue = "premium", IsActive = true }
            }
        };

        // Act
        var result = await _service.EvaluateRuleAsync(rule, userContext);

        // Assert
        result.Should().BeFalse();
    }
}
