// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using FeatureFlags.Models;
using FeatureFlags.Services;

/// <summary>
/// Example: Integrating feature flags into ASP.NET Core middleware and controllers.
/// This demonstrates real-world usage patterns in a production application.
/// </summary>

// Middleware for feature flag based routing
public class FeatureFlagMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IFeatureFlagService _featureFlagService;

    public FeatureFlagMiddleware(RequestDelegate next, IFeatureFlagService featureFlagService)
    {
        _next = next;
        _featureFlagService = featureFlagService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add user context to HttpContext for use in handlers
        var userContext = ExtractUserContext(context);
        context.Items["UserContext"] = userContext;

        // Check if route is behind a feature flag
        if (IsFeatureFlagRoute(context.Request.Path))
        {
            var flagKey = ExtractFlagKeyFromRoute(context.Request.Path);
            var isEnabled = await _featureFlagService.IsEnabledAsync(flagKey, userContext);

            if (!isEnabled)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                await context.Response.WriteAsync($"Feature '{flagKey}' is not available");
                return;
            }
        }

        await _next(context);
    }

    private UserContext ExtractUserContext(HttpContext context)
    {
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty;

        return new UserContext
        {
            UserId = userId,
            Email = email
        };
    }

    private bool IsFeatureFlagRoute(PathString path)
    {
        // Check if path contains a feature flag marker
        return path.Value?.StartsWith("/ff-") ?? false;
    }

    private string ExtractFlagKeyFromRoute(PathString path)
    {
        // Extract flag key from path like /ff-new-checkout
        var parts = path.Value?.Substring(4).Split('/');
        return parts?[0] ?? "";
    }
}

// Controller using feature flags
public class FeatureFlagEnabledControllerExample
{
    private readonly IFeatureFlagService _featureFlagService;
    private readonly IServiceProvider _serviceProvider;

    public FeatureFlagEnabledControllerExample(
        IFeatureFlagService featureFlagService,
        IServiceProvider serviceProvider)
    {
        _featureFlagService = featureFlagService;
        _serviceProvider = serviceProvider;
    }

    public async Task HandleRequestAsync(HttpContext context)
    {
        var userContext = (UserContext)context.Items["UserContext"]!;

        // Route to different implementations based on feature flags
        var response = context.Request.Path.Value switch
        {
            "/api/checkout" => await HandleCheckoutAsync(userContext),
            "/api/products" => await HandleProductsAsync(userContext),
            "/api/recommendations" => await HandleRecommendationsAsync(userContext),
            _ => "Not Found"
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { data = response });
    }

    private async Task<string> HandleCheckoutAsync(UserContext userContext)
    {
        // Check which checkout implementation to use
        var useNewCheckout = await _featureFlagService.IsEnabledAsync("new-checkout", userContext);

        if (useNewCheckout)
        {
            return "Using new checkout flow with AI-powered order summary";
        }
        else
        {
            return "Using legacy checkout flow";
        }
    }

    private async Task<string> HandleProductsAsync(UserContext userContext)
    {
        // Check for different product list features
        var hasFacets = await _featureFlagService.IsEnabledAsync("product-facets", userContext);
        var hasQuickView = await _featureFlagService.IsEnabledAsync("product-quick-view", userContext);

        var features = new[]
        {
            hasFacets ? "✓ Product facets" : "✗ Product facets",
            hasQuickView ? "✓ Quick view" : "✗ Quick view"
        };

        return $"Features: {string.Join(", ", features)}";
    }

    private async Task<string> HandleRecommendationsAsync(UserContext userContext)
    {
        // Get A/B test variant for recommendations
        var variant = await _featureFlagService.GetVariantAsync("recommendations-algorithm", userContext);

        return $"Using recommendation algorithm: {variant.Name}";
    }
}

// Usage in Program.cs
public static class FeatureFlagsStartupConfiguration
{
    public static void ConfigureFeatureFlags(this WebApplicationBuilder builder)
    {
        // Add FeatureFlags services (already done in DI setup)
        // builder.Services.AddFeatureFlags(builder.Configuration);

        // Register middleware
        builder.Services.AddScoped<FeatureFlagMiddleware>();
    }

