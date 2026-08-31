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
public sealed class RateLimitingMiddleware : IDisposable
{
    private readonly RequestDelegate _next;
    private readonly RateLimitOptions _options;
    private readonly ConcurrentDictionary<string, RequestHistory> _requestHistory;
    private readonly CancellationTokenSource _cleanupCancellationTokenSource;
    private readonly Task _cleanupTask;
    private int _disposed;

    public RateLimitingMiddleware(RequestDelegate next, RateLimitOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _requestHistory = new ConcurrentDictionary<string, RequestHistory>();
        _cleanupCancellationTokenSource = new CancellationTokenSource();

        // Cleanup old entries periodically
        _cleanupTask = Task.Run(() => CleanupExpiredEntriesAsync(_cleanupCancellationTokenSource.Token));
    }

    public async Task InvokeAsync(HttpContext context, CancellationToken cancellationToken = default)
    {
        var clientId = GetClientIdentifier(context);

        // Check and record the request atomically for this client
        var rateLimitResult = TryRecordRequest(clientId);
        if (!rateLimitResult.IsAllowed)
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

        // Add rate limit headers to response
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-RateLimit-Limit"] = _options.MaxRequests.ToString(System.Globalization.CultureInfo.InvariantCulture);
            context.Response.Headers["X-RateLimit-Remaining"] = rateLimitResult.RemainingRequests.ToString(System.Globalization.CultureInfo.InvariantCulture);
            context.Response.Headers["X-RateLimit-Reset"] = rateLimitResult.ResetTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private RateLimitResult TryRecordRequest(string clientId)
    {
        var history = _requestHistory.GetOrAdd(clientId, _ => new RequestHistory());
        lock (history.SyncRoot)
        {
            var now = DateTime.UtcNow;
            var cutoffTime = now.AddSeconds(-_options.WindowSeconds);
            while (history.Timestamps.Count > 0 && history.Timestamps.Peek() <= cutoffTime)
            {
                history.Timestamps.Dequeue();
            }

            if (history.Timestamps.Count >= _options.MaxRequests)
            {
                return new RateLimitResult(false, 0, 0);
            }

            history.Timestamps.Enqueue(now);
            history.LastAccessTime = now;

            var remainingRequests = Math.Max(0, _options.MaxRequests - history.Timestamps.Count);
            var resetTime = history.Timestamps.Peek().AddSeconds(_options.WindowSeconds);
            var resetSeconds = (long)Math.Max(0, (resetTime - DateTime.UtcNow).TotalSeconds);
            return new RateLimitResult(true, remainingRequests, resetSeconds);
        }
    }

    /// <summary>
    /// Gets the rate limit options used by this middleware instance.
    /// </summary>
    /// <returns>The rate limit options.</returns>
    public RateLimitOptions GetRateLimitOptions()
    {
        return _options;
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

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _cleanupCancellationTokenSource.Cancel();
        try
        {
            _cleanupTask.GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            // Cancellation is expected during disposal.
        }
        finally
        {
            _cleanupCancellationTokenSource.Dispose();
        }
    }

    private async Task CleanupExpiredEntriesAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);

                var cutoffTime = DateTime.UtcNow.AddMinutes(-10);
                foreach (var entry in _requestHistory)
                {
                    lock (entry.Value.SyncRoot)
                    {
                        if (entry.Value.LastAccessTime < cutoffTime)
                        {
                            _requestHistory.TryRemove(entry);
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch
            {
                // A cleanup failure should not prevent future cleanup attempts.
            }
        }
    }

    private readonly record struct RateLimitResult(bool IsAllowed, int RemainingRequests, long ResetTime);

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
