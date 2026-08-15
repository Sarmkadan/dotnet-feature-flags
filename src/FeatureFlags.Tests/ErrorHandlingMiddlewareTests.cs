using FeatureFlags.Middleware;
using FeatureFlags.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using System.Net;

namespace FeatureFlags.Tests;

public class ErrorHandlingMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<ErrorHandlingMiddleware>> _loggerMock;
    private readonly ErrorHandlingMiddleware _middleware;

    public ErrorHandlingMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
        _middleware = new ErrorHandlingMiddleware(_nextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_NullNext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ErrorHandlingMiddleware(null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ErrorHandlingMiddleware(_nextMock.Object, null!));
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNext()
    {
        var context = new DefaultHttpContext();
        await _middleware.InvokeAsync(context);

        _nextMock.Verify(next => next(context), Times.Once);
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_FeatureFlagException_ReturnsBadRequest()
    {
        _nextMock.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(new FeatureFlagException("Flag error"));
        var context = new DefaultHttpContext();
        
        await _middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_KeyNotFoundException_ReturnsNotFound()
    {
        _nextMock.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(new KeyNotFoundException());
        var context = new DefaultHttpContext();
        
        await _middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task InvokeAsync_ArgumentException_ReturnsBadRequest()
    {
        _nextMock.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(new ArgumentException("Arg error"));
        var context = new DefaultHttpContext();
        
        await _middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_GenericException_ReturnsInternalServerError()
    {
        _nextMock.Setup(next => next(It.IsAny<HttpContext>())).ThrowsAsync(new Exception("Generic error"));
        var context = new DefaultHttpContext();
        
        await _middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
    }
}
