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
/// Unit tests for FeatureFlagService covering flag evaluation, routing by rollout type,
/// and validation of inputs using mocked repository dependencies.
/// </summary>
public sealed class FeatureFlagServiceTests
{
    private readonly FeatureFlagService _service;
    private readonly Mock<IFeatureFlagRepository> _featureFlagRepositoryMock;
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<IRuleEvaluationService> _ruleEvaluationServiceMock;
    private readonly Mock<IPercentageRolloutService> _percentageRolloutServiceMock;
    private readonly Mock<ILogger<FeatureFlagService>> _loggerMock;

    public FeatureFlagServiceTests()
    {
        _featureFlagRepositoryMock = new Mock<IFeatureFlagRepository>();
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _ruleEvaluationServiceMock = new Mock<IRuleEvaluationService>();
        _percentageRolloutServiceMock = new Mock<IPercentageRolloutService>();
        _loggerMock = new Mock<ILogger<FeatureFlagService>>();

        _service = new FeatureFlagService(
            _featureFlagRepositoryMock.Object,
            _auditLogRepositoryMock.Object,
            _ruleEvaluationServiceMock.Object,
            _percentageRolloutServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task IsEnabledAsync_WithEmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.IsEnabledAsync(string.Empty, userContext));
    }

    [Fact]
    public async Task IsEnabledAsync_WithInvalidUserContext_ThrowsInvalidOperationException()
    {
        // Arrange — UserId is absent, making context invalid
        var userContext = new UserContext { Email = "user@test.com" };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.IsEnabledAsync("some-flag", userContext));
    }

    [Fact]
    public async Task IsEnabledAsync_WhenFlagNotFound_ReturnsFalse()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };
        _featureFlagRepositoryMock
            .Setup(r => r.GetByKeyAsync("missing-flag"))
            .ReturnsAsync((FeatureFlag?)null);

        // Act
        var result = await _service.IsEnabledAsync("missing-flag", userContext);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_WhenFlagIsDisabled_ReturnsFalse()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };
        _featureFlagRepositoryMock
            .Setup(r => r.GetByKeyAsync("disabled-flag"))
            .ReturnsAsync(new FeatureFlag { Id = 1, Key = "disabled-flag", IsEnabled = false, RolloutType = RolloutType.Full });

        // Act
        var result = await _service.IsEnabledAsync("disabled-flag", userContext);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_WithFullRolloutAndEnabledFlag_ReturnsTrue()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };
        _featureFlagRepositoryMock
            .Setup(r => r.GetByKeyAsync("full-flag"))
            .ReturnsAsync(new FeatureFlag { Id = 1, Key = "full-flag", IsEnabled = true, RolloutType = RolloutType.Full });

        // Act
        var result = await _service.IsEnabledAsync("full-flag", userContext);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEnabledAsync_WithNoneRolloutAndEnabledFlag_ReturnsFalse()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };
        _featureFlagRepositoryMock
            .Setup(r => r.GetByKeyAsync("none-flag"))
            .ReturnsAsync(new FeatureFlag { Id = 1, Key = "none-flag", IsEnabled = true, RolloutType = RolloutType.None });

        // Act
        var result = await _service.IsEnabledAsync("none-flag", userContext);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_WithPercentageRollout_DelegatesToPercentageService()
    {
        // Arrange
        var userContext = new UserContext { UserId = "user1", Email = "user@test.com" };
        var flag = new FeatureFlag { Id = 1, Key = "pct-flag", IsEnabled = true, RolloutType = RolloutType.Percentage, PercentageRollout = 50 };
        _featureFlagRepositoryMock.Setup(r => r.GetByKeyAsync("pct-flag")).ReturnsAsync(flag);
        _percentageRolloutServiceMock.Setup(s => s.EvaluateAsync(flag, userContext)).ReturnsAsync(true);

        // Act
        var result = await _service.IsEnabledAsync("pct-flag", userContext);

        // Assert
        result.Should().BeTrue();
        _percentageRolloutServiceMock.Verify(s => s.EvaluateAsync(flag, userContext), Times.Once);
    }

    [Fact]
    public async Task CreateFeatureFlagAsync_WhenKeyAlreadyExists_ThrowsInvalidOperationException()
    {
        // Arrange
        var featureFlag = new FeatureFlag { Key = "existing-flag", DisplayName = "Existing Flag", RolloutType = RolloutType.Full };
        _featureFlagRepositoryMock
            .Setup(r => r.KeyExistsAsync("existing-flag"))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateFeatureFlagAsync(featureFlag, "admin"));
    }
}
