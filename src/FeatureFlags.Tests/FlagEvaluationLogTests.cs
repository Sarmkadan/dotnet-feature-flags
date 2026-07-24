using System;
using FeatureFlags.Models;
using Xunit;

namespace FeatureFlags.Tests;

public class FlagEvaluationLogTests
{
    [Fact]
    public void Constructor_InitializesPropertiesWithDefaultValues()
    {
        // Arrange & Act
        var log = new FlagEvaluationLog();

        // Assert
        Assert.Equal(string.Empty, log.FlagName);
        Assert.Equal(string.Empty, log.UserId);
        Assert.False(log.Result);
        Assert.Equal(DateTime.UtcNow, log.Timestamp, TimeSpan.FromSeconds(1));
        Assert.Equal(string.Empty, log.Reason);
    }

    [Fact]
    public void Constructor_WithParameters_InitializesPropertiesCorrectly()
    {
        // Arrange
        var flagName = "new-feature-flag";
        var userId = "user-123";
        var result = true;
        var timestamp = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var reason = "PercentageRollout";

        // Act
        var log = new FlagEvaluationLog
        {
            FlagName = flagName,
            UserId = userId,
            Result = result,
            Timestamp = timestamp,
            Reason = reason
        };

        // Assert
        Assert.Equal(flagName, log.FlagName);
        Assert.Equal(userId, log.UserId);
        Assert.Equal(result, log.Result);
        Assert.Equal(timestamp, log.Timestamp);
        Assert.Equal(reason, log.Reason);
    }

    [Fact]
    public void FlagName_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var flagName = "test-flag";

        // Act
        var log = new FlagEvaluationLog { FlagName = flagName };

        // Assert
        Assert.Equal(flagName, log.FlagName);
    }

    [Fact]
    public void UserId_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var userId = "test-user";

        // Act
        var log = new FlagEvaluationLog { UserId = userId };

        // Assert
        Assert.Equal(userId, log.UserId);
    }

    [Fact]
    public void Result_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var expectedTrue = true;
        var expectedFalse = false;

        // Act
        var logTrue = new FlagEvaluationLog { Result = expectedTrue };
        var logFalse = new FlagEvaluationLog { Result = expectedFalse };

        // Assert
        Assert.Equal(expectedTrue, logTrue.Result);
        Assert.Equal(expectedFalse, logFalse.Result);
    }

    [Fact]
    public void Timestamp_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var expected = new DateTime(2024, 6, 1, 14, 25, 30, DateTimeKind.Utc);

        // Act
        var log = new FlagEvaluationLog { Timestamp = expected };

        // Assert
        Assert.Equal(expected, log.Timestamp);
    }

    [Fact]
    public void Reason_SetAndGet_ReturnsExpectedValue()
    {
        // Arrange
        var reason = "FlagDisabled";

        // Act
        var log = new FlagEvaluationLog { Reason = reason };

        // Assert
        Assert.Equal(reason, log.Reason);
    }

    [Fact]
    public void Timestamp_DefaultsToUtcNow()
    {
        // Arrange & Act
        var log = new FlagEvaluationLog();

        // Assert
        Assert.InRange(log.Timestamp, DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void EmptyFlagName_IsAllowed()
    {
        // Arrange
        var flagName = string.Empty;

        // Act
        var log = new FlagEvaluationLog { FlagName = flagName };

        // Assert
        Assert.Equal(string.Empty, log.FlagName);
    }

    [Fact]
    public void WhitespaceFlagName_IsAllowed()
    {
        // Arrange
        var whitespace = "   ";

        // Act
        var log = new FlagEvaluationLog { FlagName = whitespace };

        // Assert
        Assert.Equal(whitespace, log.FlagName);
    }

    [Fact]
    public void EmptyUserId_IsAllowed()
    {
        // Arrange
        var userId = string.Empty;

        // Act
        var log = new FlagEvaluationLog { UserId = userId };

        // Assert
        Assert.Equal(string.Empty, log.UserId);
    }

    [Fact]
    public void EmptyReason_IsAllowed()
    {
        // Arrange
        var reason = string.Empty;

        // Act
        var log = new FlagEvaluationLog { Reason = reason };

        // Assert
        Assert.Equal(string.Empty, log.Reason);
    }

    [Fact]
    public void NullFlagName_DoesNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() => new FlagEvaluationLog { FlagName = null! });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void NullUserId_DoesNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() => new FlagEvaluationLog { UserId = null! });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void NullReason_DoesNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() => new FlagEvaluationLog { Reason = null! });

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void BoundaryDateTimeValues_AreHandledCorrectly()
    {
        // Arrange
        var minDate = DateTime.MinValue;
        var maxDate = DateTime.MaxValue;

        // Act
        var logMin = new FlagEvaluationLog { Timestamp = minDate };
        var minResult = logMin.Timestamp;

        var logMax = new FlagEvaluationLog { Timestamp = maxDate };
        var maxResult = logMax.Timestamp;

        // Assert
        Assert.Equal(minDate, minResult);
        Assert.Equal(maxDate, maxResult);
    }

    [Fact]
    public void MultipleProperties_CanBeSetIndependently()
    {
        // Arrange
        var flagName = "multi-test";
        var userId = "user-456";
        var result = true;
        var timestamp = new DateTime(2024, 3, 10, 9, 15, 0, DateTimeKind.Utc);
        var reason = "RulesBased";

        // Act
        var log = new FlagEvaluationLog
        {
            FlagName = flagName,
            UserId = userId,
            Result = result,
            Timestamp = timestamp,
            Reason = reason
        };

        // Assert
        Assert.Equal(flagName, log.FlagName);
        Assert.Equal(userId, log.UserId);
        Assert.True(log.Result);
        Assert.Equal(timestamp, log.Timestamp);
        Assert.Equal(reason, log.Reason);
    }
}