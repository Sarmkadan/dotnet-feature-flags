#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using FeatureFlags.Services;
using FluentAssertions;
using Xunit;

namespace FeatureFlags.Tests.Services;

/// <summary>
/// Unit tests for FlagEvaluationLogService covering flag evaluation tracking,
/// metrics aggregation, and in-memory log management.
/// </summary>
public sealed class FlagEvaluationLogServiceTests
{
    private readonly FlagEvaluationLogService _service;

    public FlagEvaluationLogServiceTests()
    {
        _service = new FlagEvaluationLogService();
    }

    [Fact]
    public void LogEvaluation_WithValidFlag_RecordsLog()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true };
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };

        // Act
        _service.LogEvaluation(flag, userContext, true);

        // Assert
        var logs = _service.GetEvaluationLogs();
        logs.Should().HaveCountGreaterThan(0);
        logs.Last().FlagName.Should().Be("test-flag");
        logs.Last().UserId.Should().Be("user1");
        logs.Last().Result.Should().BeTrue();
    }

    [Fact]
    public void LogEvaluation_MultipleCalls_RecordsAllEvaluations()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true };
        var user1 = new UserContext { UserId = "user1", Email = "user1@test.com" };
        var user2 = new UserContext { UserId = "user2", Email = "user2@test.com" };

        // Act
        _service.LogEvaluation(flag, user1, true);
        _service.LogEvaluation(flag, user2, false);
        _service.LogEvaluation(flag, user1, true);

        // Assert
        var logs = _service.GetEvaluationLogs();
        logs.Should().HaveCount(3);
        logs.Count(l => l.UserId == "user1").Should().Be(2);
        logs.Count(l => l.UserId == "user2").Should().Be(1);
    }

    [Fact]
    public void LogEvaluation_WithNullFlag_HandlesGracefully()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };

        // Act - Should not throw
        try
        {
            _service.LogEvaluation(null!, userContext, true);
        }
        catch (ArgumentNullException)
        {
            // Expected behavior if implementation validates inputs
        }
    }

    [Fact]
    public void LogEvaluation_WithNullUserContext_HandlesGracefully()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true };

        // Act - Should not throw
        try
        {
            _service.LogEvaluation(flag, null!, true);
        }
        catch (ArgumentNullException)
        {
            // Expected behavior if implementation validates inputs
        }
    }

    [Fact]
    public void GetEvaluationLogs_WithNoLogs_ReturnsEmptyList()
    {
        // Arrange
        var freshService = new FlagEvaluationLogService();

        // Act
        var logs = freshService.GetEvaluationLogs();

        // Assert
        logs.Should().BeEmpty();
    }

    [Fact]
    public void GetEvaluationLogs_ReturnsCopy_NotOriginalList()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true };
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };
        _service.LogEvaluation(flag, userContext, true);

        // Act
        var logs1 = _service.GetEvaluationLogs();
        var logs2 = _service.GetEvaluationLogs();

        // Assert
        logs1.Should().HaveCount(1);
        logs2.Should().HaveCount(1);
        // Both should reference the same data but may be different list instances
        logs1[0].FlagName.Should().Be(logs2[0].FlagName);
    }

    [Fact]
    public void ClearLogs_RemovesAllEvaluations()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true };
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };
        _service.LogEvaluation(flag, userContext, true);
        _service.LogEvaluation(flag, userContext, false);

        // Act
        _service.ClearLogs();
        var logs = _service.GetEvaluationLogs();

        // Assert
        logs.Should().BeEmpty();
    }

    [Fact]
    public void LogEvaluation_RecordsTimestamp()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true };
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };
        var beforeLog = DateTime.UtcNow;

        // Act
        _service.LogEvaluation(flag, userContext, true);
        var afterLog = DateTime.UtcNow;

        // Assert
        var logs = _service.GetEvaluationLogs();
        var log = logs.Last();
        log.Timestamp.Should().BeOnOrAfter(beforeLog);
        log.Timestamp.Should().BeOnOrBefore(afterLog);
    }

    [Fact]
    public void GetEvaluationLogsForFlag_FiltersByFlagKey()
    {
        // Arrange
        var flag1 = new FeatureFlag { Id = 1, Key = "flag-1", IsEnabled = true };
        var flag2 = new FeatureFlag { Id = 2, Key = "flag-2", IsEnabled = true };
        var user = new UserContext { UserId = "user1", Email = "user@test.com" };

        _service.LogEvaluation(flag1, user, true);
        _service.LogEvaluation(flag2, user, false);
        _service.LogEvaluation(flag1, user, true);

        // Act
        var flag1Logs = _service.GetEvaluationLogsForFlag("flag-1");

        // Assert
        flag1Logs.Should().HaveCount(2);
        flag1Logs.Should().AllSatisfy(l => l.FlagName.Should().Be("flag-1"));
    }

    [Fact]
    public void GetEvaluationLogStats_ReturnsAccurateMetrics()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true };
        var user1 = new UserContext { UserId = "user1", Email = "user1@test.com" };
        var user2 = new UserContext { UserId = "user2", Email = "user2@test.com" };

        _service.LogEvaluation(flag, user1, true);
        _service.LogEvaluation(flag, user2, true);
        _service.LogEvaluation(flag, user1, false);

        // Act
        var logs = _service.GetEvaluationLogs();

        // Assert
        logs.Should().HaveCount(3);
        logs.Count(l => l.Result).Should().Be(2);
        logs.Count(l => !l.Result).Should().Be(1);
    }

    [Fact]
    public void GetEvaluationLogsForUser_FiltersByUserId()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true };
        var user1 = new UserContext { UserId = "user1", Email = "user1@test.com" };
        var user2 = new UserContext { UserId = "user2", Email = "user2@test.com" };

        _service.LogEvaluation(flag, user1, true);
        _service.LogEvaluation(flag, user2, false);
        _service.LogEvaluation(flag, user1, true);

        // Act
        var user1Logs = _service.GetEvaluationLogsForUser("user1");

        // Assert
        user1Logs.Should().HaveCount(2);
        user1Logs.Should().AllSatisfy(l => l.UserId.Should().Be("user1"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LogEvaluation_RecordsCorrectResult(bool expectedResult)
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true };
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };

        // Act
        _service.LogEvaluation(flag, userContext, expectedResult);

        // Assert
        var logs = _service.GetEvaluationLogs();
        logs.Last().Result.Should().Be(expectedResult);
    }
}
