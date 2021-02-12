#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using FeatureFlags.Models;
using Xunit;

namespace FeatureFlags.Tests.Models;

/// <summary>
/// Extension methods for UserContextTests providing additional testing utilities
/// for UserContext model validation and attribute manipulation scenarios.
/// </summary>
public static class UserContextTestsExtensions
{
    /// <summary>
    /// Creates a valid UserContext with default required fields populated.
    /// </summary>
    /// <param name="userId">The user identifier</param>
    /// <param name="email">The user email address</param>
    /// <returns>A fully initialized UserContext with required fields set</returns>
    /// <exception cref="ArgumentNullException"><paramref name="email"/> is null</exception>
    public static UserContext CreateValidUserContext(this UserContextTests _, string userId = "test-user", string email = "test@example.com")
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentException.ThrowIfNullOrEmpty(userId);

        return new UserContext
        {
            UserId = userId,
            Email = email,
            Country = "US",
            Tier = "premium",
            Region = "west"
        };
    }

    /// <summary>
    /// Creates a UserContext with custom attributes pre-populated.
    /// </summary>
    /// <param name="customAttributes">Dictionary of custom attributes to set</param>
    /// <returns>A UserContext with specified custom attributes</returns>
    /// <exception cref="ArgumentNullException"><paramref name="customAttributes"/> is null</exception>
    public static UserContext WithCustomAttributes(this UserContextTests _, Dictionary<string, string> customAttributes)
    {
        ArgumentNullException.ThrowIfNull(customAttributes);

        var userContext = new UserContext
        {
            UserId = "test-user",
            Email = "test@example.com"
        };

        foreach (var kvp in customAttributes)
        {
            userContext.SetCustomAttribute(kvp.Key, kvp.Value);
        }

        return userContext;
    }

    /// <summary>
    /// Asserts that a UserContext has the expected attribute value.
    /// </summary>
    /// <param name="userContext">The UserContext to validate</param>
    /// <param name="attributeName">The attribute name to check</param>
    /// <param name="expectedValue">The expected attribute value</param>
    /// <exception cref="ArgumentNullException"><paramref name="userContext"/> or <paramref name="attributeName"/> is null</exception>
    public static void ShouldHaveAttribute(this UserContextTests _, UserContext userContext, string attributeName, string expectedValue)
    {
        ArgumentNullException.ThrowIfNull(userContext);
        ArgumentException.ThrowIfNullOrEmpty(attributeName);

        var actualValue = userContext.GetAttribute(attributeName);
        Assert.Equal(expectedValue, actualValue);
    }

    /// <summary>
    /// Asserts that a UserContext is valid (has non-empty UserId and Email).
    /// </summary>
    /// <param name="userContext">The UserContext to validate</param>
    /// <exception cref="ArgumentNullException"><paramref name="userContext"/> is null</exception>
    public static void ShouldBeValid(this UserContextTests _, UserContext userContext)
    {
        ArgumentNullException.ThrowIfNull(userContext);
        Assert.True(userContext.IsValid());
    }

    /// <summary>
    /// Asserts that a UserContext is invalid (missing required fields).
    /// </summary>
    /// <param name="userContext">The UserContext to validate</param>
    /// <exception cref="ArgumentNullException"><paramref name="userContext"/> is null</exception>
    public static void ShouldBeInvalid(this UserContextTests _, UserContext userContext)
    {
        ArgumentNullException.ThrowIfNull(userContext);
        Assert.False(userContext.IsValid());
    }

    /// <summary>
    /// Creates a UserContext with a specific consistent hash value for testing.
    /// Useful for testing percentage-based rollout scenarios where you need deterministic hash values.
    /// </summary>
    /// <param name="userId">The user identifier (numeric values produce more predictable hashes)</param>
    /// <param name="flagKey">The feature flag key to use for hashing</param>
    /// <returns>A UserContext configured to produce a specific hash value</returns>
    /// <exception cref="ArgumentNullException"><paramref name="userId"/> or <paramref name="flagKey"/> is null</exception>
    public static UserContext CreateUserContextWithHash(this UserContextTests _, string userId, string flagKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(userId);
        ArgumentException.ThrowIfNullOrEmpty(flagKey);

        // Numeric user IDs produce more predictable hash values
        return new UserContext
        {
            UserId = userId,
            Email = "hash-test@example.com"
        };
    }

    /// <summary>
    /// Gets the consistent hash value for a UserContext with a given flag key.
    /// </summary>
    /// <param name="userContext">The UserContext to hash</param>
    /// <param name="flagKey">The feature flag key</param>
    /// <returns>The computed hash value (0-99)</returns>
    /// <exception cref="ArgumentNullException"><paramref name="userContext"/> or <paramref name="flagKey"/> is null</exception>
    public static int GetHash(this UserContextTests _, UserContext userContext, string flagKey)
    {
        ArgumentNullException.ThrowIfNull(userContext);
        ArgumentException.ThrowIfNullOrEmpty(flagKey);

        return userContext.GetConsistentHash(flagKey);
    }

    /// <summary>
    /// Creates a collection of UserContext objects for testing batch operations.
    /// </summary>
    /// <param name="count">Number of UserContext objects to create</param>
    /// <returns>IEnumerable of UserContext objects</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 0</exception>
    public static IEnumerable<UserContext> CreateUserContextCollection(this UserContextTests _, int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be negative");
        }

        for (int i = 0; i < count; i++)
        {
            yield return new UserContext
            {
                UserId = $"user-{i:D4}",
                Email = $"user{i}@example.com",
                Country = i % 2 == 0 ? "US" : "EU"
            };
        }
    }

    /// <summary>
    /// Creates a UserContext with a specific tier level for testing tier-based rollouts.
    /// </summary>
    /// <param name="tier">The tier level (free, premium, enterprise)</param>
    /// <returns>A UserContext with specified tier</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tier"/> is null</exception>
    public static UserContext WithTier(this UserContextTests _, string tier)
    {
        ArgumentException.ThrowIfNullOrEmpty(tier);

        var userContext = new UserContext
        {
            UserId = "tier-test-user",
            Email = "tier@example.com"
        };
        userContext.SetCustomAttribute("tier", tier);
        return userContext;
    }
}