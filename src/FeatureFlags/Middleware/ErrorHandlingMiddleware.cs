#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Net;
using System.Text.Json;
using FeatureFlags.Exceptions;
using Serilog;

namespace FeatureFlags.Middleware;

/// <summary>
/// Global exception handler middleware that catches all unhandled exceptions and returns standardized error responses.
/// Logs exceptions at appropriate levels and ensures consistent error response format across the API.
/// </summary>
public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private const int BadRequestStatusCode = (int)HttpStatusCode.BadRequest;
    private const string ResourceNotFoundMessage = "Resource not found";
    private const string UnexpectedErrorMessage = "An unexpected error occurred";
    private const string NotFoundErrorCode = "NotFound";
    private const string ValidationErrorCode = "ValidationError";
    private const string InternalServerErrorCode = "InternalServerError";

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("InvokeAsync started for request {RequestPath} {RequestMethod}",
            context.Request.Path, context.Request.Method);

        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception caught in InvokeAsync for request {RequestPath} {RequestMethod}",
                context.Request.Path, context.Request.Method);
            await HandleExceptionAsync(context, exception);
        }

        _logger.LogInformation("InvokeAsync completed for request {RequestPath} {RequestMethod}",
            context.Request.Path, context.Request.Method);
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        // Categorize exceptions to return appropriate status codes
        if (exception is FeatureFlagException ffEx)
        {
            context.Response.StatusCode = BadRequestStatusCode;
            response.StatusCode = BadRequestStatusCode;
            response.Message = ffEx.Message;
            response.ErrorCode = ffEx.GetType().Name;
        }
        else if (exception is KeyNotFoundException)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.Message = ResourceNotFoundMessage;
            response.ErrorCode = NotFoundErrorCode;
        }
        else if (exception is ArgumentException argEx)
        {
            context.Response.StatusCode = BadRequestStatusCode;
            response.StatusCode = BadRequestStatusCode;
            response.Message = argEx.Message;
            response.ErrorCode = ValidationErrorCode;
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.Message = UnexpectedErrorMessage;
            response.ErrorCode = InternalServerErrorCode;

            Log.Warning("Unhandled exception fell back to generic InternalServerError response for exception type {ExceptionType}",
                exception.GetType().Name);
        }

        Log.Error(exception, "Unhandled exception: {ErrorCode} - {Message}",
            response.ErrorCode, response.Message);

        return context.Response.WriteAsJsonAsync(response);
    }

    /// <summary>
    /// Standardized error response format returned by the API.
    /// </summary>
    private class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public override string ToString() => $"ErrorResponse {{ StatusCode = {StatusCode}, Message = {Message}, ErrorCode = {ErrorCode}, Timestamp = {Timestamp} }}";
    }
}
