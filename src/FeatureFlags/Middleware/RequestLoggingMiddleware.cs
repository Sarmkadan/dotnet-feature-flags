#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Text;
using Serilog;
using Serilog.Context;

namespace FeatureFlags.Middleware;

/// <summary>
/// Middleware that logs all HTTP requests and responses, including request body, response status, and execution time.
/// Provides comprehensive observability for API calls and helps identify performance issues.
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = context.TraceIdentifier;

        // Log incoming request
        var request = context.Request;
        var requestBody = await ReadRequestBodyAsync(request);

        Log.Information("Incoming {Method} {Path} [{RequestId}]",
            request.Method,
            request.Path,
            requestId);

        if (!string.IsNullOrEmpty(requestBody) && requestBody.Length < 1024)
        {
            Log.Debug("Request body: {Body}", requestBody);
        }

        // Capture response for logging
        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        try
        {
            await _next(context);

            stopwatch.Stop();

            // Log response
            var responseBody = await ReadResponseBodyAsync(context);

            Log.Information(
                "Completed {Method} {Path} - {StatusCode} in {ElapsedMilliseconds}ms [{RequestId}]",
                request.Method,
                request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                requestId);

            if (!string.IsNullOrEmpty(responseBody) && responseBody.Length < 1024 && context.Response.StatusCode >= 400)
            {
                Log.Debug("Error response body: {Body}", responseBody);
            }

            // Copy response to original stream
            await responseBodyStream.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
    {
        // Only read body for methods that typically have one
        if (request.Method != HttpMethods.Post && request.Method != HttpMethods.Put && request.Method != HttpMethods.Patch)
        {
            return string.Empty;
        }

        request.EnableBuffering();

        using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        request.Body.Position = 0;

        return body;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();

        context.Response.Body.Seek(0, SeekOrigin.Begin);

        return body;
    }
}
