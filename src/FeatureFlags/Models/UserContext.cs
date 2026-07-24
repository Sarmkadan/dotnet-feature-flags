#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using FeatureFlags.Utilities;

namespace FeatureFlags.Models;

/// <summary>
/// Represents the context of a user for evaluating feature flags.
/// Contains user identity and attributes used for targeting and rollout decisions.
/// </summary>
public sealed class UserContext
{
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Country { get; set; }

    public string? Tier { get; set; }

    public string? Region { get; set; }

    public Dictionary<string, string> CustomAttributes { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Retrieves an attribute value from either standard properties or custom attributes.
    /// </summary>
    public string? GetAttribute(string attributeName)
    {
        return attributeName.ToLower() switch
        {
            "userid" => UserId,
            "email" => Email,
            "country" => Country,
            "tier" => Tier,
            "region" => Region,
            _ => CustomAttributes.TryGetValue(attributeName, out var value) ? value : null
        };
    }

    /// <summary>
    /// Sets a custom attribute value for advanced targeting scenarios.
    /// </summary>
    public void SetCustomAttribute(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Attribute key cannot be empty", nameof(key));

        CustomAttributes[key] = value;
    }

    /// <summary>
    /// Validates the user context has required identity information.
    /// </summary>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) && !string.IsNullOrWhiteSpace(Email);
    }

    /// <summary>
    /// Generates a stable hash for consistent percentage-based rollout evaluation.
    /// Uses userId as the stable identifier for rollout bucketing.
    ///
    /// Algorithm: SHA-256 hash of "{canonicalUserId}:{featureFlagKey}" where canonicalUserId
    /// is the numeric UserId converted to string if it's numeric, otherwise the raw UserId.
    /// The first 4 bytes of the hash are converted to a uint and used for bucket calculation.
    /// This ensures:
    /// - Same input always produces same output (deterministic across app restarts)
    /// - Uniform distribution across buckets (cryptographic hash properties)
    /// - No collisions for different inputs (extremely low probability)
    /// </summary>
    /// <param name="featureFlagKey">The feature flag key for rollout bucketing</param>
    /// <returns>Bucket number between 0 and 99 inclusive</returns>
    public int GetConsistentHash(string featureFlagKey)
    {
        if (string.IsNullOrWhiteSpace(featureFlagKey))
        {
            throw new ArgumentException("Feature flag key cannot be null or empty", nameof(featureFlagKey));
        }

        string canonicalUserId = UserId;
        if (long.TryParse(UserId, out long numericUserId))
        {
            // If UserId is numeric, use its canonical string representation
            canonicalUserId = numericUserId.ToString();
        }

        var combined = $"{canonicalUserId}:{featureFlagKey}";
        return HashingUtilities.ComputeHashBucket(combined, 100);
    }
}
