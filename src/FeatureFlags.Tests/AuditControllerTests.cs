using FeatureFlags.Controllers;
using FeatureFlags.Models;
using FeatureFlags.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FeatureFlags.Tests.Controllers;

public class AuditControllerTests
{
    private readonly Mock<IAuditLogService> _mockAuditLogService;
    private readonly Mock<IFeatureFlagService> _mockFeatureFlagService;
    private readonly Mock<ILogger<AuditController>> _mockLogger;
    private readonly AuditController _controller;

    public AuditControllerTests()
    {
        _mockAuditLogService = new Mock<IAuditLogService>();
        _mockFeatureFlagService = new Mock<IFeatureFlagService>();
        _mockLogger = new Mock<ILogger<AuditController>>();

        _controller = new AuditController(
            _mockAuditLogService.Object,
            _mockFeatureFlagService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task GetFlagAuditLog_ReturnsOk_WhenLogsFound()
    {
        // Arrange
        int flagId = 1;
        var logs = new List<AuditLog> { new AuditLog { FeatureFlagId = flagId } };
        _mockAuditLogService.Setup(s => s.GetAuditLogsPagedAsync(flagId, It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetFlagAuditLog(flagId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeOfType<PaginatedApiResponse<AuditLog>>();
    }

    [Fact]
    public async Task GetFlagAuditLog_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        _mockAuditLogService.Setup(s => s.GetAuditLogsPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetFlagAuditLog(1);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)result).StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetAuditLogsByUser_ReturnsOk_WithLogs()
    {
        // Arrange
        string username = "testuser";
        var logs = new List<AuditLog> { new AuditLog { ChangedBy = username } };
        _mockAuditLogService.Setup(s => s.GetAuditLogsByUserAsync(username))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetAuditLogsByUser(username);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAuditLogsByDateRange_ReturnsBadRequest_WhenStartDateAfterEndDate()
    {
        // Act
        var result = await _controller.GetAuditLogsByDateRange(DateTime.Now.AddDays(1), DateTime.Now);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetAuditSummary_ReturnsOk_WithSummaryData()
    {
        // Arrange
        var logs = new List<AuditLog> { new AuditLog { Action = Enums.AuditAction.Created } };
        _mockAuditLogService.Setup(s => s.GetChangeHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(logs);

        // Act
        var result = await _controller.GetAuditSummary();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
