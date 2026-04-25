#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Security.Claims;
using System.Text;

namespace FeatureFlags.Middleware;

/// <summary>
/// API key authentication middleware that validates incoming requests contain valid API keys.
/// Supports both header-based (X-API-Key) and query parameter-based API keys for flexibility.
/// Designed to work with simple API key validation and can be extended for OAuth2/JWT.
/// </summary>
public sealed class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AuthenticationOptions _options;

    public AuthenticationMiddleware(RequestDelegate next, AuthenticationOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authentication for swagger and health check endpoints
        if (IsPublicEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var apiKey = ExtractApiKey(context.Request);

        if (string.IsNullOrEmpty(apiKey) || !ValidateApiKey(apiKey))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized: Invalid or missing API key" });
            return;
        }

        // Set up user principal from API key
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, apiKey),
            new Claim(ClaimTypes.Name, "API Client"),
            new Claim("ApiKey", apiKey)
        };

        var identity = new ClaimsIdentity(claims, "ApiKey");
        var principal = new ClaimsPrincipal(identity);

        context.User = principal;

        await _next(context);
    }

    private bool IsPublicEndpoint(PathString path)
    {
        var publicPaths = new[] { "/swagger", "/health", "/metrics" };
        return publicPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }

    private string? ExtractApiKey(HttpRequest request)
    {
        // Try header first
        if (request.Headers.TryGetValue("X-API-Key", out var headerValue))
        {
            return headerValue.ToString();
        }

        // Fall back to query parameter
        request.Query.TryGetValue("api_key", out var queryValue);
        return queryValue.ToString();
    }

    private bool ValidateApiKey(string apiKey)
    {
        // If no validation keys configured, allow all
        if (!_options.ValidApiKeys.Any())
        {
            return true;
        }

        return _options.ValidApiKeys.Contains(apiKey);
    }
}

/// <summary>
/// Configuration options for API key authentication.
/// </summary>
public sealed class AuthenticationOptions
{
    public List<string> ValidApiKeys { get; set; } = new();
    public bool RequireApiKey { get; set; } = true;
}
