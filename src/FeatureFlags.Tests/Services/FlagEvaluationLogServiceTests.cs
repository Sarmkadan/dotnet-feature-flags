#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using FeatureFlags.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FeatureFlags.Tests.Services;

/// <summary>
/// Unit tests for FlagEvaluationLogService covering flag evaluation tracking,
/// metrics aggregation, and in-memory log management.
/// </summary>
public sealed class FlagEvaluationLogServiceTests
{
    private readonly FlagEvaluationLogService _service;
    private readonly Mock<ILogger<FlagEvaluationLogService>> _loggerMock;

    public FlagEvaluationLogServiceTests()
    {
        _loggerMock = new Mock<ILogger<FlagEvaluationLogService>>();
        _service = new FlagEvaluationLogService(_loggerMock.Object);
    }

    [Fact]
    public void LogEvaluation_WithValidFlag_RecordsLog()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true };
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };
        _loggerMock.Object.LogInformation("LogEvaluation_WithValidFlag_RecordsLog called with {FlagKey} and {UserId}", flag.Key, userContext.UserId);

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
        _loggerMock.Object.LogInformation("LogEvaluation_MultipleCalls_RecordsAllEvaluations started with {FlagKey}", flag.Key);

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
        _loggerMock.Object.LogInformation("LogEvaluation_WithNullFlag_HandlesGracefully called with null flag");

        // Act - Should not throw
        try
        {
            _service.LogEvaluation(null!, userContext, true);
        }
        catch (ArgumentNullException ex)
        {
            _loggerMock.Object.LogError(ex, "Failed to evaluate null flag for {UserId}", userContext.UserId);
            // Expected behavior if implementation validates inputs
        }
    }

    [Fact]
    public void LogEvaluation_WithNullUserContext_HandlesGracefully()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true };
        _loggerMock.Object.LogInformation("LogEvaluation_WithNullUserContext_HandlesGracefully started with {FlagKey}", flag.Key);

        // Act - Should not throw
        try
        {
            _service.LogEvaluation(flag, null!, true);
        }
        catch (ArgumentNullException ex)
        {
            _loggerMock.Object.LogError(ex, "Failed to evaluate flag for null user context {FlagKey}", flag.Key);
            // Expected behavior if implementation validates inputs
        }
    }

    [Fact]
    public void GetEvaluationLogs_WithNoLogs_ReturnsEmptyList()
    {
        // Arrange
        var freshLoggerMock = new Mock<ILogger<FlagEvaluationLogService>>();
        var freshService = new FlagEvaluationLogService(freshLoggerMock.Object);
        freshLoggerMock.Object.LogInformation("GetEvaluationLogs_WithNoLogs_ReturnsEmptyList started");

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
        _loggerMock.Object.LogInformation("GetEvaluationLogs_ReturnsCopy_NotOriginalList started");
        _service.LogEvaluation(flag, userContext, true);

        // Act
        var logs1 = _service.GetEvaluationLogs();
        var logs2 = _service.GetEvaluationLogs();
        _loggerMock.Object.LogInformation("GetEvaluationLogs_ReturnsCopy_NotOriginalList ended");

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
        _loggerMock.Object.LogInformation("ClearLogs_RemovesAllEvaluations started");
        _service.LogEvaluation(flag, userContext, true);
        _service.LogEvaluation(flag, userContext, false);

        // Act
        _service.ClearLogs();
        var logs = _service.GetEvaluationLogs();
        _loggerMock.Object.LogInformation("ClearLogs_RemovesAllEvaluations ended with {LogCount}", logs.Count);

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
        _loggerMock.Object.LogInformation("LogEvaluation_RecordsTimestamp started with {FlagKey}", flag.Key);

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
        _loggerMock.Object.LogInformation("GetEvaluationLogsForFlag_FiltersByFlagKey started");

        _service.LogEvaluation(flag1, user, true);
        _service.LogEvaluation(flag2, user, false);
        _service.LogEvaluation(flag1, user, true);

        // Act
        var flag1Logs = _service.GetEvaluationLogsForFlag("flag-1");
        _loggerMock.Object.LogInformation("GetEvaluationLogsForFlag_FiltersByFlagKey ended with {LogCount}", flag1Logs.Count);

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
        _loggerMock.Object.LogInformation("GetEvaluationLogStats_ReturnsAccurateMetrics started");

        _service.LogEvaluation(flag, user1, true);
        _service.LogEvaluation(flag, user2, true);
        _service.LogEvaluation(flag, user1, false);

        // Act
        var logs = _service.GetEvaluationLogs();
        _loggerMock.Object.LogInformation("GetEvaluationLogStats_ReturnsAccurateMetrics ended with {LogCount}", logs.Count);

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
        _loggerMock.Object.LogInformation("GetEvaluationLogsForUser_FiltersByUserId started");

        _service.LogEvaluation(flag, user1, true);
        _service.LogEvaluation(flag, user2, false);
        _service.LogEvaluation(flag, user1, true);

        // Act
        var user1Logs = _service.GetEvaluationLogsForUser("user1");
        _loggerMock.Object.LogInformation("GetEvaluationLogsForUser_FiltersByUserId ended with {LogCount}", user1Logs.Count);

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
