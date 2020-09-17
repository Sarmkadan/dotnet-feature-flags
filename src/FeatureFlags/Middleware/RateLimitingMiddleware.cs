#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Net;

namespace FeatureFlags.Middleware;

/// <summary>
/// Rate limiting middleware that restricts the number of requests per IP address within a time window.
/// Uses a sliding window approach to prevent API abuse and ensure fair resource usage across clients.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitOptions _options;
    private readonly ConcurrentDictionary<string, RequestHistory> _requestHistory;

    public RateLimitingMiddleware(RequestDelegate next, RateLimitOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _requestHistory = new ConcurrentDictionary<string, RequestHistory>();

        // Cleanup old entries periodically
        _ = Task.Run(async () => await CleanupExpiredEntriesAsync());
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);

        // Check rate limit
        if (!IsRequestAllowed(clientId))
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers.Add("Retry-After", _options.WindowSeconds.ToString());
            context.Response.Headers.Add("X-RateLimit-Limit", _options.MaxRequests.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining", "0");

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                retryAfter = _options.WindowSeconds
            });

            return;
        }

        RecordRequest(clientId);

        // Add rate limit headers to response
        var remaining = GetRemainingRequests(clientId);
        context.Response.Headers.Add("X-RateLimit-Limit", _options.MaxRequests.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", remaining.ToString());
        context.Response.Headers.Add("X-RateLimit-Reset", GetResetTime(clientId).ToString());

        await _next(context);
    }

    private bool IsRequestAllowed(string clientId)
    {
        if (!_requestHistory.TryGetValue(clientId, out var history))
        {
            return true;
        }

        // Remove expired requests outside the window
        var cutoffTime = DateTime.UtcNow.AddSeconds(-_options.WindowSeconds);
        history.Timestamps = new Queue<DateTime>(history.Timestamps.Where(t => t > cutoffTime));

        return history.Timestamps.Count < _options.MaxRequests;
    }

    private void RecordRequest(string clientId)
    {
        _requestHistory.AddOrUpdate(clientId,
            new RequestHistory { Timestamps = new Queue<DateTime> { DateTime.UtcNow } },
            (_, history) =>
            {
                history.Timestamps.Enqueue(DateTime.UtcNow);
                history.LastAccessTime = DateTime.UtcNow;
                return history;
            });
    }

    private int GetRemainingRequests(string clientId)
    {
        if (!_requestHistory.TryGetValue(clientId, out var history))
        {
            return _options.MaxRequests;
        }

        var cutoffTime = DateTime.UtcNow.AddSeconds(-_options.WindowSeconds);
        var validRequests = history.Timestamps.Count(t => t > cutoffTime);

        return Math.Max(0, _options.MaxRequests - validRequests);
    }

    private long GetResetTime(string clientId)
    {
        if (!_requestHistory.TryGetValue(clientId, out var history) || history.Timestamps.Count == 0)
        {
            return 0;
        }

        var oldestRequest = history.Timestamps.Peek();
        var resetTime = oldestRequest.AddSeconds(_options.WindowSeconds);

        return (long)Math.Max(0, (resetTime - DateTime.UtcNow).TotalSeconds);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID first, fallback to IP
        var userId = context.User?.FindFirst("sub")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }

    private async Task CleanupExpiredEntriesAsync()
    {
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5));

                var cutoffTime = DateTime.UtcNow.AddMinutes(-10);
                var expiredKeys = _requestHistory
                    .Where(kvp => kvp.Value.LastAccessTime < cutoffTime)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _requestHistory.TryRemove(key, out _);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private class RequestHistory
    {
        public Queue<DateTime> Timestamps { get; set; } = new();
        public DateTime LastAccessTime { get; set; } = DateTime.UtcNow;
    }
}

/// <summary>
/// Configuration options for rate limiting.
/// </summary>
public class RateLimitOptions
{
    public int MaxRequests { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
}