    public static void UseFeatureFlags(this WebApplication app)
    {
        // Use the middleware
        app.UseMiddleware<FeatureFlagMiddleware>();
    }
}

// Example: Feature flag-based caching middleware
public class FeatureFlagCachingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IFeatureFlagService _featureFlagService;

    public FeatureFlagCachingMiddleware(RequestDelegate next, IFeatureFlagService featureFlagService)
    {
        _next = next;
        _featureFlagService = featureFlagService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userContext = (UserContext?)context.Items["UserContext"];
        if (userContext == null)
        {
            await _next(context);
            return;
        }

        // Check if aggressive caching is enabled for this user
        var useAggressiveCaching = await _featureFlagService
            .IsEnabledAsync("aggressive-http-caching", userContext);

        if (useAggressiveCaching)
        {
            context.Response.Headers.CacheControl = "public, max-age=3600";
        }
        else
        {
            context.Response.Headers.CacheControl = "public, max-age=60";
        }

        await _next(context);
    }
}

// Example: Feature flag-based rate limiting
public class FeatureFlagRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IFeatureFlagService _featureFlagService;

    public FeatureFlagRateLimitMiddleware(RequestDelegate next, IFeatureFlagService featureFlagService)
    {
        _next = next;
        _featureFlagService = featureFlagService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userContext = (UserContext?)context.Items["UserContext"];
        if (userContext == null)
        {
            await _next(context);
            return;
        }

        // Check if user should have relaxed rate limits (e.g., premium users)
        var hasRelaxedRateLimits = await _featureFlagService
            .IsEnabledAsync("relaxed-rate-limits-for-premium", userContext);

        var rateLimitKey = hasRelaxedRateLimits ? "premium-user" : "standard-user";
        context.Items["RateLimitKey"] = rateLimitKey;

        await _next(context);
    }
}

// Example: Using feature flags in a service class
public class ProductService
{
    private readonly IFeatureFlagService _featureFlagService;

    public ProductService(IFeatureFlagService featureFlagService)
    {
        _featureFlagService = featureFlagService;
    }

    public async Task<Product> GetProductAsync(int id, UserContext userContext)
    {
        var product = new Product { Id = id, Name = "Sample Product" };

        // Add advanced pricing for certain users
        var hasAdvancedPricing = await _featureFlagService
            .IsEnabledAsync("advanced-pricing", userContext);
        if (hasAdvancedPricing)
        {
            product.Price = CalculateAdvancedPrice(product.BasePrice, userContext);
        }
        else
        {
            product.Price = product.BasePrice;
        }

        // Add recommendations if enabled
        var hasRecommendations = await _featureFlagService
            .IsEnabledAsync("product-recommendations", userContext);
        if (hasRecommendations)
        {
            product.Recommendations = await GetRecommendationsAsync(id);
        }

        return product;
    }

    private decimal CalculateAdvancedPrice(decimal basePrice, UserContext context)
    {
        // Complex pricing logic
        return basePrice * 0.9m; // 10% discount for demo
    }

    private async Task<string[]> GetRecommendationsAsync(int productId)
    {
        await Task.Delay(10); // Simulate work
        return new[] { "Related Product 1", "Related Product 2" };
    }
}

// Supporting classes
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal BasePrice { get; set; } = 99.99m;
    public decimal Price { get; set; }
    public string[] Recommendations { get; set; } = Array.Empty<string>();
}

// Extension method for easy middleware registration
public static class FeatureFlagMiddlewareExtensions
{
    public static IApplicationBuilder UseFeatureFlagRouting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FeatureFlagMiddleware>();
    }

    public static IApplicationBuilder UseFeatureFlagCaching(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FeatureFlagCachingMiddleware>();
    }

    public static IApplicationBuilder UseFeatureFlagRateLimiting(this IApplicationBuilder app)
    {
        return app.UseMiddleware<FeatureFlagRateLimitMiddleware>();
    }
}
