# Authentication Middleware

This document describes the `AuthenticationMiddleware` class located in `src/FeatureFlags/Middleware/AuthenticationMiddleware.cs`.

## Overview

The `AuthenticationMiddleware` is an ASP.NET Core middleware component that provides API key authentication for HTTP requests. It validates incoming requests for the presence of a valid API key, which can be supplied either via the `X-API-Key` header or the `api_key` query parameter.

The middleware is designed to be simple and flexible, allowing for easy integration into ASP.NET Core applications. It supports skipping authentication for predefined public endpoints (such as Swagger UI, health checks, and metrics) and sets up a user principal upon successful validation.

## How It Works

### Request Processing

When a request passes through the middleware, the following steps occur:

1. **Public Endpoint Check**: The middleware first checks if the request path matches any of the configured public endpoints (`/swagger`, `/health`, `/metrics`). If it does, the request is allowed to proceed without authentication.

2. **API Key Extraction**: For non-public endpoints, the middleware attempts to extract an API key from the request:
   - First, it looks for the `X-API-Key` header.
   - If not found, it falls back to the `api_key` query parameter.

3. **API Key Validation**: The extracted API key is validated against a list of valid API keys provided via the `AuthenticationOptions`:
   - If the list of valid API keys is empty, validation is skipped and the request is allowed (this effectively disables authentication).
   - Otherwise, the key must exactly match one of the keys in the valid list.

4. **Authentication Failure**: If the API key is missing or invalid, the middleware returns an HTTP 401 Unauthorized response with a JSON body: `{ "error": "Unauthorized: Invalid or missing API key" }`.

5. **Successful Authentication**: Upon successful validation, the middleware creates a `ClaimsPrincipal` with the following claims:
   - `ClaimTypes.NameIdentifier`: The API key value
   - `ClaimTypes.Name`: Set to "API Client"
   - A custom claim `"ApiKey"`: The API key value
   This principal is then assigned to `HttpContext.User`, making it available to downstream components.

6. **Request Continuation**: After setting the user principal, the middleware calls the next delegate in the pipeline to continue processing the request.

### Security Notes

- The middleware performs a simple string comparison for API key validation. In production environments, consider using a secure comparison method to prevent timing attacks if this becomes a concern.
- The middleware does not hash or encrypt API keys; it is the responsibility of the application to store and manage API keys securely.
- The `RequireApiKey` property in `AuthenticationOptions` is currently not used by the middleware logic. Authentication is required unless the valid API keys list is empty.

## Configuration

The middleware is configured via the `AuthenticationOptions` class:

```csharp
public sealed class AuthenticationOptions
{
    public List<string> ValidApiKeys { get; set; } = new();
    public bool RequireApiKey { get; set; } = true;
}
```

### ValidApiKeys

- A list of strings representing valid API keys.
- If this list is empty, the middleware will allow all requests (authentication is effectively disabled).
- Example: `new List<string> { "key1", "key2", "key3" }`

### RequireApiKey

- A boolean flag indicating whether API key authentication is required.
- **Note**: This property is not currently used in the middleware's validation logic. The middleware only checks if `ValidApiKeys` is empty to bypass authentication. To enforce authentication when keys are configured, ensure `ValidApiKeys` is non-empty.

## Usage

To use the middleware in an ASP.NET Core application, register it in the middleware pipeline, typically in `Program.cs` or `Startup.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configure authentication options
builder.Services.Configure<AuthenticationOptions>(options =>
{
    options.ValidApiKeys = new List<string> { "your-api-key-here" };
    // options.RequireApiKey = true; // Not used by middleware
});

// Add the middleware to the pipeline
var app = builder.Build();

app.UseMiddleware<AuthenticationMiddleware>();

// Other middleware and endpoint definitions
app.MapControllers();

app.Run();
```

### Dependency Injection

The middleware expects `RequestDelegate` and `AuthenticationOptions` to be provided via dependency injection. When using `app.UseMiddleware<AuthenticationMiddleware>()`, ASP.NET Core will automatically resolve these dependencies.

### Alternative Registration

You can also register the middleware using extension methods for cleaner code:

```csharp
public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(
        this IApplicationBuilder builder,
        Action<AuthenticationOptions>? setupAction = null)
    {
        if (setupAction != null)
        {
            builder.Services.Configure(setupAction);
        }

        return builder.UseMiddleware<AuthenticationMiddleware>();
    }
}

// Usage:
app.UseApiKeyAuthentication(options =>
{
    options.ValidApiKeys = new List<string> { "key1", "key2" };
});
```

## Public Endpoints

By default, the middleware skips authentication for the following paths:
- `/swagger` (and any sub-paths, e.g., `/swagger/index.html`)
- `/health` (and any sub-paths)
- `/metrics` (and any sub-paths)

These paths are checked using `PathString.StartsWithSegments` with case-insensitive comparison.

To modify the public endpoints, you would need to modify the `IsPublicEndpoint` method in the middleware source code.

## Error Responses

When authentication fails, the middleware returns:
- **Status Code**: 401 Unauthorized
- **Content-Type**: application/json
- **Body**: `{ "error": "Unauthorized: Invalid or missing API key" }`

## Claims Principal

Upon successful authentication, the following claims are added to the user's identity:
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier`: The API key value
- `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name`: "API Client"
- `ApiKey`: The API key value (custom claim)

These claims can be accessed in controllers or other middleware via `User.Claims` or `User.Identity.Name`.

## Extensibility

The middleware is designed to be a starting point for API key authentication. For more advanced scenarios (such as OAuth2, JWT, or integration with ASP.NET Core's authentication system), consider extending or replacing this middleware with the built-in authentication middleware provided by ASP.NET Core.

## Notes

- The middleware does not perform any validation on the format of the API key (e.g., length, character set). It is assumed that the API keys are managed securely elsewhere.
- The middleware is sealed and cannot be inherited. To modify its behavior, you would need to copy and modify the source code or create a wrapper middleware.
- The middleware is thread-safe and does not maintain any state between requests.