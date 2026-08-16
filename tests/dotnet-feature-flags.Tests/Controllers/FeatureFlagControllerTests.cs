#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using FeatureFlags.Controllers;
using FeatureFlags.Models;
using FeatureFlags.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FeatureFlags.Tests.Controllers;

public sealed class FeatureFlagControllerTests
{
    private readonly FeatureFlagController _controller;
    private readonly Mock<IFeatureFlagService> _featureFlagServiceMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<ILogger<FeatureFlagController>> _loggerMock;

    public FeatureFlagControllerTests()
    {
        _featureFlagServiceMock = new Mock<IFeatureFlagService>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _loggerMock = new Mock<ILogger<FeatureFlagController>>();

        _controller = new FeatureFlagController(
            _featureFlagServiceMock.Object,
            _auditLogServiceMock.Object,
            _loggerMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "testuser"),
        }, "mock"));

        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user }
        };
    }

    [Fact]
    public async Task EvaluateFeatureFlag_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new EvaluationRequest { FeatureFlagKey = "test-flag", UserId = "user1", Email = "user@test.com" };
        _featureFlagServiceMock
            .Setup(s => s.IsEnabledAsync(request.FeatureFlagKey, It.IsAny<UserContext>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.EvaluateFeatureFlag(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task EvaluateFeatureFlag_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.EvaluateFeatureFlag(null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetByKey_WithValidKey_ReturnsOk()
    {
        // Arrange
        var key = "test-flag";
        var flag = new FeatureFlag { Id = 1, Key = key };
        _featureFlagServiceMock
            .Setup(s => s.GetFeatureFlagByKeyAsync(key))
            .ReturnsAsync(flag);

        // Act
        var result = await _controller.GetByKey(key);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetByKey_WithNonExistentKey_ReturnsNotFound()
    {
        // Arrange
        _featureFlagServiceMock
            .Setup(s => s.GetFeatureFlagByKeyAsync("missing"))
            .ReturnsAsync((FeatureFlag?)null);

        // Act
        var result = await _controller.GetByKey("missing");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WithValidFeatureFlag_ReturnsCreatedAtAction()
    {
        // Arrange
        var flag = new FeatureFlag { Key = "new-flag", DisplayName = "New Flag" };
        _featureFlagServiceMock
            .Setup(s => s.CreateFeatureFlagAsync(flag, It.IsAny<string>()))
            .ReturnsAsync(flag);

        // Act
        var result = await _controller.Create(flag);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Enable_CallsService()
    {
        // Act
        var result = await _controller.Enable(1);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _featureFlagServiceMock.Verify(s => s.EnableFeatureFlagAsync(1, It.IsAny<string>()), Times.Once);
    }
}
