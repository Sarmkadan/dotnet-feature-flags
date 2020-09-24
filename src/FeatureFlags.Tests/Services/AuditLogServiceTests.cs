#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Enums;
using FeatureFlags.Exceptions;
using FeatureFlags.Models;
using FeatureFlags.Repository;
using FeatureFlags.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FeatureFlags.Tests.Services;

/// <summary>
/// Unit tests for AuditLogService covering audit retrieval, filtering, and error handling.
/// </summary>
public sealed class AuditLogServiceTests
{
    private readonly AuditLogService _service;
    private readonly Mock<IAuditLogRepository> _repositoryMock;
    private readonly Mock<ILogger<AuditLogService>> _loggerMock;

    public AuditLogServiceTests()
    {
        _repositoryMock = new Mock<IAuditLogRepository>();
        _loggerMock = new Mock<ILogger<AuditLogService>>();
        _service = new AuditLogService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAuditLogsAsync_WithValidId_ReturnsLogs()
    {
        // Arrange
        var featureFlagId = 1;
        var logs = new List<AuditLog>
        {
            new AuditLog { Id = 1, FeatureFlagId = 1, Action = AuditAction.Created, ChangedBy = "admin" },
            new AuditLog { Id = 2, FeatureFlagId = 1, Action = AuditAction.Updated, ChangedBy = "admin" }
        };
        _repositoryMock.Setup(r => r.GetByFeatureFlagIdAsync(featureFlagId)).ReturnsAsync(logs);

        // Act
        var result = await _service.GetAuditLogsAsync(featureFlagId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(log => log.FeatureFlagId.Should().Be(featureFlagId));
        _repositoryMock.Verify(r => r.GetByFeatureFlagIdAsync(featureFlagId), Times.Once);
    }

    [Fact]
    public async Task GetAuditLogsAsync_WithInvalidId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAuditLogsAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAuditLogsAsync(-1));
    }

    [Fact]
    public async Task GetAuditLogsAsync_WhenRepositoryThrows_ThrowsFeatureFlagDataException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByFeatureFlagIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<FeatureFlagDataException>(() => _service.GetAuditLogsAsync(1));
    }

    [Fact]
    public async Task GetAuditLogsPagedAsync_WithValidParameters_ReturnsPaginatedLogs()
    {
        // Arrange
        var featureFlagId = 1;
        var pageNumber = 1;
        var pageSize = 10;
        var logs = new List<AuditLog>
        {
            new AuditLog { Id = 1, FeatureFlagId = 1, Action = AuditAction.Created, ChangedBy = "admin" }
        };
        _repositoryMock
            .Setup(r => r.GetByFeatureFlagIdPagedAsync(featureFlagId, pageNumber, pageSize))
            .ReturnsAsync(logs);

        // Act
        var result = await _service.GetAuditLogsPagedAsync(featureFlagId, pageNumber, pageSize);

        // Assert
        result.Should().HaveCount(1);
        _repositoryMock.Verify(r => r.GetByFeatureFlagIdPagedAsync(featureFlagId, pageNumber, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetAuditLogsPagedAsync_WithInvalidPageNumber_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAuditLogsPagedAsync(1, 0, 10));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAuditLogsPagedAsync(1, -1, 10));
    }

    [Fact]
    public async Task GetAuditLogsPagedAsync_WithInvalidPageSize_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAuditLogsPagedAsync(1, 1, 0));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAuditLogsPagedAsync(1, 1, -1));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAuditLogsPagedAsync(1, 1, 10001));
    }

    [Fact]
    public async Task GetAuditLogsByUserAsync_WithValidUser_ReturnsUserLogs()
    {
        // Arrange
        var changedBy = "admin";
        var logs = new List<AuditLog>
        {
            new AuditLog { Id = 1, FeatureFlagId = 1, Action = AuditAction.Created, ChangedBy = "admin" },
            new AuditLog { Id = 2, FeatureFlagId = 2, Action = AuditAction.Updated, ChangedBy = "admin" }
        };
        _repositoryMock.Setup(r => r.GetByChangedByAsync(changedBy)).ReturnsAsync(logs);

        // Act
        var result = await _service.GetAuditLogsByUserAsync(changedBy);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(log => log.ChangedBy.Should().Be(changedBy));
    }

    [Fact]
    public async Task GetAuditLogsByUserAsync_WithEmptyUser_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAuditLogsByUserAsync(""));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetAuditLogsByUserAsync("   "));
    }

    [Fact]
    public async Task GetRecentAuditLogsAsync_WithValidCount_ReturnsRecentLogs()
    {
        // Arrange
        var count = 5;
        var now = DateTime.UtcNow;
        var allLogs = new List<AuditLog>
        {
            new AuditLog { Id = 1, FeatureFlagId = 1, Action = AuditAction.Created, ChangedBy = "admin", ChangedAt = now.AddHours(-3) },
            new AuditLog { Id = 2, FeatureFlagId = 2, Action = AuditAction.Updated, ChangedBy = "admin", ChangedAt = now.AddHours(-1) },
            new AuditLog { Id = 3, FeatureFlagId = 3, Action = AuditAction.Deleted, ChangedBy = "admin", ChangedAt = now }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(allLogs);

        // Act
        var result = await _service.GetRecentAuditLogsAsync(count);

        // Assert
        result.Should().HaveCount(3);
        result.First().Id.Should().Be(3); // Most recent first
    }

    [Fact]
    public async Task GetRecentAuditLogsAsync_WithInvalidCount_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetRecentAuditLogsAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetRecentAuditLogsAsync(-1));
    }

    [Fact]
    public async Task GetRecentAuditLogsAsync_WhenRepositoryThrows_ThrowsFeatureFlagDataException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<FeatureFlagDataException>(() => _service.GetRecentAuditLogsAsync(5));
    }
}
