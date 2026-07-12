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
public sealed class RateLimitingMiddleware
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

    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        var clientId = GetClientIdentifier(context);

        // Check rate limit
        if (!IsRequestAllowed(clientId))
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.Headers["Retry-After"] = _options.WindowSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
            context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString(System.Globalization.CultureInfo.InvariantCulture);
            context.Response.Headers["X-RateLimit-Remaining"] = "0";

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
        context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString(System.Globalization.CultureInfo.InvariantCulture);
        context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString(System.Globalization.CultureInfo.InvariantCulture);
        context.Response.Headers["X-RateLimit-Reset"] = GetResetTime(clientId).ToString(System.Globalization.CultureInfo.InvariantCulture);

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
        lock (history.SyncRoot)
        {
            while (history.Timestamps.Count > 0 && history.Timestamps.Peek() <= cutoffTime)
            {
                history.Timestamps.Dequeue();
            }

            return history.Timestamps.Count < _options.MaxRequests;
        }
    }

    private void RecordRequest(string clientId)
    {
        var history = _requestHistory.GetOrAdd(clientId, _ => new RequestHistory());
        lock (history.SyncRoot)
        {
            history.Timestamps.Enqueue(DateTime.UtcNow);
            history.LastAccessTime = DateTime.UtcNow;
        }
    }

    private int GetRemainingRequests(string clientId)
    {
        if (!_requestHistory.TryGetValue(clientId, out var history))
        {
            return _options.MaxRequests;
        }

        var cutoffTime = DateTime.UtcNow.AddSeconds(-_options.WindowSeconds);
        int validRequests;
        lock (history.SyncRoot)
        {
            validRequests = history.Timestamps.Count(t => t > cutoffTime);
        }

        return Math.Max(0, _options.MaxRequests - validRequests);
    }

    private long GetResetTime(string clientId)
    {
        if (!_requestHistory.TryGetValue(clientId, out var history) || history.Timestamps.Count == 0)
        {
            return 0;
        }

        DateTime oldestRequest;
        lock (history.SyncRoot)
        {
            if (history.Timestamps.Count == 0)
            {
                return 0;
            }

            oldestRequest = history.Timestamps.Peek();
        }

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

    private sealed class RequestHistory
    {
        public object SyncRoot { get; } = new();
        public Queue<DateTime> Timestamps { get; } = new();
        public DateTime LastAccessTime { get; set; } = DateTime.UtcNow;
    }
}

/// <summary>
/// Configuration options for rate limiting.
/// </summary>
public sealed class RateLimitOptions
{
    public int MaxRequests { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
}
