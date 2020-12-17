# RateLimitingMiddleware

Middleware that limits the number of requests a client can make within a sliding time window. It tracks request timestamps and blocks further requests once the configured maximum is exceeded, returning a 429 (Too Many Requests) response.

## API

### `public RateLimitingMiddleware(int maxRequests, int windowSeconds)`
**Purpose**  
Initializes a new instance of the middleware with the specified limits.

**Parameters**  
- `maxRequests`: The maximum number of requests allowed in the window. Must be greater than zero.  
- `windowSeconds`: The length of the sliding window in seconds. Must be greater than zero.

**Return value**  
None (constructor).

**Exceptions**  
- `ArgumentOutOfRangeException` if `maxRequests` ≤ 0 or `windowSeconds` ≤ 0.

### `public async Task InvokeAsync(HttpContext context, RequestDelegate next)`
**Purpose**  
Processes an incoming HTTP request, enforcing the rate limit, and either passes the request to the next middleware or returns a 429 response.

**Parameters**  
- `context`: The `HttpContext` for the current request.  
- `next`: The delegate representing the remaining middleware pipeline.

**Return value**  
A `Task` that completes when the request has been processed.

**Exceptions**  
- `ArgumentNullException` if `context` or `next` is `null`.

### `public Queue<DateTime> Timestamps`
**Purpose**  
Holds the timestamps of requests that have occurred within the current sliding window. The queue is purged of entries older than `WindowSeconds` on each request.

**Type**  
`Queue<DateTime>`

**Remarks**  
Accessed directly by the middleware logic; no parameters or return value.

### `public DateTime LastAccessTime`
**Purpose**  
Stores the UTC timestamp of the most recent request processed by this middleware instance.

**Type**  
`DateTime`

**Remarks**  
Updated on each request after the timestamp queue is pruned.

### `public int MaxRequests`
**Purpose**  
Defines the maximum number of requests permitted within the sliding window.

**Type**  
`int`

**Remarks**  
Set via the constructor and immutable for the lifetime of the middleware.

### `public int WindowSeconds`
**Purpose**  
Defines the duration (in seconds) of the sliding window used for rate limiting.

**Type**  
`int`

**Remarks**  
Set via the constructor and immutable for the lifetime of the middleware.

## Usage

### Registering the middleware in an ASP.NET Core pipeline
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Allow up to 10 requests per 60 seconds per client
    app.UseMiddleware<RateLimitingMiddleware>(10, 60);

    app.UseRouting();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}
```

### Customizing limits via dependency injection
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Options pattern example (if the project exposes options)
    services.Configure<RateLimitOptions>(opts =>
    {
        opts.MaxRequests = 5;
        opts.WindowSeconds = 30;
    });
}

public void Configure(IApplicationBuilder app)
{
    // Resolve options and pass them to the middleware
    var opts = app.ApplicationServices.GetRequiredService<IOptions<RateLimitOptions>>();
    app.UseMiddleware<RateLimitingMiddleware>(opts.Value.MaxRequests, opts.Value.WindowSeconds);

    // ... rest of pipeline
}
```

## Notes
- The middleware is **not thread‑safe** for concurrent requests because it directly manipulates a `Queue<DateTime>` without synchronization. In high‑traffic scenarios consider replacing the queue with a thread‑safe collection (e.g., `ConcurrentQueue<DateTime>`) or adding explicit locking around reads/writes.
- Timestamps are stored as local `DateTime` values; for consistency across time zones or when the server clock may change, use `DateTime.UtcNow` when enqueuing and comparing.
- If `MaxRequests` is set to a very large value, the queue may grow unbounded until old entries are purged; the purge logic removes entries older than `WindowSeconds` on each invocation.
- Setting `WindowSeconds` to a very small value (e.g., 0 or 1) may cause the window to reset almost every request, effectively allowing bursts of requests up to `MaxRequests` per call.
- The middleware does not differentiate clients by IP address, API key, or any other identifier; it applies a global limit across all callers. To implement per‑client limits, incorporate a store (e.g., `MemoryCache` or `IDistributedCache`) keyed by client identifier outside of this middleware.
