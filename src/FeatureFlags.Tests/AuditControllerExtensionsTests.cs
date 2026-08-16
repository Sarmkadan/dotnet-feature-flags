using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FeatureFlags.Controllers;
using FeatureFlags.Models;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace FeatureFlags.Tests.Controllers;

public class AuditControllerExtensionsTests
{
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<IFeatureFlagService> _featureFlagServiceMock;
    private readonly Mock<ILogger<AuditController>> _loggerMock;
    private readonly AuditController _controller;

    public AuditControllerExtensionsTests()
    {
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _featureFlagServiceMock = new Mock<IFeatureFlagService>();
        _loggerMock = new Mock<ILogger<AuditController>>();
        _controller = new AuditController(_auditLogServiceMock.Object, _featureFlagServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetRecentActivity_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var logs = new List<AuditLog> { new AuditLog { Id = 1, FeatureFlagId = 1, Action = FeatureFlags.Enums.AuditAction.Enabled } };
        _auditLogServiceMock.Setup(s => s.GetChangeHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetRecentActivity();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetRecentActivity_Returns500_WhenExceptionThrown()
    {
        // Arrange
        _auditLogServiceMock.Setup(s => s.GetChangeHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Service failure"));

        // Act
        var result = await _controller.GetRecentActivity();

        // Assert
        result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetAuditLogsByAction_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var logs = new List<AuditLog> { new AuditLog { Id = 1, FeatureFlagId = 1, Action = FeatureFlags.Enums.AuditAction.Enabled } };
        _auditLogServiceMock.Setup(s => s.GetChangeHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetAuditLogsByAction(FeatureFlags.Enums.AuditAction.Enabled);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetFilteredAuditLogs_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var logs = new List<AuditLog> { new AuditLog { Id = 1, FeatureFlagId = 1, Action = FeatureFlags.Enums.AuditAction.Enabled } };
        _auditLogServiceMock.Setup(s => s.GetChangeHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetFilteredAuditLogs(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetMostRecentChanges_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var logs = new List<AuditLog> { new AuditLog { Id = 1, FeatureFlagId = 1, Action = FeatureFlags.Enums.AuditAction.Enabled } };
        _auditLogServiceMock.Setup(s => s.GetChangeHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetMostRecentChanges();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetEnhancedChangeHistory_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        _auditLogServiceMock.Setup(s => s.GetAuditLogsAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<AuditLog> { new AuditLog { Id = 1, FeatureFlagId = 1, Action = FeatureFlags.Enums.AuditAction.Enabled } });

        // Act
        var result = await _controller.GetEnhancedChangeHistory(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
