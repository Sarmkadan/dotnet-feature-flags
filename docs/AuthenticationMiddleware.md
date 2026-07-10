# AuthenticationMiddleware

ASP.NET Core middleware that enforces API key authentication on incoming HTTP requests. It inspects the request for a valid API key and either allows the request to proceed to the next middleware in the pipeline or short-circuits with a 401 Unauthorized response. The middleware supports an optional mode where authentication can be disabled entirely, as well as a configurable list of accepted API keys.

## API

### `AuthenticationMiddleware`

```csharp
public AuthenticationMiddleware(RequestDelegate next, List<string> validApiKeys, bool requireApiKey)
```

Constructs the middleware instance.

- **Parameters**
  - `next` — The next `RequestDelegate` in the pipeline. Invoked when authentication succeeds or when `requireApiKey` is `false`.
  - `validApiKeys` — The list of API key strings that are considered valid. Must not be `null`; an empty list means no keys will match.
  - `requireApiKey` — When `true`, a valid API key must be present in the request. When `false`, all requests pass through without authentication.
- **Throws**
  - `ArgumentNullException` if `validApiKeys` is `null`.

### `InvokeAsync`

```csharp
public async Task InvokeAsync(HttpContext context)
```

Processes an HTTP request for API key authentication.

- **Parameters**
  - `context` — The `HttpContext` for the current request.
- **Returns**
  - A `Task` representing the asynchronous operation.
- **Behavior**
  - If `RequireApiKey` is `false`, immediately calls the next middleware.
  - Otherwise, extracts the API key from the request (typically from a header or query string) and checks it against `ValidApiKeys`.
  - On a match, calls the next middleware.
  - On a mismatch or missing key, sets the response status code to `401` and writes an "Unauthorized" message to the response body. The next middleware is **not** invoked.
- **Throws**
  - No documented exceptions are thrown directly by this method. Exceptions from the next middleware propagate normally.

### `ValidApiKeys`

```csharp
public List<string> ValidApiKeys { get; }
```

The list of API key strings accepted by this middleware instance. Set during construction and immutable thereafter. An empty list causes all requests to be rejected when `RequireApiKey` is `true`.

### `RequireApiKey`

```csharp
public bool RequireApiKey { get; }
```

Indicates whether API key authentication is enforced. When `false`, the middleware acts as a pass-through regardless of the request contents or the values in `ValidApiKeys`.

## Usage

### Example 1: Required API key with multiple valid keys

```csharp
var validKeys = new List<string> { "prod-key-123", "prod-key-456" };
var middleware = new AuthenticationMiddleware(
    next: async (ctx) => await ctx.Response.WriteAsync("Hello, world!"),
    validApiKeys: validKeys,
    requireApiKey: true
);

// In a request pipeline:
// GET / HTTP/1.1
// X-Api-Key: prod-key-123
// -> 200 OK, body: "Hello, world!"
//
// GET / HTTP/1.1
// X-Api-Key: wrong-key
// -> 401 Unauthorized
```

### Example 2: Authentication disabled (pass-through mode)

```csharp
var middleware = new AuthenticationMiddleware(
    next: async (ctx) => await ctx.Response.WriteAsync("Public content"),
    validApiKeys: new List<string>(),
    requireApiKey: false
);

// All requests proceed regardless of headers:
// GET / HTTP/1.1
// -> 200 OK, body: "Public content"
```

## Notes

- **Key extraction mechanism**: The exact source of the API key (header name, query parameter) is an implementation detail of `InvokeAsync`. Consumers should refer to the project's conventions or configuration to supply keys correctly.
- **Empty `ValidApiKeys` with `RequireApiKey = true`**: All requests will be rejected with 401, since no key can ever match. This is a valid configuration for temporarily locking down an endpoint.
- **Thread safety**: `ValidApiKeys` and `RequireApiKey` are set at construction and treated as read-only during request processing. The middleware instance is safe to use concurrently across multiple requests. If the list of valid keys needs to change at runtime, replace the middleware instance rather than modifying the list in place.
- **Short-circuiting**: When authentication fails, the response is written and the pipeline is terminated. No downstream middleware executes, and no further modifications to the response should be attempted by upstream components after the middleware completes.
- **Ordering**: This middleware should be placed early in the pipeline, before any middleware that performs sensitive operations or returns protected data.
