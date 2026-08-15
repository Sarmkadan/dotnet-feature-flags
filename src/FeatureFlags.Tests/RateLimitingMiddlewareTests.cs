using FeatureFlags.Middleware;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using FluentAssertions;
using System.Security.Claims;
using System.Net;

namespace FeatureFlags.Tests;

public class RateLimitingMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly RateLimitOptions _options;
    private readonly RateLimitingMiddleware _middleware;

    public RateLimitingMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _options = new RateLimitOptions
        {
            MaxRequests = 2,
            WindowSeconds = 5
        };
        _middleware = new RateLimitingMiddleware(_nextMock.Object, _options);
    }

    [Fact]
    public void Constructor_NullNext_ThrowsArgumentNullException()
    {
        Action act = () => new RateLimitingMiddleware(null!, _options);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Action act = () => new RateLimitingMiddleware(_nextMock.Object, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void GetRateLimitOptions_ReturnsCorrectOptions()
    {
        var options = _middleware.GetRateLimitOptions();
        options.Should().Be(_options);
    }

    [Fact]
    public async Task InvokeAsync_WithinLimit_CallsNext()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        await _middleware.InvokeAsync(context);

        _nextMock.Verify(next => next(context), Times.Once);
        context.Response.StatusCode.Should().NotBe(429);
    }

    [Fact]
    public async Task InvokeAsync_ExceedsLimit_Returns429()
    {
        var ip = IPAddress.Parse("192.168.1.1");
        var context1 = new DefaultHttpContext();
        context1.Connection.RemoteIpAddress = ip;
        var context2 = new DefaultHttpContext();
        context2.Connection.RemoteIpAddress = ip;
        var context3 = new DefaultHttpContext();
        context3.Connection.RemoteIpAddress = ip;

        await _middleware.InvokeAsync(context1);
        await _middleware.InvokeAsync(context2);
        await _middleware.InvokeAsync(context3);

        context3.Response.StatusCode.Should().Be(429);
        _nextMock.Verify(next => next(context3), Times.Never);
    }

    [Fact]
    public async Task InvokeAsync_IdentifiesByUser_WhenSubClaimExists()
    {
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim("sub", "user123")
        }));

        await _middleware.InvokeAsync(context);

        _nextMock.Verify(next => next(context), Times.Once);
        context.Response.StatusCode.Should().NotBe(429);
    }
}
