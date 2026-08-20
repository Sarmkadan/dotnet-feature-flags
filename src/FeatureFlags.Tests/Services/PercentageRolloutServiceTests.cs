#nullable enable
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
public sealed class PercentageRolloutServiceTests
{
    private readonly IPercentageRolloutService _service;
    private readonly Mock<ILogger<PercentageRolloutService>> _loggerMock;

    public PercentageRolloutServiceTests()
    {
        _loggerMock = new Mock<ILogger<PercentageRolloutService>>();
        _service = new PercentageRolloutService(_loggerMock.Object);
        _loggerMock.Object.LogInformation("PercentageRolloutServiceTests instance created");
    }

    [Fact]
    public async Task EvaluateAsync_WithNullFeatureFlag_ThrowsArgumentNullException()
    {
        _loggerMock.Object.LogInformation("Starting test EvaluateAsync_WithNullFeatureFlag_ThrowsArgumentNullException");
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.EvaluateAsync(null!, userContext));
        _loggerMock.Object.LogInformation("Finished test EvaluateAsync_WithNullFeatureFlag_ThrowsArgumentNullException");
    }

    [Fact]
    public async Task EvaluateAsync_WithNullUserContext_ThrowsArgumentNullException()
    {
        _loggerMock.Object.LogInformation("Starting test EvaluateAsync_WithNullUserContext_ThrowsArgumentNullException");
        // Arrange
        var featureFlag = new FeatureFlag { Id = 1, Key = "test-flag", PercentageRollout = 50 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.EvaluateAsync(featureFlag, null!));
        _loggerMock.Object.LogInformation("Finished test EvaluateAsync_WithNullUserContext_ThrowsArgumentNullException");
    }

    [Fact]
    public async Task EvaluateAsync_With100Percent_ReturnsTrue()
    {
        _loggerMock.Object.LogInformation("Starting test EvaluateAsync_With100Percent_ReturnsTrue");
        // Arrange
        var featureFlag = new FeatureFlag { Id = 1, Key = "test-flag", PercentageRollout = 100 };
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act
        var result = await _service.EvaluateAsync(featureFlag, userContext);

        // Assert
        result.Should().BeTrue();
        _loggerMock.Object.LogInformation("Finished test EvaluateAsync_With100Percent_ReturnsTrue");
    }

    [Fact]
    public async Task EvaluateAsync_With0Percent_ReturnsFalse()
    {
        _loggerMock.Object.LogInformation("Starting test EvaluateAsync_With0Percent_ReturnsFalse");
        // Arrange
        var featureFlag = new FeatureFlag { Id = 1, Key = "test-flag", PercentageRollout = 0 };
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act
        var result = await _service.EvaluateAsync(featureFlag, userContext);

        // Assert
        result.Should().BeFalse();
        _loggerMock.Object.LogInformation("Finished test EvaluateAsync_With0Percent_ReturnsFalse");
    }

    [Fact]
    public void IsUserInRollout_SameUserConsistentHash_ReturnsSameResult()
    {
        _loggerMock.Object.LogInformation("Starting test IsUserInRollout_SameUserConsistentHash_ReturnsSameResult");
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };
        var flagKey = "test-flag";
        var rolloutPercentage = 50;

        // Act
        var result1 = _service.IsUserInRollout(userContext, flagKey, rolloutPercentage);
        var result2 = _service.IsUserInRollout(userContext, flagKey, rolloutPercentage);

        // Assert
        result1.Should().Be(result2);
        _loggerMock.Object.LogInformation("Finished test IsUserInRollout_SameUserConsistentHash_ReturnsSameResult");
    }

    [Fact]
    public void GetUserBucket_ReturnsValueBetween0And99()
    {
        _loggerMock.Object.LogInformation("Starting test GetUserBucket_ReturnsValueBetween0And99");
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };
        var flagKey = "test-flag";

        // Act
        var bucket = _service.GetUserBucket(userContext, flagKey);

        // Assert
        bucket.Should().BeGreaterThanOrEqualTo(0);
        bucket.Should().BeLessThan(100);
        _loggerMock.Object.LogInformation("Finished test GetUserBucket_ReturnsValueBetween0And99");
    }

    [Fact]
    public void IsUserInRollout_WithNullUserContext_ThrowsArgumentNullException()
    {
        _loggerMock.Object.LogInformation("Starting test IsUserInRollout_WithNullUserContext_ThrowsArgumentNullException");
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _service.IsUserInRollout(null!, "flag", 50));
        _loggerMock.Object.LogInformation("Finished test IsUserInRollout_WithNullUserContext_ThrowsArgumentNullException");
    }

    [Fact]
    public void IsUserInRollout_WithNullFlagKey_ThrowsArgumentException()
    {
        _loggerMock.Object.LogInformation("Starting test IsUserInRollout_WithNullFlagKey_ThrowsArgumentException");
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.IsUserInRollout(userContext, null!, 50));
        _loggerMock.Object.LogInformation("Finished test IsUserInRollout_WithNullFlagKey_ThrowsArgumentException");
    }

    [Fact]
    public void IsUserInRollout_WithInvalidPercentage_ThrowsArgumentException()
    {
        _loggerMock.Object.LogInformation("Starting test IsUserInRollout_WithInvalidPercentage_ThrowsArgumentException");
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@example.com" };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.IsUserInRollout(userContext, "flag", 101));
        Assert.Throws<ArgumentException>(() => _service.IsUserInRollout(userContext, "flag", -1));
        _loggerMock.Object.LogInformation("Finished test IsUserInRollout_WithInvalidPercentage_ThrowsArgumentException");
    }

    [Fact]
    public void IsUserInRollout_With0Percent_ReturnsFalseForAllUsers()
    {
        _loggerMock.Object.LogInformation("Starting test IsUserInRollout_With0Percent_ReturnsFalseForAllUsers");
        // Arrange
        var flagKey = "test-flag";

        // Act & Assert - 0% rollout must never enable any user
        for (int i = 0; i < 1000; i++)
        {
            var userContext = new UserContext { UserId = $"user{i}", Email = $"user{i}@example.com" };
            var result = _service.IsUserInRollout(userContext, flagKey, 0);
            result.Should().BeFalse("0% rollout must never enable any user");
        }
        _loggerMock.Object.LogInformation("Finished test IsUserInRollout_With0Percent_ReturnsFalseForAllUsers");
    }

    [Fact]
    public void IsUserInRollout_With100Percent_ReturnsTrueForAllUsers()
    {
        _loggerMock.Object.LogInformation("Starting test IsUserInRollout_With100Percent_ReturnsTrueForAllUsers");
        // Arrange
        var flagKey = "test-flag";

        // Act & Assert - 100% rollout must always enable all users
        for (int i = 0; i < 1000; i++)
        {
            var userContext = new UserContext { UserId = $"user{i}", Email = $"user{i}@example.com" };
            var result = _service.IsUserInRollout(userContext, flagKey, 100);
            result.Should().BeTrue("100% rollout must always enable all users");
        }
        _loggerMock.Object.LogInformation("Finished test IsUserInRollout_With100Percent_ReturnsTrueForAllUsers");
    }

    [Fact]
    public void IsUserInRollout_BoundaryBucketComparison_With1Percent()
    {
        _loggerMock.Object.LogInformation("Starting test IsUserInRollout_BoundaryBucketComparison_With1Percent");
        // Arrange
        var flagKey = "boundary-test";

        // Act & Assert - For 1% rollout, only bucket 0 should be enabled
        // We test that at least one user gets bucket 0 and is enabled
        bool foundBucket0Enabled = false;
        for (int i = 0; i < 1000; i++)
        {
            var userContext = new UserContext { UserId = $"user{i}", Email = $"user{i}@example.com" };
            var bucket = _service.GetUserBucket(userContext, flagKey);
            var isEnabled = _service.IsUserInRollout(userContext, flagKey, 1);

            if (bucket == 0)
            {
                isEnabled.Should().BeTrue("Bucket 0 must be enabled for 1% rollout");
                foundBucket0Enabled = true;
            }
        }

        foundBucket0Enabled.Should().BeTrue("At least one user should hash to bucket 0");
        _loggerMock.Object.LogInformation("Finished test IsUserInRollout_BoundaryBucketComparison_With1Percent");
    }

    [Fact]
    public void IsUserInRollout_BoundaryBucketComparison_With99Percent()
    {
        _loggerMock.Object.LogInformation("Starting test IsUserInRollout_BoundaryBucketComparison_With99Percent");
        // Arrange
        var flagKey = "boundary-test";

        // Act & Assert - For 99% rollout, buckets 0-98 should be enabled, bucket 99 should not
        bool foundBucket99Disabled = false;
        bool foundBucket98Enabled = false;

        for (int i = 0; i < 1000; i++)
        {
            var userContext = new UserContext { UserId = $"user{i}", Email = $"user{i}@example.com" };
            var bucket = _service.GetUserBucket(userContext, flagKey);
            var isEnabled = _service.IsUserInRollout(userContext, flagKey, 99);

            if (bucket == 99)
            {
                isEnabled.Should().BeFalse("Bucket 99 must NOT be enabled for 99% rollout");
                foundBucket99Disabled = true;
            }

            if (bucket == 98)
            {
                isEnabled.Should().BeTrue("Bucket 98 must be enabled for 99% rollout");
                foundBucket98Enabled = true;
            }
        }

        foundBucket99Disabled.Should().BeTrue("At least one user should hash to bucket 99");
        foundBucket98Enabled.Should().BeTrue("At least one user should hash to bucket 98");
        _loggerMock.Object.LogInformation("Finished test IsUserInRollout_BoundaryBucketComparison_With99Percent");
    }

    [Fact]
    public void IsUserInRollout_BoundaryBucketComparison_With98Percent()
    {
        _loggerMock.Object.LogInformation("Starting test IsUserInRollout_BoundaryBucketComparison_With98Percent");
        // Arrange
        var flagKey = "boundary-test";

        // Act & Assert - For 98% rollout, buckets 0-97 should be enabled, bucket 98 should not
        bool foundBucket98Disabled = false;
        bool foundBucket97Enabled = false;

        for (int i = 0; i < 1000; i++)
        {
            var userContext = new UserContext { UserId = $"user{i}", Email = $"user{i}@example.com" };
            var bucket = _service.GetUserBucket(userContext, flagKey);
            var isEnabled = _service.IsUserInRollout(userContext, flagKey, 98);

            if (bucket == 98)
            {
                isEnabled.Should().BeFalse("Bucket 98 must NOT be enabled for 98% rollout");
                foundBucket98Disabled = true;
            }

            if (bucket == 97)
            {
                isEnabled.Should().BeTrue("Bucket 97 must be enabled for 98% rollout");
                foundBucket97Enabled = true;
            }
        }

        foundBucket98Disabled.Should().BeTrue("At least one user should hash to bucket 98");
        foundBucket97Enabled.Should().BeTrue("At least one user should hash to bucket 97");
        _loggerMock.Object.LogInformation("Finished test IsUserInRollout_BoundaryBucketComparison_With98Percent");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(75)]
    [InlineData(100)]
    public void IsUserInRollout_DistributionTest(int percentage)
    {
        _loggerMock.Object.LogInformation("Starting test IsUserInRollout_DistributionTest with {Percentage}", percentage);
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
        _loggerMock.Object.LogInformation("Finished test IsUserInRollout_DistributionTest with {Percentage}", percentage);
    }
}