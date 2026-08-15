using FeatureFlags.Middleware;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using FluentAssertions;
using System.Security.Claims;

namespace FeatureFlags.Tests;

public class AuthenticationMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly AuthenticationOptions _options;
    private readonly AuthenticationMiddleware _middleware;

    public AuthenticationMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _options = new AuthenticationOptions
        {
            ValidApiKeys = new List<string> { "valid-key-1", "valid-key-2" }
        };
        _middleware = new AuthenticationMiddleware(_nextMock.Object, _options);
    }

    [Fact]
    public async Task InvokeAsync_PublicEndpoint_CallsNextDelegate()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/swagger";

        await _middleware.InvokeAsync(context);

        _nextMock.Verify(next => next(context), Times.Once);
        context.Response.StatusCode.Should().NotBe(401);
    }

    [Fact]
    public async Task InvokeAsync_NoKeyProvided_ReturnsUnauthorized()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/flag";

        await _middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
        _nextMock.Verify(next => next(context), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_InvalidKey_ReturnsUnauthorized()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/flag";
        context.Request.Headers["X-API-Key"] = "invalid-key";

        await _middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(401);
        _nextMock.Verify(next => next(context), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_ValidKeyHeader_SetsUserAndCallsNext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/flag";
        context.Request.Headers["X-API-Key"] = "valid-key-1";

        await _middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().NotBe(401);
        _nextMock.Verify(next => next(context), Times.Once);
        context.User.Identity.Should().NotBeNull();
        context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("valid-key-1");
    }

    [Fact]
    public async Task InvokeAsync_ValidKeyQuery_SetsUserAndCallsNext()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/flag";
        context.Request.QueryString = new QueryString("?api_key=valid-key-2");

        await _middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().NotBe(401);
        _nextMock.Verify(next => next(context), Times.Once);
        context.User.Identity.Should().NotBeNull();
        context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Should().Be("valid-key-2");
    }

    [Fact]
    public async Task InvokeAsync_NoValidApiKeysConfigured_AllowsAll()
    {
        var options = new AuthenticationOptions { ValidApiKeys = new List<string>() };
        var middleware = new AuthenticationMiddleware(_nextMock.Object, options);
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/flag";
        context.Request.Headers["X-API-Key"] = "any-key";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().NotBe(401);
        _nextMock.Verify(next => next(context), Times.Once);
    }
}
