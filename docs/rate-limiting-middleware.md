# Rate Limiting Middleware

This document describes the behavior and configuration of the `RateLimitingMiddleware` in the `FeatureFlags.Middleware` namespace.

## Overview

The `RateLimitingMiddleware` is an ASP.NET Core middleware component that implements rate limiting to protect APIs from abuse and ensure fair resource usage. It uses a sliding window algorithm to track requests per client (identified by user ID or IP address) and enforces configurable limits.

## Key Features

- **Sliding Window Algorithm**: Tracks requests in a rolling time window for more accurate rate limiting compared to fixed window approaches.
- **Per-Client Identification**: Identifies clients by user ID (from JWT "sub" claim) when available, falling back to IP address.
- **Thread-Safe**: Uses concurrent collections and locking to safely handle concurrent requests.
- **Automatic Cleanup**: Background task periodically removes stale entries to prevent memory leaks.
- **Standard HTTP Responses**: Returns HTTP 429 (Too Many Requests) with appropriate headers when limits are exceeded.
- **Informative Headers**: Adds rate limit information to successful responses (X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset).

## Configuration

The middleware is configured via the `RateLimitOptions` class:

```csharp
public sealed class RateLimitOptions
{
    private const int DefaultMaxRequests = 100;
    private const int DefaultWindowSeconds = 60;

    public int MaxRequests { get; set; } = DefaultMaxRequests;
    public int WindowSeconds { get; set; } = DefaultWindowSeconds;
}
```

### Default Values
- **MaxRequests**: 100 requests
- **WindowSeconds**: 60 seconds (1 minute)

### Example Configuration

In your `Program.cs` or `Startup.cs`:

```csharp
builder.Services.AddSingleton<RateLimitOptions>(options =>
{
    options.MaxRequests = 50;      // Allow 50 requests
    options.WindowSeconds = 30;    // per 30-second window
});

app.UseMiddleware<RateLimitingMiddleware>();
```

## How It Works

### Client Identification
1. Attempts to extract user ID from the `sub` claim in the JWT token (`context.User.FindFirst("sub")`)
2. If no user ID is available, falls back to the client's IP address (`context.Connection.RemoteIpAddress`)
3. Client identifiers are stored as strings in the format:
   - `user:{userId}` for authenticated users
   - `ip:{ipAddress}` for unauthenticated clients

### Request Processing
For each incoming request:
1. The middleware retrieves or creates a request history for the client
2. It removes timestamps older than the current window (sliding window approach)
3. If the request count exceeds `MaxRequests`:
   - Returns HTTP 429 (Too Many Requests)
   - Sets `Retry-After` header to the window size in seconds
   - Sets `X-RateLimit-Limit` and `X-RateLimit-Remaining` (0) headers
   - Returns a JSON error response: `{ "error": "Rate limit exceeded", "retryAfter": <windowSeconds> }`
4. If the request is allowed:
   - Records the current timestamp
   - Calculates remaining requests and reset time
   - Adds rate limit headers to the response via `OnStarting` callback:
     - `X-RateLimit-Limit`: Maximum requests allowed in the window
     - `X-RateLimit-Remaining`: Requests remaining in the current window
     - `X-RateLimit-Reset`: Seconds until the oldest request in the window expires
   - Calls the next middleware in the pipeline

### Cleanup Mechanism
- A background task runs every `CleanupIntervalMinutes` (default: 5 minutes)
- Removes client entries that haven't been accessed in `ExpiredEntryAgeMinutes` (default: 10 minutes)
- This prevents memory leaks from clients that are no longer making requests

## Response Headers

### When Rate Limited (HTTP 429)
- `Retry-After`: Number of seconds to wait before making another request
- `X-RateLimit-Limit`: Configured maximum requests per window
- `X-RateLimit-Remaining`: Always 0 when rate limited
- Response body: JSON with error message and retryAfter value

### Successful Requests (HTTP 2XX)
- `X-RateLimit-Limit`: Configured maximum requests per window
- `X-RateLimit-Remaining`: Number of requests remaining in the current window
- `X-RateLimit-Reset`: Seconds until the rate limit window resets

## Thread Safety
- Uses `ConcurrentDictionary` for storing client histories
- Each client's history is protected by a lock (`SyncRoot`) to ensure thread-safe operations on the timestamp queue
- The background cleanup task also respects these locks when accessing client histories

## Disposal
The middleware implements `IDisposable` to properly clean up resources:
- Cancels the background cleanup task
- Waits for the task to complete
- Disposes the cancellation token source

## Usage Notes
- The middleware should be placed early in the pipeline, typically after routing but before authentication/authorization if you want to limit based on IP for unauthenticated requests.
- For authenticated scenarios, placing it after authentication allows limiting by user ID.
- The sliding window approach provides smoother rate limiting compared to fixed window algorithms, preventing bursts at window boundaries.
- Consider adjusting `MaxRequests` and `WindowSeconds` based on your API's expected traffic patterns and protection needs.