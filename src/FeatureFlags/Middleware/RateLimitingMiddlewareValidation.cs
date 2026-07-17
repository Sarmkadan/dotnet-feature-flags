#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace FeatureFlags.Middleware;

/// <summary>
/// Provides validation helpers for <see cref="RateLimitingMiddleware"/> and <see cref="RateLimitOptions"/>.
/// </summary>
public static class RateLimitingMiddlewareValidation
{
    /// <summary>
    /// Validates a <see cref="RateLimitOptions"/> instance.
    /// </summary>
    /// <param name="value">The rate limit options to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RateLimitOptions? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.MaxRequests <= 0)
        {
            problems.Add($"MaxRequests must be greater than 0, but was {value.MaxRequests}.");
        }

        if (value.WindowSeconds <= 0)
        {
            problems.Add($"WindowSeconds must be greater than 0, but was {value.WindowSeconds}.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="RateLimitOptions"/> instance is valid.
    /// </summary>
    /// <param name="value">The rate limit options to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this RateLimitOptions? value)
    {
        return value is not null && !value.Validate().Any();
    }

    /// <summary>
    /// Ensures that a <see cref="RateLimitOptions"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The rate limit options to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this RateLimitOptions? value)
    {
    ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RateLimitOptions is invalid. Problems:\n{string.Join("\n", problems)}",
                nameof(value));
        }
    }

    /// <summary>
    /// Validates a <see cref="RateLimitingMiddleware"/> instance.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this RateLimitingMiddleware? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate the internal RateLimitOptions
        var optionsProblems = value.GetRateLimitOptions()?.Validate() ?? new List<string> { "RateLimitOptions cannot be null." };
        problems.AddRange(optionsProblems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="RateLimitingMiddleware"/> instance is valid.
    /// </summary>
    /// <param name="value">The middleware instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this RateLimitingMiddleware? value)
    {
        return value is not null && !value.Validate().Any();
    }

    /// <summary>
    /// Ensures that a <see cref="RateLimitingMiddleware"/> instance is valid, throwing an exception if not.
    /// </summary>
    /// <param name="value">The middleware instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this RateLimitingMiddleware? value)
    {
    ArgumentNullException.ThrowIfNull(value);

    var problems = value.Validate();


        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"RateLimitingMiddleware is invalid. Problems:\n{string.Join("\n", problems)}",
                nameof(value));
        }
    }
}
