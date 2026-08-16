#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Claims;
using FeatureFlags.Controllers;
using FeatureFlags.Integration;
using FeatureFlags.Services;
using FeatureFlags.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace FeatureFlags.Tests.Controllers;

public sealed class AdminControllerTests
{
    private readonly Mock<IWebhookService> _webhookServiceMock;
    private readonly Mock<IFeatureFlagService> _featureFlagServiceMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<AdminController>> _loggerMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _webhookServiceMock = new Mock<IWebhookService>();
        _featureFlagServiceMock = new Mock<IFeatureFlagService>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<AdminController>>();

        _controller = new AdminController(
            _webhookServiceMock.Object,
            _featureFlagServiceMock.Object,
            _cacheServiceMock.Object,
            _loggerMock.Object);

        // Setup user context
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("sub", "test-user") }));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
    }

    [Fact]
    public async Task RegisterWebhook_ValidRequest_ReturnsCreated()
    {
        var request = new RegisterWebhookRequest { Url = "https://example.com/webhook" };
        _webhookServiceMock.Setup(s => s.RegisterWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<WebhookEventType>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Webhook { Id = 1 });

        var result = await _controller.RegisterWebhook(request);

        result.Should().BeOfType<CreatedResult>();
    }

    [Fact]
    public async Task RegisterWebhook_InvalidUrl_ReturnsBadRequest()
    {
        var request = new RegisterWebhookRequest { Url = "invalid-url" };

        var result = await _controller.RegisterWebhook(request);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetWebhooks_ReturnsOk()
    {
        _webhookServiceMock.Setup(s => s.GetAllActiveWebhooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Webhook>());

        var result = await _controller.GetWebhooks();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task DeleteWebhook_ExistingWebhook_ReturnsNoContent()
    {
        _webhookServiceMock.Setup(s => s.DeleteWebhookAsync(1)).ReturnsAsync(true);

        var result = await _controller.DeleteWebhook(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteWebhook_NonExistingWebhook_ReturnsNotFound()
    {
        _webhookServiceMock.Setup(s => s.DeleteWebhookAsync(1)).ReturnsAsync(false);

        var result = await _controller.DeleteWebhook(1);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ClearCache_ReturnsNoContent()
    {
        var result = await _controller.ClearCache();

        result.Should().BeOfType<NoContentResult>();
        _cacheServiceMock.Verify(s => s.ClearAsync(), Times.Once);
    }

    [Fact]
    public void GetHealth_ReturnsOk()
    {
        var result = _controller.GetHealth();

        result.Should().BeOfType<OkObjectResult>();
    }
}
