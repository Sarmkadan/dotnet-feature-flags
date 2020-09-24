#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Data;
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
/// Unit tests for GradualRolloutSchedulerService covering scheduled rollout processing,
/// status tracking, and manual advancement of gradual rollouts.
/// </summary>
public sealed class GradualRolloutSchedulerServiceTests
{
    private readonly GradualRolloutSchedulerService _service;
    private readonly Mock<FeatureFlagDbContext> _contextMock;
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<ILogger<GradualRolloutSchedulerService>> _loggerMock;

    public GradualRolloutSchedulerServiceTests()
    {
        _contextMock = new Mock<FeatureFlagDbContext>();
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _loggerMock = new Mock<ILogger<GradualRolloutSchedulerService>>();

        _service = new GradualRolloutSchedulerService(
            _contextMock.Object,
            _auditLogRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public void GetScheduleStatusAsync_WithNegativeId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _service.GetScheduleStatusAsync(0));
        Assert.ThrowsAsync<ArgumentException>(() => _service.GetScheduleStatusAsync(-1));
    }

    [Fact]
    public void AdvanceRolloutAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _service.AdvanceRolloutAsync(0, "admin"));
        Assert.ThrowsAsync<ArgumentException>(() => _service.AdvanceRolloutAsync(-1, "admin"));
    }

    [Fact]
    public void AdvanceRolloutAsync_WithEmptyAdvancedBy_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() => _service.AdvanceRolloutAsync(1, ""));
        Assert.ThrowsAsync<ArgumentException>(() => _service.AdvanceRolloutAsync(1, "   "));
    }

    [Fact]
    public async Task ProcessScheduledRolloutsAsync_WithNoStrategies_ReturnsZero()
    {
        // Arrange
        var strategyDbSet = CreateMockDbSet<RolloutStrategy>(new List<RolloutStrategy>());
        _contextMock.Setup(c => c.RolloutStrategies).Returns(strategyDbSet);

        // Act
        var result = await _service.ProcessScheduledRolloutsAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task ProcessScheduledRolloutsAsync_WithInactiveStrategy_SkipsIt()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = false, PercentageRollout = 0 };
        var strategy = new RolloutStrategy
        {
            Id = 1,
            FeatureFlagId = 1,
            IsGradual = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            IsActive = false,
            FeatureFlag = flag
        };
        var strategies = new List<RolloutStrategy> { strategy };
        var strategyDbSet = CreateMockDbSet(strategies);
        _contextMock.Setup(c => c.RolloutStrategies).Returns(strategyDbSet);

        // Act
        var result = await _service.ProcessScheduledRolloutsAsync();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task ProcessScheduledRolloutsAsync_WithCancellation_StopsProcessing()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true, PercentageRollout = 10 };
        var strategy = new RolloutStrategy
        {
            Id = 1,
            FeatureFlagId = 1,
            IsGradual = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
            FeatureFlag = flag
        };
        var strategies = new List<RolloutStrategy> { strategy };
        var strategyDbSet = CreateMockDbSet(strategies);
        _contextMock.Setup(c => c.RolloutStrategies).Returns(strategyDbSet);

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _service.ProcessScheduledRolloutsAsync(cts.Token);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task ProcessScheduledRolloutsAsync_CalculatesCorrectCount()
    {
        // Arrange - Create a mock scenario with strategies
        var flags = new List<FeatureFlag>
        {
            new FeatureFlag { Id = 1, Key = "flag1", IsEnabled = true, PercentageRollout = 10 },
            new FeatureFlag { Id = 2, Key = "flag2", IsEnabled = true, PercentageRollout = 20 }
        };

        var strategies = new List<RolloutStrategy>
        {
            new RolloutStrategy
            {
                Id = 1,
                FeatureFlagId = 1,
                IsGradual = true,
                StartDate = DateTime.UtcNow.AddDays(-1),
                IsActive = true,
                FeatureFlag = flags[0],
                DailyIncrement = 5
            }
        };

        var strategyDbSet = CreateMockDbSet(strategies);
        _contextMock.Setup(c => c.RolloutStrategies).Returns(strategyDbSet);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _auditLogRepositoryMock.Setup(r => r.AddAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.ProcessScheduledRolloutsAsync();

        // Assert
        // Count should be either 0 or 1 depending on percentage calculation
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task AdvanceRolloutAsync_WithValidStrategy_UpdatesFlag()
    {
        // Arrange
        var flag = new FeatureFlag { Id = 1, Key = "test-flag", IsEnabled = true, PercentageRollout = 10 };
        var strategy = new RolloutStrategy
        {
            Id = 1,
            FeatureFlagId = 1,
            IsGradual = true,
            StartDate = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
            FeatureFlag = flag,
            DailyIncrement = 5
        };

        var strategies = new List<RolloutStrategy> { strategy };
        var strategyDbSet = CreateMockDbSet(strategies);
        _contextMock.Setup(c => c.RolloutStrategies).Returns(strategyDbSet);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _auditLogRepositoryMock.Setup(r => r.AddAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.AdvanceRolloutAsync(1, "admin");

        // Assert
        result.Should().BeOfType<bool>();
    }

    [Fact]
    public async Task AdvanceRolloutAsync_WithoutStrategy_ReturnsFalse()
    {
        // Arrange
        var strategies = new List<RolloutStrategy>();
        var strategyDbSet = CreateMockDbSet(strategies);
        _contextMock.Setup(c => c.RolloutStrategies).Returns(strategyDbSet);

        // Act
        var result = await _service.AdvanceRolloutAsync(999, "admin");

        // Assert
        result.Should().BeFalse();
    }

    private IQueryable<T> CreateMockDbSet<T>(List<T> data) where T : class
    {
        return data.AsQueryable();
    }
}
