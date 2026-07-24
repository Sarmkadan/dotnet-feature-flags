using System;
using FeatureFlags.Models;
using Xunit;

namespace FeatureFlags.Tests;

public class FlagEvaluationLogExtensionsTests
{
    private readonly FlagEvaluationLog _testLog = new()
    {
        FlagName = "new-feature",
        UserId = "user-123",
        Result = true,
        Timestamp = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc),
        Reason = "PercentageRollout"
    };

    [Fact]
    public void ToFormattedString_WithValidLog_ReturnsFormattedString()
    {
        // Act
        var formatted = _testLog.ToFormattedString();

        // Assert
        Assert.Equal(
            "[2024-06-15 14:30:00] Feature Flag: 'new-feature', User: 'user-123', Result: ENABLED, Reason: 'PercentageRollout'",
            formatted
        );
    }

    [Fact]
    public void ToFormattedString_WithDisabledResult_ReturnsCorrectFormat()
    {
        // Arrange
        var disabledLog = new FlagEvaluationLog
        {
            FlagName = "old-feature",
            UserId = "user-456",
            Result = false,
            Timestamp = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Reason = "FlagDisabled"
        };

        // Act
        var formatted = disabledLog.ToFormattedString();

        // Assert
        Assert.Equal(
            "[2024-01-01 00:00:00] Feature Flag: 'old-feature', User: 'user-456', Result: DISABLED, Reason: 'FlagDisabled'",
            formatted
        );
    }

    [Fact]
    public void ToFormattedString_WithNullLog_ThrowsArgumentNullException()
    {
        // Arrange
        FlagEvaluationLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.ToFormattedString());
    }

    [Fact]
    public void MatchesResult_WithMatchingResult_ReturnsTrue()
    {
        // Act
        var result = _testLog.MatchesResult(true);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesResult_WithNonMatchingResult_ReturnsFalse()
    {
        // Act
        var result = _testLog.MatchesResult(false);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MatchesResult_WithDisabledLogAndExpectedDisabled_ReturnsTrue()
    {
        // Arrange
        var disabledLog = new FlagEvaluationLog
        {
            FlagName = "disabled-feature",
            UserId = "user-789",
            Result = false,
            Timestamp = DateTime.UtcNow,
            Reason = "FlagDisabled"
        };

        // Act
        var result = disabledLog.MatchesResult(false);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void MatchesResult_WithNullLog_ThrowsArgumentNullException()
    {
        // Arrange
        FlagEvaluationLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.MatchesResult(true));
    }

    [Fact]
    public void WithResult_WithNewResult_ReturnsNewLogWithUpdatedResult()
    {
        // Act
        var newLog = _testLog.WithResult(false);

        // Assert
        Assert.NotSame(_testLog, newLog);
        Assert.Equal(_testLog.FlagName, newLog.FlagName);
        Assert.Equal(_testLog.UserId, newLog.UserId);
        Assert.Equal(_testLog.Timestamp, newLog.Timestamp);
        Assert.Equal(_testLog.Reason, newLog.Reason);
        Assert.False(newLog.Result);
    }

    [Fact]
    public void WithResult_WithSameResult_ReturnsNewLogWithSameResult()
    {
        // Act
        var newLog = _testLog.WithResult(true);

        // Assert
        Assert.NotSame(_testLog, newLog);
        Assert.True(newLog.Result);
    }

    [Fact]
    public void WithResult_WithNullLog_ThrowsArgumentNullException()
    {
        // Arrange
        FlagEvaluationLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullLog!.WithResult(true));
    }

    [Fact]
    public void IsWithinTimeRange_WithTimestampWithinRange_ReturnsTrue()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2024, 6, 30, 23, 59, 59, DateTimeKind.Utc);

        // Act
        var result = _testLog.IsWithinTimeRange(startTime, endTime);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsWithinTimeRange_WithTimestampBeforeRange_ReturnsFalse()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 16, 0, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2024, 6, 30, 23, 59, 59, DateTimeKind.Utc);

        // Act
        var result = _testLog.IsWithinTimeRange(startTime, endTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsWithinTimeRange_WithTimestampAfterRange_ReturnsFalse()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2024, 6, 14, 23, 59, 59, DateTimeKind.Utc);

        // Act
        var result = _testLog.IsWithinTimeRange(startTime, endTime);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsWithinTimeRange_WithExactBoundaryStart_ReturnsTrue()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var result = _testLog.IsWithinTimeRange(startTime, endTime);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsWithinTimeRange_WithExactBoundaryEnd_ReturnsTrue()
    {
        // Arrange
        var startTime = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var endTime = new DateTime(2024, 6, 15, 14, 30, 0, DateTimeKind.Utc);

        // Act
        var result = _testLog.IsWithinTimeRange(startTime, endTime);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsWithinTimeRange_WithNullLog_ThrowsArgumentNullException()
    {
        // Arrange
        FlagEvaluationLog? nullLog = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => nullLog!.IsWithinTimeRange(DateTime.UtcNow, DateTime.UtcNow.AddHours(1))
        );
    }
}