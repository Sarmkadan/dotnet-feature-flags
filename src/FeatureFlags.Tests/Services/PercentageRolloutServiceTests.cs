// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using FeatureFlags.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace FeatureFlags.Tests.Services;

/// <summary>
/// Unit tests for percentage-based rollout evaluation.
/// Tests consistent hashing and rollout percentage calculations.
/// </summary>
public class PercentageRolloutServiceTests
{
    private readonly IPercentageRolloutService _service;
    private readonly Mock<ILogger<PercentageRolloutService>> _loggerMock;

    public PercentageRolloutServiceTests()
    {
        _loggerMock = new Mock<ILogger<PercentageRolloutService>>();
        _service = new PercentageRolloutService(_loggerMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_WithNullFeatureFlag_ThrowsArgumentNullException()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.EvaluateAsync(null!, userContext));
    }

    [Fact]
    public async Task EvaluateAsync_WithNullUserContext_ThrowsArgumentNullException()
    {
        // Arrange
        var featureFlag = new FeatureFlag { Id = 1, Key = "test-flag", PercentageRollout = 50 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.EvaluateAsync(featureFlag, null!));
    }

    [Fact]
    public async Task EvaluateAsync_With100Percent_ReturnsTrue()
    {
        // Arrange
        var featureFlag = new FeatureFlag { Id = 1, Key = "test-flag", PercentageRollout = 100 };
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act
        var result = await _service.EvaluateAsync(featureFlag, userContext);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_With0Percent_ReturnsFalse()
    {
        // Arrange
        var featureFlag = new FeatureFlag { Id = 1, Key = "test-flag", PercentageRollout = 0 };
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act
        var result = await _service.EvaluateAsync(featureFlag, userContext);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsUserInRollout_SameUserConsistentHash_ReturnsSameResult()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };
        var flagKey = "test-flag";
        var rolloutPercentage = 50;

        // Act
        var result1 = _service.IsUserInRollout(userContext, flagKey, rolloutPercentage);
        var result2 = _service.IsUserInRollout(userContext, flagKey, rolloutPercentage);

        // Assert
        result1.Should().Be(result2);
    }

    [Fact]
    public void GetUserBucket_ReturnsValueBetween0And99()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };
        var flagKey = "test-flag";

        // Act
        var bucket = _service.GetUserBucket(userContext, flagKey);

        // Assert
        bucket.Should().BeGreaterThanOrEqualTo(0);
        bucket.Should().BeLessThan(100);
    }

    [Fact]
    public void IsUserInRollout_WithNullUserContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.IsUserInRollout(null!, "flag", 50));
    }

    [Fact]
    public void IsUserInRollout_WithNullFlagKey_ThrowsArgumentException()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.IsUserInRollout(userContext, null!, 50));
    }

    [Fact]
    public void IsUserInRollout_WithInvalidPercentage_ThrowsArgumentException()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.IsUserInRollout(userContext, "flag", 101));
        Assert.Throws<ArgumentException>(() => _service.IsUserInRollout(userContext, "flag", -1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public void IsUserInRollout_DistributionTest(int percentage)
    {
        // Arrange
        var flagKey = "test-flag";
        var enabledCount = 0;
        var totalUsers = 1000;

        // Act
        for (int i = 0; i < totalUsers; i++)
        {
            var userContext = new UserContext { UserId = $"user{i}", Email = $"user{i}@example.com" };
            if (_service.IsUserInRollout(userContext, flagKey, percentage))
                enabledCount++;
        }

        // Assert
        var actualPercentage = (enabledCount * 100) / totalUsers;
        actualPercentage.Should().BeCloseTo(percentage, delta: 5);
    }
}
