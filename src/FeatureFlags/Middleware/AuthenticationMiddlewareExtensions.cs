#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.Middleware;

/// <summary>
/// Extension methods for configuring and using AuthenticationMiddleware in ASP.NET Core pipelines.
/// Provides fluent API for adding authentication middleware with various configuration options.
/// </summary>
public static class AuthenticationMiddlewareExtensions
{
    /// <summary>
    /// Adds AuthenticationMiddleware to the pipeline with default configuration.
    /// </summary>
    /// <param name="app">The IApplicationBuilder instance</param>
    /// <returns>IApplicationBuilder for chaining</returns>
    public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder app)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return app.UseMiddleware<AuthenticationMiddleware>();
    }

    /// <summary>
    /// Adds AuthenticationMiddleware to the pipeline with custom configuration options.
    /// </summary>
    /// <param name="app">The IApplicationBuilder instance</param>
    /// <param name="configureOptions">Action to configure AuthenticationOptions</param>
    /// <returns>IApplicationBuilder for chaining</returns>
    public static IApplicationBuilder UseAuthenticationMiddleware(
        this IApplicationBuilder app,
        Action<AuthenticationOptions> configureOptions)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (configureOptions == null)
        {
            throw new ArgumentNullException(nameof(configureOptions));
        }

        var options = new AuthenticationOptions();
        configureOptions(options);

        return app.UseMiddleware<AuthenticationMiddleware>(options);
    }

    /// <summary>
    /// Adds AuthenticationMiddleware to the pipeline with predefined valid API keys.
    /// </summary>
    /// <param name="app">The IApplicationBuilder instance</param>
    /// <param name="validApiKeys">Collection of valid API keys</param>
    /// <returns>IApplicationBuilder for chaining</returns>
    public static IApplicationBuilder UseAuthenticationMiddleware(
        this IApplicationBuilder app,
        IEnumerable<string> validApiKeys)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        if (validApiKeys == null)
        {
            throw new ArgumentNullException(nameof(validApiKeys));
        }

        var options = new AuthenticationOptions
        {
            ValidApiKeys = new List<string>(validApiKeys)
        };

        return app.UseMiddleware<AuthenticationMiddleware>(options);
    }

    /// <summary>
    /// Adds AuthenticationMiddleware to the pipeline with explicit API key requirement control.
    /// </summary>
    /// <param name="app">The IApplicationBuilder instance</param>
    /// <param name="requireApiKey">Whether API key is required (true) or optional (false)</param>
    /// <returns>IApplicationBuilder for chaining</returns>
    public static IApplicationBuilder UseAuthenticationMiddleware(
        this IApplicationBuilder app,
        bool requireApiKey)
    {
        if (app == null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        var options = new AuthenticationOptions
        {
            RequireApiKey = requireApiKey
        };

        return app.UseMiddleware<AuthenticationMiddleware>(options);
    }
}